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
using CmdLib;
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
            m_confCtl.Visibility = Visibility.Collapsed;
            //set txtbox property
            m_txtKafkaLog.TextWrapping = TextWrapping.Wrap;
            m_txtKafkaLog.AutoWordSelection = true;
            m_txtKafkaLog.IsReadOnly = true;
            m_txtKafkaLog.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        }
        ~MainWindow()
        {
            AppConfig.Close();
        }

        private  void SShOnLogin(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                m_sshLoginCtl.Visibility = Visibility.Collapsed;
                m_sshLoginCtl.Dispatcher.Invoke(() => m_sshLoginCtl.Visibility = Visibility.Collapsed);
                m_confCtl.Dispatcher.Invoke(() => m_confCtl.Visibility = Visibility.Visible);
                m_confCtl.m_taaIp = m_sshLoginCtl.m_ip;
                m_confCtl.m_icmd = new TaaCmdUnix(new Glue.SShCmd(m_sshLoginCtl.m_ssh, m_sshLoginCtl.m_scp));
                m_sshLoginCtl.Login(sender, e);
                m_confCtl.Login(sender, e);
            });
        }
        private void SShOnLogout(object sender, RoutedEventArgs e)
        {
            m_confCtl.Visibility = Visibility.Collapsed;
            m_sshLoginCtl.Visibility = Visibility.Visible;
            m_confCtl.Logout(sender, e);
            m_sshLoginCtl.Logout(sender, e);
        }
       
        private void TaaConfOnLogout(object sender, RoutedEventArgs e)
        {
            m_confCtl.Visibility = Visibility.Collapsed;
            m_sshLoginCtl.Visibility = Visibility.Visible;
            m_confCtl.Logout(sender,e);
            m_sshLoginCtl.Logout(sender,e);
        }
        
        private void OnConsumeMsg(object sender, string e)
        {
            m_txtKafkaLog.Dispatcher.Invoke(()=> { m_txtKafkaLog.AppendText(e+System.Environment.NewLine); });
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
