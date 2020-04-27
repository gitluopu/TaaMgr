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
using Common;
namespace KafkaCtl
{
    /// <summary>
    /// Interaction logic for ProducerCtl.xaml
    /// </summary>
    public partial class ProducerCtl : UserControl, INotifyPropertyChanged
    {
        public ProducerCtl()
        {
            if (AppConfig.m_conf.AppSettings.Settings["producer.broker"] == null)
                AppConfig.m_conf.AppSettings.Settings.Add("producer.broker", "172.16.2.82");
            if (AppConfig.m_conf.AppSettings.Settings["producer.topic"] == null)
                AppConfig.m_conf.AppSettings.Settings.Add("producer.topic", "audit");
            if (AppConfig.m_conf.AppSettings.Settings["producer.cnt"] == null)
                AppConfig.m_conf.AppSettings.Settings.Add("producer.cnt", "20");
            if (AppConfig.m_conf.AppSettings.Settings["producer.msg"] == null)
                AppConfig.m_conf.AppSettings.Settings.Add("producer.msg", "test");
            if (AppConfig.m_conf.AppSettings.Settings["producer.interval"] == null)
                AppConfig.m_conf.AppSettings.Settings.Add("producer.interval", "1000");
            InitializeComponent();
            m_btnStop.IsEnabled = false;
            DataContext = this;
        }
        private  void ProduceClick(object sender, RoutedEventArgs arg)
        {
            m_btnProduce.IsEnabled = false;
            m_btnStop.IsEnabled = true;
            int cntMax = int.Parse(m_cnt);
            int interval = int.Parse(m_interval);
            BrokerUnit bu = new BrokerUnit(m_broker);
            try
            {
                Common.Net.TcpConnectionTest(bu.m_ip, bu.m_port, int.Parse(AppConfig.m_conf.AppSettings.Settings["operationTimeout"].Value));
            }
            catch (Exception ex)
            {
                m_btnProduce.IsEnabled = true;
                m_btnStop.IsEnabled = false;
                MessageBox.Show(ex.Message);
                return;
            }
            m_cts = new CancellationTokenSource();
            Task.Run(() =>
            {
                int okayCnt = 0;
                int errCnt = 0;
                int sendCnt = 0;
                HashSet<string> errSet= new HashSet<string>();
                Action<DeliveryReport<Null, string>> handler = new Action<DeliveryReport<Null, string>>((r) =>{
                    if (r.Error.IsError)
                    { errCnt++; errSet.Add(r.Error.Reason); }
                    else
                        okayCnt++;
                });
                var config = new ProducerConfig
                {
                    BootstrapServers = bu.m_broker,
                    MessageTimeoutMs = 3000,
                   
                };
                
                using (var p = new ProducerBuilder<Null, string>(config).Build())
                {
                    for (int i = 0; i < cntMax; i++)
                    { 
                        p.Produce(m_topic, new Message<Null, string> {Value = m_msg },handler);
                        sendCnt++;
                        if (interval > 0)
                            Thread.Sleep(interval);
                        if (m_cts.IsCancellationRequested)
                            break;
                    }
                    p.Flush(m_cts.Token);
                }
                int recvCnt = okayCnt + errCnt;
                string retMsg = "send/recv/error:" + sendCnt + "/" + recvCnt + "/" + errCnt;
                if(errCnt>0)
                {
                    foreach (var ele in errSet)
                        retMsg += "," + ele;
                }
                OnProduceCompleted?.Invoke(this, retMsg);
            });
            m_btnProduce.IsEnabled = true;
            m_btnStop.IsEnabled = false;
        }
    private void StopClick(object sender, RoutedEventArgs arg)
    {
        m_cts.Cancel();
    }

        private CancellationTokenSource m_cts;

    public string m_broker
    {
        get { return AppConfig.m_conf.AppSettings.Settings["producer.broker"].Value; }
        set
        {
            AppConfig.m_conf.AppSettings.Settings["producer.broker"].Value = value;
            OnPropertyChanged("m_broker");
        }
    }
    private string _m_topic;

    public string m_topic
    {
        get { return AppConfig.m_conf.AppSettings.Settings["producer.topic"].Value; }
        set
        {
            AppConfig.m_conf.AppSettings.Settings["producer.topic"].Value = value;
            OnPropertyChanged("m_topic");
        }
    }


    public string m_msg
    {
        get { return AppConfig.m_conf.AppSettings.Settings["producer.msg"].Value; }
        set
        {
            AppConfig.m_conf.AppSettings.Settings["producer.msg"].Value = value;
            OnPropertyChanged("m_msg");
        }
    }

    public string m_cnt
    {
        get { return AppConfig.m_conf.AppSettings.Settings["producer.cnt"].Value; }
        set
        {
            AppConfig.m_conf.AppSettings.Settings["producer.cnt"].Value = value;
            OnPropertyChanged("m_cnt");
        }
    }
    public string m_interval
    {
        get { return AppConfig.m_conf.AppSettings.Settings["producer.interval"].Value; }
        set
        {
            AppConfig.m_conf.AppSettings.Settings["producer.interval"].Value = value;
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
