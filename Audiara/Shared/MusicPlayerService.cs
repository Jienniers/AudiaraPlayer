using System.Windows.Controls;

namespace Audiara.Shared;

public static class MusicPlayerService
{
    public static void PlayMusic(MediaElement mediaElement, string filepath)
    {
        mediaElement.Source = new Uri(filepath, UriKind.RelativeOrAbsolute);
        mediaElement.Play();
    }
}