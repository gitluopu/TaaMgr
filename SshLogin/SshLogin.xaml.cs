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
            DataContext = this;
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
                //if(m_user!="root") //try to login as root
                //{
                //    IDictionary<Renci.SshNet.Common.TerminalModes, uint> termkvp = new Dictionary<Renci.SshNet.Common.TerminalModes, uint>();
                //    termkvp.Add(Renci.SshNet.Common.TerminalModes.ECHO, 53);
                //    ShellStream shellStream = m_ssh.CreateShellStream("xterm", 80, 24, 800, 600, 1024, termkvp);
                //    shellStream.WriteLine("su root");
                //    shellStream.Expect("Password:");
                //    shellStream.WriteLine("Silvers$R7!");
                //}
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
        public event RoutedEventHandler OnLogin;
        public event RoutedEventHandler OnLogout;
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
