using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
namespace Common
{
   
    public static class Dir
    {
        private static string m_path = "taaMgr.run/";
        public static string GetCacheDir()
        {
            if (!Directory.Exists(m_path))
            {
                Directory.CreateDirectory(m_path);
            }
            return m_path;
        }
    }

    public static class AppConfig
    {
      public static Configuration m_conf;
        static AppConfig()
        {
            Open();
        }
      public static void Open()
        {
            m_conf = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        }
        public static void Close()
        {
            m_conf.Save();
        }
    }
}
