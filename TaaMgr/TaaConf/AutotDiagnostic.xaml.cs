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
namespace TaaConf
{
    /// <summary>
    /// Interaction logic for AutotDiagnostic.xaml
    /// </summary>
    public partial class AutotDiagnostic : UserControl
    {
        public AutotDiagnostic(ITaaCmd cmd)
        {
            InitializeComponent();
            m_cmd = cmd;
            m_items = new List<DiagnoseItem>();
            m_items.Add(new DiagnoseItem("auth", "look up /etc/license/IN-SEC.lic and TAAMaster.log", m_cmd.IsAuthOkay));
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
            public string m_name { get; set; }
            public string m_desc { get; set; }
            public DianoseFunc m_func { get; set; }
            public DiagnoseResult m_result { get; set; }
        }

        private ITaaCmd m_cmd { get; set; }
        public List<DiagnoseItem> m_items { get; set; }
    }
}
