using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Audiara.Shared;
using Microsoft.Win32;

namespace Audiara
{
    // Main window of the Audiara music player
    public partial class MainWindow : Window
    {
        // Indicates whether the user is currently dragging the slider
        private bool _isSliderBeingDragged = false;

        // Timer to update playback position every 100ms
        private DispatcherTimer _playbackTimer;

        // Index of currently playing song in playlist
        public int CurrentPlaylistIndex = 0;

        // Is a song currently playing
        private bool _isPlaying = false;

        // Path to the currently loaded song
        private string _currentSongPath;

        // Static list to store user-marked favorite songs (FileName -> FilePath)
        public static readonly Dictionary<string, string> FavoriteSongs = new();

        // Indicates whether the window is in maximized state
        private bool _isWindowMaximized = false;

        // List of songs in the playlist
        public static readonly List<string> PlaylistSongs = new();

        public MainWindow()
        {
            InitializeComponent();
            InitializeMediaPlayer();
            PlaySongFromArgs(); // Handles song passed via command-line
        }

        // Play a song if provided as a command-line argument
        private void PlaySongFromArgs()
        {
            string[] args = Environment.GetCommandLineArgs();

            if (args.Length > 1)
            {
                string filePath = args[1];

                MusicPlayerService.PlayMusic(mediaElement, filePath);

                _currentSongPath = filePath;
                UiHelpers.UpdateSongDetailsDisplay(Mp3FileDetail, filePath);
                playbackSlider.Value = 0;
                playbackProgressBar.Value = 0;
                _isPlaying = true;
            }
        }

        // Setup media element and start the playback timer
        private void InitializeMediaPlayer()
        {
            mediaElement.LoadedBehavior = MediaState.Manual;
            mediaElement.MediaOpened += MediaElement_MediaOpened;
            mediaElement.MediaEnded += MediaElement_MediaEnded;

            _playbackTimer = new DispatcherTimer(
                TimeSpan.FromMilliseconds(100),
                DispatcherPriority.Input,
                Timer_Tick,
                this.Dispatcher
            );
        }

        // Helper class for updating UI elements related to songs
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

        // Play the next song in the playlist
        public void PlayNextSong()
        {
            if (CurrentPlaylistIndex < PlaylistSongs.Count)
            {
                string fileNameToGet = PlaylistSongs[CurrentPlaylistIndex];
                MusicPlayerService.PlayMusic(mediaElement, fileNameToGet);
                UiHelpers.UpdateSongDetailsDisplay(Mp3FileDetail, fileNameToGet);
                _currentSongPath = fileNameToGet;
                _isPlaying = true;
                CurrentPlaylistIndex++;
            }
            else
            {
                _isPlaying = false;
                mediaElement.Stop();
            }
        }

        // Play a selected song from the favorites list
        public void PlaySongFromFavorites(string filepath)
        {
            MusicPlayerService.PlayMusic(mediaElement, filepath);
            _currentSongPath = filepath;
            UiHelpers.UpdateSongDetailsDisplay(Mp3FileDetail, filepath);
            playbackSlider.Value = 0;
            playbackProgressBar.Value = 0;
            _isPlaying = true;
        }

        // Allow dragging the window by clicking anywhere on the main area
        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        // Application control buttons
        private void CloseAppButtonClick(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
        private void MinimizeButtonClick(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        // Open file picker to select and play a new song
        private void OpenAndPlaySongButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                FileName = "Music",
                DefaultExt = ".mp3",
                Filter = "Audio Files (.mp3)|*.mp3"
            };

            if (dialog.ShowDialog() == true)
            {
                _currentSongPath = dialog.FileName;
                MusicPlayerService.PlayMusic(mediaElement, _currentSongPath);
                UiHelpers.UpdateSongDetailsDisplay(Mp3FileDetail, _currentSongPath);
                playbackSlider.Value = 0;
                playbackProgressBar.Value = 0;
                _isPlaying = true;
            }
        }

        // Called when media is loaded and ready
        private void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (mediaElement.NaturalDuration.HasTimeSpan)
            {
                playbackSlider.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
                playbackSlider.Value = 0;
                _playbackTimer.Start();
            }
        }

