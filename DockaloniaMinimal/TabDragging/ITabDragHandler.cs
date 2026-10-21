using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using DockaloniaMinimal.Tabbing;
using DockaloniaMinimal.Tabbing.TabControls;

namespace DockaloniaMinimal.TabDragging;

/// <summary>
/// Interface used to handle the tab dragging in the <see cref="RearrangeTabControl"/>
/// Allows user to implement their own dragging logic if the default is undesirable.
/// </summary>
public interface ITabDragHandler
{
    /// <summary>
    /// The <see cref="RearrangeTabControl"/> that owns the handler.
    /// </summary>
    public RearrangeTabControl? Owner {get; init; }
    
    /// <summary>
    ///  Function to be called when the drag starts.
    /// </summary>
    /// <param name="e"> Pointer event.</param>
    /// <param name="draggedTab"> The dragging tab.</param>
    public void OnDragStarted(PointerEventArgs e, (TabItem tabItem, Point topLeft) draggedTab);
    
    
    /// <summary>
    ///  Function used to implement all dragging logic.
    /// </summary>
    /// <param name="e"></param>
    /// <param name="draggedTab"></param>
    public void HandleTabDragging(PointerEventArgs e, (TabItem tabItem, Point topLeft) draggedTab);


    /// <summary>
    /// Function to be called when the drag should end 
    /// </summary>
    /// <param name="draggedTab"></param>
    public void OnDragCompleted((TabItem tabItem, Point topLeft) draggedTab);
    
    
    /// <summary>
    ///  Returns a static <see cref="ITabDragHandler"/> that does provide any functions implementations to disable dragging.
    /// </summary>
    public static StaticDragHandler NoDrag(RearrangeTabControl control) => new(control);
    
    
    /// <summary>
    ///  Class used to represent a static drag handler it does nothing mean <see cref="TabItem"/> will not react.
    /// </summary>
    public class StaticDragHandler(RearrangeTabControl control) : ITabDragHandler
    {
        public RearrangeTabControl? Owner { get; init; } = control;
        public void HandleTabDragging(PointerEventArgs e, (TabItem tabItem, Point topLeft) draggedTab) { }

        public void OnDragStarted(PointerEventArgs e, (TabItem tabItem, Point topLeft) draggedTab) { }

        public void OnDragCompleted((TabItem tabItem, Point topLeft) draggedTab) { }
    }
}