using UE3Plugin.Engine;

using System.Runtime.InteropServices;
using System;

namespace UE3Plugin.Target
{
    internal static class RocketLeague
    {
        public struct GNames
        {
            public TArray<FNameEntry> Names;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct FNameEntry
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
            byte[] pad_0000;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string Name;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct UObject
        {
            public IntPtr VTableObject;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
            byte[] pad_0008;

            public Int32 ObjectInternalInteger;
            public Int32 NetIndex;
            public IntPtr Outer;
            public FName Name;
            public IntPtr Class;
            public IntPtr ObjectArchetype;
        }
    }
}
