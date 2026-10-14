using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using DockaloniaMinimal;
using DockaloniaMinimal.Docking;
using DockaloniaMinimal.Previews;

namespace Test.Minimal;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        
        // Init docking code 
        // All of this code will bind the fallback values but these values can be chnaged.
        DockingAppMinimal.InitDocking(); // Required line 
        DockingAppMinimal.PreviewTabDockTemplate = tab => new PreviewDropTab(tab);
        DockingAppMinimal.NewDockDropWindowTemplate = dragArgs => new Window();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}