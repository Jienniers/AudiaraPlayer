using System.IO;
using System.Windows;
using System.Windows.Controls;
using Audiara.Shared;

namespace Audiara
{
    public partial class FavoritesDialog : Window
    {
        // Counter to keep track of how many favorites are displayed
        private int _favoriteCount = 0;

        // Shortcut to access the shared list of favorite songs from MainWindow
        private Dictionary<string, string> FavoriteSongs => MainWindow.FavoriteSongs;

        public FavoritesDialog()
        {
            InitializeComponent();
            UpdateFavoritesListUI();  // Populate the UI with the current list of favorites
            RemoveMissingFiles();     // Clean up any missing file references
        }

        // Removes favorites whose file paths no longer exist on disk
        private void RemoveMissingFiles()
        {
            List<string> toRemove = new List<string>();

            foreach (var kv in FavoriteSongs)
            {
                // Check if the file still exists
                if (!File.Exists(kv.Value))
                {
                    // Notify the user and mark the song for removal
                    MessageBoxService.ShowError($"{kv.Key} was removed because it was not found.");
                    toRemove.Add(kv.Key);
                }
            }

            // Remove invalid entries from the favorite list
            foreach (var key in toRemove)
            {
                FavoriteSongs.Remove(key);
            }

            // Refresh the UI to reflect the updated list
            UpdateFavoritesListUI();
        }

        // Refreshes the ListBox UI with the current favorite songs
        private void UpdateFavoritesListUI()
        {
            FavoriteSongsListBox.Items.Clear();
            _favoriteCount = 0;

            foreach (var kv in FavoriteSongs)
            {
                _favoriteCount++;
                // Use a helper to format and add each item to the ListBox
                ListBoxHelper.AddItem(FavoriteSongsListBox, _favoriteCount.ToString(), kv.Key);
            }
        }

        // Handles the Remove button click to delete the selected favorite
        private void OnRemoveFavoriteClick(object sender, RoutedEventArgs e)
        {
            string selected = GetSelectedFavoriteTitle();

            if (!string.IsNullOrEmpty(selected) && FavoriteSongs.ContainsKey(selected))
            {
                FavoriteSongs.Remove(selected);
                UpdateFavoritesListUI();
            }
        }

        // Handles the Play button click to play the selected favorite
        private void OnPlayFavoriteClick(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                string selected = GetSelectedFavoriteTitle();

                if (!string.IsNullOrEmpty(selected) && FavoriteSongs.ContainsKey(selected))
                {
                    string path = FavoriteSongs[selected];

                    // Ensure the file still exists before attempting to play
                    if (!File.Exists(path))
                    {
                        MessageBoxService.ShowError("Music File wasn't found. Removing it.");
                        FavoriteSongs.Remove(selected);
                        UpdateFavoritesListUI();
                        return;
                    }

                    // Play the song using the main window's method and close this dialog
                    mainWindow.PlaySongFromFavorites(path);
                    Close();
                }
                else
                {
                    MessageBoxService.ShowError("No music was selected.");
                }
            }
        }

        // Retrieves the title (description) of the selected favorite from the ListBox
        private string GetSelectedFavoriteTitle()
        {
            if (FavoriteSongsListBox.SelectedItem is ListBoxItem selectedListBoxItem)
            {
                // Assumes the ListBoxItem contains a StackPanel with the second child as the title text
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