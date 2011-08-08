using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using RectanglePacker = AdamMil.Mathematics.Geometry.RectanglePacker;

namespace GameLib.Video
{

/// <summary>Implements methods that use a <see cref="Rectangle"/> for the specific purpose of packing images into an
/// OpenGL texture.
/// </summary>
public static class TexturePacker
{
  /// <summary>Packs images of the given sizes into a rectangle sized properly for an OpenGL texture. It returns the
  /// positions within the texture where the images should be stored, and a <see cref="RectanglePacker"/> representing
  /// the texture. This override will query OpenGL to see if it has the GL_ARB_texture_non_power_of_two extension, and
  /// so can only be called after OpenGL has been initialized.
  /// </summary>
  public static Point[] PackTexture(Size[] sizes, out RectanglePacker packer)
  {
    return PackTexture(sizes, !OpenGL.HasNonPowerOfTwoExtension, false, out packer);
  }

  /// <summary>Packs images of the given sizes into a rectangle sized properly for an OpenGL texture. It returns the
  /// positions within the texture where the images should be stored, and a <see cref="RectanglePacker"/> representing
  /// the texture. If <paramref name="addBorder"/> is true, a one pixel space will be reserved around each image,
  /// although the returned points will point to the start of the image, not the border. This override will query
  /// OpenGL to see if it has the GL_ARB_texture_non_power_of_two extension, and so can only be called after OpenGL has
  /// been initialized.
  /// </summary>
  public static Point[] PackTexture(Size[] sizes, bool addBorder, out RectanglePacker packer)
  {
    return PackTexture(sizes, !OpenGL.HasNonPowerOfTwoExtension, addBorder, out packer);
  }

  /// <summary>Packs images of the given sizes into a rectangle sized properly for an OpenGL texture. It returns the
  /// positions within the texture where the images should be stored, and a <see cref="RectanglePacker"/> representing
  /// the texture. If <paramref name="addBorder"/> is true, a one pixel space will be reserved around each image,
  /// although the returned points will point to the start of the image, not the border. If
  /// <paramref name="requirePowerOfTwo"/> is true, the texture dimensions will be a power of two. This override can be
  /// called even when OpenGL has not be initialized yet.
  /// </summary>
  public static Point[] PackTexture(Size[] sizes, bool addBorder, bool requirePowerOfTwo, out RectanglePacker packer)
  {
    ValidateSizes(sizes);

    // sort the sizes and keep track of the original indices
    int[] indices = new int[sizes.Length];
    for(int i=0; i<indices.Length; i++) indices[i] = i;
    sizes = (Size[])sizes.Clone();
    Array.Sort(sizes, indices, RectanglePacker.SizeComparer.Instance);

    // calculate the minimum dimensions and size needed
    int minWidth=0, minHeight=0, minSize=0;
    for(int i=0; i<sizes.Length; i++)
    {
      Size size = sizes[i];
      if(addBorder && size.Width != 0 && size.Height != 0)
      {
        size.Width  += 2;
        size.Height += 2;
        sizes[i] = size;
      }
      if(size.Width > minWidth) minWidth = size.Width;
      if(size.Height > minHeight) minHeight = size.Height;
      minSize += size.Width * size.Height;
    }

    minSize += minSize/12; // add a bit of extra space to reduce the chance of a resize, since it's not likely to fit exactly
    // TODO: tune this heuristic

    // calculate the starting size
    int width, height;
    if(requirePowerOfTwo)
    {
      width = height = 1;
      while(width < minWidth) width *= 2;
      while(height < minHeight) height *= 2;

      for(int size=width * height; size < minSize; size *= 2)
      {
        if(height < width) height *= 2;
        else width *= 2;
      }
    }
    else
    {
      int sqrt = (int)Math.Ceiling(Math.Sqrt(minSize));
      if(minWidth > sqrt)
      {
        width  = minWidth;
        height = (minSize+width-1) / width;
      }
      else if(minHeight > sqrt)
      {
        height = minHeight;
        width  = (minSize+height-1) / height;
      }
      else
      {
        width = height = sqrt;
      }
    }

    Point[] points = new Point[sizes.Length];
    while(true)
    {
      packer = new RectanglePacker(width, height);
      for(int i=0; i<sizes.Length; i++)
      {
        Point? point = packer.TryAdd(sizes[i]);
        if(!point.HasValue) goto enlargePacker;

        if(addBorder && sizes[i].Width != 0 && sizes[i].Height != 0)
        {
          point = new Point(point.Value.X+1, point.Value.Y+1);
        }
        points[indices[i]] = point.Value;
      }
      return points;

      enlargePacker:
      if(requirePowerOfTwo)
      {
        if(height < width) height *= 2;
        else width *= 2;
      }
      else
      {
        // if we don't require a power of two, enlarge the image by 25% (unless it's small, in which case it's doubled)
        if(height < width) height += (height <= 32 ? height : height/4);
        else width += (width <= 32 ? width : width/4);
      }
    }
  }

  static void ValidateSizes(Size[] sizes)
  {
    if(sizes == null) throw new ArgumentNullException();

    for(int i=0; i<sizes.Length; i++)
    {
      if(sizes[i].Width < 0 || sizes[i].Height < 0) throw new ArgumentOutOfRangeException();
    }
  }
}

} // namespace GameLib.Video