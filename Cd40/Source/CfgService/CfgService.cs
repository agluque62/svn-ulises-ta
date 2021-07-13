using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using U5ki.Infrastructure;
using U5ki.CfgService.Properties;

using Utilities;
using ProtoBuf;
using NLog;

using Translate;
using Newtonsoft.Json;
namespace U5ki.CfgService
{
    class CfgService : BaseCode, IService, IDisposable
    {
        const string LastCfgFile = "u5ki.LastCfg.bin";
        const string LastCfgFileJson = "u5ki.LastCfg.json";
        const string TYPE_POOL_NM = "0";
        const string TYPE_POOL_EE = "1";

        #region IService
        public string Name => "Cd40ConfigService";

        public ServiceStatus Status { get; set; }

        public bool Master { get; set; }

        public object AllDataGet()
        {
            throw new NotImplementedException();
        }

        public bool Commander(ServiceCommands cmd, string par, ref string err, List<string> resp = null)
        {
            throw new NotImplementedException();
        }

        public bool DataGet(ServiceCommands cmd, ref List<object> rsp)
        {
            throw new NotImplementedException();
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
        }

        #endregion Constructors

        #region Methods
        private void Init()
        {

        }
        private void Clear()
        {

        }
        private void SupervisionRoutine()
        {
            while (SupervisionTaskSync.WaitOne(TimeSpan.FromMilliseconds(100)) == false)
            {
                // Tarea 1. Configurar la escucha de los avisos MCAST.

                // Tarea 2. Chequear periodicamente la configuracion.
            }
        }
        private void OnChannelError(object sender, string error)
        {
            LogError<CfgService>($"CfgService OnChannelError => {error}");
            Stop();
        }
        private void OnMasterStatusChanged(object sender, bool master)
        {
            WorkingThread.Enqueue("OnMasterStatusChanged", () =>
            {
                Master = master;
                //Para que el servicio de configuración entre el último master
                if (Master) Task.Delay(TimeSpan.FromSeconds(3)).Wait();
                if (Master)
                {
                    // Slave to Master.
                    Slave2Master();
                }
                else
                {
                    // Master to Slave.
                    Master2Slave();
                }
            });
        }
        #endregion Methods

        #region Internals
        void Master2Slave()
        {
            try
            {
                CfgRegistry?.SetValue(Identifiers.CfgTopic, Identifiers.CfgRsId, (Cd40Cfg)null);
                CfgRegistry?.Publish(null, false);
            }
            catch (Exception x)
            {
                ExceptionManage<CfgService>("OnMasterStatusChangedSlave", x, "Cambiando a SLAVE. Excepcion: " + x.Message);
            }
            finally
            {
                LogInfo<CfgService_old>("CfgService => SLAVE");
            }

        }
        void Slave2Master()
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
                            // todo.
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
                        // todo
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
            // TODO OnCheckCfg(null, null);
        }
        #endregion Internals

        #region Properties
        private EventQueue WorkingThread { get; set; }
        private Registry CfgRegistry { get; set; }
        private UdpSocket CfgChangesListener { get; set; }
        private Cd40Cfg LastCfg { get; set; }
        private Task SupervisionTask { get; set; }
        private ManualResetEvent SupervisionTaskSync { get; set; }

        #endregion

    }
}
