using System;
using System.Collections.Generic;
using System.Configuration;
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
using Path = System.IO.Path;

namespace MusicPlayer.Dialogs
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        private string settingsJsonFilePath = "Data/Settings.json";
        

        public Settings()
        {
            InitializeComponent();
            Startup();
        }

        private void Startup()
        {
            Functions.starupFunctions startupFunction = new Functions.starupFunctions();
            startupFunction.AddFolderUpdateValueJson(this);
        }
        class Functions
        {
            public class starupFunctions
            {
                public void AddFolderUpdateValueJson(Settings settings)
                {
                    Functions functions = new Functions();
                    string updateFolderKey = "AddFolderUpdate";
                    if (File.Exists(settings.settingsJsonFilePath))
                    {
                        string value = functions.GetValueFromJsonKey(settings.settingsJsonFilePath, "AddFolderUpdate");
                        switch (value)
                        {
                            case "true":
                                settings.AddFolderComboBox.SelectedIndex = 0;
                                break;
                            case "false":
                                settings.AddFolderComboBox.SelectedIndex = 1;
                                break;
                            case null:
                                settings.AddFolderComboBox.SelectedIndex = 1;
                                break;
                        }
                    }
                    else
                    {
                        Dictionary<string, string> UpdateFolderSettingData = new Dictionary<string, string>();
                        UpdateFolderSettingData.Add(updateFolderKey, "true");
                        functions.AddDataToJsonFile(settings.settingsJsonFilePath, UpdateFolderSettingData);
                        settings.AddFolderComboBox.SelectedIndex = 0;
                    }
                }
            }

            public void AddDataToJsonFile(string filePath, Dictionary<string, string> newData)
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
                    // Add or update the key-value pair
                    existingData[entry.Key] = entry.Value;
                }

                // Serialize the updated dictionary back to JSON
                string jsonString = JsonSerializer.Serialize(existingData, new JsonSerializerOptions { WriteIndented = true });

                // Write the JSON data back to the file
                File.WriteAllText(filePath, jsonString);
            }

            public string GetValueFromJsonKey(string jsonPath, string targetKey)
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
        }

        private void AddFolderComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Functions functions = new Functions();
            int selectedIndex = AddFolderComboBox.SelectedIndex;
            string updateFolderKey = "AddFolderUpdate";
            Dictionary<string, string> UpdateFolderSettingData = new Dictionary<string, string>();
            if (selectedIndex == 0)
            {
                UpdateFolderSettingData.Add(updateFolderKey, "true");
                functions.AddDataToJsonFile(settingsJsonFilePath, UpdateFolderSettingData);
            }
            else if (selectedIndex == 1)
            {
                UpdateFolderSettingData.Add(updateFolderKey, "false");
                functions.AddDataToJsonFile(settingsJsonFilePath, UpdateFolderSettingData);
            }
        }
    }
}
