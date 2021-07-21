using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Net;

using U5ki.Infrastructure;
using U5ki.CfgService.Properties;

using Utilities;
using ProtoBuf;
using NLog;

using Translate;
using Newtonsoft.Json;
namespace U5ki.CfgService
{
    public class CfgService : BaseCode, IService, IDisposable
    {
        const string LastCfgFile = "u5ki.LastCfg.bin";
        const string LastCfgFileJson = "u5ki.LastCfg.json";
        const string TYPE_POOL_NM = "0";
        const string TYPE_POOL_EE = "1";
        int SoapTimeout = (int)TimeSpan.FromSeconds(5).TotalMilliseconds;

        #region IService
        public string Name => "Cd40ConfigService";

        public ServiceStatus Status { get; set; }

        public bool Master { get; set; }

        public object AllDataGet()
        {
            return new
            {
                std = Status.ToString(),
                level = Status != ServiceStatus.Running ? "Error" : Master == true ? "Master" : "Slave",
                cfg_activa = Master == false ? "El servicio no esta en modo MASTER" :
                    LastCfg != null ? LastCfg.Version : "No hay ninguna configuracion cargada...",
            };
        }

        public bool Commander(ServiceCommands cmd, string par, ref string err, List<string> resp = null)
        {
            string spar = (string)par;
            switch (cmd)
            {
                case ServiceCommands.GetDefaultConfigId:
                    if (Master == false)
                    {
                        err = "El servicio no esta en modo MASTER";
                    }
                    else if (LastCfg != null)
                    {
                        if (resp != null)
                            resp.Add(LastCfg.Version);
                        err = LastCfg.Version;
                        return true;
                    }
                    else
                    {
                        err = "No hay ninguna configuracion cargada...";
                    }
                    break;
            }
            return false;
        }

        public bool DataGet(ServiceCommands cmd, ref List<object> rsp)
        {
            return false;
        }

        public void Start()
        {
            if (Status != ServiceStatus.Running)
            {
                WorkingThread?.Start();
                ExceptionManageInit();
                WorkingThread?.Enqueue("Starting CfgService", () =>
                {
                    LogInfo<CfgService>("Starting CfgService...");
                    Master = false;
                    Init();
                    SupervisionTaskSync = new ManualResetEvent(false);
                    SupervisionTask = Task.Run(() => SupervisionRoutine());
                    Status = ServiceStatus.Running;
                    LogInfo<CfgService>("CfgService started...");
                });
            }
            else
            {
                // El servicio esta ya arrancado.
                LogError<CfgService>("Error on starting CfgService. Already running...");
            }
        }

        public void Stop()
        {
            if (Status == ServiceStatus.Running)
            {
                var sync = new ManualResetEvent(false);
                WorkingThread?.Enqueue("Ending CfgService", () =>
                {
                    LogInfo<CfgService>("Ending CfgService...");

                    WorkingThread.InternalStop();
                    
                    SupervisionTaskSync.Set();
                    SupervisionTask.Wait(TimeSpan.FromSeconds(10));

                    Clear();
                    Status = ServiceStatus.Stopped;
                    sync.Set();
                    LogInfo<CfgService>("CfgService ended...");
                });
                var res = sync.WaitOne(TimeSpan.FromSeconds(10));
            }
            else
            {
                // El servicio no está arrancado.
                LogError<CfgService>("Error on ending CfgService. Service is not running...");
            }
        }

        #endregion IService

        #region Constructors
        public CfgService()
        {
            Status = ServiceStatus.Stopped;
            Master = false;
            WorkingThread = new EventQueue();
            CfgRegistry = null;
            CfgChangesListener = null;
            LastCfg = null;
        }
        public void Dispose()
        {
            // todo
        }

        #endregion Constructors

