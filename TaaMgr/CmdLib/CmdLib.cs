﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using Common;
namespace CmdLib
{
    public enum DiagnoseStatus
    {
        Okay,
        Unknown,
        Error,
        Waiting
    }
    public class CmnCmdResult
    {
        public CmnCmdResult(string res, string err, int code)
        {
            m_result = res;
            m_error = err;
            m_exitCode = code;
        }
        public string m_result;
        public string m_error;
        public int m_exitCode;
    }
    public class DiagnoseResult : INotifyPropertyChanged
    {
        public DiagnoseResult()
        {
            m_status = DiagnoseStatus.Waiting;
            m_msg = "";
        }
        private DiagnoseStatus _m_status;
        public DiagnoseStatus m_status { get => _m_status; set { _m_status = value; OnPropertyChanged("m_status"); } }
        private string _m_msg;
        public string m_msg { get => _m_msg; set { _m_msg = value; OnPropertyChanged("m_msg"); } }
        public object m_param;
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public interface ICmnCmd
    {
        void Download(string src, FileInfo dst);
        void Download(string src, DirectoryInfo dst);
        void Upload(FileInfo src, string dst);
        void Upload(DirectoryInfo src, string dst);
        CmnCmdResult RunCommand(string txt);
    }

    public delegate DiagnoseResult DianoseFunc(DiagnoseResult diag);

    public interface ITaaCmd
    {
        ICmnCmd GetCmnCmd();
        void Start();
        void Stop();
        void Restart();
        string GetVersion();
        string GetBroker();
        string GetSave2File();
        void SetBroker(string broker);
        void SetSave2File(string broker);
        string GetNwPushIntval();
        void SetNwPushIntval(string interval);
        string GetAutoDrop();
        void SetAutoDrop(string min);
        string GetFreeDiskSpace();
        void SetFreeDiskSpace(string sz);
        string GetRedisSvr();
        void SetRedisSvr(string sz);

        DiagnoseResult IsAuthOkay(DiagnoseResult dres);
        DiagnoseResult IsKfkaOkay(DiagnoseResult dres);
    }
    public partial class TaaCmdUnix : ITaaCmd
    {
        public ICmnCmd GetCmnCmd()
        {
            return m_icmd;
        }
        public TaaCmdUnix(ICmnCmd icmd)
        {
            m_icmd = icmd;
        }
        public void Start()
        {
            m_icmd.RunCommand("systemctl start in-sec-taa");
        }
        public void Stop()
        {
            m_icmd.RunCommand("systemctl stop in-sec-taa");
        }
        public void Restart()
        {
            m_icmd.RunCommand("systemctl restart in-sec-taa");
        }
        public string GetVersion()
        {
            return m_icmd.RunCommand("cat /opt/in-sec/taa/bin/README.md").m_result;
        }
        delegate string JsonProperEditor(string src);
        delegate string JsonPropReader(string src);

