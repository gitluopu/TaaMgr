using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Renci.SshNet;
namespace SshLogin
{
    /// <summary>
    /// Interaction logic for SShLoginCtl.xaml
    /// </summary>
    public partial class SShLoginCtl : UserControl,INotifyPropertyChanged
    {
        public SShLoginCtl()
        {
            InitializeComponent();
            m_btnConnect.IsEnabled = true;
            m_btnDisconnect.IsEnabled = false;
            DataContext = this;
        }

        
        private void BtnConnectClick(object sender, RoutedEventArgs arg)
        {
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
                if(m_ssh.IsConnected)
                {
                    m_ssh.Disconnect();
                }
                return;
            }
            m_btnConnect.IsEnabled = false;
            m_btnDisconnect.IsEnabled = true;
            OnConnect?.Invoke(this,arg);
        }
        private void BtnDisconnectClick(object sender, RoutedEventArgs e)
        {
            m_ssh.Disconnect();
            m_scp.Disconnect();
            m_btnConnect.IsEnabled = true;
            m_btnDisconnect.IsEnabled = false;
            OnDisconnect?.Invoke(this, e);
        }
        private SshClient m_ssh;
        private ScpClient m_scp;
        private string _m_ip;

        public string m_ip
        {
            get { return _m_ip; }
            set
            {
                _m_ip = value;
                OnPropertyChanged("m_ip");
            }
        }

        private string _m_user;

        public string m_user
        {
            get { return _m_user; }
            set
            {
                _m_user = value;
                OnPropertyChanged("m_user");
            }
        }

        public string m_passwd { get; set; }
        public event RoutedEventHandler OnConnect;
        public event RoutedEventHandler OnDisconnect;
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
