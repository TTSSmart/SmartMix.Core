namespace SmartMix.Core.Common.Helpers
{
    public static class HardDrive
    {
        /// <summary>
        /// Получить свободное пространство в мегабайтах по пути куда нужно сохранить файл
        /// </summary>
        /// <param name="path">путь куда нужно сохранить файл</param>
        /// <returns></returns>
        public static double GetFreeSpace(string path)
        {
            DriveInfo[] drives = DriveInfo.GetDrives(); //получить все логические диски
            DriveInfo systemDriveInfo = drives[0]; //для инициализации //тут будет храниться найденный каталог
            Char[] ch = new Char[3];
            path.CopyTo(0, ch, 0, 3);
            string systemCatalog = new string(ch);

            foreach (var drive in drives)
            {
                if (drive.Name.ToLower() == systemCatalog.ToLower())
                {
                    systemDriveInfo = drive;
                    break;
                }
            }

            return systemDriveInfo.TotalFreeSpace / 1048576d; //тоже самое что: 1024 / 1024;

        }

        /// <summary>
        /// Получить букву локального диска
        /// </summary>
        /// <param name="path">путь из которого получаем букву локального диска</param>
        /// <returns></returns>
        public static string GetLocalDiscName(string path)
        {
            var ch = new Char[3];
            path.CopyTo(0, ch, 0, 3);
            return new string(ch);
        }
    }
}
