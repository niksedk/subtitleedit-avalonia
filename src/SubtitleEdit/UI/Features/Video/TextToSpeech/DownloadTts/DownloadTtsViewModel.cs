//using System.Globalization;
//using System.Timers;
//using CommunityToolkit.Mvvm.ComponentModel;
//using CommunityToolkit.Mvvm.Input;
//using Nikse.SubtitleEdit.Logic.Download;
//using SubtitleAlchemist.Features.Video.TextToSpeech.Engines;
//using SubtitleAlchemist.Features.Video.TextToSpeech.Voices;
//using SubtitleAlchemist.Logic;
//using Timer = System.Timers.Timer;

//namespace SubtitleAlchemist.Features.Video.TextToSpeech.DownloadTts
//{
//    public partial class DownloadTtsPopupModel : ObservableObject
//    {
//        public DownloadTtsPopup? Popup { get; set; }

//        [ObservableProperty]
//        public partial string TitleText { get; set; }

//        [ObservableProperty]
//        public partial float ProgressValue { get; set; }

//        [ObservableProperty]
//        public partial string Progress { get; set; }

//        [ObservableProperty]
//        public partial string Error { get; set; }

//        private Task? _downloadTask;
//        private Task? _downloadTaskVoiceModel;
//        private Task? _downloadTaskVoiceConfig;
//        private readonly Timer _timer = new();

//        private readonly ITtsDownloadService _ttsDownloadService;
//        private readonly CancellationTokenSource _cancellationTokenSource;
//        private readonly MemoryStream _downloadStream;
//        private readonly MemoryStream _downloadStreamModel;
//        private readonly MemoryStream _downloadStreamConfig;
//        private readonly IZipUnpacker _zipUnpacker;
//        private readonly object _lock = new();
//        private string _modelFileName;
//        private string _configFileName;

//        public DownloadTtsPopupModel(ITtsDownloadService ttsDownloadService, IZipUnpacker zipUnpacker)
//        {
//            _ttsDownloadService = ttsDownloadService;
//            _zipUnpacker = zipUnpacker;

//            _cancellationTokenSource = new CancellationTokenSource();

//            _downloadStream = new MemoryStream();
//            _downloadStreamModel = new MemoryStream();
//            _downloadStreamConfig = new MemoryStream();

//            _modelFileName = string.Empty;
//            _configFileName = string.Empty;
//            TitleText = string.Empty;

//            Progress = "Starting...";
//            Error = string.Empty;

//            _timer.Interval = 500;
//            _timer.Elapsed += OnTimerOnElapsed;
//            _timer.Start();
//        }

//        private void OnTimerOnElapsed(object? sender, ElapsedEventArgs args)
//        {
//            lock (_lock)
//            {
//                if (!_timer.Enabled)
//                {
//                    return;
//                }


//                if (_downloadTask is { IsCompleted: true })
//                {
//                    _timer.Stop();

//                    if (_downloadStream.Length == 0)
//                    {
//                        Progress = "Download failed";
//                        Error = "No data received";
//                        return;
//                    }

//                    var folder = Piper.GetSetPiperFolder();
//                    _downloadStream.Position = 0;
//                    _zipUnpacker.UnpackZipStream(_downloadStream, folder, "piper", false, new List<string>(), null);

//                    _downloadStream.Dispose();

//                    if (Popup != null)
//                    {
//                        MainThread.BeginInvokeOnMainThread(() =>
//                        {
//                            Popup.Close(true);
//                        });
//                    }
//                }
//                else if (_downloadTask is { IsFaulted: true })
//                {
//                    _timer.Stop();
//                    var ex = _downloadTask.Exception?.InnerException ?? _downloadTask.Exception;
//                    if (ex is OperationCanceledException)
//                    {
//                        Progress = "Download canceled";
//                        Close();
//                    }
//                    else
//                    {
//                        Progress = "Download failed";
//                        Error = ex?.Message ?? "Unknown error";
//                    }
//                }

//                if (_downloadTaskVoiceModel is { IsCompleted: true } && _downloadTaskVoiceConfig is { IsCompleted: true })
//                {
//                    _timer.Stop();

