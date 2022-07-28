using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Net;
using System.Timers;

using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

using HMI.CD40.Module.Properties;
using HMI.Model.Module.Messages;
using HMI.CD40.Module.Snmp;

using U5ki.Infrastructure;
using Utilities;
using NLog;
//using Lextm.SharpSnmpLib;
//using Lextm.SharpSnmpLib.Messaging;

namespace HMI.CD40.Module.BusinessEntities
{
#if _AUDIOGENERIC_

    public enum eAudioDeviceTypes { NONE, MICRONAS, CMEDIA, GENERIC_PTT, GENERIC, SIMUL }

    /// <summary>
    /// Definicion de la Clase que Gestiona un dispositivo de audio en referencia a PTT, JACK y Volumen.
    /// </summary>
#if DEBUG
    public interface ISndDevIO
#else
    interface ISndDevIO
#endif
    {
        event GenericEventHandler<bool> JackConnected;
        event GenericEventHandler<bool> PttPulsed;

        /// <summary>
        /// Propiedades....
        /// </summary>
        CORESIP_SndDevType Type { get; set; }
        CORESIP_SndDevType SubType { get; set; }
        bool Jack { get; set; }
        bool Ptt { get; set; }
        bool IsValid { get; set; }

        /// <summary>
        /// Procedimientos...
        /// </summary>
        void Close();

        // int SetGpio(int gpio, byte estado);
        // byte GetGpio();
        //void LedAltavoz(bool OnOff);
        void SenGrabacion(bool OnOff);

        /** 20180409. Para obtener la version del FIRMWARE*/
        string SwVersion { get; }
        string SwDate { get; }
    }

