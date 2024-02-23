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
using Microsoft.Web.WebView2.Wpf;
using System.Diagnostics;



namespace MusicPlayer
{
    public partial class MainWindow : Window
    {
        private bool isDraggingSlider = false;
        private DispatcherTimer timer;
        private DispatcherTimer timeTimer;
        public List<String> playlist_songs = new List<String>();
        private int playlistIndex = 0;
        internal bool isPlaying = false;
        private string songPlayingPath;
        public Dictionary<string, string> FavJsonData = new Dictionary<string, string>();
        private bool ytMusicOpened = false;
        private bool maximized = false;
        private string settingsJson = "Data/settings.json";


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
            this.timeTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(100), DispatcherPriority.Input, Time_Timer_Tick, this.Dispatcher);
        }

        internal class CallFunctions()
        {

            internal async void InitializeWebView(WebView2 webView)
            {
                await webView.EnsureCoreWebView2Async(null);
            }

            internal void LoadWebPage(string url, WebView2 webView)
            {
                webView.Source = new Uri(url);
            }

            internal void AddDataToJsonFile(string filePath, Dictionary<string, string> newData)
            {
                string directoryPath = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                Dictionary<string, string> existingData = new Dictionary<string, string>();
                if (File.Exists(filePath))
                {
                    string existingJson = File.ReadAllText(filePath);
                    existingData = JsonSerializer.Deserialize<Dictionary<string, string>>(existingJson);
                }
                foreach (var entry in newData)
                {
                    if (!existingData.ContainsKey(entry.Key))
                    {
                        existingData.Add(entry.Key, entry.Value);
                    }
                }
                string jsonString = JsonSerializer.Serialize(existingData, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, jsonString);
            }

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

            internal static string getValueFromJson(string targetKey, string jsonPath)
            {

                if (File.Exists(jsonPath))
                {
                    try
                    {
                        string jsonString = File.ReadAllText(jsonPath);
                        using (JsonDocument document = JsonDocument.Parse(jsonString))
                        {
                            if (document.RootElement.TryGetProperty(targetKey, out JsonElement value))
                            {
                                //Console.WriteLine($"Value for key '{targetKey}': {value}");
                                return value.ToString();
                            }
                            else
                            {
                                Console.WriteLine($"Key '{targetKey}' not found in the JSON file.");
                            }
                        }
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"Error parsing JSON: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("JSON file not found.");
                }

                return string.Empty;
            }
        }

        private void PlayNextSong()
        {
            if (playlistIndex < playlist_songs.Count)
            {
                mediaElement.Source = new Uri(playlist_songs[playlistIndex], UriKind.RelativeOrAbsolute);
                mediaElement.Play();
                string fileNameToGet = playlist_songs[playlistIndex];
                CallFunctions.updateFileDetail(Mp3FileDetail, fileNameToGet);
                isPlaying = true;
                playlistIndex++;
            }
            else
            {
                isPlaying = false;
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
            PlayNextSong();
        }

        private async void Timer_Tick(object sender, EventArgs e)
        {
            double currentPosition = this.mediaElement.Position.TotalSeconds;
            Dispatcher.Invoke(() =>
            {
                if (!this.isDraggingSlider)
                {
                    this.slider.Value = currentPosition;
                }

                if (isPlaying)
                {
                    CallFunctions.displayDuration(currentPosition, startDuration);
                    totalDuration.Content = mediaElement.NaturalDuration.ToString();
                }
            });
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
                    string fileNameToGet = playlist_songs[playlistIndex];
                    CallFunctions.updateFileDetail(Mp3FileDetail, fileNameToGet);
                    playlistIndex++;
                    if (playlistIndex >= playlist_songs.Count)
                    {
                        playlistIndex = 0;
                    }
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
            CallFunctions callFunctions = new CallFunctions();
            callFunctions.AddDataToJsonFile(filePath, FavJsonData);

            MessageBox.Show($"{Path.GetFileName(songPlayingPath)} has been added to favourites.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void FavouriteButtonClick(object sender, RoutedEventArgs e)
        {
            FavDialog favDialog = new FavDialog();
            favDialog.ShowDialog();
        }

        private void YoutubeMusicButtonClick(object sender, RoutedEventArgs e)
        {
            CallFunctions callFunctions = new CallFunctions();
            mediaElement.Stop();
            try
            {
                if (!ytMusicOpened)
                {
                    callFunctions.InitializeWebView(webView);
                    callFunctions.LoadWebPage("https://music.youtube.com/", webView);
                    ytMusicGrid.Visibility = Visibility.Visible;
                    ytMusicOpened = true;
                    YoutubeMusicbtn.Content = "Close YT Music";
                }
                else
                {
                    callFunctions.InitializeWebView(webView);
                    webView.Source = new Uri("about:blank");
                    ytMusicGrid.Visibility = Visibility.Hidden;
                    ytMusicOpened = false;
                    YoutubeMusicbtn.Content = "Open YT Music";
                }
            }
            catch (Exception ew)
            {
                throw ew;
            }
        }

        private void SettingButtonClick(object sender, RoutedEventArgs e)
        {
            Settings setting = new Settings();
            setting.ShowDialog();
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
            if (playlistIndex > 0)
            {
                playlistIndex--;
            }
            else
            {
                playlistIndex = playlist_songs.Count - 1;
            }

            PlayCurrentSong();
        }

        private void NextSongButtonClick(object sender, RoutedEventArgs e)
        {
            if (playlistIndex < playlist_songs.Count - 1)
            {
                playlistIndex++;
            }
            else
            {
                playlistIndex = 0;
            }

            PlayCurrentSong();
        }

        private void PlayCurrentSong()
        {
            if (playlistIndex >= 0 && playlistIndex < playlist_songs.Count)
            {
                mediaElement.Source = new Uri(playlist_songs[playlistIndex], UriKind.RelativeOrAbsolute);
                mediaElement.Play();
                string fileNameToGet = playlist_songs[playlistIndex];
                CallFunctions.updateFileDetail(Mp3FileDetail, fileNameToGet);
                if (!File.Exists(playlist_songs[playlistIndex])) MessageBox.Show("File not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Time_Timer_Tick(object sender, EventArgs e)
        {
            if (CallFunctions.getValueFromJson("ShowTime", settingsJson) == "true")
            {
                DateTime currentTime = DateTime.Now;
                string formattedTime = currentTime.ToString("h:mm tt");
                TimeLabel.Content = formattedTime;
                timeTimer.Start();
            }
            else
            {
                TimeLabel.Content = string.Empty;
            }
        }
    }
}
