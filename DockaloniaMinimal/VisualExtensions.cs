using Avalonia;
using Avalonia.Controls;

namespace DockaloniaMinimal;

public static class VisualExtensions
{
    /// <summary>
    /// Extension method to allow a user to get the bounding rect of a visual element.
    /// </summary>
    /// <param name="self"></param>
    /// <param name="visual"></param>
    /// <returns></returns>
    public static Rect GetBoundsOf(this Visual self, Visual visual) 
    {
        var topLeft = visual.TranslatePoint(new Point(0, 0), self)!.Value;
        var bottomRight = visual.TranslatePoint(new Point(visual.Bounds.Width, visual.Bounds.Height), self)!.Value;
        return new Rect(topLeft, bottomRight);
    }

    /// <summary>
    ///  Extends the visual to allow Pseudo classes to be toggled on and off.
    /// </summary>
    /// <param name="self">The visual.</param>
    /// <param name="className"> The name of the pseudo class to add.</param>
    /// <param name="toggleValue">Whether the class should be on or off.</param>
    public static void SetPseudoClass(this StyledElement self,string className ,bool toggleValue) 
        => ((IPseudoClasses)self.Classes)
            .Set(className[0] == ':' ? className : ':' + className, toggleValue);
    
}