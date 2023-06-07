using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

using ProtoBuf;
using NLog;

using U5ki.Infrastructure;
using Utilities;
using System.Runtime.CompilerServices;
using Lextm.SharpSnmpLib.Pipeline;
using System.Runtime.Remoting.Channels;

namespace U5ki.RdService
{
    static class LocalExtensions
    {
        public static string ListToString(this MSStatus @this)
        {
            var sb = new StringBuilder($"\n\tMSStatus List, Tx Main Entries {@this.main_nodes.Count()}:");
            @this.main_nodes.ForEach(node => sb.Append($"\n\t\t{node.res}, on {node.site}."));
            sb.Append($"\n\t               Rx Disabled Entries {@this.disabled_nodes.Count()}:");
            @this.disabled_nodes.ForEach(node => sb.Append($"\n\t\t{node.res}, on {node.site}."));
            return sb.ToString();
        }
    }

    public class MSTxPersistence
    {
        const string FileName = "MSTxPersistence.json";
        static public void Init(Registry rdReg)
        {
            RdRegRef = rdReg;

            RdRegRef.ResourceChanged += OnResourceChanged;
            RdRegRef.MasterStatusChanged += (wsender, master_status) =>
            {
                Master = master_status;
                //Sea master o slave mando el fichero que tengo. El otro debera compara las fecha y hora
                try
                {
                    Status = ServicesHelpers.DeserializeObject<MSStatus>(File.ReadAllText(FileName));
                    Log.Trace($"OnMasterStatusChanged File loaded. {Status.ListToString()}");
                    Publish();
                }
                catch (Exception x)
                {
                    Status.main_nodes.Clear();
                    Status.disabled_nodes.Clear();
                    Status.rd_destination.Clear();
                    Log.Error("OnMasterStatusChanged Loading File Exception", x);
                }
            };

            RdRegRef?.SubscribeToTopic<MSStatus>(Identifiers.RdTopic);
            Log.Trace("Modulo Incializado.");
        }

        static public bool SelectFrequency(string id_dest, string freq)
        {
            Log.Trace($"Selecting Frequency {id_dest}. Frequency {freq}");

            if (Master == false) return false;

            var res = DataAccess(() =>
            {
                /** Borro si esta*/
                Status.rd_destination?.RemoveAll(i => i.id_destino == id_dest);
                /** Añado la frecuencia */
                Status.rd_destination?.Add(new MSRdDestination() { id_destino = id_dest, selectedfrequency = freq });
                Log.Trace($"Frequency Selected {id_dest}. selectedfrequency {freq}");

                SaveAndPublish();
            });
            return res;
        }

        static public string GetSelectFrequency(string id_dest)
        {
            Log.Trace($"Get Selected Frequency {id_dest}.");

            MSRdDestination found = null;
            var res = DataAccess(() =>
            {
                found = Status.rd_destination.Where(i => i.id_destino == id_dest).ToList().FirstOrDefault();
                Log.Trace($"Resource {id_dest} is {found}");
            });
            return (found == null)? null : found.selectedfrequency;
        }


