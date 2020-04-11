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
    /// Interaction logic for ConsumerCtl.xaml
    /// </summary>
    public partial class ConsumerCtl : UserControl, INotifyPropertyChanged
    {
        public ConsumerCtl()
        {
            InitializeComponent();
            m_btnConsume.IsEnabled = true;
            DataContext = this;
        }

        private void ConsumeClick(object sender, RoutedEventArgs arg)
        {
            m_btnConsume.IsEnabled = false;
            Task.Run(() =>
            {
                string broker;
                UInt32 cntMax = UInt32.Parse(m_cnt);
                if (m_broker.Contains(":"))
                {
                    broker = m_broker;
                }
                else
                {
                    broker = m_broker + ":9092";
                }
                var conf = new ConsumerConfig
                {
                    GroupId = "test-consumer-group",

                    BootstrapServers = m_broker,
                    SocketTimeoutMs = 3000,
                    // Note: The AutoOffsetReset property determines the start offset in the event
                    // there are not yet any committed offsets for the consumer group for the
                    // topic/partitions of interest. By default, offsets are committed
                    // automatically, so in this example, consumption will only start from the
                    // earliest message in the topic 'my-topic' the first time you run the program.
                    AutoOffsetReset = AutoOffsetReset.Earliest
                };
                using (var c = new ConsumerBuilder<Ignore, string>(conf).Build())
                {
                    c.Subscribe(m_topic);
                    m_cts = new CancellationTokenSource();
                    uint cnt = 0;
                    try
                    {
                        while (true)
                        {
                            try
                            {
                                var cr = c.Consume(m_cts.Token);
                                OnConsumeMsg?.Invoke(this, cr.Message.Value);
                                cnt++;
                                if (cntMax != 0)
                                {
                                    if (cnt >= cntMax)
                                    {
                                        m_cts.Cancel();
                                        break;
                                    }
                                }
                                Thread.Sleep(10);
                                //Console.WriteLine($"Consumed message '{cr.Value}' at: '{cr.TopicPartitionOffset}'.");
                            }
                            catch (ConsumeException ex)
                            {
                                MessageBox.Show(ex.Message);
                                //Log.LogErr(e.Error.Reason);
                                //Console.WriteLine($"Error occured: {e.Error.Reason}");
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // Ensure the consumer leaves the group cleanly and final offsets are committed.
                        c.Close();
                        //return;
                    }
                }
                m_btnConsume.Dispatcher.Invoke(() => { m_btnConsume.IsEnabled = true; });
            });
        }
        private void StopClick(object sender, RoutedEventArgs arg)
        {
            if(m_cts!=null)
            {
                m_cts.Cancel();
            }
            m_btnConsume.IsEnabled = true;
        }
        private string _m_broker;

        public string m_broker
        {
            get { return _m_broker; }
            set { _m_broker = value;
                OnPropertyChanged("m_broker");
            }
        }
        private string _m_topic;

        public string m_topic
        {
            get { return _m_topic; }
            set { _m_topic = value;
                OnPropertyChanged("m_topic");
            }
        }

        private string _m_cnt;

        public string m_cnt
        {
            get { return _m_cnt; }
            set { _m_cnt = value;
                OnPropertyChanged("m_cnt");
            }
        }

        private CancellationTokenSource m_cts;
        public event EventHandler<string> OnConsumeMsg;
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
