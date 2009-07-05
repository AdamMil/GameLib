using System;
using System.Drawing;
using GameLib.Input;
using GameLib.Video;

namespace GameLib.Forms
{

/// <summary>The direction an arrow points. <seealso cref="IControlRenderer.DrawArrow"/></summary>
public enum ArrowDirection { Up, Down, Left, Right }

#region IControlRenderer
public interface IControlRenderer
{
  event ModeChangedHandler VideoModeChanged;
  IGuiRenderTarget CreateDrawTarget(Control control);
  void DrawArrow(IGuiRenderTarget target, Rectangle rect, ArrowDirection direction, int arrowSize, Color color);
  void DrawArrowButton(IGuiRenderTarget target, Rectangle rect, ArrowDirection direction, int arrowSize,
                       bool depressed, Color bgColor, Color arrowColor);
  void DrawBackgroundColor(Control control, PaintEventArgs e, Color backColor);
  void DrawBackgroundImage(Control control, PaintEventArgs e, IGuiImage backImage, ContentAlignment imageAlignment);
  void DrawBorder(IGuiRenderTarget target, Rectangle rect, BorderStyle style, Color color);
  void DrawBorder(Control control, PaintEventArgs e, BorderStyle style, Color color);
  void DrawBox(IGuiRenderTarget target, Rectangle rect, Color color);
  void DrawCheckBox(Control control, PaintEventArgs e, Point drawPoint, bool isChecked, bool depressed, bool enabled);
  void DrawLine(IGuiRenderTarget target, Point p1, Point p2, Color color, bool antialiased);
  int GetBorderWidth(BorderStyle style);
  Size GetCheckBoxSize(Control control);
  int GetScrollBarEndSize(Control control);
  bool IsTranslucent(IGuiRenderTarget target);
}
#endregion

#region ControlRenderer
public abstract class ControlRenderer : IControlRenderer
{
  public event ModeChangedHandler VideoModeChanged
  {
    add { Video.Video.ModeChanged += value; }
    remove { Video.Video.ModeChanged -= value; }
  }

  public abstract IGuiRenderTarget CreateDrawTarget(Control control);

  public abstract void DrawArrow(IGuiRenderTarget target, Rectangle rect, ArrowDirection direction, int arrowSize,
                                 Color color);

  public abstract void DrawArrowButton(IGuiRenderTarget target, Rectangle rect, ArrowDirection direction,
                                       int arrowSize, bool depressed, Color bgColor, Color arrowColor);

  public void DrawBackgroundColor(Control control, PaintEventArgs e, Color backColor)
  {
    if(control == null || e == null) throw new ArgumentNullException();
    if(backColor.A != 0) e.Target.FillArea(e.DrawRect, backColor);
  }

  public void DrawBackgroundImage(Control control, PaintEventArgs e, IGuiImage backImage, 
                                  ContentAlignment imageAlignment)
  {
    if(control == null || e == null) throw new ArgumentNullException();
    if(backImage != null)
    {
      backImage.Draw(new Rectangle(Point.Empty, backImage.Size), e.Target,
                     new Rectangle(UIHelper.CalculateAlignment(control.GetDrawRect(), backImage.Size, imageAlignment),
                                   backImage.Size));
    }
  }

  public abstract void DrawBorder(IGuiRenderTarget target, Rectangle rect, BorderStyle style, Color color);

  public void DrawBorder(Control control, PaintEventArgs e, BorderStyle style, Color color)
  {
    if(control == null || e == null) throw new ArgumentNullException();
    if(style != BorderStyle.None && color.A != 0) DrawBorder(e.Target, control.GetDrawRect(), style, color);
  }

  public abstract void DrawBox(IGuiRenderTarget target, Rectangle rect, Color color);

  public abstract void DrawCheckBox(Control control, PaintEventArgs e, Point drawPoint, bool isChecked,
                                    bool depressed, bool enabled);

  public abstract void DrawLine(IGuiRenderTarget target, Point p1, Point p2, Color color, bool antialiased);

  /// <summary>Returns the thickness of a border, in pixels.</summary>
  public int GetBorderWidth(BorderStyle border)
  {
    switch(border & BorderStyle.TypeMask)
    {
      case BorderStyle.FixedFlat:
      case BorderStyle.Fixed3D: return 1;
      case BorderStyle.FixedThick:
      case BorderStyle.Resizeable: return 2;
      default: return 0;
    }
  }

  public Size GetCheckBoxSize(Control control)
  {
    return new Size(13, 13);
  }

  public int GetScrollBarEndSize(Control control)
  {
    return 16;
  }

