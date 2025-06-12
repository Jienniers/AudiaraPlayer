using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Audiara.Shared;
using Microsoft.Win32;

namespace Audiara
{
    public partial class MainWindow : Window
    {
        private bool _isSliderBeingDragged = false;
        private DispatcherTimer _playbackTimer;
        public int CurrentPlaylistIndex = 0;
        private bool _isPlaying = false;
        private string _currentSongPath;
        public static readonly Dictionary<string, string> FavoriteSongs = new Dictionary<string, string>();
        private bool _isWindowMaximized = false;
        
        public static readonly List<String> PlaylistSongs = new List<String>();
        
        public MainWindow()
        {
            InitializeComponent();
            InitializeMediaPlayer();
            PlaySongFromArgs();
        }

        private void PlaySongFromArgs()
        {
            string[] args = Environment.GetCommandLineArgs();

            if (args.Length > 1)
            {
                string filePath = args[1];
                
                MusicPlayerService.PlayMusic(mediaElement, filePath);
                
                _currentSongPath = filePath;
                UiHelpers.UpdateSongDetailsDisplay(Mp3FileDetail,filePath);
                playbackSlider.Value = 0;
                playbackProgressBar.Value = 0;
                _isPlaying = true;
            }
        }

        private void InitializeMediaPlayer()
        {
            mediaElement.LoadedBehavior = MediaState.Manual;
            mediaElement.MediaOpened += MediaElement_MediaOpened;
            this.mediaElement.MediaEnded += MediaElement_MediaEnded;
            this._playbackTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(100), DispatcherPriority.Input, Timer_Tick, this.Dispatcher);
        }

        internal class UiHelpers()
        {

            internal static void UpdateSongDetailsDisplay(TextBlock mp3FileDetail, string filePath)
            {
                mp3FileDetail.Text = "File: " + Path.GetFileName(filePath);
            }

            internal static void UpdateTimeLabel(double currentPosition, Label startDurationLabel)
            {
                int totalSeconds = (int)currentPosition;
                int hours = totalSeconds / 3600;
                int minutes = (totalSeconds % 3600) / 60;
                int seconds = totalSeconds % 60;

                startDurationLabel.Content = $"{hours:D2}:{minutes:D2}:{seconds:D2}";
            }
        }

        public void PlayNextSong()
        {
            if (CurrentPlaylistIndex < PlaylistSongs.Count)
            {
                string fileNameToGet = PlaylistSongs[CurrentPlaylistIndex]; // don't increment yet

                MusicPlayerService.PlayMusic(mediaElement, fileNameToGet);
                UiHelpers.UpdateSongDetailsDisplay(Mp3FileDetail, fileNameToGet);
                _currentSongPath = fileNameToGet;
                _isPlaying = true;

                CurrentPlaylistIndex++; // increment AFTER playing current song
            }
            else
            {
                _isPlaying = false;
                mediaElement.Stop();
            }
        }


