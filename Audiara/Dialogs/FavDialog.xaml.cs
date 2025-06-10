using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Audiara.Classes;

namespace Audiara
{
    /// <summary>
    /// Interaction logic for FavDialog.xaml
    /// </summary>
    public partial class FavDialog : Window
    {

        private int CountnumFav = 0;
        private Dictionary<String, String> FavSongsList = new Dictionary<string, string>();
        string jsonPath = PublicObjects.Jsons.JsonFilePaths.favouriteJsonFilePath;
        public FavDialog()
        {
            InitializeComponent();
            startup();
        }

        private void startup()
        {
            refreshListBox();
            verifyFile();
        }

        private void verifyFile()
        {
            bool errorDisplayed = false;
            try
            {
                foreach (var value in FavSongsList.Values)
                {
                    if (!File.Exists(value))
                    {
                        if (!errorDisplayed) MessageBox.Show($"{PublicObjects.Jsons.GetValueFromJsonKey(jsonPath, value)} was removed because it was not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        errorDisplayed = true;
                        RemoveKeyAndUpdateFile(jsonPath, PublicObjects.Jsons.GetValueFromJsonKey(jsonPath, value));
                        SongsFavsListBox.Items.Clear();
                        CountnumFav = 0;
                        foreach (string items in FavSongsList.Keys)
                        {
                            CountnumFav++;
                            PublicObjects.ListBoxs.AddItemToListBox(SongsFavsListBox, CountnumFav.ToString(), items);
                        }
                    }
                }
                errorDisplayed = false;
            }
            catch(Exception ex) { 
                throw new Exception(ex.ToString());
            }
        }

        private void RemoveKeyAndUpdateFile(string filePath, string keyToRemove)
        {
            // Check if the key exists in the dictionary
            if (FavSongsList.ContainsKey(keyToRemove))
            {
                // Remove the key from the dictionary
                FavSongsList.Remove(keyToRemove);

                // Update the JSON file with the modified dictionary
                string json = JsonSerializer.Serialize(FavSongsList, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json);

                // Remove the corresponding item from the ListBox
                PublicObjects.ListBoxs.RemoveItemFromListBox(SongsFavsListBox, keyToRemove);
            }
            else
            {
                Console.WriteLine($"Key '{keyToRemove}' does not exist in the dictionary.");
            }
        }

        private string GetSelectedDescription()
        {
            if (SongsFavsListBox.SelectedItem is ListBoxItem selectedListBoxItem)
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

        private void refreshListBox()
        {
            
            // Check if the file exists
            if (File.Exists(jsonPath))
            {
                // Read the JSON data from the file
                string json = File.ReadAllText(jsonPath);

                // Deserialize the JSON string into a dictionary
                Dictionary<string, string> data = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                // Check if the dictionary is not null
                if (data != null)
                {
                    // Iterate through keys and print them
                    foreach (string key in data.Keys)
                    {
                        CountnumFav++;
                        // Check if the key already exists in the dictionary
                        if (!FavSongsList.ContainsKey(key))
                        {
                            PublicObjects.ListBoxs.AddItemToListBox(SongsFavsListBox, CountnumFav.ToString(), key);
                            FavSongsList.Add(key, data[key]);
                        }
                        else
                        {
                            Console.WriteLine($"Key '{key}' already exists in the dictionary.");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("The JSON data is empty or invalid.");
                }
            }
            else
            {
                Console.WriteLine("The JSON file does not exist.");
            }
        }


        private void RemoveFav(object sender, RoutedEventArgs e)
        {
            RemoveKeyAndUpdateFile(jsonPath, GetSelectedDescription());
            SongsFavsListBox.Items.Clear();
            CountnumFav = 0;
            foreach (string items in FavSongsList.Keys)
            {
                CountnumFav++;
                PublicObjects.ListBoxs.AddItemToListBox(SongsFavsListBox, CountnumFav.ToString(), items);
            }
        }

        private void PlayFav(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                if (!File.Exists(FavSongsList[GetSelectedDescription()]))
                {
                    MessageBox.Show("Music File wasnt found, Removing it.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    RemoveKeyAndUpdateFile(jsonPath, PublicObjects.Jsons.GetValueFromJsonKey(jsonPath, FavSongsList[GetSelectedDescription()]));
                    SongsFavsListBox.Items.Clear();
                    CountnumFav = 0;
                    foreach (string items in FavSongsList.Keys)
                    {
                        CountnumFav++;
                        PublicObjects.ListBoxs.AddItemToListBox(SongsFavsListBox, CountnumFav.ToString(), items);
                    }
                    return;
                }
                if (GetSelectedDescription() != string.Empty)
                {
                    mainWindow.FavPlaySong(FavSongsList[GetSelectedDescription()]);
                    Close();
                }
                else
                {
                    MessageBox.Show("No music was selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