        #region Managers
        private void SupervisionRoutine()
        {
            LastCheckDate = DateTime.MinValue;
            while (SupervisionTaskSync.WaitOne(TimeSpan.FromMilliseconds(100)) == false)
            {
                var elapsed = DateTime.Now - LastCheckDate;
                if (elapsed > TimeSpan.FromSeconds(Settings.Default.CfgRefreshSegTime))
                {
                    WorkingThread.Enqueue("", () =>
                    {
                        if (Master)
                        {
                            // Tarea 1. Configurar la escucha de los avisos MCAST.
                            InitCfgChangesListener();

                            // Tarea 2. Chequear periodicamente la configuracion.
                            TestCfg((version) =>
                            {
                                if (GetCfg(version))
                                {
                                    PublishCfg();
                                    SaveCfg();
                                }
                            });
                        }
                    });
                    LastCheckDate = DateTime.Now;
                }
            }
        }
        private void OnChannelError(object sender, string error)
        {
            LogError<CfgService>($"CfgService OnChannelError => {error}");
            Stop();
        }
        private void OnMasterStatusChanged(object sender, bool master)
        {
            //Para que el servicio de configuración entre el último master
            if (master) Task.Delay(TimeSpan.FromSeconds(3)).Wait();

            WorkingThread.Enqueue("OnMasterStatusChanged", () =>
            {
                Master = master;
                if (Master)
                {
                    // Slave to Master.
                    ToMaster();
                }
                else
                {
                    // Master to Slave.
                    ToSlave();
                }
            });
        }
        private void OnResourceChanged(object sender, RsChangeInfo e)
        {
            if (e.Content != null)
            {
                WorkingThread.Enqueue("OnResourceChanged", () =>
                {
                    if (!Master)
                    {
                        try
                        {
                            MemoryStream ms = new MemoryStream(Tools.Decompress(e.Content));
                            LastCfg = Serializer.Deserialize<Cd40Cfg>(ms);
                            using (FileStream file = File.Create(LastCfgFile))
                            {
                                Serializer.Serialize(file, LastCfg);
                            }
                            string json = JsonConvert.SerializeObject(LastCfg);
                            File.WriteAllText(LastCfgFileJson, json);
                        }
                        catch (Exception exc)
                        {
                            ExceptionManage<CfgService>("OnResourceChanged", exc, $"Slave save cfg to file {LastCfg.Version} Excepción => {exc.Message}");
                        }
                    }
                });
            }
        }
        private void OnMulticastMsg(object sender, DataGram dg)
        {
            if (dg.Data.Length > 0)
            {
                try
                {
                    var datastr = Encoding.Default.GetString(dg.Data, 0, dg.Data.Length);
                    var cmd = datastr.Substring(0, 1);
                    var par = datastr.Substring(1);
                    if (cmd == Settings.Default.ConfigByte)
                    {
                        var currentversion = par;
                        WorkingThread.Enqueue("OnMulticastMsg (Configuration change notification)", () =>
                        {
                            if (currentversion != LastCfg.Version)
                            {
                                LogInfo<CfgService>($"Recibida notificacion de cambio de configuracion => {currentversion}");
                                if (GetCfg(currentversion))
                                {
                                    PublishCfg();
                                    SaveCfg();
                                }
                            }
                        });
                    }
                }
                catch(Exception x)
                {
                    ExceptionManage<CfgService>("OnMulticastMsg", x, $"");
                }
            }
        }
        #endregion Managers

