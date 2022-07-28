//#define _TRACEAGENT_
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Timers;

using System.ComponentModel;
using System.Reflection;

using U5ki.Infrastructure.Properties;
using NLog;
using Utilities;
using System.IO;
using System.Collections;
using System.Management;
using VideoPlayerController;

namespace U5ki.Infrastructure
{

    /// <summary>
    /// 
    /// </summary>
    public static partial class SipAgent
	{
        private static string UG5K_REC_CONF_FILE = "ug5krec-config.ini";
        
		/// <summary>
        /// 
        /// </summary>
        static InfoReceivedCb OnInfoReceived;
        public static event InfoReceivedCb InfoReceived
		{
			add {/*_Cb.*/OnInfoReceived += value; }
			remove {/*_Cb.*/OnInfoReceived -= value; }
		}
        /// <summary>
        /// 
        /// </summary>
        static KaTimeoutCb OnKaTimeout;
        public static event KaTimeoutCb KaTimeout
		{
			add { /*_Cb.*/OnKaTimeout += value; }
			remove { /*_Cb.*/OnKaTimeout -= value; }
		}
        /// <summary>
        /// 
        /// </summary>
        static IncomingSubscribeConfCb OnIncomingSubscribeConf;
        public static event IncomingSubscribeConfCb IncomingSubscribeConf
        {
            add {/*_Cb.*/OnIncomingSubscribeConf += value; }
            remove {/*_Cb.*/OnIncomingSubscribeConf -= value; }
        }

        static IncomingSubscribeConfAccCb OnIncomingSubscribeConfAcc;
        public static event IncomingSubscribeConfAccCb IncomingSubscribeConfAcc
        {
            add {/*_Cb.*/OnIncomingSubscribeConfAcc += value; }
            remove {/*_Cb.*/OnIncomingSubscribeConfAcc -= value; }
        }

        /// <summary>
        /// 
        /// </summary>
        static RdInfoCb OnRdInfo;
        public static event RdInfoCb RdInfo
		{
			add { /*_Cb.*/OnRdInfo += value; }
			remove { /*_Cb.*/OnRdInfo -= value; }
		}
        /// <summary>
        /// 
        /// </summary>
        static CallStateCb OnCallState;
        public static event CallStateCb CallState
		{
			add { /*_Cb.*/OnCallState += value; }
			remove { /*_Cb.*/OnCallState -= value; }
		}
        /// <summary>
        /// 
        /// </summary>
        static CallIncomingCb OnCallIncoming;
        public static event CallIncomingCb CallIncoming
		{
			add {/*_Cb.*/OnCallIncoming += value; }
			remove {/*_Cb.*/OnCallIncoming -= value; }
		}
        /// <summary>
        /// 
        /// </summary>
        static TransferRequestCb OnTransferRequest;
        public static event TransferRequestCb TransferRequest
		{
			add {/*_Cb.*/OnTransferRequest += value; }
			remove {/*_Cb.*/OnTransferRequest -= value; }
		}
        /// <summary>
        /// 
        /// </summary>
        static TransferStatusCb OnTransferStatus;
        public static event TransferStatusCb TransferStatus
		{
			add {/*_Cb.*/OnTransferStatus += value; }
			remove {/*_Cb.*/OnTransferStatus -= value; }
		}
        /// <summary>
        /// 
        /// </summary>
        static ConfInfoCb OnConfInfo;
        public static event ConfInfoCb ConfInfo
		{
			add {/*_Cb.*/OnConfInfo += value; }
			remove {/*_Cb.*/OnConfInfo -= value; }
		}

        static ConfInfoAccCb OnConfInfoAcc;
        public static event ConfInfoAccCb ConfInfoAcc
        {
            add {/*_Cb.*/OnConfInfoAcc += value; }
            remove {/*_Cb.*/OnConfInfoAcc -= value; }
        }

        /// <summary>
        /// 
        /// </summary>
        static DialogNotifyCb OnDialogNotify;
        public static event DialogNotifyCb DialogNotify
        {
            add {/*_Cb.*/OnDialogNotify += value; }
            remove {/*_Cb.*/OnDialogNotify -= value; }
        }

        /// <summary>
        /// 
        /// </summary>
        static PagerCb OnPager;
        public static event PagerCb Pager
        {
            add {/*_Cb.*/OnPager += value; }
            remove {/*_Cb.*/OnPager -= value; }
        }

        static WG67SubscriptionStateCb OnWG67SubscriptionState;
        public static event WG67SubscriptionStateCb WG67SubscriptionState
        {
            add {/*_Cb.*/OnWG67SubscriptionState += value; }
            remove {/*_Cb.*/OnWG67SubscriptionState -= value; }
        }

