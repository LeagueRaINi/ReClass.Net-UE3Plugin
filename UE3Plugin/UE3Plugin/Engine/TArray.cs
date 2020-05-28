using ReClassNET.Memory;

using System.Runtime.InteropServices;
using System;

namespace UE3Plugin.Engine
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct TArray<T> where T : struct
    {
        public IntPtr Data;
        public Int32 Num;
        public Int32 Max;

        public T Read(RemoteProcess process, int idx, bool deref)
        {
            var ptrData = this[idx];
            if (ptrData == IntPtr.Zero)
                return default;

            if (deref)
                ptrData = process.ReadRemoteIntPtr(ptrData);

            if (ptrData == IntPtr.Zero)
                return default;

            return process.ReadRemoteObject<T>(ptrData);
        }

        public IntPtr this[int idx]
        {
            get {
                return Data + (idx * IntPtr.Size);
            }
        }
    }
}