        #region Internals
        void Init()
        {
            CfgRegistry = new Registry(Identifiers.CfgMasterTopic);
            CfgRegistry.ChannelError += OnChannelError;
            CfgRegistry.MasterStatusChanged += OnMasterStatusChanged;
            CfgRegistry.ResourceChanged += OnResourceChanged;

            CfgRegistry.SubscribeToMasterTopic(Identifiers.CfgMasterTopic);
            CfgRegistry.SubscribeToTopic<Cd40Cfg>(Identifiers.CfgTopic);
            CfgRegistry.SubscribeToTopic<SrvMaster>(Identifiers.CfgMasterTopic);
            CfgRegistry.Join(Identifiers.CfgMasterTopic, Identifiers.CfgTopic);

            InitCfgChangesListener();
        }
        void Clear()
        {
            ClearInitCfgChangesListener();

            CfgRegistry.ChannelError -= OnChannelError;
            CfgRegistry.MasterStatusChanged -= OnMasterStatusChanged;
            CfgRegistry.ResourceChanged -= OnResourceChanged;
            CfgRegistry.Dispose();
            CfgRegistry = null;
        }
        void ToSlave()
        {
            try
            {
                ClearInitCfgChangesListener();

                CfgRegistry?.SetValue(Identifiers.CfgTopic, Identifiers.CfgRsId, (Cd40Cfg)null);
                CfgRegistry?.Publish(null, false);
            }
            catch (Exception x)
            {
                ExceptionManage<CfgService>("OnMasterStatusChangeToSlave", x, "Cambiando a SLAVE. Excepcion: " + x.Message);
            }
            finally
            {
                LogInfo<CfgService_old>("CfgService => SLAVE");
            }

        }
        void ToMaster()
        {
            if (LastCfg == null)
            {
                if (File.Exists(LastCfgFile))
                {
                    using (FileStream file = File.OpenRead(LastCfgFile))
                    {
                        try
                        {
                            LastCfg = Serializer.Deserialize<Cd40Cfg>(file);
                        }
                        catch (Exception x)
                        {
                            ExceptionManage<CfgService>("ToMaster", x, $"On Saving BIN Configuration");
                        }
                    }
                }
                else if (File.Exists(LastCfgFileJson))
                {
                    try
                    {
                        var content = File.ReadAllText(LastCfgFileJson);
                        LastCfg = JsonConvert.DeserializeObject<Cd40Cfg>(content);
                    }
                    catch (Exception x)
                    {
                        ExceptionManage<CfgService>("ToMaster", x, $"On Saving JSON Configuration");
                    }
                }
            }
            if (LastCfg != null)
            {
                CfgRegistry?.SetValue(Identifiers.CfgTopic, Identifiers.CfgRsId, LastCfg);
                CfgRegistry?.Publish(LastCfg.Version);
            }
            else
            {
                LogInfo<CfgService_old>("No cfg file found", U5kiIncidencias.U5kiIncidencia.IGRL_U5KI_NBX_INFO, "CfgService", "MASTER");
            }
            /** Provoca una chequeo inmediato de la Configuracion */
            LastCheckDate = DateTime.MinValue;
        }
        void InitCfgChangesListener()
        {
            if (CfgChangesListener == null)
            {
                using (SoapCfg.InterfazSOAPConfiguracion soapSrv = new U5ki.CfgService.SoapCfg.InterfazSOAPConfiguracion())
                {
                    soapSrv.Timeout = SoapTimeout;
                    var mcp = TrySoap.Get(Settings.Default.CfgSystemId, soapSrv.GetParametrosMulticast, TrySoapLog);
                    if (mcp.First == false) return;

                    CfgChangesListener = new UdpSocket(Settings.Default.MCastItf4Config, ((int)mcp.Second?.PuertoMulticastConfiguracion));
                    CfgChangesListener.MaxReceiveThreads = 1;
                    CfgChangesListener.NewDataEvent += OnMulticastMsg;
                    CfgChangesListener.Base.JoinMulticastGroup(IPAddress.Parse(mcp.Second?.GrupoMulticastConfiguracion));
                    CfgChangesListener.BeginReceive();
                }
            }
        }
        void ClearInitCfgChangesListener()
        {
            if (CfgChangesListener != null) CfgChangesListener.NewDataEvent -= OnMulticastMsg;
            CfgChangesListener?.Dispose();
            CfgChangesListener = null;
        }
        void TestCfg(Action<string> ExecuteChange)
        {
            using (SoapCfg.InterfazSOAPConfiguracion soapSrv = new U5ki.CfgService.SoapCfg.InterfazSOAPConfiguracion())
            {
                try
                {
                    soapSrv.Timeout = SoapTimeout;
                    /** Comprueba si han habido cambios de configuracion */
                    string soapVersion = soapSrv.GetVersionConfiguracion(Settings.Default.CfgSystemId);
                    if ((LastCfg != null) && (soapVersion == LastCfg.Version))
                    {
                        LogTrace<CfgService>($"Configuración actual {LastCfg.Version} coincide con configuración SOAP {soapVersion}");
                        return;
                    }
                    ExecuteChange(soapVersion);
                }
                catch(Exception x)
                {
                    ExceptionManage<CfgService>("TestCfg", x, $"On Testing Config");
                }
            }
        }
        bool GetCfg(string version)
        {
            string systemId = Settings.Default.CfgSystemId;
            using (SoapCfg.InterfazSOAPConfiguracion soapSrv = new U5ki.CfgService.SoapCfg.InterfazSOAPConfiguracion())
            {
                soapSrv.Timeout = SoapTimeout;
                /** Parámetros Multicast*/
                var mcp = TrySoap.Get(systemId, soapSrv.GetParametrosMulticast, TrySoapLog);
                if (mcp.First == false) return false;
                /** Obtiene una copia de la configuracion en el formato SOAP del servicio */
                var soapSysCfg = TrySoap.Get(systemId, soapSrv.GetConfigSistema, TrySoapLog);
                if (soapSysCfg.First == false) return false;
                /** Extrae de la Configuracion SOAP la lista de 'hosts' ??? */
                var hosts = soapSysCfg.Second?.PlanAsignacionUsuarios
                    .Where(h => !string.IsNullOrEmpty(h.IdUsuario))
                    .Select(h => h.IdHost).Distinct()
                    .ToList();
                /** De la lista de Hosts, determina los pares Host / usuarios */
                var IdHostsIdUser = hosts.Select(h => new Pair<string, SoapCfg.LoginTerminalVoz>(h, TrySoap.Get(systemId, h, soapSrv.LoginTop, TrySoapLog).Second))
                    .Where(p => p.Second != default && !string.IsNullOrEmpty(p.Second.IdUsuario))
                    .Select(u => new Pair<string,string>(u.First, u.Second.IdUsuario))
                    .ToList();
                /** Configuracion HF */
                var hfpool = TrySoap.Get(systemId, soapSrv.GetPoolHfElement, TrySoapLog);
                if (hfpool.First == false) return false;

                /** De la lista Host / usuarios determina los usuarios y los dominantes*/
                var soapUsers = IdHostsIdUser.Select(u => new Pair<string, SoapCfg.CfgUsuario>(u.First, TrySoap.Get(systemId, u.Second, soapSrv.GetCfgUsuario, TrySoapLog).Second))
                    .Where(su => su.Second != default)
                    .ToList();
                /** TODO Ojo a repetidos */
                var dominantUsers = soapUsers.ToDictionary(p => p.First, p => p.Second.IdIdentificador);
                /** Configuracion M+N */
                var nmPool = TrySoap.Get(systemId, TYPE_POOL_NM, soapSrv.GetPoolNMElements, TrySoapLog);
                if (nmPool.First == false) return false;
                /** De la lista de Hosts, determina los usuarios y de enlaces a usuarios internos y externos ??? y de 'usarios dominantes' ??? */
                var soapUsersExLinks = IdHostsIdUser.Select(p => TrySoap.Get(systemId, p.Second, soapSrv.GetListaEnlacesExternos, TrySoapLog).Second)
                    .ToList();
                /** Configuracion M+N */
                var eePool = TrySoap.Get(systemId, TYPE_POOL_EE, soapSrv.GetPoolNMElements, TrySoapLog);
                if (eePool.First == false) return false;

                var soapUsersInLinks = IdHostsIdUser.Select(p => TrySoap.Get(systemId, p.Second, soapSrv.GetListaEnlacesInternos, TrySoapLog).Second)
                    .ToList();

                /** Transformar la configuracion recibida a la que se va a distribuir (formato NBX) */
                Cd40Cfg cfg = new Cd40Cfg();
                try
                {
                    cfg.Version = version;
                    cfg.CfgMcastGroup = mcp.Second.GrupoMulticastConfiguracion;
                    cfg.CfgMcastPort = (int)mcp.Second.PuertoMulticastConfiguracion;

                    cfg.ConfiguracionGeneral = new ConfiguracionSistema();
                    CfgTranslators.Translate(cfg.ConfiguracionGeneral, soapSysCfg.Second);

                    /** Añade los Usuarios... */
                    for (int i = 0, to = soapUsers.Count; i < to; i++)
                    {
                        ConfiguracionUsuario user = new ConfiguracionUsuario();
                        CfgTranslators.Translate(user, soapUsers[i].Second, soapUsersExLinks[i], soapUsersInLinks[i]);
                        cfg.ConfiguracionUsuarios.Add(user);
                    }

                    /** Añade los usuarios 'dominantes' */
                    foreach (KeyValuePair<string, string> p in dominantUsers)
                    {
                        AsignacionUsuariosDominantesTV dominantUser = new AsignacionUsuariosDominantesTV
                        {
                            IdHost = p.Key,
                            IdUsuario = p.Value
                        };
                        cfg.ConfiguracionGeneral.PlanAsignacionUsuariosDominantes.Add(dominantUser);
                    }

                    /** Equipos HF, M+N y EE */
                    hfpool.Second?.ToList().ForEach(hf => CfgTranslators.Translate(cfg, hf));
                    nmPool.Second?.ToList().ForEach(nm => CfgTranslators.Translate(cfg, nm));
                    eePool.Second?.ToList().ForEach(ee => CfgTranslators.Translate(cfg, ee, false));

                    LastCfg = cfg;
                }
                catch (Exception x)
                {
                    ExceptionManage<CfgService>("GetCfg", x, $"On Translating Configuration.");
                }
                return true;
            }
        }
        void SaveCfg()
        {
            try
            {
                using (FileStream file = File.Create(LastCfgFile))
                {
                    Serializer.Serialize(file, LastCfg);
                }
                string json = JsonConvert.SerializeObject(LastCfg);
                File.WriteAllText(LastCfgFileJson, json);
            }
            catch (Exception x)
            {
                ExceptionManage<CfgService>("SaveCfg", x, $"Exception On saving to file configuration {LastCfg.Version} Excepción: {x.Message}");
            }
        }
        void PublishCfg()
        {
            LogInfo<CfgService>("Publicando nueva configuración: " + LastCfg.Version, U5kiIncidencias.U5kiIncidencia.IGRL_U5KI_NBX_INFO,
                "CfgService", CTranslate.translateResource("Publicando nueva configuración: " + LastCfg.Version));

            CfgRegistry.SetValue(Identifiers.CfgTopic, Identifiers.CfgRsId, LastCfg);
            CfgRegistry.Publish(LastCfg.Version);
        }
        void TrySoapLog(string msg, Exception x = null)
        {
            LogDebug<CfgService>(msg);
            if (x != null)
            {
                ExceptionManage<CfgService>("TrySoapLog", x, $"On TrySopLog");
            }
        }
        class TrySoap : BaseCode
        {
            public static Pair<bool, T> Get<T>(string p, Func<string, T> method, Action<string, Exception> log)
            {
                try
                {
                    log?.Invoke($"method => {method} p => {p}", null);
                    return new Pair<bool, T>(true, method(p));
                }
                catch (Exception x)
                {
                    log?.Invoke($"method => {method}, p => {p}, Exception => {x.Message}", x);
                    var cont = !(x is WebException) || (x as WebException).Status != WebExceptionStatus.Timeout;
                    return new Pair<bool, T>(cont, default);
                }
            }
            public static Pair<bool, T> Get<T>(string p1, string p2, Func<string, string, T> method, Action<string, Exception> log)
            {
                try
                {
                    log?.Invoke($"method => {method}, p1 => {p1}, p2 => {p2}", null);
                    return new Pair<bool, T>(true, method(p1, p2));
                }
                catch (Exception x)
                {
                    log?.Invoke($"method => {method}, p => {p1}, p2 => {p2}, Exception => {x.Message}", x);
                    var cont = !(x is WebException) || (x as WebException).Status != WebExceptionStatus.Timeout;
                    return new Pair<bool, T>(cont, default);
                }
            }
        }

        #endregion Internals

        #region Properties
        private EventQueue WorkingThread { get; set; }
        private Registry CfgRegistry { get; set; }
        private UdpSocket CfgChangesListener { get; set; }
        private Cd40Cfg LastCfg { get; set; }
        private Task SupervisionTask { get; set; }
        private ManualResetEvent SupervisionTaskSync { get; set; }
        private DateTime LastCheckDate { get; set; }

        #endregion

    }

}
