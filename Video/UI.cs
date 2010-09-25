/*
GameLib is a library for developing games and other multimedia applications.
http://www.adammil.net/
Copyright (C) 2002-2010 Adam Milazzo

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.
You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
*/

using System;
using System.Drawing;

namespace GameLib.Video
{

public static class UIHelper
{
  /// <summary>Blends two colors together.</summary>
  /// <param name="a">The first color.</param>
  /// <param name="b">The second color.</param>
  /// <returns>A <see cref="Color"/> that is the average of <paramref name="a"/> and <paramref name="b"/>.</returns>
  public static Color Blend(Color a, Color b) { return Color.FromArgb((a.R+b.R)/2, (a.G+b.G)/2, (a.B+b.B)/2); }

  /// <summary>Calculate the point at which an object should be drawn.</summary>
  /// <param name="container">The container in which the object will be drawn.</param>
  /// <param name="itemSize">The size of the object to draw.</param>
  /// <param name="align">The alignment of the object within the container.</param>
  /// <returns>The point at which the object should be drawn. Note that this point may be outside the container
  /// if the object is too large.
  /// </returns>
  public static Point CalculateAlignment(Rectangle container, Size itemSize, ContentAlignment align)
  {
    int x=0, y=0;

    if(IsAlignedMiddle(align)) y = (container.Height - itemSize.Height) / 2;
    else if(IsAlignedBottom(align)) y = container.Height - itemSize.Height;

    if(IsAlignedCenter(align)) x = (container.Width - itemSize.Width) / 2;
    else if(IsAlignedRight(align)) x = container.Width - itemSize.Width;

    return new Point(x + container.X, y + container.Y);
  }

  /// <summary>Given a base color, returns a dark color for use in 3D shading.</summary>
  /// <param name="baseColor">The base color used to calculate the dark color.</param>
  /// <returns>A new color that is equal to or darker than <paramref name="baseColor"/>.</returns>
  public static Color GetDarkColor(Color baseColor)
  {
    return Color.FromArgb(baseColor.R/2, baseColor.G/2, baseColor.B/2);
  }

  /// <summary>Given a base color, returns a light color for use in 3D shading.</summary>
  /// <param name="baseColor">The base color used to calculate the light color.</param>
  /// <returns>A new color that is equal to or lighter than <paramref name="baseColor"/>.</returns>
  public static Color GetLightColor(Color baseColor)
  {
    return Color.FromArgb(baseColor.R+(255-baseColor.R)*2/3, baseColor.G+(255-baseColor.G)*2/3,
                          baseColor.B+(255-baseColor.B)*2/3);
  }

  /// <summary>Returns true if <paramref name="align"/> specifies left alignment.</summary>
  /// <param name="align">The alignment value to check.</param>
  /// <returns>True if <paramref name="align"/> specifies left alignment, false otherwise.</returns>
  public static bool IsAlignedLeft(ContentAlignment align)
  {
    return align == ContentAlignment.TopLeft || align == ContentAlignment.MiddleLeft ||
           align == ContentAlignment.BottomLeft;
  }

  /// <summary>Returns true if <paramref name="align"/> specifies horizontally centered alignment.</summary>
  /// <param name="align">The alignment value to check.</param>
  /// <returns>True if <paramref name="align"/> specifies horizontally centered alignment, false otherwise.</returns>
  public static bool IsAlignedCenter(ContentAlignment align)
  {
    return align == ContentAlignment.MiddleCenter || align == ContentAlignment.TopCenter ||
           align == ContentAlignment.BottomCenter;
  }

  /// <summary>Returns true if <paramref name="align"/> specifies right alignment.</summary>
  /// <param name="align">The alignment value to check.</param>
  /// <returns>True if <paramref name="align"/> specifies right alignment, false otherwise.</returns>
  public static bool IsAlignedRight(ContentAlignment align)
  {
    return align==ContentAlignment.TopRight || align==ContentAlignment.MiddleRight ||
           align==ContentAlignment.BottomRight;
  }

  /// <summary>Returns true if <paramref name="align"/> specifies top alignment.</summary>
  /// <param name="align">The alignment value to check.</param>
  /// <returns>True if <paramref name="align"/> specifies top alignment, false otherwise.</returns>
  public static bool IsAlignedTop(ContentAlignment align)
  {
    return align==ContentAlignment.TopLeft || align==ContentAlignment.TopCenter ||
           align==ContentAlignment.TopRight;
  }

  /// <summary>Returns true if <paramref name="align"/> specifies vertically centered alignment.</summary>
  /// <param name="align">The alignment value to check.</param>
  /// <returns>True if <paramref name="align"/> specifies vertically centered alignment, false otherwise.</returns>
  public static bool IsAlignedMiddle(ContentAlignment align)
  {
    return align==ContentAlignment.MiddleCenter || align==ContentAlignment.MiddleLeft ||
           align==ContentAlignment.MiddleRight;
  }

  /// <summary>Returns true if <paramref name="align"/> specifies bottom alignment.</summary>
  /// <param name="align">The alignment value to check.</param>
  /// <returns>True if <paramref name="align"/> specifies bottom alignment, false otherwise.</returns>
  public static bool IsAlignedBottom(ContentAlignment align)
  {
    return align==ContentAlignment.BottomLeft || align==ContentAlignment.BottomCenter ||
           align==ContentAlignment.BottomRight;
  }
}

} // namespace GameLib.Video