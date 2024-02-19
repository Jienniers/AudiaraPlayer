using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.IO;
using ShapesPath = System.Windows.Shapes.Path;
using System.Drawing.Imaging;
using System.Windows.Documents;
using System.Text.Json;
using Microsoft.Web.WebView2.Core;
using MusicPlayer.Dialogs;



namespace MusicPlayer
{
    public partial class MainWindow : Window
    {
        private bool isDraggingSlider = false;
        private DispatcherTimer timer;
        public List<String> playlist_songs = new List<String>();
        private int playlistIndex = 0;
        private bool isPlaying = false;
        private string songPlayingPath;
        public Dictionary<string, string> FavJsonData = new Dictionary<string, string>();
        private bool ytMusicOpened = false;


        public MainWindow()
        {
            InitializeComponent();
            LoadAppStartup();
        }

        private void LoadAppStartup()
        {
            mediaElement.LoadedBehavior = MediaState.Manual;
            mediaElement.MediaOpened += MediaElement_MediaOpened;
            this.mediaElement.MediaEnded += MediaElement_MediaEnded;
            this.timer = new DispatcherTimer(TimeSpan.FromMilliseconds(100), DispatcherPriority.Input, Timer_Tick, this.Dispatcher);
        }

        private async void InitializeWebView()
        {
            await webView.EnsureCoreWebView2Async(null);
        }

        private void LoadWebPage(string url)
        {
            webView.Source = new Uri(url);
        }

        private void AddDataToJsonFile(string filePath, Dictionary<string, string> newData)
        {
            // Get the directory path from the file path
            string directoryPath = Path.GetDirectoryName(filePath);

            // Create the directory if it doesn't exist
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Initialize existing data or an empty dictionary
            Dictionary<string, string> existingData = new Dictionary<string, string>();

            // Check if the file exists
            if (File.Exists(filePath))
            {
                // Read existing JSON data from the file
                string existingJson = File.ReadAllText(filePath);

                // Deserialize the JSON string into a dictionary
                existingData = JsonSerializer.Deserialize<Dictionary<string, string>>(existingJson);
            }

            // Merge existing data with new data
            foreach (var entry in newData)
            {
                // Check if the key already exists in the existing data
                if (!existingData.ContainsKey(entry.Key))
                {
                    // Add the new key-value pair to the existing data
                    existingData.Add(entry.Key, entry.Value);
                }
            }

            // Convert the merged dictionary to a JSON string
            string jsonString = JsonSerializer.Serialize(existingData, new JsonSerializerOptions { WriteIndented = true });

            // Write the JSON string back to the file, creating it if it doesn't exist
            File.WriteAllText(filePath, jsonString);
        }

        private void PlayNextSong()
        {
            if (playlistIndex < playlist_songs.Count)
            {
                mediaElement.Source = new Uri(playlist_songs[playlistIndex], UriKind.RelativeOrAbsolute);
                mediaElement.Play();
                playlistIndex++;
            }
            else
            {
                mediaElement.Stop();
            }
        }

        public void FavPlaySong(string filepath)
        {
            mediaElement.Source = new Uri(filepath, UriKind.RelativeOrAbsolute);
            mediaElement.Play();
            this.slider.Value = 0;
            this.progressBar.Value = 0;
            isPlaying = true;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
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
                mediaElement.Source = new Uri(songPlayingPath, UriKind.RelativeOrAbsolute);
                mediaElement.Play();
            }
            this.slider.Value = 0;
            this.progressBar.Value = 0;
            isPlaying = true;
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
            PlayNextSong();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!this.isDraggingSlider)
            {
                double currentPosition = this.mediaElement.Position.TotalSeconds;
                this.slider.Value = currentPosition;
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!this.isDraggingSlider)
            {
                this.mediaElement.Position = TimeSpan.FromSeconds(slider.Value);
            }
            if (!timer.IsEnabled)
            {
                timer.Start();
            }
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
            if (isPlaying)
            {
                mediaElement.Play();
            }
            else
            {
                if (playlistIndex < playlist_songs.Count)
                {
                    mediaElement.Source = new Uri(playlist_songs[playlistIndex], UriKind.RelativeOrAbsolute);
                    mediaElement.Play();
                    playlistIndex++;
                }
            }
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
                MessageBox.Show("No Song Playing", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string filePath = "Data\\Favourites.json";
            FavJsonData = new Dictionary<string, string>
        {
            { Path.GetFileName(songPlayingPath), songPlayingPath },
        };

            // Call the function to add data to the JSON file
            AddDataToJsonFile(filePath, FavJsonData);

            MessageBox.Show($"Data has been added to {filePath}");
        }

        private async void FavouriteButtonClick(object sender, RoutedEventArgs e)
        {
            FavDialog favDialog = new FavDialog();
            favDialog.ShowDialog();
        }

        private void YoutubeMusicButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ytMusicOpened)
                {
                    InitializeWebView();
                    LoadWebPage("https://music.youtube.com/");
                    ytMusicGrid.Visibility = Visibility.Visible;
                    ytMusicOpened = true;
                }
                else
                {
                    InitializeWebView();
                    webView.Source = new Uri("about:blank");
                    ytMusicGrid.Visibility = Visibility.Hidden;
                    ytMusicOpened = false;
                }
            }catch (Exception ew)
            {
                throw ew;
            }
        }

        private void SettingButtonClick(object sender, RoutedEventArgs e)
        {
            Settings setting = new Settings();
            setting.ShowDialog();
        }
    }
}
