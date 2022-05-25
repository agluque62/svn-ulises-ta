#define _SBCs_VERSION_
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Diagnostics;
using System.Timers;

using Newtonsoft.Json;
using NLog;

using U5ki.Infrastructure;
using U5ki.TifxService.Properties;

using Utilities;

using Translate;
namespace U5ki.TifxService
{
    public class TifxService : BaseCode, IService
    {
        string GroupIp { get; set; }
        string AdapterIp { get; set; }
        int GroupPort { get; set; }
        public TifxService()
        {
            GroupIp = Settings.Default.tifxMcastIp;
            AdapterIp = Settings.Default.tifxMcastSrc;
            GroupPort = Settings.Default.tifxMcastPort;
            last_cfg = null;
        }

        public TifxService(string groupIp, string adapterIp, int groupPort, Cd40Cfg cfg = null)
        {
            GroupIp = groupIp;
            GroupPort = groupPort;
            AdapterIp = adapterIp;
            last_cfg = cfg;
        }

        #region IService Members
        /// <summary>
        /// 
        /// </summary>
        public string Name
        {
            get { return "Cd40TifxService"; }
        }
        /// <summary>
        /// 
        /// </summary>
        public ServiceStatus Status
        {
            get { return _Status; }
        }

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
            return false;
        }

        /** 20170217. AGL. Nueva interfaz de comandos. Orientada a estructuras definidas en 'Infraestructure' */
        public bool DataGet(ServiceCommands cmd, ref List<Object> rsp)
        {
            if (cmd == ServiceCommands.TifxDataGet)
            {
                //foreach (GwInfo gw in _LastGwInfo.Values)
                //{
                //    rsp.Add(gw);
                //}
                rsp.Add(JData);
                return true;
            }

            return false;
        }
        public object AllDataGet()
        {
            return new
            {
                std = Status.ToString(),
                level = Status != ServiceStatus.Running ? "Error" : Master == true ? "Master" : "Slave",
                data = from gw in _LastGwInfo.Values
                       select new
                       {
                           id = gw.GwId,
                           ip = gw.GwIp,
                           tp = gw.Type,
                           ver = gw.Version,
                           res = from res in gw.Resources
                                 select new
                                 {
#if _SBCs_VERSION_
                                     id = res.Type > 4 ? res.GwIp : res.RsId,
#else
                                     id = res.RsId,
#endif
                                     dep = GetResourceDep(res), // ;GetGwResourceIpInfo(res, gw.GwIp),
                                     prio = res.Priority,
                                     std = res.State,
                                     tp = res.Type,
                                     ver = res.Version
                                 }
                       }
            };
        }

        /** */
        private string JData
        {
            get
            {
                var data = from gw in _LastGwInfo.Values
                           select new
                           {
                               id = gw.GwId,
                               ip = gw.GwIp,
                               tp = gw.Type,
                               ver = gw.Version,
                               res = from res in gw.Resources
                                     select new
                                     {
#if _SBCs_VERSION_
                                         id = res.Type > 4 ? res.GwIp : res.RsId,
#else
                                         id = res.RsId,
#endif
                                         dep = GetResourceDep(res), // ;GetGwResourceIpInfo(res, gw.GwIp),
                                         prio = res.Priority,
                                         std = res.State,
                                         tp = res.Type,
                                         ver = res.Version
                                     }
                           };

                return JsonConvert.SerializeObject(data);
            }
        }

        /** Fin de la Modificacion */

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
                LogInfo<TifxService>("Iniciando Servicio...");
                ExceptionManageInit();

                _Master = false;
                _Status = ServiceStatus.Running;

                _WorkingThread.Start();

                _Timer.Interval = 1000;
                _Timer.AutoReset = false;
                _Timer.Elapsed += OnTimeElapsed;
                _Timer.Enabled = true;

                /** 20180220. Abro el puerto con la propiedad de comparticion */
                _GwChangesListener = new UdpSocket(GroupPort, true);
                _GwChangesListener.MaxReceiveThreads = 1;
                _GwChangesListener.NewDataEvent += OnNewData;

