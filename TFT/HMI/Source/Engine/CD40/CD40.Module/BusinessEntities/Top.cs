using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net.NetworkInformation;
using System.Timers;

using Microsoft.Practices.ObjectBuilder;

using HMI.Infrastructure.Interface;
using HMI.Model.Module.BusinessEntities;
using HMI.CD40.Module.Properties;
using HMI.CD40.Module.Services;
using HMI.CD40.Module.Snmp;

using U5ki.Infrastructure;
using Utilities;

using NLog;
using ProtoBuf;

namespace HMI.CD40.Module.BusinessEntities
{
    /// <summary>
    /// 
    /// </summary>
	static class Top
	{

        /// <summary>
        /// 
        /// </summary>
        public static bool ScreenSaverEnabled
        {
            get { return _ScreenSaverEnabled; }
            set { _ScreenSaverEnabled = value; }
        }
        
        /// <summary>
        /// 
        /// </summary>
		public static string HostId
		{
			get { return _HostId; }
		}

        /// <summary>
        /// 
        /// </summary>
		public static string SipIp
		{
			get { return _SipIp; }
		}

        /// <summary>
        /// 
        /// </summary>
		public static EventQueue WorkingThread
		{
			get { return _WorkingThread; }
		}

        /// <summary>
        /// 
        /// </summary>
		public static EventQueue PublisherThread
		{
			get { return _PublisherThread; }
		}

        /// <summary>
        /// 
        /// </summary>
		public static HwManager Hw
		{
			get { return _HwManager; }
		}

        /// <summary>
        /// 
        /// </summary>
		public static TopRegistry Registry
		{
			get { return _Registry; }
		}

        /// <summary>
        /// 
        /// </summary>
		public static CfgManager Cfg
		{
			get { return _CfgManager; }
		}

        /// <summary>
        /// 
        /// </summary>
		public static SipManager Sip
		{
			get { return _SipManager; }
		}

        /// <summary>
        /// 
        /// </summary>
		public static TlfManager Tlf
		{
			get { return _TlfManager; }
		}

        /// <summary>
        /// 
        /// </summary>
		public static LcManager Lc
		{
			get { return _LcManager; }
		}

        /// <summary>
        /// 
        /// </summary>
		public static RdManager Rd
		{
			get { return _RdManager; }
		}

        /// <summary>
        /// Gestor de las Mezclas de Audio en el puesto.
        /// </summary>
		public static MixerManager Mixer
		{
			get { return _MixerManager; }
		}

        /// <summary>
        /// Gestor de las grabaciones del puesto.
        /// </summary>
        public static RecorderManager Recorder
        {
            get { return _RecorderManager; }
        }

        /// <summary>
        /// 
        /// </summary>
        public static ReplayManager Replay
        {
            get { return _ReplayManager; }
        }

