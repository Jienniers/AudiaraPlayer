using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Audiara.Classes;
using Audiara.Shared;
using Microsoft.Win32;

namespace Audiara
{
    public partial class MainWindow : Window
    {
        private bool _isDraggingSlider = false;
        private DispatcherTimer _timer;
        private int _playlistIndex = 0;
        internal bool IsPlaying = false;
        private string _songPlayingPath;
        private Dictionary<string, string> _favJsonData = new Dictionary<string, string>();
        private bool _maximized = false;
        
        public static List<String> PlaylistSongs = new List<String>();
        
        public MainWindow()
        {
            InitializeComponent();
            LoadAppStartup();
            HandleStartup();
        }

        private void HandleStartup()
        {
            string[] args = Environment.GetCommandLineArgs();

            if (args.Length > 1)
            {
                string filePath = args[1];
                
                MusicPlayerService.PlayMusic(mediaElement, filePath);
                
                _songPlayingPath = filePath;
                CallFunctions.UpdateFileDetail(Mp3FileDetail,filePath);
                slider.Value = 0;
                progressBar.Value = 0;
                IsPlaying = true;
            }
        }

        private void LoadAppStartup()
        {
            mediaElement.LoadedBehavior = MediaState.Manual;
            mediaElement.MediaOpened += MediaElement_MediaOpened;
            this.mediaElement.MediaEnded += MediaElement_MediaEnded;
            this._timer = new DispatcherTimer(TimeSpan.FromMilliseconds(100), DispatcherPriority.Input, Timer_Tick, this.Dispatcher);
        }

        internal class CallFunctions()
        {

            internal static void UpdateFileDetail(TextBlock mp3FileDetail, string filePath)
            {
                mp3FileDetail.Text = "File: " + Path.GetFileName(filePath);
            }

            internal static void DisplayDuration(double currentPosition, Label startDuration)
            {
                int totalSeconds = (int)currentPosition;
                int hours = totalSeconds / 3600;
                int minutes = (totalSeconds % 3600) / 60;
                int seconds = totalSeconds % 60;

                startDuration.Content = $"{hours:D2}:{minutes:D2}:{seconds:D2}";
            }
        }

        public void PlayNextSong()
        {
            if (_playlistIndex < PlaylistSongs.Count)
            {
                _playlistIndex++;
                string fileNameToGet = PlaylistSongs[_playlistIndex];
                
                MusicPlayerService.PlayMusic(mediaElement, fileNameToGet);
                
                CallFunctions.UpdateFileDetail(Mp3FileDetail, fileNameToGet);
                _songPlayingPath = fileNameToGet;
                IsPlaying = true;
            }
            else
            {
                IsPlaying = false;
                mediaElement.Stop();
            }
        }

        public void FavPlaySong(string filepath)
        {
            MusicPlayerService.PlayMusic(mediaElement, filepath);
            _songPlayingPath = filepath;
            CallFunctions.UpdateFileDetail(Mp3FileDetail, filepath);
            this.slider.Value = 0;
            this.progressBar.Value = 0;
            IsPlaying = true;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) DragMove();
        }