        /// <summary>
        /// Se supone que a esta rutina se llama en modo MASTER.
        /// </summary>
        /// <param name="frec"></param>
        /// <param name="main"></param>
        /// <param name="standby"></param>
        /// <returns></returns>
        static public bool SelectMain(IRdResource main, IRdResource standby)
        {
            Log.Trace($"Selecting Main {main.ID}. Standby {standby.ID}");
            
            if (Master == false) return false;

            var res = DataAccess(() =>
            {
                /** Borro los dos si estan */
                Status.main_nodes?.RemoveAll(i => i.res == main.ID || i.res == standby.ID);
                /** Añado el Main */
                Status.main_nodes?.Add(new MSNodeInfo() { site = main.Site ?? "none", res = main.ID });
                Log.Trace($"Main Selected {main.ID}. Standby {standby.ID}");

                SaveAndPublish();
            });
            return res;
        }
        static public bool IsMain(IRdResource resource)
        {
            Log.Trace($"Query Main for {resource.ID}");
            string found = default;
            var res = DataAccess(() =>
            {
                found = Status.main_nodes.Where(i => i.site == resource.Site && i.res == resource.ID)
                    .ToList()
                    .Count() > 0 ? "Main" : "StandBy";

                Log.Trace($"Resource {resource.ID} is {found}. {Status.ListToString()}");
            });
            return res ? found == "Main" : false;
        }
        static public bool DisableNode(IRdResource resource, bool disable)
        {
            Log.Trace($"Disabling node {resource.ID} ==> {disable}");

            if (Master == false) return false;
            
            var res = DataAccess(() =>
            {
                /** Lo borro de la lista */
                Status.disabled_nodes?.RemoveAll(i => i.res == resource.ID);
                /** */
                if (disable == true)
                {
                    Status.disabled_nodes?.Add(new MSNodeInfo() { site = resource.Site ?? "none", res = resource.ID });                
                    Log.Trace($"Node {resource.ID} Disabled");
                }
                else
                {
                    Log.Trace($"Node {resource.ID} Enabled");
                }

                SaveAndPublish();
            });
            return res;
        }
        static public bool IsNodeDisabled(IRdResource resource)
        {
            Log.Trace($"Query Disabled for {resource.ID}");
            string found = default;
            var res = DataAccess(() =>
            {
                found = Status.disabled_nodes.Where(i => i.site == resource.Site && i.res == resource.ID)
                    .ToList()
                    .Count() > 0 ? "Disabled" : "Enabled";

                Log.Trace($"Resource {resource.ID} is {found}. {Status.ListToString()}");
            });
            return res ? found == "Disabled" : false;
        }
        /// <summary>
        /// Se supone que a esta rutina se llama en modo MASTER.
        /// </summary>
        /// <param name="cfg"></param>
        static void ProcessNewConfig(Cd40Cfg cfg)
        {
            int modified = 0;
            Log.Trace($"Procesando Nueva Configuracion {cfg.Version}");
            try
            {
                /** Leer los datos consolidados */
                //var Info = ServicesHelpers.DeserializeObject<List<MSTransmiterInfo>>(File.ReadAllText(FileName));
                //Status.nodes_info.Clear();
                //Info.ForEach(i => Status.nodes_info.Add(i));
                Status = ServicesHelpers.DeserializeObject<MSStatus>(File.ReadAllText(FileName));
                Log.Trace($"File loaded. {Status.ListToString()}");
            }
            catch (Exception x)
            {
                Log.Error("Loading File Exception", x);
                Status.main_nodes.Clear();
                Status.disabled_nodes.Clear();
                modified++;
            }
            /** Ajustar los datos a la nueva configuracion. */
            var allRes = cfg.ConfiguracionUsuarios
                .SelectMany(u => u.RdLinks)
                .SelectMany(l => l.ListaRecursos)
                .GroupBy(r => r.IdRecurso)
                .Select(g => g.First())
//                .Where(r => r.Tipo == 1 || r.Tipo == 2)
                .ToList();

            /** Borra de las listas aquellos que han sido eliminadas de configuracion */
            modified += Status.main_nodes.RemoveAll(n => NotInCfg(allRes, n));
            modified += Status.disabled_nodes.RemoveAll(n => NotInCfg(allRes, n));

            var allfreq = cfg.ConfiguracionUsuarios
                .SelectMany(u => u.RdLinks)
                .ToList();

            modified += Status.rd_destination.RemoveAll(n => Rd_destination_NotInCfg(allfreq, n.id_destino));           

            if (modified > 0)
            {
                Log.Trace($"List Fixed. {Status.ListToString()}");
                SaveAndPublish();
            }
        }
        static void OnResourceChanged(object sender, RsChangeInfo e)
        {
            if (e.Type == Identifiers.TypeId(typeof(Cd40Cfg)) && e.Content != null)
            {
                try
                {
                    Log.Trace($"OnResourceChanged Cd40Cfg Event Received. Master {Master}");
                    if (Master == true)
                    {
                        MemoryStream ms = new MemoryStream(Tools.Decompress(e.Content));
                        var cfg = Serializer.Deserialize<Cd40Cfg>(ms);
                        DataAccess(() =>
                        {
                            ProcessNewConfig(cfg);
                        });
                    }
                }
                catch (Exception x)
                {
                    Log.Error("Deserializing Data Exception", x);
                }
            }
            else if (e.Type == Identifiers.TypeId(typeof(MSStatus)) && e.Content != null)
            {
                try
                {
                    //Tomo la fecha y hora de mi fichero actual
                    MSStatus prevStatus = new MSStatus();
                    string prev_datetime = null;
                    try
                    {
                        prevStatus = ServicesHelpers.DeserializeObject<MSStatus>(File.ReadAllText(FileName));
                        prev_datetime = prevStatus.datetime;
                        Log.Trace($"OnResourceChanged previous File loaded. {Status.ListToString()}");
                    }
                    catch (Exception x)
                    {
                        prevStatus = null;
                        prev_datetime = null;
                        Log.Error("OnResourceChanged Loading File Exception", x);
                    }

                    Log.Trace($"OnResourceChanged MSStatus Event Received.  Master {Master}");
                    MemoryStream ms = new MemoryStream(e.Content);
                    MSStatus MSNodesInfo = Serializer.Deserialize<MSStatus>(ms);
                    Log.Trace($"OnResourceChanged MSStatus Event Received. Master {Master}. Info = { MSNodesInfo.ListToString()}");
                    DataAccess(() =>
                    {
                        Status = MSNodesInfo;
                        int comp = CompareDateTime(prev_datetime, Status.datetime);

                        //Comparo la fecha y hora del recibido con el que tengo
                        if (comp < 0)
                        {
                            try
                            {
                                //El que me llega es posterior y es con el que me quedo
                                Log.Trace($"OnResourceChanged MSStatus Event Received. Master {Master}. Saved to {FileName}");
                                File.WriteAllText(FileName, ServicesHelpers.SerializeObject(MSNodesInfo/*.nodes_info*/));                                
                            }
                            catch (Exception x)
                            {
                                Log.Error("OnResourceChanged Writing File Exception", x);
                            }
                        }
                        else
                        {
                            //El que me llega es anterior o actual, por tanto lo descarto
                            Status = prevStatus;
                            Log.Trace($"OnResourceChanged MSStatus Event Received. It is rejected because it is previous");
                            if (comp > 0)
                            {
                                //Y si es anterior tambien envio el mio para que el resto de puestos se queden con el
                                Publish();
                            }
                        }                            
                    });
                }
                catch (Exception x)
                {
                    Log.Error("Deserializing Data Exception", x);
                }
            }
        }
        static void SaveAndPublish()
        {
            /** Publico los cambios */
            try
            {
                SetStatusDateTime();
                File.WriteAllText(FileName, ServicesHelpers.SerializeObject(Status/*.nodes_info*/));
                Publish();
                Log.Trace($"Data Saved and Published. {Status.ListToString()}");
            }
            catch (Exception x)
            {
                Log.Error("Writing File or Publishing data Exception", x);
            }
        }