        /// <summary>
        /// 
        /// </summary>
        delegate void VoidDelegate();
        public static void Init()
		{
#if _NEWSTART_
            /** AGL.START Controlado */
            int Contador = 0;
            var NewList = new List<VoidDelegate>
            {
                delegate ()
                {
			        if (string.IsNullOrEmpty(_HostId) || string.IsNullOrEmpty(_SipIp))
			        {
				        List<string> ips = General.GetOperationalV4Ips();

				        if (string.IsNullOrEmpty(_SipIp))
				        {
					        _SipIp = (ips.Count > 0) ? ips[ips.Count - 1] : "127.0.0.1";
				        }

				        if (string.IsNullOrEmpty(_HostId))
				        {
					        _HostId = "PICT" + _SipIp.Substring(_SipIp.LastIndexOf('.') + 1);
				        }
			        }
                },
                delegate (){InitSnmp();},
                delegate (){_WorkingThread = new EventQueue();},
                delegate(){_PublisherThread = new EventQueue();},
                delegate ()
                {
#if _AUDIOGENERIC_
                    // switch (Properties.Settings.Default.TipoAudioUsb)
                    switch (HwManager.AudioDeviceType)
                    {
                        case eAudioDeviceTypes.MICRONAS:
                            _HwManager = new MicronasHwManager();
                            break;
                        case eAudioDeviceTypes.CMEDIA:
                            _HwManager = new HidCMediaHwManager();
                            break;
                        case eAudioDeviceTypes.GENERIC_PTT:     // Cascos y Altavoces USB...
                            _HwManager = new HidGenericHwManager(true);
                            break;
                        case eAudioDeviceTypes.GENERIC:         // Cascos sin PTT y Altavoces USB...
                            _HwManager = new HidGenericHwManager(false);
                            break;
                        case eAudioDeviceTypes.SIMUL:           // Simulacion de Tarjetas HID
                            _HwManager = new SimCMediaHwManager();
                            break;
                        default:
                            throw new Exception("Dispositivos de Audio no Conocidos...");
                    }
#else
                    _HwManager = new HwManager();
#endif
                },
                delegate(){_Registry = new TopRegistry();},
                delegate(){_CfgManager = new CfgManager();},
                delegate(){_SipManager = new SipManager();},
                delegate(){_MixerManager = new MixerManager();},
                delegate(){_TlfManager = new TlfManager();},
                delegate(){_LcManager = new LcManager();},
                delegate(){_RdManager = new RdManager();},
                delegate(){_RecorderManager = new RecorderManager(Settings.Default.GLP);},
                delegate(){_ReplayManager = new ReplayManager();}
            };
            var nNames = new List<string> 
            { "_SipIp & _HostId", "InitSnmp", "_WorkingThread", "_PublisherThread", "HwManager", 
                "Registry", "CfgManager", "SipManager", "MixedManager", "TlfManager", "LcManager", "RdManager", "RecorderManager", "ReplayManager" 
            };
            foreach (VoidDelegate _new in NewList)
            {
                try
                {
                    _new();
                    Contador++;
                }
                catch(Exception x)
                {
                    _Logger.Fatal("Error en Rutina {1} de Creacion TOP: \n{0}", x.Message, nNames[Contador]);
                }
            }
#else

            if (string.IsNullOrEmpty(_HostId) || string.IsNullOrEmpty(_SipIp))
            {
                List<string> ips = General.GetOperationalV4Ips();

                if (string.IsNullOrEmpty(_SipIp))
                {
                    _SipIp = (ips.Count > 0) ? ips[ips.Count - 1] : "127.0.0.1";
                }

                if (string.IsNullOrEmpty(_HostId))
                {
                    _HostId = "PICT" + _SipIp.Substring(_SipIp.LastIndexOf('.') + 1);
                }

            }

            /* Inicializa la gestion SNMP */
            InitSnmp();

            _WorkingThread = new EventQueue();
            _PublisherThread = new EventQueue();

#if _AUDIOGENERIC_
            /** AGL.CMEDIA */
            switch (Properties.Settings.Default.TipoAudioUsb)
            {
                case 0:     // IAU. Micronas.
                    _HwManager = new MicronasHwManger();
                    break;
                case 1:     // IAU. CMedia.
                    _HwManager = new HidCMediaHwManager();
                    break;
                case 2:     // Cascos y Altavoces USB...
                    _HwManager = new HidGenericHwManager(true);
                    break;
                case 3:     // Cascos sin PTT y Altavoces USB...
                    _HwManager = new HidGenericHwManager(true);
                    break;
                default:
                    throw new Exception("HwManager de tipo Desconocido...");
            }
#else
            _HwManager = new HwManager();
#endif
            /** */

            _Registry = new TopRegistry();
            _CfgManager = new CfgManager();
            _SipManager = new SipManager();
            _MixerManager = new MixerManager();
            _TlfManager = new TlfManager();
            _LcManager = new LcManager();
            _RdManager = new RdManager();

            _RecorderManager = new RecorderManager();
            _ReplayManager = new ReplayManager();
            /** */
#endif

#if _NEWSTART_
            /** AGL.START Controlado */
            Contador = 0;
            var InitList = new List<VoidDelegate> 
            { 
                delegate() {if (_HwManager!=null) {_HwManager.Init();} }, 
                delegate() {if (_Registry != null) _Registry.Init();}, 
                delegate() {if (_CfgManager != null) _CfgManager.Init();}, 
                delegate() {if (_SipManager != null) _SipManager.Init();},
                delegate() {if (_MixerManager != null) _MixerManager.Init();}, 
                delegate() {if (_TlfManager != null) _TlfManager.Init();}, 
                delegate() {if (_LcManager != null) _LcManager.Init();}, 
                delegate() {if (_RdManager != null) _RdManager.Init();}, 
                delegate() {if (_RecorderManager != null) _RecorderManager.Init();}
            };
            var iNames = new List<string> { "HwManager", "Registry", "CfgManager", "SipManager", "MixedManager", "TlfManager", "LcManager", "RdManager", "RecorderManager" };
            foreach (VoidDelegate init in InitList)
            {
                try
                {
                    init();
                    Contador++;
                }
                catch (Exception x)
                {
                    _Logger.Fatal("Error en Rutina {1} de Inicializacion TOP: \n{0}", x.Message, iNames[Contador]);
                }
            }
#else
            _HwManager.Init();
            _Registry.Init();
            _CfgManager.Init();
            _SipManager.Init();
            _MixerManager.Init(_HwManager.ListaDispositivos);
            _TlfManager.Init();
            _LcManager.Init();
            _RdManager.Init();

            _RecorderManager.Init();
#endif
#if _NICMON_V0_
            /** */        
            NetworkIFSupervisor.AutoReset = true;
            NetworkIFSupervisor.Elapsed += NetworkChange_NetworkAvailabilityChanged;
            NetworkIFSupervisor.Enabled = Settings.Default.SNMPEnabled == 1;
            _Logger.Info("TIMER NetworkIFSupervisor Arrancado...");
#else
            string jconfig = Properties.Settings.Default.LanTeamConfigs.Count > Properties.Settings.Default.LanTeamType ?
                Properties.Settings.Default.LanTeamConfigs[Properties.Settings.Default.LanTeamType] : "";
            mon = new NicEventMonitor(jconfig,
                    (lan, status) =>
                    {
                        string oid = lan == 0 ? Settings.Default.NetworkIF_1_Oid : Settings.Default.NetworkIF_2_Oid;
                        SnmpIntObject.Get(oid).Value = (int)status;

                        _Logger.Info(String.Format("Notificado cambio en LAN {0} => {1}",lan,status));
                    }, (m, x) =>
                    {
                        _Logger.Error(String.Format("Error Message: {0}",m));
                    }/*, filePath*/);
            _Logger.Info("NetworkIFSupervisor Arrancado...");
#endif

            /** 20170309. AGL. Supervision Cliente NTP. */
            NtpClientSupervisor.AutoReset = true;
            NtpClientSupervisor.Elapsed += NtpClientSupervisor_tick;
            NtpClientSupervisor.Enabled = Settings.Default.SNMPEnabled == 1;
            _Logger.Info("TIMER NtpClientSupervisor Arrancado...");
            /*****************/
        }

