using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using U5ki.Infrastructure;
using U5ki.CfgService.Properties;

using Utilities;
using ProtoBuf;
using NLog;

using Translate;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.Services.Protocols;

namespace U5ki.CfgService
{
    /// <summary>
    /// 
    /// </summary>
    internal class CfgService : BaseCode, IService
    {
        /// <summary>
        /// 
        /// </summary>
        public CfgService()
        {
        }

        #region IService Members
        /// <summary>
        /// 
        /// </summary>
        public string Name
        {
            get { return "Cd40ConfigService"; }
        }
        /// <summary>
        /// 
        /// </summary>
        public ServiceStatus Status
        {
            get { return _Status; }
        }

        /**
         * AGL 20120706. Para la Interfaz de control
         * */
        /// <summary>
        /// 
        /// </summary>
        public bool Master
        {
            get { return _Master; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmd"></param>
        /// <param name="resp"></param>
        public bool Commander(ServiceCommands cmd, string par, ref string err, List<string> resp = null)
        {
            string spar = (string)par;
#if _LOCKING_
            lock (_lock)
            {
#endif
            switch (cmd)
            {
                case ServiceCommands.LoadDefaultCfg:

                    if (_Master == false)
                    {
                        err = "El servicio no esta en modo MASTER";
                    }
                    else if (File.Exists(DefaultCfgFile(spar)))
                    {
                        // using (var file = File.OpenRead(_DefaultCfgFile))
                        using (FileStream file = File.OpenRead(DefaultCfgFile(spar)))
                        {
                            _LastCfg = Serializer.Deserialize<Cd40Cfg>(file);

                            _Registry.SetValue(Identifiers.CfgTopic, Identifiers.CfgRsId, _LastCfg);
                            _Registry.Publish(_LastCfg.Version);    

                            // y Lo salvo en el fichero de última configuracion.
                            using (FileStream file1 = File.Create(_LastCfgFile))
                            {
                                // Serializer.Serialize(file1, _LastCfgFile);
                                Serializer.Serialize(file1, _LastCfg);
                            }
                        }
                        LogInfo<CfgService>("Activada configuración " + spar, U5kiIncidencias.U5kiIncidencia.IGRL_U5KI_NBX_INFO,
                            "CfgService", CTranslate.translateResource("Activada configuración " + spar));
                        return true;
                    }
                    else
                    {
                        err = "No existe la configuracion " + DefaultCfgFile(spar);
                    }
                    break;

                case ServiceCommands.SetDefaultCfg:
                    if (_Master == false)
                    {
                        err = "El servicio no esta en modo MASTER";
                    }

                    else if (_LastCfg != null)
                    {
                        // using (var file = File.Create(_DefaultCfgFile))
                        using (FileStream file = File.Create(DefaultCfgFile(spar)))
                        {
                            Serializer.Serialize(file, _LastCfg);
                        }
                        // _Registry.Send<string>(Identifiers.CfgTopic, Identifiers.CFG_SAVE_AS_DEFAULT_MSG, "");
                        _Registry.Send<string>(Identifiers.CfgTopic, Identifiers.CFG_SAVE_AS_DEFAULT_MSG, spar);
                        LogInfo<CfgService>("Generada configuración " + spar + " por defecto...", U5kiIncidencias.U5kiIncidencia.IGRL_U5KI_NBX_INFO,
                            "CfgService", CTranslate.translateResource("Generada configuración " + spar + "por defecto..."));
                        return true;
                    }
                    else
                    {
                        err = "No hay ninguna configuracion cargada...";
                    }
                    break;

                case ServiceCommands.ListDefaultCfg:
                    break;

                case ServiceCommands.DelDefaultCfg:
                    string file_name = DefaultCfgFile((string)par);
                    if (_Master == false)
                    {
                        err = "El servicio no esta en modo MASTER";
                    }
                    else if (File.Exists(file_name))
                    {
                        File.Delete(file_name);

                        _Registry.Send<string>(Identifiers.CfgTopic, Identifiers.CFG_DEL_DEFAULT, file_name);
                        LogInfo<CfgService>("Configuración " + spar + " eliminada...", U5kiIncidencias.U5kiIncidencia.IGRL_U5KI_NBX_INFO,
                            "CfgService", CTranslate.translateResource("Configuración " + spar + " eliminada..."));
                        return true;
                    }
                    else
                    {
                        err = "El Fichero: " + file_name + " no existe"; ;
                    }
                    break;

                case ServiceCommands.GetDefaultConfigId:
                    if (_Master == false)
                    {
                        err = "El servicio no esta en modo MASTER";
                    }
                    else if (_LastCfg != null)
                    {
                        if (resp != null)
                            resp.Add(_LastCfg.Version);
                        err = _LastCfg.Version;
                        return true;
                    }
                    else
                    {
                        err = "No hay ninguna configuracion cargada...";
                    }
                    break;

                default:
                    break;
            }
#if _LOCKING_
            }
#endif

            return false;
        }
        /**
         * Fin del cambio */

        /** 20170217. AGL. Nueva interfaz de comandos. Orientada a estructuras definidas en 'Infraestructure' */
        public bool DataGet(ServiceCommands cmd, ref List<Object> rsp)
        {
            return false;
        }
        /** Fin de la Modificacion */
        public object AllDataGet()
        {
            return new
            {
                std = Status.ToString(),
                level = Status != ServiceStatus.Running ? "Error" : Master == true ? "Master" : "Slave",
                cfg_activa = _Master == false ? "El servicio no esta en modo MASTER" :
                    _LastCfg != null ? _LastCfg.Version : "No hay ninguna configuracion cargada...",
            };
        }

        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            try
            {
#if _LOCKING_
                lock (_lock)
                {
#endif

                LogInfo<CfgService>("Iniciando Servicio...");
                ExceptionManageInit();

                _Master = false;
                _LastCfg = null;
                _Status = ServiceStatus.Running;

                _WorkingThread.Start();

                _CheckCfg.AutoReset = false;
                _CheckCfg.Elapsed += OnCheckCfg;

                InitRegistry();

                _CheckCfg.Enabled = true;
                LogInfo<CfgService>("Servicio iniciado.", U5kiIncidencias.U5kiIncidencia.IGRL_U5KI_NBX_INFO, "CfgService", CTranslate.translateResource("Servicio iniciado."));
            }
#if _LOCKING_
			}
#endif
            catch (Exception ex)
            {
                //LogException<CfgService>("Error arrancando servicio de configuracion.", ex);
                ExceptionManage<CfgService>("Start", ex, "En START. Excepcion: " + ex.Message);
                _CheckCfg.Enabled = false;
                Stop();
            }
        }

        /// <summary>
        /// InitRegistry realiza la inicialización del _Registry
        /// </summary>
        private void InitRegistry()
        {
            _Registry = new Registry(Identifiers.CfgMasterTopic);
            _Registry.ChannelError += OnChannelError;
            _Registry.MasterStatusChanged += OnMasterStatusChanged;
            _Registry.ResourceChanged += OnResourceChanged;

            /**
             * AGL 20120706 Para Sincronizar Configuraciones.
             * */
            _Registry.UserMsgReceived += OnUserMsgReceived;
            /**
             * Fin de la modificacion */

            _Registry.SubscribeToMasterTopic(Identifiers.CfgMasterTopic);
            _Registry.SubscribeToTopic<Cd40Cfg>(Identifiers.CfgTopic);
            _Registry.SubscribeToTopic<SrvMaster>(Identifiers.CfgMasterTopic);
            _Registry.Join(Identifiers.CfgMasterTopic, Identifiers.CfgTopic);
        }
        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            LogInfo<CfgService>("Iniciando parada servicio Configuracion.");
#if _LOCKING_
            lock (_lock)
            {
                if (_Status == ServiceStatus.Running)
                {
                    Dispose();
                    _Status = ServiceStatus.Stopped;
                }
            }
            _WorkingThread.Stop();
#else
            _WorkingThread.Stop();
            if (_Status == ServiceStatus.Running)
            {
                Dispose();
                _Status = ServiceStatus.Stopped;
            }
#endif
            LogInfo<CfgService>("Servicio detenido.", U5kiIncidencias.U5kiIncidencia.IGRL_U5KI_NBX_INFO, "CfgService", CTranslate.translateResource("Servicio detenido."));
        }

