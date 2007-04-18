using System;
using System.Drawing;

namespace GameLib.Video
{

/// <summary>Common border styles.</summary>
[Flags]
public enum BorderStyle
{
  /// <summary>No border.</summary>
  None=0,
  /// <summary>A solid-color border.</summary>
  FixedFlat=1,
  /// <summary>A border composed of two colors, used to give the appearance of light hitting a 3D object at an angle.
  /// </summary>
  Fixed3D=2,
  /// <summary>A thick border with a 3D appearance.</summary>
  FixedThick=3,
  /// <summary>A border that signifies that the control can be resized by dragging its edges.</summary>
  Resizeable=4,
  /// <summary>A mask that selects the type of the border (eg, flat, 3d, thick, etc).</summary>
  TypeMask=7,

  /// <summary>A flag that indicates that the border is depressed rather than raised.</summary>
  Depressed=8,
};

public static class UIHelpers
{ 
  /// <summary>The direction an arrow points.<seealso cref="DrawArrow"/></summary>
  public enum Arrow { Up, Down, Left, Right }

  public static readonly Size CheckSize = new Size(7, 6);

  /// <summary>Returns true if <paramref name="align"/> specifies left alignment.</summary>
  /// <param name="align">The alignment value to check.</param>
  /// <returns>True if <paramref name="align"/> specifies left alignment, false otherwise.</returns>
  public static bool AlignedLeft(ContentAlignment align)
  { return align==ContentAlignment.TopLeft || align==ContentAlignment.MiddleLeft ||
           align==ContentAlignment.BottomLeft;
  }
  /// <summary>Returns true if <paramref name="align"/> specifies horizontally centered alignment.</summary>
  /// <param name="align">The alignment value to check.</param>
  /// <returns>True if <paramref name="align"/> specifies horizontally centered alignment, false otherwise.</returns>
  public static bool AlignedCenter(ContentAlignment align)
  { return align==ContentAlignment.MiddleCenter || align==ContentAlignment.TopCenter ||
           align==ContentAlignment.BottomCenter;
  }
  /// <summary>Returns true if <paramref name="align"/> specifies right alignment.</summary>
  /// <param name="align">The alignment value to check.</param>
  /// <returns>True if <paramref name="align"/> specifies right alignment, false otherwise.</returns>
  public static bool AlignedRight(ContentAlignment align)
  { return align==ContentAlignment.TopRight || align==ContentAlignment.MiddleRight ||
           align==ContentAlignment.BottomRight;
  }
  /// <summary>Returns true if <paramref name="align"/> specifies top alignment.</summary>
  /// <param name="align">The alignment value to check.</param>
  /// <returns>True if <paramref name="align"/> specifies top alignment, false otherwise.</returns>
  public static bool AlignedTop(ContentAlignment align)
  { return align==ContentAlignment.TopLeft || align==ContentAlignment.TopCenter ||
           align==ContentAlignment.TopRight;
  }
  /// <summary>Returns true if <paramref name="align"/> specifies vertically centered alignment.</summary>
  /// <param name="align">The alignment value to check.</param>
  /// <returns>True if <paramref name="align"/> specifies vertically centered alignment, false otherwise.</returns>
  public static bool AlignedMiddle(ContentAlignment align)
  { return align==ContentAlignment.MiddleCenter || align==ContentAlignment.MiddleLeft ||
           align==ContentAlignment.MiddleRight;
  }
  /// <summary>Returns true if <paramref name="align"/> specifies bottom alignment.</summary>
  /// <param name="align">The alignment value to check.</param>
  /// <returns>True if <paramref name="align"/> specifies bottom alignment, false otherwise.</returns>
  public static bool AlignedBottom(ContentAlignment align)
  { return align==ContentAlignment.BottomLeft || align==ContentAlignment.BottomCenter ||
           align==ContentAlignment.BottomRight;
  }

  /// <summary>Blends two colors together.</summary>
  /// <param name="a">The first color.</param>
  /// <param name="b">The second color.</param>
  /// <returns>A <see cref="Color"/> that is the average of <paramref name="a"/> and <paramref name="b"/>.</returns>
  public static Color Blend(Color a, Color b) { return Color.FromArgb((a.R+b.R)/2, (a.G+b.G)/2, (a.B+b.B)/2); }

  /// <summary>Returns the thickness of a border, in pixels.</summary>
  /// <param name="border">The border style thats thickness will be returned.</param>
  /// <returns>The thickness of the specified border, in pixels.</returns>
  public static int BorderWidth(BorderStyle border)
  { switch(border&BorderStyle.TypeMask)
    { case BorderStyle.FixedFlat: case BorderStyle.Fixed3D: return 1;
      case BorderStyle.FixedThick: case BorderStyle.Resizeable: return 2;
      default: return 0;
    }
  }

