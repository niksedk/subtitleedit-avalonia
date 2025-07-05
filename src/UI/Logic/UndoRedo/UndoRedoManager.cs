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
    private IUndoRedoClient? _hashProvider;

    private int _lastKnownHash;
    private bool _isChangeDetectionActive;
    private bool _disposed;

    public StateAppliedHandler? OnStateApplied { get; set; }

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

    public void SetupChangeDetection(IUndoRedoClient hashProvider, TimeSpan? interval = null)
    {
        lock (_lock)
        {
            _hashProvider = hashProvider;
            _lastKnownHash = hashProvider.GetFastHash();

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

                if (_hashProvider != null)
                {
                    _lastKnownHash = _hashProvider.GetFastHash();
                }

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

            if (_hashProvider != null)
            {
                _lastKnownHash = _hashProvider.GetFastHash();
            }
        }

        // Apply the current state (the action represents the current state)
        OnStateApplied?.Invoke(action);
    }

    public UndoRedoItem? Undo()
    {
        UndoRedoItem? currentItem = null;

        lock (_lock)
        {
            if (_undoList.Count == 0)
            {
                return null;
            }

            var currentHash = _hashProvider?.GetFastHash() ?? 0;

            currentItem = _undoList.Last();
            if (_undoList.Count > 1) // always keep last undo item (initial starting state)
            {
                _undoList.RemoveAt(_undoList.Count - 1);
            }

            if (currentItem.Hash == currentHash && _undoList.Count > 0)
            {
                currentItem = _undoList.Last();
                if (_undoList.Count > 1) // always keep last undo item (initial starting state)
                {
                    _undoList.RemoveAt(_undoList.Count - 1);
                }
            }

            _redoList.Add(currentItem);

            _lastKnownHash = currentItem.Hash;

            return currentItem;
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


            item = _redoList.Last();
            _redoList.RemoveAt(_redoList.Count - 1);

            //TODO: fix
            //// Check if the last item in redo list matches the current hash
            //var currentHash = _hashProvider?.GetFastHash() ?? 0;
            //if (_redoList.Last().Hash == currentHash && _redoList.Count > 0)
            //{
            //    item = _redoList.Last();
            //    _redoList.RemoveAt(_redoList.Count - 1);
            //}

            // Add back to undo list
            _undoList.Add(item);

            // Update last known hash if hash provider is available
            if (_hashProvider != null)
            {
                _lastKnownHash = _hashProvider.GetFastHash();
            }
        }

        // Apply the redo state
        if (item != null)
        {
            OnStateApplied?.Invoke(item);
        }

        return item;
    }

    private void CheckForChanges(object? state)
    {
        if (_hashProvider == null || !_isChangeDetectionActive)
        {
            return;
        }

        try
        {
            var currentHash = _hashProvider.GetFastHash();

            lock (_lock)
            {
                var lastUndoItem = _undoList.LastOrDefault();
                if (lastUndoItem != null && lastUndoItem.Hash == currentHash)
                {
                    return;
                }

                if (_lastKnownHash != currentHash)
                {
                    _lastKnownHash = currentHash;

                    var undoRedoItem = _hashProvider.MakeUndoRedoObject("Changes detected");
                    if (undoRedoItem == null)
                    {
                        return;
                    }
                    //if (undoRedoItem.Hash == _lastKnownHash)
                    //{
                    //    return;
                    //}

                    if (lastUndoItem != null)
                    {
                        if (lastUndoItem.Subtitles.Length + 1 == undoRedoItem.Subtitles.Length)
                        {
                            undoRedoItem.Description = "Changes detected: line added";
                        }
                        else if (lastUndoItem.Subtitles.Length - 1 == undoRedoItem.Subtitles.Length)
                        {
                            undoRedoItem.Description = "Changes detected: line deleted";
                        }
                        else if (lastUndoItem.Subtitles.Length == undoRedoItem.Subtitles.Length)
                        {
                            var difference = false;
                            for (var i = 0; i < lastUndoItem.Subtitles.Length; i++)
                            {
                                var p1 = lastUndoItem.Subtitles[i];
                                var p2 = undoRedoItem.Subtitles[i];

                                if (p1.Text != p2.Text || p1.StartTime != p2.StartTime || p1.EndTime != p2.EndTime)
                                {
                                    difference = true;
                                    break;
                                }
                            }

                            if (!difference)
                            {
                                return;
                            }
                        }
                    }

                    Do(undoRedoItem);
                }
            }
        }
        catch
        {
            // Ignore exceptions in change detection
        }
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

    public void SetLastActionDescription(string description)
    {
        var last = UndoList.LastOrDefault();
        if (last != null)
        {
            last.Description = description;
        }
    }
}
