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
    Hits = Poly.SplitIntoConvexPolygons();
    Size = diameter;
    Pos  = pos;
    Vel  = vel;
    AngleVel = (App.Rand.Next(360)-180) * vel.Length/128;
  }

  public bool Collide(Line segment)
  { segment.Start.X -= Pos.X; segment.Start.Y -= Pos.Y;
    foreach(Polygon p in Hits) if(p.ConvexContains(segment.Start)) return true;
    for(int i=0; i<Poly.Length; i++)
    { LineIntersectInfo info = segment.GetIntersection(Poly.GetEdge(i));
      if(info.OnFirst && info.OnSecond) return true;
    }
    return false;
  }

  public void Draw()
  { GL.glTranslatef(Pos.X, Pos.Y, 0);
    GL.glRotatef(Angle, 0, 0, 1);
    GL.glBegin(GL.GL_LINE_LOOP);
    for(int i=0; i<Poly.Length; i++) GL.glVertex2f(Poly[i].X, Poly[i].Y);
    GL.glEnd();
    GL.glLoadIdentity();
  }
  
  public Polygon Poly = new Polygon();
  public Point   Pos;
  public Vector  Vel;
  public float   Size, Angle, AngleVel;
  Polygon[] Hits;
}
#endregion

public class Bullet
{ public Bullet(Point pos, Vector vel) { Pos=pos; Vel=vel; Life=2; }

  public void Draw()
  { GL.glBegin(GL.GL_POINTS);
    GL.glVertex2f(Pos.X, Pos.Y);
    GL.glEnd();
  }

  public bool Update()
  { Life -= App.TimeDelta;
    if(Life<0) return false;
    Pos = App.Wrap(Pos+Vel*App.TimeDelta);
    return true;
  }

  public Point  Pos;
  public Vector Vel;
  public float  Life;
}

#region Ship
public class Ship
{ public void Draw()
  { GL.glTranslatef(Pos.X, Pos.Y, 0);
    GL.glRotatef(Angle, 0, 0, 1);
    GL.glBegin(GL.GL_LINE_LOOP);
    GL.glVertex2f(0, 10);
    GL.glVertex2f(8, -10);
    GL.glVertex2f(-8, -10);
    GL.glEnd();
    GL.glLoadIdentity();
  }

  public void Update()
  { const int turnSpeed=300, accel=300, bulletSpeed=250;
    if(Keyboard.Pressed(Key.Left)) Angle += turnSpeed*App.TimeDelta;
    if(Keyboard.Pressed(Key.Right)) Angle -= turnSpeed*App.TimeDelta;
    float angle = Angle*(float)Math.PI/180;
    if(Keyboard.Pressed(Key.Up))
    { Vel += new Vector(0, accel).Rotated(angle)*App.TimeDelta;
    }
    if(Delay>0) Delay -= App.TimeDelta;
    else if(Keyboard.Pressed(Key.Space) && Delay<=0)
    { App.AddBullet(new Bullet(Pos + new Vector(0, 10).Rotated(angle),
                               new Vector(0, bulletSpeed).Rotated(angle) + Vel));
      Delay = 0.2f;
    }
    Pos = App.Wrap(Pos + Vel*App.TimeDelta);
  }

  public Point  Pos;
  public Vector Vel;
  public float  Angle, AngleVel, Fade;
  float Delay;
}
#endregion

class App
{ public static Random Rand = new Random();
  public static float TimeDelta;

  const int STARTSIZE = 75;

  public static void AddBullet(Bullet bullet) { if(bullets.Count<10) bullets.Add(bullet); }

  public static Point Wrap(Point p)
  { if(p.X<0) p.X=640;
    else if(p.X>640) p.X=0;
    if(p.Y<0) p.Y=480;
    else if(p.Y>480) p.Y=0;
    return p;
  }

  static void Main()
  { Initialize();

    for(int i=0; i<8; i++)
      asteroids.Add(new Asteroid(75, new Point(Rand.Next(640), Rand.Next(480)),
                    new Vector(Rand.Next(320)-160, Rand.Next(240)-120)));
    ship.Pos = new Point(320, 240);
    
    float lastTime = (float)Timing.Seconds;
    while(true)
    { Event e;
      while((e=Events.NextEvent(0))!=null)
      { Input.ProcessEvent(e);
        if(Keyboard.Pressed(Key.Escape) || e is QuitEvent) goto done;
      }
      float time = (float)Timing.Seconds;
      TimeDelta = time-lastTime; lastTime=time;
      UpdateWorld();
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
    foreach(Asteroid a in asteroids) a.Draw();
    foreach(Bullet b in bullets) b.Draw();
    ship.Draw();
    Video.Flip();
  }

  static void Initialize()
  { Events.Initialize();
    Input.Initialize();
    Video.Initialize();
    Mouse.SystemCursorVisible = false;
    WM.WindowTitle = "Space Rocks";
    SetMode(640, 480);
  }

  static void SetMode(int width, int height)
  { Video.SetGLMode(width, height, 16);
    GL.glClearColor(0.0f, 0.0f, 0.0f, 1.0f);
    GL.glHint(GL.GL_LINE_SMOOTH_HINT, GL.GL_NICEST);
    GL.glHint(GL.GL_PERSPECTIVE_CORRECTION_HINT, GL.GL_FASTEST);
    GL.glHint(GL.GL_POINT_SMOOTH_HINT, GL.GL_NICEST);
    GL.glPointSize(2);
    GL.glShadeModel(GL.GL_FLAT);

    GL.glMatrixMode(GL.GL_PROJECTION);
    GLU.gluOrtho2D(0, 640, 0, 480);
    GL.glMatrixMode(GL.GL_MODELVIEW);
  }
  
  static void UpdateWorld()
  { ArrayList segs=bullets.Count>0 ? new ArrayList(bullets.Count) : null;

    for(int bi=0; bi<bullets.Count; bi++)
    { Bullet b = (Bullet)bullets[bi];
      Line seg = new Line(b.Pos, b.Vel*TimeDelta);
      if(b.Update()) segs.Add(seg);
      else bullets.RemoveAt(bi--);
    }

    for(int ai=0; ai<asteroids.Count; ai++)
    { Asteroid a = (Asteroid)asteroids[ai];
      if(segs!=null)
        for(int bi=0; bi<segs.Count; bi++)
          if(a.Collide((Line)segs[bi]))
          { asteroids.RemoveAt(ai--);
            if(a.Size>35)
              for(int i=0; i<2; i++)
                asteroids.Add(new Asteroid(a.Size*2/3, a.Pos,
                                           a.Vel + new Vector(Rand.Next(160)-80, Rand.Next(120)-60)));
            bullets.RemoveAt(bi);
            segs.RemoveAt(bi);
            goto nextAst;
          }

      a.Angle += a.AngleVel*TimeDelta;
      a.Pos = Wrap(a.Pos + a.Vel*TimeDelta);

      nextAst:;
    }

    ship.Update();
  }
  
  static ArrayList asteroids = new ArrayList(), bullets = new ArrayList();
  static Ship ship = new Ship();
}

} // namespace Asteroids