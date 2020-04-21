using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CmdLib;
namespace TaaConf
{
   
    public class DiagnoseItem
    {
        public DiagnoseItem(string name,string desc, DianoseFunc f)
        {
            m_name = name;
            m_desc = desc;
            m_func = f;
        }
        public string m_name { get; set; }
        public string m_desc { get; set; }
        public DianoseFunc m_func { get; set; }
    }
    public class AutoDiagnose
    {
        public AutoDiagnose()
        {
            m_items = new List<DiagnoseItem>();
            m_items.Add(new DiagnoseItem("auth", "look up /etc/license/IN-SEC.lic and TAAMaster.log",null));
        }
        private List<DiagnoseItem> m_items;
    }
}
