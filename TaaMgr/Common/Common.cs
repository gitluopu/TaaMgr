using System;
using System.IO;
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
 
}
