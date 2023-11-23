using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.Diagnostics;

using Microsoft.Practices.CompositeUI;
using Microsoft.Practices.CompositeUI.EventBroker;

using HMI.Model.Module.Constants;
using HMI.Model.Module.Messages;
using HMI.Model.Module.BusinessEntities;
using HMI.Model.Module.Properties;
using Utilities;

using NLog;


namespace HMI.Model.Module.Services
{
    class RadioSpecialFunctionsService
    {
        #region DEBUG

        #endregion DEBUG

        #region TIPOS

        public class SyncObject
        {
            public enum WaitingMode { NoWaiting, WaitingRx, WaitingTx, WaitingAudiovia }
            public void prepare(WaitingMode mode)
            {
                mre.Reset();
                prepared = mode;
            }
            public bool wait(int timeout)
            {
                bool res = mre.WaitOne(timeout);
                return res;
            }
            public void signal(WaitingMode mode)
            {
                if (prepared == mode)
                {
                    prepared = WaitingMode.NoWaiting;
                    mre.Set();
                }
            }

            System.Threading.ManualResetEvent mre = new System.Threading.ManualResetEvent(false);
            WaitingMode prepared = WaitingMode.NoWaiting;
        }
        class RdPositionStatus
        {
            public int Position { get; set; }
            public bool TxStatus { get; set; }
            public bool RxStatus { get; set; }
            public RdRxAudioVia AudioVia { get; set; }
            public RdPositionStatus() { }
            public RdPositionStatus(RdPositionStatus o)
            {
                Position = o.Position;
                TxStatus = o.TxStatus;
                RxStatus = o.RxStatus;
                AudioVia = o.AudioVia;
                sync = o.sync;
            }
            public void RestoreRx(IEngineCmdManagerService EngineCmdManager)
            {
                LogManager.GetLogger("RSFService").Trace("Restoring Rx on {0}", Position);

                sync.prepare(SyncObject.WaitingMode.WaitingRx);
                EngineCmdManager.SetRdRx(Position, true, true);
                sync.wait(500);

                LogManager.GetLogger("RSFService").Trace("Rx on {0} Restored", Position);
            }
            public void RestoreTx(IEngineCmdManagerService EngineCmdManager)
            {
                LogManager.GetLogger("RSFService").Trace("Restoring Tx on {0}", Position);

                sync.prepare(SyncObject.WaitingMode.WaitingTx);
                EngineCmdManager.ConfirmRdTx(Position);
                sync.wait(500);

                LogManager.GetLogger("RSFService").Trace("Tx on {0} Restored", Position);
            }
            public void RestoreAudioVia(IEngineCmdManagerService EngineCmdManager)
            {
                LogManager.GetLogger("RSFService").Trace("Restoring AudioVia on {0}", Position);

                sync.prepare(SyncObject.WaitingMode.WaitingAudiovia);
                EngineCmdManager.SetRdAudio(Position, AudioVia, true);
                sync.wait(500);

                LogManager.GetLogger("RSFService").Trace("AudioVia on {0} Restored", Position);
            }
            public void SaveRx(bool newRx)
            {
                RxStatus = newRx;
                if (newRx == true)
                {
                    sync.signal(SyncObject.WaitingMode.WaitingRx);
                    LogManager.GetLogger("RSFService").Trace("Signal Rx Change on {0}", Position);
                }
            }
            public void SaveTx(bool newTx)
            {
                TxStatus = newTx;
                if (newTx == true)
                {
                    sync.signal(SyncObject.WaitingMode.WaitingTx);
                    LogManager.GetLogger("RSFService").Trace("Signal Tx Change on {0}", Position);
                }
            }
            public void SaveAudiovia(RdRxAudioVia newVia)
            {
                AudioVia = newVia;
                if (newVia != RdRxAudioVia.NoAudio)
                {
                    sync.signal(SyncObject.WaitingMode.WaitingAudiovia);
                    LogManager.GetLogger("RSFService").Trace("Signal Audiovia Change on {0}", Position);
                }
            }

            private SyncObject sync = new SyncObject();
        }
        class RdPositionsStatus
        {
            public Int32 Page { get; set; }
            public Int32 PageSize { get; set; }
            public List<RdPositionStatus> LastRdStatus = new List<RdPositionStatus>();
            public Dictionary<Int32, List<Int32>> LastRtxStatus = new Dictionary<int, List<int>>();
        }
        class RdFrecStatus
        {
            public string frec { get; set; }
            public bool Tx { get; set; }
            public bool Rx { get; set; }
            public RdRxAudioVia Via { get; set; }
        }

        #endregion TIPOS

        #region ATRIBUTOS

        IEngineCmdManagerService EngineCmdManager = null;
        StateManagerService StateManager = null;
        bool RdStatusRecoveryEnabled = (Properties.Settings.Default.RdStatusRetriveEnableAndStoreDelay > 0);
        bool RdRtxStatusRetrieveEnabled = Properties.Settings.Default.RdRtxStatusRetrieveEnable;
        int ChangesRdRtxStatusRetrieveEnabled = 0;
        bool RdStatusRecoveryWithoutPersistence = (Properties.Settings.Default.RdStatusRetriveEnableAndStoreDelay == 1);
        Task SaveStatusTask = null;
        Task RestoringRtxStatusTask = null;
        Int32 CountdownToSave = 0;
        object locker = new object();
        RdPositionsStatus RPS = new RdPositionsStatus();
        Dictionary<string, bool> Availability = new Dictionary<string, bool>();

        bool EventInit = false;
        bool EventPos = false;