        /// <summary>
        /// 
        /// </summary>
        public static void Start()
		{
#if _NEWSTART_
            /** AGL.START Controlado */
            var StartList = new List<VoidDelegate> 
            { 
                SnmpAgent.Start, 
                delegate() { if (_WorkingThread != null) _WorkingThread.Start("working",Settings.Default.OverloadQueueWarning);}, 
                delegate() { if (_PublisherThread != null) _PublisherThread.Start("publisher", Settings.Default.OverloadQueueWarning);},
                delegate() { if (_RdManager != null) _RdManager.Start();}, 
                delegate() { if (_LcManager != null) _LcManager.Start();}, 
                delegate() { if (_TlfManager != null) _TlfManager.Start();}, 
                delegate() { if (_SipManager != null) _SipManager.Start();}, 
                delegate() { if (_RecorderManager != null) _RecorderManager.Start();}, 
                delegate() { if (_MixerManager != null) _MixerManager.Start();}, 
                delegate() { if (_CfgManager != null) _CfgManager.Start();},            
                delegate() { if (_Registry != null) _Registry.Start();}, 
                /** 20190107. Incluir en las versiones el componente CMEDIA */
                delegate() { if (_HwManager != null) {_HwManager.Start(); SetCurrentSwVersion();} }
            };

            int n = 0;
            _RecorderManager.LstDispositivos = _HwManager.ListaSndDev;
            foreach (VoidDelegate _start in StartList)
            {
                try
                {
                    _start();
                    n++;
                }
                catch (Exception x)
                {
                    _Logger.Fatal("Error en Rutina Arranque {0} en TOP: \n{1}",n, x.Message);
                }
            }
#else
            if (Settings.Default.SNMPEnabled == 1)
            {
                SnmpAgent.Start();

                SnmpIntObject.Get(Settings.Default.TopStOid).Value = 1;
                SnmpIntObject.Get(Settings.Default.TopOid).Value = 0;	// Tipo de elemento Hw: 0 => Top
            }

            _WorkingThread.Start();
            _PublisherThread.Start();

            _RdManager.Start();
            _LcManager.Start();
            _TlfManager.Start();
            //_MixerManager.Start();
            _SipManager.Start();
            _CfgManager.Start();
            _Registry.Start();
            _HwManager.Start();

            _RecorderManager.Start(_HwManager.ListaSndDev);
            _MixerManager.Start();
            /** */
#endif
        }

