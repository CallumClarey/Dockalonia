using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace DockaloniaMinimal.Previews;



/// <summary>
/// Control displayed when you hover over the 
/// </summary>
public class ContentGhost : PreviewWindow
{
    // Apply Default content 
    [SetsRequiredMembers]
    public ContentGhost(TabItem sourceTab)
    {
        ContentContainer = new Border
        {
            // High transparency background (alpha 0.4)
            Background = new SolidColorBrush(Colors.GhostWhite, 0.4),
            BorderBrush = new SolidColorBrush(Colors.GhostWhite, 0.8),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(2),
            Padding = new Thickness(2),
            IsHitTestVisible = false,
            Width = sourceTab.Width,
            Height = sourceTab.Height,
            Child = new ContentControl
            {
                Content = sourceTab.Header,
                ContentTemplate = sourceTab.HeaderTemplate,
                FontSize = sourceTab.FontSize,
            }
        };

        // Measure layout immediately so DesiredSize is populated
        ContentContainer.Measure(Size.Infinity);
        ContentContainer.Arrange(new Rect(ContentContainer.DesiredSize));
        
        WindowDecorations = WindowDecorations.None;
        ShowInTaskbar = false;
        Topmost = true;
        CanResize = false;
        SizeToContent = SizeToContent.WidthAndHeight;
        Background = Brushes.Transparent;
        TransparencyLevelHint = [WindowTransparencyLevel.Transparent];
        Content = ContentContainer;
    }

}