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
            DataContext = this;
        }
        private  void ProduceClick(object sender, RoutedEventArgs arg)
        {
            m_btnProduce.IsEnabled = false;
            bStop = false;
            
            Task.Run(() =>
            {
                string broker;
                int cntMax = int.Parse(m_cnt);
                int interval = int.Parse(m_interval);
                if (m_broker.Contains(":"))
                {
                    broker = m_broker;
                }
                else
                {
                    broker = m_broker + ":9092";
                }
                var config = new ProducerConfig { BootstrapServers = broker,
                    SocketTimeoutMs = 3000,
                    MessageTimeoutMs =3000,
                    TransactionTimeoutMs = 3000,
                };

                // If serializers are not specified, default serializers from
                // `Confluent.Kafka.Serializers` will be automatically used where
                // available. Note: by default strings are encoded as UTF8.
                using (var p = new ProducerBuilder<Null, string>(config).Build())
                {
                    try
                    {
                        int cnt = 0;
                            while(bStop==false)
                            { 
                                p.Produce(m_topic, new Message<Null, string> { Value = m_msg });
                            cnt++;
                            if(cnt>=cntMax)
                            {
                                break;
                            }
                                Thread.Sleep(interval);
                            }
                        OnProduceCompleted?.Invoke(this,"send "+ cnt + " times successfully");


                    }
                    catch (ProduceException<Null, string> e)
                    {
                        MessageBox.Show(e.Message);
                    }
                }
                m_btnProduce.Dispatcher.Invoke(()=> { m_btnProduce.IsEnabled = true; });
                
            });
        }
        private void StopClick(object sender, RoutedEventArgs arg)
        {
            m_btnProduce.IsEnabled = true;
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
