using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.IO;
using Audiara.Classes;
using Audiara.Shared;
using Audiara.Dialogs;
using Microsoft.Web.WebView2.Wpf;
using static Audiara.Classes.PublicObjects;

namespace Audiara
{
    public partial class MainWindow : Window
    {
        private bool isDraggingSlider = false;
        private DispatcherTimer timer;
        private List<String> playlist_songs = PublicObjects.playlistSongs;
        private int playlistIndex = 0;
        internal bool isPlaying = false;
        private string songPlayingPath;
        private Dictionary<string, string> FavJsonData = new Dictionary<string, string>();
        private bool ytMusicOpened = false;
        private bool maximized = false;
        public static bool youtubeMusicPlaying = false;

        //keys from settings json
        string keepPlayingYoutubeMusic = PublicObjects.Jsons.SettingsJsonFileKeys.keepPlayingKeyJson;


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
                mediaElement.Source = new Uri(filePath, UriKind.RelativeOrAbsolute);
                mediaElement.Play();
                songPlayingPath = filePath;
                CallFunctions.updateFileDetail(Mp3FileDetail,filePath);
                slider.Value = 0;
                progressBar.Value = 0;
                isPlaying = true;
            }
        }

        private void LoadAppStartup()
        {
            mediaElement.LoadedBehavior = MediaState.Manual;
            mediaElement.MediaOpened += MediaElement_MediaOpened;
            this.mediaElement.MediaEnded += MediaElement_MediaEnded;
            this.timer = new DispatcherTimer(TimeSpan.FromMilliseconds(100), DispatcherPriority.Input, Timer_Tick, this.Dispatcher);
        }

        internal class CallFunctions()
        {

            internal static void updateFileDetail(TextBlock Mp3FileDetail, string FilePath)
            {
                Mp3FileDetail.Text = "File: " + Path.GetFileName(FilePath);
            }

            internal static void displayDuration(double currentPosition, Label startDuration)
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
            if (youtubeMusicPlaying)
            {
                MessageBoxService.YoutubeMusicPlaying();
                return;
            }
            if (playlistIndex < playlist_songs.Count)
            {
                playlistIndex++;
                string fileNameToGet = playlist_songs[playlistIndex];
                PublicObjects.PlayMusic(mediaElement, fileNameToGet);
                CallFunctions.updateFileDetail(Mp3FileDetail, fileNameToGet);
                songPlayingPath = fileNameToGet;
                isPlaying = true;
            }
            else
            {
                isPlaying = false;
                mediaElement.Stop();
            }
        }

        public void FavPlaySong(string filepath)
        {
            PublicObjects.PlayMusic(mediaElement, filepath);
            songPlayingPath = filepath;
            CallFunctions.updateFileDetail(Mp3FileDetail, filepath);
            this.slider.Value = 0;
            this.progressBar.Value = 0;
            isPlaying = true;
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
            if (youtubeMusicPlaying)
            {
                MessageBoxService.YoutubeMusicPlaying();
                return;
            }
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                FileName = "Music",
                DefaultExt = ".mp3",
                Filter = "Audio Files (.mp3)|*.mp3"
            };
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                songPlayingPath = dialog.FileName;
                PublicObjects.PlayMusic(mediaElement, songPlayingPath);
                CallFunctions.updateFileDetail(Mp3FileDetail, songPlayingPath);
                this.slider.Value = 0;
                this.progressBar.Value = 0;
                isPlaying = true;
            }
        }

        private void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (this.mediaElement.NaturalDuration.HasTimeSpan)
            {
                this.slider.Maximum = this.mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
                this.slider.Value = 0;
                this.timer.Start();
            }
        }

        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            this.timer.Stop();
            this.slider.Value = 0;
            this.progressBar.Value = 0;
            if (isPlaying) isPlaying = false;
            Mp3FileDetail.Text = "";
            startDuration.Content = "00:00:00";
            totalDuration.Content = "00:00:00";
            mediaElement.Stop();
            PlayNextSong();
          
        }

        private async void Timer_Tick(object sender, EventArgs e)
        {
            double currentPosition = this.mediaElement.Position.TotalSeconds;
            Dispatcher.Invoke(() =>
            {
                if (!this.isDraggingSlider) this.slider.Value = currentPosition;

                if (isPlaying)
                {
                    CallFunctions.displayDuration(currentPosition, startDuration);
                    totalDuration.Content = mediaElement.NaturalDuration.ToString();
                }
            });
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!this.isDraggingSlider) this.mediaElement.Position = TimeSpan.FromSeconds(slider.Value);

            if (!timer.IsEnabled) timer.Start();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            this.timer.Stop();
            this.timer.Tick -= Timer_Tick;
        }

        private void PauseButtonClick(object sender, RoutedEventArgs e)
        {
            mediaElement.Pause();
        }

        private void ResumeSongButtonClick(object sender, RoutedEventArgs e)
        {
            if (youtubeMusicPlaying)
            {
                MessageBoxService.YoutubeMusicPlaying();
            }
            else
            {
                mediaElement.Play();
            }
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
            if (songPlayingPath is null)
            {
                MessageBoxService.NoSongPlaying();
                return;
            }
            FavJsonData = new Dictionary<string, string>
        {
            { Path.GetFileName(songPlayingPath), songPlayingPath },
        };

            // Call the function to add data to the JSON file
            //PublicObjects.Jsons.AddDataToJsonFile(favouritesJson, FavJsonData);

            MessageBoxService.ShowSuccess($"{Path.GetFileName(songPlayingPath)} has been added to favourites.");
        }

        private async void FavouriteButtonClick(object sender, RoutedEventArgs e)
        {
            FavDialog favDialog = new FavDialog();
            favDialog.ShowDialog();
        }

        private void MaximizeButtonClick(object sender, RoutedEventArgs e)
        {
            if (!maximized)
            {
                MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
                MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;
                WindowState = WindowState.Maximized;
                maximized = true;
            }
            else
            {
                WindowState = WindowState.Normal;
                maximized = false;
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == Window.WindowStateProperty && (WindowState)e.NewValue != WindowState.Minimized && (WindowState)e.NewValue != WindowState.Normal)
            {
                if (!maximized)
                {
                    MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
                    MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;
                    WindowState = WindowState.Maximized;
                    maximized = true;
                }
                else
                {
                    WindowState = WindowState.Normal;
                    maximized = false;
                }
            }
        }

        private void PreviousSongButtonClick(object sender, RoutedEventArgs e)
        {
            bool indexGreaterThan0 = (playlistIndex > 0);
            _ = (indexGreaterThan0) ? playlistIndex-- : playlistIndex = playlist_songs.Count - 1;

            PlayCurrentSong();
        }

        private void NextSongButtonClick(object sender, RoutedEventArgs e)
        {
            bool indexLessThanList = (playlistIndex < playlist_songs.Count - 1);
            _ = (indexLessThanList) ? playlistIndex++ : playlistIndex = 0;

            PlayCurrentSong();
        }

        private void PlayCurrentSong()
        {
            if (playlistIndex >= 0 && playlistIndex < playlist_songs.Count)
            {
                PublicObjects.PlayMusic(mediaElement, playlist_songs[playlistIndex]);
                string fileNameToGet = playlist_songs[playlistIndex];
                songPlayingPath = fileNameToGet;
                CallFunctions.updateFileDetail(Mp3FileDetail, fileNameToGet);
                if (!File.Exists(playlist_songs[playlistIndex]))
                {
                    MessageBoxService.FileNotFound();
                }
            }
        }
    }
}
