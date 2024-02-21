using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MusicPlayer
{
    /// <summary>
    /// Interaction logic for FavDialog.xaml
    /// </summary>
    public partial class FavDialog : Window
    {

        private int CountnumFav = 0;
        private Dictionary<String, String> FavSongsList = new Dictionary<string, string>();
        string jsonPath = "Data\\Favourites.json";
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
                        if (!errorDisplayed) MessageBox.Show($"{GetKeyFromJsonValue(jsonPath, value)} was removed because it was not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        errorDisplayed = true;
                        RemoveKeyAndUpdateFile(jsonPath, GetKeyFromJsonValue(jsonPath, value));
                        SongsFavs.Items.Clear();
                        CountnumFav = 0;
                        foreach (string items in FavSongsList.Keys)
                        {
                            CountnumFav++;
                            AddItemToListBox(CountnumFav.ToString(), items);
                        }
                    }
                }
                errorDisplayed = false;
            }
            catch(Exception ex) { 
                throw new Exception(ex.ToString());
            }
        }

        public string GetKeyFromJsonValue(string jsonPath, string targetValue)
        {
            // Read the JSON data from the file
            string json = File.ReadAllText(jsonPath);

            // Deserialize the JSON string into a dictionary
            Dictionary<string, string> data = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

            // Use LINQ to find the key based on the value
            var matchingKeys = data.Where(pair => pair.Value == targetValue)
                                   .Select(pair => pair.Key);

            // If there's a match, return the first matching key; otherwise, return null
            return matchingKeys.FirstOrDefault();
        }

        private void AddItemToListBox(string itemName, string itemDescription)
        {
            StackPanel stackPanel = new StackPanel();
            stackPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;

            TextBlock textBlockName = new TextBlock();
            textBlockName.Text = itemName;
            textBlockName.Margin = new Thickness(5);

            TextBlock textBlockDescription = new TextBlock();
            textBlockDescription.Text = itemDescription;
            textBlockDescription.Margin = new Thickness(5);


            stackPanel.Children.Add(textBlockName);
            stackPanel.Children.Add(textBlockDescription);

            ListBoxItem listBoxItem = new ListBoxItem();
            listBoxItem.Content = stackPanel;
            listBoxItem.Tag = itemDescription;

            SongsFavs.Items.Add(listBoxItem);
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
                RemoveItemFromListBox(keyToRemove);
            }
            else
            {
                Console.WriteLine($"Key '{keyToRemove}' does not exist in the dictionary.");
            }
        }

        private void RemoveItemFromListBox(string keyToRemove)
        {
            // Create a list to store items to remove
            var itemsToRemove = new List<ListBoxItem>();

            // Iterate through ListBox items and add items to remove list
            foreach (var item in SongsFavs.Items.OfType<ListBoxItem>())
            {
                if (item.Content is StackPanel stackPanel)
                {
                    foreach (var child in stackPanel.Children)
                    {
                        if (child is TextBlock textBlock && textBlock.Text == keyToRemove)
                        {
                            itemsToRemove.Add(item);
                            break;
                        }
                    }
                }
            }

            // Remove items outside of the iteration
            foreach (var itemToRemove in itemsToRemove)
            {
                SongsFavs.Items.Remove(itemToRemove);
            }
        }

        private string GetSelectedDescription()
        {
            if (SongsFavs.SelectedItem is ListBoxItem selectedListBoxItem)
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
                            AddItemToListBox(CountnumFav.ToString(), key);
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
            SongsFavs.Items.Clear();
            CountnumFav = 0;
            foreach (string items in FavSongsList.Keys)
            {
                CountnumFav++;
                AddItemToListBox(CountnumFav.ToString(), items);
            }
        }

        private void PlayFav(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                if (!File.Exists(FavSongsList[GetSelectedDescription()]))
                {
                    MessageBox.Show("Music File wasnt found, Removing it.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    RemoveKeyAndUpdateFile(jsonPath, GetKeyFromJsonValue(jsonPath, FavSongsList[GetSelectedDescription()]));
                    SongsFavs.Items.Clear();
                    CountnumFav = 0;
                    foreach (string items in FavSongsList.Keys)
                    {
                        CountnumFav++;
                        AddItemToListBox(CountnumFav.ToString(), items);
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
