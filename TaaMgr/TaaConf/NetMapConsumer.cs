using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CmdLib;
using Confluent.Kafka;
namespace TaaConf
{
    public class NetMapConsumer
    {
        public long m_interval { get; private set; }
        private readonly object m_dataLock = new object();
        public event EventHandler<Tuple<long, long>> OnHandleMsgDone;
        class NetMapData
        {
            public NetMapData()
            {
                Reset();
            }
            public void Reset()
            {
                m_pps = 0;
                m_bps = 0;
                m_cnt = 0;
            }
            public NetMapData ShallowCopy()
            {
                return (NetMapData)this.MemberwiseClone();
            }
            public long m_pps { get; set; }
            public long m_bps { get; set; }
            public long m_cnt { get; set; }
        }
        private NetMapData m_dataTmp;
        private List<NetMapData> m_data;
        public NetMapConsumer(string interval)
        {
            m_interval = long.Parse(interval);
            m_data = new List<NetMapData>();
            m_dataTmp = new NetMapData();
            m_timer = new System.Windows.Threading.DispatcherTimer();
            m_ct = new CancellationTokenSource();
            m_timer.Tick += (s, e) =>
            {
                lock (m_dataLock)
                {
                    if (m_dataTmp.m_cnt != 0)
                    {
                        OnHandleMsgDone?.Invoke(this,new Tuple<long, long>(m_dataTmp.m_pps, m_dataTmp.m_bps));
                        m_dataTmp.Reset();
                    }
                }
                if(m_ct.IsCancellationRequested)
                {
                    m_timer.Stop();
                }
            };
            m_timer.Interval = new TimeSpan(0, 0, Convert.ToInt32(m_interval));
            m_timer.Start();
        }
        private EventHandler<Tuple<long, long>> m_sendMsgEvent;
        public void RegisterSendMsgEvent(EventHandler<Tuple<long, long>> e)
        {
            m_sendMsgEvent += e;
        }
       
        public void HandleMsg(string msg)
        {
            CmdLib.Confs.NetMap.CNetMap[] netMaps;
            try
            {
                netMaps = CmdLib.Confs.NetMap.CNetMap.FromJson(msg);
            }
            catch (Exception ex)
            {
                throw new OperationCanceledException(ex.Message + "srcStr=" + msg);
            }
            long subPps = 0, subBps = 0;
            long avePps = 0, aveBps = 0;
            foreach (var ele in netMaps)
            {
                subPps += ele.Packets;
                subBps += ele.Bytes;
            }
            avePps = subPps / m_interval;
            aveBps = subBps / m_interval;
            lock (m_dataLock)
            {
                m_dataTmp.m_pps += avePps;
                m_dataTmp.m_bps += aveBps;
                m_dataTmp.m_cnt++;
            }
            return;
        }
        public void Stop()
        {
            m_ct.Cancel();
            
        }
        CancellationTokenSource m_ct;
        System.Windows.Threading.DispatcherTimer m_timer;       
    }
}
