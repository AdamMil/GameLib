/*
This is an example application for the GameLib multimedia/gaming library.
http://www.adammil.net/
Copyright (C) 2004-2005 Adam Milazzo

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
using System.Collections.Generic;
using GameLib;
using GameLib.Audio;
using GameLib.CD;
using GameLib.Events;
using GameLib.Input;
using GameLib.Video;
using GameLib.Interop.OpenGL;
using AdamMil.Mathematics.Geometry.TwoD;

namespace Asteroids
{

  #region Asteroid
  class Asteroid
  {
    const int SIDES=8;

    public Asteroid(double diameter, Point pos, Vector vel)
    {
      retryPoly:
      double angleLeft = (SIDES-2)*Math.PI;
      double lengthLeft = diameter * Math.PI;
      Point pt = new Point();
      Vector vect = new Vector(1, 0);
      Poly.AddPoint(pt);
      for(int side=0, tries=0; side<SIDES; tries=0, side++)
      {
        retryEdge:
        double goalAngle  = angleLeft/(SIDES-side);
        double goalLength = lengthLeft/(SIDES-side);
        double angle = goalAngle  + goalAngle*0.6f*App.Rand.NextDouble()-goalAngle*0.3f;
        double length = goalLength + goalAngle*0.6f*App.Rand.NextDouble()-goalAngle*0.3f;
        Vector tv = (-vect.Normal*length).Rotated(angle);
        Line segment = new Line(pt, tv);
        for(int i=0; i<side; i++)
        {
          LineIntersection info = segment.GetIntersectionInfo(Poly.GetEdge(i));
          if(info.OnFirst && info.OnSecond && info.Point!=segment.Start && info.Point!=segment.End)
            if(++tries==5) { Poly.Clear(); goto retryPoly; }
            else goto retryEdge;
        }
        if(side<SIDES-2)
        {
          angleLeft -= angle; lengthLeft -= length;
          Poly.AddPoint(pt += vect=tv);
        }
      }

      Vector cent = new Vector(Poly.GetCentroid());
      for(int i=0; i<Poly.Length; i++) Poly[i] -= cent;
      Hits = Poly.Split();
      Size = diameter;
      Pos  = pos;
      Vel  = vel;
      AngleVel = (App.Rand.Next(360)-180) * vel.Length/128;
    }

    public bool Collided(Line segment)
    {
      segment.Start.X -= Pos.X; segment.Start.Y -= Pos.Y;
      foreach(Polygon p in Hits) if(p.ConvexContains(segment.Start)) return true;
      for(int i=0; i<Poly.Length; i++)
      {
        LineIntersection info = segment.GetIntersectionInfo(Poly.GetEdge(i));
        if(info.OnFirst && info.OnSecond) return true;
      }
      return false;
    }

    public bool Collided(Ship ship)
    {
      for(int i=0; i<ship.Poly.Length; i++)
      {
        Line edge = ship.Poly.GetEdge(i);
        edge.Start.Offset(ship.Pos.X, ship.Pos.Y);
        if(Collided(edge)) return true;
      }
      return false;
    }

    public void Draw()
    {
      GL.glTranslated(Pos.X, Pos.Y, 0);
      GL.glRotated(Angle, 0, 0, 1);
      GL.glColor(System.Drawing.Color.Gray);
      foreach(Polygon poly in Hits)
      {
        GL.glBegin(GL.GL_POLYGON);
        for(int i=0; i<poly.Length; i++) GL.glVertex2d(poly[i].X, poly[i].Y);
        GL.glEnd();
      }
      GL.glBegin(GL.GL_LINE_LOOP);
      GL.glColor(System.Drawing.Color.White);
      for(int i=0; i<Poly.Length; i++) GL.glVertex2d(Poly[i].X, Poly[i].Y);
      GL.glEnd();
      GL.glLoadIdentity();
    }

    public Polygon Poly = new Polygon();
    public Point Pos;
    public Vector Vel;
    public double Size, Angle, AngleVel;
    Polygon[] Hits;
  }
  #endregion

  #region Bullet
  public class Bullet
  {
    public Bullet(Point pos, Vector vel) { Pos=pos; Vel=vel; Life=2; }

    public void Draw()
    {
      GL.glBegin(GL.GL_POINTS);
      GL.glColor(System.Drawing.Color.White);
      GL.glVertex2d(Pos.X, Pos.Y);
      GL.glEnd();
    }

    public bool Update()
    {
      Life -= App.TimeDelta;
      if(Life<0) return false;
      Pos = App.Wrap(Pos+Vel*App.TimeDelta);
      return true;
    }

    public Point Pos;
    public Vector Vel;
    public double Life;
  }
  #endregion

  #region Ship
  public class Ship
  {
    public void Draw()
    {
      GL.glTranslated(Pos.X, Pos.Y, 0);
      GL.glRotated(Angle, 0, 0, 1);

      byte alpha = (byte)(64 + Fade*191);
      GL.glBegin(GL.GL_POLYGON);
      GL.glColor(alpha, System.Drawing.Color.LightGreen);
      for(int i=0; i<Poly.Length; i++) GL.glVertex2d(Poly[i].X, Poly[i].Y);
      GL.glEnd();

      GL.glBegin(GL.GL_LINE_LOOP);
      GL.glColor(alpha, System.Drawing.Color.Green);
      for(int i=0; i<Poly.Length; i++) GL.glVertex2d(Poly[i].X, Poly[i].Y);
      GL.glEnd();

      GL.glLoadIdentity();
    }

    public void Reset()
    {
      Pos   = new Point(320, 240);
      Vel   = new Vector();
      Fade  = 0;
      Angle = 0;
    }

    public void Update()
    {
      const int turnSpeed=300, accel=300, bulletSpeed=250;
      if(Keyboard.Pressed(Key.Left)) Angle += turnSpeed*App.TimeDelta;
      if(Keyboard.Pressed(Key.Right)) Angle -= turnSpeed*App.TimeDelta;
      double angle = Angle*Math.PI/180;
      if(Keyboard.Pressed(Key.Up))
      {
        Vel += new Vector(0, accel).Rotated(angle)*App.TimeDelta;
      }
      if(Delay>0) Delay -= App.TimeDelta;
      else if(Keyboard.Pressed(Key.Space) && Delay<=0)
      {
        App.AddBullet(new Bullet(Pos + new Vector(0, 10).Rotated(angle),
                                 new Vector(0, bulletSpeed).Rotated(angle) + Vel));
        Delay = 0.2f;
      }
      Pos = App.Wrap(Pos + Vel*App.TimeDelta);
      if((Fade+=0.2f*App.TimeDelta)>1) Fade=1;
    }

    public Polygon Poly = new Polygon(new Point[] { new Point(0, 10), new Point(8, -10), new Point(-8, -10) });
    public Point Pos;
    public Vector Vel;
    public double Angle, AngleVel, Fade;
    double Delay;
  }
  #endregion

  class App
  {
    public static Random Rand = new Random();
    public static double TimeDelta;

#if DEBUG
    static string dataPath = "../../../";
#else
  static string dataPath = "./data/";
#endif
    const int STARTSIZE = 75;

    public static void AddBullet(Bullet bullet)
    {
      if(bullets.Count<10) bullets.Add(bullet);
      shot.Play();
      score -= 5;
    }

    public static Point Wrap(Point p)
    {
      if(p.X<0) p.X=640;
      else if(p.X>640) p.X=0;
      if(p.Y<0) p.Y=480;
      else if(p.Y>480) p.Y=0;
      return p;
    }

    static void Main()
    {
      Initialize();

      double lastTime = Timing.Seconds;
      try
      {
        while(true)
        {
          Event e;
          while((e=Events.NextEvent(0))!=null)
          {
            Input.ProcessEvent(e);
            if(Keyboard.Pressed(Key.Escape) || e.Type==EventType.Quit) goto done;
            if(e.Type==EventType.Exception) throw ((ExceptionEvent)e).Exception;
          }
          if(asteroids.Count==0)
          {
            int xv=(int)(160*difficulty), yv=(int)(120*difficulty);
            for(int i=0; i<8; i++)
              asteroids.Add(new Asteroid(75,
                              new Point(Rand.Next(640), Rand.Next(480)),
                              new Vector(Rand.Next(xv)-xv/2, Rand.Next(yv)-yv/2)));
            ship.Reset();
            bullets.Clear();
            difficulty *= 1.1f;
          }
          double time = Timing.Seconds;
          TimeDelta = time-lastTime; lastTime=time;
          UpdateWorld();
          Draw();
        }
        done: ;
      }
      finally { Deinitialize(); }
    }

    static void Deinitialize()
    {
      if(playedDrive != -1) CD.Drives[playedDrive].Stop();
      Audio.Deinitialize();
      Video.Deinitialize();
      foreach(Drive d in CD.Drives) d.Stop();
    }

    static void Draw()
    {
      GL.glClear(GL.GL_COLOR_BUFFER_BIT);
      foreach(Asteroid a in asteroids) a.Draw();
      foreach(Bullet b in bullets) b.Draw();
      if(lives>0) ship.Draw();
      Video.Flip();
    }

    static void Initialize()
    {
      CD.Initialize();
      Events.Initialize();
      Input.Initialize();
      Video.Initialize();
      Audio.Initialize();
      Audio.AllocateChannels(8);
      // if there's a music cd in the drive, use it
      foreach(Drive d in CD.Drives) if(d.Status==CDStatus.Playing) goto cdmusic;
      for(int i=0; i<CD.Drives.Count; i++)
        if(CD.Drives[i].CDInDrive)
          foreach(Track t in CD.Drives[i].Tracks)
            if(t.Type==TrackType.Audio)
            {
              playedDrive = i;
              CD.Drives[i].Play(t.Start, CD.Drives[i].Length-t.Start);
              goto cdmusic;
            }
      new SoundFileSource(dataPath+"music.ogg").Play(Audio.Infinite);
      cdmusic:
      shot = new SampleSource(new SoundFileSource(dataPath+"woot.wav"));
      exp  = new SampleSource(new SoundFileSource(dataPath+"explode.wav"));
      shot.Both = Audio.MaxVolume/2;
      Mouse.SystemCursorVisible = false;
      WM.WindowTitle = "Space Rocks";
      SetMode(640, 480);
    }

    static void SetMode(int width, int height)
    {
      Video.SetGLMode(width, height, 16);
      GL.glEnable(GL.GL_BLEND);
      GL.glBlendFunc(GL.GL_SRC_ALPHA, GL.GL_ONE_MINUS_SRC_ALPHA);
      GL.glClearColor(0.0f, 0.0f, 0.0f, 1.0f);
      GL.glHint(GL.GL_LINE_SMOOTH_HINT, GL.GL_NICEST);
      GL.glHint(GL.GL_PERSPECTIVE_CORRECTION_HINT, GL.GL_FASTEST);
      GL.glHint(GL.GL_POINT_SMOOTH_HINT, GL.GL_NICEST);
      GL.glHint(GL.GL_POLYGON_SMOOTH_HINT, GL.GL_FASTEST);
      GL.glPointSize(2);
      GL.glShadeModel(GL.GL_FLAT);

      GL.glMatrixMode(GL.GL_PROJECTION);
      GLU.gluOrtho2D(0, 640, 0, 480);
      GL.glMatrixMode(GL.GL_MODELVIEW);
    }

    static void UpdateWorld()
    {
      List<Line> segs = bullets.Count>0 ? new List<Line>(bullets.Count) : null;

      for(int bi=0; bi<bullets.Count; bi++)
      {
        Bullet b = (Bullet)bullets[bi];
        Line seg = new Line(b.Pos, b.Vel*TimeDelta);
        if(b.Update()) segs.Add(seg);
        else bullets.RemoveAt(bi--);
      }

      for(int ai=0; ai<asteroids.Count; ai++)
      {
        Asteroid a = (Asteroid)asteroids[ai];
        bool broke=false;
        if(segs!=null)
          for(int bi=0; bi<segs.Count; bi++)
            if(a.Collided((Line)segs[bi]))
            {
              asteroids.RemoveAt(ai--);
              broke=true;
              bullets.RemoveAt(bi);
              segs.RemoveAt(bi);
              break;
            }
        if(!broke)
        {
          if(ship.Fade==1 && a.Collided(ship))
          {
            lives--;
            ship.Reset();
            broke=true;
          }
        }
        else
        {
          score += (int)a.Size;
          exp.Play();
          if(a.Size>35)
          {
            for(int i=0; i<2; i++)
              asteroids.Add(new Asteroid(a.Size*2/3, a.Pos, a.Vel/4 + 
                            new Vector(Rand.Next(160)-80, Rand.Next(120)-60)));
            continue;
          }
        }

        a.Angle += a.AngleVel*TimeDelta;
        a.Pos = Wrap(a.Pos + a.Vel*TimeDelta);
      }

      if(lives>0) ship.Update();
    }

    static SampleSource shot, exp;
    static List<Asteroid> asteroids = new List<Asteroid>();
    static List<Bullet> bullets = new List<Bullet>();
    static Ship ship = new Ship();
    static double difficulty = 1.0f;
    static int lives = 3, score, playedDrive = -1;
  }

} // namespace Asteroids
