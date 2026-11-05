
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Dockalonia.Previews;
using Dockalonia.Tabbing;
using Dockalonia.Tabbing.TabControls;

namespace Dockalonia.Docking;

/// <summary>
/// Owns the lifetime of a tab detach operation: shows the drag preview, tracks pointer
/// events across every open window, and either docks the tab or opens a new window for it.
/// </summary>
public class DetachHandler : IDisposable
{
    public PreviewWindow? PreviewWindow { get; set; }

    public DraggingOutTabArgs? CurrentDragArgs { get; private set; }
    
    private readonly List<Window> _trackedWindows = [];
    
    private IDockingControl? _currentPreviewOwner = null;
    
    private DetachableTabControl? _currentTabControlOwner = null;
    
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

    /// <summary>
    /// Starts listening for pointer movement and release events on the given window so a
    /// drag can continue across window boundaries.
    /// </summary>
    /// <param name="window">The window to track.</param>
    private void Track(Window window)
    {
        _trackedWindows.Add(window);

        window.AddHandler(InputElement.PointerMovedEvent, OnAnyPointerMoved,
            RoutingStrategies.Tunnel, handledEventsToo: true);
        window.AddHandler(InputElement.PointerReleasedEvent, OnAnyPointerReleased,
            RoutingStrategies.Tunnel, handledEventsToo: true);
    }

    /// <summary>
    /// Stops listening for pointer events on the given window.
    /// </summary>
    /// <param name="window">The window to stop tracking.</param>
    private void Untrack(Window window)
    {
        window.RemoveHandler(InputElement.PointerMovedEvent, OnAnyPointerMoved);
        window.RemoveHandler(InputElement.PointerReleasedEvent, OnAnyPointerReleased);
    }

    /// <summary>
    /// Starts a detach operation by creating the drag preview window for the given tab
    /// and beginning to track the pointer across all open windows.
    /// </summary>
    /// <param name="e">The arguments describing the tab being dragged out.</param>
    /// <param name="control">The tab control that the tab was detached from.</param>
    public void Enable(DraggingOutTabArgs e, DetachableTabControl control)
    {
        // Make sure all previous data is gone
        CurrentDragArgs = e;
        PreviewWindow =
            DockingApp.DragoutControlTemplate?.Invoke(e)
            ?? new ContentGhost(e.TabDragRef);
        
        _currentTabControlOwner = control;
        PreviewWindow.Show();
        SyncTrackedWindows();
    }

