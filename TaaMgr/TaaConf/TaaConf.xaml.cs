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
            m_confs.Add(new ConfItem("luopu","boy"));
            m_confs.Add(new ConfItem("luojianuo", "girl"));
            DataContext = this;
        }
        public class ConfItem
            {
            public ConfItem(string name,string val)
            {
                m_name = name;
                m_value = val;
            }
            public string m_name { get; set; }
            public string m_value { get; set; }
            }
        public List<ConfItem> m_confs { get; set; }
    }
}