        private string GetProperty(string path, JsonPropReader reader)
        {
            using (StreamReader sr = new StreamReader(path))
            {
                string content = sr.ReadToEnd();
                return reader(content);
            }

        }
        private void SetProperty(string path, JsonProperEditor editor)
        {
            StreamReader sr = new StreamReader(path);
            string content = sr.ReadToEnd();
            string newContent = editor(content);
            sr.Close();
            StreamWriter sw = new StreamWriter(path);
            sw.Write(newContent);
            sw.Close();
        }
        public string GetBroker()
        {
            string[] policies =
           {
                Common.Dir.GetCacheDir() + @"svrplugin/Child/PolicyPlugin.svrplugin",
                Common.Dir.GetCacheDir() + @"svrplugin/Main/PolicyPlugin.svrplugin"
            };
            string[] strategies =
            {
               Common.Dir.GetCacheDir() + @"svrplugin/Child/StrategyPatternKafkaProducer.svrplugin",
               Common.Dir.GetCacheDir() + @"svrplugin/Main/StrategyPatternKafkaProducer.svrplugin",
            };
            HashSet<string> ret = new HashSet<string>();
            foreach (var path in policies)
            {
                try
                {
                    string str = GetProperty(path, (src) =>
                {
                    var obj = Confs.PolicyPlugin.CPolicyPlugin.FromJson(src);
                    string broker =  obj.Config.Consumerconfig.Brokers;
                    ret.Add(broker);
                    broker = obj.Config.Producerconfig.Brokers;
                    ret.Add(broker);
                    return broker;
                });
                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            foreach (var path in strategies)
            {
                try
                {
                    string str = GetProperty(path, (src) =>
                    {
                        var obj = Confs.StrategyPatternKafkaProducer.CStrategyPatternKafkaProducer.FromJson(src);
                        return obj.Config.Brokers;
                    });
                    ret.Add(str);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }
            string retStr = "";
            foreach (var ele in ret)
            {
                if (retStr.Length > 0)
                {
                    retStr += ",";
                }
                retStr += ele;
            }
            return retStr;
        }

        public void SetBroker(string broker)
        {
            string[] policies =
           {
                Common.Dir.GetCacheDir() + @"svrplugin/Child/PolicyPlugin.svrplugin",
                Common.Dir.GetCacheDir() + @"svrplugin/Main/PolicyPlugin.svrplugin"
            };
            string[] strategies =
            {
               Common.Dir.GetCacheDir() + @"svrplugin/Child/StrategyPatternKafkaProducer.svrplugin",
               Common.Dir.GetCacheDir() + @"svrplugin/Main/StrategyPatternKafkaProducer.svrplugin",
            };
            foreach (var path in policies)
            {
                try
                {
                    SetProperty(path, (src) =>
                    {
                        var obj = Confs.PolicyPlugin.CPolicyPlugin.FromJson(src);
                        obj.Config.Consumerconfig.Brokers = broker;
                        obj.Config.Producerconfig.Brokers = broker;
                        return Confs.PolicyPlugin.Serialize.ToJson(obj);
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            foreach (var path in strategies)
            {
                try
                {
                    SetProperty(path, (src) =>
                    {
                        var obj = Confs.StrategyPatternKafkaProducer.CStrategyPatternKafkaProducer.FromJson(src);
                        obj.Config.Brokers = broker;
                        return Confs.StrategyPatternKafkaProducer.Serialize.ToJson(obj);
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

        }
        public string GetSave2File()
        {
            string path = Common.Dir.GetCacheDir() + @"svrplugin/Child/StrategyPatternPlugin.svrplugin";
            string str = "";
            try
            {
                str = GetProperty(path, (src) =>
                {
                    var obj = Confs.StrategyPatternPlugin.CStrategyPatternPlugin.FromJson(src);
                    return obj.Config.IsNeedSaveFile.ToString();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return str;
        }
        public void SetSave2File(string s)
        {
            string path = Common.Dir.GetCacheDir() + @"svrplugin/Child/StrategyPatternPlugin.svrplugin";
            try
            {
                SetProperty(path, (src) =>
                {
                    var obj = Confs.StrategyPatternPlugin.CStrategyPatternPlugin.FromJson(src);
                    obj.Config.IsNeedSaveFile = long.Parse(s);
                    return Confs.StrategyPatternPlugin.Serialize.ToJson(obj);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public string GetNwPushIntval()
        {
            string nw = Common.Dir.GetCacheDir() + @"svrplugin/Child/NetworkAnalysis.svrplugin";
            string fmeNw = Common.Dir.GetCacheDir() + @"svrplugin/Main/FmeNetworkAnalysis.svrplugin";
            string nwInterval = "";
            string fmeInterval = "";
            try
            {
                nwInterval = GetProperty(nw, (src) =>
                {
                    string dst;
                    var obj = Confs.NetworkAnalysis.CNetworkAnalysis.FromJson(src);
                    dst = obj.Config.Pushinterval.ToString();
                    return dst;
                });
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            try
            {
                fmeInterval = GetProperty(fmeNw, (src) =>
          {
              string dst;
              var obj = Confs.FmeNetworkAnalysis.CFmeNetworkAnalysis.FromJson(src);
              dst = obj.Config.Pushinterval.ToString();
              return dst;
          });
            }
            catch { }
            if (nwInterval != fmeInterval && fmeInterval != "")
            {
                MessageBox.Show("NetworkAnalysis and FmeNetworkAnalysis pushInterval are different,we select NetworkAnalysis");
            }

            return nwInterval;
        }
        public void SetNwPushIntval(string interval)
        {
            string nw = Common.Dir.GetCacheDir() + @"svrplugin/Child/NetworkAnalysis.svrplugin";
            string fmeNw = Common.Dir.GetCacheDir() + @"svrplugin/Main/FmeNetworkAnalysis.svrplugin";
            try
            {
                SetProperty(nw, (src) =>
                {
                    string dst;
                    var obj = Confs.NetworkAnalysis.CNetworkAnalysis.FromJson(src);
                    obj.Config.Pushinterval = long.Parse(interval);
                    dst = Confs.NetworkAnalysis.Serialize.ToJson(obj);
                    return dst;
                });
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            try
            {
                SetProperty(fmeNw, (src) =>
                {
                    string dst;
                    var obj = Confs.FmeNetworkAnalysis.CFmeNetworkAnalysis.FromJson(src);
                    obj.Config.Pushinterval = long.Parse(interval);
                    dst = Confs.FmeNetworkAnalysis.Serialize.ToJson(obj);
                    return dst;
                });
            }
            catch { }
        }

        public string GetAutoDrop()
        {
            string[] path ={ Common.Dir.GetCacheDir() + @"svrplugin/Child/LocalRedisClientPlugin.svrplugin",
            Common.Dir.GetCacheDir() + @"svrplugin/Main/LocalRedisClientPlugin.svrplugin"
            };
            HashSet<string> minSet = new HashSet<string>();
            foreach (var ele in path)
            {
                try
                {
                    string min = GetProperty(ele, (src) =>
                 {
                     string dst;
                     var obj = Confs.LocalRedisClient.CLocalRedisClient.FromJson(src);
                     dst = obj.Config.MaxDelayMin.ToString();
                     return dst;
                 });
                    minSet.Add(min);
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
            if (minSet.Count > 1)
                MessageBox.Show("autoDrop in main and child are different");
            string retStr = "";
            foreach (var ele in minSet)
            {
                if (retStr.Length > 0)
                    retStr += ",";
                retStr += ele;
            }
            return retStr;
        }
        public void SetAutoDrop(string min)
        {
            string[] path ={ Common.Dir.GetCacheDir() + @"svrplugin/Child/LocalRedisClientPlugin.svrplugin",
            Common.Dir.GetCacheDir() + @"svrplugin/Main/LocalRedisClientPlugin.svrplugin"
            };
            foreach (var ele in path)
            {
                try
                {
                    SetProperty(ele, (src) =>
                    {
                        string dst;
                        var obj = Confs.LocalRedisClient.CLocalRedisClient.FromJson(src);
                        obj.Config.MaxDelayMin = long.Parse(min);
                        dst = Confs.LocalRedisClient.Serialize.ToJson(obj);
                        return dst;
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        public string GetFreeDiskSpace()
        {
            string path = Common.Dir.GetCacheDir() + @"svrplugin/Main/FileManagerPlugin.svrplugin";
            string min = "";
            try
            {
                min = GetProperty(path, (src) =>
                {
                    string dst;
                    var obj = Confs.FileManager.CFileManager.FromJson(src);
                    dst = obj.Config.DiskFreeSpace.ToString();
                    return dst;
                });
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            return min;
        }
        public void SetFreeDiskSpace(string sz)
        {
            string path = Common.Dir.GetCacheDir() + @"svrplugin/Main/FileManagerPlugin.svrplugin";
            try
            {
                SetProperty(path, (src) =>
                {
                    string dst;
                    var obj = Confs.FileManager.CFileManager.FromJson(src);
                    obj.Config.DiskFreeSpace = long.Parse(sz);
                    dst = Confs.FileManager.Serialize.ToJson(obj);
                    return dst;
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public string GetRedisSvr()
        {
            string[] path ={ Common.Dir.GetCacheDir() + @"svrplugin/Child/LocalRedisClientPlugin.svrplugin",
            Common.Dir.GetCacheDir() + @"svrplugin/Main/LocalRedisClientPlugin.svrplugin"
            };
            HashSet<String> redisSet = new HashSet<String>();
            foreach (var ele in path)
            {
                try
                {
                    string min = GetProperty(ele, (src) =>
                    {
                        RedisUnit ru = new RedisUnit();
                        var obj = Confs.LocalRedisClient.CLocalRedisClient.FromJson(src);
                        ru.m_ip = obj.Config.Host;
                        ru.m_port = (int)obj.Config.Port;
                        ru.m_passwd = obj.Config.Password;
                        redisSet.Add(ru.ToString());
                        return ru.m_ip;
                    });
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
            if (redisSet.Count > 1)
                MessageBox.Show("autoDrop in main and child are different");
            string retStr = "";
            foreach (var ele in redisSet)
            {
                if (retStr.Length > 0)
                    retStr += ",";
                retStr += ele;
            }
            return retStr;
        }
        public void SetRedisSvr(string rs)
        {
            RedisUnit ru = new RedisUnit(rs);
            string[] path ={ Common.Dir.GetCacheDir() + @"svrplugin/Child/LocalRedisClientPlugin.svrplugin",
            Common.Dir.GetCacheDir() + @"svrplugin/Main/LocalRedisClientPlugin.svrplugin"
            };
            foreach (var ele in path)
            {
                try
                {
                    SetProperty(ele, (src) =>
                    {
                        string dst;
                        var obj = Confs.LocalRedisClient.CLocalRedisClient.FromJson(src);
                        obj.Config.Host = ru.m_ip;
                        obj.Config.Password = ru.m_passwd;
                        obj.Config.Port = ru.m_port;
                        dst = Confs.LocalRedisClient.Serialize.ToJson(obj);
                        return dst;
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        private ICmnCmd m_icmd;
    }


    public partial class TaaCmdUnix
    {
        public DiagnoseResult IsAuthOkay(DiagnoseResult dres)
        {
            var res = m_icmd.RunCommand("ls /etc/license/IN-SEC.lic");
            if (res.m_exitCode != 0)
            {
                dres.m_status = DiagnoseStatus.Error;
                dres.m_msg += res.m_error;
            }
            //This system without authorization, Please contact the manufacturer!
            res = m_icmd.RunCommand("tail /var/log/in-sec-taa/TAAMaster.log -n 20");
            if (res.m_exitCode == 0)
            {
                if (res.m_result.Contains("without authorization"))
                {
                    dres.m_status = DiagnoseStatus.Error;
                    dres.m_msg += "This system without authorization, Please contact the manufacturer!";
                }
            }
            if (dres.m_status == DiagnoseStatus.Waiting)
                dres.m_status = DiagnoseStatus.Okay;
            return dres;
        }

        public DiagnoseResult IsKfkaOkay(DiagnoseResult dres)
        {
            BrokerUnit bu = dres.m_param as BrokerUnit;
            try
            {
                Net.TcpConnectionTest(bu.m_ip, bu.m_port, int.Parse(AppConfig.m_conf.AppSettings.Settings["operationTimeout"].Value));
            }
            catch (Exception ex)
            {
                dres.m_status = DiagnoseStatus.Error;
                dres.m_msg += ex.Message;
            }
            CmnCmdResult res = m_icmd.RunCommand("ss -anp | grep "+ bu.m_port.ToString() + " | grep TAA");
            if (res.m_exitCode != 0)
            {
                dres.m_status = DiagnoseStatus.Error;
                dres.m_msg += res.m_error;
            }
            else
            {
                int TAACount = Regex.Matches(res.m_result, "TAA").Count;
                int ESTABCount = Regex.Matches(res.m_result, "ESTAB").Count;
                if (TAACount!=ESTABCount)
                {
                    dres.m_status = DiagnoseStatus.Error;
                    dres.m_msg += res.m_result;
                }
            }
            if (dres.m_status == DiagnoseStatus.Waiting)
                dres.m_status = DiagnoseStatus.Okay;
            return dres;
        }
    }
}
