using Microsoft.Win32;

namespace KOAStudio.Core.Helpers
{
    public class OcxPathHelper
    {
        public static string GetOcxPathFromClassID(string classID)
        {
            var regPath = @"\CLSID\" + classID + @"\InProcServer32\";
            return GetDefaultRegistryValue(Registry.ClassesRoot, regPath);
        }

        public static string GetClassIDFromProgID(string progID)
        {
            var regPath = progID + @"\CLSID\";
            return GetDefaultRegistryValue(Registry.ClassesRoot, regPath);
        }

        private static string GetDefaultRegistryValue(RegistryKey rootKey, string regPath)
        {
            try
            {
                using var regKey = rootKey.OpenSubKey(regPath);
                if (regKey != null)
                {
                    string defaultValue = (string)regKey.GetValue("");
                    {
                        return defaultValue;
                    }
                }
            }
            catch
            {
                //log error
            }
            return string.Empty;
        }

        public static string GetOcxPathFromCLSID(string clsID)
        {
            var regPath = @"\CLSID\" + clsID + @"\InProcServer32\";
            return GetDefaultRegistryValue(Registry.ClassesRoot, regPath);
        }

        public static string GetOcxPathFromWOW6432NodeCLSID(string clsID)
        {
            var regPath = @"\WOW6432Node\CLSID\" + clsID + @"\InProcServer32\";
            return GetDefaultRegistryValue(Registry.ClassesRoot, regPath);
        }
    }
}
