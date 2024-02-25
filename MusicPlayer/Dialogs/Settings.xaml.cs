using MusicPlayer.Classes;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Taskbar;
using Path = System.IO.Path;

namespace MusicPlayer.Dialogs
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        private string settingsJsonFilePath = PublicObjects.Jsons.JsonFilePaths.settingsJsonFilePath;
        private string showTimeKeyJson = PublicObjects.Jsons.SettingsJsonFileKeys.showTimeKeyJson;
        private string timeFormatKeyJson = PublicObjects.Jsons.SettingsJsonFileKeys.timeFormatKeyJson;
        private string keepPlayingKeyJson = PublicObjects.Jsons.SettingsJsonFileKeys.keepPlayingKeyJson;


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
                    if (File.Exists(settings.settingsJsonFilePath))
                    {
                        string showTime = PublicObjects.Jsons.GetValueFromJsonKey(settings.settingsJsonFilePath, settings.showTimeKeyJson);
                        string timeFormat = PublicObjects.Jsons.GetValueFromJsonKey(settings.settingsJsonFilePath, settings.timeFormatKeyJson);
                        string keepplayingYoutubeMusic = PublicObjects.Jsons.GetValueFromJsonKey(settings.settingsJsonFilePath, settings.keepPlayingKeyJson);

                        switch (showTime)
                        {
                            case "true":
                                settings.ShowTimeComboBox.SelectedIndex = 0;
                                break;
                            case "false":
                                settings.ShowTimeComboBox.SelectedIndex = 1;
                                break;
                            case null:
                                settings.ShowTimeComboBox.SelectedIndex = 1;
                                break;
                        }

                        switch (timeFormat)
                        {
                            case "12":
                                settings.timeFormatComboBox.SelectedIndex = 0;
                                break;
                            case "24":
                                settings.timeFormatComboBox.SelectedIndex = 1;
                                break;
                        }

                        switch (keepplayingYoutubeMusic)
                        {
                            case "true":
                                settings.KeepPlayingYoutubeMusicComboBox.SelectedIndex = 0;
                                break;
                            case "false":
                                settings.KeepPlayingYoutubeMusicComboBox.SelectedIndex = 1;
                                break;
                        }
                    }
                    else
                    {
                        Dictionary<string, string> UpdateFolderSettingData = new Dictionary<string, string>();
                        UpdateFolderSettingData.Add(settings.showTimeKeyJson, "true");
                        UpdateFolderSettingData.Add(settings.timeFormatKeyJson, "12");
                        UpdateFolderSettingData.Add(settings.keepPlayingKeyJson, "False");
                        PublicObjects.Jsons.AddDataToJsonFile(settings.settingsJsonFilePath, UpdateFolderSettingData);
                        settings.ShowTimeComboBox.SelectedIndex = 0;
                        settings.timeFormatComboBox.SelectedIndex = 0;
                        settings.KeepPlayingYoutubeMusicComboBox.SelectedIndex = 1;
                    }
                }
            }
        }

        private void ShowTimeComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Show Time or not Combo Box
            int showTimeselectedIndex = ShowTimeComboBox.SelectedIndex;
            Dictionary<string, string> UpdateFolderSettingData = new Dictionary<string, string>();
            if (showTimeselectedIndex == 0)
            {
                UpdateFolderSettingData.Add(showTimeKeyJson, "true");
                PublicObjects.Jsons.AddDataToJsonFile(settingsJsonFilePath, UpdateFolderSettingData);
            }
            else if (showTimeselectedIndex == 1)
            {
                UpdateFolderSettingData.Add(showTimeKeyJson, "false");
                PublicObjects.Jsons.AddDataToJsonFile(settingsJsonFilePath, UpdateFolderSettingData);
            }

        }

        private void TimeFormatComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //12 hour or 24 Hour ComboBox Change
            int timeFormatselectedIndex = timeFormatComboBox.SelectedIndex;
            Dictionary<string, string> UpdateFolderSettingData = new Dictionary<string, string>();
            if (timeFormatselectedIndex == 0)
            {
                UpdateFolderSettingData.Add(timeFormatKeyJson, "12");
                PublicObjects.Jsons.AddDataToJsonFile(settingsJsonFilePath, UpdateFolderSettingData);
            }
            else if (timeFormatselectedIndex == 1)
            {
                UpdateFolderSettingData.Add(timeFormatKeyJson, "24");
                PublicObjects.Jsons.AddDataToJsonFile(settingsJsonFilePath, UpdateFolderSettingData);
            }
        }


        private void KeepPlayingComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Keep the music playing or not after YT Music has been closed
            int timeFormatselectedIndex = KeepPlayingYoutubeMusicComboBox.SelectedIndex;
            Dictionary<string, string> UpdateFolderSettingData = new Dictionary<string, string>();
            if (timeFormatselectedIndex == 0)
            {
                //MainWindow.youtubeMusicPlaying = true;
                UpdateFolderSettingData.Add(keepPlayingKeyJson, "true");
                PublicObjects.Jsons.AddDataToJsonFile(settingsJsonFilePath, UpdateFolderSettingData);
            }
            else if (timeFormatselectedIndex == 1)
            {
                //MainWindow.youtubeMusicPlaying = false;
                UpdateFolderSettingData.Add(keepPlayingKeyJson, "false");
                PublicObjects.Jsons.AddDataToJsonFile(settingsJsonFilePath, UpdateFolderSettingData);
            }
        }
    }
}
