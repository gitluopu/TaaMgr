using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CmdLib;
using Confluent.Kafka;
using PPs;
using Common;
namespace TaaConf
{
    /// <summary>
    /// Interaction logic for TaaConfCtl.xaml
    /// </summary>
    public partial class TaaConfCtl : UserControl, INotifyPropertyChanged
    {
        public TaaConfCtl()
        {
            InitializeComponent();
            m_confs = new List<ConfItem>();
            m_confs.Add(new ConfItem("version", () => { return m_icmd.GetVersion(); }, null));
            m_confs.Add(new ConfItem("broker", () => { return m_icmd.GetBroker(); }, (str) => { m_icmd.SetBroker(str); }));
            m_confs.Add(new ConfItem("save2file", () => { return m_icmd.GetSave2File(); }, (str) => { m_icmd.SetSave2File(str); }));
            m_confs.Add(new ConfItem("netmapInv", () => { return m_icmd.GetNwPushIntval(); }, (str) => { m_icmd.SetNwPushIntval(str); }));
            m_confs.Add(new ConfItem("autoDrop", () => { return m_icmd.GetAutoDrop(); }, (str) => { m_icmd.SetAutoDrop(str); }));
            m_confs.Add(new ConfItem("freeSpace", () => { return m_icmd.GetFreeDiskSpace(); }, (str) => { m_icmd.SetFreeDiskSpace(str); }));
            DataContext = this;

        }

