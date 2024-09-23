using Microsoft.Win32;

namespace WebMonitorAlarm
{
    /// <summary>
    ///     注册表辅助类
    /// </summary>
    public class RegistryHelper
    {
       // private const string SoftName = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Namespace;//您的软件所用到的注册表节点，所有键值对将在该节下创建

        public static string SoftName = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Namespace;

        /// <summary>
        ///     取得注册表值
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetKeyValue(string name)
        {
            RegistryKey hkml = Registry.CurrentUser;
            RegistryKey software = hkml.OpenSubKey("SOFTWARE", true);
            if (software != null)
            {
                RegistryKey subKeys = software.OpenSubKey(SoftName, true);
                if (subKeys != null)
                {
                    string registData = (subKeys.GetValue(name) ?? "").ToString();
                    return registData;
                }
            }
            return string.Empty;
        }

        /// <summary>
        ///     添加注册表键值对
        /// </summary>
        /// <param name="key"></param>
        /// <param name="keyValue"></param>
        public static void AddKey(string key, string keyValue)
        {
            RegistryKey hklm = Registry.CurrentUser;
            RegistryKey software = hklm.OpenSubKey("SOFTWARE", true);
            if (software != null)
            {
                RegistryKey subKey = software.CreateSubKey(SoftName);
                if (subKey != null) subKey.SetValue(key, keyValue);
            }
        }

        /// <summary>
        ///     删除注册表键
        /// </summary>
        /// <param name="key"></param>
        public static void DeleteKey(string key)
        {
            RegistryKey hkml = Registry.CurrentUser;
            RegistryKey software = hkml.OpenSubKey("SOFTWARE", true);
            if (software != null)
            {
                RegistryKey subKey = software.OpenSubKey(SoftName, true);
                if (subKey != null)
                {
                    string[] aimnames = subKey.GetSubKeyNames();
                    foreach (string aimKey in aimnames)
                    {
                        if (aimKey == key)
                            subKey.DeleteSubKeyTree(key);
                    }
                }
            }
        }

        /// <summary>
        ///     检查注册表是否存在键
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool IsRegisted(string key)
        {
            try
            {
                RegistryKey hkml = Registry.CurrentUser;
                RegistryKey software = hkml.OpenSubKey("SOFTWARE", true);
                if (software != null)
                {
                    RegistryKey subKeys = software.OpenSubKey(SoftName, true);
                    if (subKeys != null)
                    {
                        string[] keyNames = subKeys.GetValueNames();
                        foreach (string keyName in keyNames)
                        {
                            if (keyName == key)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
            return false;
        }
    }

}
