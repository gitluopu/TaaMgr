﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
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
using System.IO;

namespace KafkaCtl
{
    /// <summary>
    /// Interaction logic for ConsumerCtl.xaml
    /// </summary>
    public partial class ConsumerCtl : UserControl, INotifyPropertyChanged
    {
        public List<string> m_liTopics { get; set; }
        public ConsumerCtl()
        {
            if (AppConfig.m_conf.AppSettings.Settings["consumer.broker"] == null)
                AppConfig.m_conf.AppSettings.Settings.Add("consumer.broker", "172.16.2.82");
            if (AppConfig.m_conf.AppSettings.Settings["consumer.topic"] == null)
                AppConfig.m_conf.AppSettings.Settings.Add("consumer.topic", "audit");
            if (AppConfig.m_conf.AppSettings.Settings["consumer.cnt"] == null)
                AppConfig.m_conf.AppSettings.Settings.Add("consumer.cnt", "20");
            if (AppConfig.m_conf.AppSettings.Settings["consumer.groupId"] == null)
                AppConfig.m_conf.AppSettings.Settings.Add("consumer.groupId", "");
            InitializeComponent();
            m_btnConsume.IsEnabled = true;
            m_btnStop.IsEnabled = false;
            m_liTopics = new List<string>();
            m_liTopics.Add("audit");
            m_liTopics.Add("alert");
            m_liTopics.Add("traffic");
            m_liTopics.Add("event");
            m_liTopics.Add("PushResponse");

            DataContext = this;
            m_save2File = false;
        }

        private void ConsumeClick(object sender, RoutedEventArgs arg)
        {
            m_btnConsume.IsEnabled = false;
            m_btnStop.Dispatcher.Invoke(() => { m_btnStop.IsEnabled = true; });
            BrokerUnit bu = new BrokerUnit(m_broker);
                try
                {
                    Common.Net.TcpConnectionTest(bu.m_ip, bu.m_port,int.Parse(AppConfig.m_conf.AppSettings.Settings["operationTimeout"].Value));
                }
                catch (Exception ex)
                {
                    m_btnConsume.IsEnabled = true;
                    m_btnStop.IsEnabled = false;
                    MessageBox.Show(ex.Message);
                    return;
                }
            Task.Run(() =>
        {
            UInt32 cntMax = UInt32.Parse(m_cnt);
            m_cts = new CancellationTokenSource();
            StreamWriter writer = null;
            if (m_save2File)
                writer = new StreamWriter(Common.Dir.GetCacheDir() + bu.m_ip + "-" + m_topic + ".log");
            string groupId = null;
            if (m_groupId == "")
                groupId = DateTime.Now.ToString();
            else
                groupId = m_groupId;
            var conf = new ConsumerConfig
            {
                GroupId = groupId,
                //AutoOffsetReset = AutoOffsetReset.Latest,
                BootstrapServers = bu.m_broker,
                    //SocketTimeoutMs = 1000,
                    //SessionTimeoutMs = 1000,
                    //MetadataRequestTimeoutMs=1000,
                    ReconnectBackoffMs = 1000,
                ReconnectBackoffMaxMs = 3000,


                    // Note: The AutoOffsetReset property determines the start offset in the event
                    // there are not yet any committed offsets for the consumer group for the
                    // topic/partitions of interest. By default, offsets are committed
                    // automatically, so in this example, consumption will only start from the
                    // earliest message in the topic 'my-topic' the first time you run the program.
                    AutoOffsetReset = AutoOffsetReset.Latest
            };
            using (var c = new ConsumerBuilder<Ignore, string>(conf).Build())
            {
                c.Subscribe(m_topic);
                uint cnt = 0;
                try
                {
                    while (true)
                    {
                        try
                        {
                            var cr = c.Consume(m_cts.Token);
                            if (m_save2File)
                                writer.Write(cr.Message.Value + System.Environment.NewLine);
                            OnConsumeMsg?.Invoke(this, cr.Message.Value);
                            cnt++;
                            if (cntMax != 0)
                            {
                                if (cnt >= cntMax)
                                {
                                    c.Close();
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
            if (m_save2File)
                writer.Close();

            m_btnConsume.Dispatcher.Invoke(() => { m_btnConsume.IsEnabled = true; });
            m_btnStop.Dispatcher.Invoke(() => { m_btnStop.IsEnabled = false; });
        });
        }
        private void StopClick(object sender, RoutedEventArgs arg)
        {
            m_cts.Cancel();
            m_btnConsume.IsEnabled = true;
            m_btnStop.IsEnabled = false;
        }

        public string m_broker
        {
            get { return AppConfig.m_conf.AppSettings.Settings["consumer.broker"].Value; }
            set
            {
                AppConfig.m_conf.AppSettings.Settings["consumer.broker"].Value = value;
                OnPropertyChanged("m_broker");
            }
        }

        public string m_topic
        {
            get { return AppConfig.m_conf.AppSettings.Settings["consumer.topic"].Value; }
            set
            {
                AppConfig.m_conf.AppSettings.Settings["consumer.topic"].Value = value;
                OnPropertyChanged("m_topic");
            }
        }

        public string m_cnt
        {
            get { return AppConfig.m_conf.AppSettings.Settings["consumer.cnt"].Value; }
            set
            {
                AppConfig.m_conf.AppSettings.Settings["consumer.cnt"].Value = value;
                OnPropertyChanged("m_cnt");
            }
        }
        public string m_groupId
        {
            get { return AppConfig.m_conf.AppSettings.Settings["consumer.groupId"].Value; }
            set
            {
                AppConfig.m_conf.AppSettings.Settings["consumer.groupId"].Value = value;
                OnPropertyChanged("m_groupId");
            }
        }
        private bool _m_save2File;

        public bool m_save2File
        {
            get { return _m_save2File; }
            set
            {
                _m_save2File = value;
                OnPropertyChanged("m_save2File");
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
