using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
using StackExchange.Redis;
namespace TaaConf
{
    /// <summary>
    /// Interaction logic for TaaKey.xaml
    /// </summary>
    
    public class KeyItem: INotifyPropertyChanged
    {
        private string _m_name;

        public string m_name
        {
            get { return _m_name; }
            set { _m_name = value;
                OnPropertyChanged("m_name");
            }
        }
        private string _m_filenum;

        public string m_filenum
        {
            get { return _m_filenum; }
            set { _m_filenum = value;
                OnPropertyChanged("m_filenum");
            }
        }

        private string _m_packets;

        public string m_packets
        {
            get { return _m_packets; }
            set { _m_packets = value;
                OnPropertyChanged("m_packets");
            }
        }

        private string _m_ppps;

        public string m_ppps
        {
            get { return _m_ppps; }
            set { _m_ppps = value;
                OnPropertyChanged("m_ppps");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public partial class TaaKey : UserControl
    {
        public List<KeyItem> m_keys { get; set; }
        private CancellationTokenSource m_cts;
        private IDatabase m_db;
        private TaaConfCtl m_conf;
        public TaaKey(TaaConfCtl conf)
        {
            InitializeComponent();
            m_conf = conf;
            string redisInfo = null;
            foreach (var ele in m_conf.m_confs)
                if (ele.m_name == "redis")
                    redisInfo = ele.m_value;
            string[] units = redisInfo.Split(',');
            string[] eles = units[0].Split(':');
            string ip = eles[0];
            string port = eles[1];
            if (ip == "127.0.0.1")
                ip = m_conf.m_taaIp;
            string addr = ip + ":" + port;
            MessageBox.Show(addr);
            m_keys = new List<KeyItem>();
            m_redis = ConnectionMultiplexer.Connect(addr +",Password=insec@666");
            m_db = m_redis.GetDatabase(1);
            m_cts = new CancellationTokenSource();
            PollTaaKeyOnce();
            DataContext = this;
            this.Unloaded += (sender,e) => { m_cts.Cancel(); };
            Task.Run(()=> { PollTaaKeys(); });
        }
        private void PollTaaKeys()
        {
            while(true)
            {
                if (m_cts.IsCancellationRequested)
                    break;
                PollTaaKeyOnce();
                Thread.Sleep(1000);
            }
        }
        private void PollTaaKeyOnce()
        {
            for (int i = 0; i < 8; i++)
            {
                int j = i + 10000;
                string key = "TAA0" + j.ToString();
                try { 
                if (m_db.KeyExists(key))
                {
                    var t1 = m_db.HashScan(key);
                    var v1 = t1.ToArray();
                    Array.Sort(v1, (x, y) => string.Compare(x.Name, y.Name));
                    KeyItem ki = null;
                    foreach (var ele in m_keys)
                    {
                        if (ele.m_name == key)
                            ki = ele;
                    }
                    if (ki == null)
                    {
                        ki = new KeyItem();
                        ki.m_name = key;
                        m_keys.Add(ki);
                    }
                    foreach (var pair in v1)
                    {
                        if (pair.Name == "filenum")
                            ki.m_filenum = pair.Value;
                        if (pair.Name == "packets")
                            ki.m_packets = pair.Value;
                        if (pair.Name == "ppps")
                            ki.m_ppps = pair.Value;
                    }
                }
                }catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    break;
                }
            }
        }
        private ConnectionMultiplexer m_redis;
    }
}
