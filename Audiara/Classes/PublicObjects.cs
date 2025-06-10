using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Audiara.Shared;

namespace Audiara.Classes
{
    public class PublicObjects
    {

        public static List<String> PlaylistSongs = new List<String>();

        public static void PlayMusic(MediaElement mediaElement, string filepath)
        {
            mediaElement.Source = new Uri(filepath, UriKind.RelativeOrAbsolute);
            mediaElement.Play();
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
                public static readonly string FavouriteJsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Audiara/Data/Favourites.json");
            }
        }
    }
}