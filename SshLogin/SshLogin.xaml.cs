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
using Renci.SshNet;
using Common;
namespace SshLogin
{
    /// <summary>
    /// Interaction logic for SShLoginCtl.xaml
    /// </summary>
    public partial class SShLoginCtl : UserControl,INotifyPropertyChanged
    {
        public SShLoginCtl()
        {
            if (AppConfig.m_conf.AppSettings.Settings["sshlogin.ip"] == null)
                AppConfig.m_conf.AppSettings.Settings.Add("sshlogin.ip", "172.16.2.82");
            if (AppConfig.m_conf.AppSettings.Settings["sshlogin.user"] == null)
                AppConfig.m_conf.AppSettings.Settings.Add("sshlogin.user", "root");
            if (AppConfig.m_conf.AppSettings.Settings["sshlogin.passwd"] == null)
                AppConfig.m_conf.AppSettings.Settings.Add("sshlogin.passwd", "Silvers$R7!");
            InitializeComponent();
            DataContext = this;
            Loaded += (s,arg) => m_pwBox.Password = m_passwd;
        }

        
        private async void LoginClick(object sender, RoutedEventArgs arg)
        {
            await Task.Run(() =>
            {
                m_btnLogin.Dispatcher.Invoke(()=> { m_btnLogin.IsEnabled = false; });
                
                m_passwd = m_pwBox.Password;
                m_ssh = new SshClient(m_ip, m_user, m_passwd);
                m_ssh.ConnectionInfo.Timeout = TimeSpan.FromSeconds(3);
                m_scp = new ScpClient(m_ip, m_user, m_passwd);
                m_scp.ConnectionInfo.Timeout = TimeSpan.FromSeconds(3);
                try
                {
                    m_ssh.Connect();
                    m_scp.Connect();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    if (m_ssh.IsConnected)
                    {
                        m_ssh.Disconnect();
                    }
                    m_btnLogin.Dispatcher.Invoke(() => { m_btnLogin.IsEnabled = true; });
                    return;
                }
                OnLogin?.Invoke(this, arg);
            });
        }
        public void Login(object sender, RoutedEventArgs e)
        {
           
        }
        public void Logout(object sender, RoutedEventArgs e)
        {
            m_scp.Disconnect();
            m_ssh.Disconnect();
            m_btnLogin.IsEnabled = true;
        }
        public SshClient m_ssh { get; private set; }
        public ScpClient m_scp { get; private set; }


        public string m_ip
        {
            get { return AppConfig.m_conf.AppSettings.Settings["sshlogin.ip"].Value; }
            set
            {
                AppConfig.m_conf.AppSettings.Settings["sshlogin.ip"].Value = value;
                OnPropertyChanged("m_ip");
            }
        }

        
        public string m_user
        {
            get { return AppConfig.m_conf.AppSettings.Settings["sshlogin.user"].Value; }
            set
            {
                AppConfig.m_conf.AppSettings.Settings["sshlogin.user"].Value = value;
                OnPropertyChanged("m_user");
            }
        }
        public string m_passwd
        {
            get { return AppConfig.m_conf.AppSettings.Settings["sshlogin.passwd"].Value; }
            set
            {
                AppConfig.m_conf.AppSettings.Settings["sshlogin.passwd"].Value = value;
                //OnPropertyChanged("m_passwd");
            }
        }
        
        public event RoutedEventHandler OnLogin;
        public event RoutedEventHandler OnLogout;
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
