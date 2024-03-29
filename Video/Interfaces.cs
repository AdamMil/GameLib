﻿/*
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

using System.Drawing;
using Font = GameLib.Fonts.Font;

namespace GameLib.Video
{

public interface IGuiImage
{
  Size Size { get; }
  void Draw(Rectangle srcRect, IGuiRenderTarget target, Rectangle destRect);
}

public interface IGuiRenderTarget : IGuiImage
{
  Rectangle ClipRect { get; set; }
  void Clear();
  int DrawText(Font font, string text, Point pt);
  Point DrawText(Font font, string text, Rectangle rect, ContentAlignment align);
  void FillArea(Rectangle area, Color color);
}

} // namespace GameLib.Video