        /// <summary>
        /// 
        /// </summary>
		public static void End()
		{
			if (Settings.Default.SNMPEnabled == 1)
			{
				SnmpIntObject.Get(Settings.Default.TopStOid).Value = 0;
				SnmpIntObject.Get(Settings.Default.TopOid).Value = -1;	// Tipo de elemento Hw: -1 => Error
			}

			if (_WorkingThread != null)
			{
				_WorkingThread.Stop();
			}

			if (_PublisherThread != null)
			{
				_PublisherThread.Stop();
			}

            if (_MixerManager != null)
            {
                _MixerManager.End();
            }
            if (_RdManager != null)
			{
				_RdManager.End();
			}
			if (_LcManager != null)
			{
				_LcManager.End();
			}
			if (_TlfManager != null)
			{
				_TlfManager.End();
			}
			if (_SipManager != null)
			{
				_SipManager.End();
			}
			if (_CfgManager != null)
			{
				_CfgManager.End();
			}
			if (_Registry != null)
			{
				_Registry.End();
			}
            //if (_MixerManager != null)
            //{
            //    _MixerManager.End();
            //}
			if (_RecorderManager != null)
			{
				_RecorderManager.End();
			}
			if (_HwManager != null)
			{
				_HwManager.End();
			}

            if (Settings.Default.SNMPEnabled == 1)
            {
                SnmpAgent.Close();
            }

#if _NICMON_V0_
            /** 20170309. AGL. No se cerraban los TIMER. */
            NetworkIFSupervisor.Enabled = false;
            NetworkIFSupervisor.Elapsed -= NetworkChange_NetworkAvailabilityChanged;
#else
            if (mon != null)
            {
                mon.Dispose();
                mon = null;
            }
#endif
            NtpClientSupervisor.Enabled = false;
            NtpClientSupervisor.Elapsed -= NtpClientSupervisor_tick;
            /*****************/
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="status"></param>
		public static void SendTrapScreenSaver(bool status)
		{
			if (Settings.Default.SNMPEnabled == 1)
				SnmpIntObject.Get(Settings.Default.StandbyPanelOid).Value = status ? 0 : 1;
		}

		#region Private Members
        private static bool _ScreenSaverEnabled = false;
		private static Logger _Logger = LogManager.GetCurrentClassLogger();

		private static string _HostId = Settings.Default.IdHost;
		private static string _SipIp = Settings.Default.SipIp;

		private static EventQueue _WorkingThread;
		private static EventQueue _PublisherThread;
		private static HwManager _HwManager;
		private static TopRegistry _Registry;
		private static CfgManager _CfgManager;
		private static SipManager _SipManager;
		private static TlfManager _TlfManager;
		private static LcManager _LcManager;
		private static RdManager _RdManager;
		private static MixerManager _MixerManager;
		private static RecorderManager _RecorderManager;
        private static ReplayManager _ReplayManager;

#if _NICMON_V0_
        private static Timer NetworkIFSupervisor = new Timer(5000);
#else
        private static NicEventMonitor mon = null;
#endif
        /** 20170309. AGL. Supervision NTP Client */
        private static Timer NtpClientSupervisor = new Timer(5000);
        /*************/

        /// <summary>
        /// 
        /// </summary>
		private static void InitSnmp()
		{
			if (Settings.Default.SNMPEnabled == 1)
			{
				SnmpAgent.Init(_SipIp);

				System.Net.IPEndPoint maintenanceTrapEp = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(Settings.Default.MaintenanceTrapsIp), 162);

				SnmpAgent.Store.Add(new SnmpIntObject(Settings.Default.TopOid, -1, maintenanceTrapEp));
				SnmpAgent.Store.Add(new SnmpIntObject(Settings.Default.TopStOid, -1, maintenanceTrapEp));
				SnmpAgent.Store.Add(new SnmpIntObject(Settings.Default.MicrospeakerRdOid, -1, maintenanceTrapEp));
                SnmpAgent.Store.Add(new SnmpIntObject(Settings.Default.MicrospeakerLcOid, -1, maintenanceTrapEp));
                SnmpAgent.Store.Add(new SnmpIntObject(Settings.Default.MicrospeakerHfOid, -1, maintenanceTrapEp));
                SnmpAgent.Store.Add(new SnmpIntObject(Settings.Default.RecordCableOid, -1, maintenanceTrapEp));
                SnmpAgent.Store.Add(new SnmpIntObject(Settings.Default.AlumnJackOid, -1, maintenanceTrapEp));
				SnmpAgent.Store.Add(new SnmpIntObject(Settings.Default.InstructorJackOid, -1, maintenanceTrapEp));
				SnmpAgent.Store.Add(new SnmpIntObject(Settings.Default.StandbyPanelOid, -1, maintenanceTrapEp));
				SnmpAgent.Store.Add(new SnmpIntObject(Settings.Default.RadioPageOid, -1, maintenanceTrapEp));
                SnmpAgent.Store.Add(new SnmpStringObject(Settings.Default.StartingBriefingSessionOid, "", maintenanceTrapEp));
                SnmpAgent.Store.Add(new SnmpStringObject(Settings.Default.StartingReplayOid, "", maintenanceTrapEp));

				SnmpAgent.Store.Add(new SnmpStringObject(Settings.Default.EstablishedTfCallOid, "", maintenanceTrapEp));
				SnmpAgent.Store.Add(new SnmpStringObject(Settings.Default.IncommingTfCallOid, "", maintenanceTrapEp));
				SnmpAgent.Store.Add(new SnmpStringObject(Settings.Default.EndingTfCallOid, "", maintenanceTrapEp));
				SnmpAgent.Store.Add(new SnmpStringObject(Settings.Default.OutgoingTfCallOid, "", maintenanceTrapEp));
				SnmpAgent.Store.Add(new SnmpStringObject(Settings.Default.TlfFacilityOid, "", maintenanceTrapEp));
				SnmpAgent.Store.Add(new SnmpStringObject(Settings.Default.PttOid, "", maintenanceTrapEp));

                SnmpAgent.Store.Add(new SnmpIntObject(Settings.Default.NetworkIF_1_Oid, 0, maintenanceTrapEp));
                SnmpAgent.Store.Add(new SnmpIntObject(Settings.Default.NetworkIF_2_Oid, 0, maintenanceTrapEp));
                /** 20170309. AGL. Estado de Sincronismo NTP*/
                SnmpAgent.Store.Add(new SnmpStringObject(Settings.Default.NtpClientStatus_Oid, "ntp status", maintenanceTrapEp));
                /***********/
                /** 20170706. AGL. Control de configuracion sw en OID */
                SnmpAgent.Store.Add(new SnmpStringObject(Settings.Default.CurrentSwVersionOid, (new Utilities.VersionDetails("versiones.json")).ToString(), maintenanceTrapEp));
                /***********/
                System.Threading.Thread.Sleep(300);

				_Logger.Info("Iniciado agente SNMP. Puerto TRAP: {0}", maintenanceTrapEp.Port);
			}
		}

