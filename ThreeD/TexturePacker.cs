using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;

namespace GameLib.Video
{

#region RectanglePacker
/// <summary>Implements an algorithm to pack a number of rectangles into a larger rectangle. This can be used, for
/// instance, to pack small images into a single OpenGL texture. It is not guaranteed to find the optimal packing (that
/// problem is NP-hard), but it works quite well and very quickly. It is more efficient to add all of the rectangles
/// at once using <see cref="TryAdd(Size[])"/>, rather than adding them individually, since that gives the algorithm
/// more information to work with.
/// </summary>
public class RectanglePacker
{
  /// <summary>Initializes a new <see cref="RectanglePacker"/> that will attempt to pack rectangles into a larger
  /// rectangle of the given dimensions.
  /// </summary>
  public RectanglePacker(int width, int height)
  {
    if(width <= 0 || height <= 0) throw new ArgumentOutOfRangeException();
    root = new Node(null, 0, 0, width, height);
  }

  /// <summary>Gets the amount of space used within the larger rectangle. All of the smaller rectangles can be
  /// contained within a region of this size.
  /// </summary>
  public Size Size
  {
    get { return size; }
  }

  /// <summary>Gets the total size of the larger rectangle, which is equal to the dimensions passed to the constructor.</summary>
  public Size TotalSize
  {
    get { return new Size(root.Width, root.Height); }
  }

  /// <summary>Adds a rectangle of the given size, and returns point where the rectangle was placed, or null if the
  /// rectangle didn't fit.
  /// </summary>
  public Point? TryAdd(Size size)
  {
    return TryAdd(size.Width, size.Height);
  }

  /// <summary>Adds a rectangle of the given size, and returns point where the rectangle was placed, or null if the
  /// rectangle didn't fit.
  /// </summary>
  public Point? TryAdd(int width, int height)
  {
    if(width <= 0 || height <= 0) throw new ArgumentOutOfRangeException();

    Point? pt = root.TryAdd(width, height);
    if(pt.HasValue)
    {
      int right = pt.Value.X + width, bottom = pt.Value.Y + height;
      if(right  > size.Width)  size.Width  = right;
      if(bottom > size.Height) size.Height = bottom;
    }
    return pt;
  }

  /// <summary>Adds the given rectangles, and returns an array containing the points where they were added. If not all
  /// rectangles could be added, the corresponding points will be null.
  /// </summary>
  public Point?[] TryAdd(Size[] sizes)
  {
    Point?[] points;
    TryAdd(sizes, out points);
    return points;
  }

  /// <summary>Adds the given rectangles, and returns an array containing the points where they were added, and a
  /// boolean value that indicates whether all rectangles were added successfully. If not all rectangles could be
  /// added, the corresponding points will be null.
  /// </summary>
  public bool TryAdd(Size[] sizes, out Point?[] points)
  {
    ValidateSizes(sizes);
    sizes = (Size[])sizes.Clone(); // clone the array so we don't modify the original
    Array.Sort(sizes, SizeComparer.Instance);
    points = new Point?[sizes.Length];
    bool allAdded = true;
    for(int i=0; i<sizes.Length; i++)
    {
      Point? point = TryAdd(sizes[i]);
      if(!point.HasValue) allAdded = false;
      points[i] = point;
    }
    return allAdded;
  }

  internal static void ValidateSizes(Size[] sizes)
  {
    if(sizes == null) throw new ArgumentNullException();

    for(int i=0; i<sizes.Length; i++)
    {
      if(sizes[i].Width <= 0 || sizes[i].Height <= 0) throw new ArgumentOutOfRangeException();
    }
  }

  #region SizeComparer
  internal sealed class SizeComparer : IComparer<Size>
  {
    SizeComparer() { }

    public int Compare(Size a, Size b)
    {
      int cmp = b.Height - a.Height; // sort by height descending, then width descending
      return cmp == 0 ? b.Width - a.Width : cmp;
    }