  /// <summary>Calculate the point at which an object should be drawn.</summary>
  /// <param name="container">The container in which the object will be drawn.</param>
  /// <param name="item">The size of the object to draw.</param>
  /// <param name="align">The alignment of the object within the container.</param>
  /// <returns>The point at which the object should be drawn. Note that this point may be outside the container
  /// if the object is too large.
  /// </returns>
  public static Point CalculateAlignment(Rectangle container, Size item, ContentAlignment align)
  { Point ret = new Point();
    if(AlignedLeft(align)) ret.X = container.X;
    else if(AlignedCenter(align)) ret.X = container.X + (container.Width - item.Width)/2;
    else ret.X = container.Right - item.Width;

    if(AlignedTop(align)) ret.Y = container.Y;
    else if(AlignedMiddle(align)) ret.Y = container.Y + (container.Height - item.Height)/2;
    else ret.Y = container.Bottom - item.Height;
    return ret;
  }

  public static void DrawArrow(Surface surface, Rectangle rect, Arrow arrow, int size, Color color)
  { int x, y, s, si;
    switch(arrow)
    { case Arrow.Up: case Arrow.Down:
        x=rect.X+(rect.Width-1)/2; y=rect.Y+(rect.Height-size)/2;
        if(arrow==Arrow.Up) { s=0; si=1; }
        else { s=size-1; si=-1; }
        for(int i=0; i<size; s+=si,i++) Primitives.Line(surface, x-s, y+i, x+s, y+i, color);
        break;
      case Arrow.Left: case Arrow.Right:
        x=rect.X+(rect.Width-size)/2; y=rect.Y+(rect.Height-1)/2;
        if(arrow==Arrow.Left) { s=0; si=1; }
        else { s=size-1; si=-1; }
        for(int i=0; i<size; s+=si,i++) Primitives.Line(surface, x+i, y-s, x+i, y+s, color);
        break;
    }
  }

  public static void DrawArrowBox(Surface surface, Rectangle rect, Arrow arrow, int size, bool depressed,
                                  Color bgColor, Color arrowColor)
  { surface.Fill(rect, bgColor);
    DrawBorder(surface, rect, BorderStyle.FixedThick, bgColor, depressed);
    if(depressed) rect.Offset(1, 1);
    DrawArrow(surface, rect, arrow, size, arrowColor);
  }

  /// <summary>Draws a border using default colors.</summary>
  /// <param name="surface">The <see cref="Surface"/> into which the border will be drawn.</param>
  /// <param name="rect">The bounds of the border.</param>
  /// <param name="border">The border style to use.</param>
  /// <param name="depressed">True if the border should be shaded for a depressed object and false for a raised
  /// object.
  /// </param>
  /// <remarks>This method simply calls <see cref="DrawBorder(Surface,Rectangle,BorderStyle,Color,bool)"/> with
  /// default color values.
  /// <seealso cref="DrawBorder(Surface,Rectangle,BorderStyle,Color,Color,bool)"/>
  /// </remarks>
  public static void DrawBorder(Surface surface, Rectangle rect, BorderStyle border, bool depressed)
  { switch(border&BorderStyle.TypeMask)
    { case BorderStyle.FixedFlat: DrawBorder(surface, rect, border, SystemColors.ControlDarkDark, depressed); break;
      case BorderStyle.Fixed3D: case BorderStyle.FixedThick: case BorderStyle.Resizeable:
        DrawBorder(surface, rect, border, SystemColors.ControlLight, SystemColors.ControlDark, depressed);
        break;
    }
  }

  /// <summary>Draws a border using the specified base color.</summary>
  /// <param name="surface">The <see cref="Surface"/> into which the border will be drawn.</param>
  /// <param name="rect">The bounds of the border.</param>
  /// <param name="border">The border style to use.</param>
  /// <param name="color">The base color of the border.</param>
  /// <remarks>The border will be drawn in a depressed style if <paramref name="border"/> contains the
  /// <see cref="BorderStyle.Depressed"/> flag.
  /// </remarks>
  public static void DrawBorder(Surface surface, Rectangle rect, BorderStyle border, Color color)
  { DrawBorder(surface, rect, border, color, (border&BorderStyle.Depressed)!=0);
  }

  /// <summary>Draws a border using the specified base color.</summary>
  /// <param name="surface">The <see cref="Surface"/> into which the border will be drawn.</param>
  /// <param name="rect">The bounds of the border.</param>
  /// <param name="border">The border style to use.</param>
  /// <param name="color">The base color of the border.</param>
  /// <param name="depressed">True if the border should be shaded for a depressed object and false for a raised
  /// object.
  /// </param>
  /// <remarks>This method calls <see cref="DrawBorder(Surface,Rectangle,BorderStyle,Color,Color,bool)"/> with
  /// appropriate color values calculated from the base color.
  /// <seealso cref="DrawBorder(Surface,Rectangle,BorderStyle,Color,Color,bool)"/>
  /// </remarks>
  public static void DrawBorder(Surface surface, Rectangle rect, BorderStyle border, Color color, bool depressed)
  { switch(border&BorderStyle.TypeMask)
    { case BorderStyle.FixedFlat: DrawBorder(surface, rect, border, color, color, depressed); break;
      case BorderStyle.Fixed3D: case BorderStyle.FixedThick: case BorderStyle.Resizeable:
        DrawBorder(surface, rect, border, GetLightColor(color), GetDarkColor(color), depressed);
        break;
    }
  }

