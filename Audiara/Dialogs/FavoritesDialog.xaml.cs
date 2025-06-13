using System.IO;
using System.Windows;
using System.Windows.Controls;
using Audiara.Shared;

namespace Audiara
{
    public partial class FavoritesDialog : Window
    {
        private int _favoriteCount = 0;
        private Dictionary<string, string> FavoriteSongs => MainWindow.FavoriteSongs; // Refer to static memory

        public FavoritesDialog()
        {
            InitializeComponent();
            UpdateFavoritesListUI();
            RemoveMissingFiles();
        }

        private void RemoveMissingFiles()
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

            UpdateFavoritesListUI();
        }

        private void UpdateFavoritesListUI()
        {
            FavoriteSongsListBox.Items.Clear();
            _favoriteCount = 0;

            foreach (var kv in FavoriteSongs)
            {
                _favoriteCount++;
                ListBoxHelper.AddItem(FavoriteSongsListBox, _favoriteCount.ToString(), kv.Key);
            }
        }

        private void OnRemoveFavoriteClick(object sender, RoutedEventArgs e)
        {
            string selected = GetSelectedFavoriteTitle();

            if (!string.IsNullOrEmpty(selected) && FavoriteSongs.ContainsKey(selected))
            {
                FavoriteSongs.Remove(selected);
                UpdateFavoritesListUI();
            }
        }

        private void OnPlayFavoriteClick(object sender, RoutedEventArgs e)
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
                        UpdateFavoritesListUI();
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
            if (FavoriteSongsListBox.SelectedItem is ListBoxItem selectedListBoxItem)
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