#define _CORESIP_1_
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using U5ki.Infrastructure;
using ProtoBuf;
using Utilities;

#if _CORESIP_2_
using CoreSipNet;
#endif

namespace u5ki.PhoneService
{
    public class PhoneService : BaseCode, IService
    {
        #region IService Interfaz

        public string Name { get; set; } 

        public ServiceStatus Status { get; set; }

        public bool Master { get; set; }

        public bool Commander(ServiceCommands cmd, string par, ref string err, List<string> resp = null)
        {
            throw new NotImplementedException();
        }

        public bool DataGet(ServiceCommands cmd, ref List<object> rsp)
        {
            if (smpAccesMain.Acquire())
            {
                List<object> lista = new List<object>();
                lista.Add(ServicesHelpers.SerializeObject(new
                {
                    id = Name,                                   // TODO. Construir los datos que se quieran obtener del servicio....
                    status = Status,
                    mode = Master ? "Master" : "Slave",
                }));
                rsp = new List<object>(lista);
                smpAccesMain.Release();
                return true;
            }
            return false;
        }

        public void Start()
        {
            if (smpAccesMain.Acquire())
            {

                try
                {
                    LogInfo<PhoneService>("Iniciando Servicio...");

                    ResourcesSetup();

                    LogInfo<PhoneService>("Servicio Iniciado.",
                        U5kiIncidencias.U5kiIncidencia.IGRL_U5KI_NBX_INFO, "PhoneService",
                        Translate.CTranslate.translateResource("Servicio iniciado."));          // TODO... Translate....
                }
                catch (Exception x)
                {
                    ResourcesRelease();
                    LogException<PhoneService>("Excepcion arrancando servicio", x, false);
                }
                finally
                {
                    smpAccesMain.Release();
                }
            }
        }

        public void Stop()
        {
            if (smpAccesMain.Acquire())
            {
                try
                {
                    LogInfo<PhoneService>("Iniciando parada servicio");

                    ResourcesRelease();
                }
                catch (Exception x)
                {
                    LogException<PhoneService>("Excepcion deteniendo servicio", x, false);
                }
                finally
                {
                    LogInfo<PhoneService>("Servicio Detenido.", U5kiIncidencias.U5kiIncidencia.IGRL_U5KI_NBX_INFO,
                        "PhoneService", Translate.CTranslate.translateResource("Servicio detenido."));               // TODO. Translate.
                    smpAccesMain.Release();
                }
            }
        }

        #endregion IService Interfaz

        #region Contructores
        public PhoneService()
        {
            Name = "PhoneService";
            Status = ServiceStatus.Stopped;
            Master = false;
            LastVersion = String.Empty;
            SipAgentRunning = false;
            TraceControl = new ServicesHelpers.TraceInOut<PhoneService>();
        }
        #endregion

        #region Gestion Global del Servicio.

        /// <summary>
        /// Se Ejecuta en el START...
        /// </summary>
        private void ResourcesSetup()
        {
            Int64 tcontrol = TraceControl.TraceIn();

            Master = false;
            LastVersion = String.Empty;
            Status = ServiceStatus.Running;


            /** Inicializacion de las comunicaciones por MULTICAST */
            _Registry = new Registry(Identifiers.PhoneMasterTopic);

            _Registry.ChannelError += OnChannelError;
            _Registry.MasterStatusChanged += OnMasterStatusChanged;
            _Registry.ResourceChanged += OnResourceChanged;

            _Registry.SubscribeToMasterTopic(Identifiers.PhoneMasterTopic);
            _Registry.SubscribeToTopic<SrvMaster>(Identifiers.PhoneMasterTopic);
            _Registry.SubscribeToTopic<Cd40Cfg>(Identifiers.CfgTopic);
            _Registry.Join(Identifiers.PhoneMasterTopic, Identifiers.PhoneTopic, Identifiers.CfgTopic);

            /** */
            SipAgentStart();

            /** Otras Inicializaciones */
            TraceControl.TraceOut(tcontrol);
        }

        /// <summary>
        /// Se ejecuta en el STOP...
        /// </summary>
        private void ResourcesRelease()
        {
            Int64 tcontrol = TraceControl.TraceIn();

            if (Master == true)
            {
                SipAgentStop();
                ServiceDeactivate();
            }

            if (_Registry != null)
            {
                _Registry.Dispose();
                _Registry = null;
            }

            // TODO. Limpiar los demas Recursos...
            TraceControl.TraceOut(tcontrol);
        }

