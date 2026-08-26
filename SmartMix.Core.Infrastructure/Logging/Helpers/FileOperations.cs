namespace TTS.Logger
{
    using System;
    using System.IO;
    using System.IO.Compression;

    /// <summary>
    /// Вспомогательный класс операций над файлами.
    /// </summary>
    internal static class FileOperations
    {
        /// <summary>
        /// Удаление файлов старше месяца из папки
        /// </summary>
        /// <param name="dirPath">папка</param>
        /// <param name="ext"></param>
        public static void DeleteOldFilesInDir(string dirPath, string ext)
        {
            string[] files = Directory.GetFiles(dirPath);

            foreach (string file in files)
            {
                var fi = new FileInfo(file);
                if (fi.LastWriteTime < DateTime.Now.AddMonths(-1) && fi.Extension.Contains(ext))
                    fi.Delete();
            }
        }

        /// <summary>
        /// Сжатие файла <paramref name="filePath"/> в gz
        /// </summary>
        /// <param name="filePath">Абсолютный путь к файлу.</param>
        private static void CompressFile(string filePath)
        {
            try
            {
                FileInfo fi = new FileInfo(filePath);
                if (fi.Length > 0)
                {
                    using (FileStream fs = fi.OpenRead())
                    {
                        if ((File.GetAttributes(fi.FullName) & FileAttributes.Hidden) != FileAttributes.Hidden && fi.Extension != ".gz")
                        {
                            using (FileStream outFile = File.Create(fi.FullName + ".gz"))
                            {
                                using (var compress = new GZipStream(outFile, CompressionMode.Compress))
                                {
                                    fs.CopyTo(compress);
                                }
                            }
                        }
                        fs.Close();
                    }
                }

                // удаляем в любом случае
                fi.Delete();
            }
            catch (Exception)
            {
                // ignore
            }
        }

        /// <summary>
        /// Сжатие всех файлов в папке за исключением <paramref name="excludeFile"/>.
        /// </summary>
        /// <param name="files">Список файлов</param>
        /// <param name="excludeFile">Файл на исключение</param>
        public static void CompressAllFiles(string[] files, string excludeFile)
        {
            foreach (string file in files)
            {
                if (file == excludeFile) continue;

                CompressFile(file);
            }
        }
    }
}