using SmartMix.Core.Common.WinApi.Enums;
using System.Drawing;

namespace SmartMix.Core.Common.WinApi.Interfaces
{
    public interface ITaskbar
    {
        Rectangle Bounds { get; }

        TaskbarPosition Position { get; }

        Point Location { get; }

        /// <summary>
        /// Размер панели задач
        /// </summary>
        Size Size { get; }

        bool AlwaysOnTop { get; }

        /// <summary>
        /// Скрывается ли панель задач автоматически
        /// </summary>
        bool AutoHide { get; }
    }
}
