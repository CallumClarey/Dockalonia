using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace DockaloniaMinimal.Docking;



/// <summary>
///     Arguments for when a tab is dragged out of its control container.
///     Provides the first pointer position for when the tab is first detached.
/// </summary>
/// <param name="Pointer">Last Positon of the pointer when it leaves the <see cref="TabControl"/>.</param>
/// <param name="TabDragRef"> Reference to the dragged out <see cref="TabItem"/>.</param>
/// <param name="Offset"> The offset of the drag to top left corner of the box.</param>
/// <param name="ContentSize"> The size of the content in the tab.</param>
public record DraggingOutTabArgs(
    PointerEventArgs Pointer,
    TabItem TabDragRef,
    Point? Offset,
    Size? ContentSize
);
