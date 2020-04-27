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
using CmdLib;
using Common;
namespace TaaConf
{
    /// <summary>
    /// Interaction logic for AutotDiagnostic.xaml
    /// </summary>
    public partial class AutotDiagnostic : UserControl
    {
        public AutotDiagnostic(TaaConfCtl conf)
        {
            InitializeComponent();
            m_conf = conf;
            BrokerUnit bu=null;
            foreach (var ele in m_conf.m_confs)
                if (ele.m_name == "broker")
                    bu = new BrokerUnit(ele.m_value);
            m_items = new List<DiagnoseItem>();
            m_items.Add(new DiagnoseItem("auth", "look up /etc/license/IN-SEC.lic and TAAMaster.log", m_conf.m_icmd.IsAuthOkay));
            m_items.Add(new DiagnoseItem("kafka", "connect broker", m_conf.m_icmd.IsKfkaOkay, bu));
            DataContext = this;
            Task.Run(() =>{ 
            foreach (var ele in m_items)
                 ele.m_func(ele.m_result);
            });
        }
        public class DiagnoseItem
        {
            public DiagnoseItem(string name, string desc, DianoseFunc f)
            {
                m_name = name;
                m_desc = desc;
                m_func = f;
                m_result = new DiagnoseResult();
            }
            public DiagnoseItem(string name, string desc, DianoseFunc f,object param)
            {
                m_name = name;
                m_desc = desc;
                m_func = f;
                m_result = new DiagnoseResult();
                m_result.m_param = param;
            }
            public string m_name { get; set; }
            public string m_desc { get; set; }
            public DianoseFunc m_func { get; set; }
            public DiagnoseResult m_result { get; set; }
        }

        private TaaConfCtl m_conf { get; set; }
        public List<DiagnoseItem> m_items { get; set; }
    }
}