        int CurrentConfigHasCode = default(int);
        bool InitWindow = false;


#if _RDPAGESTIMING_
        private Task RdPageChangeDeallocationTask = null;
        private PageSetMsg PageSetPending = null;
        private Int32 RdPageChangeDeallocationTimer { get; set; }
#endif

        #endregion ATRIBUTOS

        #region CONSTRUCTOR y EVENTOS SUBSCRITOS
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stateManager"></param>
        /// <param name="engineCmdManager"></param>
        public RadioSpecialFunctionsService([ServiceDependency] StateManagerService stateManager,
            [ServiceDependency] IEngineCmdManagerService engineCmdManager)
        {
            StateManager = stateManager;
            EngineCmdManager = engineCmdManager;

            Task.Factory.StartNew(() =>
            {
                Task.Delay(5000).Wait();
                RestoreOnInitialEventsFails();
            });

            txInProgressControl = new TxInProgressControl((code) =>
            {
                /** 20190313. Desacoplo el evento para evitar los lazos de muerte */
                Task.Factory.StartNew(() =>
                {
                    Log.Trace("TxInProgressControl EventError {0}", code);

                    if (code == 1 || code == 2)
                        EngineCmdManager.GenerateRadioBadOperationTone(-1);

                    General.SafeLaunchEvent(TxInProgressError, this, new TxInProgressErrorCode(code) { });
                });
            });
            LogManager.GetLogger("RSFService").Trace("Starting RadioStatusRecoveryService");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        [EventSubscription(EventTopicNames.RdInfoEngine, ThreadOption.UserInterface)]
        public void OnRdInfoEngine(object sender, RangeMsg<RdInfo> msg)
        {
            var configData = msg.Info.ToList().Select(i => new { i.Dst, i.Alias });
            var newConfig = Newtonsoft.Json.JsonConvert.SerializeObject(configData);
            var newConfigHashCode = newConfig.GetHashCode();
            Log.Trace("Processing Event {0} Config Hash Code {1}", EventTopicNames.RdInfoEngine, newConfigHashCode);
            //if (newConfigHashCode != CurrentConfigHasCode)
            //{
            //    CurrentConfigHasCode = newConfigHashCode;
            //    /** AGL. Notifica los cambios de configuracion. */
            //    EventInit = true;
            //    Init();
            //    Log.Trace($"Cambio de Configuracion recibida...");
            //}
            //else
            //{
            //    Log.Trace($"Configuracion Identica recibida. Arrancando Ventana de Inicializacion");
            //    InitWindow = true;
            //    Task.Factory.StartNew(() =>
            //    {
            //        Task.Delay(TimeSpan.FromMilliseconds(150)).Wait();
            //        InitWindow = false;
            //        Log.Trace($"Ventana de Inicializacion cerrada por tiempo.");
            //    });
            //}
            EventInit = true;
            Init();
            InitWindow = true;
            Task.Factory.StartNew(() =>
            {
                Task.Delay(TimeSpan.FromMilliseconds(150)).Wait();
                InitWindow = false;
                Log.Trace($"Ventana de Inicializacion cerrada por tiempo.");
            });
#if DEBUG
            var path = $"logs\\{DateTime.Now.ToString("yyyyMMdd_HHmmss")}_ReceivedConfig.json";
            System.IO.File.WriteAllText(path,
                              Newtonsoft.Json.JsonConvert.SerializeObject(configData, Newtonsoft.Json.Formatting.Indented));
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        [EventSubscription(EventTopicNames.RdPosStateEngine, ThreadOption.UserInterface)]
        public void OnRdPosStateEngine(object sender, RangeMsg<RdState> msg)
        {
            Log.Trace("(2) Processing Event {0}: {1}", EventTopicNames.RdPosStateEngine, msg);
            EventPos = true;

            //if (InitWindow == true)
            //{
            //    InitWindow = false;
            //    Init();
            //    Log.Trace($"Ventana de Inicializacion cerrada por Evento.");
            //}
            /** AGL. Notifica cambios de estadp en posiciones radio, Tx, Tx, Ptt, sqh, ... */
            int pos = msg.From;
            msg.Info.ToList().ForEach(item =>
            {
                EventOnPos(pos++, item);
                /** 20190205 */
                if (txInProgressControl != null)
                {
                    /** */
                    if (!(item.Ptt == PttState.ExternPtt && item.PttSrcId.StartsWith("Rtx") == false))
                    {
                        txInProgressControl.RtxGroupEvent(StateManager, item);
                    }
                }
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [EventSubscription(EventTopicNames.RtxChanged, ThreadOption.Publisher)]
        public void OnRtxChanged(object sender, EventArgs e)
        {
            Log.Trace("Processing Event {0}, Grp {1}",
                EventTopicNames.RtxChanged, StateManager.Radio.Rtx);

            if (StateManager.Radio.Rtx == 0)
                RtxSave();
        }

#if _RDPAGESTIMING_
        /// <summary>
        /// 
        /// </summary>
        [EventPublication(EventTopicNames.RdPageEngine, PublicationScope.Global)]
        public event EventHandler<PageMsg> RdPageEngine;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [EventSubscription(EventTopicNames.RdPageChanging, ThreadOption.Publisher)]
        public void OnRdPageChanging(object sender, PageSetMsg e)
        {
            LogManager.GetLogger("RSFService").Trace("Procesando {0}: ", EventTopicNames.RdPageChanging);

            bool DelayPage = Properties.Settings.Default.RdPageChangeDeallocationDelay > 0;
            if (!DelayPage)
            {
                EngineCmdManager.SetRdPage(e.OldPage, e.NewPage, e.PageSize);
            }
            else
            {
                DelayRdPageChange(e);
            }
            /** AGL. Info para notificar cuando se efectua la asignacion */
            General.SafeLaunchEvent(RdPageEngine, this,
                new PageMsg(e.NewPage, Properties.Settings.Default.RdPageChangeDeallocationDelay));
        }
#endif
#endregion CONSTRUCTOR y EVENTOS SUBSCRITOS

#region METODOS PRIVADOS

#region ESTADOS TX-RX

        /// <summary>
        /// Gestiona un evento de Asignacion / Desasignacion.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="NewState"></param>
        void EventOnPos(int pos, RdState NewState)
        {
            if (RdStatusRecoveryEnabled == false)
                return;
            if (RPS.LastRdStatus.Count <= pos)
                return;
            RdPositionStatus lastStatus = new RdPositionStatus(RPS.LastRdStatus[pos]);
            bool assignEvent = lastStatus.RxStatus != NewState.Rx || lastStatus.TxStatus != NewState.Tx || lastStatus.AudioVia != NewState.AudioVia;
            if (assignEvent == true)
            {
                RdDst onlinepos = StateManager.Radio[pos];

                if (InitWindow == true)
                {
                    Availability[onlinepos.Frecuency] = false;
                }

                Log.Trace("EventOnPos {0} (Frec={1}): Tx=>{2}, Rx=>{3}, Ad=>{4}, Available=>{5}, Restored=>{6}",
                    pos, onlinepos.Frecuency, onlinepos.Tx, onlinepos.Rx, onlinepos.AudioVia, !onlinepos.Unavailable,
                    onlinepos.Restored);

                /** 20180716. Si viene un ASPA durante el PTT en una posicion seleccionada en TX, generar el tono de Falsa Maniobra */
                if (onlinepos.Unavailable == true && lastStatus.TxStatus && StateManager.Radio.PttOn)
                {
                    EngineCmdManager.GenerateRadioBadOperationTone(2000);
                }

                if (OffOnTransition(onlinepos) == true)
                {
                    Log.Trace("EventOnPos => Restoring {0}", pos);
                    Restore(pos);
                }
                else if (onlinepos.Unavailable == false)
                {
                    Log.Trace("EventOnPos => Saving {0}", pos);
                    Save(pos);
                }
                else
                {
                    Log.Trace("EventOnPos => Ignoring {0}", pos);
                }
            }
            else
            {
                Log.Trace("EventOnPos => Not AssignEvent on {0}", pos);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        void Init()
        {
            if (RdStatusRecoveryEnabled /*&& !RdStatusRecoveryWithoutPersistence */&& StateManager != null)
            {
                /** Reconstruyo la lista */
                //if (RPS.LastRdStatus.Count == 0)
                //{
                    RPS.LastRdStatus = StateManager.Radio.Destinations.Select(pos => new RdPositionStatus()
                    {
                        Position = pos.Id,
                        TxStatus = false,
                        RxStatus = false,
                        AudioVia = RdRxAudioVia.NoAudio
                    }).ToList();
                //}
                RPS.PageSize = StateManager.Radio.PageSize;

                //Availability = new Dictionary<string, bool>();

                if (!RdStatusRecoveryWithoutPersistence)
                {
                    /** Leer el Fichero de Frecuencias */
                    if (System.IO.File.Exists("RadioFrecStatus.json"))
                    {
                        var laststatus = Newtonsoft.Json.JsonConvert.DeserializeObject<List<RdFrecStatus>>(
                            System.IO.File.ReadAllText("RadioFrecStatus.json"));

                        // RQF34 ------Cambiado Frecuency por Literal por los cambios de identificadores.
                        /** Rellenar las posiciones que coincidan en la página activa. */
                        var affected = (from fr in laststatus
                                        join ds in StateManager.Radio.Destinations on fr.frec equals ds.Literal                                        
                                        join ps in RPS.LastRdStatus on ds.Id equals ps.Position
                                        where OnPage(ps.Position, StateManager.Radio.Page, RPS.PageSize) == true
                                        select new { pos = ps.Position, Tx = fr.Tx, Rx = fr.Rx, Via = fr.Via }).ToList();

                        affected.ForEach(item =>
                        {
                            RPS.LastRdStatus[item.pos].TxStatus = item.Tx;
                            RPS.LastRdStatus[item.pos].RxStatus = item.Rx;
                            RPS.LastRdStatus[item.pos].AudioVia = item.Via;
                        });
                    }

                    /** Leer el Fichero de Retransmisiones */
                    if (System.IO.File.Exists("RadioRtxStatus.json"))
                    {
                        var lastfrec = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(
                            System.IO.File.ReadAllText("RadioRtxStatus.json"));
                        // RQF34 Cambiado Frecuency por Literal por los cambios de identificadores.
                        var currpos = (from dst in StateManager.Radio.Destinations
                                       join fr in lastfrec on dst.Literal equals fr
                                       where OnPage(dst.Id, 0, StateManager.Radio.PageSize) == true
                                       select (dst.Id)).ToList();
                        RPS.LastRtxStatus[1] = currpos;
                    }

                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        void Save(int pos = -1)
        {
            if (RdStatusRecoveryEnabled == false || StateManager == null)
                return;

            CountdownToSave = Properties.Settings.Default.RdStatusRetriveEnableAndStoreDelay;
            if (pos >= 0 && pos < RPS.LastRdStatus.Count)
            {
                RdPositionStatus p = RPS.LastRdStatus[pos];
                RdDst onlinepos = StateManager.Radio[pos];

                if (onlinepos.Unavailable == false) // OJO....
                {
                    p.SaveTx(onlinepos.Tx);
                    p.SaveRx(onlinepos.Rx);
                    p.SaveAudiovia(onlinepos.AudioVia);
                }
            }
            if (!RdStatusRecoveryWithoutPersistence && SaveStatusTask == null)
            {
                SaveStatusTask = Task.Factory.StartNew(() =>
                {
                    while (CountdownToSave-- > 0)
                    {
                        Task.Delay(1000).Wait();
                    }
                    try
                    {
                        RPS.Page = StateManager.Radio.Page;
                        // RQF34 Cambiado Frecuency por Literal por los cambios de identificadores.
                        var output1 = (from ps in RPS.LastRdStatus
                                       join ds in StateManager.Radio.Destinations
                                       on ps.Position equals ds.Id
                                       where ds.IsConfigurated == true && OnPage(ps.Position, StateManager.Radio.Page, RPS.PageSize)
                                       select new RdFrecStatus
                                       {
                                           //frec = ds.Frec,
                                           frec = ds.Literal,
                                           Tx = ps.TxStatus,
                                           Rx = ps.RxStatus,
                                           Via = ps.AudioVia
                                       }).ToList();

                        System.IO.File.WriteAllText("RadioFrecStatus.json",
                             Newtonsoft.Json.JsonConvert.SerializeObject(output1, Newtonsoft.Json.Formatting.Indented));

                        /** Log de la Operacion */
                        Log.Trace("Radio Positions States saved...");
                    }
                    finally
                    {
                        SaveStatusTask = null;
                    }
                });
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        void Restore(int pos)
        {
            if (RdStatusRecoveryEnabled == false || StateManager == null)
                return;

            if (pos < RPS.LastRdStatus.Count)
            {
                var s = RPS.LastRdStatus.Where(r => r.Position == pos)
                    .Select(n => new RdPositionStatus(n)).ToList();

                Task.Factory.StartNew(() =>
                {
                    /** 20180716. Incidencia #3648. Espero a PTT-OFF */
                    var isRecoveringTx = s.Where(p => p.TxStatus).FirstOrDefault() == null ? false : s.Where(p => p.TxStatus).FirstOrDefault().TxStatus;
                    int count = 600;                                       /** Espero 60 seg.. a que desaparezca el PTT. */
                    bool falsaManiobraSignal = false;
                    while (isRecoveringTx && StateManager.Radio.PttOn == true && --count > 0)
                    {
                        Task.Delay(100).Wait();
                        /** Señalizacion Acustica de Falsa Maniobra durante 2 segundos */
                        if (!falsaManiobraSignal)
                        {
                            EngineCmdManager.GenerateRadioBadOperationTone(2000);
                            falsaManiobraSignal = true;
                        }
                    }
                    s.ForEach(item => { SetRdStatus(item); });
                });
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        void SetRdStatus(RdPositionStatus s)
        {
            lock (locker)
            {
                if (s.Position < Radio.NumDestinations && s.AudioVia != RdRxAudioVia.NoAudio)
                {
                    RdDst onlinepos = StateManager.Radio[s.Position];
                    if (s.RxStatus == true)
                    {
                        s.RestoreRx(EngineCmdManager);
                        if (s.AudioVia != RdRxAudioVia.Speaker)
                        {
                            s.RestoreAudioVia(EngineCmdManager);
                        }
                    }
                    if (s.TxStatus == true)
                    {
                        s.RestoreTx(EngineCmdManager);
                    }
                    /** Log de Operacion */
                    LogManager.GetLogger("RSFService").Trace("RadioStatusRecovery. Recuperado Estado de Posicion Radio {0}({1}) => Tx={2}, Rx={3}, Audio={4}.",
                        onlinepos.Id, onlinepos.Frecuency, s.TxStatus, s.RxStatus, s.AudioVia);

                    RtxRestoreFor(s.Position);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="onlinepos"></param>
        /// <returns></returns>
        bool OffOnTransition(RdDst onlinepos)
        {
            bool lastAvailable = Availability.ContainsKey(onlinepos.Frecuency) ? Availability[onlinepos.Frecuency] : false;
            bool currAvailable = onlinepos.Unavailable == false;
            Availability[onlinepos.Frecuency] = currAvailable;
            return (lastAvailable == false && currAvailable == true);
        }

#endregion

#region ESTADOS RTX
        /// <summary>
        /// 
        /// </summary>
        void RtxSave()
        {
            if (RdStatusRecoveryEnabled == false || StateManager == null)
                return;

            Task.Factory.StartNew(() =>
            {
                Task.Delay(1000).Wait();
                
                int rtxGroup = 1;

                /** Actualizo la tabla ONLINE */
                List<Int32> rtxPosList = new List<int>();
                var rtxPos = StateManager.Radio.Destinations.Where(
                    pos => pos.Tx == true && pos.RtxGroup == rtxGroup).ToList();
                rtxPos.ForEach(pos => rtxPosList.Add(pos.Id));
                RPS.LastRtxStatus[rtxGroup] = rtxPosList;

                /** Actualizo el Fichero */
                // RQF34 Cambiado Frecuency por Literal por los cambios de identificadores.
                var rtxFrec = (from ds in StateManager.Radio.Destinations
                                where ds.Tx == true && ds.RtxGroup == rtxGroup
                                select ds.Literal).ToList();
                System.IO.File.WriteAllText("RadioRtxStatus.json",
                Newtonsoft.Json.JsonConvert.SerializeObject(rtxFrec,
                    Newtonsoft.Json.Formatting.Indented));
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        void RtxRestoreFor(int pos)
        {
            //RQF36
            if (RdRtxStatusRetrieveEnabled != ((StateManager.Permissions & Permissions.PermisoRTXSect) == Permissions.PermisoRTXSect))
            {
                if (ChangesRdRtxStatusRetrieveEnabled++>0)
                    RdRtxStatusRetrieveEnabled = ((StateManager.Permissions & Permissions.PermisoRTXSect) == Permissions.PermisoRTXSect);
            }
            if (RdStatusRecoveryEnabled == false || RdRtxStatusRetrieveEnabled == false || StateManager == null)
                return;

            if (RestoringRtxStatusTask == null)
            {
                RestoringRtxStatusTask = Task.Factory.StartNew(() =>
                {
                    Task.Delay(1500).Wait();

                    //LALM 220321 Espero a que se pongan en TX 10*1.5 segundos
                    int cont = 0;
                    while (cont < 10)
                    {
                        RPS.LastRtxStatus.ToList().ForEach(item =>
                        {
                            var rtxGrp = (from s in item.Value
                                          join p in StateManager.Radio.Destinations on s equals p.Id
                                          where p.Tx == true
                                          select new { pos = p.Id, std = RtxState.Add })
                                            .ToDictionary(e => e.pos, e => e.std);
                            var rtxGrp1 = (from s in item.Value
                                          join p in StateManager.Radio.Destinations on s equals p.Id
                                          select new { pos = p.Id, std = RtxState.Add })
                                            .ToDictionary(e => e.pos, e => e.std);
                            if (rtxGrp.Count ==rtxGrp1.Count)
                            {
                                cont = 10;
                            }
                            cont++;
                           Task.Delay(1500).Wait();
                        });
                    }

                    try
                    {
                        RPS.LastRtxStatus.ToList().ForEach(item =>
                        {
                            var rtxGrp = (from s in item.Value
                                          join p in StateManager.Radio.Destinations on s equals p.Id
                                          where p.Tx == true
                                          select new { pos = p.Id, std = RtxState.Add })
                                            .ToDictionary(e => e.pos, e => e.std);

                            if (rtxGrp.Count > 1)
                            {
                                 EngineCmdManager.SetRtxGroup(item.Key, rtxGrp, true);   // Fuerza la formacion del grupo...
                            }
                        });
                    }
                    finally
                    {
                        RestoringRtxStatusTask = null;
                    }
                });
            }
        }
#endregion
        /// <summary>
        /// 
        /// </summary>
        void RestoreOnInitialEventsFails()
        {
            if (EventInit == false)
            {
                Log.Error("RestoreOnInitialEventsFails. INIT");
                Init();
            }

            if (EventInit == false || EventPos == false)
            {
                Log.Error("RestoreOnInitialEventsFails. Generating Events.");
                for (int pos = 0; pos < 32; pos++)
                {
                    EventOnPos(pos,
                        new RdState(false, false, string.Empty, PttState.NoPtt, SquelchState.NoSquelch, RdRxAudioVia.NoAudio, 0, FrequencyState.Available, "", 0, "","")//inserto selected frecuency
                        );
                }
            }
            EventInit = EventPos = true;
        }

#if _RDPAGESTIMING_
#region DELAYED RDPAGES CHANGES
        void DelayRdPageChange(PageSetMsg e)
        {
            RdPageChangeDeallocationTimer = Properties.Settings.Default.RdPageChangeDeallocationDelay;

            /** Siempre hay que mantener la OldPage inicial porque es donde se efectuaran las
             desasignaciones, si al final haya cambio de pagina */
            PageSetPending = PageSetPending == null ? new PageSetMsg(e.OldPage, e.NewPage, e.PageSize) :
                new PageSetMsg(PageSetPending.OldPage, e.NewPage, e.PageSize);

            if (RdPageChangeDeallocationTask == null)
            {
                RdPageChangeDeallocationTask = Task.Factory.StartNew(() =>
                {
                    while (--RdPageChangeDeallocationTimer >= 0)
                    {
                        Task.Delay(1000).Wait();
                    }
                    try
                    {
                        if (PageSetPending.NewPage != PageSetPending.OldPage)
                            EngineCmdManager.SetRdPage(PageSetPending.OldPage, PageSetPending.NewPage, PageSetPending.PageSize);
                    }
                    finally
                    {
                        RdPageChangeDeallocationTask = null;
                        PageSetPending = null;
                    }
                });
            }
        }
#endregion
#endif
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Position"></param>
        /// <param name="page"></param>
        /// <param name="sizepage"></param>
        /// <returns></returns>
        bool OnPage(int Position, int page, int sizepage)
        {
            int inferior = page * sizepage;
            int superior = (page + 1) * sizepage;
            return Position >= inferior && Position < superior;
        }

        Logger Log { get => LogManager.GetLogger("RSFService"); }

#endregion METODOS PRIVADOS

#region SUPERVISION ERRORES EN TX RADIO

        /** 20180205. AGl. Control de los Grupos RTX */
        [EventSubscription(EventTopicNames.PttOnChanged, ThreadOption.Publisher)]
        public void OnPttOnChanged(object sender, EventArgs e)
        {
            if (txInProgressControl != null)
            {
                txInProgressControl.NewLocalPttEvent(StateManager);
            }
        }

        [EventPublication(EventTopicNames.TxInProgressError, PublicationScope.Global)]
        public event EventHandler<TxInProgressErrorCode> TxInProgressError;
        [EventPublication(EventTopicNames.CouplingInProgressError, PublicationScope.Global)]
        public event EventHandler<CouplingInProgressErrorCode> CouplingInProgressErrorCode;

        internal class TxInProgressControl
        {
            enum eRtxGroupStates { Inactive, TxInProgress, RtxInProgress }
            private eRtxGroupStates grpStatus = eRtxGroupStates.Inactive;
            private eRtxGroupStates GrpStatus { get { return grpStatus; } set { grpStatus = value; } }
            private Action<int> EventError = null;
            private Object TxInProgressControlLocker = new object();
            private int TxConfirmationTime { get; set; }
            private int SquelchConfirmationTime { get; set; }
            public TxInProgressControl(Action<int> eventError)
            {
                TxConfirmationTime = Properties.Settings.Default.RdTxConfirmationTime;
                SquelchConfirmationTime = Properties.Settings.Default.RdSquelchConfirmationTime;
                EventError = eventError;
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="stateManager"></param>
            /// <param name="dst"></param>
            public void RtxGroupEvent(StateManagerService stateManager, RdState dst)
            {
                lock (TxInProgressControlLocker)
                {
                    var TxInProgressSupervisionEnable = TxConfirmationTime > 10 || SquelchConfirmationTime > 10;
                    if (TxInProgressSupervisionEnable)
                    {
                        LogManager.GetLogger("RSFService").Trace("RtxGroupEvent...");
                        bool thereIsGroup = stateManager.Radio.Destinations.Where(d => d.RtxGroup > 0).ToList().Count > 0;
                        if (thereIsGroup && dst.RtxGroup > 0)   // Si hay grupo y el evento es en el grupo.
                        {
                            bool eventRtxOn = dst.Ptt == PttState.NoPtt && dst.Squelch == SquelchState.SquelchOnlyPort;
                            bool eventTxOn = dst.Ptt == PttState.PttOnlyPort;
                            bool eventOff = dst.Ptt == PttState.NoPtt && dst.Squelch == SquelchState.NoSquelch;

                            LogManager.GetLogger("RSFService").Trace("RtxGroupEvent Processing {0}, {1}, {2}, {3}",
                                GrpStatus, eventRtxOn, eventTxOn, eventOff);

                            switch (GrpStatus)
                            {
                                case eRtxGroupStates.Inactive:
                                    if (eventRtxOn)
                                    {
                                        GrpStatus = eRtxGroupStates.RtxInProgress;
                                        StartRtxSupervisor(stateManager);
                                    }
                                    else if (eventTxOn)
                                    {
                                        GrpStatus = eRtxGroupStates.TxInProgress;
                                    }
                                    break;
                                case eRtxGroupStates.TxInProgress:
                                    if (eventRtxOn)
                                    {
                                        GrpStatus = eRtxGroupStates.RtxInProgress;
                                        StartRtxSupervisor(stateManager);
                                    }
                                    else if (eventOff)
                                    {
                                        var squelchsInGroup = stateManager.Radio.Destinations.Where(d => d.RtxGroup > 0 && d.Squelch != SquelchState.NoSquelch).ToList().Count;

                                        LogManager.GetLogger("RSFService").Trace("RtxGroupEvent Processing eventOff on TxInProgress {0}", squelchsInGroup);

                                        if (squelchsInGroup == 0)
                                        {
                                            GrpStatus = eRtxGroupStates.Inactive;
                                            EventError(4);
                                        }
                                    }
                                    break;

                                case eRtxGroupStates.RtxInProgress:
                                    if (eventTxOn)
                                    {
                                        GrpStatus = eRtxGroupStates.TxInProgress;
                                        CancelRtxSupervisor();
                                    }
                                    else if (eventOff)
                                    {
                                        var squelchsInGroup = stateManager.Radio.Destinations.Where(d => d.RtxGroup > 0 && d.Squelch != SquelchState.NoSquelch).ToList().Count;
                                        LogManager.GetLogger("RSFService").Trace("RtxGroupEvent Processing eventOff on RtxInProgress {0}", squelchsInGroup);
                                        if (squelchsInGroup == 0)
                                        {
                                            GrpStatus = eRtxGroupStates.Inactive;
                                            CancelRtxSupervisor();
                                            EventError(4);
                                        }
                                    }
                                    break;
                            }
                        }
                        else if (!thereIsGroup)
                        {
                            GrpStatus = eRtxGroupStates.Inactive;
                            LogManager.GetLogger("RSFService").Trace("RtxGroupEvent Not Processed");
                        }
                    }
                }
            }
            private Task RtxSpTask = null;
            private System.Threading.ManualResetEvent RtxSpTaskAbort = null;
            private void StartRtxSupervisor(StateManagerService stateManager)
            {
                if (RtxSpTask == null)
                {
                    RtxSpTaskAbort = new System.Threading.ManualResetEvent(false);
                    RtxSpTask = Task.Factory.StartNew(() =>
                    {
                        var time = (TxConfirmationTime > SquelchConfirmationTime ? TxConfirmationTime : SquelchConfirmationTime) + 500;
                        LogManager.GetLogger("RSFService")
                            .Trace("RtxSupervisor Processing Started");
                        if (RtxSpTaskAbort.WaitOne(time) == false)
                        {
                            lock (TxInProgressControlLocker)
                            {
                                if (GrpStatus == eRtxGroupStates.RtxInProgress)
                                {
                                    var items = stateManager.Radio.Destinations.Where(d => d.RtxGroup > 0).ToList().Count;
                                    var itemsInSquelch = stateManager.Radio.Destinations.Where(d => d.RtxGroup > 0 && d.Squelch != SquelchState.NoSquelch).ToList().Count;
                                    var itemsInPtt = stateManager.Radio.Destinations.Where(d => d.RtxGroup > 0 && d.Ptt == PttState.ExternPtt).ToList().Count;

                                    LogManager.GetLogger("RSFService")
                                        .Trace("RtxSupervisor Processing RtxInProgress {0}, {1}, {2}",
                                         items, itemsInSquelch, itemsInPtt);

                                    if (itemsInSquelch != items || itemsInPtt != (items - 1))
                                    {
                                        // Error en Grupo
                                        if (EventError != null)
                                            EventError(3);
                                    }
                                    else
                                    {
                                    }
                                }
                                else
                                {
                                    LogManager.GetLogger("RSFService")
                                        .Trace("RtxSupervisor Processing Not in RtxInProgress");
                                }
                            }
                        }
                        else
                        {
                            LogManager.GetLogger("RSFService")
                                .Trace("RtxSupervisor Processing Canceled");
                        }
                        lock (TxInProgressControlLocker)
                        {
                            RtxSpTaskAbort = null;
                            RtxSpTask = null;
                        }
                    });
                }
            }
            private void CancelRtxSupervisor()
            {
                if (RtxSpTask != null && RtxSpTaskAbort != null)
                    RtxSpTaskAbort.Set();
            }

            private System.Threading.ManualResetEvent TxInProgressSpCancel = null;
            private Task TxInProgressSpTask = null;
            public void LocalPttEvent(StateManagerService stateManager)
            {
                lock (TxInProgressControlLocker)
                {
                    var TxInProgressSupervisionEnable = TxConfirmationTime > 10 || SquelchConfirmationTime > 10;
                    if (TxInProgressSupervisionEnable)
                    {
                        if (stateManager.Radio.PttOn)
                        {
                            // PTTON de Operador.
                            LogManager.GetLogger("RSFService")
                                .Trace("LocalPttEvent Processing PTT-ON");
                            if (TxInProgressSpCancel == null)
                            {
                                // PTT Supervisado
                                TxInProgressSpCancel = new System.Threading.ManualResetEvent(false);
                                TxInProgressSpTask = Task.Factory.StartNew(() =>
                                {
                                    var NewSqhConfirmationTime = SquelchConfirmationTime;
                                    if (TxConfirmationTime > 10)
                                    {
                                        // Espera la confirmacion de TX
                                        LogManager.GetLogger("RSFService")
                                            .Trace("LocalPttEvent Starting Supervision TX {0}", TxConfirmationTime);
                                        if (!TxInProgressSpCancel.WaitOne(TxConfirmationTime))
                                        {
                                            // Chequea que todos los PTT estan confirmados...
                                            lock (TxInProgressControlLocker)
                                            {
                                                var txSelItems = stateManager.Radio.Destinations.Where(d => d.Tx == true).ToList().Count;
                                                var inPttItems = stateManager.Radio.Destinations.Where(d => d.Tx == true && (
                                                    d.Ptt == PttState.PttOnlyPort ||
                                                    d.Ptt == PttState.PttPortAndMod ||
                                                    d.Ptt == PttState.ExternPtt ||
                                                    d.Ptt == PttState.Blocked)).ToList().Count;
                                                bool confirmados = txSelItems == inPttItems;
                                                NewSqhConfirmationTime -= TxConfirmationTime;

                                                LogManager.GetLogger("RSFService")
                                                    .Trace(string.Format("LocalPttEvent Supervision TX processed {0}, {1}, {2}"),
                                                     txSelItems, inPttItems, confirmados);

                                                if (!confirmados)
                                                {
                                                    EventError(1);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            NewSqhConfirmationTime = 0;
                                            LogManager.GetLogger("RSFService")
                                                .Trace("LocalPttEvent Supervision TX Canceled");
                                        }
                                    }
                                    if (NewSqhConfirmationTime > 10)
                                    {
                                        // Espera la confirmacion de portadoras.
                                        LogManager.GetLogger("RSFService")
                                            .Trace("LocalPttEvent Starting Supervision RX {0}", NewSqhConfirmationTime);
                                        if (NewSqhConfirmationTime > 10 && !TxInProgressSpCancel.WaitOne(NewSqhConfirmationTime))
                                        {
                                            // Chequea que todos los SQH estan confirmados...
                                            lock (TxInProgressControlLocker)
                                            {
                                                var inCarrierErrorItems = stateManager.Radio.Destinations.Where(d => d.Ptt == PttState.CarrierError).ToList().Count;
                                                var inPttItems = stateManager.Radio.Destinations.Where(d => d.Tx == true && (
                                                d.Ptt == PttState.PttOnlyPort ||
                                                d.Ptt == PttState.PttPortAndMod ||
                                                d.Ptt == PttState.ExternPtt ||
                                                d.Ptt == PttState.Blocked)).ToList().Count;
                                                var inSqhItems = stateManager.Radio.Destinations.Where(d => d.Tx == true && (d.Squelch == SquelchState.SquelchOnlyPort ||
                                                    d.Squelch == SquelchState.SquelchPortAndMod)).ToList().Count;
                                                bool confirmados = inCarrierErrorItems == 0 && inPttItems == inSqhItems;
                                                if (!confirmados)
                                                {
                                                    EventError(2);
                                                }
                                                LogManager.GetLogger("RSFService")
                                                    .Trace("LocalPttEvent Supervision RX processed {0}, {1}, {2}, {3}",
                                                     inCarrierErrorItems, inPttItems, inSqhItems, confirmados);
                                            }
                                        }
                                        else
                                        {
                                            LogManager.GetLogger("RSFService")
                                                .Trace("LocalPttEvent Supervision RX Canceled");
                                        }
                                    }
                                    lock (TxInProgressControlLocker)
                                    {
                                        TxInProgressSpCancel = null;
                                    }
                                });
                            }
                            else
                            {
                                // PTT ON No supervisado...
                                LogManager.GetLogger("RSFService").Debug("PTT ON No supervisado...");
                            }
                        }
                        else
                        {
                            // PTTOFF de Operador.
                            if (TxInProgressSpCancel != null)
                            {
                                TxInProgressSpCancel.Set();
                            }
                            else
                            {
                                // PTT OFF No supervisado...
                                LogManager.GetLogger("RSFService").Debug("PTT OFF No supervisado...");
                            }
                            EventError(0);
                        }
                    }
                }
            }

            public void NewLocalPttEvent(StateManagerService stateManager)
            {
                lock (TxInProgressControlLocker)
                {
                    if (stateManager.Radio.PttOn)
                    {
                        // PTTON de Operador.
                        LogManager.GetLogger("RSFService")
                            .Trace("NewLocalPttEvent Processing PTT-ON");
                        if (TxInProgressSpCancel == null)
                        {
                            // PTT Supervisado
                            TxInProgressSpCancel = new System.Threading.ManualResetEvent(false);
                            TxInProgressSpTask = Task.Factory.StartNew(() =>
                            {
                                DateTime pttStart = DateTime.Now;
                                TimeSpan txGuard = TimeSpan.FromMilliseconds(TxConfirmationTime);
                                TimeSpan rxGuard = TimeSpan.FromMilliseconds(SquelchConfirmationTime);
                                bool lastTxStatus = true;
                                bool lastRxStatus = true;
                                while (TxInProgressSpCancel.WaitOne(TimeSpan.FromMilliseconds(200)) == false)
                                {
                                    TimeSpan elapsed = DateTime.Now - pttStart;
                                    if (txGuard.TotalMilliseconds > 0 && txGuard < elapsed)
                                    {
                                        /** Supervision de TX. Chequea que todos los PTT estan confirmados... */
                                        lock (TxInProgressControlLocker)
                                        {
                                            if (LCOutActivity(stateManager) == true)
                                            {
                                                lastTxStatus = true;
                                                pttStart = DateTime.Now;
                                            }
                                            else
                                            {
                                                var txSelItems = stateManager.Radio.Destinations.Where(d => d.Tx == true && d.TipoFrecuencia!=TipoFrecuencia_t.HF).ToList().Count;
                                                var inPttItems = stateManager.Radio.Destinations.Where(d => d.Tx == true && d.TipoFrecuencia != TipoFrecuencia_t.HF && (
                                                    d.Ptt == PttState.PttOnlyPort ||
                                                    d.Ptt == PttState.PttPortAndMod ||
                                                    d.Ptt == PttState.ExternPtt ||
                                                    d.Ptt == PttState.Blocked || 
                                                    d.Ptt == PttState.CarrierError 
                                                    )).ToList().Count;
                                                bool confirmados = txSelItems == inPttItems;

                                                if (!confirmados && confirmados != lastTxStatus)
                                                {
                                                    EventError(1);
                                                    LogManager.GetLogger("RSFService")
                                                        .Error("NewLocalPttEvent Supervision TX failed {0}, {1}, {2}",
                                                         txSelItems, inPttItems, confirmados);
                                                }
                                                lastTxStatus = confirmados;
                                            }
                                        }
                                    }
                                    if (rxGuard.TotalMilliseconds > 0 && rxGuard < elapsed)
                                    {
                                        /** Supervision de RX. Chequea que todos los SQH estan confirmados.. */
                                        lock (TxInProgressControlLocker)
                                        {
                                            if (LCOutActivity(stateManager) == true)
                                            {
                                                lastRxStatus = true;
                                                pttStart = DateTime.Now;
                                            }
                                            else {
                                                var inCarrierErrorItems = stateManager.Radio.Destinations.Where(d => d.Ptt == PttState.CarrierError).ToList().Count;
                                                var inPttItems = stateManager.Radio.Destinations.Where(d => d.Tx == true && (
                                                d.Ptt == PttState.PttOnlyPort ||
                                                d.Ptt == PttState.PttPortAndMod ||
                                                d.Ptt == PttState.ExternPtt ||
                                                d.Ptt == PttState.Blocked)).ToList().Count;
                                                var inSqhItems = stateManager.Radio.Destinations.Where(d => d.Tx == true && (d.Squelch == SquelchState.SquelchOnlyPort ||
                                                    d.Squelch == SquelchState.SquelchPortAndMod)).ToList().Count;
                                                bool confirmados = inCarrierErrorItems == 0 && inPttItems == inSqhItems;

                                                if (!confirmados && lastRxStatus != confirmados)
                                                {
                                                    EventError(2);
                                                    LogManager.GetLogger("RSFService")
                                                        .Error("NewLocalPttEvent Supervision RX failed {0}, {1}, {2}, {3}",
                                                         inCarrierErrorItems, inPttItems, inSqhItems, confirmados);
                                                }
                                                lastRxStatus = confirmados;
                                            }
                                        }
                                    }
                                }
                                /** Abortar el Proceso */
                                lock (TxInProgressControlLocker)
                                {
                                    TxInProgressSpCancel = null;
                                }
                            });
                        }
                        else
                        {
                            // PTT ON No supervisado...
                            LogManager.GetLogger("RSFService").Debug("NewLocalPttEvent. PTT ON No supervisado...");
                        }
                    }
                    else
                    {
                        // PTTOFF de Operador.
                        LogManager.GetLogger("RSFService")
                            .Trace("NewLocalPttEvent Processing PTT-OFF");
                        if (TxInProgressSpCancel != null)
                        {
                            TxInProgressSpCancel.Set();
                        }
                        else
                        {
                            // PTT OFF No supervisado...
                            LogManager.GetLogger("RSFService").Debug("NewLocalPttEvent. PTT OFF No supervisado...");
                        }
                        EventError(0);
                    }
                }

            }
            bool LCOutActivity(StateManagerService stateManager)
            {
                var lcDestInTx = stateManager.Lc.Destinations.Where(d => d.Tx == LcTxState.Tx ||
                    d.Tx == LcTxState.Out || d.Tx == LcTxState.Congestion || d.Tx == LcTxState.Busy).Count();
                return lcDestInTx > 0;
            }
        }

        protected TxInProgressControl txInProgressControl = null;
#endregion

    }
}
