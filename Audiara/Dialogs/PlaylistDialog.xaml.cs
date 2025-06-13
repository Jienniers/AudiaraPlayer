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
        // Fields
        private int _playlistItemCount = 0;
        private readonly Dictionary<string, string> _playlistFiles = new();
        private readonly MainWindow _mainWindow;

        // Constructor
        public PlaylistDialog(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            LoadInitialPlaylistItems();
        }

        // Lifecycle
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            SyncPlaylistToMainWindow();
        }

        // Initialization Helpers
        private void LoadInitialPlaylistItems()
        {
            foreach (string filePath in MainWindow.PlaylistSongs)
            {
                string fileName = Path.GetFileName(filePath);

                if (!_playlistFiles.ContainsValue(filePath))
                {
                    _playlistItemCount++;
                    _playlistFiles.Add(fileName, filePath);
                    ListBoxHelper.AddItem(SongsPlaylist, _playlistItemCount.ToString(), fileName);
                }
            }
        }

        private void SyncPlaylistToMainWindow()
        {
            MainWindow.PlaylistSongs.Clear();
            foreach (var path in _playlistFiles.Values)
            {
                MainWindow.PlaylistSongs.Add(path);
            }
        }

        // UI Event Handlers
        private void OnAddFileClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                FileName = "Music",
                DefaultExt = ".mp3",
                Filter = "Audio Files (.mp3)|*.mp3"
            };

            if (dialog.ShowDialog() == true)
            {
                string fullPath = dialog.FileName;
                string fileName = Path.GetFileName(fullPath);

                if (_playlistFiles.ContainsValue(fullPath))
                {
                    MessageBox.Show("This mp3 file is already in the playlist.", "Error", MessageBoxButton.OK);
                    return;
                }

                _playlistItemCount++;
                _playlistFiles.Add(fileName, fullPath);
                ListBoxHelper.AddItem(SongsPlaylist, _playlistItemCount.ToString(), fileName);
            }
        }

        private void OnAddFolderClick(object sender, RoutedEventArgs e)
        {
            using var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string folderPath = dialog.SelectedPath;
                try
                {
                    string[] mp3Files = Directory.GetFiles(folderPath, "*.mp3");
                    foreach (string file in mp3Files)
                    {
                        string fileName = Path.GetFileName(file);
                        if (_playlistFiles.ContainsKey(fileName)) continue;

                        _playlistItemCount++;
                        _playlistFiles.Add(fileName, file);
                        ListBoxHelper.AddItem(SongsPlaylist, _playlistItemCount.ToString(), fileName);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        private void OnRemoveFileClick(object sender, RoutedEventArgs e)
        {
            string selectedFileName = GetSelectedDescription();
            if (!_playlistFiles.ContainsKey(selectedFileName)) return;

            string fullPath = _playlistFiles[selectedFileName];

            _playlistFiles.Remove(selectedFileName);
            MainWindow.PlaylistSongs.Remove(fullPath);

            RefreshPlaylistUI();
        }

        private void OnClearPlaylistClick(object sender, RoutedEventArgs e)
        {
            _playlistFiles.Clear();
            MainWindow.PlaylistSongs.Clear();
            _playlistItemCount = 0;
            SongsPlaylist.Items.Clear();
        }

        private void OnPlayPlaylistClick(object sender, RoutedEventArgs e)
        {
            MainWindow.PlaylistSongs.Clear();
            foreach (string path in _playlistFiles.Values)
            {
                MainWindow.PlaylistSongs.Add(path);
            }

            string selectedFileName = GetSelectedDescription();
            if (!string.IsNullOrEmpty(selectedFileName) && _playlistFiles.ContainsKey(selectedFileName))
            {
                string selectedPath = _playlistFiles[selectedFileName];
                int index = MainWindow.PlaylistSongs.IndexOf(selectedPath);
                _mainWindow.CurrentPlaylistIndex = index != -1 ? index : 0;
            }
            else
            {
                _mainWindow.CurrentPlaylistIndex = 0;
            }

            _mainWindow.PlayNextSong();
            Close();
        }

        // UI Helpers
        private void RefreshPlaylistUI()
        {
            SongsPlaylist.Items.Clear();
            _playlistItemCount = 0;
            foreach (var item in _playlistFiles)
            {
                _playlistItemCount++;
                ListBoxHelper.AddItem(SongsPlaylist, _playlistItemCount.ToString(), item.Key);
            }
        }

        private string GetSelectedDescription()
        {
            if (SongsPlaylist.SelectedItem is ListBoxItem selectedItem &&
                selectedItem.Content is StackPanel stackPanel &&
                stackPanel.Children.Count > 1 &&
                stackPanel.Children[1] is TextBlock descriptionTextBlock)
            {
                return descriptionTextBlock.Text;
            }

            return string.Empty;
        }
    }
}
