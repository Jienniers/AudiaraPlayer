using System.IO;
using System.Windows;
using System.Windows.Controls;
using Audiara.Shared;

namespace Audiara
{
    public partial class FavDialog : Window
    {
        private int _favoriteSongCount = 0;
        private Dictionary<string, string> _favoriteSongs => MainWindow.FavoriteSongs; // Refer to static memory

        public FavDialog()
        {
            InitializeComponent();
            RefreshListBox();
            VerifyFiles();
        }

        private void VerifyFiles()
        {
            List<string> toRemove = new List<string>();

            foreach (var kv in _favoriteSongs)
            {
                if (!File.Exists(kv.Value))
                {
                    MessageBox.Show($"{kv.Key} was removed because it was not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    toRemove.Add(kv.Key);
                }
            }

            foreach (var key in toRemove)
            {
                _favoriteSongs.Remove(key);
            }

            RefreshListBox();
        }

        private void RefreshListBox()
        {
            FavoritesListBox.Items.Clear();
            _favoriteSongCount = 0;

            foreach (var kv in _favoriteSongs)
            {
                _favoriteSongCount++;
                ListBoxHelper.AddItem(FavoritesListBox, _favoriteSongCount.ToString(), kv.Key);
            }
        }

        private void RemoveSelectedFavorite(object sender, RoutedEventArgs e)
        {
            string selected = GetSelectedFavoriteTitle();

            if (!string.IsNullOrEmpty(selected) && _favoriteSongs.ContainsKey(selected))
            {
                _favoriteSongs.Remove(selected);
                RefreshListBox();
            }
        }

        private void PlaySelectedFavorite(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                string selected = GetSelectedFavoriteTitle();

                if (!string.IsNullOrEmpty(selected) && _favoriteSongs.ContainsKey(selected))
                {
                    string path = _favoriteSongs[selected];

                    if (!File.Exists(path))
                    {
                        MessageBox.Show("Music File wasn't found. Removing it.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        _favoriteSongs.Remove(selected);
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

        private string GetSelectedFavoriteTitle()
        {
            if (FavoritesListBox.SelectedItem is ListBoxItem selectedListBoxItem)
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