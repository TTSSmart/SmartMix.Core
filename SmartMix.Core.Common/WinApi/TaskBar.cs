using SmartMix.Core.Common.WinApi.Enums;
using SmartMix.Core.Common.WinApi.Interfaces;
using SmartMix.Core.Common.WinApi.Structures;
using System.Drawing;
using System.Runtime.InteropServices;

namespace SmartMix.Core.Common.WinApi
{
    public sealed class Taskbar : ITaskbar
    {
        private const string ClassName = "Shell_TrayWnd";

        public Taskbar()
        {
            IntPtr taskbarHandle = User32.FindWindow(ClassName, null);

            APPBARDATA data = new APPBARDATA();
            data.cbSize = (uint)Marshal.SizeOf(typeof(APPBARDATA));
            data.hWnd = taskbarHandle;
            IntPtr result = Shell32.SHAppBarMessage(ABM.GetTaskbarPos, ref data);
            if (result == IntPtr.Zero)
                throw new InvalidOperationException();

            Position = (TaskbarPosition)data.uEdge;
            Bounds = Rectangle.FromLTRB(data.rc.left, data.rc.top, data.rc.right, data.rc.bottom);

            data.cbSize = (uint)Marshal.SizeOf(typeof(APPBARDATA));
            result = Shell32.SHAppBarMessage(ABM.GetState, ref data);
            int state = result.ToInt32();
            AlwaysOnTop = (state & ABS.AlwaysOnTop) == ABS.AlwaysOnTop;
            AutoHide = (state & ABS.Autohide) == ABS.Autohide;
        }

        public Rectangle Bounds { get; private set; }

        public TaskbarPosition Position { get; private set; }

        public Point Location
        {
            get { return Bounds.Location; }
        }

        /// <summary>
        ///     Размер панели задач
        /// </summary>
        public Size Size
        {
            get { return Bounds.Size; }
        }

        //Всегда возвращает false под Windows 7 (проверялось на Windows 7 RC Ultimate)
        public bool AlwaysOnTop { get; private set; }

        /// <summary>
        ///     Скрывается ли панель задач автоматически
        /// </summary>
        public bool AutoHide { get; private set; }
    }

    public static class ABS
    {
        public const int Autohide = 0x0000001;
        public const int AlwaysOnTop = 0x0000002;
    }
}
