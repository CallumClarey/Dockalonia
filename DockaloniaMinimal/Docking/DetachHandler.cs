
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using DockaloniaMinimal.Previews;
using DockaloniaMinimal.Tabbing;

namespace DockaloniaMinimal.Docking;

public class DetachHandler : IDisposable
{
    public PreviewWindow? PreviewWindow { get; set; }

    public DraggingOutTabArgs? CurrentDragArgs { get; private set; }
    
    private readonly List<Window> _trackedWindows = [];
    
    private IDockingControl? _currentPreviewOwner = null;
    
    /// <summary>
    /// Compares the live set of open windows against what we're currently tracking,
    /// and subscribes to anything new. Cheap enough to call on every pointer move.
    /// </summary>
    private void SyncTrackedWindows()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime lifetime)
            return;

        foreach (var window in lifetime.Windows)
        {
            if (!_trackedWindows.Contains(window))
                Track(window);
        }
    }

    private void Track(Window window)
    {
        _trackedWindows.Add(window);

        window.AddHandler(InputElement.PointerMovedEvent, OnAnyPointerMoved,
            RoutingStrategies.Tunnel, handledEventsToo: true);
        window.AddHandler(InputElement.PointerReleasedEvent, OnAnyPointerReleased,
            RoutingStrategies.Tunnel, handledEventsToo: true);
    }

    private void Untrack(Window window)
    {
        window.RemoveHandler(InputElement.PointerMovedEvent, OnAnyPointerMoved);
        window.RemoveHandler(InputElement.PointerReleasedEvent, OnAnyPointerReleased);
    }

    // enable preview window
    public void Enable(DraggingOutTabArgs e)
    {
        CurrentDragArgs = e;
        PreviewWindow =
            DockingAppMinimal.DragoutControlTemplate?.Invoke(e)
            ?? new ContentGhost(e.TabDragRef);
        
        PreviewWindow.Show();
        SyncTrackedWindows();
    }

    // On Pointer moved logic
    private void OnAnyPointerMoved(object? sender, PointerEventArgs e)
    {
        // Pick up any window that opened since the drag started, before moving the ghost.
        SyncTrackedWindows();
        UpdatePosition(e);
        
        // Catch if its null
        if(CurrentDragArgs is null) return;
        
        IsHoveredControlDockable(e, out var control);
        // If we are not hoving a dockable control 
        if(control is null)
        {
            _currentPreviewOwner?.HideDockingPreview(CurrentDragArgs with { Pointer = e });
            _currentPreviewOwner = null;
            return;
        }
        
        // If the control is not equal swap
        if (!control.Equals(_currentPreviewOwner))
        {
            _currentPreviewOwner?.HideDockingPreview(CurrentDragArgs with { Pointer = e });
            _currentPreviewOwner = control;
        }
     
        control.DisplayDockingPreview(CurrentDragArgs with { Pointer = e });
    }

    private void OnAnyPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        // If the there is no drag args we cant do anything
        if(CurrentDragArgs is null)
        {
            Dispose();
            return;
        }
        
        // Create new docking args with pointer event 
        var newDockingArgs = CurrentDragArgs with { Pointer = e };
        
        // Handle the dock
        if (IsHoveredControlDockable(e, out var control) && CurrentDragArgs is not null)
        {
            // Create docking args 
            control?.HandleDocking(newDockingArgs);
            control?.HideDockingPreview(newDockingArgs);
        }
        else
        {
            var screenPoint = new PixelPoint(0,0);
            if (e.Source is Visual source)
            {
                 screenPoint = source.PointToScreen(e.GetPosition(source));
                // screenPoint is a PixelPoint, valid across every monitor
                //Console.WriteLine(screenPoint.X + ":" + screenPoint.Y);
            }
            
            var createdWindow = 
                DockingAppMinimal.NewDockDropWindowTemplate?.Invoke(newDockingArgs, screenPoint) 
                ?? new DefaultDropoutWindow(newDockingArgs,screenPoint);
            
            createdWindow.Show();
        }
        
        Dispose();
    }

    
    /// <summary>
    /// Centers the ghost under the mouse cursor, in absolute screen coordinates
    /// so it can be dragged across monitor boundaries.
    /// </summary>
    public void UpdatePosition(PointerEventArgs e)
    {
        if(PreviewWindow is null) return;
        
        // Use whichever window actually raised the event as the coordinate
        // reference, since it's guaranteed to still be attached to a visual tree.
        if (e.Source is not Visual source)
            return;

        var screenPoint = source.PointToScreen(e.GetPosition(source));
        var contentContainer = PreviewWindow.ContentContainer;

        var width = contentContainer.Bounds.Width > 0 ? contentContainer.Bounds.Width : contentContainer.DesiredSize.Width;
        var height = contentContainer.Bounds.Height > 0 ? contentContainer.Bounds.Height : contentContainer.DesiredSize.Height;

        PreviewWindow.Position = new PixelPoint(
            screenPoint.X - (int)(width / 2),
            screenPoint.Y - (int)(height / 2));
    }

    
    /// <summary>
    /// Clean up the object make sure that everything is reset ready for next drag.
    /// </summary>
    public void Dispose()
    {
        foreach (var window in _trackedWindows) Untrack(window);
        PreviewWindow?.Hide();
        PreviewWindow = null;
        CurrentDragArgs = null;
        _trackedWindows.Clear();
    }

    // Will check what element is under the mouse
    private bool IsHoveredControlDockable(PointerEventArgs e, out IDockingControl? control)
    {
        // if the source isn't visual return 
        if (e.Source is not Visual visual)
        {
            control = null;
            return false;
        }
        
        // Look under all windows for hit test
        var screenPoint = visual.PointToScreen(e.GetPosition(visual));
        var result = HitTestAllWindows(screenPoint);

        if (result is not { } found)
        {
            control = null;
            return false;
        }
        
        var targetControl = found.hit
            .GetSelfAndVisualAncestors()
            .OfType<IDockingControl>()
            .FirstOrDefault();
        
        control = targetControl;
        return targetControl is not null;
    }
    
    
    /// <summary>
    /// Hit-tests a screen-space point against every tracked window, returning the
    /// first (topmost, hit-testable) visual found, and which window it belongs to.
    /// </summary>
    private (Window window, Visual hit)? HitTestAllWindows(PixelPoint screenPoint)
    {
        foreach (var window in _trackedWindows)
        {
            // Is the point even within this window's bounds? Skip the hit-test if not.
            var bounds = new PixelRect(window.Position, PixelSize.FromSize(window.ClientSize, window.RenderScaling));
            if (!bounds.Contains(screenPoint))
                continue;

            var localPoint = window.PointToClient(screenPoint);
            var hit = window.InputHitTest(localPoint);

            if (hit is Visual visual)
                return (window, visual);
        }
        return null;
    }
}