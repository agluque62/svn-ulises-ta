using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Xml;
using System.Management;

using NLog;

namespace HMI.CD40.Module.BusinessEntities

{
    class NICEventMonitor : IDisposable
    {
        /// <summary>
        /// Log this class
        /// </summary>
        private static Logger _Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 
        /// </summary>
        public enum LanStatus { Unknown=0, Up=1, Down=2 }
        /// <summary>
        /// 
        /// </summary>
        public event Action<int> StatusChanged;
        public event Action<string> MessageError;
#if OLD
        /// <summary>
        /// 
        /// </summary>
        public LanStatus Lan1Status { get; set; }
        public LanStatus Lan2Status { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Lan1Device { get; set; }
        public string Lan2Device { get; set; }
#else
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

#endif

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lan1dev"></param>
        /// <param name="lan2dev"></param>
        public NICEventMonitor(string nicservice, bool fileInsteadLog = false, string logName = "System", string lan1dev = "Unkown", string lan2dev = "Unkown") 
        {
            try
            {
                NicService = nicservice;
                FileInsteadLog = fileInsteadLog;
                LogName = logName;
#if OLD
                Lan1Status = LanStatus.Unknown;
                Lan2Status = LanStatus.Unknown;
                Lan1Device = lan1dev;
                Lan2Device = lan2dev;
#endif
                UpEventId = 123;
                DownEventId = 83;
                LogEntriesGet();
                SearchForPhysicalDevices();
                watcher = new EventLogWatcher(new EventLogQuery(LogName, FileInsteadLog ? PathType.FilePath : PathType.LogName));
                watcher.EventRecordWritten += new EventHandler<EventRecordWrittenEventArgs>(watcher_EventRecordWritten);
            }
            catch (Exception x)
            {
                _Logger.Error(String.Format("NICEventMonitor-1 Exception {0}", x.Message), x);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="teaming"></param>
        public NICEventMonitor(string teaming)
        {
            try
            {
                if (teaming == "Marvell")
                {
                    NicService = "yukonw7";
                    FileInsteadLog = false;
                    LogName = "System";

                    UpEventId = 123;
                    DownEventId = 83;
#if OLD
                    Lan1Device = "UNKNOWN";
                    Lan2Device = "UNKNOWN";
                    Lan1Status = LanStatus.Unknown;
                    Lan2Status = LanStatus.Unknown;
#endif
                    LogEntriesGet();
                    SearchForPhysicalDevices();
                }
                else
                {
                    NicService = "UNKNOWN";
                    FileInsteadLog = false;
                    LogName = "System";

                    UpEventId = 123;
                    DownEventId = 83;
#if OLD
                    Lan1Device = "UNKNOWN";
                    Lan2Device = "UNKNOWN";
                    Lan1Status = LanStatus.Unknown;
                    Lan2Status = LanStatus.Unknown;
#endif
                    return;
                }
                watcher = new EventLogWatcher(new EventLogQuery(LogName, FileInsteadLog ? PathType.FilePath : PathType.LogName));
                watcher.EventRecordWritten += new EventHandler<EventRecordWrittenEventArgs>(watcher_EventRecordWritten);
            }
            catch (Exception x)
            {
                _Logger.Error(String.Format("NICEventMonitor-2 Exception {0}", x.Message), x);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            try
            {
                if (watcher.Enabled == false)
                {
                    InitialStatusGet();
                    watcher.Enabled = true;
                }
            }
            catch (Exception x)
            {
                _Logger.Error(String.Format("NICEventMonitor.Start Exception {0}", x.Message), x);
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
                if (watcher.Enabled == true)
                {
                    watcher.Enabled = false;
#if OLD
                    Lan1Status = Lan1Status == LanStatus.Unknown ? LanStatus.Unknown : LanStatus.Down;
                    Lan2Status = Lan2Status == LanStatus.Unknown ? LanStatus.Unknown : LanStatus.Down;
#else
                    NICList.Clear();
#endif
                }
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
#if OLD
                using (EventLog eventLog = new EventLog(LogName))
                {
                    eventLog.Source = "EventSimulate";

                    eventLog.WriteEntry(String.Format("Log message {0} example", lan == 0 ? Lan1Device : Lan2Device),
                        EventLogEntryType.Information,
                        status == false ? DownEventId : UpEventId, 1);

                    eventLog.Close();
                }
#else
                if (lan < NICList.Count)
                {
                    using (EventLog eventLog = new EventLog(LogName))
                    {
                        eventLog.Source = "EventSimulate";
                        eventLog.WriteEntry(String.Format("Log message {0} example", NICList[lan].DeviceId),
                            EventLogEntryType.Information,
                            status == false ? DownEventId : UpEventId, 1);

                        eventLog.Close();
                    }
                }
#endif
            }
            catch (Exception x)
            {
                _Logger.Error(String.Format("NICEventMonitor Simulate Exception {0}", x.Message), x);
                RaiseMessageError(x.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void watcher_EventRecordWritten(object sender, EventRecordWrittenEventArgs e)
        {
            if (e.EventRecord.Id == UpEventId)
            {
#if OLD
                if (e.EventRecord.ToXml().Contains(Lan1Device) == true)
                {
                    Lan1Status = LanStatus.Up;
                    RaiseStatusChanged(0);
                }
                else if (e.EventRecord.ToXml().Contains(Lan2Device) == true)
                {
                    Lan2Status = LanStatus.Up;
                    RaiseStatusChanged(1);
                }
                else
                {
                    RaiseMessageError(e.EventRecord.ToXml());
                }
#elif OLD_1
                for (int lan = 0; lan<NICList.Count; lan++)
                {
                    if (e.EventRecord.ToXml().Contains(NICList[lan].DeviceId) == true)
                    {
                        NICList[lan].Status = LanStatus.Up;
                        RaiseStatusChanged(lan);
                        break;
                    }
                }
#else
                string idLan = e.EventRecord.Properties[0].Value.ToString();
                NICItem lan = NICList.Where(nic => nic.DeviceId == idLan).FirstOrDefault();
                lan.Status = LanStatus.Up;
                RaiseStatusChanged(lan.Index);

#endif
                _Logger.Info("UpEventRecord: {0}", e.EventRecord.ToXml());
            }
            else if (e.EventRecord.Id == DownEventId)
            {
#if OLD
                if (e.EventRecord.ToXml().Contains(Lan1Device) == true)
                {
                    Lan1Status = LanStatus.Down;
                    RaiseStatusChanged(0);
                }
                else if (e.EventRecord.ToXml().Contains(Lan2Device) == true)
                {
                    Lan2Status = LanStatus.Down;
                    RaiseStatusChanged(0);
                }
                else
                {
                    RaiseMessageError(e.EventRecord.ToXml());
                }
#elif OLD_1
                for (int lan = 0; lan < NICList.Count; lan++)
                {
                    if (e.EventRecord.ToXml().Contains(NICList[lan].DeviceId) == true)
                    {
                        NICList[lan].Status = LanStatus.Down;
                        RaiseStatusChanged(lan);
                        break;
                    }
                }
#else
                string idLan = e.EventRecord.Properties[0].Value.ToString();
                NICItem lan = NICList.Where(nic => nic.DeviceId == idLan).FirstOrDefault();
                lan.Status = LanStatus.Down;
                RaiseStatusChanged(lan.Index);
#endif
                _Logger.Info("DownEventRecord: {0}", e.EventRecord.ToXml());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void ScanForDevices()
        {
            try
            {
                ManagementObjectSearcher mos = null;
                mos = new ManagementObjectSearcher(@"SELECT * FROM   Win32_NetworkAdapter WHERE  Manufacturer != 'Microsoft'");
                IList<ManagementObject> managementObjectList = mos.Get()
                                                                  .Cast<ManagementObject>()
                                                                  .OrderBy(p => Convert.ToUInt32(p.Properties["Index"].Value))
                                                                  .ToList();
                _Logger.Info("ScanForDevices....");
                foreach (ManagementObject mo in managementObjectList)
                {
                    Console.Clear();
                    _Logger.Info("Device {0}", mo.Path);
                    foreach (PropertyData pd in mo.Properties)
                    {
                        Console.WriteLine(pd.Name + ": " + (pd.Value ?? "N/A"));
                        _Logger.Info(pd.Name + ": " + (pd.Value ?? "N/A"));
                    }
                    _Logger.Info("", mo.Path);
                    Console.ReadKey(true);
                }
            }
            catch (Exception x)
            {
                _Logger.Error(String.Format("NICEventMonitor ScanForDevices Exception {0}", x.Message), x);
                RaiseMessageError(x.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string Info()
        {
            return String.Format("LogName: {0}, NicService: {1}, UpEventId: {2}, DownEventId: {3}", LogName, NicService, UpEventId, DownEventId);
        }

        /// <summary>
        /// 
        /// </summary>
        protected void SearchForPhysicalDevices()
        {
            try
            {
#if OLD_1
                ManagementObjectSearcher mos = null;
                mos = new ManagementObjectSearcher(@"SELECT * FROM   Win32_NetworkAdapter WHERE  Manufacturer != 'Microsoft'");
                IList<ManagementObject> managementObjectList = mos.Get()
                                                                  .Cast<ManagementObject>()
                                                                  .Where(p => (p.Properties["ServiceName"].Value != null && p.Properties["ServiceName"].Value.ToString().ToLower() == NicService.ToLower()))
                                                                  .OrderBy(p => Convert.ToUInt32(p.Properties["Index"].Value))
                                                                  .ToList();
                
                _Logger.Info("SearchForPhysicalDevices {0}", managementObjectList.Count);

                int nLan = 0;
                foreach (ManagementObject mo in managementObjectList)
                {
                    string LanDevice = mo.Properties["MACAddress"].Value.ToString().Replace(":","");
#if OLD
                    Lan1Device = nLan == 0 ? LanDevice : Lan1Device;
                    Lan1Status = nLan == 0 ? LanStatus.Down : Lan1Status;

                    Lan2Device = nLan == 1 ? LanDevice : Lan2Device;
                    Lan2Status = nLan == 1 ? LanStatus.Down : Lan2Status;
                    _Logger.Info("SearchForPhysicalDevices. Device {0}: {1}", nLan, LanDevice);
#else
                    NICList.Add(new NICItem() { DeviceId = LanDevice, Status = LanStatus.Down });
#endif
                    nLan++;
                }
#else
                int Index = 0;
                NICList.Clear();
                foreach (EventRecord evento in _LogEntries)
                {
                    if (evento.Properties.Count > 0)
                    {
                        string idDevice = evento.Properties[0].Value.ToString();
                        if (NICList.Count < 2 && NICList.Contains(new NICItem() { DeviceId = idDevice }) == false)
                        {
                            NICList.Add(new NICItem() { DeviceId = idDevice, Status = LanStatus.Down, Index = Index++ });
                        }
                    }
                }
                NICList = NICList.OrderBy(lan => lan.DeviceId).ToList();
#endif
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
#if OLD_1
                EventLogQuery logquery = new EventLogQuery(LogName, FileInsteadLog ? PathType.FilePath : PathType.LogName);

                List<EventRecord> Entries = new List<EventRecord>();
                EventLogReader elr = new EventLogReader(logquery);
                EventRecord entry;
                while ((entry = elr.ReadEvent()) != null)
                {
                    if (entry.Id == DownEventId || entry.Id == UpEventId)
                        Entries.Add(entry);
                }
#endif
#if OLD
                /** Separarlos */
                List<EventRecord> last_lan1_ev_down = Entries.Where(e => DownEventId == (e.Id) && e.ToXml().Contains(Lan1Device)).OrderByDescending(e => e.TimeCreated).ToList();
                List<EventRecord> last_lan2_ev_down = Entries.Where(e => DownEventId == (e.Id) && e.ToXml().Contains(Lan2Device)).OrderByDescending(e => e.TimeCreated).ToList();
                List<EventRecord> last_lan1_ev_up = Entries.Where(e => UpEventId == (e.Id) && e.ToXml().Contains(Lan1Device)).OrderByDescending(e => e.TimeCreated).ToList();
                List<EventRecord> last_lan2_ev_up = Entries.Where(e => UpEventId == (e.Id) && e.ToXml().Contains(Lan2Device)).OrderByDescending(e => e.TimeCreated).ToList();

                long last_lan1_down = last_lan1_ev_down.Count == 0 ? DateTime.MinValue.Ticks : last_lan1_ev_down[0].TimeCreated.Value.Ticks;
                long last_lan2_down = last_lan2_ev_down.Count == 0 ? DateTime.MinValue.Ticks : last_lan2_ev_down[0].TimeCreated.Value.Ticks;
                long last_lan1_up = last_lan1_ev_up.Count == 0 ? DateTime.MinValue.Ticks : last_lan1_ev_up[0].TimeCreated.Value.Ticks;
                long last_lan2_up = last_lan2_ev_up.Count == 0 ? DateTime.MinValue.Ticks : last_lan2_ev_up[0].TimeCreated.Value.Ticks;

                Lan1Status = last_lan1_up > last_lan1_down ? LanStatus.Up : LanStatus.Down;
                Lan2Status = last_lan2_up > last_lan2_down ? LanStatus.Up : LanStatus.Down; 
#elif OLD_1
                for (int lan = 0; lan < NICList.Count; lan++)
                {
                    string LanDevice = NICList[lan].DeviceId;
                    List<EventRecord> last_lan_ev_down = Entries.Where(e => DownEventId == (e.Id) && e.ToXml().Contains(LanDevice)).OrderByDescending(e => e.TimeCreated).ToList();
                    List<EventRecord> last_lan_ev_up = Entries.Where(e => UpEventId == (e.Id) && e.ToXml().Contains(LanDevice)).OrderByDescending(e => e.TimeCreated).ToList();

                    long last_lan_down = last_lan_ev_down.Count == 0 ? DateTime.MinValue.Ticks : last_lan_ev_down[0].TimeCreated.Value.Ticks;
                    long last_lan_up = last_lan_ev_up.Count == 0 ? DateTime.MinValue.Ticks : last_lan_ev_up[0].TimeCreated.Value.Ticks;

                    NICList[lan].Status = last_lan_up > last_lan_down ? LanStatus.Up : LanStatus.Down;
                }
#else
                for (int lan = 0; lan < NICList.Count; lan++)
                {
                    string LanDevice = NICList[lan].DeviceId;
                    List<EventRecord> last_lan_ev_down = _LogEntries.Where(e => DownEventId == (e.Id) && e.ToXml().Contains(LanDevice)).OrderByDescending(e => e.TimeCreated).ToList();
                    List<EventRecord> last_lan_ev_up = _LogEntries.Where(e => UpEventId == (e.Id) && e.ToXml().Contains(LanDevice)).OrderByDescending(e => e.TimeCreated).ToList();

                    long last_lan_down = last_lan_ev_down.Count == 0 ? DateTime.MinValue.Ticks : last_lan_ev_down[0].TimeCreated.Value.Ticks;
                    long last_lan_up = last_lan_ev_up.Count == 0 ? DateTime.MinValue.Ticks : last_lan_ev_up[0].TimeCreated.Value.Ticks;

                    NICList[lan].Status = last_lan_up > last_lan_down ? LanStatus.Up : LanStatus.Down;
                }
#endif
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
#if DEBUG1
            EventLogQuery logquery = new EventLogQuery("d:\\Datos\\Empresa\\_SharedPrj\\UlisesV5000i-MN\\Incidencias\\20161004. Misma MAC en ambos NIC Marvell\\eventos-basico.evtx", PathType.FilePath);
#else
            EventLogQuery logquery = new EventLogQuery(LogName, FileInsteadLog ? PathType.FilePath : PathType.LogName);
#endif

            EventLogReader elr = new EventLogReader(logquery);
            EventRecord entry;

            _LogEntries.Clear();
            while ((entry = elr.ReadEvent()) != null)
            {
                if (entry.ProviderName==NicService && ( entry.Id == DownEventId || entry.Id == UpEventId))
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
        /// <summary>
        /// 
        /// </summary>
        private EventLogWatcher watcher = null;
        private bool FileInsteadLog { get; set; }
        private string LogName { get; set; }
        private string NicService { get; set; }
        private int UpEventId { get; set; }
        private int DownEventId { get; set; }
        private List<EventRecord> _LogEntries = new List<EventRecord>();

    }
}