        public void PlaySongFromFavorites(string filepath)
        {
            MusicPlayerService.PlayMusic(mediaElement, filepath);
            _currentSongPath = filepath;
            UiHelpers.UpdateSongDetailsDisplay(Mp3FileDetail, filepath);
            this.playbackSlider.Value = 0;
            this.playbackProgressBar.Value = 0;
            _isPlaying = true;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) DragMove();
        }

        private void CloseAppButtonClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MinimizeButtonClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void OpenAndPlaySongButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                FileName = "Music",
                DefaultExt = ".mp3",
                Filter = "Audio Files (.mp3)|*.mp3"
            };
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                _currentSongPath = dialog.FileName;
                MusicPlayerService.PlayMusic(mediaElement, _currentSongPath);
                UiHelpers.UpdateSongDetailsDisplay(Mp3FileDetail, _currentSongPath);
                this.playbackSlider.Value = 0;
                this.playbackProgressBar.Value = 0;
                _isPlaying = true;
            }
        }

        private void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (this.mediaElement.NaturalDuration.HasTimeSpan)
            {
                this.playbackSlider.Maximum = this.mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
                this.playbackSlider.Value = 0;
                this._playbackTimer.Start();
            }
        }

        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            this._playbackTimer.Stop();
            this.playbackSlider.Value = 0;
            this.playbackProgressBar.Value = 0;
            if (_isPlaying) _isPlaying = false;
            Mp3FileDetail.Text = "";
            startDuration.Content = "00:00:00";
            totalDurationLabel.Content = "00:00:00";
            mediaElement.Stop();
            PlayNextSong();
          
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            double currentPosition = this.mediaElement.Position.TotalSeconds;
            Dispatcher.Invoke(() =>
            {
                if (!this._isSliderBeingDragged) this.playbackSlider.Value = currentPosition;

                if (_isPlaying)
                {
                    UiHelpers.UpdateTimeLabel(currentPosition, startDuration);
                    totalDurationLabel.Content = mediaElement.NaturalDuration.ToString();
                }
            });
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!this._isSliderBeingDragged) this.mediaElement.Position = TimeSpan.FromSeconds(playbackSlider.Value);

            if (!_playbackTimer.IsEnabled) _playbackTimer.Start();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            this._playbackTimer.Stop();
            this._playbackTimer.Tick -= Timer_Tick;
        }

        private void PausePlaybackButton_Click(object sender, RoutedEventArgs e)
        {
            mediaElement.Pause();
        }

        private void ResumePlaybackButton_Click(object sender, RoutedEventArgs e)
        {
            mediaElement.Play();
        }

        private void StopPlaybackButton_Click(object sender, RoutedEventArgs e)
        {
            mediaElement.Stop();
            this.playbackSlider.Value = 0;
            this.playbackProgressBar.Value = 0;
        }

        private void OpenPlaylistDialogButton_Click(object sender, RoutedEventArgs e)
        {
            Playlist pla = new Playlist(this);
            pla.ShowDialog();
        }

        private void AddCurrentSongToFavoritesButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentSongPath))
            {
                MessageBoxService.NoSongPlaying();
                return;
            }

            string fileName = Path.GetFileName(_currentSongPath);

            if (!MainWindow.FavoriteSongs.ContainsKey(fileName))
            {
                MainWindow.FavoriteSongs.Add(fileName, _currentSongPath);
                MessageBoxService.ShowSuccess($"{fileName} has been added to favourites.");
            }
            else
            {
                MessageBoxService.ShowError($"{fileName} is already in favourites.");
            }
        }


        private void OpenFavoritesDialogButton_Click(object sender, RoutedEventArgs e)
        {
            FavDialog favDialog = new FavDialog();
            favDialog.ShowDialog();
        }

        private void ToggleWindowMaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isWindowMaximized)
            {
                MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
                MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;
                WindowState = WindowState.Maximized;
                _isWindowMaximized = true;
            }
            else
            {
                WindowState = WindowState.Normal;
                _isWindowMaximized = false;
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == WindowStateProperty && (WindowState)e.NewValue != WindowState.Minimized && (WindowState)e.NewValue != WindowState.Normal)
            {
                if (!_isWindowMaximized)
                {
                    MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
                    MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;
                    WindowState = WindowState.Maximized;
                    _isWindowMaximized = true;
                }
                else
                {
                    WindowState = WindowState.Normal;
                    _isWindowMaximized = false;
                }
            }
        }

        private void PlayPreviousSongButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentPlaylistIndex = GetPreviousSongIndex();
            PlaySongFromPlaylistSelectedIndex();
        }

        private void PlayNextSongButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentPlaylistIndex = GetNextSongIndex();
            PlaySongFromPlaylistSelectedIndex();
        }

        private int GetPreviousSongIndex()
        {
            return (CurrentPlaylistIndex > 0) ? CurrentPlaylistIndex - 1 : PlaylistSongs.Count - 1;
        }

        private int GetNextSongIndex()
        {
            return (CurrentPlaylistIndex < PlaylistSongs.Count - 1) ? CurrentPlaylistIndex + 1 : 0;
        }


        private void PlaySongFromPlaylistSelectedIndex()
        {
            if (CurrentPlaylistIndex < 0 || CurrentPlaylistIndex >= PlaylistSongs.Count)
                return;

            string songPath = PlaylistSongs[CurrentPlaylistIndex];

            if (!File.Exists(songPath))
            {
                MessageBoxService.FileNotFound();
                return;
            }

            MusicPlayerService.PlayMusic(mediaElement, songPath);
            _currentSongPath = songPath;
            UiHelpers.UpdateSongDetailsDisplay(Mp3FileDetail, songPath);
            _isPlaying = true;
        }
        
        private void PlaybackSlider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _isSliderBeingDragged = true;
        }

        private void PlaybackSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _isSliderBeingDragged = false;
            mediaElement.Position = TimeSpan.FromSeconds(playbackSlider.Value);
        }


    }
}
