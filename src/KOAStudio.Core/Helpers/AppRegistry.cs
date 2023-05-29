using System;

namespace KOAStudio.Core.Helpers
{
    public interface IAppRegistry
    {
        public T GetValue<T>(string SectionName, string KeyName, T DefValue);
        public bool SetValue<T>(string SectionName, string KeyName, T Value);
    }

    public class AppRegistry : IAppRegistry
    {
        private static readonly Microsoft.Win32.RegistryKey CurrentUser = Microsoft.Win32.RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.CurrentUser, Microsoft.Win32.RegistryView.Default);
        private readonly string? CorpAssemKey;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="corpName">회사명</param>
        public AppRegistry(string? corpName)
        {
            var exeName = System.Windows.Application.ResourceAssembly.GetName().Name;
            CorpAssemKey = $"Software\\{corpName}\\{exeName}";
        }

        public T GetValue<T>(string SectionName, string KeyName, T DefValue)
        {
            T retValue;

            string subKeyName = $"{CorpAssemKey}\\{SectionName}";
            using (var regkey = CurrentUser.OpenSubKey(subKeyName))
            {
                if (regkey is null) return DefValue;
                var value = regkey.GetValue(KeyName, DefValue);
                if (value is null) return DefValue;

                try
                {
                    retValue = (T)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                    retValue = DefValue;
                }
            }

            return retValue;
        }

        public bool SetValue<T>(string SectionName, string KeyName, T Value)
        {
            if (Value is null) return false;

            string subKeyName = $"{CorpAssemKey}\\{SectionName}";
            using (var regkey = CurrentUser.CreateSubKey(subKeyName))
            {
                regkey.SetValue(KeyName, Value, Microsoft.Win32.RegistryValueKind.Unknown);
            }
            return true;
        }
    }
}
