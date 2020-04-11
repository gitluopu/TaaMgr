using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Configuration;
using SshLogin;
using TaaConf;
using CommonCmd;
using Common;
using KafkaCtl;
using System.IO;

namespace TaaMgr
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            m_confCtl.Visibility = Visibility.Hidden;

            //set txtbox property
            m_txtKafkaLog.TextWrapping = TextWrapping.Wrap;
            m_txtKafkaLog.AutoWordSelection = true;
            m_txtKafkaLog.IsReadOnly = true;
            m_txtKafkaLog.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

            this.Closed += (sender, e) => {
                Configuration cf = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                cf.AppSettings.Settings["consumerBroker"].Value = m_consumerCtl.m_broker;
                cf.AppSettings.Settings["consumerTopic"].Value = m_consumerCtl.m_topic;
                cf.AppSettings.Settings["consumerCnt"].Value = m_consumerCtl.m_cnt;

                cf.AppSettings.Settings["producerBroker"].Value = m_producerCtl.m_broker;
                cf.AppSettings.Settings["producerTopic"].Value = m_producerCtl.m_topic;
                cf.AppSettings.Settings["producerCnt"].Value = m_producerCtl.m_cnt;
                cf.AppSettings.Settings["producerInterval"].Value = m_producerCtl.m_interval;
                cf.AppSettings.Settings["producerMsg"].Value = m_producerCtl.m_msg;
                cf.Save();
            };
        }
        private void SShLoginOnLoad(object sender, RoutedEventArgs e)
        {
            var sshLogin = sender as SShLoginCtl;
            sshLogin.m_ip = ConfigurationManager.AppSettings["sshIp"];
            sshLogin.m_user = ConfigurationManager.AppSettings["sshUser"];
            sshLogin.m_pwBox.Password = ConfigurationManager.AppSettings["sshPasswd"];
            //
            //cfa.AppSettings.Settings["NAME"].Value = "WANGLICHAO";
        }
        private void SShLoginOnConnect(object sender, RoutedEventArgs e)
        {
            m_confCtl.Visibility = Visibility.Visible;
            var sshLogin = sender as SShLoginCtl;
            Configuration cf = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            cf.AppSettings.Settings["sshIp"].Value = sshLogin.m_ip;
            cf.AppSettings.Settings["sshUser"].Value = sshLogin.m_user;
            cf.AppSettings.Settings["sshPasswd"].Value = sshLogin.m_passwd;
            cf.Save();

        }
        private void SShLoginOnDisconnect(object sender, RoutedEventArgs e)
        {
            m_confCtl.Visibility = Visibility.Hidden;
        }
        private void TaaConfIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (m_confCtl.Visibility==Visibility.Visible)
            {
                m_confCtl.m_icmd = new TaaCmdUnix(new Glue.SShCmd(m_sshLoginCtl.m_ssh,m_sshLoginCtl.m_scp));
                m_confCtl.InitData();
            }
            else
            {
                m_confCtl.m_icmd = null;
            }
        }
        private void OnConsumerLoaded(object sender, RoutedEventArgs e)
        {

            m_consumerCtl.m_broker = ConfigurationManager.AppSettings["consumerBroker"];
            m_consumerCtl.m_topic = ConfigurationManager.AppSettings["consumerTopic"];
            m_consumerCtl.m_cnt = ConfigurationManager.AppSettings["consumerCnt"];
        }
        
        private void OnConsumeMsg(object sender, string e)
        {
            m_txtKafkaLog.Dispatcher.Invoke(()=> { m_txtKafkaLog.AppendText(e+System.Environment.NewLine); });
        }

        private void OnProducerLoaded(object sender, RoutedEventArgs e)
        {
            m_producerCtl.m_broker = ConfigurationManager.AppSettings["producerBroker"] ;
            m_producerCtl.m_topic =ConfigurationManager.AppSettings["producerTopic"] ;
            m_producerCtl.m_cnt = ConfigurationManager.AppSettings["producerCnt"] ;
            m_producerCtl.m_interval =  ConfigurationManager.AppSettings["producerInterval"] ;
            m_producerCtl.m_msg = ConfigurationManager.AppSettings["producerMsg"] ;
        }
        
        private void OnProduceCompleted(object sender, string e)
        {
            m_txtKafkaLog.Dispatcher.Invoke(() => { m_txtKafkaLog.AppendText(e+ System.Environment.NewLine); });
        }
        private void ClearClick(object sender, RoutedEventArgs e)
        {
            m_txtKafkaLog.Clear();
        }
        
    }
}
