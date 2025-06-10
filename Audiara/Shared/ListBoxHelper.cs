using System.Windows;
using System.Windows.Controls;

namespace Audiara.Shared;

public static class ListBoxHelper
{
    public static void AddItem(ListBox targetListBox, string title, string subtitle)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal
        };

        var titleText = new TextBlock
        {
            Text = title,
            Margin = new Thickness(5)
        };

        var subtitleText = new TextBlock
        {
            Text = subtitle,
            Margin = new Thickness(5)
        };

        panel.Children.Add(titleText);
        panel.Children.Add(subtitleText);

        var listItem = new ListBoxItem
        {
            Content = panel,
            Tag = subtitle
        };

        targetListBox.Items.Add(listItem);
    }

    public static void RemoveItem(ListBox targetListBox, string matchText)
    {
        var itemsToRemove = new List<ListBoxItem>();

        foreach (var item in targetListBox.Items.OfType<ListBoxItem>())
        {
            if (item.Content is StackPanel panel)
            {
                foreach (var child in panel.Children)
                {
                    if (child is TextBlock textBlock && textBlock.Text == matchText)
                    {
                        itemsToRemove.Add(item);
                        break;
                    }
                }
            }
        }

        foreach (var item in itemsToRemove)
        {
            targetListBox.Items.Remove(item);
        }
    }
}
