using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Dockalonia;
using Dockalonia.Docking;
using Dockalonia.Previews;

namespace Test.Minimal;

public partial class App : Application
{
    /// <summary>
    /// Loads the application XAML and enables docking using the sample templates.
    /// </summary>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        
        // Init docking code 
        // All of this code will bind the custom configure values that can be changed 
        DockingApp.InitDocking(); // Required line 
        DockingApp.PreviewTabDockTemplate = tab => new PreviewDropTab(tab);
        DockingApp.NewDockDropWindowTemplate = (dragArgs, point) => new DefaultDropoutWindow(dragArgs, point);
        DockingApp.DragoutControlTemplate = args => new ContentGhost(args.TabDragRef);
    }

    /// <summary>
    /// Creates and assigns the sample main window once Avalonia has finished initializing.
    /// </summary>
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}