using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Confluent.Kafka;
namespace KafkaCtl
{
    /// <summary>
    /// Interaction logic for ProducerCtl.xaml
    /// </summary>
    public partial class ProducerCtl : UserControl, INotifyPropertyChanged
    {
        public ProducerCtl()
        {
            InitializeComponent();
            m_btnStop.IsEnabled = false;
            DataContext = this;
        }
        private async void ProduceClick(object sender, RoutedEventArgs arg)
        {
            m_btnProduce.IsEnabled = false;
            m_btnStop.IsEnabled = true;
            int cntMax = int.Parse(m_cnt);
            int interval = int.Parse(m_interval);
            string ip;
            string port;
            string broker;
            if (m_broker.Contains(":"))
            {
                string[] ary = m_broker.Split(':');
                ip = ary[0];
                port = ary[1];
                broker = m_broker;
            }
            else
            {
                ip = m_broker;
                port = "9092";
                broker = ip + ":" + port;
            }
            using (var tcpClient = new TcpClient())
            {
                try
                {
                    if (!tcpClient.ConnectAsync(ip, int.Parse(port)).Wait(3000))
                    {
                        throw new Exception("connect to " + ip + ":" + port + " timeout within 3000ms");
                    }
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                    if (ex.InnerException != null)
                    {
                        msg += ex.Message;
                    }
                    m_btnProduce.IsEnabled = true;
                    m_btnStop.IsEnabled = false;
                    MessageBox.Show(msg);
                    return;
                }


                tcpClient.Close();
            }
            if (m_broker.Contains(":"))
            {
                broker = m_broker;
            }
            else
            {
                broker = m_broker + ":9092";
            }
            var config = new ProducerConfig
            {
                BootstrapServers = broker,
                MessageTimeoutMs = 3000,
            };

            // If serializers are not specified, default serializers from
            // `Confluent.Kafka.Serializers` will be automatically used where
            // available. Note: by default strings are encoded as UTF8.
            using (var p = new ProducerBuilder<Null, string>(config).Build())
            {

                int cnt = 0;
                while (cnt < cntMax)
                {
                    try
                    {
                        var res = await p.ProduceAsync(m_topic, new Message<Null, string> { Value = m_msg });
                        OnProduceCompleted?.Invoke(this, res.Message.ToString());
                        cnt++;
                    }
                    catch (ProduceException<Null, string> e)
                    {
                        MessageBox.Show(e.Message);
                        break;
                    }
                    Thread.Sleep(interval);
                }
                OnProduceCompleted?.Invoke(this, "send " + cnt + " times");

            }
            m_btnProduce.Dispatcher.Invoke(() => { m_btnProduce.IsEnabled = true; });
            m_btnStop.Dispatcher.Invoke(() => { m_btnStop.IsEnabled = true; });
        }
        private void StopClick(object sender, RoutedEventArgs arg)
        {
            m_btnProduce.IsEnabled = true;
            m_btnStop.IsEnabled = false;
            bStop = true;
        }
        private bool bStop;
        private string _m_broker;

        public string m_broker
        {
            get { return _m_broker; }
            set
            {
                _m_broker = value;
                OnPropertyChanged("m_broker");
            }
        }
        private string _m_topic;

        public string m_topic
        {
            get { return _m_topic; }
            set
            {
                _m_topic = value;
                OnPropertyChanged("m_topic");
            }
        }

        private string _m_msg;

        public string m_msg
        {
            get { return _m_msg; }
            set
            {
                _m_msg = value;
                OnPropertyChanged("m_msg");
            }
        }
        private string _m_cnt;

        public string m_cnt
        {
            get { return _m_cnt; }
            set
            {
                _m_cnt = value;
                OnPropertyChanged("m_cnt");
            }
        }


        private string _m_interval;

        public string m_interval
        {
            get { return _m_interval; }
            set
            {
                _m_interval = value;
                OnPropertyChanged("m_interval");
            }
        }
        public event EventHandler<string> OnProduceCompleted;
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
