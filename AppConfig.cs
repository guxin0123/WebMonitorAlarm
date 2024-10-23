using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebMonitorAlarm
{
    internal class AppConfig
    {

        public string Urls { get; set; }
        public string TimeNum { get; set; }
        public string TimeUnit { get; set; }
        public string SendUrl { get; set; }
        public string Status { get; set; }
        public string AutoStart { get; set; }
        public AppConfig()
        {
            Urls = "http://192.168.0.1";
            TimeNum = "30";
            TimeUnit = "秒";
            SendUrl = "123456789";
            Status = "0";
            AutoStart = "0";

        }

        public static AppConfig GetVal()
        {
            AppConfig appConfig = new AppConfig();
            if (RegistryHelper.GetKeyValue("flag") == "true")
            {
                appConfig.Urls = RegistryHelper.GetKeyValue("Urls");
                appConfig.TimeNum = RegistryHelper.GetKeyValue("TimeNum");
                appConfig.TimeUnit = RegistryHelper.GetKeyValue("TimeUnit");
                appConfig.SendUrl = RegistryHelper.GetKeyValue("SendUrl");
                appConfig.Status = RegistryHelper.GetKeyValue("Status");
                appConfig.AutoStart = RegistryHelper.GetKeyValue("AutoStart");
            }
            return appConfig;
        }

        public static void SetVal(AppConfig appConfig)
        {
            RegistryHelper.AddKey("flag", "true");
            RegistryHelper.AddKey("Urls", appConfig.Urls);
            RegistryHelper.AddKey("TimeNum", appConfig.TimeNum);
            RegistryHelper.AddKey("TimeUnit", appConfig.TimeUnit);
            RegistryHelper.AddKey("SendUrl", appConfig.SendUrl);
            RegistryHelper.AddKey("Status", appConfig.Status);
            RegistryHelper.AddKey("AutoStart", appConfig.AutoStart);

        }
    }
}
