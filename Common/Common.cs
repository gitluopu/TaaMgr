using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Net.Sockets;

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
            if (m_conf.AppSettings.Settings["operationTimeout"] == null)
                m_conf.AppSettings.Settings.Add("operationTimeout","3000");
        }
        public static void Close()
        {
            m_conf.Save();
        }
    }
    public class BrokerUnit
    {
        public BrokerUnit()
        { }
        public BrokerUnit(string ip,int port,string broker)
        {
            m_ip = ip;
            m_port = port;
            m_broker = broker;
        }
        public string m_broker;
        public string m_ip;
        public int m_port;
    }

    public class RedisUnit
    {
        public RedisUnit()
        { }
        public RedisUnit(string ip, int port, string passwd)
        {
            m_ip = ip;
            m_port = port;
            m_passwd = passwd;
        }
        public string m_ip;
        public int m_port;
        public string m_passwd;
    }
    public static class Net
    {
        public static void TcpConnectionTest(string ip, int port, int ms)
        {
            using (var tcpClient = new TcpClient())
            {
                try
                {
                    if (!tcpClient.ConnectAsync(ip, port).Wait(ms))
                    {
                        throw new Exception("connect to " + ip + ":" + port + " timeout within 3000ms");
                    }
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                    if (ex.InnerException != null)
                    {
                        msg += ex.InnerException.Message;
                    }
                    throw new Exception(msg);
                }
            }
        }
        public static BrokerUnit GetBrokerUnit(string _broker)
        {
            string broker, ip, port;
            if(_broker.Contains(":"))
            {
                broker = _broker;
                string[] strs = _broker.Split(':');
                ip = strs[0];
                port = strs[1];
            }else
            {
                ip = _broker;
                port = "9092";
                broker = ip + ":" + port;
            }
            return new BrokerUnit(ip, int.Parse(port), broker);
        }
    }
}
