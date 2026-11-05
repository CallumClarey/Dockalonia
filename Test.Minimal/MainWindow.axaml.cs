using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Dockalonia.Tabbing.TabControls;
using Dockalonia.Tabbing.Tabs;

namespace Test.Minimal;

public partial class MainWindow : Window
{
    private Point _lastPointer;

    /// <summary>
    /// Loads the window XAML and wires up the pointer tracking and the P key shortcut
    /// that toggles pinning for the tab under the pointer.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        PointerMoved += (_, e) => _lastPointer = e.GetPosition(this);
        KeyDown += (_, e) =>
        {
            if (e.Key != Key.P) return;
            foreach (var tabControl in this.GetVisualDescendants().OfType<DockaloniaTabControl>())
            {
                var local = this.TranslatePoint(_lastPointer, tabControl) ?? _lastPointer;
                int idx;
                if (!tabControl.GetTabItemAt(local, out var tabItem, out idx) ||
                    tabItem is not PinnableTab tab) continue;
                tab.IsPinned = !tab.IsPinned;
                break;
            }
        };
    }
}
