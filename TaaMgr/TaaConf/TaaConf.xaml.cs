using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
using CommonCmd;
namespace TaaConf
{
    /// <summary>
    /// Interaction logic for TaaConfCtl.xaml
    /// </summary>
    public partial class TaaConfCtl : UserControl
    {
        public TaaConfCtl()
        {
            InitializeComponent();
            m_confs = new List<ConfItem>();
            m_confs.Add(new ConfItem("version", () => { return m_icmd.GetVersion(); }, null));
            m_confs.Add(new ConfItem("broker",()=>{ return m_icmd.GetBroker();},null));
            //m_confs.Add(new ConfItem("version",);
            //m_confs.Add(new ConfItem("version",);
            //m_confs.Add(new ConfItem("version",);
            //m_confs.Add(new ConfItem("version",);
            //m_confs.Add(new ConfItem("version",);
            //m_confs.Add(new ConfItem("version",);
            //m_confs.Add(new ConfItem("version",);
            DataContext = this;
        }
        public class ConfItem : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged(string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            public delegate string get_t();
            public delegate void set_t(string val);
            public ConfItem(string name, get_t _Get, set_t _Set)
            {
                m_name = name;
                m_value = "unknown";
                Get = _Get;
                Set = _Set;
            }
            public void InitData()
            {
                m_value = Get();
                m_valueBak = m_value;
            }
            public void SetDataChanged()
            {
                if(m_value!=m_valueBak)
                {
                    try { 
                    Set(m_value);
                    }catch(Exception e)
                    {
                        MessageBox.Show(e.Message);
                    }
                }
            }
            private string _m_value;
            public string m_name { get; set; }
            public string m_value { get=>_m_value; set { _m_value = value;OnPropertyChanged("m_value"); } }
            public string m_valueBak { get; set; }
            public get_t Get;
            public set_t Set;
        }
        public void InitData()
        {
            string remoteSvrluginPath = "/opt/in-sec/taa/svrplugin";
            string localSvrluginPath = Common.Dir.GetCacheDir() + "svrplugin";
            DirectoryInfo di = new DirectoryInfo(localSvrluginPath);
            if (di.Exists)
            {
                di.Delete(true);
            }
            di.Create();
            try { 
            m_icmd.GetCmnCmd().Download(remoteSvrluginPath,di);
            }catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            foreach (var ele in m_confs)
            {
                ele.InitData();
            }
        }
        public void SetDataChanged()
        {
            foreach (var ele in m_confs)
            {
                ele.SetDataChanged();
            }
        }
        public List<ConfItem> m_confs { get; set; }
        public ITaaCmd m_icmd;
    }
}