  /// <summary>Draws a border using the specified colors.</summary>
  /// <param name="surface">The <see cref="Surface"/> into which the border will be drawn.</param>
  /// <param name="rect">The bounds of the border.</param>
  /// <param name="border">The border style to use.</param>
  /// <param name="c1">The first color value. For 3D surfaces, this should be the lighter of the two colors.</param>
  /// <param name="c2">The second color value. For 3D surfaces, this should be the darker of the two colors.</param>
  /// <param name="depressed">True if the border should be shaded for a depressed object and false for a raised
  /// object.
  /// </param>
  /// <remarks>Borders with a thickness greater than one pixel are drawn inside the bounding rectangle.</remarks>
  public static void DrawBorder(Surface surface, Rectangle rect, BorderStyle border, Color c1, Color c2, bool depressed)
  { switch(border&BorderStyle.TypeMask)
    { case BorderStyle.FixedFlat: Primitives.Box(surface, rect, c1); break;
      case BorderStyle.Fixed3D:
        if(depressed) { Color t=c1; c1=c2; c2=t; }
        Primitives.Line(surface, rect.X, rect.Y, rect.Right-1, rect.Y, c1);
        Primitives.Line(surface, rect.X, rect.Y, rect.X, rect.Bottom-1, c1);
        Primitives.Line(surface, rect.X, rect.Bottom-1, rect.Right-1, rect.Bottom-1, c2);
        Primitives.Line(surface, rect.Right-1, rect.Y, rect.Right-1, rect.Bottom-1, c2);
        break;
      case BorderStyle.FixedThick: case BorderStyle.Resizeable:
        Color c3, c4;
        if(depressed) { c3=c2; c4=SystemColors.ControlLightLight; c2=c1; c1=SystemColors.ControlDarkDark; }
        else { c4=c2; c2=SystemColors.ControlDarkDark; c3=SystemColors.ControlLightLight; }
        Primitives.Line(surface, rect.X, rect.Y, rect.Right-1, rect.Y, c1);
        Primitives.Line(surface, rect.X, rect.Y, rect.X, rect.Bottom-1, c1);
        Primitives.Line(surface, rect.X, rect.Bottom-1, rect.Right-1, rect.Bottom-1, c2);
        Primitives.Line(surface, rect.Right-1, rect.Y, rect.Right-1, rect.Bottom-1, c2);
        rect.Inflate(-1, -1);
        Primitives.Line(surface, rect.X, rect.Y, rect.Right-1, rect.Y, c3);
        Primitives.Line(surface, rect.X, rect.Y, rect.X, rect.Bottom-1, c3);
        Primitives.Line(surface, rect.X, rect.Bottom-1, rect.Right-1, rect.Bottom-1, c4);
        Primitives.Line(surface, rect.Right-1, rect.Y, rect.Right-1, rect.Bottom-1, c4);
        break;
    }
  }

  /// <summary>Draws a check mark at a given point.</summary>
  /// <param name="surface">The <see cref="Surface"/> into which the check mark will be drawn.</param>
  /// <param name="x">The X coordinate of the top-left corner of the check mark.</param>
  /// <param name="y">The Y coordinate of the top-left corner of the check mark.</param>
  /// <param name="color">The color to draw the check with.</param>
  public static void DrawCheck(Surface surface, int x, int y, Color color)
  { DrawCheck(surface, new Point(x, y), color);
  }

  /// <summary>Draws a check mark at a given point.</summary>
  /// <param name="surface">The <see cref="Surface"/> into which the check mark will be drawn.</param>
  /// <param name="point">The <see cref="Point"/> representing top-left corner of the check mark.</param>
  /// <param name="color">The color to draw the check with.</param>
  public static void DrawCheck(Surface surface, Point point, Color color)
  { for(int yo=0; yo<3; yo++)
    { Primitives.Line(surface, point.X, point.Y+yo+2, point.X+2, point.Y+yo+4, color);
      Primitives.Line(surface, point.X+3, point.Y+yo+3, point.X+6, point.Y+yo, color);
    }
  }

  /// <summary>Given a base color, returns a dark color for use in 3D shading.</summary>
  /// <param name="baseColor">The base color used to calculate the dark color.</param>
  /// <returns>A new color that is equal to or darker than <paramref name="baseColor"/>.</returns>
  public static Color GetDarkColor(Color baseColor)
  { return Color.FromArgb(baseColor.R/2, baseColor.G/2, baseColor.B/2);
  }

  /// <summary>Given a base color, returns a light color for use in 3D shading.</summary>
  /// <param name="baseColor">The base color used to calculate the light color.</param>
  /// <returns>A new color that is equal to or lighter than <paramref name="baseColor"/>.</returns>
  public static Color GetLightColor(Color baseColor)
  { return Color.FromArgb(baseColor.R+(255-baseColor.R)*2/3, baseColor.G+(255-baseColor.G)*2/3,
                          baseColor.B+(255-baseColor.B)*2/3);
  }
}

} // namespace GameLib.Video