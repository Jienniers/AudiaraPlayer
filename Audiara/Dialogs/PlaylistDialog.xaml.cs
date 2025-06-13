using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Audiara.Shared;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace Audiara
{
    /// <summary>
    /// Interaction logic for PlaylistDialog.xaml
    /// </summary>
    public partial class PlaylistDialog : Window
    {
        private int _playlistItemCount = 0;
        Dictionary<String, String> _playlistFiles = new Dictionary<String, String>();

        private MainWindow _mainWindow;
        public PlaylistDialog(MainWindow mainWindow)
        {
            InitializeComponent();
            this._mainWindow = mainWindow;
            foreach (string filePath in MainWindow.PlaylistSongs)
            {
                string fileNameOnly = Path.GetFileName(filePath);
        
                // Avoid duplicates
                if (!_playlistFiles.ContainsValue(filePath))
                {
                    _playlistItemCount++;
                    _playlistFiles.Add(fileNameOnly, filePath);
                    ListBoxHelper.AddItem(SongsPlaylist, _playlistItemCount.ToString(), fileNameOnly);
                }
            }
        }

        private void OnAddFileClick(object sender, RoutedEventArgs e)
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
                _playlistItemCount++;
                if (!_playlistFiles.ContainsValue(filename))
                {
                    _playlistFiles.Add(fileNameOnly, filename);
                    ListBoxHelper.AddItem(SongsPlaylist, _playlistItemCount.ToString(), fileNameOnly);
                }
                else
                {
                    MessageBox.Show("Already that mp3 exists.", "Error", MessageBoxButton.OK);
                }
            }
        }

        private void OnPlayPlaylistClick(object sender, RoutedEventArgs e)
        {
            MainWindow.PlaylistSongs.Clear();

            foreach (string items in _playlistFiles.Values)
            {
                MainWindow.PlaylistSongs.Add(items);
            }

            // Get the selected file name from ListBox
            string selectedFileName = GetSelectedDescription();

            if (!string.IsNullOrEmpty(selectedFileName) && _playlistFiles.ContainsKey(selectedFileName))
            {
                string selectedFullPath = _playlistFiles[selectedFileName];

                // Find the index of the selected song in PlaylistSongs
                int selectedIndex = MainWindow.PlaylistSongs.IndexOf(selectedFullPath);
                if (selectedIndex != -1)
                {
                    _mainWindow.CurrentPlaylistIndex = selectedIndex;
                }
                else
                {
                    _mainWindow.CurrentPlaylistIndex = 0; // fallback
                }
            }
            else
            {
                _mainWindow.CurrentPlaylistIndex = 0; // fallback
            }

            _mainWindow.PlayNextSong();
            Close();
        }


        private void OnRemoveFileClick(object sender, RoutedEventArgs e)
        {
            if (SongsPlaylist.SelectedItem != null)
            {
                string selectedFileName = GetSelectedDescription();

                if (_playlistFiles.ContainsKey(selectedFileName))
                {
                    string fullPath = _playlistFiles[selectedFileName];

                    // Remove from both _playlistFiles and PlaylistSongs
                    _playlistFiles.Remove(selectedFileName);
                    MainWindow.PlaylistSongs.Remove(fullPath);

                    // Rebuild the ListBox
                    SongsPlaylist.Items.Clear();
                    _playlistItemCount = 0;
                    foreach (var item in _playlistFiles)
                    {
                        _playlistItemCount++;
                        ListBoxHelper.AddItem(SongsPlaylist, _playlistItemCount.ToString(), item.Key);
                    }
                }
            }
        }

        private void OnClearPlaylistClick(object sender, RoutedEventArgs e)
        {
            SongsPlaylist.Items.Clear();
            _playlistFiles.Clear();
            MainWindow.PlaylistSongs.Clear();
            _playlistItemCount = 0;
        }

        private void OnAddFolderClick(object sender, RoutedEventArgs e)
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
                            _playlistItemCount++;
                            ListBoxHelper.AddItem(SongsPlaylist, _playlistItemCount.ToString(), Path.GetFileName(mp3File));
                            _playlistFiles.Add(Path.GetFileName(mp3File), mp3File);   
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

            // Sync _playlistFiles to MainWindow.PlaylistSongs
            MainWindow.PlaylistSongs.Clear();
            foreach (var path in _playlistFiles.Values)
            {
                MainWindow.PlaylistSongs.Add(path);
            }
        }

    }
}
