using System.IO;
using System.Windows;
using System.Windows.Controls;
using Audiara.Shared;

namespace Audiara
{
    public partial class FavoritesDialog : Window
    {
        private int _favoriteSongCount = 0;
        private Dictionary<string, string> FavoriteSongs => MainWindow.FavoriteSongs; // Refer to static memory

        public FavoritesDialog()
        {
            InitializeComponent();
            RefreshListBox();
            VerifyFiles();
        }

        private void VerifyFiles()
        {
            List<string> toRemove = new List<string>();

            foreach (var kv in FavoriteSongs)
            {
                if (!File.Exists(kv.Value))
                {
                    MessageBoxService.ShowError($"{kv.Key} was removed because it was not found.");
                    toRemove.Add(kv.Key);
                }
            }

            foreach (var key in toRemove)
            {
                FavoriteSongs.Remove(key);
            }

            RefreshListBox();
        }

        private void RefreshListBox()
        {
            FavoritesListBox.Items.Clear();
            _favoriteSongCount = 0;

            foreach (var kv in FavoriteSongs)
            {
                _favoriteSongCount++;
                ListBoxHelper.AddItem(FavoritesListBox, _favoriteSongCount.ToString(), kv.Key);
            }
        }

        private void RemoveSelectedFavorite(object sender, RoutedEventArgs e)
        {
            string selected = GetSelectedFavoriteTitle();

            if (!string.IsNullOrEmpty(selected) && FavoriteSongs.ContainsKey(selected))
            {
                FavoriteSongs.Remove(selected);
                RefreshListBox();
            }
        }

        private void PlaySelectedFavorite(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                string selected = GetSelectedFavoriteTitle();

                if (!string.IsNullOrEmpty(selected) && FavoriteSongs.ContainsKey(selected))
                {
                    string path = FavoriteSongs[selected];

                    if (!File.Exists(path))
                    {
                        MessageBoxService.ShowError("Music File wasn't found. Removing it.");
                        FavoriteSongs.Remove(selected);
                        RefreshListBox();
                        return;
                    }

                    mainWindow.PlaySongFromFavorites(path);
                    Close();
                }
                else
                {
                    MessageBoxService.ShowError("No music was selected.");
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