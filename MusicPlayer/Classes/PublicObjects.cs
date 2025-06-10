using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MusicPlayer.Shared;

namespace MusicPlayer.Classes
{
    public class PublicObjects
    {
        public static string timeFormat(DateTime currentTime, string formatPerameter)
        {
            if (formatPerameter == "12")
            {
                return currentTime.ToString("h:mm tt");
            }
            else
            {
                return currentTime.ToString("HH:mm:ss");
            }
            
        }

        public static List<String> playlistSongs = new List<String>();

        public static void PlayMusic(MediaElement mediaElement, string filepath)
        {
            if (MainWindow.youtubeMusicPlaying)
            {
                MessageBoxService.YoutubeMusicPlaying();
            }
            else
            {
                mediaElement.Source = new Uri(filepath, UriKind.RelativeOrAbsolute);
                mediaElement.Play();
            }
        }

        public class Jsons
        {

            public static string GetValueFromJsonKey(string jsonPath, string targetKey)
            {
                string json = File.ReadAllText(jsonPath);
                Dictionary<string, string> data = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (data.TryGetValue(targetKey, out string targetValue))
                {
                    return targetValue;
                }
                else
                {
                    return null;
                }
            }

            public static void AddDataToJsonFile(string filePath, Dictionary<string, string> newData)
            {
                string directoryPath = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                Dictionary<string, string> existingData = new Dictionary<string, string>();
                if (File.Exists(filePath))
                {
                    string existingJson = File.ReadAllText(filePath);
                    existingData = JsonSerializer.Deserialize<Dictionary<string, string>>(existingJson);
                }

                foreach (var entry in newData)
                {
                    existingData[entry.Key] = entry.Value;
                }

                string jsonString = JsonSerializer.Serialize(existingData, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, jsonString);
            }

            public class JsonFilePaths
            {
                public static readonly string settingsJsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MusicPlayer/Data/Settings.json");
                public static readonly string favouriteJsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MusicPlayer/Data/Favourites.json");
            }

            public class SettingsJsonFileKeys
            {
                public static readonly string showTimeKeyJson = "ShowTime";
                public static readonly string timeFormatKeyJson = "TimeFormat";
                public static readonly string keepPlayingKeyJson = "YTMusicKeepPlaying";
            }
        }

        public class ListBoxs
        {
            public static void AddItemToListBox(ListBox listBox, string itemName, string itemDescription)
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

                listBox.Items.Add(listBoxItem);
            }

            public static void RemoveItemFromListBox(ListBox listbox, string keyToRemove)
            {
                var itemsToRemove = new List<ListBoxItem>();
                foreach (var item in listbox.Items.OfType<ListBoxItem>())
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
                foreach (var itemToRemove in itemsToRemove)
                {
                    listbox.Items.Remove(itemToRemove);
                }
            }
        }
    }
}