    public static readonly SizeComparer Instance = new SizeComparer();
  }
  #endregion

  #region Node
  /// <summary>Represents a region within the larger rectangle, and its subdivision using a binary tree.</summary>
  /// <remarks>The children of a node are arranged spatially as in the following diagram. The node encompasses the
  /// entire area. The rectangle stored at the node occupies the region labeled "rect". The children consume
  /// the rest of the area, with the first child taking all the space to the right of the rectangle and the second
  /// child taking all the space below it.
  /// <code>
  /// +------+-----------+
  /// | rect | child 1   |
  /// +------+-----------+
  /// |                  |
  /// |      child 2     |
  /// |                  |
  /// +------------------+
  /// </code>
  /// </remarks>
  sealed class Node
  {
    /// <summary>Initializes a new <see cref="Node"/> with the given size.</summary>
    public Node(Node parent, int x, int y, int width, int height)
    {
      this.Parent = parent;
      this.X      = x;
      this.Y      = y;
      this.Width  = width;
      this.Height = height;
    }

    /// <summary>Attempts to add a rectangle of the given size to this node. The X and Y offsets keep track of the
    /// offset of this node from the origin.
    /// </summary>
    public Point? TryAdd(int width, int height)
    {
      if(width > this.Width || height > this.Height) return null;

      if(RectangleStored)
      {
        // if this node has a rectangle stored here already, delegate to the children
        if(Child1 != null) // try adding it to the right first
        {
          Point? pt = Child1.TryAdd(width, height);
          // as an optimization, we'll prevent degenerate subtrees (linked lists) from forming by replacing this
          // child with our grandchild if it's an only child, or removing this child if we have no grandchildren
          if(pt.HasValue && (Child1.Child1 == null || Child1.Child2 == null))
          {
            Child1 = Child1.Child1 == null ? Child1.Child2 : Child1.Child1;
            if(Child1 != null) Child1.Parent = this;
          }
          if(pt.HasValue || Child2 == null) return pt;
        }
        if(Child2 != null) // if we couldn't add it to the first child, try adding it to the second
        {
          Point? pt = Child2.TryAdd(width, height);
          if(pt.HasValue)
          {
            // prevent degenerate subtrees (linked lists) from forming (see comment above for details)
            if(Child2.Child1 == null || Child2.Child2 == null)
            {
              Child2 = Child2.Child1 == null ? Child2.Child2 : Child2.Child1;
              if(Child2 != null) Child2.Parent = this;
            }
          }
          return pt;
        }
        else return null;
      }
      else // this node does not have a rectangle stored here yet, so store it here and subdivide this space
      {
        // only add children that have a non-empty area
        if(this.Width  != width) Child1 = new Node(this, X+width, Y, this.Width - width, height);
        if(this.Height != height) Child2 = new Node(this, X, Y+height, this.Width, this.Height - height);
        RectangleStored = true;
        return new Point(X, Y);
      }
    }

    public Node Child1, Child2, Parent;
    public int X, Y, Width, Height;
    public bool RectangleStored;
  }
  #endregion

  readonly Node root;
  Size size;
}
#endregion

#region TexturePacker
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
    RectanglePacker.ValidateSizes(sizes);

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
      if(addBorder)
      {
        size.Width  += 2;
        size.Height += 2;
        sizes[i] = size;
      }
      if(size.Width > minWidth) minWidth = size.Width;
      if(size.Height > minHeight) minHeight = size.Height;
      minSize += size.Width * size.Height;
    }

    minSize += minSize/12; // add a bit of extra space, since it's not likely to fit exactly

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
        if(addBorder) point = new Point(point.Value.X+1, point.Value.Y+1);
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
        if(height < width) height += (height <= 32 ? height : height/4);
        else width += (width <= 32 ? width : width/4);
      }
    }
  }
}
#endregion

} // namespace GameLib.Video