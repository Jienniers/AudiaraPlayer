using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Audiara.Classes;
using Audiara.Shared;

namespace Audiara
{
    /// <summary>
    /// Interaction logic for FavDialog.xaml
    /// </summary>
    public partial class FavDialog : Window
    {

        private int _countnumFav = 0;
        private Dictionary<String, String> _favSongsList = new Dictionary<string, string>();
        readonly string _jsonPath = PublicObjects.Jsons.JsonFilePaths.FavouriteJsonFilePath;
        public FavDialog()
        {
            InitializeComponent();
            Startup();
        }

        private void Startup()
        {
            RefreshListBox();
            VerifyFile();
        }

        private void VerifyFile()
        {
            bool errorDisplayed = false;
            try
            {
                foreach (var value in _favSongsList.Values)
                {
                    if (!File.Exists(value))
                    {
                        if (!errorDisplayed) MessageBox.Show($"{PublicObjects.Jsons.GetValueFromJsonKey(_jsonPath, value)} was removed because it was not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        errorDisplayed = true;
                        RemoveKeyAndUpdateFile(_jsonPath, PublicObjects.Jsons.GetValueFromJsonKey(_jsonPath, value));
                        SongsFavsListBox.Items.Clear();
                        _countnumFav = 0;
                        foreach (string items in _favSongsList.Keys)
                        {
                            _countnumFav++;
                            ListBoxHelper.AddItem(SongsFavsListBox, _countnumFav.ToString(), items);
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
            if (_favSongsList.ContainsKey(keyToRemove))
            {
                _favSongsList.Remove(keyToRemove);

                // Update the JSON file with the modified dictionary
                string json = JsonSerializer.Serialize(_favSongsList, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json);
                
                ListBoxHelper.RemoveItem(SongsFavsListBox, keyToRemove);
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

        private void RefreshListBox()
        {
            
            if (File.Exists(_jsonPath))
            {
                string json = File.ReadAllText(_jsonPath);
                
                // Deserialize the JSON string into a dictionary
                Dictionary<string, string> data = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                
                if (data != null)
                {
                    foreach (string key in data.Keys)
                    {
                        _countnumFav++;
                        
                        if (!_favSongsList.ContainsKey(key))
                        {
                            ListBoxHelper.AddItem(SongsFavsListBox, _countnumFav.ToString(), key);
                            _favSongsList.Add(key, data[key]);
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
            RemoveKeyAndUpdateFile(_jsonPath, GetSelectedDescription());
            SongsFavsListBox.Items.Clear();
            _countnumFav = 0;
            foreach (string items in _favSongsList.Keys)
            {
                _countnumFav++;
                ListBoxHelper.AddItem(SongsFavsListBox, _countnumFav.ToString(), items);
            }
        }

        private void PlayFav(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                if (!File.Exists(_favSongsList[GetSelectedDescription()]))
                {
                    MessageBox.Show("Music File wasnt found, Removing it.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    RemoveKeyAndUpdateFile(_jsonPath, PublicObjects.Jsons.GetValueFromJsonKey(_jsonPath, _favSongsList[GetSelectedDescription()]));
                    SongsFavsListBox.Items.Clear();
                    _countnumFav = 0;
                    foreach (string items in _favSongsList.Keys)
                    {
                        _countnumFav++;
                        ListBoxHelper.AddItem(SongsFavsListBox, _countnumFav.ToString(), items);
                    }
                    return;
                }
                if (GetSelectedDescription() != string.Empty)
                {
                    mainWindow.FavPlaySong(_favSongsList[GetSelectedDescription()]);
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