        public void PollTaaStatus()
        {
            m_tokenSource = new CancellationTokenSource();
            m_statusTask = Task.Run(() =>
            {
                var ct = m_tokenSource.Token;
                try
                {
                    while (true)
                    {
                        if (ct.IsCancellationRequested)
                        {

                            ct.ThrowIfCancellationRequested();
                        }
                        string status;
                        try
                        {
                            status = this.m_icmd.GetCmnCmd().RunCommand("ps ajx | grep taa|grep -v grep").m_result;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                            throw ex;
                        }
                        int childTaa = 0;
                        int mainTaa = 0;
                        int dumpCap = 0;
                        HashSet<string> infs = new HashSet<string>();
                        using (StringReader sr = new StringReader(status))
                        {
                            string line;
                            while ((line = sr.ReadLine()) != null)
                            {
                                if (line.Contains("/bin/TAA -C"))
                                {
                                    childTaa++;
                                }
                                else if (line.Contains("/bin/TAA"))
                                {
                                    mainTaa++;
                                }
                                else if (line.Contains("bin/dumpcap"))
                                {
                                    dumpCap++;
                                    string[] ary = line.Split(' ');
                                    int len = ary.Length;
                                    for (int i = 0; i < len; i++)
                                    {
                                        if (ary[i] == "-i")
                                        {
                                            if ((i + 1) < len)
                                            {
                                                infs.Add(ary[i + 1]);
                                            }
                                        }
                                    }
                                }
                            }

                        } //end stringReader
                        string _status = "";
                        string _infs = "";
                        if (mainTaa != 0 && childTaa != 0 && infs.Count != 0) //running
                        {
                            _status = "running";
                            foreach (var ele in infs)
                            {
                                if (_infs.Length > 0)
                                {
                                    _infs += ",";
                                }
                                _infs += ele;
                            }
                        }
                        else if (mainTaa != 0) //unhealthy
                        {
                            _status = "unknown";
                        }
                        else
                        {
                            _status = "stop";
                        }
                        if (_status != m_taaStatus)
                        {
                            m_taaStatus = _status;
                        }
                        if (_infs != m_taaInfs)
                        {
                            m_taaInfs = _infs;
                        }
                    }
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.Message);
                    return;
                }
            }, m_tokenSource.Token);

        }
        private void SaveClick(object sender, RoutedEventArgs e)
        {
            SetDataChanged();
            InitData();
        }
        private void RefreshClick(object sender, RoutedEventArgs e)
        {
            InitData();
        }
        private void CancelClick(object sender, RoutedEventArgs e)
        {
            foreach (var ele in m_confs)
            {
                ele.m_value = ele.m_valueBak;
            }
        }
        public void LogoutClick(object sender, RoutedEventArgs e)
        {
            OnLogout?.Invoke(sender, e);
        }
        public void Login(object sender, RoutedEventArgs e)
        {
            PollTaaStatus();
            InitData();
        }
        public void Logout(object sender, RoutedEventArgs e)
        {
            m_tokenSource.Cancel();
            m_tokenSource.Dispose();
            m_statusTask.Wait();
        }
        public class ConfItem : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged(string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            public delegate string get_t();
            public delegate void set_t(string val);
            public ConfItem(string name, get_t _Get, set_t _Set)
            {
                m_name = name;
                m_value = "unknown";
                Get = _Get;
                Set = _Set;
            }
            public void InitData()
            {
                m_value = Get();
                m_valueBak = m_value;
            }
            public void SetDataChanged()
            {
                if (m_value != m_valueBak)
                {
                    try
                    {
                        Set(m_value);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                    }
                }
            }
            private string _m_value;
            public string m_name { get; set; }
            public string m_value { get => _m_value; set { _m_value = value; OnPropertyChanged("m_value"); } }
            public string m_valueBak { get; set; }
            public get_t Get;
            public set_t Set;
        }
        public void InitData()
        {
            string remoteSvrluginPath = "/opt/in-sec/taa/svrplugin";
            string localSvrluginPath = Common.Dir.GetCacheDir() + "svrplugin";
            DirectoryInfo di = new DirectoryInfo(localSvrluginPath);
            if (di.Exists)
            {
                di.Delete(true);
            }
            di.Create();
            try
            {
                m_icmd.GetCmnCmd().Download(remoteSvrluginPath, di);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            foreach (var ele in m_confs)
            {
                ele.InitData();
            }
        }
        public void SetDataChanged()
        {
            foreach (var ele in m_confs)
            {
                ele.SetDataChanged();
            }
            string localSvrluginPath = Common.Dir.GetCacheDir() + "svrplugin";
            DirectoryInfo di = new DirectoryInfo(localSvrluginPath);
            if (di.Exists)
            {
                m_icmd.GetCmnCmd().Upload(di, "/opt/in-sec/taa/svrplugin.bak");
                m_icmd.GetCmnCmd().RunCommand("rm /tmp/svrplugin -r;mv /opt/in-sec/taa/svrplugin /tmp/svrplugin;mv /opt/in-sec/taa/svrplugin.bak /opt/in-sec/taa/svrplugin");
            }
        }
        CancellationTokenSource m_tokenSource;
        Task m_statusTask;
        public event RoutedEventHandler OnLogout;
        public List<ConfItem> m_confs { get; set; }
        public ITaaCmd m_icmd;
        private string _m_taaStatus;
        public string m_taaStatus
        {
            get { return _m_taaStatus; }
            set { _m_taaStatus = value; OnPropertyChanged("m_taaStatus"); }
        }
        private string _m_taaInfs;

        public string m_taaInfs
        {
            get { return _m_taaInfs; }
            set { _m_taaInfs = value; OnPropertyChanged("m_taaInfs"); }
        }
        private string _m_taaIp;

        public string m_taaIp
        {
            get { return _m_taaIp; }
            set { _m_taaIp = value; OnPropertyChanged("m_taaIp"); }
        }



        private void TaaStartClick(object sender, RoutedEventArgs e)
        {
            m_icmd.GetCmnCmd().RunCommand("systemctl start in-sec-taa");
        }
        private void TaaStopClick(object sender, RoutedEventArgs e)
        {
            m_icmd.GetCmnCmd().RunCommand("systemctl stop in-sec-taa");
        }
        private void TaaRestartClick(object sender, RoutedEventArgs e)
        {
            m_icmd.GetCmnCmd().RunCommand("systemctl restart in-sec-taa");
        }
        private bool Consume(string topic, CancellationTokenSource ct, EventHandler<string> msgHandler)
        {
            string ip;
            string port;
            string _broker = "";
            foreach (var ele in m_confs)
            {
                if (ele.m_name == "broker")
                {
                    _broker = ele.m_value;
                    break;
                }
            }
            if (_broker.Length < 0)
                return false;
            if (_broker.Contains(":"))
            {
                string[] ary = _broker.Split(':');
                ip = ary[0];
                port = ary[1];
            }
            else
            {
                ip = _broker;
                port = "9092";
            }
            if (ip == "127.0.0.1")
            {
                ip = m_taaIp;
            }
            try
            {
                Common.Net.TcpConnectionTest(ip, int.Parse(port), int.Parse(AppConfig.m_conf.AppSettings.Settings["operationTimeout"].Value));
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                if (ex.InnerException != null)
                {
                    msg += ex.Message;
                }
                MessageBox.Show(msg);
                return false;
            }
            Task.Run(() =>
            {
                var conf = new ConsumerConfig
                {
                    GroupId = DateTime.Now.ToString(),
                    BootstrapServers = ip + ":" + port,
                    AutoOffsetReset = AutoOffsetReset.Latest
                };
                using (var c = new ConsumerBuilder<Ignore, string>(conf).Build())
                {
                    c.Subscribe(topic);
                    try
                    {
                        while (true)
                        {
                            if (ct.IsCancellationRequested)
                                ct.Token.ThrowIfCancellationRequested();
                            try
                            {
                                var cr = c.Consume(ct.Token);
                                msgHandler(this, cr.Message.Value);
                                //Thread.Sleep(10);
                            }
                            catch (ConsumeException ex)
                            {
                                MessageBox.Show(ex.Error.Reason);
                                throw new OperationCanceledException();
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        c.Close();
                        ct.Dispose();
                    }
                }
            });
            return true;
        }
        private void TaaTrafficClick(object sender, RoutedEventArgs e)
        {
            var ct = new CancellationTokenSource();
            var ppsCtl = new PPsCtl();
            var win = new Window
            {
                Content = ppsCtl,
            };
            win.Closed += (s, arg) => { ct.Cancel(); };
            bool okay = Consume("traffic", ct, (s, msg) =>
            {
                CmdLib.Confs.Traffic.CTraffic[] traffic;
                try
                {
                    traffic = CmdLib.Confs.Traffic.CTraffic.FromJson(msg);
                }
                catch (Exception ex)
                {
                    throw new OperationCanceledException(ex.Message);
                }
                long pps = 0, bps = 0;

                foreach (var ele in traffic)
                {
                    pps += ele.Pps;
                    bps += ele.Bps;
                }
                ppsCtl.Add(new Tuple<long, long>(pps, bps));
            });
            if (okay)
                win.Show();
        }

        private void TaaNetMapClick(object sender, RoutedEventArgs e)
        {
            var ct = new CancellationTokenSource();
            var ppsCtl = new PPsCtl();
            var win = new Window
            {
                Content = ppsCtl,
            };
            var netMap = new NetMapConsumer("5");
            win.Closed += (s, arg) => { ct.Cancel(); netMap.Stop(); };
            netMap.OnHandleMsgDone += (s, t) => { ppsCtl.Add(t); };
            bool okay = Consume("netmap", ct, (s, msg) =>
            {
                netMap.HandleMsg(msg);
            });
            if (okay)
                win.Show();
        }
        private void TaaDiagnosticClick(object sender, RoutedEventArgs e)
        {
            Window win = new Window();
            AutotDiagnostic diag = new AutotDiagnostic(m_icmd);
            win.Content = diag;
            win.Show();
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