        /// <summary>
        /// Se ejectua al pasar a MASTER.
        /// </summary>
        private void ServiceActivate()
        {
            Int64 tcontrol = TraceControl.TraceIn();

            // TODO... Inicializar variables,
            SipAgentSetup();

            TraceControl.TraceOut(tcontrol);
        }
        /// <summary>
        /// Se ejecuta al pasar a SLAVE
        /// </summary>
        private void ServiceDeactivate()
        {
            Int64 tcontrol = TraceControl.TraceIn();

            // TODO..., 
            SipAgentRelease();

            TraceControl.TraceOut(tcontrol);
        }
        /// <summary>
        /// Se ejecuta al recibir una nueva configuracion...
        /// </summary>
        /// <param name="cfg"></param>
        private void ServiceSetupConfig(Cd40Cfg cfg)
        {
            Int64 tcontrol = TraceControl.TraceIn();
            // TODO...

            TraceControl.TraceOut(tcontrol);
        }
        /// <summary>
        /// Se ejecuta como rutina de limpieza de la configuracion antes de cargar una nueva...
        /// </summary>
        private void ServiceReleaseConfig()
        {
            Int64 tcontrol = TraceControl.TraceIn();

            // TODO...
            TraceControl.TraceOut(tcontrol);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="error"></param>
        private void OnChannelError(object sender, string error)
        {
            Int64 tcontrol = TraceControl.TraceIn();

            ProcessGlobalEvent(() =>
            {

                TraceControl.TraceOut(tcontrol);
            }, (x) =>
            {
                LogException<PhoneService>("OnCallState  Exception", x, false);
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="master"></param>
        private void OnMasterStatusChanged(object sender, bool master)
        {
            Int64 tcontrol = TraceControl.TraceIn();
            ProcessGlobalEvent(() =>
            {
                if (master && !Master)
                {
                    ServiceActivate();
                }
                else if (!master && Master)
                {
                    ServiceDeactivate();
                }
                Master = master;
                LastVersion = string.Empty;

                LogInfo<PhoneService>(Master ? "MASTER" : "SLAVE",
                    U5kiIncidencias.U5kiIncidencia.IGRL_U5KI_NBX_INFO, "PhoneService",
                    Master ? "MASTER" : "SLAVE");

                TraceControl.TraceOut(tcontrol);
            }, (x) =>
            {
                LogException<PhoneService>("OnMasterStatusChanged to " + master.ToString() + " Exception", x, false);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnResourceChanged(object sender, RsChangeInfo e)
        {
            Int64 tcontrol = TraceControl.TraceIn();
            ProcessGlobalEvent(() =>
            {
                if (Master && e.Content != null && e.Type == Identifiers.TypeId(typeof(Cd40Cfg)))
                {
                    MemoryStream ms = new MemoryStream(e.Content);
                    Cd40Cfg cfg = Serializer.Deserialize<Cd40Cfg>(ms);

                    if (LastVersion == cfg.Version)
                        return;

                    LastVersion = cfg.Version;
                    LogInfo<PhoneService>("Procesando Configuracion: " + LastVersion);

                    ServiceReleaseConfig();
                    ServiceSetupConfig(cfg);
                }

                TraceControl.TraceOut(tcontrol);
            }, (x) =>
            {
                LogException<PhoneService>("OnResourceChanged Exception", x, false);
            });
        }

        /// <summary>
        /// Para gestionar los eventos globales genera una thread y obtiene el acceso global a los datos...
        /// </summary>
        /// <param name="mainRoutine"></param>
        /// <param name="errorRoutine"></param>
        private void ProcessGlobalEvent(Action mainRoutine, Action<Exception> errorRoutine)
        {
            Task.Factory.StartNew(() =>
            {
                if (smpAccesMain.Acquire())
                {
                    try
                    {
                        mainRoutine();
                    }
                    catch (Exception x)
                    {
                        errorRoutine(x);
                    }
                    finally
                    {
                        smpAccesMain.Release();
                    }
                }
                else
                {
                    errorRoutine(new ApplicationException("Timeout en Semaforo de control de acceso global."));
                }
            });
        }

        #endregion

        #region Control de Agente SIP y eventos asociados
        /// <summary>
        /// 
        /// </summary>
        void SipAgentStart()
        {
            Int64 tcontrol = TraceControl.TraceIn();

            if (SipAgentRunning==false)
            {
                using (Config.LocalSettings settings = new Config.LocalSettings())
                {
                    settings.Init();
#if _CORESIP_2_
                    SipAgentNetSettings sipSettings = new SipAgentNetSettings()
                    {
                        Default = new SipAgentNetSettings.DefaultSettings()
                        {
                            DefaultCodec = settings.Sip.DefaultCodec,
                            DefaultDelayBufPframes = settings.Sip.DefaultDelayBufPframes,
                            DefaultJBufPframes = settings.Sip.DefaultJBufPframes,
                            SndSamplingRate = settings.Sip.SndSamplingRate,
                            TxLevel = settings.Sip.TxLevel,
                            RxLevel = settings.Sip.RxLevel,
                            SipLogLevel = settings.Sip.SipLogLevel,
                            TsxTout = settings.Sip.TsxTout,
                            InvProceedingDiaTout = settings.Sip.InvProceedingDiaTout,
                            InvProceedingIaTout = settings.Sip.InvProceedingIaTout,
                            InvProceedingMonitoringTout = settings.Sip.InvProceedingMonitoringTout,
                            InvProceedingRdTout = settings.Sip.InvProceedingRdTout,
                            KAMultiplier = settings.Sip.KAMultiplier,
                            KAPeriod = settings.Sip.KAPeriod
                        }
                    };
                    SipAgentNet.Init(sipSettings, settings.Sip.AgentName, settings.Ip, settings.Sip.UdpPort, 1000);
                    SipAgentNet.Start();
#endif
                    SipAgentSetup();
                    SipAgentRunning = true;
                }
            }

            TraceControl.TraceOut(tcontrol);
        }
        /// <summary>
        /// 
        /// </summary>
        void SipAgentSetup()
        {
            Int64 tcontrol = TraceControl.TraceIn();
#if _CORESIP_2_
            SipAgentNet.CallIncoming += OnCallIncoming;
            SipAgentNet.CallState += OnCallState;
#else
            SipAgent.CallIncoming += OnCallIncoming;
            SipAgent.CallState += OnCallState;
#endif
            TraceControl.TraceOut(tcontrol);
        }
        /// <summary>
        /// 
        /// </summary>
        void SipAgentRelease()
        {
            Int64 tcontrol = TraceControl.TraceIn();

#if _CORESIP_2_
            SipAgentNet.CallState -= OnCallState;
            SipAgentNet.CallIncoming -= OnCallIncoming;
#else
            SipAgent.CallState -= OnCallState;
            SipAgent.CallIncoming -= OnCallIncoming;
#endif

            TraceControl.TraceOut(tcontrol);
        }
        /// <summary>
        /// 
        /// </summary>
        void SipAgentStop()
        {
            Int64 tcontrol = TraceControl.TraceIn();

            if (SipAgentRunning == true)
            {
                SipAgentRunning = false;
                SipAgentRelease();
                //SipAgentNet.End();
            }

            TraceControl.TraceOut(tcontrol);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="call"></param>
        /// <param name="info"></param>
        /// <param name="stateInfo"></param>
#if _CORESIP_2_
        void OnCallState(int call, CoreSipNet.CORESIP_CallInfo info, CoreSipNet.CORESIP_CallStateInfo stateInfo)
#else
        void OnCallState(int call, CORESIP_CallInfo info, CORESIP_CallStateInfo stateInfo)
#endif
        {
            Int64 tcontrol = TraceControl.TraceIn();

            ProcessGlobalEvent(() =>
            {

                TraceControl.TraceOut(tcontrol);
            }, (x) =>
            {
                LogException<PhoneService>("OnCallState  Exception", x, false);
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="call"></param>
        /// <param name="call2replace"></param>
        /// <param name="info"></param>
        /// <param name="inInfo"></param>
#if _CORESIP_2_
        void OnCallIncoming(int call, int call2replace, CoreSipNet.CORESIP_CallInfo info, CoreSipNet.CORESIP_CallInInfo inInfo)
#else
        void OnCallIncoming(int call, int call2replace, CORESIP_CallInfo info, CORESIP_CallInInfo inInfo)
#endif
        {
            Int64 tcontrol = TraceControl.TraceIn();

            ProcessGlobalEvent(() =>
            {

                TraceControl.TraceOut(tcontrol);
            }, (x) =>
            {
                LogException<PhoneService>("OnCallIncoming  Exception", x, false);
            });
        }

        #endregion

        #region Private Attributes

        /// <summary>
        /// Semaforo para el acceso global a los recursos de los servicios....
        /// </summary>
        private ServicesHelpers.ManagedSemaphore smpAccesMain = new ServicesHelpers.ManagedSemaphore(1, 1, "PhoneServiceAccess", 5000);
        /// <summary>
        /// 
        /// </summary>
        private Registry _Registry=null;
        /// <summary>
        /// 
        /// </summary>
        private String LastVersion { get; set; }
        /// <summary>
        /// 
        /// </summary>
        private bool SipAgentRunning { get; set; }
        /// <summary>
        /// 
        /// </summary>
        private ServicesHelpers.TraceInOut<PhoneService> TraceControl { get; set; }

        #endregion Private Attributes

    }
}
