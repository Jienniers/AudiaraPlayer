using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Audiara.Classes;
using Audiara.Shared;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace Audiara
{
    /// <summary>
    /// Interaction logic for Playlist.xaml
    /// </summary>
    public partial class Playlist : Window
    {
        private int _playlistNum = 0;
        Dictionary<String, String> _files = new Dictionary<String, String>();

        private MainWindow _mainWindow;
        public Playlist(MainWindow mainWindow)
        {
            InitializeComponent();
            this._mainWindow = mainWindow;
            foreach (string filePath in MainWindow.PlaylistSongs)
            {
                string fileNameOnly = Path.GetFileName(filePath);
        
                // Avoid duplicates
                if (!_files.ContainsValue(filePath))
                {
                    _playlistNum++;
                    _files.Add(fileNameOnly, filePath);
                    ListBoxHelper.AddItem(SongsPlaylist, _playlistNum.ToString(), fileNameOnly);
                }
            }
        }

        private void AddFileButton(object sender, RoutedEventArgs e)
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
                string filename = dialog.FileName;
                string fileNameOnly = Path.GetFileName(filename);
                _playlistNum++;
                if (!_files.ContainsValue(filename))
                {
                    _files.Add(fileNameOnly, filename);
                    ListBoxHelper.AddItem(SongsPlaylist, _playlistNum.ToString(), fileNameOnly);
                }
                else
                {
                    MessageBox.Show("Already that mp3 exists.", "Error", MessageBoxButton.OK);
                }
            }
        }

        private void PlayPlaylist(object sender, RoutedEventArgs e)
        {
            MainWindow.PlaylistSongs.Clear();

            foreach (string items in _files.Values)
            {
                MainWindow.PlaylistSongs.Add(items);
            }

            // Get the selected file name from ListBox
            string selectedFileName = GetSelectedDescription();

            if (!string.IsNullOrEmpty(selectedFileName) && _files.ContainsKey(selectedFileName))
            {
                string selectedFullPath = _files[selectedFileName];

                // Find the index of the selected song in PlaylistSongs
                int selectedIndex = MainWindow.PlaylistSongs.IndexOf(selectedFullPath);
                if (selectedIndex != -1)
                {
                    _mainWindow._playlistIndex = selectedIndex;
                }
                else
                {
                    _mainWindow._playlistIndex = 0; // fallback
                }
            }
            else
            {
                _mainWindow._playlistIndex = 0; // fallback
            }

            _mainWindow.PlayNextSong();
            Close();
        }


        private void RemoveBtn(object sender, RoutedEventArgs e)
        {
            if (SongsPlaylist.SelectedItem != null)
            {
                string selectedFileName = GetSelectedDescription();

                if (_files.ContainsKey(selectedFileName))
                {
                    string fullPath = _files[selectedFileName];

                    // Remove from both _files and PlaylistSongs
                    _files.Remove(selectedFileName);
                    MainWindow.PlaylistSongs.Remove(fullPath);

                    // Rebuild the ListBox
                    SongsPlaylist.Items.Clear();
                    _playlistNum = 0;
                    foreach (var item in _files)
                    {
                        _playlistNum++;
                        ListBoxHelper.AddItem(SongsPlaylist, _playlistNum.ToString(), item.Key);
                    }
                }
            }
        }

        private void ClearBtn(object sender, RoutedEventArgs e)
        {
            SongsPlaylist.Items.Clear();
            _files.Clear();
            MainWindow.PlaylistSongs.Clear();
            _playlistNum = 0;
        }

        private void AddFolder(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    string selectedFolderPath = dialog.SelectedPath;
                    try
                    {
                        string[] mp3Files = Directory.GetFiles(selectedFolderPath, "*.mp3");
                        foreach (string mp3File in mp3Files)
                        {
                            _playlistNum++;
                            ListBoxHelper.AddItem(SongsPlaylist, _playlistNum.ToString(), Path.GetFileName(mp3File));
                            _files.Add(Path.GetFileName(mp3File), mp3File);   
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                }
            }
        }

        private string GetSelectedDescription()
        {
            if (SongsPlaylist.SelectedItem is ListBoxItem selectedListBoxItem)
            {
                if (selectedListBoxItem.Content is StackPanel stackPanel)
                {
                    if (stackPanel.Children.Count > 1 && stackPanel.Children[1] is TextBlock descriptionTextBlock)
                    {
                        return descriptionTextBlock.Text;
                    }
                }
            }

            return string.Empty;
        }
        
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Sync _files to MainWindow.PlaylistSongs
            MainWindow.PlaylistSongs.Clear();
            foreach (var path in _files.Values)
            {
                MainWindow.PlaylistSongs.Add(path);
            }
        }

    }
}
