/*
This is an example application for the GameLib multimedia/gaming library.
http://www.adammil.net/
Copyright (C) 2004 Adam Milazzo

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
using System.Collections;
using GameLib;
using GameLib.Events;
using GameLib.Input;
using GameLib.Video;
using GameLib.Interop.OpenGL;
using GameLib.Mathematics.TwoD;

namespace Asteroids
{

#region Asteroid
class Asteroid
{ const int SIDES=8;

  public Asteroid(float diameter, Point pos, Vector vel)
  { retryPoly:
    float angleLeft = (SIDES-2)*(float)Math.PI;
    float lengthLeft = diameter * (float)Math.PI;
    Point pt = new Point();
    Vector vect = new Vector(1, 0);
    Poly.AddPoint(pt);
    for(int side=0,tries=0; side<SIDES-1; tries=0,side++)
    { retryEdge:
      float goalAngle  = angleLeft/(SIDES-side);
      float goalLength = lengthLeft/(SIDES-side);
      float angle = goalAngle  + goalAngle*0.6f*(float)App.Rand.NextDouble()-goalAngle*0.3f;
      float length = goalLength + goalAngle*0.6f*(float)App.Rand.NextDouble()-goalAngle*0.3f;
      Vector tv = (-vect.Normal*length).Rotated(angle);
      Line segment = new Line(pt, tv);
      for(int i=0; i<side; i++)
      { LineIntersectInfo info = segment.GetIntersection(Poly.GetEdge(i));
        if(info.OnFirst && info.OnSecond && info.Point!=segment.Start && info.Point!=segment.End)
          if(++tries==5) { Poly.Clear(); goto retryPoly; }
          else goto retryEdge;
      }
      angleLeft -= angle; lengthLeft -= length;
      Poly.AddPoint(pt += vect=tv);
    }
    
    Vector cent = new Vector(Poly.GetCentroid());
    for(int i=0; i<Poly.Length; i++) Poly[i] -= cent;
    Size = diameter;
    Pos  = pos;
    Vel  = vel;
    AngleVel = (App.Rand.Next(360)-180) * vel.Length/128;
  }

  public Polygon Poly = new Polygon();
  public Point   Pos;
  public Vector  Vel;
  public float   Size, Angle, AngleVel;
}
#endregion

class App
{ public static Random Rand = new Random();

  public int STARTSIZE = 75;

  static void Main()
  { Initialize();

    for(int i=0; i<8; i++)
      asteroids.Add(new Asteroid(75, new Point(Rand.Next(640), Rand.Next(480)),
                    new Vector(Rand.Next(320)-160, Rand.Next(240)-120)));
    
    float lastTime = (float)Timing.Seconds;
    while(true)
    { Event e;
      while((e=Events.NextEvent(0))!=null)
      { Input.ProcessEvent(e);
        if(Keyboard.Pressed(Key.Escape) || e is QuitEvent) goto done;
      }
      float time = (float)Timing.Seconds;
      UpdateWorld(time-lastTime); lastTime=time;
      Draw();
    }
    done:
    Deinitialize();
  }

  static void Deinitialize()
  { Video.Deinitialize();
    Input.Deinitialize();
    Events.Deinitialize();
  }

  static void Draw()
  { GL.glClear(GL.GL_COLOR_BUFFER_BIT);
    GL.glColor(System.Drawing.Color.White);
    foreach(Asteroid a in asteroids)
    { GL.glLoadIdentity();
      GL.glTranslatef(a.Pos.X, a.Pos.Y, 0);
      GL.glRotatef(a.Angle, 0, 0, 1);
      GL.glBegin(GL.GL_LINE_LOOP);
      for(int i=0; i<a.Poly.Length; i++) GL.glVertex2f(a.Poly[i].X, a.Poly[i].Y);
      GL.glEnd();
    }
    Video.Flip();
  }

  static void Initialize()
  { Events.Initialize();
    Input.Initialize();
    Video.Initialize();
    SetMode(640, 480);
  }

  static void SetMode(int width, int height)
  { Video.SetGLMode(width, height, 16);
    GL.glClearColor(0.0f, 0.0f, 0.0f, 1.0f);
    GL.glHint(GL.GL_LINE_SMOOTH_HINT, GL.GL_NICEST);
    GL.glHint(GL.GL_PERSPECTIVE_CORRECTION_HINT, GL.GL_FASTEST);
    GL.glHint(GL.GL_POINT_SMOOTH_HINT, GL.GL_NICEST);
    GL.glShadeModel(GL.GL_FLAT);

    GL.glMatrixMode(GL.GL_PROJECTION);
    GLU.gluOrtho2D(0, 640, 0, 480);
    GL.glMatrixMode(GL.GL_MODELVIEW);
  }
  
  static void UpdateWorld(float timeDelta)
  { ArrayList rem=null, add=null;
    foreach(Asteroid a in asteroids)
    { /*if(p.ConvexContains(new Point(Mouse.X-a.Pos.X, 480-Mouse.Y-a.Pos.Y)))
        { if(rem==null) { rem = new ArrayList(); add = new ArrayList(); }
          rem.Add(a);
          if(a.Size>35)
            for(int i=0; i<2; i++)
              add.Add(new Asteroid(a.Size*2/3, a.Pos, a.Vel + new Vector(Rand.Next(160)-80, Rand.Next(120)-60)));
          goto nextAst;
        }*/

      a.Pos += a.Vel * timeDelta;
      a.Angle += a.AngleVel * timeDelta;
      if(a.Pos.X<0) a.Pos.X=640;
      else if(a.Pos.X>640) a.Pos.X=0;
      if(a.Pos.Y<0) a.Pos.Y=480;
      else if(a.Pos.Y>480) a.Pos.Y=0;

      nextAst:;
    }
    
    if(rem != null) foreach(Asteroid a in rem) asteroids.Remove(a);
    if(add != null) foreach(Asteroid a in add) asteroids.Add(a);
  }
  
  static ArrayList asteroids = new ArrayList();
}

} // namespace Asteroids