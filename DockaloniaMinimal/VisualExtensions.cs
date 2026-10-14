using Avalonia;

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
    ///  Extension method used to normalize points.
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public static Point NormalisePoint(this Point point)
    {
        var pointX = point.X;
        var pointY = point.Y;
        var length = Math.Sqrt(pointX * pointX + pointY * pointY);
        return new Point(pointX / length, pointY / length);
    }
    
    
}