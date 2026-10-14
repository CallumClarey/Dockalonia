using Avalonia.Controls;
using Avalonia.Media;

namespace DockaloniaMinimal.Previews;


/// <summary>
///  A special <see cref="TabItem"/> that will be used to display,
/// When a use reattaches a tab to a tab control.
/// </summary>
public class PreviewDropTab : TabItem
{
    // Style it as a tab item
    protected override Type StyleKeyOverride => typeof(TabItem);

    /// <summary>
    /// Set the default parameters of this tabItem.
    /// </summary>
    public PreviewDropTab(TabItem tab)
    {
        Background = new SolidColorBrush(Colors.Gray, 0.1);
        BorderBrush = new SolidColorBrush(Colors.Gray, 1);
        IsHitTestVisible = false;
        Focusable = false; 
        Width = tab.Bounds.Width;
        Height = tab.Bounds.Height;
    }
}