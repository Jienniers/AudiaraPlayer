using System.Windows;

namespace Audiara.Shared;

public static class MessageBoxService
{
    public static void ShowError(string message)
    {
        MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public static void ShowSuccess(string message)
    {
        MessageBox.Show(message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public static void NoSongPlaying() => ShowError("No song playing.");
    public static void FileNotFound() => ShowError("File not found.");
    public static void FavouriteAdded(string songName) => ShowSuccess($"{songName} was added to favourites.");
}