        static void Publish()
        {
            /** Publico los cambios */
            try
            {
                RdRegRef?.SetValue<MSStatus>(Identifiers.RdTopic, "MSStatus", Status);
                RdRegRef?.Publish();
                Log.Trace($"Data Published. {Status.ListToString()}");
            }
            catch (Exception x)
            {
                Log.Error("Writing Publishing data Exception", x);
            }
        }

        static bool DataAccess(Action invoke)
        {
            if (semaphore.WaitOne(TimeSpan.FromSeconds(5)))
            {
                invoke();
                semaphore.Release();
                return true;
            }
            return false;
        }
        static bool NotInCfg(List<CfgRecursoEnlaceExterno> cfgRecursos, MSNodeInfo node)
        {
            var inCfg = cfgRecursos.Where(r => r.IdEmplazamiento == node.site && r.IdRecurso == node.res).ToList();
            return inCfg.Count() == 0;
        }

        static bool Rd_destination_NotInCfg(List<CfgEnlaceExterno> cfgRdLinks, string id_dest)
        {
            var inCfg = cfgRdLinks.Where(r => r.Literal == id_dest).ToList();
            return inCfg.Count() == 0;
        }

        /*
         * Compara dos fechas/hora
         * Retorno menor que cero d1 es anterior a d2
         *          igual a cero son iguales
         *          mayor que cero d1 es posterior a d2         * 
         * */
        static int CompareDateTime(string d1, string d2)
        {
            if (d1 == null) return -1;
            if (d2 == null) return 1;

            string[] sd1 = d1.Split(',');
            string[] sd2 = d2.Split(',');

            Int32 Year;
            Int32 month;
            Int32 day;
            Int32 hour;
            Int32 min;
            Int32 sec;
            Int32 mil;

            if (!Int32.TryParse(sd1[0], out Year)) Year = 0;
            if (!Int32.TryParse(sd1[1], out month)) month = 0;
            if (!Int32.TryParse(sd1[2], out day)) day = 0;
            if (!Int32.TryParse(sd1[3], out hour)) hour = 0;
            if (!Int32.TryParse(sd1[4], out min)) min = 0;
            if (!Int32.TryParse(sd1[5], out sec)) sec = 0;
            if (!Int32.TryParse(sd1[6], out mil)) mil = 0;

            DateTime D1 = new DateTime(Year, month, day, hour, min, sec, mil);

            if (!Int32.TryParse(sd2[0], out Year)) Year = 0;
            if (!Int32.TryParse(sd2[1], out month)) month = 0;
            if (!Int32.TryParse(sd2[2], out day)) day = 0;
            if (!Int32.TryParse(sd2[3], out hour)) hour = 0;
            if (!Int32.TryParse(sd2[4], out min)) min = 0;
            if (!Int32.TryParse(sd2[5], out sec)) sec = 0;
            if (!Int32.TryParse(sd2[6], out mil)) mil = 0;

            DateTime D2 = new DateTime(Year, month, day, hour, min, sec, mil);

            return DateTime.Compare(D1, D2);
        }

        static void SetStatusDateTime()
        {
            DateTime dateTime = DateTime.Now;
            Status.datetime = string.Format("{0},{1},{2},{3},{4},{5},{6}",
                dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond);
        }

        static Logger Log { get => LogManager.GetLogger("MSTxPersistence"); }
        static Registry RdRegRef = null;
        static Semaphore semaphore = new Semaphore(1, 1);
        static MSStatus Status = new MSStatus();
        static bool Master = false;

        #region Solo para TEST
        static public void GenerateResourceChange<T>(T eventdata)
        {
            MemoryStream ms = new MemoryStream();
            Serializer.Serialize(ms, eventdata);
            byte[] data = ms.ToArray();

            var change = new RsChangeInfo("", "MSStatus", Identifiers.TypeId(typeof(T)), "", data);
            OnResourceChanged(null, change);
        }
        #endregion
    }
}