                /** AGL2014. Seleccion Fuente de Multicast */
                // _GwChangesListener.Base.JoinMulticastGroup(IPAddress.Parse(Settings.Default.tifxMcastIp));
                _GwChangesListener.Base.JoinMulticastGroup(IPAddress.Parse(GroupIp), IPAddress.Parse(AdapterIp));
                /** Fin de Modificacion */

                /************************/
                /** 20180709. Notifico el estado al Servicio de presencia antes de inicializar el registro ya que hay veces que los mensajes 
                 SLAVE (inicial) y MASTER llegan en orden incorrecto....*/
                ipc.Raise<bool>(_Master);
                InitRegistry();
                _GwChangesListener.BeginReceive();
                _Timer.Enabled = true;

                LogInfo<TifxService>("Servicio Iniciado.", U5kiIncidencias.U5kiIncidencia.IGRL_U5KI_NBX_INFO, "TifxService", CTranslate.translateResource("Servicio iniciado"));
#if _LOCKING_
                }
#endif
            }
            catch (Exception ex)
            {
                ExceptionManage<TifxService>("Start", ex, "OnStart Exception: " + ex.Message);
                Stop();
            }
        }

        /// <summary>
        /// InitRegistry realiza la inicializaci�n del _Registry
        /// </summary>
        private void InitRegistry()
        {
            _Registry = new Registry(Identifiers.GwMasterTopic);
            _Registry.ChannelError += OnChannelError;
            _Registry.MasterStatusChanged += OnMasterStatusChanged;
            /** Para tener acceso a la configuracion */
            _Registry.ResourceChanged += OnResourceChanged;

            _Registry.SubscribeToMasterTopic(Identifiers.GwMasterTopic);
            _Registry.SubscribeToTopic<SrvMaster>(Identifiers.GwMasterTopic);            
            _Registry.SubscribeToTopic<Cd40Cfg>(Identifiers.CfgTopic);

            _Registry.Join(Identifiers.GwMasterTopic, Identifiers.GwTopic, Identifiers.CfgTopic);
        }
        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            LogInfo<TifxService>("Iniciando parada servicio.");
#if _LOCKING_
            _WorkingThread.Stop();
            lock(_lock)
            {
			    if (_Status == ServiceStatus.Running)
			    {
				    Dispose();
				    _Status = ServiceStatus.Stopped;
			    }
            }
#else
            /** */
            _Timer.Elapsed -= OnTimeElapsed;
            _Timer.Enabled = false;
            /** */

            _WorkingThread.Stop();

            if (_Status == ServiceStatus.Running)
            {
                Dispose();
                _Status = ServiceStatus.Stopped;
            }
#endif
            LogInfo<TifxService>("Servicio Detenido.", U5kiIncidencias.U5kiIncidencia.IGRL_U5KI_NBX_INFO, "TifxService", CTranslate.translateResource("Servicio Detenido"));
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
        private Timer _Timer = new Timer();
        /// <summary>
        /// 
        /// </summary>
        private Registry _Registry;
        /// <summary>
        /// 
        /// </summary>
        private UdpSocket _GwChangesListener;

        /// <summary>
        /// AGL-2015. Genera Eventos de Master ON-OFF de este servicio...
        /// </summary>
        private InterProcessEvent ipc = new InterProcessEvent("tifx_master");
        /// <summary>
        /// 
        /// </summary>
        private Dictionary<string, GwInfo> _LastGwInfo = new Dictionary<string, GwInfo>();
#if _LOCKING_
        private Object _lock = new Object();