  public abstract bool IsTranslucent(IGuiRenderTarget target);
}
#endregion

#region SurfaceControlRenderer
public sealed class SurfaceControlRenderer : ControlRenderer
{
  public override IGuiRenderTarget CreateDrawTarget(Control control)
  {
    if(control == null) throw new ArgumentNullException();

    Surface parentSurface = control.Parent == null ? null : control.Parent.GetDrawTarget() as Surface;
    return parentSurface == null ? new Surface(control.Width, control.Height, Video.Video.DisplayFormat)
                                 : parentSurface.CreateCompatible(control.Width, control.Height);
  }

  public override void DrawArrow(IGuiRenderTarget target, Rectangle rect, ArrowDirection direction, int arrowSize,
                                 Color color)
  {
    Surface surface = GetSurface(target);
    int x, y, s, si;
    switch(direction)
    {
      case ArrowDirection.Up: case ArrowDirection.Down:
        x = rect.X + (rect.Width - 1) / 2; y = rect.Y + (rect.Height - arrowSize) / 2;
        if(direction == ArrowDirection.Up) { s = 0; si = 1; }
        else { s = arrowSize - 1; si = -1; }
        for(int i = 0; i < arrowSize; s += si, i++) Primitives.Line(surface, x - s, y + i, x + s, y + i, color);
        break;
      case ArrowDirection.Left: case ArrowDirection.Right:
        x = rect.X + (rect.Width - arrowSize) / 2; y = rect.Y + (rect.Height - 1) / 2;
        if(direction == ArrowDirection.Left) { s = 0; si = 1; }
        else { s = arrowSize - 1; si = -1; }
        for(int i = 0; i < arrowSize; s += si, i++) Primitives.Line(surface, x + i, y - s, x + i, y + s, color);
        break;
    }
  }

  public override void DrawArrowButton(IGuiRenderTarget target, Rectangle rect, ArrowDirection direction,
                                       int arrowSize, bool depressed, Color bgColor, Color arrowColor)
  {
    Surface surface = GetSurface(target);
    target.FillArea(rect, bgColor);
    DrawBorder(surface, rect, BorderStyle.FixedThick, bgColor, depressed);
    if(depressed) rect.Offset(1, 1);
    DrawArrow(target, rect, direction, arrowSize, arrowColor);
  }

  /// <summary>Paints a border using the specified base color.</summary>
  public override void DrawBorder(IGuiRenderTarget target, Rectangle rect, BorderStyle border, Color color)
  {
    if(target == null) throw new ArgumentNullException();
    if(border != BorderStyle.None && color.A != 0)
    {
      DrawBorder(GetSurface(target), rect, border, color, (border & BorderStyle.Depressed) != 0);
    }
  }

  public override void DrawBox(IGuiRenderTarget target, Rectangle rect, Color color)
  {
    Primitives.Box(GetSurface(target), rect, color);
  }

  public override void DrawCheckBox(Control control, PaintEventArgs e, Point drawPoint,
                                    bool isChecked, bool depressed, bool enabled)
  {
    Surface surface = GetSurface(e.Target);
    Rectangle box = new Rectangle(drawPoint, GetCheckBoxSize(control));
    DrawBorder(surface, box, BorderStyle.Fixed3D, enabled ? SystemColors.ActiveBorder : SystemColors.InactiveBorder,
               true);
    box.Inflate(-2, -2); // border
    surface.Fill(box, !depressed && enabled ? SystemColors.Window : SystemColors.Control);
    if(isChecked) DrawCheck(surface, box.X+1, box.Y+1, SystemColors.ControlText);
  }

  public override void DrawLine(IGuiRenderTarget target, Point p1, Point p2, Color color, bool antialiased)
  {
    Surface surface = GetSurface(target);
    if(antialiased) Primitives.LineAA(surface, p1, p2, color);
    else Primitives.Line(surface, p1, p2, color);
  }

  public override bool IsTranslucent(IGuiRenderTarget target)
  {
    Surface surface = target as Surface;
    return surface != null && surface.UsingAlpha;
  }

  /// <summary>Paints a border using default colors.</summary>
  static void DrawBorder(Surface surface, Rectangle rect, BorderStyle border, bool depressed)
  {
    switch(border & BorderStyle.TypeMask)
    {
      case BorderStyle.FixedFlat:
        DrawBorder(surface, rect, border, SystemColors.ControlDarkDark, depressed);
        break;
      case BorderStyle.Fixed3D: case BorderStyle.FixedThick: case BorderStyle.Resizeable:
        DrawBorder(surface, rect, border, SystemColors.ControlLight, SystemColors.ControlDark, depressed);
        break;
    }
  }