//                    if (_downloadStreamModel.Length == 0)
//                    {
//                        Progress = "Download failed";
//                        Error = "No data received";
//                        return;
//                    }

//                    _downloadStreamModel.Position = 0;
//                    File.WriteAllBytes(_modelFileName, _downloadStreamModel.ToArray());
//                    _downloadStreamModel.Dispose();

//                    if (_downloadStreamConfig.Length == 0)
//                    {
//                        Progress = "Download failed";
//                        Error = "No data received";
//                        return;
//                    }
//                    _downloadStreamConfig.Position = 0;
//                    File.WriteAllBytes(_configFileName, _downloadStreamConfig.ToArray());
//                    _downloadStreamConfig.Dispose();


//                    if (Popup != null)
//                    {
//                        MainThread.BeginInvokeOnMainThread(() =>
//                        {
//                            Popup.Close(true);
//                        });
//                    }
//                }
//                else if (_downloadTaskVoiceModel is { IsFaulted: true })
//                {
//                    _timer.Stop();
//                    var ex = _downloadTaskVoiceModel.Exception?.InnerException ?? _downloadTaskVoiceModel.Exception;
//                    if (ex is OperationCanceledException)
//                    {
//                        Progress = "Download canceled";
//                        Close();
//                    }
//                    else
//                    {
//                        Progress = "Download failed";
//                        Error = ex?.Message ?? "Unknown error";
//                    }
//                }

//                if (_downloadTaskVoiceConfig is { IsFaulted: true })
//                {
//                    _timer.Stop();
//                    var ex = _downloadTaskVoiceConfig.Exception?.InnerException ?? _downloadTaskVoiceConfig.Exception;
//                    if (ex is OperationCanceledException)
//                    {
//                        Progress = "Download canceled";
//                        Close();
//                    }
//                    else
//                    {
//                        Progress = "Download failed";
//                        Error = ex?.Message ?? "Unknown error";
//                    }
//                }
//            }
//        }

//        private void Close()
//        {
//            MainThread.BeginInvokeOnMainThread(() =>
//            {
//                Popup?.Close();
//            });
//        }

//        [RelayCommand]
//        public void Cancel()
//        {
//            _cancellationTokenSource.Cancel();
//            Close();
//        }

//        public void StartDownloadPiper()
//        {
//            TitleText = "Downloading Piper";

//            var downloadProgress = new Progress<float>(number =>
//            {
//                var percentage = (int)Math.Round(number * 100.0, MidpointRounding.AwayFromZero);
//                var pctString = percentage.ToString(CultureInfo.InvariantCulture);
//                ProgressValue = number;
//                Progress = $"Downloading... {pctString}%";
//            });

//            _downloadTask = _ttsDownloadService.DownloadPiper(_downloadStream, downloadProgress, _cancellationTokenSource.Token);
//        }

//        public void StartDownloadPiperVoice(PiperVoice piperVoice)
//        {
//            TitleText = $"Downloading voice: {piperVoice.Voice}";

//            var folder = Piper.GetSetPiperFolder();
//            _modelFileName = Path.Combine(folder, piperVoice.ModelShort);
//            _configFileName = Path.Combine(folder, piperVoice.ConfigShort);

//            var modelUrl = piperVoice.Model;
//            var configUrl = piperVoice.Config;

//            var downloadProgress = new Progress<float>(number =>
//            {
//                var percentage = (int)Math.Round(number * 100.0, MidpointRounding.AwayFromZero);
//                var pctString = percentage.ToString(CultureInfo.InvariantCulture);
//                ProgressValue = number;
//                Progress = $"Downloading... {pctString}%";
//            });
//            var downloadProgressNull = new Progress<float>(_ => { });

//            _downloadTaskVoiceModel = _ttsDownloadService.DownloadPiperVoice(modelUrl, _downloadStreamModel, downloadProgress, _cancellationTokenSource.Token);
//            _downloadTaskVoiceConfig = _ttsDownloadService.DownloadPiperVoice(configUrl, _downloadStreamConfig, downloadProgressNull, _cancellationTokenSource.Token);
//        }
//    }
//}
