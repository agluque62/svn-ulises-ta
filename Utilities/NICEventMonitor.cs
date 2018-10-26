using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Xml;
using System.Xml.Serialization;
using System.Management;

using NLog;
using Newtonsoft.Json;

namespace Utilities
{
    public class NicEventMonitor : IDisposable
    {
        /// <summary>
        /// Log this class
        /// </summary>
        private static Logger _Logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// 
        /// </summary>
        public enum LanStatus { Unknown = 0, Up = 1, Down = 2 }
        ///// <summary>
        ///// 
        ///// </summary>
        //public enum TeamType { Marvell, Intel, Unknown }
        /// <summary>
        /// 
        /// </summary>
        public event Action<int> StatusChanged;
        public event Action<string> MessageError;

        public class NICItem : IEquatable<NICItem>
        {
            public int Index { get; set; }
            public string DeviceId { get; set; }
            public LanStatus Status { get; set; }
            public bool Equals(NICItem other)
            {
                return other.DeviceId == DeviceId;
            }
        }
        public List<NICItem> NICList = new List<NICItem>();
        /// <summary>
        /// 
        /// </summary>
        public class NicEventMonitorConfig
        {
            /// <summary>
            /// Log this class
            /// </summary>
            public string TeamingType { get; set; }
            public string WindowsLog { get; set; }
            public string EventSource { get; set; }
            public int UpEventId { get; set; }
            public int DownEventId { get; set; }
            public int PropertyIndex { get; set; }

            public NicEventMonitorConfig()
            {
                TeamingType = "Intel";
                WindowsLog = "System";
                EventSource = "iANSMiniport";
                UpEventId = 15;
                DownEventId = 11;
                PropertyIndex = 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cfg"></param>
        public NicEventMonitor(NicEventMonitorConfig cfg, string filepath = "")
        {
            try
            {
                Cfg = cfg;
                FilePath = filepath;
                Init();
            }
            catch (Exception x)
            {
                _Logger.Error(String.Format("NICEventMonitor Contructor 1 Exception {0}", x.Message), x);
                RaiseMessageError(x.Message);
            }
            Cfg = cfg;
            FilePath = filepath;
        }

        public NicEventMonitor(string jconfig, string filepath)
        {
            try
            {
                Cfg = JsonConvert.DeserializeObject<NicEventMonitorConfig>(jconfig);
                FilePath = filepath;
                Init();
            }
            catch (Exception x)
            {
                _Logger.Error(String.Format("NICEventMonitor Contructor 2 Exception {0}", x.Message), x);
                RaiseMessageError(x.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            try
            {
                NICList.Clear();
            }
            catch (Exception x)
            {
                _Logger.Error(String.Format("NICEventMonitor Stop Exception {0}", x.Message), x);
                RaiseMessageError(x.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lan">0: LAN1, 1: LAN2</param>
        /// <param name="status"></param>
        public void EventSimulate(int lan, bool status)
        {
            try
            {
                if (lan < NICList.Count)
                {
                    using (EventLog eventLog = new EventLog(Cfg.WindowsLog))
                    {
                        eventLog.Source = "EventSimulate";
                        eventLog.WriteEntry(String.Format("Log message {0} example", NICList[lan].DeviceId),
                            EventLogEntryType.Information,
                            status == false ? Cfg.DownEventId : Cfg.UpEventId, 1);

                        eventLog.Close();
                    }
                }
            }
            catch (Exception x)
            {
                _Logger.Error(String.Format("NICEventMonitor Simulate Exception {0}", x.Message), x);
                RaiseMessageError(x.Message);
            }
        }

        protected void Init()
        {
            LogEntriesGet();
            SearchForPhysicalDevices();
            InitialStatusGet();
        }

        /// <summary>
        /// 
        /// </summary>
        protected void SearchForPhysicalDevices()
        {
            try
            {
                int Index = 0;
                NICList.Clear();
                foreach (EventRecord evento in _LogEntries)
                {
                    if (evento.Properties.Count > 0)
                    {
                        string idDevice = evento.Properties.Count > Cfg.PropertyIndex ? evento.Properties[Cfg.PropertyIndex].Value.ToString() : "Unknow";
#if DEBUG
                        if (NICList.Contains(new NICItem() { DeviceId = idDevice }) == false)
#else
                        if (NICList.Count < 2 && NICList.Contains(new NICItem() { DeviceId = idDevice }) == false)
#endif
                        {
                            NICList.Add(new NICItem() { DeviceId = idDevice, Status = LanStatus.Down, Index = Index++ });
                        }
                    }
                }
                NICList = NICList.OrderBy(lan => lan.DeviceId).ToList();
            }
            catch (Exception x)
            {
                _Logger.Error(String.Format("NICEventMonitor SearchForPhysicalDevices Exception {0}", x.Message), x);
                RaiseMessageError(x.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        protected string DeviceGetFromXml(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            XmlNode node = doc.DocumentElement.SelectSingleNode(".EventData");
            return node != null ? node.InnerText : "No encontrado";
        }

        /// <summary>
        /// 
        /// </summary>
        protected void InitialStatusGet()
        {
            try
            {
                /** Leer la lista de eventos */
                for (int lan = 0; lan < NICList.Count; lan++)
                {
                    string LanDevice = NICList[lan].DeviceId;
                    List<EventRecord> last_lan_ev_down = _LogEntries.Where(e => Cfg.DownEventId == (e.Id) && e.ToXml().Contains(LanDevice)).OrderByDescending(e => e.TimeCreated).ToList();
                    List<EventRecord> last_lan_ev_up = _LogEntries.Where(e => Cfg.UpEventId == (e.Id) && e.ToXml().Contains(LanDevice)).OrderByDescending(e => e.TimeCreated).ToList();

                    long last_lan_down = last_lan_ev_down.Count == 0 ? DateTime.MinValue.Ticks : last_lan_ev_down[0].TimeCreated.Value.Ticks;
                    long last_lan_up = last_lan_ev_up.Count == 0 ? DateTime.MinValue.Ticks : last_lan_ev_up[0].TimeCreated.Value.Ticks;

                    NICList[lan].Status = last_lan_up > last_lan_down ? LanStatus.Up : LanStatus.Down;
                }
            }
            catch (Exception x)
            {
                _Logger.Error(String.Format("NICEventMonitor InitialStatusGet Exception {0}", x.Message), x);
                RaiseMessageError(x.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected void LogEntriesGet()
        {
            string evtQuery = String.Format("*[System[(Provider/@Name=\"{0}\") and ((EventID={1}) or (EventID={2}) )]]", Cfg.EventSource, Cfg.DownEventId, Cfg.UpEventId);
            EventLogQuery logquery = FilePath == "" ? new EventLogQuery(Cfg.WindowsLog, PathType.LogName, evtQuery) :
                new EventLogQuery(FilePath, PathType.FilePath, evtQuery);

            EventLogReader elr = new EventLogReader(logquery);

            EventRecord entry;
            _LogEntries.Clear();
            while ((entry = elr.ReadEvent()) != null)
            {
                _LogEntries.Add(entry);
            }
            _LogEntries = _LogEntries.OrderByDescending(e => e.TimeCreated).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mensaje"></param>
        private void RaiseMessageError(string mensaje)
        {
            if (MessageError != null) MessageError(mensaje);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lan"></param>
        private void RaiseStatusChanged(int lan)
        {
            if (StatusChanged != null) StatusChanged(lan);
        }

        private string FilePath { get; set; }
        private NicEventMonitorConfig Cfg { get; set; }
        private List<EventRecord> _LogEntries = new List<EventRecord>();
    }
}