        /** 20180409.  Control Version en CMEDIA */
        static void SetCurrentSwVersion()
        {
            if (Settings.Default.SNMPEnabled == 1)
            {
                System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    System.Threading.Thread.Sleep(5000);

                    VersionDetails version = new VersionDetails("versiones.json");
                    /** Se añade el componente CMEDIA */
                    version.version.Components.Add(new VersionDetails.VersionDataComponent()
                    {
                        Name = "Hardware de Audio",
                        Files = new List<VersionDetails.VersionDataFileItem>()
                    {
                        new   VersionDetails.VersionDataFileItem() 
                        {
                            Path=_HwManager.HwManagerType + " #1",
                            Date=_HwManager.SwDate(0),
                            Size=_HwManager.SwVersion(0),
                            MD5=""
                        },
                        new   VersionDetails.VersionDataFileItem() 
                        {
                            Path=_HwManager.HwManagerType + " #2",
                            Date=_HwManager.SwDate(1),
                            Size=_HwManager.SwVersion(1),
                            MD5=""
                        },
                        new   VersionDetails.VersionDataFileItem() 
                        {
                            Path=_HwManager.HwManagerType + " #3",
                            Date=_HwManager.SwDate(2),
                            Size=_HwManager.SwVersion(2),
                            MD5=""
                        },
                        new   VersionDetails.VersionDataFileItem() 
                        {
                            Path=_HwManager.HwManagerType + " #4",
                            Date=_HwManager.SwDate(3),
                            Size=_HwManager.SwVersion(3),
                            MD5=""
                        }                 
                    }
                    });
                    /** Se actualiza el string de version */
                    SnmpStringObject.Get(Settings.Default.CurrentSwVersionOid).Value = version.ToString();
                });
            }
        }

