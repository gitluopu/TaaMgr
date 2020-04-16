using System;
using System.Collections.Generic;
using System.IO;
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
            try
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    string content = sr.ReadToEnd();
                    return reader(content);

                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        private void SetProperty(string path, JsonProperEditor editor)

        {
            try
            {

                StreamReader sr = new StreamReader(path);
                string content = sr.ReadToEnd();
                string newContent = editor(content);
                sr.Close();
                StreamWriter sw = new StreamWriter(path);
                sw.Write(newContent);
                sw.Close();

            }
            catch (Exception ex)
            {
                //what to do
            }
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
                string str = GetProperty(path, (src) =>
                {
                    var obj = Confs.PolicyPlugin.CPolicyPlugin.FromJson(src);
                    return obj.Config.Consumerconfig.Brokers;
                });
                ret.Add(str);
            }
            foreach (var path in policies)
            {
                string str = GetProperty(path, (src) =>
                {
                    var obj = Confs.PolicyPlugin.CPolicyPlugin.FromJson(src);
                    return obj.Config.Producerconfig.Brokers;
                });
                ret.Add(str);
            }
            foreach (var path in strategies)
            {
                string str = GetProperty(path, (src) =>
                {
                    var obj = Confs.StrategyPatternKafkaProducer.CStrategyPatternKafkaProducer.FromJson(src);
                    return obj.Config.Brokers;
                });
                ret.Add(str);
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
                SetProperty(path, (src) =>
                {
                    var obj = Confs.PolicyPlugin.CPolicyPlugin.FromJson(src);
                    obj.Config.Consumerconfig.Brokers = broker;
                    obj.Config.Producerconfig.Brokers = broker;
                    return Confs.PolicyPlugin.Serialize.ToJson(obj);
                });
            }
           
            foreach (var path in strategies)
            {
                string str = GetProperty(path, (src) =>
                {
                    var obj = Confs.StrategyPatternKafkaProducer.CStrategyPatternKafkaProducer.FromJson(src);
                    obj.Config.Brokers = broker;
                    return Confs.StrategyPatternKafkaProducer.Serialize.ToJson(obj);
                });
            }
            
        }
        public string GetSave2File()
        {
            string path = Common.Dir.GetCacheDir() + @"svrplugin/Child/StrategyPatternPlugin.svrplugin";

            string str = GetProperty(path, (src) =>
            {
                var obj = Confs.StrategyPatternPlugin.CStrategyPatternPlugin.FromJson(src);
                return obj.Config.IsNeedSaveFile.ToString();
            });
            return str;
        }
        public void SetSave2File(string s)
        {
            string path = Common.Dir.GetCacheDir() + @"svrplugin/Child/StrategyPatternPlugin.svrplugin";

            SetProperty(path, (src) =>
            {
                var obj = Confs.StrategyPatternPlugin.CStrategyPatternPlugin.FromJson(src);
                obj.Config.IsNeedSaveFile = long.Parse(s);
                return Confs.StrategyPatternPlugin.Serialize.ToJson(obj);
            });
        }
        private ICmnCmd m_icmd;
    }
}