    /// <summary>
    /// 
    /// </summary>
#if DEBUG
    public abstract class HwManager
#else
	abstract class HwManager
#endif     
	{
		// public const int OUT_GRABACION = 7;
        // public const int OUT_ALTAVOZ = 6;
        #region PUBLIC

        public const byte ON = 1;
		public const byte OFF = 0;
		public const byte OUT = 0;

        /// <summary>
        /// 
        /// </summary>        
        public event GenericEventHandler<JacksStateMsg> JacksChangedHw;
        public event GenericEventHandler<JacksStateMsg> SpeakerChangedHw;
        public event GenericEventHandler<JacksStateMsg> SpeakerExtChangedHw;
        public event GenericEventHandler<bool> PttPulsed;
		public event GenericEventHandler<SnmpIntMsg<string, int>> SetSnmpInt;

        /* AGL.REC. Para ver si el PTTHW está activado.. */
        private bool _HwPtt_Activated = false;
        public bool HwPtt_Activated
        {
            get { return _HwPtt_Activated; }
        }
        /* Fin Modificacion */

        /// <summary>
        /// 
        /// </summary>
		public bool InstructorJack
		{
			get { return _InstructorJack; }
		}

        /// <summary>
        /// 
        /// </summary>
		public bool AlumnJack
		{
			get { return _AlumnJack; }
		}

        public bool RdSpeaker
        {
            get { return _RadioSpeaker;  }
        }

        public bool LCSpeaker
        {
            get { return _LCSpeaker; }
        }

        public bool HfSpeaker
        {
            get { return _HFRadioSpeaker; }
        }
        /// <summary>
        /// 
        /// </summary>
		public Dictionary<CORESIP_SndDevType, ISndDevIO> ListaSndDev
		{
			get { return _SndDevs; }
		}

        /// <summary>
        /// 
        /// </summary>
		public List<CORESIP_SndDevType> ListaDispositivos
		{
			get { return _SOSndDvcs; }
		}

        /// <summary>
        /// 
        /// </summary>
		public HwManager()
		{
            HwSupervisor.AutoReset = false; // 20180626. #3609. Se anidan eventos y no se tratan bien en la cola. => true;
            HwSupervisor.Enabled = false;
			HwSupervisor.Elapsed += OnHwSupervisorTimerElapsed;
		}

        public virtual bool CheckDevs()
        {
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
		public virtual void Init()
		{
            /** 2016082. Supervision de Niveles de audio */
            /** 20170724. Se sustituye por una tarea externa */
            // levelsSupervisor.Init();
		}

        /// <summary>
        /// 
        /// </summary>
		public virtual void Start()
		{
		}

        /// <summary>
        /// 
        /// </summary>
		public void End()
		{
            HwSupervisor.Enabled = false;
			lock (_SndDevs)
			{
                ApagarSndSpeaker(); // Miguel

				foreach (ISndDevIO sndDev in _SndDevs.Values)
				{
					sndDev.Close();
				}
				
                _SndDevs.Clear();
				_SOSndDvcs.Clear();
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
		public ISndDevIO SndDev(CORESIP_SndDevType type)
		{
			ISndDevIO sndDev = null;
			CORESIP_SndDevType mainType = ((type == CORESIP_SndDevType.CORESIP_SND_LC_SPEAKER) ||
				(type == CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER)) ? CORESIP_SndDevType.CORESIP_SND_MAIN_SPEAKERS : type; 

			if (_SndDevs.TryGetValue(mainType, out sndDev))
			{
				sndDev.SubType = type;
			}

			return sndDev;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="ON"></param>
        public virtual void OnOffLed(CORESIP_SndDevType Type, byte ON) 
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void ApagarSndSpeaker() 
        {
        }

        /** 20180409. Para obtener la version del FIRMWARE*/
        CORESIP_SndDevType[] devTypes = new CORESIP_SndDevType[4] 
        { 
            CORESIP_SndDevType.CORESIP_SND_ALUMN_MHP, 
            CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_MHP, 
            CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER, 
            CORESIP_SndDevType.CORESIP_SND_LC_SPEAKER, 
        };
        public virtual string SwVersion(int devIndex)
        {
            if (devIndex < 4)
            {
                var devlist = from d in _SndDevs where d.Key == devTypes[devIndex] select d;
                if (devlist.Count() != 0)
                {
                    return devlist.First().Value.SwVersion;
                }
            }
            return String.Format("SwVersion Audio Dev {0} ???", devIndex);
        }
        public virtual string SwDate(int devIndex)
        {
            if (devIndex < 4)
            {
                var devlist = from d in _SndDevs where d.Key == devTypes[devIndex] select d;
                if (devlist.Count() != 0)
                {
                    return devlist.First().Value.SwDate;
                }
            }
            return String.Format("SwDate Audio Dev {0} ???", devIndex);
        }
        public string HwManagerType
        {
            get
            {
                return this.GetType().Name;
            }
        }
        /***************************************/

        /// <summary>
        /// 
        /// </summary>
        public static eAudioDeviceTypes AudioDeviceType
        {
            get
            {
                // 20180116. Nuevo orden de asignacion. 1 Simulador, 2 Las Estandar..., 3 Las propietarias...
                if (Properties.Settings.Default.AudioCardSimul == true)
                    return AudioDevice(eAudioDeviceTypes.SIMUL);

                if (Properties.Settings.Default.AudioCardStandard == true)
                {
                    // Testeo Generic
                    if ((new HidGenericHwManager(true)).Count > 0)
                        return AudioDevice(eAudioDeviceTypes.GENERIC_PTT);
                    if ((new HidGenericHwManager(false)).Count > 0)
                        return AudioDevice(eAudioDeviceTypes.GENERIC);

                    return AudioDevice(eAudioDeviceTypes.NONE);
                }

                // Testeo cmedia
                if ((new HidCMediaHwManager()).Count > 0)
                    return AudioDevice(eAudioDeviceTypes.CMEDIA);
                // Testeo Micronas...
                if ((new MicronasHwManager()).Count > 0)
                    return AudioDevice(eAudioDeviceTypes.MICRONAS);
//#if DEBUG
//                return AudioDevice(eAudioDeviceTypes.SIMUL);
//#else
//                // Testeo Generic
//                //if ((new HidGenericHwManager(true)).Count > 0)
//                //    return AudioDevice(eAudioDeviceTypes.GENERIC_PTT);

//                //if ((new HidGenericHwManager(false)).Count > 0)
//                //    return AudioDevice(eAudioDeviceTypes.GENERIC);

//                if (Properties.Settings.Default.AudioCardSimul == true)
//                    return eAudioDeviceTypes.SIMUL;

                return AudioDevice(eAudioDeviceTypes.NONE);
// #endif
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public static eAudioDeviceTypes AudioDevice(eAudioDeviceTypes tipo)
        {
            if (tipo == eAudioDeviceTypes.NONE)
                return LastAudioDevice;

            LastAudioDevice = tipo;

#if DEBUG
            tipo = LastAudioDevice;
#endif
            return tipo;
        }
        /// <summary>
        /// 
        /// </summary>
        //public const string LastDeviceKeyPath = @"SOFTWARE\Nucleodf";
        public const string LastDeviceKeyPath = @"SOFTWARE\Ulises";
        public const string LastDeviceKey = @"LastHidDevice";
        public static eAudioDeviceTypes LastAudioDevice
        {
            get
            {
                try
                {
                    Microsoft.Win32.RegistryKey pRegKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(LastDeviceKeyPath);
                    if (pRegKey != null)
                    {
                        Object regData = pRegKey.GetValue(LastDeviceKey);
                        if (regData != null)
                        {
                            return (eAudioDeviceTypes)regData;
                        }
                    }
                }
                catch (Exception x)
                {
                    _Logger.Error("Excepcion en GetLastAudioDevice: {0}", x.Message);
                }
                return eAudioDeviceTypes.NONE;
            }
            set
            {
                try
                {
                    Microsoft.Win32.RegistryKey pRegKey = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(LastDeviceKeyPath);
                    if (pRegKey != null)
                    {
                        pRegKey.SetValue(LastDeviceKey, (int)value);
                    }
                }
                catch (Exception x)
                {
                    _Logger.Error("Excepcion en SetLastAudioDevice: {0}", x.Message);
                }

            }
        }

        #endregion


        #region Protected Members

        /// <summary>
        /// 
        /// </summary>
        protected static Logger _Logger = LogManager.GetCurrentClassLogger();
		
        /// <summary>
        /// 
        /// </summary>
        protected Dictionary<CORESIP_SndDevType, ISndDevIO> _SndDevs = new Dictionary<CORESIP_SndDevType, ISndDevIO>();
        protected bool _InstructorJack = false;
        protected bool _AlumnJack = false;
        protected bool _RadioSpeaker = false;
        protected bool _LCSpeaker = false;
        protected int _NumDevices;

        /** 20160711. AGL */
        protected bool _HFRadioSpeaker = false;
        protected bool _RecordCable = false;
        /// <summary>
        /// 
        /// </summary>
        protected Timer HwSupervisor = new Timer(1000);

		/* Lista de dispositivos ordenada según los va suministrando el SO */
		protected List<CORESIP_SndDevType> _SOSndDvcs = new List<CORESIP_SndDevType>();

        /** 20160802. Supervision Niveles WinAudio */
        /** 20170724. Se sustituye por una tarea externa */
        // CMedia_MMAudioDeviceManager levelsSupervisor = new CMedia_MMAudioDeviceManager();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="connected"></param>
        protected void OnJackConnected(object sender, bool connected)
		{
#if DEBUG
            if (((ISndDevIO)sender).Type == CORESIP_SndDevType.CORESIP_SND_ALUMN_MHP)
            {
                _AlumnJack = connected || Settings.Default.JumpJacksState;
                if (Settings.Default.SNMPEnabled == 1)
                {
                    Top.WorkingThread.Enqueue("SetSnmp", delegate()
                    {
                        General.SafeLaunchEvent(SetSnmpInt, this, new SnmpIntMsg<string, int>(Settings.Default.AlumnJackOid, _AlumnJack ? 1 : 0));
                    });
                }
            }
            else if (((ISndDevIO)sender).Type == CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_MHP)
            {
                _InstructorJack = connected || Settings.Default.JumpJacksState;
                if (Settings.Default.SNMPEnabled == 1)
                {
                    Top.WorkingThread.Enqueue("SetSnmp", delegate()
                    {
                        General.SafeLaunchEvent(SetSnmpInt, this, new SnmpIntMsg<string, int>(Settings.Default.InstructorJackOid, _InstructorJack ? 1 : 0));
                    });
                }
            }

            Top.WorkingThread.Enqueue("JacksChanged", delegate()
            {
                _Logger.Debug("HwManager. OnJacksChanged {0}-{1}", _AlumnJack, _InstructorJack);
                General.SafeLaunchEvent(JacksChangedHw, this, new JacksStateMsg(_AlumnJack, _InstructorJack));
            });

#else
			ISndDevIO dev;
			bool instructorJack = false;
			bool alumnJack = false;

			lock (_SndDevs)
			{
				if (_SndDevs.TryGetValue(CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_MHP, out dev))
				{
					instructorJack = dev.Jack;
				}
				if (_SndDevs.TryGetValue(CORESIP_SndDevType.CORESIP_SND_ALUMN_MHP, out dev))
				{
					alumnJack = dev.Jack;
				}
			}

			if (Settings.Default.SNMPEnabled == 1)
			{
				Top.WorkingThread.Enqueue("SetSnmp", delegate()
				{
					General.SafeLaunchEvent(SetSnmpInt, this, new SnmpIntMsg<string, int>(Settings.Default.AlumnJackOid, alumnJack ? 1 : 0));
					General.SafeLaunchEvent(SetSnmpInt, this, new SnmpIntMsg<string, int>(Settings.Default.InstructorJackOid, instructorJack ? 1 : 0));
				});
			}

			Top.WorkingThread.Enqueue("JacksChanged", delegate()
			{
				_InstructorJack = instructorJack || Settings.Default.JumpJacksState;
				_AlumnJack = alumnJack || Settings.Default.JumpJacksState;

                _Logger.Debug("HwManager. OnJacksChanged {0}-{1}", _AlumnJack, _InstructorJack);

                General.SafeLaunchEvent(JacksChangedHw, this, new JacksStateMsg(_AlumnJack, _InstructorJack));
			});
#endif

		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="connected"></param>
        protected void OnSpkConnected(object sender, bool connected)
		{
			ISndDevIO dev;

			lock (_SndDevs)
			{
					if (((ISndDevIO)sender).Type == CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER &&
						(_SndDevs.TryGetValue(CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER, out dev)))
					{
                        _RadioSpeaker = dev.Jack;
                        if (Settings.Default.SNMPEnabled == 1)
                        {
                            Top.WorkingThread.Enqueue("SetSnmp", delegate()
					     	{
                                General.SafeLaunchEvent(SetSnmpInt, this, new SnmpIntMsg<string, int>(Settings.Default.MicrospeakerRdOid, _RadioSpeaker ? 1 : 0));
					    	});
                        }
					}

					if (((ISndDevIO)sender).Type == CORESIP_SndDevType.CORESIP_SND_LC_SPEAKER &&
                        (_SndDevs.TryGetValue(CORESIP_SndDevType.CORESIP_SND_LC_SPEAKER, out dev)))
					{
                        _LCSpeaker = dev.Jack;
                        if (Settings.Default.SNMPEnabled == 1)
                        {
				      		Top.WorkingThread.Enqueue("SetSnmp", delegate()
				    		{
                                General.SafeLaunchEvent(SetSnmpInt, this, new SnmpIntMsg<string, int>(Settings.Default.MicrospeakerLcOid, _LCSpeaker ? 1 : 0));
						    });
                        }
					}
                    Top.WorkingThread.Enqueue("SpeakerChanged", delegate()
                    {
                        _Logger.Trace("HwManager. OnSpkConnected {0} RdSpeaker {1} LCSpeaker {2}", connected, _RadioSpeaker, _LCSpeaker);
                        General.SafeLaunchEvent(SpeakerChangedHw, this, new JacksStateMsg(_RadioSpeaker, _LCSpeaker));
                    });
			}
		}

        protected void SetPresenceRdSpeaker(bool stateRadioSpeaker)
        {
            if (_RadioSpeaker != stateRadioSpeaker)
            {
                _RadioSpeaker = stateRadioSpeaker;
                Top.WorkingThread.Enqueue("SpeakerChanged", delegate ()
                {
                    _Logger.Trace("HwManager. OnSpkConnected RdSpeaker {0} ", _RadioSpeaker);
                    General.SafeLaunchEvent(SpeakerChangedHw, this, new JacksStateMsg(stateRadioSpeaker, _LCSpeaker));
                });
            }
        }

        // LALM 210420.1 presencia de altavoz LC
        // Peticiones #4810 Configurar la restricción de presencia de altavoz LC para operar en saliente y entrante
        //en Accesos Instantaneos
        protected void SetPresenceLcSpeaker(bool stateLcSpeaker)
        {
            if (_LCSpeaker != stateLcSpeaker)
            {
                _LCSpeaker = stateLcSpeaker;
                Top.WorkingThread.Enqueue("SpeakerLcChanged", delegate ()
                {
                    _Logger.Trace("HwManager. OnSpkConnected LcSpeaker {0} ", _LCSpeaker);
                    General.SafeLaunchEvent(SpeakerChangedHw, this, new JacksStateMsg(stateLcSpeaker, _LCSpeaker));
                });
            }
        }
        /// <summary>
        /// 20160712. AGL. Para capturar los eventos de presencia de HF y presencia de cable grabacion
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="connected"></param>
        protected void OnSpkConnectedExtended(object sender, bool connected)
        {
            ISndDevIO dev;

            lock (_SndDevs)
            {
                if (((ISndDevIO)sender).Type == CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER &&
                (_SndDevs.TryGetValue(CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER, out dev)))
                {
                    // Presencia de Altavoz HF.
                    _HFRadioSpeaker = dev.Ptt;
                    if (Settings.Default.SNMPEnabled == 1)
                        Top.WorkingThread.Enqueue("SetSnmp", delegate()
                        {
                            General.SafeLaunchEvent(SetSnmpInt, this, new SnmpIntMsg<string, int>(Settings.Default.MicrospeakerHfOid, _HFRadioSpeaker ? 1 : 0));
                        });
                }

                if (((ISndDevIO)sender).Type == CORESIP_SndDevType.CORESIP_SND_LC_SPEAKER &&
                    (_SndDevs.TryGetValue(CORESIP_SndDevType.CORESIP_SND_LC_SPEAKER, out dev)))
                {
                    // Presencia de Cable de Grabacion.
                    _RecordCable = dev.Ptt;
                    if (Settings.Default.SNMPEnabled == 1)
                        Top.WorkingThread.Enqueue("SetSnmp", delegate()
                        {
                            General.SafeLaunchEvent(SetSnmpInt, this, new SnmpIntMsg<string, int>(Settings.Default.RecordCableOid, _RecordCable ? 1 : 0));
                        });
                }
                Top.WorkingThread.Enqueue("SpeakerChanged", delegate()
                {
                    _Logger.Debug("HwManager. OnSpkConnectedExtended {0} HFSpeaker {1} RecCable {2}", connected, _HFRadioSpeaker, _RecordCable);
                    General.SafeLaunchEvent(SpeakerExtChangedHw, this, new JacksStateMsg(_HFRadioSpeaker, _RecordCable));
                });
            }
        }

        /// <summary>
        /// Eventos PTT de las HID.
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="pulsed"></param>
        protected void OnPttPulsed(object sender, bool pulsed)
		{
			PttSource src = ((ISndDevIO)sender).Type == CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_MHP ? PttSource.Instructor : PttSource.Alumn;

            /* AGL.REC Estado del PTT ??? */
            _HwPtt_Activated = pulsed;
            /**/
            
            Top.WorkingThread.Enqueue("PttPulsed", delegate()
			{
				General.SafeLaunchEvent(PttPulsed, src, pulsed);
			});


            // todo
			//((ISndDevIO)sender).SetGpio(OUT_GRABACION, pulsed ? ON : OFF);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void OnHwSupervisorTimerElapsed_old(object sender, ElapsedEventArgs e)
		{
			Top.WorkingThread.Enqueue("OnHwSupervisorTimerElapsed", delegate
			{
				lock (_SndDevs)
				{
                    /* AGL. Evitar una excepción en la gestión de la colección _SndDevs...*/
                    // * 
                    //foreach (SndDev sndDev in _SndDevs.Values)
                    //{
                    //    if (!sndDev.IsValid)
                    //        _SndDevs.Remove(sndDev.Type);
                    //}

                    //if (_SndDevs.Count != _NumDevices)
                    //{
                    //    End();
                    //    Init();
                    //    Start();
                    //}

                    /** */
//                    if (_SndDevs.Values.Count != 4)
                    if (CheckDevs()==false)
                    {
                        _Logger.Error("Reinicio HID Sonido...");
                        SubsystemReset();
                    }
                    else
                    {
                        foreach (ISndDevIO sndDev in _SndDevs.Values)
                        {
                            if (!sndDev.IsValid)
                            {
                                _Logger.Error("Reinicio HID Sonido por Error canal: {0}", sndDev.Type);
                                SubsystemReset();
                                break;
                            }
                        }
                    }
                    /*- Fin Modificacion -*/
                }
			});
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        enum eGlobalSubsystemStatus { INICIO, ERROR, OK}
        eGlobalSubsystemStatus globalSubsystemStatus = eGlobalSubsystemStatus.INICIO;
        private void OnHwSupervisorTimerElapsed(object sender, ElapsedEventArgs e)
        {
            /** 20180626. #3609. Se anidan eventos y no se tratan bien en la cola. */
            HwSupervisor.Enabled = false;
            //_Logger.Debug("OnHwSupervisorTimerElapsed. Disabling Timer");

            Top.WorkingThread.Enqueue("OnHwSupervisorTimerElapsed", delegate
            {
                /** 20180626. #3609. Se anidan eventos y no se tratan bien en la cola. */
                try
                {
                    int items_on_error = _SndDevs.Where(item => item.Value.IsValid == false).ToList().Count;
                    bool stdTick = CheckDevs() && (items_on_error == 0);

                    /** 20160802. Supervision de Niveles */
                    /** 20170724. Se sustituye por una tarea externa */
                    // levelsSupervisor.Tick();

                    switch (globalSubsystemStatus)
                    {
                        case eGlobalSubsystemStatus.INICIO:
                            if (stdTick == true)
                                globalSubsystemStatus = eGlobalSubsystemStatus.OK;
                            else
                            {
                                globalSubsystemStatus = eGlobalSubsystemStatus.ERROR;
                                SubsystemReset();
                            }
                            break;

                        case eGlobalSubsystemStatus.ERROR:
                            if (stdTick == true)
                                ApplicationReset();
                            else
                                SubsystemReset();
                            break;

                        case eGlobalSubsystemStatus.OK:
                            if (stdTick == false)
                                ApplicationReset();
                            break;
                    }
                }
                /** 20180626. #3609. Se anidan eventos y no se tratan bien en la cola. */
                catch (Exception x)
                {
                    _Logger.Error("OnHwSupervisorTimerElapsed Exception: {0}", x.Message);
                }
                finally
                {
                    HwSupervisor.Enabled = true;
                    //_Logger.Debug("OnHwSupervisorTimerElapsed. Enabling Timer");
                }
            });

        }

        /// <summary>
        /// 
        /// </summary>
        private void SubsystemReset()
        {
            _Logger.Error("HwManager. Subsystem RESET");

            /** */
            if (Settings.Default.JumpJacksState==false && (_InstructorJack == true || _AlumnJack == true))
                General.SafeLaunchEvent(JacksChangedHw, this, new JacksStateMsg(false, false));

            _InstructorJack = false;
            _AlumnJack = false;

            /* */
            if (_HwPtt_Activated == true)
            {
                General.SafeLaunchEvent(PttPulsed, PttSource.Instructor, false);
                General.SafeLaunchEvent(PttPulsed, PttSource.Alumn, false);
            }
            _HwPtt_Activated = false;

            /* */
            End();
            Init();
            Start();
        }

        /// <summary>
        /// 
        /// </summary>
        private void ApplicationReset()
        {
            _Logger.Error("HwManager. Application RESET");

            /** Para que no se aniden peticiones de RESET */
            HwSupervisor.AutoReset = false;
            HwSupervisor.Enabled = false;

            Process.Start("Launcher.exe", "HMI.exe");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sndDev"></param>
        protected void StartDevice(ISndDevIO sndDev)
        {
            _Logger.Debug("Encontrado HID tipo {0}", sndDev.Type);
            _SndDevs[sndDev.Type] = sndDev;

            if ((sndDev.Type == CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_MHP) ||
                (sndDev.Type == CORESIP_SndDevType.CORESIP_SND_ALUMN_MHP))
            {
                sndDev.JackConnected += OnJackConnected;
                sndDev.PttPulsed += OnPttPulsed;

                if (sndDev.Type == CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_MHP)
                    _InstructorJack = sndDev.Jack || Settings.Default.JumpJacksState;
                else
                    _AlumnJack = sndDev.Jack || Settings.Default.JumpJacksState;
                
                Top.WorkingThread.Enqueue("JacksChanged", delegate()
                {
                    General.SafeLaunchEvent(JacksChangedHw, this, new JacksStateMsg(_AlumnJack, _InstructorJack));
                });
            }

            if ((sndDev.Type == CORESIP_SndDevType.CORESIP_SND_LC_SPEAKER) ||
                (sndDev.Type == CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER))
            {
                sndDev.JackConnected += OnSpkConnected;
                /** 20160712 El PTT en estos dispositivos corresponden a la presencia de ALTAVOZ HF y CABLE DE GRABACION */
                sndDev.PttPulsed += OnSpkConnectedExtended;

                if (sndDev.Type == CORESIP_SndDevType.CORESIP_SND_LC_SPEAKER)
                {
                    _LCSpeaker = sndDev.Jack;
                    _RecordCable = sndDev.Ptt;
                }
                else
                {
                    _RadioSpeaker = sndDev.Jack;
                    _HFRadioSpeaker = sndDev.Ptt;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected void PostStart(bool bPttDevice)
        {
            if (bPttDevice)
            {
                _NumDevices = _SndDevs.Count > _NumDevices ? _SndDevs.Count : _NumDevices;

                ApagarSndSpeaker(); //Miguel
                if (Settings.Default.JumpJacksState &&
                    (SndDev(CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_MHP) == null) &&
                    (SndDev(CORESIP_SndDevType.CORESIP_SND_ALUMN_MHP) == null))
                {
                    Top.WorkingThread.Enqueue("JacksChanged", delegate()
                    {
                        _InstructorJack = true;
                        _AlumnJack = true;

                        General.SafeLaunchEvent(JacksChangedHw, this, new JacksStateMsg(true, true));
                    });
                }

                if (Settings.Default.SNMPEnabled == 1)
                {
                    Top.WorkingThread.Enqueue("SetSnmp", delegate()
                    {
                        General.SafeLaunchEvent(SetSnmpInt, this, new SnmpIntMsg<string, int>(Settings.Default.AlumnJackOid, _AlumnJack ? 1 : 0));
                        General.SafeLaunchEvent(SetSnmpInt, this, new SnmpIntMsg<string, int>(Settings.Default.MicrospeakerRdOid, _RadioSpeaker ? 1 : 0));
                        General.SafeLaunchEvent(SetSnmpInt, this, new SnmpIntMsg<string, int>(Settings.Default.MicrospeakerLcOid, _LCSpeaker ? 1 : 0));
                        General.SafeLaunchEvent(SetSnmpInt, this, new SnmpIntMsg<string, int>(Settings.Default.InstructorJackOid, _InstructorJack ? 1 : 0));
                        /** 20160711. AGL. Supervision HF-ALT y Cable Grabacion */
                        General.SafeLaunchEvent(SetSnmpInt, this, new SnmpIntMsg<string, int>(Settings.Default.MicrospeakerHfOid, _HFRadioSpeaker ? 1 : 0));
                        General.SafeLaunchEvent(SetSnmpInt, this, new SnmpIntMsg<string, int>(Settings.Default.RecordCableOid, _RecordCable ? 1 : 0));
                    });
                }
            }
            else
            {
                Top.WorkingThread.Enqueue("JacksChanged", delegate()
                {
                    _InstructorJack = false;
                    _AlumnJack = true;

                    General.SafeLaunchEvent(JacksChangedHw, this, new JacksStateMsg(true, false));
                });
            }

            /** 20180626. #3609. */
            //HwSupervisor.Enabled = true;
        }

		#endregion
	}

#else
	class SndDev
	{
    #region Uac2 Dll Interface
        /// <summary>
        /// 
        /// </summary>
		const int UAC2_GPI_REG = 0xB0A2;
		const int UAC2_GPO_REG = 0xB0A0;
		const int UAC2_PLAYBACK_VOLUME = 0x10;
		const int UAC2_PLAYBACK_BALANCE = 0x11;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uacHandle"></param>
        /// <param name="addr"></param>
        /// <param name="dataLen"></param>
        /// <param name="data"></param>
        /// <returns></returns>
		[DllImport("uac2", EntryPoint = "_UacSetMem@16", CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern int UacSetMem(IntPtr uacHandle, int addr, int dataLen, [In] byte[] data);
		[DllImport("uac2", EntryPoint = "_UacGetMem@16", CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern int UacGetMem(IntPtr uacHandle, int addr, int dataLen, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] data);
		[DllImport("uac2", EntryPoint = "_UacGetXDFP@12", CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern int UacGetXDFP(IntPtr uacHandle, int addr, out int data);
		[DllImport("uac2", EntryPoint = "_UacSetXDFP@12", CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern int UacSetXDFP(IntPtr uacHandle, int addr, int data);

    #endregion

        /// <summary>
        /// 
        /// </summary>
		public event GenericEventHandler<bool> JackConnected;
		public event GenericEventHandler<bool> PttPulsed;

        /// <summary>
        /// 
        /// </summary>
		public CORESIP_SndDevType Type
		{
			get { return _Type; }
		}

        /// <summary>
        /// 
        /// </summary>
		public CORESIP_SndDevType SubType
		{
			get { return _SubType; }
			set { _SubType = value; }
		}

        /// <summary>
        /// 
        /// </summary>
		public bool Jack
		{
			get { return _Jack; }
			private set
			{
				if (value != _Jack)
				{
					_Jack = value;
					General.SafeLaunchEvent(JackConnected, this, _Jack);
				}
			}
		}

        /// <summary>
        /// 
        /// </summary>
		public bool Ptt
		{
			get { return _Ptt; }
			private set
			{
				if (value != _Ptt)
				{
					_Ptt = value;
					General.SafeLaunchEvent(PttPulsed, this, _Ptt);
				}
			}
		}

        /// <summary>
        /// 
        /// </summary>
		public bool IsValid
		{
			get { return ((_Type != CORESIP_SndDevType.CORESIP_SND_UNKNOWN) && !_DevHandle.IsClosed); }
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="usbHandle"></param>
        /// <param name="hidHandle"></param>
        /// <param name="devPath"></param>
		public SndDev(IntPtr usbHandle, IntPtr hidHandle, string devPath)
		{
			lock (_Sync)
			{
				IntPtr devFileHandle = Native.Kernel32.CreateFile(devPath, Native.Kernel32.GENERIC_READ | Native.Kernel32.GENERIC_WRITE,
					Native.Kernel32.FILE_SHARE_READ | Native.Kernel32.FILE_SHARE_WRITE, IntPtr.Zero, Native.Kernel32.OPEN_EXISTING,
					Native.Kernel32.FILE_FLAG_OVERLAPPED, IntPtr.Zero);

				_UsbHandle = usbHandle;
				_HidHandle = hidHandle;
				_DevHandle = new SafeFileHandle(devFileHandle, true);

				if (!_DevHandle.IsInvalid)
				{
					_Stream = new FileStream(_DevHandle, FileAccess.Read, 3, true);
					_Buffer = new byte[3];

					_Stream.BeginRead(_Buffer, 0, 3, new AsyncCallback(ReadCompleted), null);
				}

				byte[] data = new byte[1];
				if (UacGetMem(_UsbHandle, UAC2_GPI_REG, 1, data) == 1)
				{
					_Type = (CORESIP_SndDevType)((data[0] >> 3) & 0x7);

					Jack = ((data[0] & JACK_FLAG) != 0);
					Ptt = _Jack && ((data[0] & PTT_FLAG) != 0);

					//PublishState();
				}
			}
		}

        /// <summary>
        /// 
        /// </summary>
		public void PublishState()
		{
			lock (_Sync)
			{
				General.SafeLaunchEvent(JackConnected, this, _Jack);
				General.SafeLaunchEvent(PttPulsed, this, _Ptt);
			}
		}

        /// <summary>
        /// 
        /// </summary>
		public void Close()
		{
			_DevHandle.Close();
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
		public bool SetOutVolume(int level)
		{
			Debug.Assert((level >= 0) && (level <= 7));

			int volume = 0x79 - ((7 - level) * 3);

			if (_Type == CORESIP_SndDevType.CORESIP_SND_MAIN_SPEAKERS)
			{
				int actualVolume, actualBalance;

				if ((UacGetXDFP(_UsbHandle, UAC2_PLAYBACK_VOLUME, out actualVolume) == 4) &&
					(UacGetXDFP(_UsbHandle, UAC2_PLAYBACK_BALANCE, out actualBalance) == 4))
				{
					actualVolume >>= 10;
					actualBalance >>= 10;

					sbyte balance = (sbyte)actualBalance;
					int leftVolume = (balance > 0) ? actualVolume - balance : actualVolume;
					int rightVolume = (balance < 0) ? actualVolume + balance : actualVolume;

					int channel = (_SubType == CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER ?
						Settings.Default.RdSpeakerOutChannel : Settings.Default.LcSpeakerOutChannel) % 2;

					int desiredLeftVolume = channel == 0 ? volume : leftVolume;
					int desiredRightVolume = channel == 1 ? volume : rightVolume;

					int newVolume = Math.Max(desiredLeftVolume, desiredRightVolume);
					int newBalance = (newVolume > desiredLeftVolume ? newVolume - desiredLeftVolume : desiredRightVolume - newVolume);

					newVolume = Math.Max(0, newVolume);
					newVolume = Math.Min(0x79, newVolume);
					newBalance = Math.Max(-21, newBalance);
					newBalance = Math.Min(21, newBalance);

					if ((UacSetXDFP(_UsbHandle, UAC2_PLAYBACK_VOLUME, newVolume << 10) == 4) &&
						(UacSetXDFP(_UsbHandle, UAC2_PLAYBACK_BALANCE, ((byte)newBalance) << 10) == 4))
					{
						return true;
					}
				}
			}
			else
			{
				if (UacSetXDFP(_UsbHandle, UAC2_PLAYBACK_VOLUME, volume << 10) == 4)
				{
					return true;
				}
			}

			return false;
		}

        /// <summary>
        /// Mover Líneas en el USB
        /// </summary>
        /// <param name="gpio"></param>
        /// <param name="estado"></param>
        /// <returns></returns>
		public int SetGpio(int gpio, byte estado)
		{
			byte mask = gpio == 6 ? (byte)0x40 : (byte)0x80;
			byte[] datos = new byte[1];

			// Recuperar estado actual
			datos[0] = GetGpio();

			datos[0] = (estado == 0 ? (byte)(datos[0] & ((byte)(~mask))) : (byte)(datos[0] | mask));
			return UacSetMem(_UsbHandle, UAC2_GPO_REG, 1, datos);
		}

        /// <summary>
        /// Leer Líneas en el USB.
        /// </summary>
        /// <returns></returns>
		public byte GetGpio()
		{
			byte[] datos = new byte[1];
			if (UacGetMem(_UsbHandle, UAC2_GPI_REG, 1, datos) == 1)
			{
				_gpio6 = (datos[0] & 0x40) == 0x40;
				_gpio7 = (datos[0] & 0x80) == 0x80;

				return datos[0];
			}

			return 0;
		}

    #region Private Members
        /// <summary>
        /// 
        /// </summary>
		private const int JACK_FLAG = 0x01;
		private const int AUC_FLAG = 0x02;
		private const int PTT_FLAG = 0x04;
        /// <summary>
        /// 
        /// </summary>
		private static Logger _Logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// 
        /// </summary>
		private IntPtr _UsbHandle;
		private IntPtr _HidHandle;
		private SafeFileHandle _DevHandle;
		private CORESIP_SndDevType _Type = CORESIP_SndDevType.CORESIP_SND_UNKNOWN;
		private CORESIP_SndDevType _SubType = CORESIP_SndDevType.CORESIP_SND_UNKNOWN;
		private FileStream _Stream;
		private byte[] _Buffer;
		private object _Sync = new object();
		private bool _Jack = false;
		private bool _Ptt = false;
		private bool _gpio6, _gpio7;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="iResult"></param>
		private void ReadCompleted(IAsyncResult iResult)
		{
			byte[] arrBuff = (byte[])iResult.AsyncState;	// retrieve the read buffer
			try
			{
				_Stream.EndRead(iResult);
				_Logger.Debug("Recibido cambio de estado en dispositivo {0}: 0x{1:X}", _Type, _Buffer[1]);

				lock (_Sync)
				{
					Jack = ((_Buffer[1] & JACK_FLAG) == 0);
					Ptt = _Jack && ((_Buffer[1] & PTT_FLAG) == 0);

					//PublishState();
				}

				_Stream.BeginRead(_Buffer, 0, 3, new AsyncCallback(ReadCompleted), null);
			}
			catch (IOException )
			{
				if (!_DevHandle.IsClosed)
				{
					Jack = false;
					Ptt = false;

					_DevHandle.Close();
				}
			}
			catch (Exception)
			{
				if (!_DevHandle.IsClosed)
				{
					Jack = false;
					Ptt = false;

					_DevHandle.Close();
				}
			}
		}

    #endregion
	}

    /// <summary>
    /// 
    /// </summary>
	class HwManager
	{
		public const int OUT_GRABACION = 7;
		public const int OUT_ALTAVOZ = 6;

		public const byte ON = 1;
		public const byte OFF = 0;
		public const byte OUT = 0;

    #region Uac2 Dll Interface
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uFlags"></param>
        /// <returns></returns>
		[DllImport("uac2", EntryPoint = "_UacBuildDeviceList@4", CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern int UacBuildDeviceList(uint uFlags);
		[DllImport("uac2", EntryPoint = "_UacGetFirstDevice@0", CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern IntPtr UacGetFirstDevice();
		[DllImport("uac2", EntryPoint = "_UacGetNextDevice@4", CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern IntPtr UacGetNextDevice(IntPtr uacHandle);
		[DllImport("uac2", EntryPoint = "_UacGetHidDevice@4", CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern IntPtr UacGetHidDevice(IntPtr uacHandle);
		[DllImport("uac2", EntryPoint = "_UacGetInstanceID_A@12", CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern int UacGetInstanceID(IntPtr uacHandle, StringBuilder id, int size);
		[DllImport("uac2", EntryPoint = "_UacGetHardwareID_A@12", CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern int UacGetHardwareID(IntPtr uacHandle, StringBuilder id, int size);
		[DllImport("uac2", EntryPoint = "_UacGetDirectShowDeviceName_A@12", CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern int UacGetDirectShowDeviceName(IntPtr uacHandle, StringBuilder id, int size);
		
    #endregion

    #region Hid Dll Interface
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hidGuid"></param>
		[DllImport("hid.dll", SetLastError = true)]
		static extern void HidD_GetHidGuid(ref Guid hidGuid);

    #endregion

        /// <summary>
        /// 
        /// </summary>
        public event GenericEventHandler<JacksStateMsg> JacksChanged;
        public event GenericEventHandler<JacksStateMsg> SpeakerChanged;
        public event GenericEventHandler<bool> PttPulsed;
		public event GenericEventHandler<SnmpIntMsg<string, int>> SetSnmpInt;

        /* AGL.REC. Para ver si el PTTHW está activado.. */
        private bool _HwPtt_Activated = false;
        public bool HwPtt_Activated
        {
            get { return _HwPtt_Activated; }
        }
        /* Fin Modificacion */

        /// <summary>
        /// 
        /// </summary>
		public bool InstructorJack
		{
			get { return _InstructorJack; }
		}

        /// <summary>
        /// 
        /// </summary>
		public bool AlumnJack
		{
			get { return _AlumnJack; }
		}

        /// <summary>
        /// 
        /// </summary>
		public Dictionary<CORESIP_SndDevType, SndDev> ListaSndDev
		{
			get { return _SndDevs; }
		}

        /// <summary>
        /// 
        /// </summary>
		public List<CORESIP_SndDevType> ListaDispositivos
		{
			get { return _SOSndDvcs; }
		}

        /// <summary>
        /// 
        /// </summary>
		public HwManager()
		{
			HwSupervisor.AutoReset = true;
			HwSupervisor.Elapsed += OnHwSupervisorTimerElapsed;
		}

        /// <summary>
        /// 
        /// </summary>
		public void Init()
		{
			/* Lista de dispositivos ordenada según los va suministrando el SO */
			
			Guid hidGuid = new Guid();
			HidD_GetHidGuid(ref hidGuid);

			UacBuildDeviceList(0);

			for (IntPtr usbHandle = UacGetFirstDevice(); usbHandle != IntPtr.Zero; usbHandle = UacGetNextDevice(usbHandle))
			{
				IntPtr hidHandle = UacGetHidDevice(usbHandle);
				if (hidHandle != IntPtr.Zero)
				{
					/*********************************************/
					//StringBuilder name = new StringBuilder(512);
					//UacGetHardwareID(hidHandle, name, 512);

					//UacGetDirectShowDeviceName(hidHandle, name, 512);
					/*********************************************/

					
					StringBuilder id = new StringBuilder(512);

					if (UacGetInstanceID(hidHandle, id, 512) != 0)
					{
						id.Replace('\\', '#');

						string devPath = string.Format("\\\\?\\{0}#{{{1}}}", id.ToString().ToLower(), hidGuid);
						SndDev sndDev = new SndDev(usbHandle, hidHandle, devPath);

						if (sndDev.IsValid)
						{
							_SOSndDvcs.Add(sndDev.Type);
						}
					}
				}
			}
		}

        /// <summary>
        /// 
        /// </summary>
		public void Start()
		{
			Guid hidGuid = new Guid();
			HidD_GetHidGuid(ref hidGuid);

			UacBuildDeviceList(0);

			HwSupervisor.Enabled = true;

			for (IntPtr usbHandle = UacGetFirstDevice(); usbHandle != IntPtr.Zero; usbHandle = UacGetNextDevice(usbHandle))
			{
				IntPtr hidHandle = UacGetHidDevice(usbHandle);
				if (hidHandle != IntPtr.Zero)
				{
					StringBuilder id = new StringBuilder(512);

					if (UacGetInstanceID(hidHandle, id, 512) != 0)
					{
						id.Replace('\\', '#');

						string devPath = string.Format("\\\\?\\{0}#{{{1}}}", id.ToString().ToLower(), hidGuid);
						SndDev sndDev = new SndDev(usbHandle, hidHandle, devPath);

						if (sndDev.IsValid)
						{
							_Logger.Info("Encontrado dispositivo de sonido tipo {0}", sndDev.Type);
							_SndDevs[sndDev.Type] = sndDev;
							
							if ((sndDev.Type == CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_MHP) ||
								(sndDev.Type == CORESIP_SndDevType.CORESIP_SND_ALUMN_MHP))
							{
								sndDev.JackConnected += OnJackConnected;
								sndDev.PttPulsed += OnPttPulsed;

								if (sndDev.Type == CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_MHP)
									_InstructorJack = sndDev.Jack;
								else
									_AlumnJack = sndDev.Jack;

								Top.WorkingThread.Enqueue("JacksChanged", delegate()
									{
										General.SafeLaunchEvent(JacksChanged, this, new JacksStateMsg(_AlumnJack, _InstructorJack));
									});
							}

							if ((sndDev.Type == CORESIP_SndDevType.CORESIP_SND_LC_SPEAKER) ||
								(sndDev.Type == CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER) ||
                                (sndDev.Type == CORESIP_SndDevType.CORESIP_SND_HF_SPEAKER) )
							{
								sndDev.JackConnected += OnSpkConnected;
                                if (sndDev.Type == CORESIP_SndDevType.CORESIP_SND_LC_SPEAKER)
                                    _LCSpeaker = sndDev.Jack;
                                else if (sndDev.Type == CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER)
                                    _RadioSpeaker = sndDev.Jack;
                                else
                                    _HFSpeaker = sndDev.Jack;
							}
						}
					}
				}
			}

			_NumDevices = _SndDevs.Count > _NumDevices ? _SndDevs.Count : _NumDevices;

            ApagarSndSpeaker(); //Miguel
			if (Settings.Default.JumpJacksState && 
				(SndDev(CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_MHP) == null) &&
				(SndDev(CORESIP_SndDevType.CORESIP_SND_ALUMN_MHP) == null))
			{
				Top.WorkingThread.Enqueue("JacksChanged", delegate()
				{
					_InstructorJack = true;
					_AlumnJack = true;

					General.SafeLaunchEvent(JacksChanged, this, new JacksStateMsg(true, true));
				});
			}

			if (Settings.Default.SNMPEnabled == 1)
			{
				Top.WorkingThread.Enqueue("SetSnmp", delegate()
				{
					General.SafeLaunchEvent(SetSnmpInt, this, new SnmpIntMsg<string, int>(Settings.Default.AlumnJackOid, _AlumnJack ? 1 : 0));
					General.SafeLaunchEvent(SetSnmpInt, this, new SnmpIntMsg<string, int>(Settings.Default.MicrospeakerRdOid, _RadioSpeaker ? 1 : 0));
                    General.SafeLaunchEvent(SetSnmpInt, this, new SnmpIntMsg<string, int>(Settings.Default.MicrospeakerLcOid, _LCSpeaker ? 1 : 0));
                    General.SafeLaunchEvent(SetSnmpInt, this, new SnmpIntMsg<string, int>(Settings.Default.MicrospeakerHfOid, _HFSpeaker ? 1 : 0));
                    General.SafeLaunchEvent(SetSnmpInt, this, new SnmpIntMsg<string, int>(Settings.Default.InstructorJackOid, _InstructorJack ? 1 : 0));
				});
			}
		}

        /// <summary>
        /// 
        /// </summary>
		public void End()
		{
			lock (_SndDevs)
			{
                ApagarSndSpeaker(); //Miguel

				foreach (SndDev sndDev in _SndDevs.Values)
				{
					sndDev.Close();
				}
				
                _SndDevs.Clear();
				_SOSndDvcs.Clear();
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
		public SndDev SndDev(CORESIP_SndDevType type)
		{
			SndDev sndDev = null;
			CORESIP_SndDevType mainType = ((type == CORESIP_SndDevType.CORESIP_SND_LC_SPEAKER) ||
				(type == CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER)) ? CORESIP_SndDevType.CORESIP_SND_MAIN_SPEAKERS : type;

			if (_SndDevs.TryGetValue(mainType, out sndDev))
			{
				sndDev.SubType = type;
			}

			return sndDev;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="ON"></param>
        public void OnOffLed(CORESIP_SndDevType Type, byte ON)//Miguel
        {
            SndDev sndDev = null;
            if (_SndDevs.TryGetValue(Type, out sndDev))
                sndDev.SetGpio(HwManager.OUT_ALTAVOZ,  ON);       
        }

    #region Private Members
        /// <summary>
        /// 
        /// </summary>
        private void ApagarSndSpeaker() //Miguel
        {                       
            foreach (SndDev sndDev in _SndDevs.Values)
            {
                if (sndDev.Type == CORESIP_SndDevType.CORESIP_SND_LC_SPEAKER)                            
                    sndDev.SetGpio(HwManager.OUT_ALTAVOZ, OFF);
                else if (sndDev.Type == CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER)                            
                    sndDev.SetGpio(HwManager.OUT_ALTAVOZ, OFF);                              
            }        
        
        }

        /// <summary>
        /// 
        /// </summary>
		private static Logger _Logger = LogManager.GetCurrentClassLogger();
		
        /// <summary>
        /// 
        /// </summary>
		private Dictionary<CORESIP_SndDevType, SndDev> _SndDevs = new Dictionary<CORESIP_SndDevType, SndDev>();
		private bool _InstructorJack = false;
		private bool _AlumnJack = false;
		private bool _RadioSpeaker = false;
		private bool _LCSpeaker = false;
        private bool _HFSpeaker = false;
		private int _NumDevices;

        /// <summary>
        /// 
        /// </summary>
		private Timer HwSupervisor = new Timer(1000);

		/* Lista de dispositivos ordenada según los va suministrando el SO */
		private List<CORESIP_SndDevType> _SOSndDvcs = new List<CORESIP_SndDevType>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="connected"></param>
		private void OnJackConnected(object sender, bool connected)
		{
			SndDev dev;
			bool instructorJack = false;
			bool alumnJack = false;

			lock (_SndDevs)
			{
				if (_SndDevs.TryGetValue(CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_MHP, out dev))
				{
					instructorJack = dev.Jack;
				}
				if (_SndDevs.TryGetValue(CORESIP_SndDevType.CORESIP_SND_ALUMN_MHP, out dev))
				{
					alumnJack = dev.Jack;
				}
			}

			if (Settings.Default.SNMPEnabled == 1)
			{
				Top.WorkingThread.Enqueue("SetSnmp", delegate()
				{
					General.SafeLaunchEvent(SetSnmpInt, this, new SnmpIntMsg<string, int>(Settings.Default.AlumnJackOid, alumnJack ? 1 : 0));
					General.SafeLaunchEvent(SetSnmpInt, this, new SnmpIntMsg<string, int>(Settings.Default.InstructorJackOid, instructorJack ? 1 : 0));
				});
			}

			Top.WorkingThread.Enqueue("JacksChanged", delegate()
			{
				_InstructorJack = instructorJack || Settings.Default.JumpJacksState;
				_AlumnJack = alumnJack || Settings.Default.JumpJacksState;

				General.SafeLaunchEvent(JacksChanged, this, new JacksStateMsg(_AlumnJack, _InstructorJack));
			});
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="connected"></param>
		private void OnSpkConnected(object sender, bool connected)
		{
			SndDev dev;
			bool radioSpk = false;
			bool lcSpk = false;
            bool hfSpk = false;

			lock (_SndDevs)
			{
                Top.WorkingThread.Enqueue("SpeakerChanged", delegate()
                {
                    General.SafeLaunchEvent(SpeakerChanged, this, new JacksStateMsg(radioSpk, lcSpk));
                });

                if (Settings.Default.SNMPEnabled == 1)
				{
					if (((SndDev)sender).Type == CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER &&
						(_SndDevs.TryGetValue(CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER, out dev)))
					{
						Top.WorkingThread.Enqueue("SetSnmp", delegate()
						{
                            _RadioSpeaker = radioSpk = dev.Jack;
							General.SafeLaunchEvent(SetSnmpInt, this, new SnmpIntMsg<string, int>(Settings.Default.MicrospeakerRdOid, radioSpk ? 1 : 0));
						});
					}

					if (((SndDev)sender).Type == CORESIP_SndDevType.CORESIP_SND_LC_SPEAKER &&
						(_SndDevs.TryGetValue(CORESIP_SndDevType.CORESIP_SND_LC_SPEAKER, out dev)))
					{
						Top.WorkingThread.Enqueue("SetSnmp", delegate()
						{
							_LCSpeaker = lcSpk = dev.Jack;
							General.SafeLaunchEvent(SetSnmpInt, this, new SnmpIntMsg<string, int>(Settings.Default.MicrospeakerLcOid, lcSpk ? 1 : 0));
						});
					}

                    if (((SndDev)sender).Type == CORESIP_SndDevType.CORESIP_SND_HF_SPEAKER &&
                        (_SndDevs.TryGetValue(CORESIP_SndDevType.CORESIP_SND_HF_SPEAKER, out dev)))
                    {
                        Top.WorkingThread.Enqueue("SetSnmp", delegate()
                        {
                            _HFSpeaker = hfSpk = dev.Jack;
                            General.SafeLaunchEvent(SetSnmpInt, this, new SnmpIntMsg<string, int>(Settings.Default.MicrospeakerHfOid, hfSpk ? 1 : 0));
                        });
                    }

				}
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="pulsed"></param>
		private void OnPttPulsed(object sender, bool pulsed)
		{
			PttSource src = ((SndDev)sender).Type == CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_MHP ? PttSource.Instructor : PttSource.Alumn;

            /* AGL.REC Estado del PTT ??? */
            _HwPtt_Activated = pulsed;
            /**/
            
            Top.WorkingThread.Enqueue("PttPulsed", delegate()
			{
				General.SafeLaunchEvent(PttPulsed, src, pulsed);
			});


			//((SndDev)sender).SetGpio(OUT_GRABACION, pulsed ? ON : OFF);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void OnHwSupervisorTimerElapsed(object sender, ElapsedEventArgs e)
		{
			Top.WorkingThread.Enqueue("OnHwSupervisorTimerElapsed", delegate
			{
				lock (_SndDevs)
				{
                    /* AGL. Evitar una excepción en la gestión de la colección _SndDevs...
                     * 
					foreach (SndDev sndDev in _SndDevs.Values)
					{
						if (!sndDev.IsValid)
							_SndDevs.Remove(sndDev.Type);
					}

					if (_SndDevs.Count != _NumDevices)
					{
						End();
						Init();
						Start();
					}
                     * */

                    foreach (SndDev sndDev in _SndDevs.Values)
                    {
                        if (!sndDev.IsValid)
                        {
                            _Logger.Warn("Error en Supervision de Canal de Sonido Tipo: {0}", sndDev.Type);

                            End();
                            Init();
                            Start();
                            break;
                        }
                    }
                    /*- Fin Modificacion -*/
                }
			});
		}

    #endregion
	}
    
#endif
}
