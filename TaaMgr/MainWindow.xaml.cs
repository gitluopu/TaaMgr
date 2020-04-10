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
            var sshLogin = sender as SShLoginCtl;
            Configuration cf = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            cf.AppSettings.Settings["sshIp"].Value = sshLogin.m_ip;
            cf.AppSettings.Settings["sshUser"].Value = sshLogin.m_user;
            cf.AppSettings.Settings["sshPasswd"].Value = sshLogin.m_passwd;
            cf.Save();

        }
        private void SShLoginOnDisconnect(object sender, RoutedEventArgs e)
        {
            
        }

    }
}
