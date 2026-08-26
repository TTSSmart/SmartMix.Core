using SmartMix.Core.Common.WinApi.Enums;
using SmartMix.Core.Common.WinApi.Structures;
using System.Runtime.InteropServices;

namespace SmartMix.Core.Common.WinApi
{
    public static class Shell32
    {
        [DllImport("shell32.dll", SetLastError = true)]
        public static extern IntPtr SHAppBarMessage(ABM dwMessage, [In] ref APPBARDATA pData);
    }
}
