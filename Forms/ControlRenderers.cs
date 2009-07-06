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

  public void DrawArrowButton(IGuiRenderTarget target, Rectangle rect, ArrowDirection direction,
                              int arrowSize, bool depressed, Color bgColor, Color arrowColor)
  {
    target.FillArea(rect, bgColor);
    DrawBorder(target, rect, BorderStyle.FixedThick | (depressed ? BorderStyle.Depressed : 0), bgColor);
    if(depressed) rect.Offset(1, 1);
    DrawArrow(target, rect, direction, arrowSize, arrowColor);
  }

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

  /// <summary>Paints a border using the specified base color.</summary>
  static void DrawBorder(Surface surface, Rectangle rect, BorderStyle border, Color color, bool depressed)
  {
    Color c1, c2, c3, c4;
    switch(border & BorderStyle.TypeMask)
    {
      case BorderStyle.FixedFlat:
        Primitives.Box(surface, rect, color);
        break;
      case BorderStyle.Fixed3D:
        if(depressed)
        {
          c2 = UIHelper.GetLightColor(color);
          c1 = UIHelper.GetDarkColor(color);
        }
        else
        {
          c1 = UIHelper.GetLightColor(color);
          c2 = UIHelper.GetDarkColor(color);
        }
        Primitives.HLine(surface, rect.X, rect.Right - 2, rect.Y, c1);
        Primitives.VLine(surface, rect.X, rect.Y, rect.Bottom - 2, c1);
        Primitives.HLine(surface, rect.X, rect.Right - 1, rect.Bottom - 1, c2);
        Primitives.VLine(surface, rect.Right - 1, rect.Y, rect.Bottom - 1, c2);
        break;
      case BorderStyle.FixedThick: case BorderStyle.Resizeable:
        if(depressed)
        {
          c3 = UIHelper.GetLightColor(color);
          c4 = UIHelper.GetLightColor(c3);
          c2 = UIHelper.GetDarkColor(color);
          c1 = UIHelper.GetDarkColor(c2);
        }
        else
        {
          c2 = UIHelper.GetLightColor(color);
          c1 = UIHelper.GetLightColor(c2);
          c3 = UIHelper.GetDarkColor(color);
          c4 = UIHelper.GetDarkColor(c3);
        }

        Primitives.HLine(surface, rect.X, rect.Right - 2, rect.Y, c1);
        Primitives.VLine(surface, rect.X, rect.Y, rect.Bottom - 2, c1);
        Primitives.HLine(surface, rect.X, rect.Right - 1, rect.Bottom - 1, c4);
        Primitives.VLine(surface, rect.Right - 1, rect.Y, rect.Bottom - 1, c4);
        rect.Inflate(-1, -1);
        Primitives.HLine(surface, rect.X, rect.Right - 2, rect.Y, c2);
        Primitives.VLine(surface, rect.X, rect.Y, rect.Bottom - 2, c2);
        Primitives.HLine(surface, rect.X, rect.Right - 1, rect.Bottom - 1, c3);
        Primitives.VLine(surface, rect.Right - 1, rect.Y, rect.Bottom - 1, c3);
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
    for(int yo = 0; yo < 3; yo++)
    {
      Primitives.Line(surface, x, y + yo + 2, x + 2, y + yo + 4, color);
      Primitives.Line(surface, x + 3, y + yo + 3, x + 6, y + yo, color);
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