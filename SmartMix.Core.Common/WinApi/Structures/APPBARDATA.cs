using SmartMix.Core.Common.WinApi.Enums;
using System.Runtime.InteropServices;

namespace SmartMix.Core.Common.WinApi.Structures
{
    [StructLayout(LayoutKind.Sequential)]
    public struct APPBARDATA
    {
        public uint cbSize;
        public IntPtr hWnd;
        public uint uCallbackMessage;
        public ABE uEdge;
        public RECT rc;
        public int lParam;
    }
}
