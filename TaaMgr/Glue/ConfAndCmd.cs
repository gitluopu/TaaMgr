using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;
using Common;
using TaaConf;
using CmdLib;
using System.IO;

namespace TaaMgr.Glue
{
    public  class SShCmd : ICmnCmd
    {
       public SShCmd(SshClient ssh,ScpClient scp)
        {
            m_ssh = ssh;
            m_scp = scp;
        }
        public void Download(string src, FileInfo dst)
        {
             m_scp.Download(src, dst);
        }

        public void Download(string src, DirectoryInfo dst)
        {
            m_scp.Download(src, dst);
        }

        public CmnCmdResult RunCommand(string txt)
        {
            var r = m_ssh.RunCommand(txt);
            CmnCmdResult res = new CmnCmdResult(r.Result,r.Error,r.ExitStatus);
            return res;
        }

        public void Upload(FileInfo src, string dst)
        {
            m_scp.Upload(src,dst);
        }

        public void Upload(DirectoryInfo src, string dst)
        {
            m_scp.Upload(src, dst);
        }
        private SshClient m_ssh;
        private ScpClient m_scp;
    }
}
