using System.Diagnostics;

namespace SmartMix.Core.Common.Helpers
{
    public static class WindowsExplorer
    {
        /// <summary>
        /// Открыть папку в Windows Explorer и выделить файл.
        /// </summary>
        /// <param name="fullFilePath">Файл, который нужно выделить</param>
        /// <exception cref="FileNotFoundException">Генерируется, если файл не найден</exception>
        public static void OpenFolderAndSelectFile(string fullFilePath)
        {
            if (!File.Exists(fullFilePath))
            {
                throw new FileNotFoundException(string.Format("Файл \"{0}\" не найден", fullFilePath));
            }

            string argument = string.Format("/select, \"{0}\"", fullFilePath);
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "Explorer.exe";
            info.Arguments = argument;
            Process.Start(info);
        }
    }
}
