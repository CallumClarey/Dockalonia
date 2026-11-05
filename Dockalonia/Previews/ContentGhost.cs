using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Dockalonia.Docking;

namespace Dockalonia.Previews;



/// <summary>
/// Drag preview shown while a tab is being dragged, displaying a semi-transparent copy
/// of the tab header under the pointer.
/// </summary>
public class ContentGhost : PreviewWindow
{
    // Apply Default content 
    /// <summary>
    /// Creates the ghost preview for the given tab, copying its header size and styling.
    /// </summary>
    /// <param name="sourceTab">The tab whose header is shown in the ghost.</param>
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

    /// <summary>
    /// Tints the ghost red to show that the tab can be dropped on the control under the pointer.
    /// </summary>
    /// <param name="draggingOutTabArgs">The arguments describing the tab being dragged.</param>
    public override void OnHoveringDockable(DraggingOutTabArgs draggingOutTabArgs)
    {
        Console.WriteLine($"OnHoveringDockable(DraggingOutTabArgs)");
        ContentContainer.Background = new SolidColorBrush(Colors.DarkRed, 0.4);
        ContentContainer.BorderBrush = new SolidColorBrush(Colors.DarkRed, 0.8);
    }
    
    /// <summary>
    /// Restores the default ghost colours when the pointer is not over a dockable control.
    /// </summary>
    /// <param name="draggingOutTabArgs">The arguments describing the tab being dragged.</param>
    public override void OnNotHoveringDockable(DraggingOutTabArgs draggingOutTabArgs)
    {
        Console.WriteLine($"OnNotHoveringDockable(DraggingOutTabArgs)");
        ContentContainer.Background = new SolidColorBrush(Colors.White, 0.4);
        ContentContainer.BorderBrush = new SolidColorBrush(Colors.White, 0.8);
    }
}