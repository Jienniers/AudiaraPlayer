using MusicPlayer.Classes;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace MusicPlayer.Dialogs
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        private string settingsJsonFilePath = PublicObjects.Jsons.JsonFilePaths.settingsJsonFilePath;
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
                        string keepplayingYoutubeMusic = PublicObjects.Jsons.GetValueFromJsonKey(settings.settingsJsonFilePath, settings.keepPlayingKeyJson);
                        
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
                        UpdateFolderSettingData.Add(settings.keepPlayingKeyJson, "False");
                        PublicObjects.Jsons.AddDataToJsonFile(settings.settingsJsonFilePath, UpdateFolderSettingData);
                        settings.KeepPlayingYoutubeMusicComboBox.SelectedIndex = 1;
                    }
                }
            }
        }

        private void KeepPlayingComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Keep the music playing or not after YT Music has been closed
            int timeFormatselectedIndex = KeepPlayingYoutubeMusicComboBox.SelectedIndex;
            Dictionary<string, string> updateFolderSettingData = new Dictionary<string, string>();
            if (timeFormatselectedIndex == 0)
            {
                //MainWindow.youtubeMusicPlaying = true;
                updateFolderSettingData.Add(keepPlayingKeyJson, "true");
                PublicObjects.Jsons.AddDataToJsonFile(settingsJsonFilePath, updateFolderSettingData);
            }
            else if (timeFormatselectedIndex == 1)
            {
                //MainWindow.youtubeMusicPlaying = false;
                updateFolderSettingData.Add(keepPlayingKeyJson, "false");
                PublicObjects.Jsons.AddDataToJsonFile(settingsJsonFilePath, updateFolderSettingData);
            }
        }
    }
}