        // Called when media playback ends
        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            _playbackTimer.Stop();
            playbackSlider.Value = 0;
            playbackProgressBar.Value = 0;
            _isPlaying = false;
            Mp3FileDetail.Text = "";
            startDuration.Content = "00:00:00";
            totalDurationLabel.Content = "00:00:00";
            mediaElement.Stop();
            PlayNextSong();
        }

        // Updates slider and time labels on timer tick
        private void Timer_Tick(object? sender, EventArgs e)
        {
            double currentPosition = mediaElement.Position.TotalSeconds;

            Dispatcher.Invoke(() =>
            {
                if (!_isSliderBeingDragged)
                    playbackSlider.Value = currentPosition;

                if (_isPlaying)
                {
                    UiHelpers.UpdateTimeLabel(currentPosition, startDuration);
                    totalDurationLabel.Content = mediaElement.NaturalDuration.ToString();
                }
            });
        }

        // Updates media position when slider is moved (if not dragging)
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_isSliderBeingDragged)
                mediaElement.Position = TimeSpan.FromSeconds(playbackSlider.Value);

            if (!_playbackTimer.IsEnabled)
                _playbackTimer.Start();
        }

        // Cleanup resources when window is closed
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _playbackTimer.Stop();
            _playbackTimer.Tick -= Timer_Tick;
        }

        // Media control buttons
        private void PausePlaybackButton_Click(object sender, RoutedEventArgs e) => mediaElement.Pause();
        private void ResumePlaybackButton_Click(object sender, RoutedEventArgs e) => mediaElement.Play();

        private void StopPlaybackButton_Click(object sender, RoutedEventArgs e)
        {
            mediaElement.Stop();
            playbackSlider.Value = 0;
            playbackProgressBar.Value = 0;
        }

        // Opens playlist dialog
        private void OpenPlaylistDialogButton_Click(object sender, RoutedEventArgs e)
        {
            PlaylistDialog pla = new(this);
            pla.ShowDialog();
        }

        // Adds current song to favorites list
        private void AddCurrentSongToFavoritesButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentSongPath))
            {
                MessageBoxService.NoSongPlaying();
                return;
            }

            string fileName = Path.GetFileName(_currentSongPath);

            if (!FavoriteSongs.ContainsKey(fileName))
            {
                FavoriteSongs.Add(fileName, _currentSongPath);
                MessageBoxService.ShowSuccess($"{fileName} has been added to favourites.");
            }
            else
            {
                MessageBoxService.ShowError($"{fileName} is already in favourites.");
            }
        }

        // Opens favorites dialog
        private void OpenFavoritesDialogButton_Click(object sender, RoutedEventArgs e)
        {
            FavoritesDialog favDialog = new();
            favDialog.ShowDialog();
        }

        // Toggles between maximized and normal window size
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

        // Detects state change to maximize window if not already
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == WindowStateProperty &&
                (WindowState)e.NewValue != WindowState.Minimized &&
                (WindowState)e.NewValue != WindowState.Normal)
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

        // Playback control: Previous and next buttons
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

        private int GetPreviousSongIndex() =>
            (CurrentPlaylistIndex > 0) ? CurrentPlaylistIndex - 1 : PlaylistSongs.Count - 1;

        private int GetNextSongIndex() =>
            (CurrentPlaylistIndex < PlaylistSongs.Count - 1) ? CurrentPlaylistIndex + 1 : 0;

        // Play a song using the selected playlist index
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

        // Detect slider drag start
        private void PlaybackSlider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _isSliderBeingDragged = true;
            
            Slider slider = sender as Slider;
            
            Point position = e.GetPosition(slider);

            // Calculate percentage of click relative to slider width
            double percentage = position.X / slider.ActualWidth;

            // Update slider value based on click
            double newValue = slider.Minimum + (slider.Maximum - slider.Minimum) * percentage;
            slider.Value = newValue;

            e.Handled = true; // Prevent default dragging behavior
        }

        // Seek media when slider drag ends
        private void PlaybackSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _isSliderBeingDragged = false;
            mediaElement.Position = TimeSpan.FromSeconds(playbackSlider.Value);
        }
    }
}