        #endregion

        #region Private Members
        /// <summary>
        /// 
        /// </summary>
        private bool _Master = false;
        /// <summary>
        /// 
        /// </summary>
        private ServiceStatus _Status = ServiceStatus.Stopped;
        /// <summary>
        /// 
        /// </summary>
        private EventQueue _WorkingThread = new EventQueue();
        /// <summary>
        /// 
        /// </summary>
        private System.Timers.Timer _CheckCfg = new System.Timers.Timer();
        /// <summary>
        /// 
        /// </summary>
        private Registry _Registry = null;
        /// <summary>
        /// 
        /// </summary>
        private UdpSocket _CfgChangesListener = null;
        /// <summary>
        /// 
        /// </summary>
        private Cd40Cfg _LastCfg = null;
        /// <summary>
        /// 
        /// </summary>
        private bool _StopCfgThread = false;
        /// <summary>
        /// Flag that is true during the recovery of data of a new confguration
        /// </summary>
        private bool _CfgOnGoing = false;
        /// <summary>
        /// 
        /// </summary>
        private string _LastVersionReceived = "";
        /// <summary>
        /// 
        /// </summary>
        private string _LastCfgFile = "u5ki.LastCfg.bin";
        private string _LastCfgFileJson = "u5ki.LastCfg.json";
        /// <summary>
        /// Tipo para consulta GetPoolNMElements
        /// </summary>
        private const string TYPE_POOL_NM = "0";
        private const string TYPE_POOL_EE = "1";
#if _LOCKING_
        private object _lock = new object();
#endif
        /// <summary>
        /// 
        /// </summary>
        private void Dispose()
        {
            /**
             * AGL 20120704. Para sincronizar las salidas...
             * */
            while (_Master == true && _CheckCfg.Enabled == false) ;
            /**
             * Fin de la Modificacion */

            _CheckCfg.Enabled = false;
            _StopCfgThread = true;
            if (_Registry != null)
            {
                _Registry.Dispose();
                _Registry = null;
            }

            if (_CfgChangesListener != null)
            {
                _CfgChangesListener.Dispose();
                _CfgChangesListener = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="error"></param>
        private void OnChannelError(object sender, string error)
        {
            _WorkingThread.Enqueue("OnChannelError", delegate ()
            {
                LogError<CfgService>("OnChannelError: " + error);

                // Hace Lock interno. lo saco del bloque.
                _WorkingThread.InternalStop();
#if _LOCKING_
                lock (_lock)
                {
#endif
                // Hace Lock interno. lo saco del bloque.                
                // _WorkingThread.InternalStop();

                Dispose();
                _Status = ServiceStatus.Stopped;
#if _LOCKING_
                }
#endif
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="master"></param>
        private void OnMasterStatusChanged(object sender, bool master)
        {
            _Master = master;

            _WorkingThread.Enqueue("OnMasterStatusChanged", delegate ()
            {
#if _LOCKING_
                lock (_lock)
                {
#endif
                if (_Master)
                    //Para que el servicio de configuración entre el último master
                    System.Threading.Thread.Sleep(3000);

                if (_Master)
                {
                    try
                    {
                        //Debug.Assert(_CfgChangesListener == null);
                        if (_LastCfg != null)
                        {
                            _Registry.SetValue(Identifiers.CfgTopic, Identifiers.CfgRsId, _LastCfg);
                            /**
                             * AGL 20120705. Siempre se envía la informacion publicada...
                             * Anterior
                            _Registry.Publish(_LastCfg.Version, false);
                             * */
                            /**
                             * Cambio */
                            _Registry.Publish(_LastCfg.Version);
                            /**
                             * Fin del cambio */
                        }
                        /**
                         * AGL 20120705. Leo la Última configuracion de disco.
                         * */
                        else if (File.Exists(_LastCfgFileJson))
                        {
                            using (StreamReader r = new StreamReader(_LastCfgFileJson))
                            {
                                string json = r.ReadToEnd();
                                _LastCfg = JsonConvert.DeserializeObject<Cd40Cfg>(json);

                                _Registry.SetValue(Identifiers.CfgTopic, Identifiers.CfgRsId, _LastCfg);
                                _Registry.Publish(_LastCfg.Version);

                                // _Logger.Info(_LastCfg.ConfiguracionGeneral.ParametrosGenerales.ToString());
                            }
                        }
                        else if (File.Exists(_LastCfgFile))
                        {
                            using (FileStream file = File.OpenRead(_LastCfgFile))
                            {
                                _LastCfg = Serializer.Deserialize<Cd40Cfg>(file);

                                _Registry.SetValue(Identifiers.CfgTopic, Identifiers.CfgRsId, _LastCfg);
                                _Registry.Publish(_LastCfg.Version);

                                // _Logger.Info(_LastCfg.ConfiguracionGeneral.ParametrosGenerales.ToString());
                            }
                        }                        
                        else
                            LogInfo<CfgService>("No cfg file found", U5kiIncidencias.U5kiIncidencia.IGRL_U5KI_NBX_INFO, "CfgService", "MASTER");
                        /**
                         * Fin del cambio.*/
                    }
                    catch (Exception x)
                    {
                        //LogException<CfgService>("Cambiando a MASTER...", x);
                        ExceptionManage<CfgService>("OnMasterStatusChangedMaster", x, "Cambiando a MASTER. Excepcion: " + x.Message);
                    }
                    finally
                    {
                        LogInfo<CfgService>("MASTER", U5kiIncidencias.U5kiIncidencia.IGRL_U5KI_NBX_INFO, "CfgService", "MASTER");
                        OnCheckCfg(null, null);
                    }
                }
                else
                {
                    try
                    {
                        _Registry.SetValue(Identifiers.CfgTopic, Identifiers.CfgRsId, (Cd40Cfg)null);
                        _Registry.Publish(null, false);

                        _CheckCfg.Enabled = false;

                        if (_CfgChangesListener != null)
                        {
                            _CfgChangesListener.Dispose();
                            _CfgChangesListener = null;
                        }

                        _StopCfgThread = true;
                    }
                    catch (Exception x)
                    {
                        //LogException<CfgService>("CONFIG Cambiando a SLAVE...", x);
                        ExceptionManage<CfgService>("OnMasterStatusChangedSlave", x, "Cambiando a SLAVE. Excepcion: " + x.Message);
                    }
                    finally
                    {
                        _CheckCfg.Enabled = false;
                        _CfgChangesListener = null;

                        LogInfo<CfgService>("SLAVE");
                    }
                }
#if _LOCKING_
                }
#endif
            });
        }        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnResourceChanged(object sender, RsChangeInfo e)
        {
            if (e.Content != null)
            {
                _WorkingThread.Enqueue("OnResourceChanged", delegate ()
                {
#if _LOCKING_
                    lock (_lock)
                    {
#endif
                    if (!_Master)
                    {
                        try
                        {
                            MemoryStream ms = new MemoryStream(Tools.Decompress(e.Content));
                            _LastCfg = Serializer.Deserialize<Cd40Cfg>(ms);
                            /**
                             * AGL 20120704. Salvo la configuracion en disco...
                             * */
                            using (FileStream file = File.Create(_LastCfgFile))
                            {
                                Serializer.Serialize(file, _LastCfg);
                            }
                            /**
                             * Fin Cambio */
                            string json = JsonConvert.SerializeObject(_LastCfg);
                            File.WriteAllText(_LastCfgFileJson, json);
                        }
                        catch (Exception exc)
                        {
                            ExceptionManage<CfgService>("OnResourceChanged", exc, "Slave save cfg to file " + _LastCfg.Version + " Excepción: " + exc.Message);
                        }
                    }
#if _LOCKING_
                    }
#endif
                });
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCheckCfg(object sender, System.Timers.ElapsedEventArgs e)
        {
            _WorkingThread.Enqueue("OnTimeElapsed", delegate ()
            {
                LogTrace<CfgService>("OnTimeElapsed-IN");
#if _LOCKING_
                lock (_lock)
                {
#endif
                if (_Master)
                {
                    try
                    {
                        _CheckCfg.Interval = TimeSpan.FromSeconds(Settings.Default.CfgRefreshSegTime).TotalMilliseconds;  // 30000;
                        _CheckCfg.Enabled = false;

                        if (_CfgChangesListener == null)
                        {
                            using (SoapCfg.InterfazSOAPConfiguracion soapSrv = new U5ki.CfgService.SoapCfg.InterfazSOAPConfiguracion())
                            {
                                SoapCfg.ParametrosMulticast mc = soapSrv.GetParametrosMulticast(Settings.Default.CfgSystemId);

                                _CfgChangesListener = new UdpSocket(Settings.Default.MCastItf4Config, ((int)mc.PuertoMulticastConfiguracion));
                                _CfgChangesListener.MaxReceiveThreads = 1;
                                _CfgChangesListener.NewDataEvent += OnSoapCfgChanged;
                                _CfgChangesListener.Base.JoinMulticastGroup(IPAddress.Parse(mc.GrupoMulticastConfiguracion));
                                _CfgChangesListener.BeginReceive();
                            }
                        }

                        _StopCfgThread = false;
                        _CfgOnGoing = true;
                        TryGetSoapCfg();
                        _CfgOnGoing = false;
                    }
                    catch (Exception ex)
                    {
                        //LogException<CfgService>("Conexion a SOAP", ex);
                        _CheckCfg.Enabled = true;
                        ExceptionManage<CfgService>("OnTimeElapsed", ex, "En conexion SOAP. Exception: " + ex.Message);
                    }
                    finally
                    {
                        LogTrace<CfgService>("OnTimeElapsed-OUT");
                    }
                }
#if _LOCKING_
                }
#endif
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="dg"></param>
        private void OnSoapCfgChanged(object sender, DataGram dg)
        {
            _WorkingThread.Enqueue("OnSoapCfgChanged", delegate ()
            {
#if _LOCKING_
                lock (_lock)
                {
#endif
                if (_Master && (dg.Data.Length > 0))
                {
                    if (Settings.Default.ConfigByte[0] == dg.Data[0] ||	// 0x31: Comando de configuración; 0x32: Comando Principal/Reserva
                        Settings.Default.ConfigByte.Length == 0)
                    {
                        if (dg.Data[1] >= 0x30)
                        {
                            try
                            {
                                string version = Encoding.ASCII.GetString(dg.Data, 1, dg.Data.Length - 1);
                                if (version == _LastVersionReceived)
                                {
                                    return;
                                }

                                _LastVersionReceived = version;
                            }
                            catch (Exception) { }

                            LogInfo<CfgService>("OnSoapCfgChanged: " + "Recibida notificacion de cambio de configuracion SOAP");
                            if (_CfgOnGoing == true)
                                _StopCfgThread = true;
                            _CheckCfg.Stop();
                            _CheckCfg.Interval = 1;
                            _CheckCfg.Start();

                        }
                    }
                }
#if _LOCKING_
                }
#endif
            });
        }
        /// <summary>
        /// Thread para la carga de una nueva configuración.
        /// </summary>
        private void TryGetSoapCfg()
        {
            try
            {
                /** Identifica la configuracion en la base de datos */
                string systemId = Settings.Default.CfgSystemId;
                LogTrace<CfgService>("TryGetSoapCfg: " + "Chequeando configuración SOAP. SystemId: " + systemId);

                /** Encapsula el cliente del servicio WEB de Configuracion */
                using (SoapCfg.InterfazSOAPConfiguracion soapSrv = new U5ki.CfgService.SoapCfg.InterfazSOAPConfiguracion())
                {
                    /** Comprueba si han habido cambios de configuracion */
                    string soapVersion = soapSrv.GetVersionConfiguracion(systemId);
                    if (_StopCfgThread) return;
                    if ((_LastCfg != null) && (soapVersion == _LastCfg.Version))
                    {
                        LogTrace<CfgService>("TryGetSoapCfg: " + "Configuración actual coincide con configuración SOAP");
                        return;
                    }

                    /** Obtiene una copia de la configuracion en el formato SOAP del servicio */
                    SoapCfg.ConfiguracionSistema soapSysCfg = soapSrv.GetConfigSistema(systemId);
                    if (_StopCfgThread) return;

                    /** */
                    List<SoapCfg.CfgUsuario> soapUsers = new List<SoapCfg.CfgUsuario>();
                    List<SoapCfg.CfgEnlaceExterno[]> soapUsersExLinks = new List<SoapCfg.CfgEnlaceExterno[]>();
                    List<SoapCfg.CfgEnlaceInterno[]> soapUsersInLinks = new List<SoapCfg.CfgEnlaceInterno[]>();
                    List<string> hosts = new List<string>();
                    Dictionary<string, string> dominantUsers = new Dictionary<string, string>();

                    /** Extrae de la Configuracion SOAP la lista de 'hosts' ??? */
                    foreach (SoapCfg.AsignacionUsuariosTV asign in soapSysCfg.PlanAsignacionUsuarios)
                    {
                        if (!string.IsNullOrEmpty(asign.IdUsuario) && !hosts.Contains(asign.IdHost))
                        {
                            hosts.Add(asign.IdHost);
                        }
                    }
                    SoapCfg.LoginTerminalVoz soapLoginCfg;
                    SoapCfg.CfgUsuario soapCfgUser;
                    /** De la lista de Hosts, determina los usuarios y de enlaces a usuarios internos y externos ??? y de 'usarios dominantes' ??? */
                    foreach (string host in hosts)
                    {
                        soapLoginCfg = soapSrv.LoginTop(systemId, host);
                        if (_StopCfgThread) return;

                        if (!string.IsNullOrEmpty(soapLoginCfg.IdUsuario))
                        {
                            soapCfgUser = soapSrv.GetCfgUsuario(systemId, soapLoginCfg.IdUsuario);
                            soapUsers.Add(soapCfgUser);
                            dominantUsers[host] = soapCfgUser.IdIdentificador;
                            if (_StopCfgThread) return;

                            soapUsersExLinks.Add(soapSrv.GetListaEnlacesExternos(systemId, soapLoginCfg.IdUsuario));
                            if (_StopCfgThread)
                                return;

                            soapUsersInLinks.Add(soapSrv.GetListaEnlacesInternos(systemId, soapLoginCfg.IdUsuario));
                            if (_StopCfgThread) return;

                        }
                    }

                    /** Nueva configuracion en formato 'nodebox' */
                    Cd40Cfg cfg = new Cd40Cfg();
                    cfg.Version = soapVersion;
                    cfg.ConfiguracionGeneral = new ConfiguracionSistema();

                    /** Carga la configuracion 'nodebox' desde la configuracion SOAP. */
                    CfgTranslators.Translate(cfg.ConfiguracionGeneral, soapSysCfg);

                    /** Añade los Usuarios... */
                    for (int i = 0, to = soapUsers.Count; i < to; i++)
                    {
                        ConfiguracionUsuario user = new ConfiguracionUsuario();

                        CfgTranslators.Translate(user, soapUsers[i], soapUsersExLinks[i], soapUsersInLinks[i]);
                        cfg.ConfiguracionUsuarios.Add(user);
                    }

                    /** Añade los usuarios 'dominantes' */
                    foreach (KeyValuePair<string, string> p in dominantUsers)
                    {
                        AsignacionUsuariosDominantesTV dominantUser = new AsignacionUsuariosDominantesTV();
                        dominantUser.IdHost = p.Key;
                        dominantUser.IdUsuario = p.Value;

                        cfg.ConfiguracionGeneral.PlanAsignacionUsuariosDominantes.Add(dominantUser);
                    }

                    /** AGL. Carga la Configuracion HF */
                    SoapCfg.PoolHfElement[] hfpool = soapSrv.GetPoolHfElement(systemId);
                    if (hfpool != null)
                    {
                        foreach (SoapCfg.PoolHfElement hfelement in hfpool)
                        {
                            CfgTranslators.Translate(cfg, hfelement);
                        }
                    }

                    SoapCfg.Node[] nmPool = soapSrv.GetPoolNMElements(systemId, TYPE_POOL_NM);
                    if (nmPool != null)
                    {
                        foreach (SoapCfg.Node nmelement in nmPool)
                        {
#if DEBUG1
                            nmelement.ModeloEquipo = 1000;
#endif
                            CfgTranslators.Translate(cfg, nmelement);
                        }
                    }
                    SoapCfg.Node[] eePool = soapSrv.GetPoolNMElements(systemId, TYPE_POOL_EE);
                    if (eePool != null)
                    {
                        foreach (SoapCfg.Node extElement in eePool)
                        {
#if DEBUG1
                            nmelement.ModeloEquipo = 1000;
#endif
                            CfgTranslators.Translate(cfg, extElement, false);
                        }
                    }

                    //Añade las conferencias preprogramadas
                    SoapCfg.ConferenciasPreprogramadas conferences = soapSrv.GetConferenciasPreprogramadas(systemId);
                    if (conferences != null)
                    {
                        CfgTranslators.Translate(cfg, conferences);
                    }

                    try
                    {
                        /**
                         * AGL 20120705. Salvar la configuracion en disco
                         * */
                        using (FileStream file = File.Create(_LastCfgFile))
                        {
                            Serializer.Serialize(file, cfg);
                        }

                        string json = JsonConvert.SerializeObject(cfg);
                        File.WriteAllText(_LastCfgFileJson, json);
                    }
                    catch (Exception exc)
                    {
                        ExceptionManage<CfgService>("TryGetSoapCfg", exc, "En TryGetSoapCfg save to file " + cfg.Version + " Excepción: " + exc.Message);
                    }
                    /**
                     * Fin de Modificacion */

                    /** Salva y distribuye la configuracion */
                    _WorkingThread.Enqueue("PublishNewCfg", delegate ()
                    {
                        try
                        {
                            if (_Master && !_StopCfgThread)
                            {
                                _LastCfg = cfg;
                                LogInfo<CfgService>("Publicando nueva configuración: " + cfg.Version, U5kiIncidencias.U5kiIncidencia.IGRL_U5KI_NBX_INFO,
                                    "CfgService", CTranslate.translateResource("Publicando nueva configuración: " + cfg.Version));                                

                                _Registry.SetValue(Identifiers.CfgTopic, Identifiers.CfgRsId, cfg);
                                _Registry.Publish(cfg.Version);
                            }
                        }
                        catch (Exception ex)
                        {
                            //LogException<CfgService>("TryGetSoapCfg: Error publicando configuracion " + cfg.Version, ex);
                            ExceptionManage<CfgService>("TryGetSoapCfg", ex, "En TryGetSoapCfg. Excepcion: " + ex.Message + ". publicando configuracion " + cfg.Version);

                            _WorkingThread.InternalStop();
                            Dispose();
                            _Status = ServiceStatus.Stopped;
                        }
                    });
                }
            }
            catch (ThreadAbortException ex)
            {
                Thread.ResetAbort();
                ExceptionManage<CfgService>("TryGetSoapCfg", ex, "En TryGetSoapCfg Excepcion: " + ex.Message);
            }
            catch (Exception ex)
            {
                //LogException<CfgService>("TryGetSoapCfg: Intento fallido de obtener la configuracion SOAP.", ex);
                ExceptionManage<CfgService>("TryGetSoapCfg", ex, "En TryGetSoapCfg Excepcion: " + ex.Message);
            }
            finally
            {
                _CheckCfg.Enabled = true;
            }
            return;
        }

        string SystemId => Settings.Default.CfgSystemId;
        /// <summary>
        /// AGL 20120706. Para Sincronizar Configuracion por Defecto.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        private void OnUserMsgReceived(object sender, SpreadDataMsg msg)
        {
#if _LOCKING_
            lock (_lock)
            {
#endif
            if (msg.Type == Identifiers.CFG_SAVE_AS_DEFAULT_MSG)
            {
                if (_Master == false)
                {
                    if (_LastCfg != null)
                    {
                        MemoryStream ms = new MemoryStream(msg.Data, 0, msg.Length);
                        string par = Serializer.Deserialize<string>(ms);
                        using (FileStream file = File.Create(DefaultCfgFile(par)))
                        {
                            Serializer.Serialize(file, _LastCfg);
                            LogInfo<CfgService>("Generada configuracion por defecto", U5kiIncidencias.U5kiIncidencia.IGRL_U5KI_NBX_INFO,
                                "CfgService", CTranslate.translateResource("Generada configuracion por defecto"));
                        }
                    }
                    else
                    {
                        LogError<CfgService>(CTranslate.translateResource("No tengo configuración válida"),
                            U5kiIncidencias.U5kiIncidencia.IGRL_U5KI_NBX_ERROR);
                    }
                }
            }
            if (msg.Type == Identifiers.CFG_DEL_DEFAULT)
            {
                if (_Master == false)
                {
                    MemoryStream ms = new MemoryStream(msg.Data, 0, msg.Length);
                    string file_name = Serializer.Deserialize<string>(ms);
                    if (File.Exists(file_name))
                    {
                        File.Delete(file_name);
                    }

                }
            }
#if _LOCKING_
            }
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cfgnumber"></param>
        /// <returns></returns>
        private string DefaultCfgFile(string cfgnumber)
        {
            return string.Format("u5ki.DefaultCfg.{0}.bin", cfgnumber);
        }

        /**
         * Fin del cambio */

        #endregion
    }

    public class CfgServiceAsync : BaseCode, IService, IDisposable
    {
        #region DI
        public IUlisesSoapService SoapService { get; set; } = new RealSoapService();
        public IUlisesMcastService McastService { get; set; } = new Registry();
        #endregion DI

        #region IService
        public string Name => "Cd40ConfigService";

        public ServiceStatus Status { get; set; } = ServiceStatus.Stopped;

        public bool Master { get; set; } = false;

        public object AllDataGet()
        {
            if (Status == ServiceStatus.Running)
            {
                return works.ExecuteInAsync<object>("AllDataGet", () =>
                {
                    return new
                    {
                        std = $"{Status}",
                        level = Status != ServiceStatus.Running ? "Error" : Master == true ? "Master" : "Slave",
                        cfg_activa = Master == false ? "El servicio no esta en modo MASTER" :
                            lastCfg != null ? lastCfg.Cfg?.Version : "No hay ninguna configuracion cargada...",
                    };
                }).Result;
            }
            return null;
        }

        public bool Commander(ServiceCommands cmd, string par, ref string err, List<string> resp = null)
        {
            var result = works.ExecuteInAsync<string>("AllDataGet", () =>
            {
                string internalResult = null;
                switch (cmd)
                {
                    case ServiceCommands.SetDefaultCfg:
                        internalResult = SetPreconfig(par);
                        break; 
                    case ServiceCommands.LoadDefaultCfg:
                        internalResult = LoadPreconfig(par);
                        break;
                    case ServiceCommands.DelDefaultCfg:
                        internalResult = DeletePreconfig(par);
                        break;
                    case ServiceCommands.GetDefaultConfigId:
                        internalResult = ConfigIdForRemote(resp);
                        break;
                    default:
                        internalResult = $"Cmd {cmd} Not Implemented!";
                        break;
                }
                return internalResult;
            }).Result;
            err = result != null ? result.ToString() : resp.Count > 0 ? resp.ElementAt(0) : string.Empty;
            return result == null;
        }

        public bool DataGet(ServiceCommands cmd, ref List<object> rsp)
        {
            throw new NotImplementedException("Not implemented Method on CfgService");
        }

        public void Start()
        {
            if (Status != ServiceStatus.Stopped)
            {
                throw new InvalidOperationException("Service already started...");
            }
            mainWorkerCancelControl = new CancellationTokenSource();
            Task.Run(MainWorker);
            LogDebug<CfgServiceAsync>("Service Started.");
        }

        public void Stop()
        {
            if (Status != ServiceStatus.Running)
            {
                throw new InvalidOperationException("Service already stopped...");
            }
            mainWorkerCancelControl?.Cancel();
            while (mainWorkerCancelControl != null) Task.Delay(100).Wait();
            LogDebug<CfgServiceAsync>("Service Stopped.");
        }
        #endregion IService

        #region IDisposable
        public void Dispose()
        {
        }
        #endregion IDisposable

        bool InitializationProc()
        {
            try
            {
                Master = false;
                lastCfg = null;
                works.Start().Wait();
                RegistryInit();
                LogDebug<CfgService>("Service Initiated.", U5kiIncidencias.U5kiIncidencia.IGRL_U5KI_NBX_INFO, "CfgService", CTranslate.translateResource("Servicio iniciado."));
                return true;
            }
            catch (Exception x)
            {
                LogException<CfgServiceAsync>("On InitializationProc", x, false);
            }
            return false;
        }
        async void MainWorker()
        {
            while (mainWorkerCancelControl.IsCancellationRequested == false)
            {
                if (InitializationProc() == true)
                {
                    DateTime lastCheck = DateTime.MinValue;
                    UdpClient LanControl = new UdpClient();
                    SoapControl soap = new SoapControl(SoapService, mainWorkerCancelControl);
                    Status = ServiceStatus.Running;

                    LogDebug<CfgService>("Service running");
                    while (mainWorkerCancelControl.IsCancellationRequested == false)
                    {
                        await works.ExecuteInAsync("MainWorker", () =>
                        {
                            if (Master)
                            {
                                if (ForceCheck || DateTime.Now - lastCheck > TimeSpan.FromSeconds(20))
                                {
                                    CheckForConfig(soap).Wait();
                                    lastCheck = DateTime.Now;
                                    ForceCheck = false;
                                }
                                CheckForIpOrder(LanControl).Wait();
                            }
                            return true;
                        });
                        Task.Delay(100).Wait(); // TODO. En una variable...
                    }
                    Status = ServiceStatus.Stopped;
                    RegistryDispose();
                    works.Stop().Wait();
                }
                else
                {
                    Task.Delay(TimeSpan.FromSeconds(10)).Wait(); // TODO. En una variable...
                }
            }
            mainWorkerCancelControl = null;
        }
        async Task CheckForConfig(SoapControl soap)
        {
            try
            {
                LogDebug<CfgServiceAsync>($"Checking for new Config");
                if (await soap.CheckConfig(lastCfg?.Cfg.Version) == false) return;
                LogDebug<CfgServiceAsync>($"New Configuration version detected");
                if (await soap.LoadSoapConfig() == false) return;
                var newCfg = await soap.GetNewCd40Cfg();
                if (newCfg == null) return;
                LogDebug<CfgServiceAsync>($"New Configuration loaded => {newCfg.Version}");

                LogInfo<CfgService>("Publicando nueva configuración: " + newCfg.Version, U5kiIncidencias.U5kiIncidencia.IGRL_U5KI_NBX_INFO,
                    "CfgService", CTranslate.translateResource("Publicando nueva configuración: " + newCfg.Version));

                McastService.SetValue(Identifiers.CfgTopic, Identifiers.CfgRsId, newCfg);
                McastService.Publish(newCfg.Version);
                if (lastCfg == null)
                    lastCfg = new FileConfig();
                lastCfg.Cfg = newCfg;
                FileConfig.Save(ConfFilename, newCfg, lastCfg.McastParams);
                LogDebug<CfgServiceAsync>($"New Configuration saved => {newCfg.Version}");
            }
            catch (Exception x)
            {
                LogException<CfgServiceAsync>("CheckForConfig", x, false);
            }
        }
        async Task<bool> CheckForIpOrder(UdpClient lan)
        {
            try
            {
                if (lan.Client.IsBound == false)
                {
                }
                return true;
            }
            catch (Exception x)
            {
                LogException<CfgServiceAsync>("CheckForIpOrder", x, false);
            }
            return false;
        }
        #region Registry and Handlers
        void RegistryInit()
        {
            McastService.Init(Identifiers.CfgMasterTopic);
            McastService.ChannelError += OnChannelError;
            McastService.MasterStatusChanged += OnMasterStatusChanged;
            McastService.ResourceChanged += OnResourceChanged;
            McastService.UserMsgReceived += OnUserMsgReceived;

            McastService.SubscribeToMasterTopic(Identifiers.CfgMasterTopic);
            McastService.SubscribeToTopic<Cd40Cfg>(Identifiers.CfgTopic);
            McastService.SubscribeToTopic<SrvMaster>(Identifiers.CfgMasterTopic);
            McastService.Join(Identifiers.CfgMasterTopic, Identifiers.CfgTopic);
            LogDebug<CfgServiceAsync>($"MCASTservice initializated");
        }
        void RegistryDispose()
        {
            McastService?.Dispose();
            McastService = null;
            LogDebug<CfgServiceAsync>($"MCAST Service disposed.");
        }
        void OnChannelError(object sender, string error)
        {
            LogError<CfgServiceAsync>($"OnChannelError => {error}");
            mainWorkerCancelControl?.Cancel();
        }
        void OnMasterStatusChanged(object sender, bool master)
        {
            LogDebug<CfgServiceAsync>($"OnMasterStatusChanged Received, MASTER => {master}");
            works.Enqueue("OnMasterStatusChange", () =>
            {
                try
                {
                    var change = master != Master;
                    if (change)
                    {
                        if (master)
                        {
                            ChangeToMaster();
                        }
                        else
                        {
                            ChangeToSlave();
                        }
                    }
                }
                catch (Exception x)
                {
                    LogException<CfgServiceAsync>($"OnMasterStatusChanged Exception", x, false);
                }
            }).Wait();
        }
        void OnResourceChanged(object sender, RsChangeInfo e)
        {
            LogDebug<CfgServiceAsync>($"OnResourceChanged event Received => {e.Id}");
            works.Enqueue("OnResourceChanges", () =>
            {
                try
                {
                    if (Master)
                    {
                        return;
                    }
                    if (e?.Content != null)
                    {
                        MemoryStream ms = new MemoryStream(Tools.Decompress(e.Content));
                        lastCfg.Cfg = Serializer.Deserialize<Cd40Cfg>(ms);
                        FileConfig.Save(ConfFilename, lastCfg.Cfg, lastCfg.McastParams);
                    }
                }
                catch (Exception x)
                {
                    LogException<CfgServiceAsync>($"OnResourceChanged Exception", x, false);
                }
            }).Wait();
        }
        void OnUserMsgReceived(object sender, SpreadDataMsg msg)
        {
            LogDebug<CfgServiceAsync>($"OnUserMsgReceived event Received => {msg.Type}");
            works.Enqueue("OnUserMsgReceived", () =>
            {
                try
                {
                    if (msg.Type == Identifiers.CFG_SAVE_AS_DEFAULT_MSG)
                    {
                        if (Master == false)
                        {
                            if (lastCfg != null)
                            {
                                MemoryStream ms = new MemoryStream(msg.Data, 0, msg.Length);
                                string par = Serializer.Deserialize<string>(ms);
                                FileConfig.Save(PreconfFilename(par), lastCfg.Cfg, lastCfg.McastParams);
                                LogInfo<CfgService>("Generada configuracion por defecto", U5kiIncidencias.U5kiIncidencia.IGRL_U5KI_NBX_INFO,
                                    "CfgService", CTranslate.translateResource("Generada configuracion por defecto"));
                            }
                            else
                            {
                                LogError<CfgService>(CTranslate.translateResource("No tengo configuración válida"),
                                    U5kiIncidencias.U5kiIncidencia.IGRL_U5KI_NBX_ERROR);
                            }
                        }
                    }
                    else if (msg.Type == Identifiers.CFG_DEL_DEFAULT)
                    {
                        if (Master == false)
                        {
                            MemoryStream ms = new MemoryStream(msg.Data, 0, msg.Length);
                            string file_name = Serializer.Deserialize<string>(ms);
                            if (File.Exists(file_name))
                            {
                                File.Delete(file_name);
                                LogDebug<CfgServiceAsync>($"Preconfiguration => {file_name} deleted!");
                            }
                        }
                    }
                    else
                    {
                        LogWarn<CfgServiceAsync>($"User message unknow!");
                    }
                }
                catch (Exception x)
                {
                    LogException<CfgServiceAsync>($"OnUserMsgReceived Exception", x, false);
                }
            }).Wait();
        }
        #endregion Registry and Handlers
        string SetPreconfig(string par)
        {
            if (Master == false)
            {
                return "El servicio no esta en modo MASTER";
            }
            else if (lastCfg != null)
            {
                FileConfig.Save(PreconfFilename(par), lastCfg.Cfg, lastCfg.McastParams);
                McastService?.Send<string>(Identifiers.CfgTopic, Identifiers.CFG_SAVE_AS_DEFAULT_MSG, par);
                LogInfo<CfgService>("Generada configuración " + par + " por defecto...", U5kiIncidencias.U5kiIncidencia.IGRL_U5KI_NBX_INFO,
                    "CfgService", CTranslate.translateResource("Generada configuración " + par + "por defecto..."));
                return null;
            }
            return "No hay ninguna configuracion cargada...";
        }
        string LoadPreconfig(string par)
        {
            if (Master == false)
            {
                return "El servicio no esta en modo MASTER";
            }
            else if (File.Exists(PreconfFilename(par)))
            {
                FileConfig.Load(PreconfFilename(par), (cfg, mscast) =>
                {
                    lastCfg = new FileConfig() { Cfg = cfg, McastParams = mscast };

                    McastService.SetValue(Identifiers.CfgTopic, Identifiers.CfgRsId, lastCfg.Cfg);
                    McastService.Publish(lastCfg.Cfg.Version);

                    FileConfig.Save(ConfFilename, lastCfg.Cfg, lastCfg.McastParams);
                });

                LogInfo<CfgService>("Activada configuración " + par, U5kiIncidencias.U5kiIncidencia.IGRL_U5KI_NBX_INFO,
                    "CfgService", CTranslate.translateResource("Activada configuración " + par));
                return null;
            }
            else
            {
               return $"No existe la configuracion {PreconfFilename(par)}";
            }
        }
        string DeletePreconfig(string par)
        {
            if (Master == false)
            {
                return "El servicio no esta en modo MASTER";
            }
            else if (File.Exists(PreconfFilename(par)))
            {
                File.Delete(PreconfFilename(par));

                McastService.Send<string>(Identifiers.CfgTopic, Identifiers.CFG_DEL_DEFAULT, PreconfFilename(par));
                LogInfo<CfgService>("Configuración " + par + " eliminada...", U5kiIncidencias.U5kiIncidencia.IGRL_U5KI_NBX_INFO,
                    "CfgService", CTranslate.translateResource("Configuración " + par + " eliminada..."));
                return null;
            }
            return $"El Fichero: {PreconfFilename(par)} no existe"; ;
        }
        string ConfigIdForRemote(List<string> data)
        {
            if (Master == false)
            {
                return "El servicio no esta en modo MASTER";
            }
            else if (lastCfg != null)
            {
                data?.Add(lastCfg.Cfg.Version);
                return null;
            }
            return "No hay ninguna configuracion cargada...";
        }
        void ChangeToMaster()
        {
            LogDebug<CfgServiceAsync>($"Changing to Master. Waiting for Sync");
            Task.Delay(TimeSpan.FromSeconds(3)).Wait();     // TODO. tiempo configurable.
            if (lastCfg != null)
            {
                McastService.SetValue(Identifiers.CfgTopic, Identifiers.CfgRsId, lastCfg.Cfg);
                McastService.Publish(lastCfg.Cfg.Version);
            }
            else if (File.Exists(ConfFilename))
            {
                FileConfig.Load(ConfFilename, (cfg, mcast) =>
                {
                    lastCfg = new FileConfig() { Cfg = cfg, McastParams = mcast };
                    McastService.SetValue(Identifiers.CfgTopic, Identifiers.CfgRsId, lastCfg.Cfg);
                    McastService.Publish(lastCfg.Cfg.Version);
                });
            }
            else
            {
                LogError<CfgService>("No cfg file found", U5kiIncidencias.U5kiIncidencia.IGRL_U5KI_NBX_INFO, "CfgService", "MASTER");
            }
            Master = true;
            // Forzar un peticion de configuracion en el MainWorker
            LogDebug<CfgServiceAsync>($"Changed to Master.");
            ForceCheck = true;
        }
        void ChangeToSlave()
        {
            LogDebug<CfgServiceAsync>($"Changing to Standby.");
            McastService.SetValue(Identifiers.CfgTopic, Identifiers.CfgRsId, (Cd40Cfg)null);
            McastService.Publish(null, false);
            Master = false;
            LogDebug<CfgServiceAsync>($"Changed to Standby.");
        }

        string PreconfFilename(string cfgnumber) => $"u5ki.DefaultCfg.{cfgnumber}.json";
        string ConfFilename => $"u5ki.Cfg.json";
        internal class FileConfig 
        {
            public Cd40Cfg Cfg { get; set; }
            public SoapCfg.ParametrosMulticast McastParams { get; set; }
            public static void Save(string fileName, Cd40Cfg cfg, SoapCfg.ParametrosMulticast mcast)
            {
                try
                {
                    var fileData = new FileConfig() { Cfg = cfg, McastParams = mcast };
                    var strData = JsonConvert.SerializeObject(fileData);
                    File.WriteAllText(fileName, strData);
                }
                catch (Exception )
                {
                    throw;
                }
            }
            public static void Load(string fileName, Action<Cd40Cfg, SoapCfg.ParametrosMulticast> response)
            {
                try
                {
                    var strData = File.Exists(fileName) ? File.ReadAllText(fileName) : string.Empty;
                    if (strData != string.Empty)
                    {
                        var fileData = JsonConvert.DeserializeObject<FileConfig>(strData);
                        response(fileData?.Cfg, fileData?.McastParams);
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        private FileConfig lastCfg = null;
        private ActionQueueAsync works = new ActionQueueAsync();
        private CancellationTokenSource mainWorkerCancelControl = null;
        private bool ForceCheck = false;
    }
    internal class SoapControl
    {
        internal class SoapUserConfiguration
        {
            public string Pict {get; set;}
            public SoapCfg.CfgUsuario CfgUsuario { get; set; }
            public SoapCfg.CfgEnlaceExterno[] ExLinks { get; set; }
            public SoapCfg.CfgEnlaceInterno[] InLinks { get; set; }
            public SoapUserConfiguration(IUlisesSoapService SoapSrv, string pict, string IdUsuario)
            {
                Pict = pict;
                CfgUsuario = SoapSrv.GetCfgUsuario(IdUsuario);
                ExLinks = SoapSrv.GetListaEnlacesExternos(IdUsuario);
                InLinks = SoapSrv.GetListaEnlacesInternos(IdUsuario);
            }
        }
        internal class SoapDominantConfiguration
        {
            public string Pict = null;
            public string UserId = null;
            public SoapDominantConfiguration(IUlisesSoapService SoapSrv, string pict)
            {
                Pict = pict;
                UserId = SoapSrv.LoginTop(Pict)?.IdUsuario;
            }
        }
        IUlisesSoapService SoapSrv { get; set; }
        CancellationTokenSource CancelControl { get; set; }
        public SoapControl(IUlisesSoapService service, CancellationTokenSource cancelControl)
        {
            CancelControl = cancelControl;
            SoapSrv = service;
        }
        public Task<bool> CheckConfig(string lastVersion)
        {
            return Task.Run( () =>
            {
                Version = SoapSrv?.GetVersionConfiguracion();
                return CancelControl.IsCancellationRequested ? false : Version != lastVersion ? true : false;
            });
        }
        public Task<bool> LoadSoapConfig()
        {
            return Task.Run(() =>
            {
                soapSysCfg = SoapSrv?.GetConfigSistema();
                if (CancelControl.IsCancellationRequested) return false;
                
                picts = soapSysCfg.PlanAsignacionUsuarios
                    .Select(p => p.IdHost)
                    .Distinct();
                cfgUsers = picts
                    .Select(p => new SoapDominantConfiguration(SoapSrv, p))
                    .Where(l => l.UserId != null)
                    .WithCancellation(CancelControl.Token)
                    .Select(c => new SoapUserConfiguration(SoapSrv, c.Pict, c.UserId));

                hfpool = SoapSrv?.GetPoolHfElement();
                if (CancelControl.IsCancellationRequested) return false;

                nmPool = SoapSrv?.GetPoolNMElements(TYPE_POOL_NM);
                if (CancelControl.IsCancellationRequested) return false;

                eePool = SoapSrv?.GetPoolNMElements(TYPE_POOL_EE);
                if (CancelControl.IsCancellationRequested) return false;

                conferences = SoapSrv?.GetConferenciasPreprogramadas();
                return true;
            });
        }
        public Task<Cd40Cfg> GetNewCd40Cfg()
        {
            return Task.Run(() =>
            {
                var Cfg = new Cd40Cfg()
                {
                    Version = Version,
                    ConfiguracionGeneral = new ConfiguracionSistema()
                };
                CfgTranslators.Translate(Cfg.ConfiguracionGeneral, soapSysCfg);
                cfgUsers.ToList().ForEach(u => 
                {
                    Cfg.ConfiguracionUsuarios.Add(TranslateUser(u.CfgUsuario, u.ExLinks, u.InLinks));
                    Cfg.ConfiguracionGeneral.PlanAsignacionUsuariosDominantes.Add(new AsignacionUsuariosDominantesTV() { IdHost = u.Pict, IdUsuario = u.CfgUsuario.IdIdentificador });
                });

                hfpool.ToList().ForEach(item => CfgTranslators.Translate(Cfg, item));
                nmPool.ToList().ForEach(item => CfgTranslators.Translate(Cfg, item, true));
                eePool.ToList().ForEach(item => CfgTranslators.Translate(Cfg, item, false));

                CfgTranslators.Translate(Cfg, conferences);

                return Cfg;
            });
        }
        ConfiguracionUsuario TranslateUser(SoapCfg.CfgUsuario userCfg, SoapCfg.CfgEnlaceExterno[] eLinks, SoapCfg.CfgEnlaceInterno[] iLinks)
        {
            var user = new ConfiguracionUsuario();
            CfgTranslators.Translate(user, userCfg, eLinks, iLinks);
            return user;
        }
        const string TYPE_POOL_NM = "0";
        const string TYPE_POOL_EE = "1";

        string Version = string.Empty;
        SoapCfg.ConfiguracionSistema soapSysCfg = null;
        IEnumerable<SoapUserConfiguration> cfgUsers = null;
        IEnumerable<string> picts = null;
        IEnumerable<SoapCfg.PoolHfElement> hfpool = null;
        IEnumerable<SoapCfg.Node> nmPool = null;
        IEnumerable<SoapCfg.Node> eePool = null;
        SoapCfg.ConferenciasPreprogramadas conferences = null;
    }

    public interface IUlisesSoapService
    {
        string GetVersionConfiguracion();
        SoapCfg.ConfiguracionSistema GetConfigSistema();
        SoapCfg.LoginTerminalVoz LoginTop(string pict);
        SoapCfg.CfgUsuario GetCfgUsuario(string userId);
        SoapCfg.CfgEnlaceExterno[] GetListaEnlacesExternos(string userId);
        SoapCfg.CfgEnlaceInterno[] GetListaEnlacesInternos(string userId);
        SoapCfg.PoolHfElement[] GetPoolHfElement();
        SoapCfg.Node[] GetPoolNMElements(string elementType);
        SoapCfg.ConferenciasPreprogramadas GetConferenciasPreprogramadas();
    }
    internal class RealSoapService : IUlisesSoapService
    {
        SoapCfg.InterfazSOAPConfiguracion SoapSrv { get; set; } = new SoapCfg.InterfazSOAPConfiguracion();
        string SystemId => Settings.Default.CfgSystemId;

        public SoapCfg.CfgUsuario GetCfgUsuario(string userId) => SoapSrv?.GetCfgUsuario(SystemId, userId);
        public SoapCfg.ConferenciasPreprogramadas GetConferenciasPreprogramadas() => SoapSrv?.GetConferenciasPreprogramadas(SystemId);
        public SoapCfg.ConfiguracionSistema GetConfigSistema() => SoapSrv?.GetConfigSistema(SystemId);
        public SoapCfg.CfgEnlaceExterno[] GetListaEnlacesExternos(string userId) => SoapSrv?.GetListaEnlacesExternos(SystemId, userId);
        public SoapCfg.CfgEnlaceInterno[] GetListaEnlacesInternos(string userId) => SoapSrv?.GetListaEnlacesInternos(SystemId, userId);
        public SoapCfg.PoolHfElement[] GetPoolHfElement() => SoapSrv?.GetPoolHfElement(SystemId);
        public SoapCfg.Node[] GetPoolNMElements(string elementType) => SoapSrv?.GetPoolNMElements(SystemId, elementType);
        public string GetVersionConfiguracion() => SoapSrv?.GetVersionConfiguracion(SystemId);
        public SoapCfg.LoginTerminalVoz LoginTop(string pict) => SoapSrv?.LoginTop(SystemId, pict);
    }

    internal class ActionQueueAsync
    {
        public int SecondsTimeout { get; set; } = 10;
        public int MillisecondsTick { get; set; } = 50;
        public ActionQueueAsync() { }
        public Task Start()
        {
            return Task.Run(() =>
            {
                if (cts == null)
                {
                    cts = new CancellationTokenSource();
                    Task.Run(Executer);
                }
            });
        }
        public Task Stop()
        {
            return Task.Run(() =>
            {
                if (cts != null)
                {
                    cts.Cancel();
                    while (cts != null) Task.Delay(MillisecondsTick).Wait();
                }
            });
        }
        public Task<T> ExecuteInAsync<T>(string id, Func<T> action)
        {
            return Task.Run(() =>
            {
                var sync = new ManualResetEvent(false);
                T retorno = default;
                queue.Add(() =>
                {
                    try
                    {
                        retorno = action();
                    }
                    catch (Exception)
                    {
                        // todo... Grabar la excepción
                        throw;
                    }
                    finally
                    {
                        sync.Set();
                    }
                });
                sync.WaitOne(TimeSpan.FromSeconds(SecondsTimeout));
                return retorno;
            });
        }
        public Task Enqueue(string id, Action action)
        {
            return Task.Run(() =>
            {
                queue.Add(() =>
                {
                    try
                    {
                        action();
                    }
                    catch (Exception)
                    {
                        // todo... Grabar la excepción
                        throw;
                    }
                });
            });
        }
        async void Executer()
        {
            // Borrar posibles datos anteriores
            Clear();
            while (cts.IsCancellationRequested == false)
            {
                while (queue.Count > 0)
                {
                    if (cts.IsCancellationRequested == true) break;
                    var action = queue.Take();
                    action();
                }
                await Task.Delay(MillisecondsTick);
            }
            cts = null;
        }
        void Clear()
        {
            while (queue.Count > 0)
            {
                queue.TryTake(out _);
            }
        }
        readonly BlockingCollection<Action> queue = new BlockingCollection<Action>(new ConcurrentQueue<Action>());
        CancellationTokenSource cts = null;
    }
    public static class CancelExtention
    {
        public static IEnumerable<T> WithCancellation<T>(this IEnumerable<T> en, CancellationToken token)
        {
            foreach (var item in en)
            {
                token.ThrowIfCancellationRequested();
                yield return item;
            }
        }
    }
}