        static WG67SubscriptionReceivedCb OnWG67SubscriptionReceived;
        public static event WG67SubscriptionReceivedCb WG67SubscriptionReceived
        {
            add {/*_Cb.*/OnWG67SubscriptionReceived += value; }
            remove {/*_Cb.*/OnWG67SubscriptionReceived -= value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public static OptionsReceiveCb OnOptionsReceive;		
        public static event OptionsReceiveCb OptionsReceive
		{
			add { /*_Cb.*/OnOptionsReceive += value; }
			remove { /*_Cb.*/OnOptionsReceive -= value; }
		}
        /// <summary>
        /// 
        /// </summary>
        public static CfwrOptReceivedAccCb OnCfwrOptReceived;
        public static event CfwrOptReceivedAccCb CfwrOptReceived
        {
            add { /*_Cb.*/OnCfwrOptReceived += value; }
            remove { /*_Cb.*/OnCfwrOptReceived -= value; }
        }

        /// <summary>
        /// 
        /// </summary>
        //public static CfwrOptResponseCb OnCfwrOptResponse;
        //public static event CfwrOptResponseCb CfwrOptresponse
        //{
        //    add { /*_Cb.*/OnCfwrOptResponse += value; }
        //    remove { /*_Cb.*/OnCfwrOptResponse -= value; }
        //}
        /// <summary>
        /// 
        /// </summary>
        public static CfwrOptResponseAccCb OnCfwrOptResponse;
        public static event CfwrOptResponseAccCb CfwrOptResponse
        {
            add { /*_Cb.*/OnCfwrOptResponse += value; }
            remove { /*_Cb.*/OnCfwrOptResponse -= value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public static MovedTemporallyCb OnMovedTemporally;
        public static event MovedTemporallyCb MovedTemporally
        {
            add { /*_Cb.*/OnMovedTemporally += value; }
            remove { /*_Cb.*/OnMovedTemporally -= value; }
        }

        /// <summary>
        /// 
        /// </summary>
        static SubPresCb OnSubPres;
        public static event SubPresCb SubPres
        {
            add {/*_Cb.*/OnSubPres += value; }
            remove {/*_Cb.*/OnSubPres -= value; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="accId">Identificador del Agente (string libre)</param>
        /// <param name="ip">Direccion IP del Agente para las sesiones SIP.</param>
        /// <param name="port">Puerto UDP del Agente para las sesiones SIP.</param>
        /// <param name="max_calls">Maxima cantidad de llamadas que soporta.</param>
        /// <param name="proxy_ip">Si no es null es la Ip del proxy para rutear los paquetes SIP.</param>
        /** 20180208. */
        static bool IsInitialized = false;
        static bool IsStarted = false;

        //Settings que se puedan acceder desde otros proyectos de la solución
        public static uint _KAPeriod = Settings.Default.KAPeriod;
        class clsPlayDevices
        {
            [StructLayout(LayoutKind.Sequential, Pack = 4)]
            public struct WaveInCaps
            {
                public short wMid;
                public short wPid;
                public int vDriverVersion;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
                public char[] szPname;
                public uint dwFormats;
                public short wChannels;
                public short wReserved1;
            }

            public struct WaveOutCaps
            {
                public short wMid;
                public short wPid;
                public int vDriverVersion;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
                public char[] szPname;
                public uint dwFormats;
                public short wChannels;
                public short wReserved1;
                public short dwSupport;
            }

            [DllImport("winmm.dll")]
            public static extern int waveInGetNumDevs();
            [DllImport("winmm.dll", EntryPoint = "waveOutGetDevCaps")]
            public static extern int waveInGetDevCapsA(int uDeviceID,
                                 ref WaveInCaps lpCaps, int uSize);
            [DllImport("winmm.dll")]
            public static extern int waveOutGetNumDevs();
            [DllImport("winmm.dll", EntryPoint = "waveOutGetDevCaps")]
            public static extern int waveOutGetDevCapsA(int uDeviceID,
                                 ref WaveOutCaps lpCaps, int uSize);
            ArrayList arrLst = new ArrayList();
            ArrayList alias = new ArrayList();
            ArrayList name = new ArrayList();
            //using to store all sound recording devices strings 

            public int Count
            //to return total sound recording devices found
            {
                get { return arrLst.Count; }
            }
            public string this[int indexer]
            //return spesipic sound recording device name
            {
                get { return (string)arrLst[indexer]; }
            }
            public clsPlayDevices() //fill sound recording devices array
            {

                int waveOutDevicesCount = waveOutGetNumDevs(); //get total
                if (waveOutDevicesCount > 0)
                {
                    for (int uDeviceID = 0; uDeviceID < waveOutDevicesCount; uDeviceID++)
                    {
                        WaveOutCaps waveOutCaps = new WaveOutCaps();
                        waveOutGetDevCapsA(uDeviceID, ref waveOutCaps,
                                          Marshal.SizeOf(typeof(WaveInCaps)));
                        if (!arrLst.Contains(waveOutCaps.szPname))
                        {
                            //string[] DevWinName1 = Devices.DeviceNames.Split(separatingStrings, System.StringSplitOptions.RemoveEmptyEntries);
                            string DevWinName = new string(waveOutCaps.szPname).Remove(
                                       new string(waveOutCaps.szPname).IndexOf('\0'));
                            string[] DevWinName1 = DevWinName.Split('(');
                            alias.Add(DevWinName1[0]);

                        }
                        //clean garbage
                    }
                }




                ManagementObjectSearcher objSearcher = new ManagementObjectSearcher(
                "SELECT * FROM CIM_ManagedSystemElement");
         //       ManagementObjectSearcher objSearcher = new ManagementObjectSearcher(
         //"SELECT * FROM Win32_SoundDevice");
          //      ManagementObjectSearcher objSearcher = new ManagementObjectSearcher(
         //"SELECT * FROM Win32_PnPDevicePropertyString ");

                ManagementObjectCollection objCollection = objSearcher.Get();

              arrLst.Clear();
                try 
                {
                    //int n = objCollection.Count;
                    int cont = 0;
                    foreach (ManagementObject obj in objCollection)
                    {
                        bool guardar = false;
                        cont += 1;
                        if (cont == 10000)
                            break;
                        foreach (PropertyData property in obj.Properties)
                        {
                            //Console.Out.WriteLine(String.Format("{0}:{1}", property.Name, property.Value));
                            //string st = String.Format("{0}:{1}", property.Name, property.Value);
                            if ((property.Name == "PNPClass") && property.Value != null && (property.Value.ToString() == "AudioEndpoint"))
                            {
                                guardar = true;
                                //if (st.Contains("USB Headset"))
                                //    arrLst.Add(st);
                            }

                        }
                        if (guardar)
                        {
                            foreach (PropertyData property in obj.Properties)
                            {
                                //Console.Out.WriteLine(String.Format("{0}:{1}", property.Name, property.Value));
                                string st = String.Format("{0}:{1}", property.Name, property.Value);
                                if (property.Name == "Caption")
                                    arrLst.Add(st);
                                /*else if (property.Name == "Name")
                                    arrLst.Add(st);
                                else
                                    arrLst.Add(st);*/
                            }
                        }
                    }
                }
                catch (Exception exc)
                {
                    _Logger.Error("explorando dispositivos: ", exc);
                }
                alias.Clear();
                name.Clear();
                foreach(string s in arrLst)
                {
                    string[] separatingStrings = { "capture","(" };
                    string[] s1 = s.Split(separatingStrings, System.StringSplitOptions.RemoveEmptyEntries);
                    string s3 = s1[1];
                    string hw = s1[2];
                    alias.Add(s3);
                    name.Add(hw);
                }
            }
        }
        //RQF24
        public static void Record(bool on)
        {
            // Falta por implementar esta función.
#if IMPLEMENTED
            CORESIP_Record(on);

#endif
            CORESIP_Error err;
            // Notificar al módulo de grabación que ha cambiado la configuración.
            CORESIP_RecCmdType valor = (on)?CORESIP_RecCmdType.CORESIP_REC_ENABLE:
                           CORESIP_RecCmdType.CORESIP_REC_DISABLE;
            if (CORESIP_RecorderCmd(valor, out err) != 0)
            {
                throw new Exception(err.Info);
            }
            _Logger.Debug("Notificado al módulo de CoreSip {0}",valor.ToString());
        }

        class AudioDeviceInfo
        {
            public string id { get; set; }
            public float vmax { get; set; }
            public float vmin { get; set; }
            public int channel { get; set; }
            public bool speaker { get; set; }
        }



        public static void Init(string accId, string ip, uint port, uint max_calls = 32, string proxyIP=null)
		{
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.Init");
#endif

            /** 20180208 */
            if (!IsInitialized)
            {
                IsInitialized = true;

                _Ip = ip;
                _Port = port;

                /** 20180208. Para que convivan mas de un proceso con la misma CORESIP */
                CallbacksInit();

                /** */
                CORESIP_Config cfg = new CORESIP_Config();

                cfg.HostId = accId;     //GRABACION VOIP
                cfg.IpAddress = ip;
                cfg.Port = port;
                cfg.Cb = _Cb;
                cfg.DefaultCodec = Settings.Default.DefaultCodec;
                cfg.DefaultDelayBufPframes = Settings.Default.DefaultDelayBufPframes;
                cfg.DefaultJBufPframes = Settings.Default.DefaultJBufPframes;
                cfg.SndSamplingRate = Settings.Default.SndSamplingRate;
                cfg.RxLevel = Settings.Default.RxLevel;
                cfg.TxLevel = Settings.Default.TxLevel;
                cfg.LogLevel = Settings.Default.SipLogLevel;
                cfg.TsxTout = Settings.Default.TsxTout;
                cfg.InvProceedingIaTout = Settings.Default.InvProceedingIaTout;
                cfg.InvProceedingMonitoringTout = Settings.Default.InvProceedingMonitoringTout;
                cfg.InvProceedingDiaTout = Settings.Default.InvProceedingDiaTout;
                cfg.InvProceedingRdTout = Settings.Default.InvProceedingRdTout;

                // AGL 20131121.
                cfg.EchoTail = Settings.Default.EchoTail;
                cfg.EchoLatency = Settings.Default.EchoLatency;
                // FM           

                /// JCAM 18/01/2016
                /// Grabación según norma ED-137
                //cfg.RecordingEd137 = Settings.Default.RecordingEd137;
                //LALM211221
                cfg.RecordingEd137 = (uint)GetGrabacionED137();

                cfg.max_calls = max_calls;     // Maximo número de llamadas por defecto en el puesto
                cfg.TimeToDiscardRdInfo = Settings.Default.SQSourceConfirmTime;

                cfg.DIA_TxAttenuation_dB = Settings.Default.DIA_TxAttenuation_dB;
                cfg.IA_TxAttenuation_dB = Settings.Default.IA_TxAttenuation_dB;
                cfg.RD_TxAttenuation_dB = Settings.Default.RD_TxAttenuation_dB;

                cfg.UseDefaultSoundDevices = 0;

                CORESIP_Error err;
                if (CORESIP_Init(cfg, out err) != 0)
                {
                        throw new Exception(err.Info);
                }

                try
                {
                    if (proxyIP == null)
                        //Crea una cuenta que sólo existe cuando no hay configuración
                        SipAgent.CreateAccount(accId);
                    else
                        SipAgent.CreateAccountProxyRouting(accId, proxyIP);
                }
                catch (Exception exc)
                {
                    _Logger.Error("CreateAccount", exc);
                }

                if (CORESIP_Set_Ed137_version('C', 'C', out err) != 0)
                {
                    _Logger.Error("CORESIP_Set_Ed137_version "+err.Info);
                }

                CORESIP_SndWindowsDevices Devices;
                List<string> DevWinName = new List<string>();
                if (CORESIP_GetWindowsSoundDeviceNames(0, out Devices, out err) != 0)
                {

                }
                else
                {
                    _Logger.Info("CORESIP_GetWindowsSoundDeviceNames  ndevices_found " + Devices.ndevices_found + " " + Devices.DeviceNames);
                    string[] separatingStrings = { "<###>"};
                    string[] DevWinName1 = Devices.DeviceNames.Split(separatingStrings, System.StringSplitOptions.RemoveEmptyEntries);
                    foreach (string device in DevWinName1)
                    {
                        _Logger.Info("CORESIP_GetWindowsSoundDeviceNames: " + device);
                    }
                    DevWinName.AddRange(DevWinName1);
                }



#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.Init");
#endif
            }
        }

        public static bool Valid(string device)
        {
            if (GetNameDevice(-1, device)!=null)
                return true;
            return false;
        }

        public static void Asignacion(CORESIP_SndDevType tipo, string device)
        {
            // Asignacion
            CORESIP_Error err;
            if (!Valid(device))
            {
                _Logger.Info("CORESIP_SetSNDDeviceWindowsName: " + tipo + "Not valid device: " + device);

            }
            else if (CORESIP_SetSNDDeviceWindowsName(tipo, device, out err) != 0)
            {
                _Logger.Info("Error CORESIP_SetSNDDeviceWindowsName: " + tipo + "device: " + device + err.Info);
            }
            else
            {
                _Logger.Info("CORESIP_SetSNDDeviceWindowsName: " + tipo + "device: " + device);

            }
        }

        public static string GetNameDevice(int indice,string mascara=null)
        {
            CORESIP_Error err;
            CORESIP_SndWindowsDevices Devices;
            List<string> DevWinName = new List<string>();
            if (CORESIP_GetWindowsSoundDeviceNames(0, out Devices, out err) != 0)
            {
                return null;
            }
            else
            {
                _Logger.Info("CORESIP_GetWindowsSoundDeviceNames  ndevices_found " + Devices.ndevices_found + " " + Devices.DeviceNames);
                string[] separatingStrings = { "<###>" };
                string[] DevWinName1 = Devices.DeviceNames.Split(separatingStrings, System.StringSplitOptions.RemoveEmptyEntries);
                DevWinName.AddRange(DevWinName1);
                if (mascara != null && mascara.Length>0)
                {
                    foreach (string dev in DevWinName1)
                    {
                        //_Logger.Info("GetNameDevice  " + mascara+ " "+ dev);
                        try
                        {
                            if ((dev!=null) && (dev.Length > 0) && (dev.Contains(mascara)))
                            {
                                _Logger.Info("GetNameDevice  " + mascara);
                                return dev;
                            }
                            else  if ((dev == null) || (dev.Length == 0))
                            {
                                if (mascara!= "-none-")
                                    _Logger.Info("GetNameDevice  " + mascara + " No Encontrada");
                                return null;
                            }
                        }
                        catch (Exception excep)
                        {
                            return null;
                        }
                    }
                }
                if (mascara==null && DevWinName1.Length > indice)
                {
                    _Logger.Info("Buscando dispositivo + Devices.ndevices_found "+ indice.ToString() + " " + DevWinName1[indice]);
                    return DevWinName[indice];
                }
                else
                    return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
		public static void Start()
		{
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.Start");
#endif
            /** 20180208 */
            if (!IsStarted)
            {
                CORESIP_Error err;
                if (CORESIP_Start(out err) != 0)
                {
                    throw new Exception(err.Info);
                }
                IsStarted = true;
            }
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.Start");
#endif
        }
        /// <summary>
        /// 
        /// </summary>
		public static void End()
		{
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.End");
#endif
            if (IsStarted || IsInitialized)
            {

                CORESIP_End();
                _Accounts.Clear();
                IsInitialized = IsStarted = false;
            }

#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.End");
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="level"></param>
		public static void SetLogLevel(LogLevel level)
		{
			uint eqLevel = 0;

#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.SetLogLevel {0}", level.Name);
#endif
            if (level == LogLevel.Fatal)
			{
				eqLevel = 1;
			}
			else if (level == LogLevel.Error)
			{
				eqLevel = 2;
			}
			else if (level == LogLevel.Warn)
			{
				eqLevel = 3;
			}
			else if (level == LogLevel.Info)
			{
				eqLevel = 4;
			}
			else if (level == LogLevel.Debug)
			{
				eqLevel = 5;
			}
			else if (level == LogLevel.Trace)
			{
				eqLevel = 6;
			}
			CORESIP_Error err;
			CORESIP_SetLogLevel(eqLevel, out err);
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.SetLogLevel");
#endif
        }

        /**
         *	CreateAccount. Registra una cuenta SIP en el Módulo. @ref SipAgent::CreateAccount
         *	@param	acc			Puntero al Numero de Abonado (usuario). NO a la uri.
         *	@param	defaultAcc	Marca si esta cuenta pasa a ser la Cuenta por Defecto.
         *	@param	accId		Puntero a el identificador de cuenta asociado.
         *	@param	error		Puntero @ref CORESIP_Error a la Estructura de error
         *	@return				Codigo de Error
         */
        public static void CreateAccount(string accId)
		{
			int id;
			CORESIP_Error err;
			string sipAcc = string.Format("<sip:{0}@{1}:{2}>", accId, _Ip, _Port);

#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.CreateAccount");
#endif
            if (CORESIP_CreateAccount(sipAcc, 0, out id, out err) != 0)
			{
				throw new Exception(err.Info);
			}
			_Accounts[accId] = id;
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.CreateAccount");
#endif
        }

        /**
         *	CORESIP_CreateAccountProxyRouting. Registra una cuenta SIP en el Módulo y los paquetes sip se enrutan por el proxy. @ref SipAgent::CreateAccount
         *	@param	acc			Puntero al Numero de Abonado (usuario). NO a la uri.
         *	@param	defaultAcc	Marca si esta cuenta pasa a ser la Cuenta por Defecto.
         *	@param	accId		Puntero a el identificador de cuenta asociado.
         *  @param	proxy_ip	Si es distinto de NULL. IP del proxy Donde se quieren enrutar los paquetes.
         *	@param	error		Puntero @ref CORESIP_Error a la Estructura de error
         *	@return				Codigo de Error
         */
        /// <summary>
        /// 
        /// </summary>
        /// <param name="accId"></param>
        public static void CreateAccountProxyRouting(string accId, string proxyIP)
        {
            int id;
            CORESIP_Error err;
            string sipAcc = string.Format("<sip:{0}@{1}:{2}>", accId, _Ip, _Port);

#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.CreateAccount");
#endif
            //Cuenta por defecto
            if (CORESIP_CreateAccountProxyRouting(sipAcc, 1, out id, proxyIP, out err) != 0)
            {
                throw new Exception(err.Info);
            }
            _Accounts[accId] = id;
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.CreateAccount");
#endif
        }

        /**
         *	CreateAccountAndRegisterInProxy. Crea una cuenta y se registra en el SIP proxy. Los paquetes sip se rutean por el SIP proxy también.
         *	@param	acc			Puntero al Numero de Abonado (usuario). NO a la uri.
         *	@param	defaultAcc	Si es diferente a '0', indica que se creará la cuenta por Defecto.
         *	@param	accId		Puntero a el identificador de cuenta asociado que retorna.
         *	@param	proxy_ip	IP del proxy.
         *	@param	expire_seg  Tiempo en el que expira el registro en segundos.
         *	@param	username	Si no es necesario autenticación, este parametro será NULL
         *	@param  pass		Password. Si no es necesario autenticación, este parametro será NULL
         *	@param  DisplayName	Display name que va antes de la sip URI, se utiliza para como nombre a mostrar
         *	@param	isfocus		Si el valor es true, indica que es Focus, para establecer llamadas multidestino. por defecto es falso.
         *	@param	error		Puntero @ref CORESIP_Error a la Estructura de error
         *	@return				Codigo de Error
         */
        /// <summary>
        /// 
        /// </summary>
        /// <param name="accId"></param>
        public static void CreateAccountAndRegisterInProxy(string accId, string proxy_ip, uint expire_seg, string username, string pass, string displayName, bool isFocus = false)
        {
            int id;
            CORESIP_Error err;
            int isFocusArg = 0;

            if (isFocus) isFocusArg = 1;
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.CreateAccountAndRegisterInProxy");
#endif
            if (CORESIP_CreateAccountAndRegisterInProxy(accId, 0, out id, proxy_ip, expire_seg, username, pass, displayName, isFocusArg, out err) != 0)
            {
                throw new Exception(err.Info);
            }
            _Accounts[accId] = id;
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.CreateAccountAndRegisterInProxy");
#endif
        }

           /// <summary>
           /// 
           /// </summary>
           public static void DestroyAccount(string accId)
            {
        #if _TRACEAGENT_
                    _Logger.Debug("Entrando en SipAgent.DestroyAccount");
        #endif
                    CORESIP_Error err;

                    if (CORESIP_DestroyAccount(_Accounts[accId], out err) != 0)
                    {
                        _Logger.Error("SipAgent.DestroyAccount: " + err.Info);
                    }

                    _Accounts.Remove(accId);
        #if _TRACEAGENT_
                    _Logger.Debug("Saliendo de SipAgent.DestroyAccount");
        #endif
        }
        /// <summary>
        /// 
        /// </summary>
		public static void DestroyAccounts()
		{
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.DestroyAccounts");
#endif
            foreach (int id in _Accounts.Values)
			{
				CORESIP_Error err;

				if (CORESIP_DestroyAccount(id, out err) != 0)
				{
                    _Logger.Error("SipAgent.DestroyAccounts: " + err.Info);
				}
			}
			_Accounts.Clear();
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.DestroyAccounts");
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="inDevId"></param>
        /// <param name="outDevId"></param>
        /// <returns></returns>
		public static int AddSndDevice(CORESIP_SndDevType type, int inDevId, int outDevId)
		{
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.AddSndDevice {0}, {1}, {2}", type.ToString(), inDevId, outDevId);
#endif
            _Logger.Debug("Entrando en SipAgent.AddSndDevice {0}, {1}, {2}", type.ToString(), inDevId, outDevId);
            CORESIP_SndDeviceInfo info = new CORESIP_SndDeviceInfo();

			info.Type = type;
			info.OsInDeviceIndex = inDevId;
			info.OsOutDeviceIndex = outDevId;

			int dev=0;
			CORESIP_Error err;
           
            if (CORESIP_AddSndDevice(info, out dev, out err) != 0)            
            {            
                throw new Exception(err.Info);                
            }

#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.AddSndDevice");
#endif
            return dev;
		}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="loop"></param>
        /// <returns></returns>
		public static int CreateWavPlayer(string file, bool loop)
		{
			int wavPlayer;
			CORESIP_Error err;
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.CreateWavPlayer {0}, {1}", file, loop);
#endif

			if (CORESIP_CreateWavPlayer(file, loop ? 1 : 0, out wavPlayer, out err) != 0)
			{
				throw new Exception(err.Info);
			}

#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.CreateWavPlayer");
#endif
            return wavPlayer;
		}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="wavPlayer"></param>
		public static void DestroyWavPlayer(int wavPlayer)
		{
			CORESIP_Error err;
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.DestroyWavPlayer {0}", wavPlayer);
#endif

			if (CORESIP_DestroyWavPlayer(wavPlayer, out err) != 0)
			{
                _Logger.Error("SipAgent.DestroyWavPlayer: " + err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.DestroyWavPlayer");
#endif
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="file"></param>
		public static int CreateWavRecorder(string file)
		{
			int wavRecorder;
			CORESIP_Error err;
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.CreateWavRecorder {0}", file);
#endif

			if (CORESIP_CreateWavRecorder(file, out wavRecorder, out err) != 0)
			{
				throw new Exception(err.Info);
			}

#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.CreateWavRecorder");
#endif
            return wavRecorder;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="wavRecorder"></param>
		public static void DestroyWavRecorder(int wavRecorder)
		{
            CORESIP_Error err;
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.DestroyWavRecorder {0}", wavRecorder);
#endif
            if (CORESIP_DestroyWavRecorder(wavRecorder, out err) != 0)
			{
				_Logger.Error("SipAgent.DestroyWavRecorder: " + err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.DestroyWavRecorder");
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rs"></param>
        /// <param name="localIp"></param>
        /// <returns></returns>
		public static int CreateRdRxPort(RdSrvRxRs rs, string localIp)
		{
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.CreateRdRxPort {0}, {1}", rs.ToString(), localIp);
#endif
            CORESIP_RdRxPortInfo info = new CORESIP_RdRxPortInfo();

			info.ClkRate = rs.ClkRate;
			info.ChannelCount = rs.ChannelCount;
			info.BitsPerSample = rs.BitsPerSample;
			info.FrameTime = rs.FrameTime;
			info.Ip = rs.McastIp;
			info.Port = rs.RdRxPort;

			int mcastPort;
			CORESIP_Error err;

			if (CORESIP_CreateRdRxPort(info, localIp, out mcastPort, out err) != 0)
			{
				throw new Exception(err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.CreateRdRxPort");
#endif

            return mcastPort;
		}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="port"></param>
		public static void DestroyRdRxPort(int port)
		{
			CORESIP_Error err;
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.DestroyRdRxPort {0}", port);
#endif

			if (CORESIP_DestroyRdRxPort(port, out err) != 0)
			{
                _Logger.Error("SipAgent.DestroyRdrxPort: " + err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.DestroyRdRxPort");
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
		public static int CreateSndRxPort(string name)
		{
			int sndRxPort;
			CORESIP_Error err;

#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.CreateSndRxPort {0}", name);
#endif
            if (CORESIP_CreateSndRxPort(name, out sndRxPort, out err) != 0)
			{
				throw new Exception(err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.CreateSndRxPort");
#endif
			return sndRxPort;
		}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="port"></param>
		public static void DestroySndRxPort(int port)
		{
			CORESIP_Error err;

#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.DestroySndRxPort {0}", port);
#endif
            if (CORESIP_DestroySndRxPort(port, out err) != 0)
			{
                _Logger.Error("SipAgent.DestroySndRxPort: " + err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.DestroySndRxPort");
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="srcId"></param>
        /// <param name="dstId"></param>
		public static void MixerLink(int srcId, int dstId)
		{
			CORESIP_Error err;
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.MixerLink {0}, {1}", srcId, dstId);
#endif
			if (CORESIP_BridgeLink(srcId, dstId, 1, out err) != 0)
			{
				throw new Exception(err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.MixerLink");
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="srcId"></param>
        /// <param name="dstId"></param>
		public static void MixerUnlink(int srcId, int dstId)
		{
			CORESIP_Error err;
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.MixerUnLink {0},{1}", srcId, dstId);
#endif
            if (CORESIP_BridgeLink(srcId, dstId, 0, out err) != 0)
			{
				throw new Exception(err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.MixerUnLink");
#endif
        }
        /// <summary>
        /// Usado para la transmision del programa HMI hacia el nodebox-master. Cuando se hace PTT.
        /// </summary>
        /// <param name="sndDevId">Identificador de dispositivo asociado a un microfono</param>
        /// <param name="id">Identificador asociado al nodebox-master.</param>
        /// <param name="ip">Direccion mcast en la que escucha el nodebox-master</param>
        /// <param name="port">Puesto UDP asociado al grupo mcast donde escucha el nodebox-master.</param>
		public static void SendToRemote(int sndDevId, string id, string ip, uint port)
		{
			CORESIP_Error err;

#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.SendToRemte {0}, {1}, {2}, {3}", sndDevId, id, ip, port);
#endif
            if (CORESIP_SendToRemote(sndDevId, 1, id, ip, port, out err) != 0)
			{
				throw new Exception(err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.SendtoRemote");
#endif
        }
        /// <summary>
        /// Usado por el nodebox-master para recibir tramas del HMI....
        /// </summary>
        /// <param name="localIp"></param>
        /// <param name="mcastIp"></param>
        /// <param name="mcastPort"></param>
		public static void ReceiveFromRemote(string localIp, string mcastIp, uint mcastPort)
		{
			CORESIP_Error err;
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.ReceiveFromRemote {0}, {1}, {2}", localIp, mcastIp, mcastPort);
#endif
            if (CORESIP_ReceiveFromRemote(localIp, mcastIp, mcastPort, out err) != 0)
			{
				throw new Exception(err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.ReceiveFromRemote");
#endif
        }
        /// <summary>
        /// Usado por el HMI para 'redireccionar' su transmision. YA no va hacia el nodebox-master. (por ejemplo para cuando utiliza telefonia).
        /// </summary>
        /// <param name="sndDevId">Identificador del dispositivo asociado a un micrófono.</param>
		public static void UnsendToRemote(int sndDevId)
		{
			CORESIP_Error err;

#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.UnsendToRemote {0}", sndDevId);
#endif
            if (CORESIP_SendToRemote(sndDevId, 0, null, null, 0, out err) != 0)
			{
                _Logger.Error("SipAgent.UnsendToRemote: " + err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.UnsendToRemote");
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="volume"></param>
		public static void SetVolume(int id, int volume)
		{
			CORESIP_Error err;

#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.SetVolume {0}, {1}", id, volume);
#endif
            if (CORESIP_SetVolume(id, volume, out err) != 0)
			{
                _Logger.Error("Error SipAgent.SetVolume {0}", err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.SetVolume");
#endif
        }

        //#5829 Nueva funcion set volumen que afecta solo al dispositivo fisico
        public static void SetVolume(CORESIP_SndDevType id, int volume)
        {
            CORESIP_Error err;

#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.SetVolume {0}, {1}", id, volume);
#endif
            if (CORESIP_SetVolumeOutputDevice(id, (uint)volume, out err) != 0)
            {
                _Logger.Error("Error SipAgent.CORESIP_SetVolumeOutputDevice {0}", err.Info);
            }
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.SetVolume");
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
		public static int GetVolume(int id)
		{
			int volume;
			CORESIP_Error err;

#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.GetVolume {0}", id);
#endif
            if (CORESIP_GetVolume(id, out volume, out err) != 0)
			{
				throw new Exception(err.Info);
			}

#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.GetVolume");
#endif
            return volume;
		}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="accId"></param>
        /// <param name="dst"></param>
        /// <param name="referBy"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
		public static int MakeTlfCall(string accId, string dst, string referBy, CORESIP_Priority priority)
		{
            int retorno = MakeTlfCall(accId, dst, referBy, priority, CORESIP_CallFlags.CORESIP_CALL_NO_FLAGS);
            return retorno;
		}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="accId"></param>
        /// <param name="dst"></param>
        /// <param name="referBy"></param>
        /// <param name="priority"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
		public static int MakeTlfCall(string accId, string dst, string referBy, CORESIP_Priority priority, CORESIP_CallFlags flags)
		{
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.MakTlfCall {0}, {1}, {2}, {3}, {4}", accId, dst, referBy, priority, flags);
#endif
            int acc;
			if (string.IsNullOrEmpty(accId) || !_Accounts.TryGetValue(accId, out acc))
			{
                _Logger.Warn("Llamada con account Id desconocida -1" + accId);
				acc = -1;
			}

			CORESIP_CallInfo info = new CORESIP_CallInfo();
			CORESIP_CallOutInfo outInfo = new CORESIP_CallOutInfo();

			info.AccountId = acc;
			info.Type = CORESIP_CallType.CORESIP_CALL_DIA;
			info.Priority = priority;
			info.CallFlags = (uint) flags;

            outInfo.DstUri = dst;
			outInfo.ReferBy = referBy;
            outInfo.RequireReplaces = false;

			int callId;
			CORESIP_Error err;

			if (CORESIP_CallMake(info, outInfo, out callId, out err) != 0)
			{
				throw new Exception(err.Info);
			}

#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.MakeTlfCall");
#endif
            return callId;
		}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="accId"></param>
        /// <param name="dst"></param>
        /// <param name="referBy"></param>
        /// <param name="priority"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static int MakeTlfCallReplaces(string accId, string dst, CORESIP_Priority priority, CORESIP_CallFlags flags, string callIdReplace,
            string toTag, string fromTag)
        {
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.MakTlfCall {0}, {1}, {2}, {3}, {4}", accId, dst, priority, flags, callIdReplace);
#endif
            int acc;
            if (string.IsNullOrEmpty(accId) || !_Accounts.TryGetValue(accId, out acc))
            {
                _Logger.Warn("Llamada con account Id desconocida -1" + accId);
                acc = -1;
            }

            CORESIP_CallInfo info = new CORESIP_CallInfo();
            CORESIP_CallOutInfo outInfo = new CORESIP_CallOutInfo();

            info.AccountId = acc;
            info.Type = CORESIP_CallType.CORESIP_CALL_DIA;
            info.Priority = priority;
            info.CallFlags = (uint)flags;

            outInfo.DstUri = dst;
            outInfo.RequireReplaces = true;
            outInfo.ToTag = fromTag;
            outInfo.FromTag = toTag;
            outInfo.CallIdToReplace = callIdReplace;
            outInfo.EarlyOnly = true;
             int callId;
            CORESIP_Error err;

            if (CORESIP_CallMake(info, outInfo, out callId, out err) != 0)
            {
                throw new Exception(err.Info);
            }

#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.MakeTlfCall");
#endif
            return callId;
        }        /// <summary>
        /// 
        /// </summary>
        /// <param name="accId"></param>
        /// <param name="dst"></param>
        /// <returns></returns>
        public static int MakeLcCall(string accId, string dst, CORESIP_CallFlags flags)
		{
			int acc;
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.MakLcCall {0}, {1}", accId, dst);
#endif
            if (string.IsNullOrEmpty(accId) || !_Accounts.TryGetValue(accId, out acc))
			{
				acc = -1;
			}

			CORESIP_CallInfo info = new CORESIP_CallInfo();
			CORESIP_CallOutInfo outInfo = new CORESIP_CallOutInfo();

            info.AccountId = acc;
			info.Type = CORESIP_CallType.CORESIP_CALL_IA;
			info.Priority = CORESIP_Priority.CORESIP_PR_URGENT;
            info.CallFlags = (uint)flags;

            outInfo.DstUri = dst;

			int callId;
			CORESIP_Error err;

			if (CORESIP_CallMake(info, outInfo, out callId, out err) != 0)
			{
				throw new Exception(err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.MakeLcCall");
#endif
			return callId;
		}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="accId"></param>
        /// <param name="dst"></param>
        /// <param name="frecuency"></param>
        /// <param name="IdDestino"></param> Identificador unico del distino radio. Puede haber 2 destinos con el mismo identificador de la frecuencia. 
        ///                                 Pero este parametro es unico para un destino radio
        /// <param name="flags"></param>
        /// <param name="mcastIp">Grupo Multicast de Recepcion para los HMI.</param>
        /// <param name="mcastPort">Puerto del grupo asociado al recurso radio.</param>
        /// /// <param name="porcentajeRSSI">Peso del valor de Qidx del tipo RSSI en el calculo del Qidx final. 0 indica que el calculo es solo centralizado. 10 que el calculo es solo el RSSI.</param>
        /// <returns></returns>
		public static int MakeRdCall(string accId, string dst, string frecuency, string IdDestino, CORESIP_CallFlags flags, string mcastIp, uint mcastPort,
            CORESIP_Priority prioridad, string Zona, CORESIP_FREQUENCY_TYPE FrequencyType,
            CORESIP_CLD_CALCULATE_METHOD CLDCalculateMethod, int BssWindows, bool AudioSync, bool AudioInBssWindow,
            int cld_supervision_time, string bss_method, uint porcentajeRSSI = 0)
		{
            int acc;
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.MakRdCall {0}, {1}, {2}, {3}, {4}, {5}, {6}", accId, dst, frecuency, flags, mcastIp,mcastPort,prioridad);
#endif
            if (string.IsNullOrEmpty(accId) || !_Accounts.TryGetValue(accId, out acc))
			{
				acc = -1;
			}

			CORESIP_CallInfo info = new CORESIP_CallInfo();
			CORESIP_CallOutInfo outInfo = new CORESIP_CallOutInfo();

			info.AccountId = acc;
			info.Type = CORESIP_CallType.CORESIP_CALL_RD;
            /* AGL*/
            info.Priority = prioridad;  // CORESIP_Priority.CORESIP_PR_EMERGENCY;
            info.CallFlags = (uint)flags;

            info.R2SKeepAlivePeriod = (int)U5ki.Infrastructure.Properties.Settings.Default.KAPeriod;
            info.R2SKeepAliveMultiplier = (int)U5ki.Infrastructure.Properties.Settings.Default.KAMultiplier;
#if _VOTER_
            /** 20160609 */
            info.PreferredCodec = 0;        // Globales.IndexOfPreferredCodec;
#endif
            //EDU 20170223
            info.Zona = Zona;
            info.FrequencyType = FrequencyType;
            info.CLDCalculateMethod = CLDCalculateMethod;
            info.BssWindows = BssWindows;
            info.AudioSync = AudioSync;
            info.AudioInBssWindow = AudioInBssWindow;
            info.cld_supervision_time = cld_supervision_time;
            info.bss_method = bss_method;
            info.porcentajeRSSI = porcentajeRSSI;

			outInfo.DstUri = dst;
			outInfo.RdFr = frecuency;
            outInfo.IdDestino = IdDestino;
            outInfo.RdMcastAddr = mcastIp;
			outInfo.RdMcastPort = mcastPort;

			int callId = -1;
			CORESIP_Error err;
            try
            {
                if (CORESIP_CallMake(info, outInfo, out callId, out err) != 0)
                {
                    _Logger.Error("SipAgent.MakeRdCall: [" + outInfo.DstUri + "]" + err.Info);
                }
            }
            catch (Exception excep)
            {
                _Logger.Error("SipAgent.MakeRdCall exception: [" + outInfo.DstUri + "]", excep);
            }
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.MakeRdCall");
#endif
			return callId;
		}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="accId"></param>
        /// <param name="dst"></param>
        /// <returns></returns>
        public static int MakeMonitoringCall(string accId, string dst, CORESIP_CallFlags flags)
		{
            //El HMI de ULises solo soporta G/G monitoring
            return MakeMonitoringCall(accId, dst, CORESIP_CallType.CORESIP_CALL_GG_MONITORING, flags);                                    
		}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="accId"></param>
        /// <param name="dst"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static int MakeMonitoringCall(string accId, string dst, CORESIP_CallType type, CORESIP_CallFlags flags)
		{
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.MakeMonitoringCall {0}, {1}, {2}", accId, dst, type);
#endif
            int acc;
			if (string.IsNullOrEmpty(accId) || !_Accounts.TryGetValue(accId, out acc))
			{
				acc = -1;
			}

			CORESIP_CallInfo info = new CORESIP_CallInfo();
			CORESIP_CallOutInfo outInfo = new CORESIP_CallOutInfo();

			info.AccountId = acc;
			info.Type = type;
			info.Priority = CORESIP_Priority.CORESIP_PR_NONURGENT;  // .CORESIP_PR_NORMAL;
            info.CallFlags = (uint)flags;

            outInfo.DstUri = dst;

			int callId;
			CORESIP_Error err;

			if (CORESIP_CallMake(info, outInfo, out callId, out err) != 0)
			{
				throw new Exception(err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.MakeMonitoringCall");
#endif
			return callId;
		}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="callId"></param>
		public static void HangupCall(int callId)
		{
			CORESIP_Error err;

#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.HangupCall {0}", callId);
#endif
            if (CORESIP_CallHangup(callId, 0, out err) != 0)
			{
                _Logger.Error("SipAgent.HangupCall_1: " + err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.HangupCall");
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="callId"></param>
        /// <param name="code"></param>
		public static void HangupCall(int callId, int code)
		{
			CORESIP_Error err;
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.HangupCall {0}, {1}", callId, code);
#endif
            if (CORESIP_CallHangup(callId, code, out err) != 0)
			{
                _Logger.Error("SipAgent.HangupCall_2: " + err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.HangupCall");
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="callId"></param>
        /// <param name="response"></param>
		public static void AnswerCall(int callId, int response)
        {
            AnswerCall(callId, response, false);
        }
        public static void AnswerCall(int callId, int response, int reason_code=0, string reason_text="")
        {
            AnswerCall(callId, response, reason_code, reason_text, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callId"></param>
        /// <param name="response"></param>
        /// <param name="addToConference"></param>
		public static void AnswerCall(int callId, int response, int reason_code,string reason_string, bool addToConference)
        {
            CORESIP_Error err;

#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.AnswerCall {0}, {1}, {2}, {3}", callId, response, reason_code, addToConference);
#endif
#if !UNIT_TEST
            if (CORESIP_CallAnswer(callId, response, addToConference ? 1 : 0, reason_code, reason_string, out err) != 0)
            {
                throw new Exception(err.Info);
            }
#endif
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.AnswerCall");
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callId"></param>
        /// <param name="response"></param>
        /// <param name="addToConference"></param>
		public static void AnswerCall(int callId, int response, bool addToConference)
		{
			CORESIP_Error err;

#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.AnswerCall {0}, {1}, {2}", callId, response, addToConference);
#endif
#if !UNIT_TEST
            if (CORESIP_CallAnswer(callId, response, addToConference ? 1 : 0, 0, null, out err) != 0)
			{
				throw new Exception(err.Info);
			}
#endif
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.AnswerCall");
#endif
        }
        /// <summary>
        /// Envia una respuesta 302 (Moved Temporally) al INVITE
        /// </summary>
        /// <param name="callId"></param>
        /// <param name="dst"> Uri del destino al que se desvia la llamada </param>
        /// <param name="reason"> Razon del desvio: "unconditional", "user-busy", etc.</param>
		public static void MovedTemporallyAnswerCall(int callId, string dst, string reason)
        {
            CORESIP_Error err;

#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.CORESIP_CallMovedTemporallyAnswer {0}, {1}, {2}", callId, dst, reason);
#endif
#if !UNIT_TEST
            if (CORESIP_CallMovedTemporallyAnswer(callId, dst, reason, out err) != 0)
            {
                throw new Exception(err.Info);
            }
#endif
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.CORESIP_CallMovedTemporallyAnswer");
#endif
        }

        /// <summary>
        /// Esta funcion debe llamarse despues de recibirse la callback MovedTemporallyCb para aceptar o rechazar la redireccion de la llamada.
        /// </summary>
        /// <param name="callId">Identificador de Llamada</param>
        /// <param name="dstUri">Nueva request uri hacia donde se desvia la llamada. Si se rechaza entonces este parametro se ignora</param>
        /// <param name="op"> Opcion (aceptar o rechazar) </param>
        public static void CallProccessRedirect(int callId, string dstUri, CORESIP_REDIRECT_OP op)
        {
#if CALLFORWARD
            CORESIP_Error err;

#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.CORESIP_CallProccessRedirect {0}, {1}, {2}", callId, dstUri, op);
#endif
#if !UNIT_TEST
            if (CORESIP_CallProccessRedirect(callId, dstUri, op, out err) != 0)
            {
                throw new Exception(err.Info);
            }
#endif
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.CORESIP_CallProccessRedirect");
#endif
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="callId"></param>
        public static void HoldCall(int callId)
		{
			CORESIP_Error err;

#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.HoldCall {0}", callId);
#endif
            if (CORESIP_CallHold(callId, 1, out err) != 0)
			{
				throw new Exception(err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.HoldCall");
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callId"></param>
        public static void CallReinvite(int callId, ReinviteType type)
        {
            CORESIP_Error err;

#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.CallCouplingReinvite {0}", callId);
#endif
            if (CORESIP_CallReinvite(callId, out err, (int) type, 22, null) != 0)
            {
                throw new Exception(err.Info);
            }
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.CallCouplingReinvite");
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callId"></param>
		public static void UnholdCall(int callId)
		{
			CORESIP_Error err;
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.UnholdCall {0}", callId);
#endif

			if (CORESIP_CallHold(callId, 0, out err) != 0)
			{
				throw new Exception(err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.UnholdCall");
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="callId"></param>
        /// <param name="dstCallId"></param>
        /// <param name="dst"></param>
		public static void TransferCall(int callId, int dstCallId, string dst, string displayName)
		{
			CORESIP_Error err;

#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.TransferCall {0}, {1}, {2}", callId, dstCallId, dst);
#endif
            if (CORESIP_CallTransfer(callId, dstCallId, dst, displayName, out err) != 0)
			{
				throw new Exception(err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.TransferCall");
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="callId"></param>
        /// <param name="pttId"></param>
        /// <param name="pttType"></param>
        public static void PttOn(int callId, ushort pttId, CORESIP_PttType pttType, CORESIP_PttMuteType pttMute)
		{
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.PttOn {0}, {1}, {2}", callId, pttId, pttType);
#endif
            CORESIP_PttInfo info = new CORESIP_PttInfo();

			info.PttType = pttType;
			info.PttId = pttId;
            if (pttType == CORESIP_PttType.CORESIP_PTT_OFF)
                info.PttMute = (uint) CORESIP_PttMuteType.DESACTIVADO;
            else
                info.PttMute = (uint) pttMute;
			CORESIP_Error err;

			if (CORESIP_CallPtt(callId, info, out err) != 0)
			{
				throw new Exception(err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.PttOn");
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="callId"></param>
		public static void PttOff(int callId)
		{
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.PttOff {0}", callId);
#endif
            CORESIP_PttInfo info = new CORESIP_PttInfo();
			info.PttType = CORESIP_PttType.CORESIP_PTT_OFF;
            info.PttMute = (uint)CORESIP_PttMuteType.DESACTIVADO;

			CORESIP_Error err;

			if (CORESIP_CallPtt(callId, info, out err) != 0)
			{
                _Logger.Warn("Error SipAgent.PttOff " + err.Info);
				//throw new Exception(err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.PttOff");
#endif
        }

        public static int GetRdQidx(int callId)
        {
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.GetRdResourceInfo {0}", callId);
#endif
            CORESIP_Error err;
            int Qidx = 0;

            if (CORESIP_GetRdQidx(callId, ref Qidx, out err) != 0)
            {
                _Logger.Warn("Error SipAgent.GetRdQidx " + err.Info);
                Qidx = -1;      //Retorna negativo si error
            }
            return Qidx;
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.GetRdResourceInfo");
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callId"></param>
		public static void AddCallToConference(int callId)
		{
			CORESIP_Error err;

#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.AddCallToConference {0}", callId);
#endif
            if (CORESIP_CallConference(callId, 1, out err) != 0)
			{
				throw new Exception(err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.AddCallToConference");
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="callId"></param>
		public static void RemoveCallFromConference(int callId)
		{
			CORESIP_Error err;
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.RemoveCallFromConference {0}", callId);
#endif
            if (CORESIP_CallConference(callId, 0, out err) != 0)
			{
                _Logger.Error("SipAgent.RemoveCallFromConference: " + err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.RemoveCallFromConference");
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="callId"></param>
        /// <param name="info"></param>
		public static void SendConfInfo(int callId, CORESIP_ConfInfo info)
		{
			CORESIP_Error err;
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.SendConfInfo {0}, {1}", callId, info.Version);
#endif

			if (CORESIP_CallSendConfInfo(callId, info, out err) != 0)
			{
                _Logger.Error("SipAgent.SendConfInfo: " + err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.SendConfInfo");
#endif
        }
        /**
         * SendConfInfoFromAcc: Envia Notify con la info de la conferencia a todas las subscripciones al evento
         *						de conferencia que tiene un account
         * @param	accid	Account id
         * @param	conf	Info de de conferencia
         * @return	0 si no hay error
         */
        public static void SendConfInfoFromAcc(string accId, CORESIP_ConfInfo info)
        {
            CORESIP_Error err;
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.SendConfInfo {0}, {1}", accId, info.Version);
#endif

            if (CORESIP_SendConfInfoFromAcc(_Accounts[accId], info, out err) != 0)
            {
                _Logger.Error("SipAgent.SendConfInfoFromAcc: " + err.Info);
            }
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.SendConfInfo");
#endif
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="callId"></param>
        /// <param name="info"></param>
		public static void SendInfo(int callId, string info)
		{
			CORESIP_Error err;

#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.SendInfo {0}, {1}", callId, info);
#endif
            if (CORESIP_CallSendInfo(callId, info, out err) != 0)
			{
                _Logger.Error("SipAgent.SendInfo: " + err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.SendInfo");
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dst"></param>
        /// 
        /**
         *	SendOptionsMsg
         *  Esta función no envia OPTIONS a traves del proxy
         *	@param	dst			Puntero a uri donde enviar OPTIONS
         *  @param	callid		callid que retorna.
         *  @param	isRadio		Si tiene valor distinto de cero el agente se identifica como radio. Si es cero, como telefonia.
         *						Sirve principalmente para poner radio.01 o phone.01 en la cabecera WG67-version
         *	@param	error		Puntero a la Estructura de error
         *	@return				Codigo de Error
         */
		public static void SendOptionsMsg(string dst, out string callid, bool isRadio)
		{
			CORESIP_Error err;
            StringBuilder callid_ = new StringBuilder(CORESIP_MAX_CALLID_LENGTH + 1);            

#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.SendOptionsMsg {0}", dst);
#endif
            int isrd = (isRadio) ? 1 : 0;
            if (CORESIP_SendOptionsMsg(dst, callid_, isrd, out err) != 0)
			{
				throw new Exception(err.Info);
			}
            callid = callid_.ToString();
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.SendOptionsMsg");
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// 
        /**
         * SendOptionsCFWD.	...
         * Envia mensaje OPTIONS necesario para la negociacion Call Forward
         * @param	accId				Account de la Coresip que utilizamos.
         * @param	dst					Uri a la que se envia OPTIONS
         * @param	cfwr_options_type	Tipo de OPTIONS para la negociacion. Es del tipo CORESIP_CFWR_OPT_TYPE
         * @param	body				Contenido del body (XML). Acabado en '\0'
         * @param	callid				callid que se retorna, acabado en '\0'.
         * @param	by_proxy			TRUE si queremos que se envie a través del proxy. Agregara cabecera route
         */
        public static void SendOptionsCFWD(string accId, string dst, CORESIP_CFWR_OPT_TYPE cfwr_options_type, string body, out string callid, bool by_proxy)
        {

            StringBuilder callid_ = new StringBuilder(CORESIP_MAX_CALLID_LENGTH + 1);
#if CALLFORWARD
            CORESIP_Error err;
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.SendOptionsCFWD {0}", dst);
#endif
            if (CORESIP_SendOptionsCFWD(_Accounts[accId], dst, cfwr_options_type, body, callid_, by_proxy, out err) != 0)
            {
                //throw new Exception(err.Info);
                _Logger.Error("Error SipAgent.SendOptionsCFWD" + err.Info);
            }
#endif
            callid = callid_.ToString();
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.SendOptionsCFWD");
#endif

        }

        /// <summary>
        /// 
        /// </summary>
        /// 
        /**
         * CORESIP_SendResponseCFWD.	...
         * Envia la respuesta al options utilizado para la negociacion de call forward
         * @param	st_code				Code de la respuesta. Si no es 200 entonces se ignora el parametro del body
         * @param	body				Contenido del body (XML). Acabado en '\0'
         * @param	hresp				Manejador necesario para enviar la respuesta
         */
        public static void SendResponseCFWD(int st_code, string body, uint hresp)
        {
#if CALLFORWARD
            CORESIP_Error err;

#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.SendResponseCFWD {0}", st_code);
#endif
            if (CORESIP_SendResponseCFWD(st_code, body, hresp, out err) != 0)
            {
                throw new Exception(err.Info);
            }
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.SendResponseCFWD");
#endif
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tsxKey"></param>
        /// <param name="txData"></param>
        /// <param name="evSub"></param>
        /// <param name="code"></param>
		public static void TransferAnswer(string tsxKey, IntPtr txData, IntPtr evSub, int code)
		{
			CORESIP_Error err;

#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.TransferAnswer {0}, {1}, {2}, {3}", tsxKey, txData, evSub, code);
#endif
            if (CORESIP_TransferAnswer(tsxKey, txData, evSub, code, out err) != 0)
			{
                _Logger.Error("SipAgent.TransferAnswer: " + err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.TransferAnswer");
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="evSub"></param>
        /// <param name="code"></param>
		public static void TransferNotify(IntPtr evSub, int code)
		{
			CORESIP_Error err;
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.Transfer {0}, {1}", evSub, code);
#endif
            try
            {
                if (CORESIP_TransferNotify(evSub, code, out err) != 0)
                {
                    _Logger.Error("SipAgent.TransferNotify: " + err.Info);
                }
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.TransferNotify");
#endif
            }
            catch (Exception exc)
            {
                _Logger.Error("SipAgent.TransferNotify: " + exc.Message);
            }
        }

        /**
         *	CreateConferenceSubscription. Crea una subscripcion por evento de conferencia
         *	@param	accId		Identificador del account.
         *  @param  dst.	    Uri del destino al que nos subscribimos
         *  @param	by_proxy.   Si true entonces el subscribe se envia a traves del proxy
         *	@return				Codigo de Error
         */
        public static void CreateConferenceSubscription(string accId, string dst, bool byProxy = true)
        {
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.CreateConferenceSubscription {0}", dst);
#endif
            CORESIP_Error err = new CORESIP_Error();

            if (CORESIP_CreateConferenceSubscription(_Accounts[accId], dst, byProxy, out err) != 0)
            {
                _Logger.Error("Error creating Conference Subscription" + err.Info);
            }

#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.CreateConferenceSubscription");
#endif
            return;
        }

        /**
         *	DestroyConferenceSubscription. Destruye una subscripcion por evento de presencia
         *  @param  dest.	Uri del destino del que nos desubscribimos
         *	@param	error		Puntero a la Estructura de error
         *	@return				Codigo de Error
         */
        public static void DestroyConferenceSubscription(string dst)
        {
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.DestroyConferenceSubscription {0}", dst);
#endif
            CORESIP_Error err = new CORESIP_Error();

            if (CORESIP_DestroyConferenceSubscription(dst, out err) != 0)
            {
                _Logger.Error("Error destroying Conference Subscription" + err.Info);
            }
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.DestroyConferenceSubscription");
#endif
        }

        /**
         *	CreateDialogSubscription. Crea una subscripcion por evento de dialogo
         *	@param	accId		Identificador del account.
         *  @param  dst.	    Uri del destino al que nos subscribimos
         *  @param	by_proxy.   Si true entonces el subscribe se envia a traves del proxy
         *	@return				error
         */
        public static bool CreateDialogSubscription(string accId, string dst, bool byProxy = false)
        {
            bool error = false;
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.CreateDialogSubscription {0}", dst);
#endif
            CORESIP_Error err = new CORESIP_Error();
            if (CORESIP_CreateDialogSubscription(_Accounts[accId], dst, byProxy, out err) != 0)
            {
                _Logger.Error("Error creating Dialog Subscription" + err.Info);
                error = true;
            }

#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.CreateDialogSubscription");
#endif
            return error;
        }

        /**
         *	DestroyDestroySubscription. Destruye una subscripcion por evento de dialogo
         *  @param  dest.	Uri del destino del que nos desubscribimos
         *	@return				Codigo de Error
         */
        public static void DestroyDialogSubscription(string dst)
        {
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.DestroyDialogSubscription {0}", dst);
#endif
            CORESIP_Error err = new CORESIP_Error();

            if (CORESIP_DestroyDialogSubscription(dst, out err) != 0)
            {
                _Logger.Error("Error destroying Dialog Subscription" + err.Info);
            }
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.DestroyDialogSubscription");
#endif
        }

        /**
          *	SendInstantMessage. Destruye una subscripcion por evento de dialogo
          *  @param  dest.	Uri del destino del que nos desubscribimos
          *	@return				Codigo de Error
          */
        public static void SendInstantMessage(string accId, string dest_uri, string text, bool by_proxy)
        {
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.SendInstantMessage {0}", dest_uri);
#endif
            CORESIP_Error error = new CORESIP_Error();
            CORESIP_SendInstantMessage(_Accounts[accId], dest_uri, text, by_proxy, out error);
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.SendInstantMessage");
#endif
        }
        /** AGL */
        public static void Wav2Remote(string file, string id, string ip, int port)
        {
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.WavToRemote {0}, {1}, {2}, {3}", file, id, ip, port);
#endif
            CORESIP_Error err = new CORESIP_Error();
            WavRemoteEnd cb = new WavRemoteEnd(Wav2RemoteEnd);
            CORESIP_Wav2RemoteStart(file, id, ip, port, ref cb, ref err);
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.Wav2Remote");
#endif
        }

        /** */
        public static void Wav2RemoteEnd(IntPtr obj)
        {
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.WavToRemoteEnd {0}", obj);
#endif
            CORESIP_Error err = new CORESIP_Error();
            CORESIP_Wav2RemoteEnd(obj, ref err);
            /** Meter un Evento.... */
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.WavToRemoteEnd");
#endif
        }

        // JCAM. 20170324
        /* GRABACION VOIP CFG */
        //#3267 RQF22 211217 RQF2
        public static void PictRecordingCfg(string ipRecorder1, string ipRecorder2,
                           int rtspPort, int rtspPort2, bool EnableGrabacionED137)
        {
            CORESIP_Error err;
            bool result = false;
            bool changes = false;

            String fullRecorderFileName = Settings.Default.RecorderServicePath + "\\"+ SipAgent.UG5K_REC_CONF_FILE;
            if (!Directory.Exists(Settings.Default.RecorderServicePath))
            {
                // Try to create the directory.
                Directory.CreateDirectory(Settings.Default.RecorderServicePath);
            }
            // Actualizar el fichero INI que maneja el módudo de grabación
            try
            {
                result = Native.Kernel32.WritePrivateProfileString("GENERAL", "DUAL_RECORDER", (!ipRecorder1.Equals("") && !ipRecorder2.Equals("")) ? "1" : "0",
                    fullRecorderFileName);
                string ip_rec_a = GetSeccionClave("RTSP", "IP_REC_A", fullRecorderFileName);
                string ip_rec_b = GetSeccionClave("RTSP", "IP_REC_B", fullRecorderFileName);
                string port_rtsp = GetSeccionClave("RTSP", "PORT_RTSP", fullRecorderFileName);
                string port_rtsp2 = GetSeccionClave("RTSP", "PORT_RTSP2", fullRecorderFileName);
                bool enablegrabacionED137 = GetSeccionClave("RTSP", "EnableGrabacionED137", fullRecorderFileName).CompareTo("False") == 0 ? false :true;
                if (ip_rec_a != ipRecorder1 ||
                    ip_rec_b != ipRecorder2 ||
                    port_rtsp != rtspPort.ToString() ||
                    port_rtsp2 != rtspPort2.ToString() ||
                    enablegrabacionED137 != EnableGrabacionED137)
                    changes = true;
                
                result |= Native.Kernel32.WritePrivateProfileString("GENERAL", "MAX_SESSIONS", "2", fullRecorderFileName);
                result |= Native.Kernel32.WritePrivateProfileString("SERVICIO", "IP_SERVICIO", "127.0.0.1", fullRecorderFileName);
                result |= Native.Kernel32.WritePrivateProfileString("SERVICIO", "PORT_IN_SERVICIO", "65003", fullRecorderFileName);
                result |= Native.Kernel32.WritePrivateProfileString("RTSP", "IP_REC_A", ipRecorder1 ?? string.Empty, fullRecorderFileName);
                result |= Native.Kernel32.WritePrivateProfileString("RTSP", "IP_REC_B", ipRecorder2 ?? string.Empty, fullRecorderFileName);
                result |= Native.Kernel32.WritePrivateProfileString("RTSP", "PORT_RTSP", rtspPort.ToString(), fullRecorderFileName);
                result |= Native.Kernel32.WritePrivateProfileString("RTSP", "PORT_RTSP2", rtspPort2.ToString(), fullRecorderFileName);
                //RQF24
                result |= Native.Kernel32.WritePrivateProfileString("RTSP ", "EnableGrabacionED137 ", EnableGrabacionED137.ToString(), fullRecorderFileName);
            }
            catch (Exception exc)
            {
                _Logger.Error("Error escribiendo fichero UG5K_REC_CONF_FILE: {0} exception {1} !!!", Marshal.GetLastWin32Error(), exc.Message);
            }
            _Logger.Debug("Modificado fichero {4} IP_REC_A:{0} IP_REC_B:{1} PORTS_RTSP:{2},{3} DUAL_RECORDER:{4}", ipRecorder1 ?? string.Empty, ipRecorder2 ?? string.Empty, rtspPort, rtspPort2, (ipRecorder1 == null && ipRecorder2 == null) ? "0" : "1", fullRecorderFileName);
            if (result == false)
            {
                _Logger.Error("Error escribiendo fichero UG5K_REC_CONF_FILE: {0} !!!", Marshal.GetLastWin32Error());
            }
            if (changes==true)
            {
                // Notificar al módulo de grabación que ha cambiado la configuración.
                if (CORESIP_RecorderCmd(CORESIP_RecCmdType.CORESIP_REC_RESET, out err) != 0)
                {
                    throw new Exception(err.Info);
                }
                _Logger.Debug("Notificado al módulo de GrabaciónEd137 {0}",EnableGrabacionED137);
            }
        }

        //RQF24
        public static int GetGrabacionED137()
        {
            uint result = 0;
            bool bresultado;
            int resultado=0;
            String fullRecorderFileName = Settings.Default.RecorderServicePath + "\\" + SipAgent.UG5K_REC_CONF_FILE;
            StringBuilder sGrabaciónED137 = new StringBuilder(10);
            try
            {
                result = Native.Kernel32.GetPrivateProfileString("RTSP", "EnableGrabacionED137", "", sGrabaciónED137, 10, fullRecorderFileName);
            }
            catch (Exception exc)
            {
                _Logger.Error("Error leyendo fichero UG5K_REC_CONF_FILE: {0} exception {1} !!!", Marshal.GetLastWin32Error(), exc.Message);
            }
            _Logger.Debug("Leyendo fichero {1} :{0}", sGrabaciónED137, fullRecorderFileName);
            if (result == 0)
            {
                _Logger.Error("Error leyendo fichero UG5K_REC_CONF_FILE: {0} !!!", Marshal.GetLastWin32Error());
            }

            bool.TryParse(sGrabaciónED137.ToString(), out bresultado);
            if (bresultado == true) return resultado=1;
            else resultado=0;
            return resultado;
        }

        public static bool GetEnableGrabacionED137()
        {
            uint result = 0;
            bool resultado;
            String fullRecorderFileName = Settings.Default.RecorderServicePath + "\\" + SipAgent.UG5K_REC_CONF_FILE;
            StringBuilder sEnableGrabacionED137 = new StringBuilder(10);
            try
            {
                result = Native.Kernel32.GetPrivateProfileString("RTSP ", "EnableGrabacionED137 ", "", sEnableGrabacionED137, 10, fullRecorderFileName);
            }
            catch (Exception exc)
            {
                _Logger.Error("Error leyendo fichero UG5K_REC_CONF_FILE: {0} exception {1} !!!", Marshal.GetLastWin32Error(), exc.Message);
            }
            _Logger.Debug("Leyendo fichero {1} :{0}", sEnableGrabacionED137, fullRecorderFileName);
            if (result <= 0)
            {
                _Logger.Error("Error leyendo fichero UG5K_REC_CONF_FILE: {0} !!!", Marshal.GetLastWin32Error());
            }

            bool.TryParse(sEnableGrabacionED137.ToString(), out resultado);
            return resultado;
        }

        //RQF24-RQF22
        public static string GetSeccionClave(string seccion,string clave, string fullRecorderFileName)
        {
            uint result = 0;
            //String fullRecorderFileName = Settings.Default.RecorderServicePath + "\\" + SipAgent.UG5K_REC_CONF_FILE;
            StringBuilder retorno  = new StringBuilder(50);
            try
            {
                result = Native.Kernel32.GetPrivateProfileString(seccion,clave, "", retorno, 50, fullRecorderFileName);
            }
            catch (Exception exc)
            {
                _Logger.Error("Error leyendo fichero UG5K_REC_CONF_FILE: {0} exception {1} !!!", Marshal.GetLastWin32Error(), exc.Message);
            }
            _Logger.Debug("Leyendo fichero {1} :{0}", retorno, fullRecorderFileName);
            if (result < 0)
            {
                _Logger.Error("Error leyendo fichero UG5K_REC_CONF_FILE: {0} !!!", seccion,clave);
            }

            return retorno.ToString();
        }


        /* GRABACION VOIP START */
        /** */
        public static void RdPttEvent(bool on, string freqId, int dev, uint priority)
        {
            CORESIP_Error err;
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.RdPttEvent {0}, {1}", freqId, dev);
#endif
            if (CORESIP_RdPttEvent(on, freqId, dev, out err, (CORESIP_PttType) priority) != 0)
            {
                throw new Exception(err.Info);
            }
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.RdPttEvent");
#endif
        }

        // JCAM. 20170323
        // RQ.012.04
        public static void RdSquEvent(bool on, string freqId, string qidxResource, string qidxMethod, uint qidxValue)
        {
            CORESIP_Error err;
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.RdSquEvent {0} Resource: {1} QidxMethod:{2} QidxValue:{3}", freqId,qidxResource,qidxMethod,qidxValue);
#endif
            if (CORESIP_RdSquEvent(on, freqId, qidxResource, qidxMethod, qidxValue, out err) != 0)
            {
                throw new Exception(err.Info);
            }
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.RdSquEvent");
#endif
        }

        /** 20180131. AGL. Para la gestion de presencia.... */
        //public static void SetPresenceSubscriptionCallBack(SubPresCb Presence_callback)
        //{
        //    CORESIP_Error err;
        //    if (CORESIP_SetPresenceSubscriptionCallBack(Presence_callback, out err) != 0)
        //    {
        //        throw new Exception(err.Info);
        //    }
        //}
        public static void CreatePresenceSubscription(string dest_uri)
        {
            CORESIP_Error err;
            if (CORESIP_CreatePresenceSubscription(dest_uri, out err) != 0)
            {
                throw new Exception(err.Info);
            }
        }
        public static void DestroyPresenceSubscription(string dest_uri)
        {
            CORESIP_Error err;
            if (CORESIP_DestroyPresenceSubscription(dest_uri, out err) != 0)
            {
                throw new Exception(err.Info);
            }
        }
        /*******************************/


        /* GRABACION VOIP END */

        /**
         * EchoCancellerLCMic.	...
         * Activa/desactiva cancelador de eco altavoz LC y Microfonos. Sirve para el modo manos libres 
         * Por defecto esta desactivado en la CORESIP
         * @param	on						true - activa / false - desactiva         
         */
        public static void EchoCancellerLCMic(bool on)
        {
            CORESIP_Error err;
            if (on != ECHandsFreeState)
            {
                ECHandsFreeState = on;
                if (CORESIP_EchoCancellerLCMic(on, out err) != 0)
                {
                    throw new Exception(err.Info);
                }
            }
        }

#region Private Members
        /// <summary>
        /// 
        /// </summary>
		private static Logger _Logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// 
        /// </summary>
		private static CORESIP_Callbacks _Cb = new CORESIP_Callbacks();
        /// <summary>
        /// 
        /// </summary>
		private static Dictionary<string, int> _Accounts = new Dictionary<string, int>();
        /// <summary>
        /// 
        /// </summary>
		private static string _Ip = null;
        public static string IP
        {
            get { return _Ip; }
        }
        /// <summary>
        /// 
        /// </summary>
		private static uint _Port = 0;

        /// <summary>
        /// Guarda el ultimo estado el ECHandsFree pedido a CORESIP, para evitar llamadas innecesarias
        /// Por defecto tiene el mismo valor que la CORESIP (false)
       /// </summary>
        private static bool ECHandsFreeState = false;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="level"></param>
        /// <param name="data"></param>
        /// <param name="len"></param>
		private static void OnLogCb(int level, string data, int len)
		{
			LogLevel eqLevel = level < 7 ? LogLevel.FromOrdinal(6 - level) : LogLevel.Off;

            //_Logger.Log(eqLevel, data);
            _Logger.Debug(data);
		}

        /** 20180208. Para que convivan mas de un proceso con la misma CORESIP */
        private static void CallbacksInit()
        {
            if (Settings.Default.SipLogLevel <= 3)
		        _Cb.OnLog = new LogCb((p1, p2, p3) => 
                {                
                        OnLogCb(p1, p2, p3);
                });
            _Cb.OnKaTimeout = new KaTimeoutCb((p1) =>
            {
                if (OnKaTimeout != null)
                    OnKaTimeout(p1);
            });
            _Cb.OnRdInfo = new RdInfoCb((p1, p2) =>
            {
                if (OnRdInfo != null)
                    OnRdInfo(p1, p2);
            });
            _Cb.OnCallState = new CallStateCb((p1, p2, p3) =>
            {
                if (OnCallState != null)
                    OnCallState(p1, p2, p3);
            });
            _Cb.OnCallIncoming = new CallIncomingCb((p1, p2, p3, p4) =>
            {
                if (OnCallIncoming != null)
                    OnCallIncoming(p1, p2, p3, p4);
            });
            _Cb.OnTransferRequest = new TransferRequestCb((p1, p2, p3) =>
            {
                if (OnTransferRequest != null)
                    OnTransferRequest(p1, p2, p3);
            });
            _Cb.OnTransferStatus = new TransferStatusCb((p1, p2) =>
            {
                if (OnTransferStatus != null)
                    OnTransferStatus(p1, p2);
            });
            _Cb.OnConfInfo = new ConfInfoCb((p1, p2, p3, p4) =>
            {
                //p1 puede ser un callId o el identificador de la cuenta de SIP
                if ((p1 & SipAgent.CORESIP_ID_TYPE_MASK) == SipAgent.CORESIP_ACC_ID)
                {
                    foreach (string accountName in _Accounts.Keys)
                        if (_Accounts[accountName] == p1)
                        {
                            if (OnConfInfoAcc != null)
                                OnConfInfoAcc(accountName, p2, p3, p4);
                            break;
                        }
                }
                else
                if (OnConfInfo != null)
                    OnConfInfo(p1, p2, p3, p4);
            });
            _Cb.OnDialogNotify = new DialogNotifyCb((p1, p2) =>
            {
                if (OnDialogNotify != null)
                    OnDialogNotify(p1, p2);
            });
            _Cb.OnPager = new PagerCb((p1, p2, p3, p4, p5, p6, p7, p8, p9, p10) =>
            {
                if (OnPager != null)
                    OnPager(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10);
            });
            _Cb.OnOptionsReceive = new OptionsReceiveCb((p1, p2, p3, p4, p5) =>
            {
                if (OnOptionsReceive != null)
                    OnOptionsReceive(p1, p2, p3, p4, p5);
            });

            _Cb.OnCfwrOptReceived = new CfwrOptReceivedCb((p1, p2, p3, p4, p5) =>
            {
                foreach (string accountName in _Accounts.Keys)
                    if (_Accounts[accountName] == p1)
                    {
                        if (OnCfwrOptReceived != null)
                            OnCfwrOptReceived(accountName, p2, p3, p4, p5);
                        break;
                    }
            });

            _Cb.OnCfwrOptResponse = new CfwrOptResponseCb((p1, p2, p3, p4, p5, p6) =>
            {
                foreach (string accountName in _Accounts.Keys)
                    if (_Accounts[accountName] == p1)
                    {
                        if (OnCfwrOptResponse != null)
                            OnCfwrOptResponse(accountName,p2, p3, p4, p5, p6);
                        break;
                    }
            });

            _Cb.OnMovedTemporally = new MovedTemporallyCb((p1, p2) =>
            {
                if (OnMovedTemporally != null)
                    OnMovedTemporally(p1, p2);
            });
            _Cb.OnWG67SubscriptionState = new WG67SubscriptionStateCb((p1) =>
            {
                if (OnWG67SubscriptionState != null)
                    OnWG67SubscriptionState(p1);
            });
            _Cb.OnWG67SubscriptionReceived = new WG67SubscriptionReceivedCb((p1, p2) =>
            {
                if (OnWG67SubscriptionReceived != null)
                    OnWG67SubscriptionReceived(p1, p2);
            });
            _Cb.OnInfoReceived = new InfoReceivedCb((p1, p2, p3) =>
            {
                if (OnInfoReceived != null)
                    OnInfoReceived(p1, p2, p3);
            });
            _Cb.OnIncomingSubscribeConf = new IncomingSubscribeConfCb((p1, p2, p3) =>
            {
                //p1 puede ser un callId o el identificador de la cuenta de SIP
                if ((p1 & SipAgent.CORESIP_ID_TYPE_MASK) == SipAgent.CORESIP_ACC_ID)
                {                        
                    foreach (string accountName in _Accounts.Keys)
                        if (_Accounts[accountName] == p1)
                        {
                            if (OnIncomingSubscribeConfAcc != null)
                                OnIncomingSubscribeConfAcc(accountName, p2, p3);
                            break;
                        }                        
                }
                else
                if (OnIncomingSubscribeConf != null)
                    OnIncomingSubscribeConf(p1, p2, p3);
            });
            _Cb.OnSubPres = new SubPresCb((p1, p2, p3) =>
            {
                if (OnSubPres != null)
                    OnSubPres(p1, p2, p3);
            });
            _Cb.OnFinWavCb = null;
        }
#endregion
	}
}
