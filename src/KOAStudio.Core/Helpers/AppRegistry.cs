namespace KOAStudio.Core.Helpers
{
    public interface IAppRegistry
    {
        public T GetValue<T>(string section, string key, T defValue);
        public bool SetValue<T>(string section, string key, T value);
    }

    public class AppRegistry : IAppRegistry
    {
        private static readonly Microsoft.Win32.RegistryKey _currentUser = Microsoft.Win32.RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.CurrentUser, Microsoft.Win32.RegistryView.Default);
        private readonly string? _corpAssemKey;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="corpName">회사명</param>
        public AppRegistry(string? corpName)
        {
            var exeName = System.Windows.Application.ResourceAssembly.GetName().Name;
            _corpAssemKey = $"Software\\{corpName}\\{exeName}";
        }

        public T GetValue<T>(string section, string key, T defValue)
        {
            T retValue;

            string subKeyName = $"{_corpAssemKey}\\{section}";
            using (var regkey = _currentUser.OpenSubKey(subKeyName))
            {
                if (regkey is null) return defValue;
                var value = regkey.GetValue(key, defValue);
                if (value is null) return defValue;

                try
                {
                    retValue = (T)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                    retValue = defValue;
                }
            }

            return retValue;
        }

        public bool SetValue<T>(string section, string key, T value)
        {
            if (value is null) return false;

            string subKeyName = $"{_corpAssemKey}\\{section}";
            using var regkey = _currentUser.CreateSubKey(subKeyName);
            regkey.SetValue(key, value, Microsoft.Win32.RegistryValueKind.Unknown);
            return true;
        }
    }
}
