using System;
using System.Runtime.InteropServices;

namespace GameLib.Interop
{

// HACK: C# doesn't support changing the calling convention of a delegate
[Serializable, AttributeUsage(AttributeTargets.Delegate)]
internal sealed class CallConvCdeclAttribute : Attribute
{
}

// HACK: get a function pointer for a delegate
[StructLayout(LayoutKind.Explicit, Size=4)]
internal sealed class DelegateMarshaller
{ public DelegateMarshaller(Delegate func) { this.func=func; }
  public unsafe IntPtr ToIntPtr()  { IntPtr ptr; Marshal.StructureToPtr(this, new IntPtr(&ptr), false); return ptr; }
  public unsafe void*  ToPointer() { void* ptr; Marshal.StructureToPtr(this, new IntPtr(&ptr), false); return ptr; }
  [MarshalAs(UnmanagedType.FunctionPtr),FieldOffset(0)] Delegate func;
}

}