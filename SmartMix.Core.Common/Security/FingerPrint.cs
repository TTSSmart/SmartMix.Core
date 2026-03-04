using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SmartMix.Core.Common.Security
{
    /// <summary>
    /// Представляет вспомогательный класс доступа к идентификатору системы.
    /// </summary>
    public static class FingerPrint
    {
        /// <summary>
        /// Представляет уникальный идентификатор системы.
        /// </summary>
        private static string _fingerPrint = string.Empty;

        /// <summary>
        /// Получает идентификатор системы и при необходимости выполняет его формирование.
        /// Возвращает результат выполнения операции.
        /// </summary>
        /// <returns>Уникальный идентификатор системы</returns>
        public static string Value()
        {
            if (!string.IsNullOrEmpty(_fingerPrint))
                return _fingerPrint;

            try
            {
                return
               _fingerPrint = GetHash("CPU >> " + CpuId() +
                                      "\nBIOS >> " + BiosId() +
                                      "\nBASE >> " + BaseId() +
                                      "\nDISK >> " + "diskId()" + // TODO лучше исправить и разделить проверку на "по-старому" и "по-новому" 
                                      "\nVIDEO >> " +
                                      "\nMAC >> ");
            }
            catch (System.Exception e)
            {
                throw new InvalidOperationException(BSU.Utils.Resources.ExceptionResource.FingerPrintGenerationError, e);
            }
        }

        private static string GetHash(string s)
        {
            MD5 sec = new MD5CryptoServiceProvider();
            byte[] bt = new ASCIIEncoding().GetBytes(s);

            return GetHexString(sec.ComputeHash(bt));
        }
        private static string GetHexString(byte[] bt)
        {
            string s = string.Empty;
            for (int i = 0; i < bt.Length; i++)
            {
                byte b = bt[i];
                int n, n1, n2;
                n = (int)b;
                n1 = n & 15;
                n2 = (n >> 4) & 15;
                if (n2 > 9)
                    s += ((char)(n2 - 10 + (int)'A')).ToString();
                else
                    s += n2.ToString();
                if (n1 > 9)
                    s += ((char)(n1 - 10 + (int)'A')).ToString();
                else
                    s += n1.ToString();
                if ((i + 1) != bt.Length && (i + 1) % 2 == 0) s += "-";
            }
            return s;
        }

        #region WMI ManagementObject

        /// <summary>
        /// Return a Hardware Identifier
        /// </summary>
        /// <param name="wmiClass">Название системного класса</param>
        /// <param name="wmiProperty">Название свойства</param>
        /// <param name="wmiMustBeTrueProperty">Наименование доп. свойства, значение которого должно быть TRUE</param>
        /// <returns>Значение свойства, если оно было найдено для указанного класса, иначе - пустая строка.</returns>
        private static string Identifier(string wmiClass, string wmiProperty, string wmiMustBeTrueProperty)
        {
            var mc = new ManagementClass(wmiClass);
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementBaseObject obj in moc)
            {
                if (obj is ManagementObject mo && mo != null)
                {
                    if (mo[wmiMustBeTrueProperty].ToString() == "True")
                    {
                        try
                        {
                            if (mo[wmiProperty] != null)
                                return mo[wmiProperty].ToString(); //Only get the first one
                        }
                        catch
                        {
                        }
                    }
                }
            }

            //
            return string.Empty;
        }

        /// <summary>
        /// Return a Hardware Identifier
        /// </summary>
        /// <param name="wmiClass">Название системного класса</param>
        /// <param name="wmiProperty">Название свойства</param>
        /// <returns>Значение свойства, если оно было найдено для указанного класса, иначе - пустая строка.</returns>
        private static string Identifier(string wmiClass, string wmiProperty)
        {
            var mc = new ManagementClass(wmiClass); // здесь мы феерически падаем, если в ОС не вся информация зарегистрирована
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementBaseObject obj in moc)
            {
                if (obj is ManagementObject mo && mo != null)
                {
                    try
                    {
                        if (mo[wmiProperty] != null)
                            return mo[wmiProperty].ToString();  //Only get the first one
                    }
                    catch
                    {
                    }
                }
            }
            //
            return string.Empty;
        }

        /// <summary>
        /// Uses first CPU identifier available in order of preference
        /// Don't get all identifiers, as it is very time consuming
        /// </summary>
        /// <returns>Return a CPU Identifier</returns>
        private static string CpuId()
        {
            string retVal = Identifier("Win32_Processor", "UniqueId");
            if (string.IsNullOrEmpty(retVal))
            {
                retVal = Identifier("Win32_Processor", "ProcessorId");
                if (string.IsNullOrEmpty(retVal))
                {
                    retVal = Identifier("Win32_Processor", "Name");
                    if (string.IsNullOrEmpty(retVal))
                        retVal = Identifier("Win32_Processor", "Manufacturer");

                    // Add clock speed for extra security
                    retVal += Identifier("Win32_Processor", "MaxClockSpeed");
                }
            }
            return retVal;
        }

        /// <summary>
        /// Return a BIOS Identifier
        /// </summary>
        /// <returns></returns>
        private static string BiosId()
        {
            return Identifier("Win32_BIOS", "Manufacturer")
                    + Identifier("Win32_BIOS", "SMBIOSBIOSVersion")
                    + Identifier("Win32_BIOS", "IdentificationCode")
                    + Identifier("Win32_BIOS", "SerialNumber")
                    + Identifier("Win32_BIOS", "ReleaseDate")
                    + Identifier("Win32_BIOS", "Version");
        }

        /// <summary>
        /// Return Main Physical Hard Drive ID
        /// </summary>
        /// <returns></returns>
        private static string DiskId()
        {
            return Identifier("Win32_DiskDrive", "Model")
                + Identifier("Win32_DiskDrive", "Manufacturer")
                + Identifier("Win32_DiskDrive", "Signature")
                + Identifier("Win32_DiskDrive", "TotalHeads");
        }

        /// <summary>
        /// Return Motherboard ID
        /// </summary>
        /// <returns></returns>
        private static string BaseId()
        {
            return Identifier("Win32_BaseBoard", "Model")
                + Identifier("Win32_BaseBoard", "Manufacturer")
                + Identifier("Win32_BaseBoard", "Name")
                + Identifier("Win32_BaseBoard", "SerialNumber");
        }

        /// <summary>
        /// Return Primary video controller ID
        /// </summary>
        /// <returns></returns>
        private static string VideoId()
        {
            return Identifier("Win32_VideoController", "DriverVersion")
                + Identifier("Win32_VideoController", "Name");
        }

        /// <summary>
        /// Return First enabled network card ID
        /// </summary>
        /// <returns></returns>
        private static string MacId()
        {
            return Identifier("Win32_NetworkAdapterConfiguration", "MACAddress", "IPEnabled");
        }

        #endregion WMI ManagementObject
    }
}
