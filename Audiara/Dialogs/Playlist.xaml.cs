using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using Audiara.Classes;
using Ookii.Dialogs.Wpf;


namespace Audiara
{
    /// <summary>
    /// Interaction logic for Playlist.xaml
    /// </summary>
    public partial class Playlist : Window
    {
        private int playlistNum = 0;
        Dictionary<String, String> files = new Dictionary<String, String>();
        private List<String> playlist_songs = PublicObjects.playlistSongs;

        private MainWindow mainWindow;
        public Playlist(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                FileName = "Music", // Default file name
                DefaultExt = ".mp3", // Default file extension
                Filter = "Audio Files (.mp3)|*.mp3" // Filter files by extension
            };
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                string filename = dialog.FileName;
                string fileNameOnly = Path.GetFileName(filename);
                playlistNum++;
                if (!files.ContainsValue(filename))
                {
                    files.Add(fileNameOnly, filename);
                    PublicObjects.ListBoxs.AddItemToListBox(SongsPlaylist, playlistNum.ToString(), fileNameOnly);
                }
                else
                {
                    MessageBox.Show("Already that mp3 exists.", "Error", MessageBoxButton.OK);
                }
            }
        }

        private void PlayPlaylist(object sender, RoutedEventArgs e)
        {
            playlist_songs.Clear();
            foreach (string items in files.Values)
            {
                playlist_songs.Add(items);
            }
            mainWindow.PlayNextSong();
            Close();
        }

        private void RemoveBtn(object sender, RoutedEventArgs e)
        {
            if (SongsPlaylist.SelectedItem != null)
            {
                playlistNum = 0;
                files.Remove(GetSelectedDescription());
                SongsPlaylist.Items.Clear();
                files.Remove(GetSelectedDescription());
                foreach (String item in files.Keys)
                {
                    playlistNum++;
                    PublicObjects.ListBoxs.AddItemToListBox(SongsPlaylist, playlistNum.ToString(), item);

                    if (playlist_songs.Contains(item))
                    {
                        playlist_songs.Remove(item);
                    }
                }
            }
        }

        private void ClearBtn(object sender, RoutedEventArgs e)
        {
            SongsPlaylist.Items.Clear();
            files.Clear();
            playlist_songs.Clear();
            playlistNum = 0;
        }

        private void AddFolder(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    string selectedFolderPath = dialog.SelectedPath;
                    try
                    {
                        string[] mp3Files = Directory.GetFiles(selectedFolderPath, "*.mp3");
                        foreach (string mp3File in mp3Files)
                        {
                            playlistNum++;
                            PublicObjects.ListBoxs.AddItemToListBox(SongsPlaylist, playlistNum.ToString(), Path.GetFileName(mp3File));
                            files.Add(Path.GetFileName(mp3File), mp3File);   
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
    }
}
