using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace CmdLib
{
    public interface ICmnCmd
    {
        void Download(string src, FileInfo dst);
        void Download(string src, DirectoryInfo dst);
        void Upload(FileInfo src, string dst);
        void Upload(DirectoryInfo src, string dst);
        string RunCommand(string txt);
    }
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

    }
    public class TaaCmdUnix : ITaaCmd
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
            return m_icmd.RunCommand("cat /opt/in-sec/taa/bin/README.md");
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
                    return obj.Config.Consumerconfig.Brokers;
                });
                    ret.Add(str);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }
            foreach (var path in policies)
            {
                try
                {
                    string str = GetProperty(path, (src) =>
                    {
                        var obj = Confs.PolicyPlugin.CPolicyPlugin.FromJson(src);
                        return obj.Config.Producerconfig.Brokers;
                    });
                    ret.Add(str);
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
                    retStr += ";";
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
                    string str = GetProperty(path, (src) =>
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
            string path = Common.Dir.GetCacheDir() + @"svrplugin/Child/LocalRedisClientPlugin.svrplugin";
            string min = "";
            try
            {
                min = GetProperty(path, (src) =>
            {
                string dst;
                var obj = Confs.LocalRedisClient.CLocalRedisClient.FromJson(src);
                dst = obj.Config.MaxDelayMin.ToString();
                return dst;
            });
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            return min;
        }
        public void SetAutoDrop(string min)
        {
            string path = Common.Dir.GetCacheDir() + @"svrplugin/Child/LocalRedisClientPlugin.svrplugin";
            try
            {
                SetProperty(path, (src) =>
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
        private ICmnCmd m_icmd;
    }
}
