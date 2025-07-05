using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.UndoRedo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class UndoRedoManager : IUndoRedoManager
{
    private readonly List<UndoRedoItem> _undoList = new();
    private readonly List<UndoRedoItem> _redoList = new();
    private readonly object _lock = new();
    private Timer? _changeDetectionTimer;
    private IUndoRedoClient? _undoRedoClient;
    private bool _isChangeDetectionActive;
    private bool _disposed;

    public List<UndoRedoItem> UndoList
    {
        get
        {
            lock (_lock)
            {
                return [.. _undoList];
            }
        }
    }

    public List<UndoRedoItem> RedoList
    {
        get
        {
            lock (_lock)
            {
                return [.. _redoList];
            }
        }
    }

    public bool CanUndo
    {
        get
        {
            lock (_lock)
            {
                return _undoList.Count > 0;
            }
        }
    }

    public bool CanRedo
    {
        get
        {
            lock (_lock)
            {
                return _redoList.Count > 0;
            }
        }
    }

    public int UndoCount
    {
        get
        {
            lock (_lock)
            {
                return _undoList.Count;
            }
        }
    }

    public int RedoCount
    {
        get
        {
            lock (_lock)
            {
                return _redoList.Count;
            }
        }
    }

    public void SetupChangeDetection(IUndoRedoClient undoRedoClient, TimeSpan? interval = null)
    {
        lock (_lock)
        {
            _undoRedoClient = undoRedoClient;
            var detectionInterval = interval ?? TimeSpan.FromSeconds(1);
            _changeDetectionTimer = new Timer(CheckForChanges, null, Timeout.InfiniteTimeSpan, detectionInterval);
        }
    }

    public void StartChangeDetection()
    {
        if (_changeDetectionTimer == null)
        {
            throw new InvalidOperationException("Change detection requires setup via SetupChangeDetection() first");
        }

        lock (_lock)
        {
            if (!_isChangeDetectionActive)
            {
                _isChangeDetectionActive = true;
                _changeDetectionTimer.Change(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
            }
        }
    }

    public void StopChangeDetection()
    {
        lock (_lock)
        {
            if (_isChangeDetectionActive)
            {
                _isChangeDetectionActive = false;
                _changeDetectionTimer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            }
        }
    }

    public void Do(UndoRedoItem action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        lock (_lock)
        {
            _undoList.Add(action);
            _redoList.Clear();
        }
    }

    public UndoRedoItem? Undo()
    {
        UndoRedoItem? undoItem = null;

        lock (_lock)
        {
            if (_undoList.Count == 0)
            {
                return null;
            }

            var currentHash = _undoRedoClient?.GetFastHash() ?? 0;

            undoItem = _undoList.Last();
            if (_undoList.Count > 1) // always keep last undo item (initial starting state)
            {
                _undoList.RemoveAt(_undoList.Count - 1);
            }

            if (_undoRedoClient != null)
            {
                _redoList.Add(undoItem);
            }

            if (undoItem.Hash == currentHash && _undoList.Count > 0)
            {
                // no changes detected, go on to the next item

                undoItem = _undoList.Last();
                if (_undoList.Count > 1) // always keep last undo item (initial starting state)
                {
                    _undoList.RemoveAt(_undoList.Count - 1);
                }

                if (_undoRedoClient != null)
                {
                    _redoList.Add(undoItem);
                }
            }

            return undoItem;
        }
    }

    public UndoRedoItem? Redo()
    {
        UndoRedoItem? item = null;

        lock (_lock)
        {
            if (_redoList.Count == 0)
            {
                return null;
            }

            var currentHash = _undoRedoClient?.GetFastHash() ?? 0;

            item = _redoList.Last();
            _redoList.RemoveAt(_redoList.Count - 1);

            _undoList.Add(item);

            if (item.Hash == currentHash && _redoList.Count > 0)
            {
                item = _redoList.Last();
                _redoList.RemoveAt(_redoList.Count - 1);

                _undoList.Add(item);
            }
        }

        return item;
    }

    public void CheckForChanges(object? state)
    {
        if (_undoRedoClient == null || !_isChangeDetectionActive)
        {
            return;
        }

        try
        {
            lock (_lock)
            {
                var currentHash = _undoRedoClient.GetFastHash();

                var lastUndoItem = _undoList.LastOrDefault();
                if (lastUndoItem?.Hash == currentHash)
                {
                    return;
                }

                var lastRedoItem = _redoList.LastOrDefault();
                if (lastRedoItem?.Hash == currentHash)
                {
                    return;
                }

                var undoRedoItem = _undoRedoClient.MakeUndoRedoObject("Changes detected");
                if (undoRedoItem == null)
                {
                    return;
                }

                if (lastUndoItem != null)
                {
                    var hasChanges = HasChangesAndSetText(lastUndoItem, undoRedoItem);
                    if (!hasChanges)
                    {
                        return;
                    }
                }

                Do(undoRedoItem);
            }
        }
        catch
        {
            // Ignore exceptions in change detection
        }
    }

    private bool HasChangesAndSetText(UndoRedoItem lastUndoItem, UndoRedoItem undoRedoItem)
    {
        if (lastUndoItem.Subtitles.Length > undoRedoItem.Subtitles.Length)
        {
            undoRedoItem.Description = string.Format(Se.Language.General.LinesDeletedX, (lastUndoItem.Subtitles.Length - undoRedoItem.Subtitles.Length));
        }
        else if (lastUndoItem.Subtitles.Length < undoRedoItem.Subtitles.Length)
        {
            undoRedoItem.Description = string.Format(Se.Language.General.LinesAddedX, (undoRedoItem.Subtitles.Length - lastUndoItem.Subtitles.Length));
        }
        else if (lastUndoItem.Subtitles.Length == undoRedoItem.Subtitles.Length)
        {
            var changedLines = new List<int>();
            var textChanges = 0;
            var timingChanges = 0;

            for (var i = 0; i < lastUndoItem.Subtitles.Length; i++)
            {
                var p1 = lastUndoItem.Subtitles[i];
                var p2 = undoRedoItem.Subtitles[i];

                bool hasTextChange = p1.Text != p2.Text;
                bool hasTimingChange = p1.StartTime.TotalMilliseconds != p2.StartTime.TotalMilliseconds ||
                                      p1.EndTime.TotalMilliseconds != p2.EndTime.TotalMilliseconds;

                if (hasTextChange || hasTimingChange)
                {
                    changedLines.Add(i + 1); // 1-based line numbering

                    if (hasTextChange)
                    {
                        textChanges++;
                    }

                    if (hasTimingChange)
                    {
                        timingChanges++;
                    }
                }
            }

            if (changedLines.Count == 0)
            {
                return false;
            }

            // Generate descriptive messages based on the type and scope of changes
            if (changedLines.Count == 1)
            {
                // Single line change - be very specific
                var lineNum = changedLines[0];
                var p1 = lastUndoItem.Subtitles[lineNum - 1];
                var p2 = undoRedoItem.Subtitles[lineNum - 1];

                bool hasTextChange = p1.Text != p2.Text;
                bool hasTimingChange = p1.StartTime.TotalMilliseconds != p2.StartTime.TotalMilliseconds ||
                                      p1.EndTime.TotalMilliseconds != p2.EndTime.TotalMilliseconds;

                if (hasTextChange && hasTimingChange)
                {
                    undoRedoItem.Description = string.Format(Se.Language.Main.LineXTextAndTimingChanged, lineNum);
                }
                else if (hasTextChange)
                {
                    // Show text change preview if not too long
                    var oldText = p1.Text.Length > 30 ? p1.Text.Substring(0, 30) + "..." : p1.Text;
                    var newText = p2.Text.Length > 30 ? p2.Text.Substring(0, 30) + "..." : p2.Text;
                    undoRedoItem.Description = string.Format(Se.Language.Main.LineXTextChangedFromYToZ, lineNum, oldText, newText);
                }
                else if (hasTimingChange)
                {
                    undoRedoItem.Description = string.Format(Se.Language.Main.LineXTimingChanged, lineNum);
                }
            }
            else if (changedLines.Count <= 5)
            {
                // Multiple lines but not too many - list them
                var linesList = string.Join(", ", changedLines);
                var changeTypes = new List<string>();

                if (textChanges > 0)
                {
                    changeTypes.Add($"{textChanges} text");
                }

                if (timingChanges > 0)
                {
                    changeTypes.Add($"{timingChanges} timing");
                }

                undoRedoItem.Description = $"Lines {linesList}: {string.Join(" and ", changeTypes)} changes";
            }
            else
            {
                // Many lines changed - give summary
                var changeTypes = new List<string>();
                if (textChanges > 0)
                {
                    changeTypes.Add($"{textChanges} text");
                }

                if (timingChanges > 0)
                {
                    changeTypes.Add($"{timingChanges} timing");
                }

                undoRedoItem.Description = $"{changedLines.Count} lines modified: {string.Join(" and ", changeTypes)} changes";
            }
        }

        return true;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        StopChangeDetection();
        _changeDetectionTimer?.Dispose();
        _disposed = true;
    }
}
