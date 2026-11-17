using Avalonia.Controls;
using Avalonia.Layout;
using DockaloniaDockspace;

namespace Test.Spaces;

public partial class MainWindow : Window
{
    /// <summary>
    /// Wires the toolbar controls to the top-level <see cref="SplitContainer"/> so the
    /// orientation and split ratio can be driven from either the toolbar or the splitter.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();

        OrientationBox.SelectionChanged += (_, _) => ApplyOrientation();

        RatioSlider.PropertyChanged += (_, e) =>
        {
            if (e.Property == Slider.ValueProperty)
                Split.Ratio = RatioSlider.Value;
        };

        // Keep the slider in sync when the splitter bar is dragged directly.
        Split.PropertyChanged += (_, e) =>
        {
            if (e.Property != SplitContainer.RatioProperty) return;
            RatioSlider.Value = Split.Ratio;
            RatioLabel.Text = Split.Ratio.ToString("P0");
        };

        ApplyOrientation();
        RatioLabel.Text = Split.Ratio.ToString("P0");
    }

    private void ApplyOrientation() =>
        Split.Orientation = OrientationBox.SelectedIndex == 1
            ? Orientation.Vertical
            : Orientation.Horizontal;
}