        private void CrossButtonClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MinimizeButtonClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void PlayButton(object sender, RoutedEventArgs e)
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
                _songPlayingPath = dialog.FileName;
                MusicPlayerService.PlayMusic(mediaElement, _songPlayingPath);
                CallFunctions.UpdateFileDetail(Mp3FileDetail, _songPlayingPath);
                this.slider.Value = 0;
                this.progressBar.Value = 0;
                IsPlaying = true;
            }
        }

        private void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (this.mediaElement.NaturalDuration.HasTimeSpan)
            {
                this.slider.Maximum = this.mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
                this.slider.Value = 0;
                this._timer.Start();
            }
        }

        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            this._timer.Stop();
            this.slider.Value = 0;
            this.progressBar.Value = 0;
            if (IsPlaying) IsPlaying = false;
            Mp3FileDetail.Text = "";
            startDuration.Content = "00:00:00";
            totalDuration.Content = "00:00:00";
            mediaElement.Stop();
            PlayNextSong();
          
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            double currentPosition = this.mediaElement.Position.TotalSeconds;
            Dispatcher.Invoke(() =>
            {
                if (!this._isDraggingSlider) this.slider.Value = currentPosition;

                if (IsPlaying)
                {
                    CallFunctions.DisplayDuration(currentPosition, startDuration);
                    totalDuration.Content = mediaElement.NaturalDuration.ToString();
                }
            });
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!this._isDraggingSlider) this.mediaElement.Position = TimeSpan.FromSeconds(slider.Value);

            if (!_timer.IsEnabled) _timer.Start();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            this._timer.Stop();
            this._timer.Tick -= Timer_Tick;
        }

        private void PauseButtonClick(object sender, RoutedEventArgs e)
        {
            mediaElement.Pause();
        }

        private void ResumeSongButtonClick(object sender, RoutedEventArgs e)
        {
            mediaElement.Play();
            //if (isPlaying)
            //{
            //    mediaElement.Play();
            //}
            //else
            //{
            //    if (playlistIndex < playlist_songs.Count)
            //    {
            //        PublicObjects.PlayMusic(mediaElement, playlist_songs[playlistIndex]);
            //        songPlayingPath = playlist_songs[playlistIndex];
            //        string fileNameToGet = playlist_songs[playlistIndex];
            //        CallFunctions.updateFileDetail(Mp3FileDetail, fileNameToGet);
            //        isPlaying = true;
            //        playlistIndex++;
            //        _ = (playlistIndex >= playlist_songs.Count) ? playlistIndex = 0 : 0;
            //    }
            //}
        }

        private void StopSongButtonClick(object sender, RoutedEventArgs e)
        {
            mediaElement.Stop();
            this.slider.Value = 0;
            this.progressBar.Value = 0;
        }

        private void PlaylistButtonClick(object sender, RoutedEventArgs e)
        {
            Playlist pla = new Playlist(this);
            pla.ShowDialog();
        }

        private void FavouriteSongButtonClick(object sender, RoutedEventArgs e)
        {
            // if (_songPlayingPath is null)
            // {
            //     MessageBoxService.NoSongPlaying();
            //     return;
            // }
            _favJsonData = new Dictionary<string, string>
        {
            { Path.GetFileName(_songPlayingPath), _songPlayingPath },
        };

            // Call the function to add data to the JSON file
            //PublicObjects.Jsons.AddDataToJsonFile(favouritesJson, FavJsonData);

            MessageBoxService.ShowSuccess($"{Path.GetFileName(_songPlayingPath)} has been added to favourites.");
        }

        private void FavouriteButtonClick(object sender, RoutedEventArgs e)
        {
            FavDialog favDialog = new FavDialog();
            favDialog.ShowDialog();
        }

        private void MaximizeButtonClick(object sender, RoutedEventArgs e)
        {
            if (!_maximized)
            {
                MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
                MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;
                WindowState = WindowState.Maximized;
                _maximized = true;
            }
            else
            {
                WindowState = WindowState.Normal;
                _maximized = false;
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == WindowStateProperty && (WindowState)e.NewValue != WindowState.Minimized && (WindowState)e.NewValue != WindowState.Normal)
            {
                if (!_maximized)
                {
                    MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
                    MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;
                    WindowState = WindowState.Maximized;
                    _maximized = true;
                }
                else
                {
                    WindowState = WindowState.Normal;
                    _maximized = false;
                }
            }
        }

        private void PreviousSongButtonClick(object sender, RoutedEventArgs e)
        {
            bool indexGreaterThan0 = (_playlistIndex > 0);
            _ = (indexGreaterThan0) ? _playlistIndex-- : _playlistIndex = PlaylistSongs.Count - 1;

            PlayCurrentSong();
        }

        private void NextSongButtonClick(object sender, RoutedEventArgs e)
        {
            bool indexLessThanList = (_playlistIndex < PlaylistSongs.Count - 1);
            _ = (indexLessThanList) ? _playlistIndex++ : _playlistIndex = 0;

            PlayCurrentSong();
        }

        private void PlayCurrentSong()
        {
            if (_playlistIndex >= 0 && _playlistIndex < PlaylistSongs.Count)
            {
                MusicPlayerService.PlayMusic(mediaElement, PlaylistSongs[_playlistIndex]);
                string fileNameToGet = PlaylistSongs[_playlistIndex];
                _songPlayingPath = fileNameToGet;
                CallFunctions.UpdateFileDetail(Mp3FileDetail, fileNameToGet);
                if (!File.Exists(PlaylistSongs[_playlistIndex]))
                {
                    MessageBoxService.FileNotFound();
                }
            }
        }
    }
}
