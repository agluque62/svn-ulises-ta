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
        bool RdStatusRecoveryWithoutPersistence = (Properties.Settings.Default.RdStatusRetriveEnableAndStoreDelay == 1);
        Task SaveStatusTask = null;
        Task RestoringRtxStatusTask = null;
        Int32 CountdownToSave = 0;
        object locker = new object();
        RdPositionsStatus RPS = new RdPositionsStatus();
        Dictionary<int, bool> Availability = new Dictionary<int, bool>();

        bool EventInit = false;
        bool EventPos = false;

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
            LogManager.GetLogger("RSFService").Trace("Processing Event {0}", EventTopicNames.RdInfoEngine, msg);

            /** AGL. Notifica los cambios de configuracion. */
            EventInit = true;
            Init(); 
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        [EventSubscription(EventTopicNames.RdPosStateEngine, ThreadOption.UserInterface)]
        public void OnRdPosStateEngine(object sender, RangeMsg<RdState> msg)
        {
            LogManager.GetLogger("RSFService").Trace("(2) Processing Event {0}: {1}", EventTopicNames.RdPosStateEngine, msg);
            EventPos = true;
            
            /** AGL. Notifica cambios de estadp en posiciones radio, Tx, Tx, Ptt, sqh, ... */            
            int pos = msg.From;
            msg.Info.ToList().ForEach(item =>
            {
                EventOnPos(pos++, item);
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
            LogManager.GetLogger("RSFService").Trace("Processing Event {0}, Grp {1}",
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

            RdPositionStatus lastStatus = new RdPositionStatus(RPS.LastRdStatus[pos]);
            bool assignEvent = lastStatus.RxStatus != NewState.Rx || lastStatus.TxStatus != NewState.Tx || lastStatus.AudioVia != NewState.AudioVia;
            if (assignEvent == true)
            {
                RdDst onlinepos = StateManager.Radio[pos];

                LogManager.GetLogger("RSFService").Trace("EventOnPos {0} (Frec={1}): Tx=>{2}, Rx=>{3}, Ad=>{4}, Available=>{5}, Restored=>{5}",
                    pos, onlinepos.Frecuency, onlinepos.Tx, onlinepos.Rx, onlinepos.AudioVia, !onlinepos.Unavailable,
                    onlinepos.Restored);

                /** 20180716. Si viene un ASPA durante el PTT en una posicion seleccionada en TX, generar el tono de Falsa Maniobra */
                if (onlinepos.Unavailable == true && lastStatus.TxStatus && StateManager.Radio.PttOn )
                {
                    EngineCmdManager.GenerateRadioBadOperationTone(2000);
                }
                
                if (OffOnTransition(onlinepos) == true)
                {
                    /** Retraso un poco las restauraciones para evitar que se queden en dos vias cuando se recuperan frecuencias del lado del tx con sqh vivo */
                    Task.Factory.StartNew(() =>
                    {
                        Task.Delay(100).Wait();
                        Restore(pos);
                    });

                    LogManager.GetLogger("RSFService").Trace("Restoring {0}", pos);
                }
                else if (onlinepos.Unavailable == false)
                {
                    LogManager.GetLogger("RSFService").Trace("Saving {0}", pos);
                    Save(pos);
                }
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
                if (RPS.LastRdStatus.Count == 0)
                {
                    RPS.LastRdStatus = StateManager.Radio.Destinations.Select(pos => new RdPositionStatus()
                    {
                        Position = pos.Id,
                        TxStatus = false,
                        RxStatus = false,
                        AudioVia = RdRxAudioVia.NoAudio
                    }).ToList();
                }
                RPS.PageSize = StateManager.Radio.PageSize;

                if (!RdStatusRecoveryWithoutPersistence)
                {
                    /** Leer el Fichero de Frecuencias */
                    if (System.IO.File.Exists("RadioFrecStatus.json"))
                    {
                        var laststatus = Newtonsoft.Json.JsonConvert.DeserializeObject<List<RdFrecStatus>>(
                            System.IO.File.ReadAllText("RadioFrecStatus.json"));

                        /** Rellenar las posiciones que coincidan */
                        var affected = (from fr in laststatus
                                        join ds in StateManager.Radio.Destinations on fr.frec equals ds.Frecuency
                                        join ps in RPS.LastRdStatus on ds.Id equals ps.Position
                                        where OnPage(ps.Position, 0, RPS.PageSize) == true
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
                        var currpos = (from dst in StateManager.Radio.Destinations
                                       join fr in lastfrec on dst.Frecuency equals fr
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

                        var output1 = (from ps in RPS.LastRdStatus
                                       join ds in StateManager.Radio.Destinations
                                       on ps.Position equals ds.Id
                                       where ds.IsConfigurated == true && OnPage(ps.Position, StateManager.Radio.Page, RPS.PageSize)
                                       select new RdFrecStatus
                                       {
                                           frec = ds.Frecuency,
                                           Tx = ps.TxStatus,
                                           Rx = ps.RxStatus,
                                           Via = ps.AudioVia
                                       }).ToList();

                        System.IO.File.WriteAllText("RadioFrecStatus.json",
                             Newtonsoft.Json.JsonConvert.SerializeObject(output1, Newtonsoft.Json.Formatting.Indented));

                        /** Log de la Operacion */
                        LogManager.GetLogger("RSFService").Trace("Radio Positions States saved...");
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
                    int count =  600;                                       /** Espero 60 seg.. a que desaparezca el PTT. */
                    bool falsaManiobraSignal = false;
                    while (isRecoveringTx && StateManager.Radio.PttOn==true && --count > 0)
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
            bool lastAvailable = Availability.ContainsKey(onlinepos.Id) ? Availability[onlinepos.Id] : false;
            bool currAvailable = onlinepos.Unavailable == false;
            Availability[onlinepos.Id] = currAvailable;
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
                var rtxFrec = (from ds in StateManager.Radio.Destinations
                               where ds.Tx == true && ds.RtxGroup == rtxGroup
                               select ds.Frecuency).ToList();

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
            if (RdStatusRecoveryEnabled == false || RdRtxStatusRetrieveEnabled == false || StateManager == null)
                return;

            if (RestoringRtxStatusTask == null)
            {
                RestoringRtxStatusTask = Task.Factory.StartNew(() =>
                {
                    Task.Delay(1500).Wait();
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
                LogManager.GetLogger("RSFService").Error("RestoreOnInitialEventsFails. INIT");
                Init();
            }

            if (EventInit == false || EventPos == false)
            {
                LogManager.GetLogger("RSFService").Error("RestoreOnInitialEventsFails. Generating Events.");
                for (int pos = 0; pos < 32; pos++)
                {
                    EventOnPos(pos,
                        new RdState(false, false, PttState.NoPtt, SquelchState.NoSquelch, RdRxAudioVia.NoAudio, 0, FrequencyState.Available, "", 0, "")
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

        #endregion METODOS PRIVADOS

    }
}