    /// <summary>
    /// Moves the drag preview with the pointer and updates the docking preview of the
    /// dockable control currently under it.
    /// </summary>
    /// <param name="sender">The window that raised the event.</param>
    /// <param name="e">The pointer event arguments.</param>
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
            PreviewWindow?.OnNotHoveringDockable(CurrentDragArgs);
            _currentPreviewOwner?.HideDockingPreview(CurrentDragArgs with { Pointer = e });
            _currentPreviewOwner = null;
            return;
        }
         
        PreviewWindow?.OnHoveringDockable(CurrentDragArgs);
        
        // If the control is not equal swap
        if (!control.Equals(_currentPreviewOwner))
        {
            _currentPreviewOwner?.HideDockingPreview(CurrentDragArgs with { Pointer = e });
            _currentPreviewOwner = control;
        }
     
        control.DisplayDockingPreview(CurrentDragArgs with { Pointer = e });
    }

    /// <summary>
    /// Finishes the drag when the pointer is released, either docking the tab into the
    /// control under the pointer or dropping it into a new window.
    /// </summary>
    /// <param name="sender">The window that raised the event.</param>
    /// <param name="e">The pointer event arguments.</param>
    private void OnAnyPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        // If the there is no drag args we cant do anything
        if(CurrentDragArgs is null || PreviewWindow is null)
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
            // Get the drop position
            var screenPoint = new PixelPoint(0, 0);
            if(sender is Visual visual) 
                GetDropPosition(visual, e, out screenPoint);
         
            // Create a new window
            var createdWindow = 
                DockingApp.NewDockDropWindowTemplate?.Invoke(newDockingArgs, screenPoint) 
                ?? new DefaultDropoutWindow(newDockingArgs,screenPoint);
            
            createdWindow.Show();
        }
        
        // Close any empty windows
        CloseWindowOnEmptyControl();
        Dispose();
    }

    
    /// <summary>
    /// Calculates the screen position for a dropped tab, clamped so the new window stays
    /// inside the working area of the screen under the pointer.
    /// </summary>
    /// <param name="visual">The visual that the pointer event belongs to.</param>
    /// <param name="e">The pointer event arguments.</param>
    /// <param name="position">The clamped screen position for the dropped window.</param>
    private void GetDropPosition(Visual visual, PointerEventArgs e, out PixelPoint position)
    {
        // Get the top level 
        var topLevel = TopLevel.GetTopLevel(visual);
        if (topLevel is null)
        {
            position = new PixelPoint(0,0);
            return;
        }
    
        // Get the mouse position in screen coordinates
        var clientPoint = e.GetPosition(visual);
        var screenPoint = visual.PointToScreen(clientPoint);
        
        // Get the screen
        var screen = topLevel.Screens?.ScreenFromPoint(screenPoint);
        if (screen is null)
        {
            position = new PixelPoint(0, 0);
            return;
        }

        var width = (int)(CurrentDragArgs?.ContentSize?.Width ?? 0);
        var height = (int)(CurrentDragArgs?.ContentSize?.Height ?? 0);
        var targetRect = new PixelRect(screenPoint.X, screenPoint.Y, Math.Max(0, width), Math.Max(0, height));
        var workingArea = screen.WorkingArea;
        
        // Line to check if overflowing
        var clampedX = screenPoint.X;
        var clampedY = screenPoint.Y;

        if (targetRect.Right > workingArea.Right)
            // 15 to avoid slight lip over onto other monitor
            clampedX = workingArea.Right - targetRect.Width - 15;
        if (targetRect.Bottom > workingArea.Bottom)
            clampedY = workingArea.Bottom - targetRect.Height;

        clampedX = Math.Max(workingArea.X, Math.Min(clampedX, workingArea.Right));
        clampedY = Math.Max(workingArea.Y, Math.Min(clampedY, workingArea.Bottom));

        position = new PixelPoint(clampedX, clampedY);
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
        PreviewWindow?.Close();
        PreviewWindow = null;
        CurrentDragArgs = null;
        _currentTabControlOwner = null;
        _trackedWindows.Clear();
    }

    /// <summary>
    /// Finds the dockable control under the pointer, if there is one, by hit testing
    /// every tracked window and walking up the visual tree.
    /// </summary>
    /// <param name="e">The pointer event arguments.</param>
    /// <param name="control">The dockable control under the pointer, or <see langword="null"/>.</param>
    /// <returns><see langword="true"/> when a dockable control was found.</returns>
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
        
        // Will walk up and find the first instance of able control
        // Example if the nested control is not dockable will walk upo
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

    /// <summary>
    ///  Closes the window that owns the control when the control is empty and no other
    ///  tab control on screen still has tabs.
    /// </summary>
    public void CloseWindowOnEmptyControl()
    {
        if(_currentTabControlOwner is null) return;
        // Check if the control is empty
        if(!_currentTabControlOwner.IsEmpty) return;
        
        // Get window and then get tab controls
        var window = _currentTabControlOwner.FindAncestorOfType<Window>();
        var detachableTabControls = window?.GetVisualDescendants()
            .OfType<DetachableTabControl>();
        if(detachableTabControls is null) return;
         
        // Loop through and check all of them are empty
        if (detachableTabControls.Any(control => !control.IsEmpty)) return;
        window?.Close();
    }
    
}