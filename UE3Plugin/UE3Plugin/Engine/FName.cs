using System.Runtime.InteropServices;
using System;

namespace UE3Plugin.Engine
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct FName
    {
        public Int32 Index;
        public Int32 Number;
    }
}
