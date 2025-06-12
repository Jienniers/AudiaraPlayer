using System.IO;
using System.Windows;
using System.Windows.Controls;
using Audiara.Shared;

namespace Audiara
{
    public partial class FavDialog : Window
    {
        private int _countnumFav = 0;
        private Dictionary<string, string> FavSongsList => MainWindow.FavoriteSongs; // Refer to static memory

        public FavDialog()
        {
            InitializeComponent();
            RefreshListBox();
            VerifyFiles();
        }

        private void VerifyFiles()
        {
            List<string> toRemove = new List<string>();

            foreach (var kv in FavSongsList)
            {
                if (!File.Exists(kv.Value))
                {
                    MessageBox.Show($"{kv.Key} was removed because it was not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    toRemove.Add(kv.Key);
                }
            }

            foreach (var key in toRemove)
            {
                FavSongsList.Remove(key);
            }

            RefreshListBox();
        }

        private void RefreshListBox()
        {
            SongsFavsListBox.Items.Clear();
            _countnumFav = 0;

            foreach (var kv in FavSongsList)
            {
                _countnumFav++;
                ListBoxHelper.AddItem(SongsFavsListBox, _countnumFav.ToString(), kv.Key);
            }
        }

        private void RemoveFav(object sender, RoutedEventArgs e)
        {
            string selected = GetSelectedDescription();

            if (!string.IsNullOrEmpty(selected) && FavSongsList.ContainsKey(selected))
            {
                FavSongsList.Remove(selected);
                RefreshListBox();
            }
        }

        private void PlayFav(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                string selected = GetSelectedDescription();

                if (!string.IsNullOrEmpty(selected) && FavSongsList.ContainsKey(selected))
                {
                    string path = FavSongsList[selected];

                    if (!File.Exists(path))
                    {
                        MessageBox.Show("Music File wasn't found. Removing it.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        FavSongsList.Remove(selected);
                        RefreshListBox();
                        return;
                    }

                    mainWindow.PlaySongFromFavorites(path);
                    Close();
                }
                else
                {
                    MessageBox.Show("No music was selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private string GetSelectedDescription()
        {
            if (SongsFavsListBox.SelectedItem is ListBoxItem selectedListBoxItem)
            {
                if (selectedListBoxItem.Content is StackPanel stackPanel &&
                    stackPanel.Children.Count > 1 &&
                    stackPanel.Children[1] is TextBlock descriptionTextBlock)
                {
                    return descriptionTextBlock.Text;
                }
            }

            return string.Empty;
        }
    }
}