#if _NICMON_V0_
        /** 20160908. */
        static NICEventMonitor monitor = null;
        static void NetworkChange_NetworkAvailabilityChanged(object sender, ElapsedEventArgs e)
        {
            if (monitor == null)
            {
                monitor = new NICEventMonitor("Marvell");
                //monitor.StatusChanged += MonitorStatusChanged;
                //monitor.MessageError += MonitorError;
                monitor.Start();
                return;
            }

            _Logger.Trace("NetworkChange_NetworkAvailabilityChanged Tick IN");
            
            NICEventMonitor.LanStatus lan1 = monitor.NICList.Count > 0 ? monitor.NICList[0].Status : NICEventMonitor.LanStatus.Unknown;
            NICEventMonitor.LanStatus lan2 = monitor.NICList.Count > 1 ? monitor.NICList[1].Status : NICEventMonitor.LanStatus.Unknown;

            SnmpIntObject.Get(Settings.Default.NetworkIF_1_Oid).Value = (int)lan1;
            SnmpIntObject.Get(Settings.Default.NetworkIF_2_Oid).Value = (int)lan2;

            _Logger.Trace("NetworkChange_NetworkAvailabilityChanged Tick OUT {0},{1}", (int)lan1, (int)lan2);
        }
#endif

        /** 20170309. AGL. Supervision Estado Ntp Client */
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void NtpClientSupervisor_tick(object sender, ElapsedEventArgs e)
        {
            NtpClientSupervisor.Enabled = false;
            try
            {
                _Logger.Trace("NtpClientSupervisor_tick IN");
                using (NtpClientStatus ntpc = new NtpClientStatus(NtpClientStatus.NtpClient.Windows))
                {
                    SnmpStringObject.Get(Settings.Default.NtpClientStatus_Oid).Value = String.Join("##", ntpc.Status.ToArray());
                    _Logger.Trace("NtpClientSupervisor_tick OUT {0}", String.Join("##", ntpc.Status.ToArray()));
                }
            }
            catch (Exception x)
            {
                _Logger.Error("NtpClientSupervisor_tick Exception: {0}", x.Message);
            }
            finally
            {
                NtpClientSupervisor.Interval = 60000;
                NtpClientSupervisor.Enabled = true;
            }
        }
        /*************/

        #endregion


        #region pruebas

        static void test1()
        {
            try
            {
                Dictionary<CORESIP_SndDevType, ISndDevIO> lista1 = new Dictionary<CORESIP_SndDevType, ISndDevIO>
                {
                    {CORESIP_SndDevType.CORESIP_SND_ALUMN_MHP, new HidCMediaSndDev()}, 
                    {CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_MHP, new HidCMediaSndDev()}
                };
                Dictionary<CORESIP_SndDevType, ISndDevIO> lista = lista1;
                foreach (ISndDevIO dev in lista.Values)
                {
                    dev.SenGrabacion(true);
                }
            }
            catch (Exception x)
            {
                _Logger.Error("Exception {0}", x.Message);
            }
        }

        #endregion
    }
}