  /// <summary>Paints a border using the specified base color.</summary>
  static void DrawBorder(Surface surface, Rectangle rect, BorderStyle border, Color color, bool depressed)
  {
    switch(border & BorderStyle.TypeMask)
    {
      case BorderStyle.FixedFlat:
        DrawBorder(surface, rect, border, color, color, depressed);
        break;
      case BorderStyle.Fixed3D: case BorderStyle.FixedThick: case BorderStyle.Resizeable:
        DrawBorder(surface, rect, border, UIHelper.GetLightColor(color), UIHelper.GetDarkColor(color), depressed);
        break;
    }
  }

  /// <summary>Paints a border using the specified colors.</summary>
  static void DrawBorder(Surface surface, Rectangle rect, BorderStyle border, Color c1, Color c2, bool depressed)
  {
    switch(border & BorderStyle.TypeMask)
    {
      case BorderStyle.FixedFlat:
        Primitives.Box(surface, rect, c1);
        break;
      case BorderStyle.Fixed3D:
        if(depressed) { Color t = c1; c1 = c2; c2 = t; }
        Primitives.Line(surface, rect.X, rect.Y, rect.Right - 1, rect.Y, c1);
        Primitives.Line(surface, rect.X, rect.Y, rect.X, rect.Bottom - 1, c1);
        Primitives.Line(surface, rect.X, rect.Bottom - 1, rect.Right - 1, rect.Bottom - 1, c2);
        Primitives.Line(surface, rect.Right - 1, rect.Y, rect.Right - 1, rect.Bottom - 1, c2);
        break;
      case BorderStyle.FixedThick: case BorderStyle.Resizeable:
        Color c3, c4;
        if(depressed) { c3 = c2; c4 = SystemColors.ControlLightLight; c2 = c1; c1 = SystemColors.ControlDarkDark; }
        else { c4 = c2; c2 = SystemColors.ControlDarkDark; c3 = SystemColors.ControlLightLight; }
        Primitives.Line(surface, rect.X, rect.Y, rect.Right - 1, rect.Y, c1);
        Primitives.Line(surface, rect.X, rect.Y, rect.X, rect.Bottom - 1, c1);
        Primitives.Line(surface, rect.X, rect.Bottom - 1, rect.Right - 1, rect.Bottom - 1, c2);
        Primitives.Line(surface, rect.Right - 1, rect.Y, rect.Right - 1, rect.Bottom - 1, c2);
        rect.Inflate(-1, -1);
        Primitives.Line(surface, rect.X, rect.Y, rect.Right - 1, rect.Y, c3);
        Primitives.Line(surface, rect.X, rect.Y, rect.X, rect.Bottom - 1, c3);
        Primitives.Line(surface, rect.X, rect.Bottom - 1, rect.Right - 1, rect.Bottom - 1, c4);
        Primitives.Line(surface, rect.Right - 1, rect.Y, rect.Right - 1, rect.Bottom - 1, c4);
        break;
    }
  }

  /// <summary>Draws a check mark at a given point.</summary>
  /// <param name="surface">The <see cref="Surface"/> into which the check mark will be drawn.</param>
  /// <param name="x">The X coordinate of the top-left corner of the check mark.</param>
  /// <param name="y">The Y coordinate of the top-left corner of the check mark.</param>
  /// <param name="color">The color to draw the check with.</param>
  static void DrawCheck(Surface surface, int x, int y, Color color)
  {
    DrawCheck(surface, new Point(x, y), color);
  }

  /// <summary>Draws a check mark at a given point.</summary>
  /// <param name="surface">The <see cref="Surface"/> into which the check mark will be drawn.</param>
  /// <param name="point">The <see cref="Point"/> representing top-left corner of the check mark.</param>
  /// <param name="color">The color to draw the check with.</param>
  static void DrawCheck(Surface surface, Point point, Color color)
  {
    for(int yo = 0; yo < 3; yo++)
    {
      Primitives.Line(surface, point.X, point.Y + yo + 2, point.X + 2, point.Y + yo + 4, color);
      Primitives.Line(surface, point.X + 3, point.Y + yo + 3, point.X + 6, point.Y + yo, color);
    }
  }

  static Surface GetSurface(IGuiRenderTarget target)
  {
    Surface surface = target as Surface;
    if(surface == null)
    {
      throw target == null ? new ArgumentNullException() : new ArgumentException("Target is not a Surface.");
    }
    return surface;
  }
}
#endregion

} // namespace GameLib.Forms