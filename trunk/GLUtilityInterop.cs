using System;
using System.Runtime.InteropServices;

namespace GameLib.Interop.GLUtility
{

[System.Security.SuppressUnmanagedCodeSecurity()]
internal class Utility
{ 
  [DllImport(Config.GLUtilityImportPath, EntryPoint="GLU_Init", CallingConvention=CallingConvention.Cdecl)]
  public static extern int  Init();
  [DllImport(Config.GLUtilityImportPath, EntryPoint="GLU_Quit", CallingConvention=CallingConvention.Cdecl)]
  public static extern void Quit();
  [DllImport(Config.GLUtilityImportPath, EntryPoint="GLU_GetError", CallingConvention=CallingConvention.Cdecl)]
  public static extern string GetError();

  [DllImport(Config.GLUtilityImportPath, EntryPoint="GLU_GetMilliseconds", CallingConvention=CallingConvention.Cdecl)]
  public static extern uint GetMilliseconds();
  [DllImport(Config.GLUtilityImportPath, EntryPoint="GLU_GetTimerFrequency", CallingConvention=CallingConvention.Cdecl)]
  public static extern long GetTimerFrequency();
  [DllImport(Config.GLUtilityImportPath, EntryPoint="GLU_GetTimerCounter", CallingConvention=CallingConvention.Cdecl)]
  public static extern long GetTimerCounter();
  [DllImport(Config.GLUtilityImportPath, EntryPoint="GLU_GetSeconds", CallingConvention=CallingConvention.Cdecl)]
  public static extern double GetSeconds();

  public static void Check(int result) { if(result<0) RaiseError(); }
  public static void RaiseError()
  { string error = GetError();
    throw error==null ? new GameLibException("Unknown error") : new GameLibException(error);
  }
}

} // namespace GameLib.Interop.GLUtility