#endif
        private int PabxFramesCount = 0;
        /// <summary>
        /// 
        /// </summary>
        private void Dispose()
        {
            _Timer.Enabled = false;

            if (_Registry != null)
            {
                _Registry.Dispose();
                _Registry = null;
            }
            if (_GwChangesListener != null)
            {
                _GwChangesListener.Dispose();
                _GwChangesListener = null;
            }

            _LastGwInfo.Clear();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rsInfo"></param>
        private void SetGwResource(RsInfo rsInfo, string gwIp)
        {
            //string RsId = GetGwResourceId(rsInfo);
            /** Linea Caliente */
            if (rsInfo.Type == 2)
            {
                GwLcRs rs = new GwLcRs();

                rs.GwIp = gwIp;

                _Registry.SetValue<GwLcRs>(Identifiers.GwTopic, rsInfo.Key, rs);
                LogDebug<TifxService>($"Publicando recurso LC [{rsInfo.Key}]: {rs}");
            } 
                /** Los demas tipos se consideran Recursos de Telefon�a */
            else if (rsInfo.Type /*== 1*/ <= 8)
            {
                GwTlfRs rs = new GwTlfRs();
                /** 20180214 */
                rs.GwIp = GetGwResourceIpInfo(rsInfo, gwIp);
                rs.Priority = rsInfo.Priority;
                rs.CallBegin = rsInfo.CallBegin;
                /** 20180212. Marca el tipo de recurso telefonico */
                rs.Type = rsInfo.Type;

                switch (rsInfo.State)
                {
                    case 0:
                        rs.St = GwTlfRs.State.Idle;
                        break;
                    case 1:
                        rs.St = GwTlfRs.State.BusyInterruptionAllow;
                        break;
                    case 2:
                        rs.St = GwTlfRs.State.BusyInterruptionNotAllow;
                        break;
                    case 3: /** Nuevo estado no disponible */
                        rs.St = GwTlfRs.State.NotAvailable;
                        break;
                    default:
                        LogDebug<TifxService>($"Estado desconocido para recurso telefonico {rsInfo}");
                        return;
                }
                _Registry.SetValue<GwTlfRs>(Identifiers.GwTopic, rsInfo.Key, rs);

                LogDebug<TifxService>($"Publicando Resource {rsInfo.Key} => [{rsInfo}");
            }
            else
            {
                LogError<TifxService>($"Recibida INFO no contemplada al actualizar recurso. Recurso {rsInfo}");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rsInfo"></param>
        private void RemoveGwResource(RsInfo rsInfo, string motivo)
        {
            //string RsId = GetGwResourceId(rsInfo);
            /** Linea Caliente */
            if (rsInfo.Type == 2)
            {
                _Registry.SetValue<GwLcRs>(Identifiers.GwTopic, rsInfo.Key, null);
                LogDebug<TifxService>($"Eliminando recurso LC {rsInfo.Key} => [{rsInfo}] por {motivo}");
            }
            else if (rsInfo.Type == 1)
            {
                _Registry.SetValue<GwTlfRs>(Identifiers.GwTopic, rsInfo.Key, null);
                LogDebug<TifxService>($"Eliminando recurso TLF {rsInfo.Key} => [{rsInfo}] por {motivo}");
            }
            /** Los demas tipos se consideran Recursos de Telefon�a */
            else if (rsInfo.Type < 9)
            {
                _Registry.SetValue<GwTlfRs>(Identifiers.GwTopic, rsInfo.Key, null);
                LogDebug<TifxService>($"Eliminando recurso TLF {rsInfo.Key} => [{rsInfo}] por {motivo}");
            }
            else
            {
                LogError<TifxService>($"Recibida INFO no contemplada al eliminar recurso. Recurso {rsInfo}");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="gwInfo"></param>
        private void SetGwResources(GwInfo gwInfo)
        {
            foreach (RsInfo rsInfo in gwInfo.Resources)
            {
                SetGwResource(rsInfo, gwInfo.GwIp);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="gwInfo"></param>
        private void ProcessGwInfo(GwInfo gwInfo)
        {
            if (_Master)
            {
                try
                {
                    GwInfo oldGwInfo = null;

                    if (_LastGwInfo.TryGetValue(gwInfo.GwId, out oldGwInfo))
                    {
                        if (oldGwInfo.Version != gwInfo.Version ||
                            oldGwInfo.GwIp != gwInfo.GwIp)
                        {
                            var forDelete = new List<RsInfo>();
                            var forActualize = new List<RsInfo>();
                            foreach (RsInfo rsInfo in gwInfo.Resources)
                            {
                                try
                                {
                                    var oldRsInfo = oldGwInfo.Resources
                                        .Where(r => r.Type == rsInfo.Type && r.Key == rsInfo.Key)
                                        .FirstOrDefault();
                                    if (oldRsInfo != null) oldRsInfo.Steps = uint.MaxValue;
                                    //RsInfo oldRsInfo = Array.Find(oldGwInfo.Resources, delegate (RsInfo rsi)
                                    //{
                                    //    if ((rsInfo.Type == rsi.Type) && (string.Compare(rsInfo.RsId, rsi.RsId, true) == 0)) //agl
                                    //    {
                                    //        rsi.Steps = uint.MaxValue;
                                    //        return true;
                                    //    }
                                    //    return false;
                                    //});
                                    if ((oldRsInfo == null) || (oldRsInfo.Version != rsInfo.Version))
                                    {
                                        //SetGwResource(rsInfo, gwInfo.GwIp);
                                        forActualize.Add(rsInfo);
                                    }
                                }
                                catch(Exception x)
                                {
                                    LogException<TifxService>($"Excepcion procesesando Recurso {rsInfo}  de GwInfo => {gwInfo} => ", x, false);
                                }
                            }
                            foreach (RsInfo rsInfo in oldGwInfo.Resources)
                            {
                                try
                                {
                                    if (rsInfo.Steps != uint.MaxValue)
                                    {
                                        //RemoveGwResource(rsInfo, "mensaje recibido");
                                        forDelete.Add(rsInfo);
                                    }
                                }
                                catch(Exception x)
                                {
                                    LogException<TifxService>($"Excepcion eliminando Recurso {rsInfo}  de GwInfo => {gwInfo} => ", x, false);
                                }
                            }
                            // Primero los Borrados
                            forDelete.ForEach((rsInfo) => RemoveGwResource(rsInfo, "mensaje recibido"));
                            _Registry.Publish();
                            // Despues los Modificados
                            forActualize.ForEach((rsInfo) => SetGwResource(rsInfo, gwInfo.GwIp));
                            _Registry.Publish();
#if DEBUG
                            LogDebug<TifxService>(String.Format("TIFX {0}. Mensaje Version {1,2} ({3} Users) Procesado. TickCount: {2}",
                                gwInfo.GwId, gwInfo.Version, gwInfo.LastReceived, gwInfo.Resources.Length));
#endif
                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        SetGwResources(gwInfo);
                        _Registry.Publish();
                    }
                }
                catch (Exception ex)
                {
                    LogException<TifxService>($"Excepcion procesesando GwInfo => {gwInfo} => ", ex, false);
                    // TODO. Revisar esto...
                    // ExceptionManage<TifxService>("ProcessGwInfo", ex, "OnProcessGwInfo Exception: " + ex.Message);
                    // _WorkingThread.InternalStop();
                    // Dispose();
                    // _Status = ServiceStatus.Stopped;
                }
            }

            gwInfo.LastReceived = Environment.TickCount;
            _LastGwInfo[gwInfo.GwId] = gwInfo;

            LogTrace<TifxService>(String.Format("{3}. TIFX {0}. Mensaje Version {1,2} Recibido. TickCount: {2}", 
                gwInfo.GwId, gwInfo.Version, gwInfo.LastReceived, PabxFramesCount));
        }

        void ProcessInfo(GwInfo newInfo)
        {
            if (!_Master) return;
            var CurrentTick = Environment.TickCount;
            LogTrace<TifxService>($"Recibido Mensaje de {newInfo.GwId} => [Ver: {newInfo.Version}, ResCount: {newInfo.Resources.Length}], TickCount: {CurrentTick} ");

            _LastGwInfo.TryGetValue(newInfo.GwId, out var oldInfo);
            if (oldInfo == null)
            {
                _LastGwInfo[newInfo.GwId] = newInfo;
                _Registry.Publish();
            }
            else
            {
                var changes = newInfo.Version != oldInfo.Version || newInfo.GwIp != oldInfo.GwIp;
                if (changes)
                {

                }
                else
                {
                    LogTrace<TifxService>($"No changes received de {newInfo.GwId}");
                }

            }
            newInfo.LastReceived = CurrentTick;
        }
        /// <summary>
        /// 
        /// </summary>
        private void CheckTouts()
        {
            try
            {
                List<string> gwToRemove = new List<string>();

                foreach (KeyValuePair<string, GwInfo> p in _LastGwInfo)
                {
                    GwInfo gwInfo = p.Value;

                    //LogDebug<TifxService>(
                    //    String.Format("Supervisando GWINFO {0}, con {1} Recursos", gwInfo.GwId, gwInfo.Resources.Length));

                    int? lastReceived = gwInfo.LastReceived;
                    if (General.TimeElapsed(ref gwInfo.LastReceived, Settings.Default.tifxPresenceTout))
                    {
                        LogDebug<TifxService>(String.Format("TIFX {0}. TIMEOUT. LastRecieved {1}, TickCount {2}",
                            gwInfo.GwId, lastReceived, Environment.TickCount));

                        gwToRemove.Add(p.Key);

                        foreach (RsInfo rsInfo in gwInfo.Resources)
                        {
                            RemoveGwResource(rsInfo, "temporizador");
                        }
                    }
                }

                if (gwToRemove.Count > 0)
                {
                    _Registry.Publish();

                    foreach (string gwId in gwToRemove)
                    {
                        _LastGwInfo.Remove(gwId);
                    }
                }
            }
            finally
            {
                _Timer.Enabled = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void RefreshActiveProxiesAndSubscribers()
        {
            foreach (GwInfo gwInfo in _LastGwInfo.Values)
            {
                if (gwInfo.Type == 3 || gwInfo.Type == 4)
                {
                    foreach (RsInfo rsInfo in gwInfo.Resources)
                    {
                        SetGwResource(rsInfo, gwInfo.GwIp);
                    }
                }
            }
            _Registry.Publish();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="error"></param>
        private void OnChannelError(object sender, string error)
        {
            _WorkingThread.Enqueue("OnChannelError", delegate()
            {
#if _LOCKING_
                lock (_lock)
                {
#endif
                LogError<TifxService>(error);

                _WorkingThread.InternalStop();
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

            _WorkingThread.Enqueue("OnMasterStatusChanged", delegate()
            {
                try
                {
#if _LOCKING_
                    lock (_lock)
                    {
#endif
                    if (_Master)
                    {
                        if (!_Timer.Enabled)
                            _Timer.Enabled = true;
                        foreach (GwInfo gwInfo in _LastGwInfo.Values)
                        {
                            SetGwResources(gwInfo);
                        }

                        _Registry.Publish();                        
                        LogInfo<TifxService>("MASTER", U5kiIncidencias.U5kiIncidencia.IGRL_U5KI_NBX_INFO, "TifxService", "MASTER");
                    }
                    else
                    {
                        _Registry.SetValue(Identifiers.GwTopic, null, null, null);
                        _Registry.Publish(null, false);
                        LogInfo<TifxService>("SLAVE", U5kiIncidencias.U5kiIncidencia.IGRL_U5KI_NBX_INFO, "TifxService", "SLAVE");
                    }

                    // AGL-2015. Notifica Evento MASTER...
                    ipc.Raise<bool>(_Master);
#if _LOCKING_
                    }
#endif
                }
                catch (Exception ex)
                {
                    //LogException<TifxService>("ERROR cambiando a estado master " + _Master, ex);
                    ExceptionManage<TifxService>("OnMasterStatusChanged", ex, "OnMasterStatusChanged => " + _Master.ToString() + " Exception: " + ex.Message);

                    _WorkingThread.InternalStop();
                    Dispose();
                    _Status = ServiceStatus.Stopped;
                }
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="dg"></param>
        private void OnNewData(object sender, DataGram dg)
        {
            if (!_Master)
                return;
            //            if ((dg.Data.Length >= 4) && (dg.Data[3] == 0x01))
            // AGL. Tramas Tipo 2 son del servicio PABX. La IP viene en el ID de Pasarela... 
            /** */
            bool TipoPermitido = (dg.Data[3] == 0x01 || dg.Data[3] == 0x05 || dg.Data[3] == 0x03 || dg.Data[3] == 0x04);
            if ((dg.Data.Length >= 4) && TipoPermitido)
            {
                try
                {
                    using (MemoryStream ms = new MemoryStream(dg.Data))
                    using (CustomBinaryFormatter bf = new CustomBinaryFormatter())
                    {
                        try
                        {
                            GwInfo gwInfo = bf.Deserialize<GwInfo>(ms);

                            _WorkingThread.Enqueue("ProcessGwInfo", delegate()
                            {
#if _LOCKING_
                            lock (_lock)
                            {
#endif
                                /** 20180208. */
                                switch (gwInfo.Type)
                                {
                                    case 5:     // Recursos de PABX Interna...
                                        gwInfo.GwIp = gwInfo.GwId;
                                        break;
                                    case 4:     // Informacion de Proxies...
                                        /** 20201021. SBCs. */
#if _SBCs_VERSION_
                                        gwInfo = NormalizeForProxies(gwInfo);
#else
                                        gwInfo.GwIp = gwInfo.GwId;
#endif
                                        break;
                                    case 3:     // Abonados Externos.
                                        ////Descarto datos del servicio de presencia de otro NBX
                                        //if (!IPAddress.Parse(Settings.Default.tifxMcastSrc).ToString().Equals(dg.Client.Address.ToString()))
                                        //    return;
                                        break;
                                    case 1:     // De Pasarela.
                                        gwInfo.GwIp = dg.Client.Address.ToString();
                                        break;
                                }

                                //if (gwInfo.Type == 1)
                                //{
                                //    gwInfo.GwIp = dg.Client.Address.ToString();
                                //}
                                //else if (gwInfo.Type == 2)
                                //{
                                //    gwInfo.GwIp = gwInfo.GwId;
                                //    PabxFramesCount++;
                                //}

                                ProcessGwInfo(gwInfo);
#if _LOCKING_
                            }
#endif
                            });
                        }
                        catch (Exception ex)
                        {
                            //LogException<TifxService>("ERROR parseando datos recibidos de pasarela", ex);
                            ExceptionManage<TifxService>("OnNewData", ex, "Excepcion " + ex.Message + ". Procesando Datos de Pasarela");
                        }
                    }
                }
                catch (Exception ex)
                {
                    ExceptionManage<TifxService>("OnNewData", ex, "Excepcion " + ex.Message + ". Serializando Datos de Pasarela");
                }
            }
            else if (dg.Data[3] != 10)  // El codigo 10 es un comando interno de la pasarela...
            {
                int max_len = dg.Data.Length <= 16 ? dg.Data.Length : 16;
                string packet = BitConverter.ToString(dg.Data).Substring(0, max_len * 3);
                LogError<TifxService>(String.Format("OnNewData: Paquete con error de formato: {0}", packet));
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private int tick_count = 0;
        private void OnTimeElapsed(object sender, ElapsedEventArgs e)
        {
            if (!_Master)
                return;
            _WorkingThread.Enqueue("TifxService:OnTimeElapsed", delegate()
            {
#if _LOCKING_
                lock (_lock)
                {
#endif
                    CheckTouts();   // Chequea que se estan enviando las tramas...

                    if ((tick_count++ % 60) == 0)   // TODO. Cada N segundos... Se reenvian las Info Activas tipos 3 y 5
                    {
                        RefreshActiveProxiesAndSubscribers();
                    }
#if _LOCKING_
                }
#endif
            });
        }

        /** 20180214. Para almacenar la configuracion */
        private Cd40Cfg last_cfg = null;
        List<U5ki.Infrastructure.DireccionamientoIP> Dependencias = null;
        private void OnResourceChanged(object sender, RsChangeInfo e)
        {
            _WorkingThread.Enqueue("TifxService:OnResourceChanged", delegate()
            {
                try
                {
                    if (e.Content != null)
                    {
                        MemoryStream ms = new MemoryStream(Tools.Decompress(e.Content));
                        last_cfg = ProtoBuf.Serializer.Deserialize<Cd40Cfg>(ms);
                        /** */
                        Dependencias = last_cfg.ConfiguracionGeneral.PlanDireccionamientoIP.Where(d =>
                            d.EsCentralIP == true).ToList();
                        LogDebug<TifxService>(String.Format("OnResourceChanged: Carga de Configuracion {0}", last_cfg.Version));
                    }
                }
                catch (Exception x)
                {
                    LogException<TifxService>("OnResourceChanged Exception", x, false);
                }
            });
        }

#endregion

#region Datos de la configuracion
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rsInfo"></param>
        /// <param name="gwIp"></param>
        /// <returns></returns>
        public string GetGwResourceIpInfo(RsInfo rsInfo, string gwIp)
        {
            /** Para recursos de pasarelas y abonados PBX, como hasta ahora */
            if (rsInfo.Type == 1 || rsInfo.Type == 2 || rsInfo.Type == 3)
                return gwIp;

            if (last_cfg != null)
            {
                if (rsInfo.Type == 4)
                {
                    /** Para abonandos. El ID es el SIP-URI <sip:UUUUU@zzz.yyy.xxx.www:ppppp> 
                        La ip debe contener la dependencia */
                    return (new SipUtilities.SipUriParser(rsInfo.RsId)).HostPort;
                }
                else if (rsInfo.Type < 9)
                {
#if _SBCs_VERSION_
                    return rsInfo.GwIp;
#else
                    /** Para proxies. El ID es el Endpoint zzz.yyy.xxx.www:ppppp 
                         La ip debe contener la dependencia */
                    return rsInfo.RsId;
#endif
                }
            }
            return gwIp;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rsInfo"></param>
        /// 
        /// <returns></returns>
//        public string GetGwResourceId(RsInfo rsInfo)
//        {
//            switch (rsInfo.Type)
//            {
//                case 4: // Para abonados ATS (Sip URI) => solo la parte de Usuario
//                    return (new SipUtilities.SipUriParser(rsInfo.RsId)).User;
//                case 7: // Para Proxies, combinaci�n DEP##EP
//                case 8:
//                    return rsInfo.Key4ExternalProxies;
//                default: // Para recursos de pasarelas y abonados PBX, como hasta ahora.
//                    return rsInfo.RsId;
//            }

////            /** Para recursos de pasarelas y abonados PBX, como hasta ahora */
////            if (rsInfo.Type == 1 || rsInfo.Type == 2 || rsInfo.Type == 3 || rsInfo.Type==5 || rsInfo.Type==6)
////                return rsInfo.RsId;

////            if (last_cfg != null)
////            {
////                if (rsInfo.Type == 4)
////                {
////                    /** Para abonandos. El ID es el SIP-URI <sip:UUUUU@zzz.yyy.xxx.www:ppppp> 
////                        La ip debe contener la dependencia */
////                    return (new SipUtilities.SipUriParser(rsInfo.RsId)).User;
////                }
////                else if (rsInfo.Type == 7 || rsInfo.Type==8)
////                {
////#if _SBCs_VERSION_
////                    return rsInfo.Key4ExternalProxies;
////#else
////                    /** Para proxies. El ID es el Endpoint zzz.yyy.xxx.www:ppppp , viene configurado con o sin puerto
////                        La ip debe contener la dependencia */
////                    string dominio = rsInfo.RsId;

////                    /** Busco la dependencia que contenga como proxy al dominio de la uri */
////                    var dependencia = last_cfg.ConfiguracionGeneral.PlanDireccionamientoIP
////                        .Where(dep => dep.TipoHost== Tipo_Elemento_HW.TEH_EXTERNO_TELEFONIA && (
////                            (dep.IpRed1 != "" && dominio.Contains(dep.IpRed1) == true) ||
////                            (dep.IpRed2 != "" && dominio.Contains(dep.IpRed2) == true) ||
////                            (dep.IpRed3 != "" && dominio.Contains(dep.IpRed3) == true) ))
////                        .FirstOrDefault();
////                    if (dependencia != null)
////                        return dependencia.IdHost;
////#endif
////                }
////            }

////            return rsInfo.RsId;
//        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rsInfo"></param>
        /// <returns></returns>
        protected string GetResourceDep(RsInfo rsInfo)
        {
            if (Dependencias == null)
                return "No configured";
            /** Para recursos de pasarelas y abonados PBX, como hasta ahora */
            if (rsInfo.Type == 1 || rsInfo.Type == 2 || rsInfo.Type == 3)
            {
                var CentralPropia = Dependencias.Where(dep => dep.Interno == true).FirstOrDefault();
                return CentralPropia == null ? "LOCAL" : CentralPropia.IdHost;
            }
#if _SBCs_VERSION_
            else if (rsInfo.Type == 4)
            {
                /** Para abonandos. El ID es el SIP-URI <sip:UUUUU@zzz.yyy.xxx.www:ppppp> 
                    La ip debe contener la dependencia */
                var dominio = new SipUtilities.SipUriParser(rsInfo.RsId).HostPort;
                var dependencia = Dependencias
                    .Where(dep =>
                        (dep.IpRed1 != "" && dominio.Contains(dep.IpRed1) == true) ||
                        (dep.IpRed2 != "" && dominio.Contains(dep.IpRed2) == true) ||
                        (dep.IpRed3 != "" && dominio.Contains(dep.IpRed3) == true))
                    .FirstOrDefault();
                if (dependencia != null)
                    return dependencia.IdHost;
            }
            else
            {
                /** Para los proxies ya est� formateado */
                return rsInfo.RsId;
            }
#else
            else if (rsInfo.Type < 9)
            {
                /** Para abonandos. El ID es el SIP-URI <sip:UUUUU@zzz.yyy.xxx.www:ppppp> 
                    La ip debe contener la dependencia */
                /** Para proxies. El ID es el Endpoint zzz.yyy.xxx.www:ppppp , viene configurado con o sin puerto
                    La ip debe contener la dependencia */
                var dominio = rsInfo.Type == 4 ? (new SipUtilities.SipUriParser(rsInfo.RsId)).HostPort : rsInfo.RsId;
                var dependencia = Dependencias
                    .Where(dep =>
                        (dep.IpRed1 != "" && dominio.Contains(dep.IpRed1) == true) ||
                        (dep.IpRed2 != "" && dominio.Contains(dep.IpRed2) == true) ||
                        (dep.IpRed3 != "" && dominio.Contains(dep.IpRed3) == true))
                    .FirstOrDefault();
                if (dependencia != null)
                    return dependencia.IdHost;
            }
#endif
            return "???";
        }
#if _SBCs_VERSION_
        /** 20201021. SBCs. */
        private class DepInfo4Proxy
        {
            public string name { get; set; }
            public int prio { get; set; }
        }
        private GwInfo NormalizeForProxies(GwInfo gwInfo)
        {
            gwInfo.GwIp = gwInfo.GwId;
            var proxies = gwInfo.Resources
                .ToList()
                .GroupBy(r => r.RsId).Select(r=>r.First());
            
            var tifxRes = new List<RsInfo>();

            foreach(var proxy in proxies)
            {
                ProxyDeps(proxy, (deps) =>
                {
                    deps.ToList().ForEach(dep =>
                    {
                        var newres = new RsInfo()
                        {
                            RsId = dep.name,
                            GwIp = proxy.RsId,
                            Type = proxy.Type,
                            Version = proxy.Version,
                            State = proxy.State,
                            Priority = (uint)dep.prio,
                            Steps = proxy.Steps,
                            CallBegin = proxy.CallBegin    
                        };
                        tifxRes.Add(newres);
                    });
                });
            }
            gwInfo.Resources = tifxRes.OrderBy(r => r.RsId).ThenBy(r=>r.Priority).ToArray();
            return gwInfo; 
        }
        private void ProxyDeps(RsInfo prx, Action<IEnumerable<DepInfo4Proxy>> DepsNotify)
        {
            var dominio = prx.RsId;
            /** Busco la dependencia que contenga como proxy al dominio de la uri */
            var deps = last_cfg.ConfiguracionGeneral.PlanDireccionamientoIP
                .Where(dep => dep.TipoHost == Tipo_Elemento_HW.TEH_EXTERNO_TELEFONIA && dep.EsCentralIP == true && (
                    (dep.IpRed1 != "" && dominio.Contains(dep.IpRed1) == true) ||
                    (dep.IpRed2 != "" && dominio.Contains(dep.IpRed2) == true) ||
                    (dep.IpRed3 != "" && dominio.Contains(dep.IpRed3) == true)))
                .GroupBy(d => d.IdHost).Select(d => d.First()) // Cuando las dependencias tienen asociado mas de un rango, aparecen repetidas.
                .Select(dep => new DepInfo4Proxy()
                {
                    name = dep.IdHost,
                    prio = (dep.IpRed1 != "" && dominio.Contains(dep.IpRed1)) ? 1 : (dep.IpRed2 != "" && dominio.Contains(dep.IpRed2)) ? 2 : 3
                });
            DepsNotify(deps);
        }
#endif
        #endregion

        #region TESTING
#if DEBUG
        public void RM4763_Test(Cd40Cfg cfg, GwInfo prxInfo)
        {
            last_cfg = cfg;
            var itemInfo = NormalizeForProxies(prxInfo);
        }
#endif
#endregion
    }
}
