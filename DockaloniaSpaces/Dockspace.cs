using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Dockalonia.Docking;
using Dockalonia.Tabbing.TabControls;

namespace DockaloniaDockspace;

public class Dockspace : SplitContainer, IDockingControl
{
    protected DetachableTabControl? FirstControl { get; private set; }
    protected DetachableTabControl? SecondControl { get; private set; }
    
    
    protected override void InjectSplitPanelContent(Grid mainGrid, ContentPresenter content)
    {
   
    }


    void IDockingControl.HandleDocking(DraggingOutTabArgs args)
    {
    }

    void IDockingControl.DisplayDockingPreview(DraggingOutTabArgs args)
    {
    }

    void IDockingControl.HideDockingPreview(DraggingOutTabArgs args)
    {
    }
}