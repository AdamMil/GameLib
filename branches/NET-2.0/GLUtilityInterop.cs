/*
GameLib is a library for developing games and other multimedia applications.
http://www.adammil.net/
Copyright (C) 2002-2005 Adam Milazzo

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
using System.Runtime.InteropServices;

namespace GameLib.Interop.GLUtility
{

[System.Security.SuppressUnmanagedCodeSecurity()]
internal static class Utility
{ 
  [DllImport(Config.GLUtilityImportPath, ExactSpelling=true, EntryPoint="GLU_Init", CallingConvention=CallingConvention.Cdecl)]
  public static extern int  Init();
  [DllImport(Config.GLUtilityImportPath, ExactSpelling=true, EntryPoint="GLU_Quit", CallingConvention=CallingConvention.Cdecl)]
  public static extern void Quit();
  [DllImport(Config.GLUtilityImportPath, ExactSpelling=true, EntryPoint="GLU_GetError", CallingConvention=CallingConvention.Cdecl)]
  public static extern string GetError();

  [DllImport(Config.GLUtilityImportPath, ExactSpelling=true, EntryPoint="GLU_GetMilliseconds", CallingConvention=CallingConvention.Cdecl)]
  public static extern uint GetMilliseconds();
  [DllImport(Config.GLUtilityImportPath, ExactSpelling=true, EntryPoint="GLU_GetTimerFrequency", CallingConvention=CallingConvention.Cdecl)]
  public static extern long GetTimerFrequency();
  [DllImport(Config.GLUtilityImportPath, ExactSpelling=true, EntryPoint="GLU_GetTimerCounter", CallingConvention=CallingConvention.Cdecl)]
  public static extern long GetTimerCounter();
  [DllImport(Config.GLUtilityImportPath, ExactSpelling=true, EntryPoint="GLU_GetSeconds", CallingConvention=CallingConvention.Cdecl)]
  public static extern double GetSeconds();
  [DllImport(Config.GLUtilityImportPath, ExactSpelling=true, EntryPoint="GLU_ResetTimer", CallingConvention=CallingConvention.Cdecl)]
  public static extern void ResetTimer();

  [DllImport(Config.GLUtilityImportPath, ExactSpelling=true, EntryPoint="GLU_Getch", CallingConvention=CallingConvention.Cdecl)]
  public static extern char Getch();
  [DllImport(Config.GLUtilityImportPath, ExactSpelling=true, EntryPoint="GLU_Getche", CallingConvention=CallingConvention.Cdecl)]
  public static extern char Getche();
  [DllImport(Config.GLUtilityImportPath, ExactSpelling=true, EntryPoint="GLU_KbHit", CallingConvention=CallingConvention.Cdecl)]
  public static extern bool KbHit();

  [DllImport(Config.GLUtilityImportPath, ExactSpelling=true, EntryPoint="GLU_MemCopy", CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern void MemCopy(void *src, void *dest, int length);
  [DllImport(Config.GLUtilityImportPath, ExactSpelling=true, EntryPoint="GLU_MemFill", CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern void MemFill(void *dest, byte value, int length);
  [DllImport(Config.GLUtilityImportPath, ExactSpelling=true, EntryPoint="GLU_MemMove", CallingConvention=CallingConvention.Cdecl)]
  public unsafe static extern void MemMove(void *src, void *dest, int length);

  public static void Check(int result) { if(result<0) RaiseError(); }
  public static void RaiseError()
  { string error = GetError();
    throw error==null ? new GameLibException("Unknown error") : new GameLibException(error);
  }
}

} // namespace GameLib.Interop.GLUtility
