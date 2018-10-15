using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using Translate;
using U5ki.Infrastructure;
using Utilities;

namespace U5ki.RdService
{
    /// <summary>
    /// 
    /// </summary>
    public class RdFrecuency : BaseCode, IDisposable
    {
        private readonly int TIME_DELAY_TO_RTX = U5ki.RdService.Properties.Settings.Default.RtxDelay;
        private readonly int TIME_DELAY_TO_DISABLE_FREQUENCY = U5ki.RdService.Properties.Settings.Default.FrequencyDisabled * 1000;

        /************************************************************************/
        /** 201702-FD. AGL. Nuevos Atributos de Configuracion y Estado. *********/
        //public enum FREQUENCY_STATUS { NotAvailable = 0, Available = 1, Degraded = 2 }
        public class NewRdFrequencyParams
        {
            //public string Zona { get; set; }
            public int cld_supervision_time { get; set; }
            public CORESIP_Priority Priority { get; set; }
            public CORESIP_FREQUENCY_TYPE FrequencyType { get; set; }
            public CORESIP_CLD_CALCULATE_METHOD CLDCalculateMethod { get; set; }
            public int BssWindows { get; set; }
            public bool AudioSync { get; set; }
            public bool AudioInBssWindow { get; set; }
            public bool NotUnassignable { get; set; }
            public int MetodosBssOfrecidos { get; set; }

            public NewRdFrequencyParams()
            {
                //Zona = "##ZONA##";
                Priority = CORESIP_Priority.CORESIP_PR_NORMAL;
                FrequencyType = CORESIP_FREQUENCY_TYPE.Simple;
                CLDCalculateMethod = CORESIP_CLD_CALCULATE_METHOD.Relative;
                BssWindows = 200;
                AudioSync = false;
                AudioInBssWindow = true;
                NotUnassignable = false;
                cld_supervision_time = 1;
                MetodosBssOfrecidos = 0;    // NINGUNO.
             }
        }
        private NewRdFrequencyParams new_params = new NewRdFrequencyParams();
        public NewRdFrequencyParams getParam
        {   
            get {return this.new_params;}
        } 
        private RdSrvFrRs.FrequencyStatusType StatusCheck;
        private RdSrvFrRs.FrequencyStatusType _Status;
        public RdSrvFrRs.FrequencyStatusType Status
        {
            get
            {
                int Tx = _RdRs.Values.Where(r => r.isTx).Count();
                int Rx = _RdRs.Values.Where(r => r.isRx).Count();
                int TxConn = _RdRs.Values.Where(r => r.isTx && r.Connected).Count();
                int RxConn = _RdRs.Values.Where(r => r.isRx && r.Connected).Count();

                RdSrvFrRs.FrequencyStatusType oldStatus = _Status;

                switch (new_params.FrequencyType)
                {
                    case CORESIP_FREQUENCY_TYPE.Simple:
                        if (TipoDeFrecuencia == "HF")
                        {
                            if (TxConn != 0 || RxConn != 0)
                            {
                                _Status = RdSrvFrRs.FrequencyStatusType.Available;
                                SendLogNewStatus(oldStatus);
                                return _Status;
                            }
                        }
                        else
                        {
                            if (Tx == 1 && Rx == 1)
                            {
                                if (TxConn == 1 && RxConn == 1)
                                {
                                    _Status = RdSrvFrRs.FrequencyStatusType.Available;
                                    SendLogNewStatus(oldStatus);
                                    return _Status;
                                }
                            }
                            //Caso de frecuencias s�lo RX
                            else if ((Rx == 1) && (Tx == 0) && (RxConn == 1))
                            {
                                _Status = RdSrvFrRs.FrequencyStatusType.Available;
                                SendLogNewStatus(oldStatus);
                                return _Status;
                            }
                        }
                        break;

                    case CORESIP_FREQUENCY_TYPE.FD:
                        /** 20180626. Incidencia #3617. Una FD puede tener un solo TX */
                        if (Tx >= 1 && Tx <= 5)
                        {
                            if (TxConn > 0 && RxConn > 0)
                            {
                                if (TxConn < Tx || RxConn < Rx)
                                {
                                    _Status = RdSrvFrRs.FrequencyStatusType.Degraded;
                                     SendLogNewStatus(oldStatus);
                                    return _Status;
                                    /*
                                    bool sameSite = false;
                                    // TODO. Meter el control de Emplazamientos.
                                    // JCAM. 27/02/2017
                                    foreach (RdResource rs in _RdRs.Values)
                                    {
                                        if (rs.Type == RdRsType.Tx)
                                            // El recurso seleccionado es un Tx, buscamos un Rx conectado en el mismo emplazameinto
                                            sameSite |= _RdRs.Values.Where(r => ((r.Type == RdRsType.Rx) && (r.Connected) && (r.Site == rs.Site))).Count() > 0;
                                        else if (rs.Type == RdRsType.Rx)
                                            // El recurso seleccionado es un Tx, buscamos un Tx conectado en el mismo emplazameinto
                                            sameSite |= _RdRs.Values.Where(r => ((r.Type == RdRsType.Tx) && (r.Connected) && (r.Site == rs.Site))).Count() > 0;
                                    }

                                    RdSrvFrRs.FrequencyStatusType st = sameSite ? RdSrvFrRs.FrequencyStatusType.Degraded : RdSrvFrRs.FrequencyStatusType.NotAvailable;
                                    _Status = st;
                                    if (oldStatus != _Status)
                                        LogInfo<RdService>("FD Status. Frequency ID: " + this.Frecuency + ". Status: " + RdSrvFrRs.FrequencyStatusType.NotAvailable,
                                                U5kiIncidencias.U5kiIncidencia.IGRL_U5KI_NBX_INFO, "RdService", "FD Status. Frequency ID: " + this.Frecuency + ". Status: " + RdSrvFrRs.FrequencyStatusType.NotAvailable);

                                    return st;
                                    */
                                }
                                _Status = RdSrvFrRs.FrequencyStatusType.Available;
                                SendLogNewStatus(oldStatus);
                                return _Status;
                            }
                        }

                        _Status = RdSrvFrRs.FrequencyStatusType.NotAvailable;
                        SendLogNewStatus(oldStatus);
                        return _Status;

                    case CORESIP_FREQUENCY_TYPE.ME:
                    case CORESIP_FREQUENCY_TYPE.Dual:
                        LogInfo<RdService>("FD Status. Frequency ID: " + this.Frecuency + ". Status: " + RdSrvFrRs.FrequencyStatusType.NotAvailable,
                                U5kiIncidencias.U5kiIncidencia.IGRL_U5KI_NBX_ERROR, "RdService", 
                                CTranslate.translateResource("Frequency type not implemented. Frequency ID: "+ this.Frecuency + ". Type: " +
                                    new_params.FrequencyType.ToString()));
                        break;
                    default:
                        LogInfo<RdService>("FD Status. Frequency ID: " + this.Frecuency + ". Status: " + RdSrvFrRs.FrequencyStatusType.NotAvailable,
                                U5kiIncidencias.U5kiIncidencia.IGRL_U5KI_NBX_ERROR, "RdService", 
                                CTranslate.translateResource("Unknow frequency type. Frequency ID: "+ this.Frecuency + ". Type: " +
                                    new_params.FrequencyType.ToString()));
                        break;
                }

                _Status = RdSrvFrRs.FrequencyStatusType.NotAvailable;
                return RdSrvFrRs.FrequencyStatusType.NotAvailable;
            }
        }
        /************************************************************************/

        //Semaforo usado para realizar el tratamiento de ptt en concurrencia
        //Evita que los cambios concurrentes de _CurrentSrcPtt y _SrcPtts, dejen el sistema incoherente,
        //por ejemplo Reset y NextPtt se ejecutan en hilos diferentes. Visto en Incidencia #3014
        private ManagedSemaphore _CurrentPttSemaphore = new ManagedSemaphore(1, 1, "CurrentPttSemaphore");

        //Cuenta de fallos detectados en el chequeo de datos internos de llamadas
        private int sanityCheckCallsFailures = 0;

        private readonly int MAX_SANITY_CHECK_CALLS_FAILURES=3;
        /// <summary>
        /// 
        /// </summary>
        public event GenericEventHandler<RdFrecuency> TimerElapsed;

        /// <summary>
        /// Guarda el valor configurado para cada frecuencia desde el CfgService
        /// Sirve para gestionar el atributo txSeleccionado de sus recursos
        /// </summary>
        private Tipo_ModoTransmision _ModoTransmision = Tipo_ModoTransmision.Ninguno;
        //Para optimizar busquedas se guarda los recursos seleccionados
        private RdResource _TxRsSelected = null;
        private String _LastSQSite = "";
        /// <summary>
        /// 
        /// </summary>
        public string Frecuency
        {
            get { return _Frecuency; }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool SupervisionPortadora { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string PttSrc
        {
            get { return _CurrentSrcPtt != null ? _CurrentSrcPtt.SrcId : string.Empty; }
        }

        private Dictionary<string, string> _Picts;
        public Dictionary<string, string> Picts
        {
            get
            {
                return _Picts;
            }
            set
            {
                _Picts = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fr"></param>
        public RdFrecuency(string fr)
        {
            _Frecuency = fr;

            _RtxSquelchTimer = new Timer(TIME_DELAY_TO_RTX);
            _RtxSquelchTimer.AutoReset = false;
            _RtxSquelchTimer.Elapsed += OnRtxSquelchElapsed;

            if (TIME_DELAY_TO_DISABLE_FREQUENCY > 0)
            {
                _DisableFrequencyTimer = new Timer(TIME_DELAY_TO_DISABLE_FREQUENCY);
                _DisableFrequencyTimer.AutoReset = false;
                _DisableFrequencyTimer.Enabled = false;
                _DisableFrequencyTimer.Elapsed += OnDisableFrequencyElapsed;
            }
        }

        #region IDisposable Members
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            _DisableFrequencyTimer.Elapsed -= OnDisableFrequencyElapsed;

            foreach (RdResource rdRs in _RdRs.Values)
            {
                rdRs.Dispose();
            }

            Reset(false);
        }

        #endregion

        /// <summary>
        /// Genera los recursos radio (canales f�sicos) asociados a la frecuencia
        /// </summary>
        public void Reset(ConfiguracionSistema sysCfg, CfgEnlaceExterno cfg, Dictionary<string, bool> selectedRs)
        {
            Debug.Assert(cfg.Literal == _Frecuency);

            // Actualizaci�n de los nuevos par�metros de la frecuencia a partir de los recibidos en cfg.
            bool hayCambios = ResetNewParams(cfg);

            Dictionary<string, RdResource> rdRsToRemove = new Dictionary<string, RdResource>(_RdRs);
            _RdRs.Clear();

            foreach (CfgRecursoEnlaceExterno rsCfg in cfg.ListaRecursos)
            {
                if (selectedRs.ContainsKey(rsCfg.IdRecurso))
                {
                    RdResource rdRs = null;
                    string[] rdUri = RsUri(rsCfg.IdRecurso, sysCfg);

                    if (rdUri.Length == 0)
                    {
                        LogDebug<RdFrecuency>(String.Format("Lista.Reset: " + "Recurso no asociado a un host [Rs={0}]", rsCfg.IdRecurso));
                        continue;
                    }

                    if (rdUri[0] != null)       // AGL 20160208. Cuando hay recursos sin URI no se configuran...
                    {
                        //20180724 #3603
                        string rsKeyOld = "";
                        bool hayCambiosEnSip = false;
                        foreach (RdResource rdRsCtlSipPort in rdRsToRemove.Values)
                        {
                            if (rsCfg.IdRecurso == rdRsCtlSipPort.ID)
                            {
                                foreach (KeyValuePair<string, RdResource> rdRsPair in rdRsToRemove)
                                {
                                    if ((rsCfg.IdRecurso == rdRsPair.Value.ID) && rdRsPair.Value.MasterMN)
                                    {
                                        if (rdUri[0] != rdRsCtlSipPort.Uri1)
                                        {
                                            rsKeyOld = rdRsCtlSipPort.Uri1;
                                            hayCambiosEnSip = true;
                                            break;
                                        }
                                    }
                                }
                                if (hayCambiosEnSip)
                                    break;
                            }
                        }
                        string rsKey;
                        if (hayCambiosEnSip)
                        {
                            rsKey = rsKeyOld.ToUpper() + rsCfg.Tipo;
                        }
                        else
                        {
                            rsKey = rdUri[0].ToUpper() + rsCfg.Tipo;
                        }
                        //20180724  #3603 FIN
                        //string rsKey = rdUri[0].ToUpper() + rsCfg.Tipo;

                        if (rdRsToRemove.TryGetValue(rsKey, out rdRs))
                        {
                            rdRsToRemove.Remove(rsKey);
                        }
                        else
                        {
                            //Caso de un N que sustituye a un M, ambos recursos tienen el mismo ID
                            //Mantengo el N, con los nuevos par�metros configurados
                            foreach (KeyValuePair<string, RdResource> rdRsPair in rdRsToRemove)
                                if ((rsCfg.IdRecurso == rdRsPair.Value.ID) && rdRsPair.Value.ReplacedMN)
                                {
                                    rdUri[0] = rdRsPair.Value.Uri1;
                                    rdUri[1] = rdRsPair.Value.Uri2;
                                    rdRsToRemove.Remove(rdRsPair.Key);
                                    rsKey = rdRsPair.Key;
                                    rdRs = rdRsPair.Value;
                                    break;
                                }
                        }
                        if (rdRs != null)
                        {
                            // JCAM 20170406.
                            // Si hay cambios en los par�metros de la frecuencia, se debe resetear las sesiones establecidas
                            // con los recursos asociados para que esos cambios tengan efecto
                            // if (hayCambios)
                            //20180724  #3603
                            if (hayCambios || hayCambiosEnSip)
                            {
                                hayCambiosEnSip = false; //#3603
                                if (rdRs.Connected)
                                    RemoveSipCall(rdRs.SipCallId, rdRs.Type);
                                rdRs.Dispose();
                                bool isReplacedMNTemp = rdRs.ReplacedMN;
                                bool isMasterMNTemp = rdRs.MasterMN;
                                rdRs = new RdResource(rsCfg.IdRecurso, rdUri[0], rdUri[1], (RdRsType)rsCfg.Tipo, cfg.Literal, rsCfg.IdEmplazamiento, selectedRs[rsCfg.IdRecurso], new_params, rsCfg);  //EDU 20170223 // JCAM 20170313                            }
                                rdRs.ReplacedMN = isReplacedMNTemp;
                                rdRs.MasterMN = isMasterMNTemp;//#3603
                            }
                            else 
                            {
                                // Hay cambios pero no requieren reinicio de sesion, 
                                // pero si hay que actualizar los datos:
                                // -el emplazamiento
                                rdRs.Site = rsCfg.IdEmplazamiento;    
                             }
                        }
                        else
                        {
                            rdRs = new RdResource(rsCfg.IdRecurso, rdUri[0], rdUri[1], (RdRsType)rsCfg.Tipo, cfg.Literal, rsCfg.IdEmplazamiento, selectedRs[rsCfg.IdRecurso], new_params, rsCfg);  //EDU 20170223 // JCAM 20170313
                        }

                        //if (!rdRs.Connected && rdRs.Selected)
                        //    ChangeSite();

                        _RdRs[rsKey] = rdRs;
                    }
                }
                else 
                {
                    LogDebug<RdFrecuency>(String.Format("Nuevo recurso ", rsCfg.IdRecurso));
                }
            }
            foreach (RdResource rdRs in rdRsToRemove.Values)
            {
                if (TipoDeFrecuencia != "HF")
                {
                    if (rdRs.Connected)
                    {
                         RemoveSipCall(rdRs.SipCallId, rdRs.Type);
                    }
                    rdRs.Dispose();
                }
                else    // 20171116. AGL. Si la frecuencia es HF, y el recurso activo es el transmisor, no se borra de la lista de recursos de la frecuencia.
                {
                    if (rdRs.Connected && rdRs.isTx)
                    {
                        string rsId = rdRs.Uri1.ToUpper() + rdRs.Type/* (uint)rdRs.Type*/;
                        _RdRs[rsId] = rdRs;
                    }
                    else
                    {
                        if (rdRs.Connected)
                        {
                            RemoveSipCall(rdRs.SipCallId, rdRs.Type);
                        }

                        rdRs.Dispose();
                    }
                }
            }

            ConfiguraModoTransmision(cfg);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="topId"></param>
        /// <param name="on"></param>
        /// <param name="responseAddr"></param>
        public void SetRx(string topId, bool on, string responseAddr)
        {
            if (_FrRs != null)
            {
                int index = _RxIds.FindIndex(delegate(string rxId) { return (rxId == topId); });
                if (on)
                {
                    if (index < 0)
                    {
                        _RxIds.Add(topId);
                    }

                    if (responseAddr != null)
                    {
                        RdRegistry.RespondToFrRxChange(responseAddr, _Frecuency, true);
                    }
                }
                else if (index >= 0)
                {
                    SetTx(topId, false, false, CORESIP_PttType.CORESIP_PTT_OFF, null);
                    _RxIds.RemoveAt(index);
                }
            }
        }

        /// <summary>
        /// Proceso de Asignaci�n en TX de una frecuencia...
        /// </summary>
        /// <param name="topId">Usuario que solicita la asignacion</param>
        /// <param name="on">Asignaci�n o Desasignacion</param>
        /// <param name="checkAlreadyAssigned"></param>
        /// <param name="pttType"></param>
        /// <param name="responseAddr">Direccion a la que 'responder' con el resultado</param>
        public void SetTx(string topId, bool on, bool checkAlreadyAssigned, CORESIP_PttType pttType, string responseAddr)
        {

            if (_FrRs != null)
            {
                int index = _TxIds.FindIndex(delegate(string txId) { return (txId == topId); });
                if (on)
                {
                    bool tx = false;
                    //Si no hay configurados recursos de TX no hago nada
                    if (_RdRs.Values.Where(r => r.isTx).Count() == 0)
                        return;
                    if (index >= 0)
                    {
                        _PttTypes[index] = pttType;
                        tx = true;
                    }
                    else if (!checkAlreadyAssigned || (_TxIds.Count == 0))
                    {
                        SetRx(topId, true, null);

                        _TxIds.Add(topId);
                        _PttTypes.Add(pttType);
                        tx = true;
                    }

                    if (responseAddr != null)
                    {
                        RdRegistry.RespondToFrTxChange(responseAddr, _Frecuency, tx);
                    }
                }
                else if (index >= 0)
                {
                    bool changed = false;
                    PttInfo pttInfo = _SrcPtts.Find(delegate(PttInfo p) { return (p.SrcId == topId); });

                    if (pttInfo != null)
                    {
                        _CurrentPttSemaphore.WaitOne();
                        _SrcPtts.Remove(pttInfo);
                        LogTrace<RdFrecuency>(_Frecuency + " SetTX:Remove " + pttInfo.SrcId + ",srcPtts.Count " + _SrcPtts.Count.ToString());

                        if ((_CurrentSrcPtt != null) && (_CurrentSrcPtt.SrcId == pttInfo.SrcId))
                        {
                            changed = NextPtt(topId);
                        }
                        _CurrentPttSemaphore.Release();
                    }

                    if ((_FrRs.RtxGroupId > 0) && (_FrRs.RtxGroupOwner == topId))
                    {
                        RemoveFromRtxGroup(false);
                        changed = true;
                    }

                    _TxIds.RemoveAt(index);
                    _PttTypes.RemoveAt(index);

                    if (changed)
                    {
                        RdRegistry.Publish(_Frecuency, _FrRs);
                    }
                }
            }
        }

        public void ReceivePtt(string topId, PttSource srcType, IEnumerable<int> srcPorts)
        {
            LogTrace<RdFrecuency>(_Frecuency + " ReceivePtt topID " + topId + ",srcType " + srcType.ToString());
            if (_FrRs != null)
            {

                bool changed = false;
                PttInfo pttInfo = _SrcPtts.Find(delegate(PttInfo p) { return (p.SrcId == topId); });

                if (srcType == PttSource.NoPtt)
                {
                    //Debug.Assert((pttInfo != null) || (_CurrentSrcPtt == null) || (_CurrentSrcPtt.SrcId != srcId));
                    if (pttInfo == null)
                        pttInfo = _CurrentSrcPtt;

                    changed = true;

                    if (pttInfo != null)
                    {
                        _CurrentPttSemaphore.WaitOne();
                        _SrcPtts.Remove(pttInfo);
                        LogTrace<RdFrecuency>(_Frecuency + " ReceivePtt:Remove " + pttInfo.SrcId + ",srcPtts.Count " + _SrcPtts.Count.ToString());

                        if ((_CurrentSrcPtt != null) && (_CurrentSrcPtt.SrcId == pttInfo.SrcId))
                        {
                            changed = NextPtt(topId);
                        }
                        _CurrentPttSemaphore.Release();
                    }
                }
                else if (((srcType != PttSource.Avion) && _TxIds.Contains(topId)) ||
                    ((srcType == PttSource.Avion) && (_FrRs.RtxGroupId > 0) && topId.StartsWith("Rtx_" + _FrRs.RtxGroupId + "_")))
                {
                    CORESIP_PttType pttType = CORESIP_PttType.CORESIP_PTT_COUPLING;
                    if (srcType != PttSource.Avion)
                    {
                        int index = _TxIds.FindIndex(delegate(string tx) { return (tx == topId); });
                        pttType = _PttTypes[index];
                    }
                    Debug.Assert(pttType != CORESIP_PttType.CORESIP_PTT_OFF);

                    _CurrentPttSemaphore.WaitOne();
                    if (pttInfo == null)
                    {
                        pttInfo = new PttInfo(topId);
                        _SrcPtts.Add(pttInfo);
                        LogTrace<RdFrecuency>(_Frecuency + " ReceivePtt:Add " + pttInfo.SrcId + ",srcPtts.Count " + _SrcPtts.Count.ToString());
                    }
                    LogTrace<RdFrecuency>(_Frecuency + " set pttType " + pttType.ToString());

                    if (pttInfo.Reset(srcType, pttType, srcPorts))
                    {
                        // JCAM. 20170316. Ya no todos los PTTs tienen la misma prioridad ...
                        /* Suponemos todos los PTT son del mismo tipo-> No es necesaria una ordenaci�n */
                        // JCAM. 20170316. Ahora s� es necesaria una ordenaci�n

                        LogTrace<RdFrecuency>(_Frecuency + " ReceivePtt:Sort " + ",srcPtts.Count " + _SrcPtts.Count.ToString() + " primero: "+_SrcPtts[0].SrcId);
                        _SrcPtts.Sort(delegate(PttInfo x, PttInfo y)
                        {
                            if (x.Type == CORESIP_PttType.CORESIP_PTT_COUPLING)
                            {
                                return (y.Type == CORESIP_PttType.CORESIP_PTT_COUPLING ? 0 : 1);
                            }
                            else if (y.Type == CORESIP_PttType.CORESIP_PTT_COUPLING)
                            {
                                return -1;
                            }

                            return ((int)y.Type - (int)x.Type);
                        });
                    }

                    if ((_CurrentSrcPtt == null) || (_CurrentSrcPtt.SrcId != _SrcPtts[0].SrcId))
                    {
                        changed = NextPtt(topId);
                    }
                    else if (_CurrentSrcPtt.SrcId == pttInfo.SrcId)
                    {
                        ActualizePtt(pttInfo);
                    }
                    _CurrentPttSemaphore.Release();
                }

                // Aunque no haya cambiado el puesto que ha hecho/quitado el ptt tiene que refrescar
                // el estado ptt de la frecuencia
                if (changed || ((pttInfo != null) && (pttInfo.SrcType != PttSource.Avion)))
                {
                    RdRegistry.Publish(_Frecuency, _FrRs);
                }
                
            }
        }

        public bool FindHost(string host)
        {
            return _TxIds.FindIndex(delegate(string tx) { return (tx == host); }) < 0 ? false : true;
        }

        public void RetryFailedConnections()
        {
            foreach (RdResource rdRs in _RdRs.Values)
            {
                if (!rdRs.ToCheck)
                {
                    rdRs.Connect();
                }
            }
        }

        public void CheckFrequency()
        {
            if (_FrRs != null)
            {
                try
                {
                    StatusCheck = _FrRs.FrequencyStatus;
                    _FrRs.FrequencyStatus = this.Status;
                    if (_FrRs.FrequencyStatus != StatusCheck)
                    {
                        RdRegistry.Publish(_Frecuency, _FrRs);
                    }

                }
                catch (Exception x)
                {
                    LogException<RdFrecuency>("CheckFrecuency VHF", x, false);
                }
            }
        }

        /// <summary>
        /// SanityCheckCalls realiza periodicamente desde RdService, una comprobaci�n del 
        /// numero de llamadas establecidas y los recursos de la frecuencia
        /// Si detecta en N ciclos seguidos una incoherencia la quita.
        /// </summary>
        public bool SanityCheckCalls()
        {
            int count = _RdRs.Count();
            count += _RdRs.Values.Where(r => r.Type == RdRsType.RxTx).Count();
            if (_SipRxCalls.Count() + _SipTxCalls.Count() > count)
            {
                LogWarn<RdFrecuency>("SanityCheckCalls fails for frec:" + Frecuency +
                    ",Tx:" + _SipTxCalls.Count() +
                    ",Rx:" + _SipRxCalls.Count() +
                    ",_RdRs:" + _RdRs.Count() + " " + sanityCheckCallsFailures);
                foreach (KeyValuePair<int, RdResource> p in _SipRxCalls)
                    LogWarn<RdFrecuency>(", id " + p.Key);
                sanityCheckCallsFailures++;
            }
            else
            {
                if (sanityCheckCallsFailures > 0) sanityCheckCallsFailures--;
            }
            if (sanityCheckCallsFailures >= MAX_SANITY_CHECK_CALLS_FAILURES)
            {
                LogWarn<RdFrecuency>("SanityCheckCalls in action for frec:" + Frecuency +
                    ",Tx:" + _SipTxCalls.Count() + 
                    ",Rx:" + _SipRxCalls.Count() +
                    ",_RdRs:" + _RdRs.Count());
                return true;
            }
            return false;
        }
                /// <summary>
        /// LimpiaLlamadaDeRecurso identifica una llamada que no tiene asociado un recurso
        /// buscando por Uri: Borra la llamada y elimina el recurso.
        /// </summary>
        /// <param name="sipCalls"> lista de llamadas de RX o TX</param>
        public void LimpiaLlamadaDeRecurso()
        {
            LimpiaLlamadaDeRecurso(_SipRxCalls);
            LimpiaLlamadaDeRecurso(_SipTxCalls);
            sanityCheckCallsFailures = 0;
        }
        /// <summary>
        /// LimpiaLlamadaDeRecurso identifica una llamada que no tiene asociado un recurso
        /// buscando por Uri: Borra la llamada y elimina el recurso.
        /// </summary>
        /// <param name="sipCalls"> lista de llamadas de RX o TX</param>
        private void LimpiaLlamadaDeRecurso(Dictionary<int, RdResource> sipCalls)
        {
            //Hace una copia para poder borrar dentro de la iteracion.
            Dictionary<int, RdResource> recursosSipCall = new Dictionary<int,RdResource> (sipCalls);
            try
            {
                foreach (RdResource recurso in recursosSipCall.Values)
                {
                    if ((_RdRs.Values.Any(r => r.Uri1.Equals(recurso.Uri1)) == false)) 
                    {
                        LogWarn<RdFrecuency>("SanityCheckCalls borra :" + recurso.Uri1);

                        RdResource rdRs = null;
                        if (sipCalls.TryGetValue(recurso.SipCallId, out rdRs))
                        {
                            RemoveSipCall(rdRs);
                            rdRs.Dispose();
                            rdRs.IsForbidden = true;
                        }
                    }
                }
                //JOI 20180425
                foreach (RdResource recurso in recursosSipCall.Values)
                {
                    if ((_RdRs.Values.Any(r => r.SipCallId.Equals(recurso.SipCallId)) == false))
                    {
                        LogWarn<RdFrecuency>("SanityCheckCalls borra :" + recurso.Uri1 + " SipCall " + recurso.SipCallId);

                        RdResource rdRs = null;
                        if (sipCalls.TryGetValue(recurso.SipCallId, out rdRs))
                        {
                            RemoveSipCall(rdRs);
                            rdRs.Dispose();
                            rdRs.IsForbidden = true;
                        }
                    }
                }


                //JOI 20180425 
            }
            catch (Exception ex)
            {
                LogError<RdFrecuency>("LimpiaLlamadaDeRecurso Exception: " + ex.Message);
            }

        }
        /// <summary>
        /// AGL. Que es el _PostPtt ???
        /// </summary>
        public void PublishChanges(object timer)
        {
            LogTrace<RdFrecuency>(_Frecuency + " PublishChanges ");
            if (_FrRs != null)
            {
                if (timer == _PostPtt)
                {
                    RdRegistry.EnablePublish(_Frecuency, true);

                    _PostPtt.Enabled = false;
                    _PostPtt.Dispose();
                    _PostPtt = null;

                    if ((_FrRs.RtxGroupId > 0) && !_SendingPttToRtxGroup &&
                        (_FrRs.Squelch != RdSrvFrRs.SquelchType.NoSquelch) &&
                        ((_CurrentSrcPtt == null) || (_CurrentSrcPtt.SrcId != _FrRs.PttSrcId)))
                    {
                        SendPttToRtxGroup(true, false);
                    }
                }
            }
        }

        /// <summary>
        /// 20170126. AGL. Retorno el RdResource, para poder identificar al recurso en los historicos de Sesiones SIP.
        /// </summary>
        public bool HandleKaTimeout(int sipCallId, out RdResource rdRsOut)
        {
            RdResource rdRs = null;
            if (_SipRxCalls.TryGetValue(sipCallId, out rdRs) || _SipTxCalls.TryGetValue(sipCallId, out rdRs))
            {
                Debug.Assert(rdRs.SipCallId == sipCallId);

                rdRs.HandleKaTimeout(sipCallId);
                RemoveSipCall(sipCallId, rdRs.Type);
                rdRsOut = rdRs;
                return true;
            }
            rdRsOut = rdRs;
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sipCallId"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        // 20170630. AGL. Propiedades para reflejar ON-LINE la seleccion de emplazamiento en recepcion...
        // 20170704. AGL. La anterior versi�n era incorrecta.
        public string SelectedSite 
        {
            get
            {
                switch (this.new_params.FrequencyType)
                {
                    case CORESIP_FREQUENCY_TYPE.Simple:
                    case CORESIP_FREQUENCY_TYPE.FD:
                        RdResource rdr = this.RdRs.Values.Where(r => r.isRx &&
                                                            (r.Squelch && r.new_params.rx_selected)).FirstOrDefault();
                        return rdr == null ? "" : rdr.Site;

                    case CORESIP_FREQUENCY_TYPE.Dual:
                    case CORESIP_FREQUENCY_TYPE.ME:
                        break;

                }
                return "";
            }
        }
        public int SelectedSiteQidx 
        {
            get
            {
                switch (this.new_params.FrequencyType)
                {
                    case CORESIP_FREQUENCY_TYPE.Simple:
                    case CORESIP_FREQUENCY_TYPE.FD:
                        RdResource rdr = this.RdRs.Values.Where(r => r.isRx &&
                                                            (r.Squelch && r.new_params.rx_selected)).FirstOrDefault();
                        return rdr == null ? 0 : rdr.new_params.rx_qidx;

                    case CORESIP_FREQUENCY_TYPE.Dual:
                    case CORESIP_FREQUENCY_TYPE.ME:
                        break;

                }
                return 0;
            }
        }
        /****************************************************************/
        public bool HandleRdInfo(int sipCallId, CORESIP_RdInfo info)
        {
            RdResource rdRs;
            bool changed = false;

            if (_SipRxCalls.TryGetValue(sipCallId, out rdRs) || _SipTxCalls.TryGetValue(sipCallId, out rdRs))
            {
                Debug.Assert(rdRs.SipCallId == sipCallId);

                LogTrace<RdFrecuency>("Estado BSS recibido desde coresip. " + "" +
                                        "Recurso: " + rdRs.ID +
                                        ". PTT: " + info.PttType +
                                        ". PttId: " + info.PttId +
                                        ". Squelch: " + info.Squelch +
                                        ". Seleccionado: " + info.rx_selected +
                                        ". info.rx_qidx: " + info.rx_qidx +
                                        ". en Site: " + rdRs.Site);

                if (rdRs.HandleRdInfo(info) && (_FrRs != null))
                {
                    bool confirmaPortadora = SupervisionPortadora;

                    if (rdRs.isTx)
                    {
                        string oldPttSrcId = _FrRs.PttSrcId;
                        if (rdRs.Ptt == RdRsPttType.OwnedPtt)
                        {
                            if (_CurrentSrcPtt != null)
                            {
                                if (confirmaPortadora && _FrRs.Squelch == RdSrvFrRs.SquelchType.NoSquelch && _WaitingForSuperviser == null)
                                {
                                    _WaitingForSuperviser = new Timer(U5ki.RdService.Properties.Settings.Default.MonitorCarrierTimeOut);
                                    _WaitingForSuperviser.Elapsed += OnTimerSuperviser;
                                    _WaitingForSuperviser.AutoReset = false;
                                    _WaitingForSuperviser.Enabled = true;
                                }

                                ConfirmaPtt();

                                /* Funciona */
                                /*
                                if (!confirmaPortadora)
                                    ConfirmaPtt();
                                else
                                {
                                    // A la espera de confirmar la portadora o confirmar PTT si ya se recibi� el squelch
                                    if (_FrRs.Squelch != RdSrvFrRs.SquelchType.NoSquelch)
                                        ConfirmaPtt();
                                    else
                                        _FrRs.PttSrcId = "";
                                }
                                */
                            }
                        }
                        else if (rdRs.Ptt == RdRsPttType.NoPtt)
                        {
                            //if (_FrRs.PttSrcId != null && _FrRs.RtxGroupId > 0)
                            //{
                            //    ReceivePtt("Rtx_" + _FrRs.RtxGroupId + "_" + _Frecuency, PttSource.NoPtt, null);
                            //}

                            if (_WaitingForSuperviser != null)
                            {
                                _WaitingForSuperviser.Enabled = false;
                                _WaitingForSuperviser.Dispose();
                                _WaitingForSuperviser = null;
                            }

                            _FrRs.PttSrcId = null;
                        }
                        else if (rdRs.Ptt == RdRsPttType.ExternalPtt)
                        {
                            _FrRs.PttSrcId = _Frecuency + "ExternalPtt";

                            if (_WaitingForSuperviser != null)
                            {
                                _WaitingForSuperviser.Enabled = false;
                                _WaitingForSuperviser.Dispose();
                                _WaitingForSuperviser = null;
                            }
                        }

                        changed = (oldPttSrcId != _FrRs.PttSrcId);
                    }

                    if (rdRs.isRx)
                    {
                        //RdSrvFrRs.SquelchType oldSquelch = _FrRs.Squelch;

                        if (confirmaPortadora)
                        {
                            if (rdRs.Squelch)
                            {
                                // PttId recibido y el PttID registrado de la sesi�n son distintos.
                                // Puede ser un ExternalPtt provocado por la confirmaci�n de portadora.
                                // Buscar un rdRs en _SipTxCall un rdRs_Tx con PttId==rdRs.PttId
                                // Si lo encuentra se trata de una confirmaci�n de portadora del recurso rdRs_Tx
                                // Si no lo encuentra ser�a un ExternalPtt
                                //bool confirmaPortadora = ConfirmaPortadora(rdRs.PttId);
                                if (rdRs.Ptt == RdRsPttType.OwnedPtt)
                                {
                                    // Confirmada portadora
                                    if (_CurrentSrcPtt != null)
                                    {
                                        if (_WaitingForSuperviser != null)
                                        {
                                            _WaitingForSuperviser.Enabled = false;
                                            _WaitingForSuperviser.Dispose();
                                            _WaitingForSuperviser = null;
                                        }

                                        ConfirmaPtt();

                                        /* Funciona */
                                        //ConfirmaPtt();
                                        /* */
                                    }
                                }
                                else
                                {
                                    _FrRs.PttSrcId = _Frecuency + "ExternalPtt";

                                    foreach (RdResource rdRs_Tx in _SipTxCalls.Values)
                                    {
                                        if (rdRs_Tx.PttId == rdRs.PttId)
                                        {
                                            // Confirmada portadora
                                            if (_CurrentSrcPtt != null)
                                            {
                                                if (_WaitingForSuperviser != null)
                                                {
                                                    _WaitingForSuperviser.Enabled = false;
                                                    _WaitingForSuperviser.Dispose();
                                                    _WaitingForSuperviser = null;
                                                }

                                                ConfirmaPtt();

                                                /* Funciona */
                                                //ConfirmaPtt();
                                                /* */
                                            }

                                            break;
                                        }
                                    }
                                }
                            }
                            else if (_FrRs.Squelch != RdSrvFrRs.SquelchType.NoSquelch && _WaitingForSuperviser == null)
                            {
                                _WaitingForSuperviser = new Timer(U5ki.RdService.Properties.Settings.Default.MonitorCarrierTimeOut);
                                _WaitingForSuperviser.Elapsed += OnTimerSuperviser;
                                _WaitingForSuperviser.AutoReset = false;
                                _WaitingForSuperviser.Enabled = true;
                            }
                        }

                        //changed = true;

                        // JCAM. El estado de squelch de la frecuencia s�lo cambia
                        // si ha cambiado el estado del recurso seleccionado en rx.
                        if (rdRs.new_params.rx_selected)
                        {
                            _FrRs.SqSite = rdRs.Site;

                            // JCAM.
                            // 27/02/2017. U5KI2.RQ.007.07
                            if (this.new_params.FrequencyType == CORESIP_FREQUENCY_TYPE.FD)
                            {
                                _FrRs.FrequencyStatus = this.Status;
                                _FrRs.ResourceId = rdRs.ID;
                                _FrRs.QidxMethod = rdRs.new_params.bss_method;
                                _FrRs.QidxValue = (UInt32)rdRs.new_params.rx_qidx;
                            }


                            if (rdRs.Squelch)
                            {
                                //Este caso es de SQ no provocado por PTT propio (avion u otro SCV).
                                if ((_CurrentSrcPtt == null) && (info.PttId == 0))
                                {
                                    _LastSQSite = rdRs.Site;
                                    EvaluaTxMute();
                                }
                                _FrRs.Squelch = RdSrvFrRs.SquelchType.SquelchOnlyPort;
                            }
                            else
                            {
                                _FrRs.Squelch = RdSrvFrRs.SquelchType.NoSquelch;
                                /*
                                _FrRs.Squelch = RdSrvFrRs.SquelchType.NoSquelch;
                                foreach (KeyValuePair<int, RdResource> p in _SipRxCalls)
                                {
                                    if (p.Value.Selected)
                                    {
                                        _FrRs.SqlResource = p.Value.Site;
                                        if (p.Value.Squelch)
                                            _FrRs.Squelch = RdSrvFrRs.SquelchType.SquelchOnlyPort;
                                        break;
                                    }
                                }
                                */
                                if ((_CurrentSrcPtt == null) && (_PostPtt != null) &&
                                    (_FrRs.Squelch == RdSrvFrRs.SquelchType.NoSquelch))
                                {
                                    RdRegistry.EnablePublish(_Frecuency, false);

                                    _PostPtt.Enabled = false;
                                    _PostPtt.Dispose();
                                    _PostPtt = null;
                                }
                            }

                            //changed |= ((oldSquelch != _FrRs.Squelch) && rdRs.new_params.rx_selected);
                            changed |= rdRs.new_params.rx_selected;
                        }
                    }
                }
                //Se ha separado la condicion 'change' de la condicion de entrada inicial
                if (changed)
                {
                    LogTrace<RdFrecuency>("Estado BSS enviado al HMI:" +
                                            ", resource ID: " + rdRs.ID +
                                            ", qidx value: " + _FrRs.QidxValue +
                                            ", qidx method: " + _FrRs.QidxMethod +
                                            ", Ptt: " + rdRs.Ptt +
                                            ", Squelch: " + rdRs.Squelch);
                    RdRegistry.Publish(_Frecuency, _FrRs);
                    if (_FrRs.RtxGroupId > 0)
                    {
                        LogTrace<RdFrecuency>("Evento en Grupo RTX (" + _FrRs.RtxGroupOwner + "): " +
                            " _SendingPttToRtxGroup: " + _SendingPttToRtxGroup +
                            ", _FrRs.Squelch: " + _FrRs.Squelch +
                            ", _FrRs.PttSrcId: " + _FrRs.PttSrcId +
                            ", _CurrentSrcPtt null ?: " + (_CurrentSrcPtt == null) +
                            ", _CurrentSrcPtt.SrcId: " + ((_CurrentSrcPtt == null) ? "null" : _CurrentSrcPtt.SrcId) +
                            ", _PostPtt null?: " + (_PostPtt == null).ToString()
                            );
                        if (!_SendingPttToRtxGroup && (_FrRs.Squelch != RdSrvFrRs.SquelchType.NoSquelch) &&
                                ((_CurrentSrcPtt == null) || (_CurrentSrcPtt.SrcId != _FrRs.PttSrcId)) &&
                                (_PostPtt == null))
                        {
                            // La retransmisi�n del Squelch se temporiza para evitar oscilaciones de coupling
                            LogTrace<RdFrecuency>("Evento en Grupo RTX: Activando TIMER...");
                            _RtxSquelchTimer.Enabled = true;
                        }
                        else if (_SendingPttToRtxGroup && ((_FrRs.Squelch == RdSrvFrRs.SquelchType.NoSquelch) ||
                            ((_CurrentSrcPtt != null) && (_CurrentSrcPtt.SrcId == _FrRs.PttSrcId))))
                        {
                            if (_RtxSquelchTimer != null)
                            {
                                _RtxSquelchTimer.Enabled = false;
                                LogTrace<RdFrecuency>("Evento en Grupo RTX (" + _FrRs.RtxGroupOwner + "): Para timer _RtxSquelch porque ptt off y sq off");
                            }

                            SendPttToRtxGroup(false, false);
                        }
                        else
                        {
                            LogTrace<RdFrecuency>("Evento en Grupo RTX (" + _FrRs.RtxGroupOwner + "): Evento en Grupo RTX: Ignorado...");
                        }
                    }
                }
                //VMG 21/09/2018
                //Se a�ade la condicion para publicar las frecuencias degradadas al HMI
                if ((_FrRs != null) && (_FrRs.FrequencyStatus == RdSrvFrRs.FrequencyStatusType.Degraded))
                    RdRegistry.Publish(_Frecuency, _FrRs);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Temporizador que se utiliza para para retrasar el ptt coupling. 
        /// Se utiliza en dos casos:
        /// relacionado con XC1, para evitar oscilaciones en el ptt de retransmisi�n y
        /// como XC2, tiempo de inhibici�n de RTX despues de un ptt propio para evitar bloqueos.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRtxSquelchElapsed(object sender, ElapsedEventArgs e)
        {
            RdService.evQueueRd.Enqueue("OnRtxSquelchElapsed", delegate()
            {
                LogTrace<RdFrecuency>("Evento en Grupo RTX (" + _FrRs.RtxGroupOwner + "): " + "OnRtxSquelchElapsed: " +
                    " _FrRs null?: " + (_FrRs==null) +
                    ", _FrRs.Squelch: " + (_FrRs==null ? "null" : _FrRs.Squelch.ToString()) 
                    );
                _RtxSquelchTimer.Enabled = false;

                if (_FrRs != null && _FrRs.Squelch != RdSrvFrRs.SquelchType.NoSquelch)
                {
                    /** 20180719. Incidencias #3668. Solo activo el grupo si al vencer el timer permanece el SQH y no hay PTT */
                    if (_CurrentSrcPtt == null)
                        SendPttToRtxGroup(true, false);
                }
            });
        }

        /// <summary>
        /// 20170126. AGL. Retorno el RdResource, para poder identificar al recurso en los historicos de Sesiones SIP.
        /// Este m�todo se llama cuando hay una conmutaci�n M+N, entre otros escenarios de inicio/fin de sesi�n.
        /// </summary>
        /// <param name="sipCallId"></param>
        /// <param name="stateInfo"></param>
        /// <returns></returns>
        public bool HandleChangeInCallState(int sipCallId, CORESIP_CallStateInfo stateInfo, out RdResource rdResOut)
        {
            rdResOut = null;
            try
                {
                foreach (RdResource rdRs in _RdRs.Values)
                {
                    bool publish = false;

                    if (rdRs.SipCallId == sipCallId)
                    {
                        //if (rdRs.ToCheck)
                        //{
                        //    rdRs.HandleChangeInCallState(stateInfo);
                        //    // Finalizar la sesi�n SIP iniciada para hacer el check
                        //    RemoveSipCall(rdRs);

                        //    _CheckStatus = stateInfo.State == CORESIP_CallState.CORESIP_CALL_STATE_CONFIRMED;
                        //}
                        //else
                        {
                            bool previousSq = rdRs.Squelch;
                            rdRs.HandleChangeInCallState(stateInfo);
                            if (rdRs.Connected)
                            {
                                publish = AddSipCall(rdRs);
                            }
                            else
                            {
                                RemoveSipCall(sipCallId, rdRs.Type);

                                if (TipoDeFrecuencia == "HF")
                                {
                                    RdSrvFrRs frRs = new RdSrvFrRs();
                                    frRs = _FrRs;

                                    foreach (RdResource rs in _RdRs.Values)
                                    {
                                        if (rs.Connected && rs.Type == RdRsType.Rx)
                                        {
                                            frRs.PttSrcId = "TxHfOff";

                                            //frRs.Squelch = _FrRs.Squelch;
                                            //frRs.RtxGroupId = _FrRs.RtxGroupId;

                                            _PttTypes.Clear();
                                            _TxIds.Clear();

                                            RdRegistry.Publish(_Frecuency, frRs);

                                            frRs.PttSrcId = string.Empty;
                                            break;
                                        }
                                    }
                                }
                                else if (TipoDeFrecuencia == "FD")
                                {
                                    RdSrvFrRs.FrequencyStatusType st = this.Status;
                                    if ((_FrRs != null) && (_FrRs.FrequencyStatus != st))
                                    {
                                        _FrRs.FrequencyStatus = st;

                                        LogInfo<RdService>("FD Status. Frequency ID: " + this.Frecuency + ". Status: " + st,
                                                U5kiIncidencias.U5kiIncidencia.IGRL_U5KI_NBX_INFO, "RdService", CTranslate.translateResource("FD Status. Frequency ID: "  + this.Frecuency + ". Status: " + st.ToString()));
                                        publish = true;
                                    }
                                    if ((rdRs.isRx) && (_FrRs != null))
                                    {
                                        //Quitar Squelch 
                                        if (previousSq && rdRs.new_params.rx_selected == true)
                                        {
                                            _FrRs.Squelch = RdSrvFrRs.SquelchType.NoSquelch;
                                            //Buscar si hay uno nuevo que meter el Squelch que no sea el mismo
                                            foreach (RdResource rdRsAux in _RdRs.Values)
                                            {
                                                if (rdRsAux.SipCallId != -1 && rdRsAux.SipCallId != sipCallId)
                                                {
                                                    if (rdRsAux.isRx && rdRsAux.Squelch && rdRsAux.new_params.rx_selected == true)
                                                    {
                                                        _FrRs.Squelch = RdSrvFrRs.SquelchType.SquelchOnlyPort;
                                                        break;
                                                    }
                                                }
                                            }
                                            publish = true;
                                        }
                                    }
                                }
                            }
                            //Hay que actualizar y publicar los datos de estado y squelch con los del 
                            //recurso que se conecta o desconecta
                            //Se protege con este sem�foro para evitar la excepcion cuando se borra _FrRs desde otro hilo
                            _CurrentPttSemaphore.WaitOne();
                            if (_FrRs != null)
                                publish |= ActualizaFrecuenciaConRecurso(rdRs);
                            _CurrentPttSemaphore.Release();
                            if (publish == true)
                                RdRegistry.PublishStatusFr(_Frecuency, _FrRs);
                        } // foreach end
                        /** */
                        rdResOut = rdRs;
                        break;
                    }
                } // foreach end
            /** */
                }
                catch (Exception x)
                {
                    LogManager.GetCurrentClassLogger().Error(" ERROR {0} ejecutando {1}",  x.Message, x.StackTrace);
                }
            EvaluaTxMute();
            return (rdResOut != null);
        }

        /// <summary>
        /// Funcion auxiliar que actualiza el objeto de publicaci�n de la frecuencia 
        /// con los datos del recurso. Tambi�n actualiza el atributo propio Squelch. 
        /// </summary>
        /// <param name="rdRs"> recurso que hay que tomar como origen de los datos</param>
        /// <param name="rtxGroupId"></param>
        /// <param name="wantedRtxGroupRdFr"></param>
        private bool ActualizaFrecuenciaConRecurso(RdResource rdRs)
        {
            bool hayCambio = false;
            //Actualiza el estado
            if (TipoDeFrecuencia == "FD")
            {
                RdSrvFrRs.FrequencyStatusType st = this.Status;

                if (_FrRs.FrequencyStatus != st)
                {
                    _FrRs.FrequencyStatus = st;

                    LogInfo<RdService>("FD Status. Frequency ID: " + this.Frecuency + ". Status: " + st,
                                U5kiIncidencias.U5kiIncidencia.IGRL_U5KI_NBX_INFO, "RdService", CTranslate.translateResource("FD Status. Frequency ID: " + this.Frecuency,st.ToString()));
                    hayCambio = true;
                    LogInfo<RdService>("ActualizaFrecuenciaConRecurso cambia:" + hayCambio + " St:" + _FrRs.FrequencyStatus);
                }
            }
            LogInfo<RdService>("ActualizaFrecuenciaConRecurso rdRs:" + rdRs.ID + " _FrRs:" + _FrRs.ResourceId);
            //Actualiza el squelch si es un recurso de rx
            if (rdRs.isRx && 
                (((TipoDeFrecuencia == "FD") && (rdRs.ID == _FrRs.ResourceId)) ||
                   TipoDeFrecuencia != "FD"))
            {
                if (rdRs.Squelch == false && _FrRs.Squelch != RdSrvFrRs.SquelchType.NoSquelch)
                {
                    _FrRs.Squelch = RdSrvFrRs.SquelchType.NoSquelch;
                    hayCambio = true;
                }
                else if (rdRs.Squelch == true && _FrRs.Squelch == RdSrvFrRs.SquelchType.NoSquelch)
                    hayCambio = true;				
            }
            LogInfo<RdService>("ActualizaFrecuenciaConRecurso cambia:" + hayCambio + " SQ:" + _FrRs.Squelch);
            return hayCambio;
        }

        public void RemoveSipCall(RdResource rdResource)
        {
            RemoveSipCall(rdResource.SipCallId, rdResource.Type);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rtxGroupOwner"></param>
        /// <param name="rtxGroupId"></param>
        /// <param name="wantedRtxGroupRdFr"></param>
        public static void ChangeRtxGroup(string rtxGroupOwner, uint rtxGroupId, List<RdFrecuency> wantedRtxGroupRdFr)
        {
            List<RdFrecuency> rtxGroupRdFr = new List<RdFrecuency>();
            List<RdFrecuency> rdFrToRemove = new List<RdFrecuency>();
            List<RdFrecuency> actualRtxGroupRdFr;

            if (_RtxGroups.TryGetValue(rtxGroupOwner.ToUpper() + rtxGroupId, out actualRtxGroupRdFr))
            {
                rdFrToRemove.AddRange(actualRtxGroupRdFr);
            }

            foreach (RdFrecuency rdFr in wantedRtxGroupRdFr)
            {
                if (rdFrToRemove.Remove(rdFr) || rdFr.CanBeInRtxGroup(rtxGroupId, rtxGroupOwner))
                {
                    rtxGroupRdFr.Add(rdFr);
                }
            }

            if (rtxGroupRdFr.Count > 0)
            {
                if (rtxGroupRdFr.Count == 1)
                {
                    rdFrToRemove.AddRange(rtxGroupRdFr);
                }
                else
                {
                    RdFrecuency frInSquelch = null;
                    foreach (RdFrecuency rdFr in rtxGroupRdFr)
                    {
                        if (rdFr._FrRs.Squelch != RdSrvFrRs.SquelchType.NoSquelch)
                            frInSquelch = rdFr;
                        else
                            rdFr.AddToRtxGroup(rtxGroupId, rtxGroupOwner);
                    }
                    // La frecuencia que est� con squelch ser� la �ltima que se a�ada al grupo
                    // para que al enviar el PTT estemos seguros que hay m�s de una frecuencia en el grupo
                    if (frInSquelch != null)
                    {
                        frInSquelch.AddToRtxGroup(rtxGroupId, rtxGroupOwner);
                    }
                }
            }

            if (rdFrToRemove.Count > 0)
            {
                rdFrToRemove = RemoveFromRtxGroupWithSquelch(rdFrToRemove);
                foreach (RdFrecuency rdFr in rdFrToRemove)
                {
                    rdFr.RemoveFromRtxGroup(true);
                }
            }
        }

        //public bool HandleWG67(IntPtr wg67, CORESIP_WG67Info wg67Info)
        //{
        //    foreach (RdResource rdRs in _RdRs.Values)
        //    {
        //        if (rdRs.WG67Subscription == wg67)
        //        {
        //            rdRs.HandleWG67Info(wg67Info);
        //            return true;
        //        }
        //    }

        //    return false;
        //}

        private bool ChangeSite()
        {
            bool changed = false;
            string alias = string.Empty;
            string idResource = string.Empty;

            // Buscar los recursos de esta frecuencia.
            foreach (RdResource rdRs in _RdRs.Values)
            {
                if (rdRs.Connected)
                {
                    changed = rdRs.Selected = true;
                    alias = rdRs.Site;
                    idResource = rdRs.ID;

                    _FrRs.SqSite = rdRs.Site;
                    _FrRs.Squelch = rdRs.Squelch ? RdSrvFrRs.SquelchType.SquelchOnlyPort : RdSrvFrRs.SquelchType.NoSquelch;
                }
                //else
                //    rdRs.Selected = false;
            }

            if (changed)
            {
                foreach (KeyValuePair<string, string> item in Picts)
                {
                    Picts[item.Key] = idResource;
                }
                RdRegistry.Publish(_Frecuency, _FrRs);
                RdRegistry.RespondToChangingSite(null, _Frecuency, alias, 1);
            }

            return changed;
        }

        public bool ChangeSite(string hostId, string frequency, string alias)
        {
            bool changed = false;
            string idResource = string.Empty;
            // Buscar los recursos de esta frecuencia.
            foreach (RdResource rdRs in _RdRs.Values)
            {
                rdRs.OldSelected = rdRs.Selected;

                if (rdRs.Connected)
                {
                    rdRs.Selected = false;
                    if (frequency == this._Frecuency && rdRs.Site == alias)
                    {
                        changed = rdRs.Selected = rdRs.OldSelected = true;
                        _FrRs.SqSite = rdRs.Site;
                        _FrRs.Squelch = rdRs.Squelch ? RdSrvFrRs.SquelchType.SquelchOnlyPort : RdSrvFrRs.SquelchType.NoSquelch;
                        idResource = rdRs.ID;
                    }
                }
            }

            if (changed)
            {
                Picts[hostId] = idResource;
                RdRegistry.Publish(_Frecuency, _FrRs);
            }
            else
            {
                foreach (RdResource rdRs in _RdRs.Values)
                {
                    rdRs.Selected = rdRs.OldSelected;
                }
            }

            return changed;
        }

        /// <summary>
        /// A�ade recursos a la frecuencia procedentes de la conmutaci�n M+N
        /// </summary>
        public void ResourceAdd(String key, RdResource resource, bool isMaster) //#3603
        {
            if (_ModoTransmision == Tipo_ModoTransmision.UltimoReceptor)
                resource.TxMute = true;
            RdRs[key] = resource;
            resource.ReplacedMN = true;
            resource.MasterMN = isMaster; //#3603
        }

        /** 20180621. AGL. Obtiene el String para MTTO referido al Transmisor seleccionado */
        public string SelectedTxSiteString
        {
            get
            {
                switch (getParam.FrequencyType)
                {
                    case CORESIP_FREQUENCY_TYPE.Simple:
                        break;
                    case CORESIP_FREQUENCY_TYPE.ME:
                    case CORESIP_FREQUENCY_TYPE.Dual:
                        break;
                    case CORESIP_FREQUENCY_TYPE.FD:
                        switch (_ModoTransmision)
                        {
                            case Tipo_ModoTransmision.Manual:
                            case Tipo_ModoTransmision.Ninguno:
                                break;
                            case Tipo_ModoTransmision.Climax:
                                return "CLX";
                            case Tipo_ModoTransmision.UltimoReceptor:
                                return _TxRsSelected==null ? "???" : _TxRsSelected.Site;
                        }
                        break;
                }
                return "---";
            }
        }

        #region Private Members
        /// <summary>
        /// 
        /// </summary>
        class PttInfo
        {
            /// <summary>
            /// 
            /// </summary>
            public string SrcId;
            /// <summary>
            /// 
            /// </summary>
            public PttSource SrcType = PttSource.NoPtt;
            /// <summary>
            /// 
            /// </summary>
            public CORESIP_PttType Type = CORESIP_PttType.CORESIP_PTT_OFF;
            /// <summary>
            /// 
            /// </summary>
            public List<int> SrcPorts = new List<int>();

            /// <summary>
            /// 
            /// </summary>
            /// <param name="srcId"></param>
            public PttInfo(string srcId)
            {
                SrcId = srcId;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="p"></param>
            public PttInfo(PttInfo p)
            {
                SrcId = p.SrcId;
                SrcType = p.SrcType;
                Type = p.Type;
                SrcPorts.AddRange(p.SrcPorts);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="srcType"></param>
            /// <param name="type"></param>
            /// <param name="srcPorts"></param>
            /// <returns></returns>
            public bool Reset(PttSource srcType, CORESIP_PttType type, IEnumerable<int> srcPorts)
            {
                CORESIP_PttType oldType = Type;

                SrcType = srcType;
                Type = type;

                SrcPorts.Clear();
                SrcPorts.AddRange(srcPorts);

                return (oldType != Type);
            }
        }


        private bool _flag = false;
        /// <summary>
        /// 
        /// </summary>
        private static Logger _Logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// 
        /// </summary>
        private static Dictionary<string, List<RdFrecuency>> _RtxGroups = new Dictionary<string, List<RdFrecuency>>();
        /// <summary>
        /// 
        /// </summary>
        private string _Frecuency = null;
        /// <summary>
        /// 
        /// </summary>
        private RdSrvFrRs _FrRs = null;
        /// <summary>
        /// 
        /// </summary>
        private PttInfo _CurrentSrcPtt = null;
        /// <summary>
        /// 
        /// </summary>
        private List<PttInfo> _SrcPtts = new List<PttInfo>();
        /// <summary>
        /// 
        /// </summary>
        private List<string> _TxIds = new List<string>();
        /// <summary>
        /// 
        /// </summary>
        private List<string> _RxIds = new List<string>();
        /// <summary>
        /// 
        /// </summary>
        private Dictionary<int, RdResource> _SipTxCalls = new Dictionary<int, RdResource>();
        /// <summary>
        /// 
        /// </summary>
        private Dictionary<int, RdResource> _SipRxCalls = new Dictionary<int, RdResource>();
        /// <summary>
        /// 
        /// </summary>
        private List<CORESIP_PttType> _PttTypes = new List<CORESIP_PttType>();
        /// <summary>
        /// 
        /// </summary>
        private Dictionary<string, RdResource> _RdRs = new Dictionary<string, RdResource>();
        public Dictionary<string, RdResource> RdRs
        {       
            get
            { return _RdRs; }
        }
        /// <summary>
        /// 
        /// </summary>
        private bool _SendingPttToRtxGroup = false;
        public bool SendingPttToRtxGroup
        {
            get { return _SendingPttToRtxGroup; }
        }
        /// <summary>
        /// 
        /// </summary>
        private Timer _PostPtt = null;
        /// <summary>
        /// 
        /// </summary>
        private Timer _WaitingForSuperviser;

        /// <summary>
        /// Temporizador que se utiliza para para retrasar el ptt coupling. 
        /// Se utiliza en dos casos:
        /// relacionado con XC1, para evitar oscilaciones en el ptt de retransmisi�n y
        /// como XC2, tiempo de inhibici�n de RTX despues de un ptt propio para evitar bloqueos.
        /// </summary>
        private Timer _RtxSquelchTimer;

        /// <summary>
        /// 
        /// </summary>
        private Timer _DisableFrequencyTimer;



        /// <summary>
        /// AGL.HF. Contiene el Tipo Frecuencia VHF, UHF o HF
        /// </summary>
        private string _FrecuencyType = "VHF";
        public string TipoDeFrecuencia
        {
            get { return _FrecuencyType; }
            set { _FrecuencyType = value; }
        }

        /// <summary>
        /// AGL.HF. Contiene la frecuencia en kHz
        /// </summary>
        private int _FrecuenciaSintonizada = 2850;
        public int FrecuenciaSintonizada
        {
            get { return _FrecuenciaSintonizada; }
            set { _FrecuenciaSintonizada = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rsId"></param>
        /// <param name="sysCfg"></param>
        /// <returns></returns>
        //private string RsUri(string rsId, ConfiguracionSistema sysCfg)
        //{
        //    string ip = sysCfg.GetGwRsIp(rsId);
        //    return (ip != null) ? string.Format("<sip:{0}@{1}>", rsId, ip) : null;
        //}

        private string[] RsUri(string rsId, ConfiguracionSistema sysCfg)
        {
            string[] rsUri = new string[2];

            string ip1 = sysCfg.GetGwRsIp(rsId, 1);
            string ip2 = sysCfg.GetGwRsIp(rsId, 2);

            rsUri[0] = (ip1 != null) ? string.Format("<sip:{0}@{1}>", rsId, ip1) : null;
            rsUri[1] = (ip2 != null) ? string.Format("<sip:{0}@{1}>", rsId, ip2) : null;

            return rsUri;
        }

        /// <summary>
        /// Esta funci�n se usa cuando se conecta un nuevo recurso a esta frecuencia
        /// </summary>
        /// <param name="rdRs"></param>
        /// <return> true si es necesario publicar el recurso, falso en caso contrario </return>
        private bool AddSipCall(RdResource rdRs)
        {

            Debug.Assert(rdRs.SipCallId >= 0);
            Debug.Assert(rdRs.Connected);
            bool publish = false;

            if (rdRs.isRx)
            {
                _SipRxCalls[rdRs.SipCallId] = rdRs;

                if (_SendingPttToRtxGroup)
                {
                    Debug.Assert(_FrRs != null);
                    Debug.Assert(_FrRs.RtxGroupId > 0);

                    SendPttToRtxGroup(true, true);
                }

                //EDU 08/06/2017
                // Buscamos un transmisor del mismo emplazamiento y frecuencia y a este receptor le enviamos el mismo PTT
                if ((_CurrentSrcPtt != null) && (rdRs.Type != RdRsType.RxTx))
                {
                    foreach (KeyValuePair<int, RdResource> q in _SipTxCalls)
                    {
                        if ((q.Value.Site == rdRs.Site) && 
                            (q.Value.Frecuency == rdRs.Frecuency))
                        {
                            SipAgent.PttOn(rdRs.SipCallId, rdRs.PttId, _CurrentSrcPtt.Type, rdRs.PttMute);
                            break;
                        }
                    }
                }
            }
            if (rdRs.isTx)
            {
                _SipTxCalls[rdRs.SipCallId] = rdRs;

                if (_CurrentSrcPtt != null)
                {
                    RdMixer.Link(_CurrentSrcPtt.SrcType, _CurrentSrcPtt.SrcPorts, rdRs.SipCallId);
                    SipAgent.PttOn(rdRs.SipCallId, rdRs.PttId, _CurrentSrcPtt.Type, rdRs.PttMute);

                    //EDU 08/06/2017
                    // Al receptor del mismo emplazamiento y frecuencia se le envia el mismo PTT type
                    if (rdRs.Type != RdRsType.RxTx)
                    {
                        foreach (KeyValuePair<int, RdResource> q in _SipRxCalls)
                        {
                            if ((q.Value.Site == rdRs.Site) && (q.Value.Frecuency == rdRs.Frecuency))
                            {
                                SipAgent.PttOn(q.Key, q.Value.PttId, _CurrentSrcPtt.Type, q.Value.PttMute);
                                break;
                            }
                        }
                    }
                }
            }

            if ((_FrRs == null) && HasSIPSession())     
            {
                _FrRs = new RdSrvFrRs();
                //Actualizo datos del squelch que est�n en los recursos asociados de RX
                RdResource rdr = this.RdRs.Values.Where(r => r.isRx && (r.Squelch && r.new_params.rx_selected)).FirstOrDefault();
                if (rdr != null)
                {
                    _FrRs.Squelch = RdSrvFrRs.SquelchType.SquelchOnlyPort;
                    _FrRs.SqSite = rdr.Site;
                }
                publish = true;
            }

            // Para abortar el envio temporizado del ASPA
            if (TipoDeFrecuencia != "HF" && HasSIPSession())
            {
                if (_DisableFrequencyTimer != null && _DisableFrequencyTimer.Enabled)
                {
                    _DisableFrequencyTimer.Enabled = false;
                    _flag = false;
                }
            }
            return publish;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sipCallId"></param>
        /// <param name="rsType"></param>
        private void RemoveSipCall(int sipCallId, RdRsType rsType)
        {
                if ((rsType == RdRsType.Rx) || (rsType == RdRsType.RxTx))
                {
                    _SipRxCalls.Remove(sipCallId);
                    if (_SipRxCalls.Count == 0 && TipoDeFrecuencia == "HF")
                        _SipTxCalls.Clear();

                    if (_SendingPttToRtxGroup)
                    {
                        Debug.Assert(_FrRs != null);
                        Debug.Assert(_FrRs.RtxGroupId > 0);
                        bool squelchInFreq = false;
                        //La RTX se mantiene s�lo si a�n queda un recurso conectado con SQ
                        foreach (KeyValuePair<int, RdResource> q in _SipRxCalls)
                        {
                            if (q.Value.Squelch)
                                squelchInFreq = true;
                        }
                        SendPttToRtxGroup(squelchInFreq, true);
                    }
                }
                if ((rsType == RdRsType.Tx) || (rsType == RdRsType.RxTx))
                {
                    RdResource rdRs = null;
                    // sipCallID puede no existir en _SipTxCalls si la llamada no ha estado establecida(CONFIRMED)
                    _SipTxCalls.TryGetValue(sipCallId, out rdRs);//EDU 08/06/2017

                    _SipTxCalls.Remove(sipCallId);

                    if (_CurrentSrcPtt != null)
                    {
                        SipAgent.PttOff(sipCallId);
                        RdMixer.Unlink(_CurrentSrcPtt.SrcType, _CurrentSrcPtt.SrcPorts, sipCallId);

                        //EDU 08/06/2017
                        //Al receptor del mismo emplazamiento y frecuencia se le envia el PTT off
                        if (rdRs != null)
                        {
                            foreach (KeyValuePair<int, RdResource> q in _SipRxCalls)
                            {
                                if ((q.Value.Site == rdRs.Site) && (q.Value.Frecuency == rdRs.Frecuency))
                                {
                                    SipAgent.PttOff(q.Key);
                                    break;
                                }
                            }
                        }
                    }
                }

            /** AGL. 20151022. Reseteo la frecuencia cuando no haya receptores o no haya transmisores. */
            /** AGL. 20160212. Esto solo es v�lido en frecuencias no HF */
            if (TipoDeFrecuencia == "HF")
            {
                if ((_FrRs != null) && !HasSIPSession() && (_SipTxCalls.Count == 0))
                {
                    Reset(false);
                }
            }
            else
            {
                if ((_FrRs != null) && !HasSIPSession())
                {
                    LogInfo<RdFrecuency>("Enviando ASPAS temporizada en " + Frecuency);
                    Reset(true);
                }
            }
        }
        /// <summary>
        /// Devuelve true si est� en servicio a nivel SIP (tiene sesion/es).
        /// Para HF y frecuencias con s�lo RX, se comprueba que hay al menos una llamada en RX
        /// Para frecuencias con RX y TX, se comprueba que hay llamadas en RX y en TX
        /// </summary>
        /// <returns>true si est� en servicio a nivel SIP seg�n su configuraci�n</returns>
        private bool HasSIPSession()
        {
            bool session = false;
            if (TipoDeFrecuencia == "HF")
            {
                if (_SipRxCalls.Count > 0)
                    session = true;
            }
            else
            {
                int TxConf = _RdRs.Values.Where(r => r.isTx).Count();
                if ((_SipRxCalls.Count > 0) && ((_SipTxCalls.Count > 0) || TxConf == 0))
                    session = true;
            }
            return session;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rtxGroupId"></param>
        /// <param name="rtxGroupOwner"></param>
        /// <returns></returns>
        private bool CanBeInRtxGroup(uint rtxGroupId, string rtxGroupOwner)
        {
            return ((_FrRs != null) &&
                (((_FrRs.RtxGroupId == 0) && _TxIds.Contains(rtxGroupOwner)) ||
                ((_FrRs.RtxGroupId == rtxGroupId) && (_FrRs.RtxGroupOwner == rtxGroupOwner))));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rtxGroupId"></param>
        /// <param name="rtxGroupOwner"></param>
        private void AddToRtxGroup(uint rtxGroupId, string rtxGroupOwner)
        {
            Debug.Assert(_FrRs != null);
            Debug.Assert((_FrRs.RtxGroupId == 0) || ((_FrRs.RtxGroupId == rtxGroupId) && (_FrRs.RtxGroupOwner == rtxGroupOwner)));
            LogTrace<RdFrecuency>(_Frecuency + " AddToRtxGroup id " + rtxGroupId.ToString());

            if (_FrRs.RtxGroupId == 0)
            {
                _FrRs.RtxGroupId = rtxGroupId;
                _FrRs.RtxGroupOwner = rtxGroupOwner;

                List<RdFrecuency> rtxGroupRdFr;
                string owner = rtxGroupOwner.ToUpper() + rtxGroupId;

                if (!_RtxGroups.TryGetValue(owner, out rtxGroupRdFr))
                {
                    rtxGroupRdFr = new List<RdFrecuency>();
                    _RtxGroups[owner] = rtxGroupRdFr;
                }

                rtxGroupRdFr.Add(this);
                RdRegistry.Publish(_Frecuency, _FrRs);

                if (_FrRs.RtxGroupId > 0 && _RtxGroups[owner].Count > 1)
                {
                    if (!_SendingPttToRtxGroup && (_FrRs.Squelch != RdSrvFrRs.SquelchType.NoSquelch) &&
                            ((_CurrentSrcPtt == null) || (_CurrentSrcPtt.SrcId != _FrRs.PttSrcId)) &&
                            (_PostPtt == null))
                    {
                        SendPttToRtxGroup(true, false);
                    }
                }
            }
            else if (_SendingPttToRtxGroup)
            {
                SendPttToRtxGroup(true, true);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void RemoveFromRtxGroup(bool publish)
        {
            LogTrace<RdFrecuency>(_Frecuency + " RemoveFromRtxGroup ");
            if ((_FrRs != null) && (_FrRs.RtxGroupId > 0))
            {
                string owner = _FrRs.RtxGroupOwner.ToUpper() + _FrRs.RtxGroupId;
                List<RdFrecuency> rtxGroupFr = _RtxGroups[owner];

                if (_FrRs.Squelch != RdSrvFrRs.SquelchType.NoSquelch)
                    SendPttToRtxGroup(false, false);

                bool removed = rtxGroupFr.Remove(this);
                Debug.Assert(removed);

                PttInfo pttInfo = _SrcPtts.Find(delegate(PttInfo p)
                {
                    if (_CurrentSrcPtt != null)
                        return (p.SrcId == _CurrentSrcPtt.SrcId);

                    return false;
                });

                if (pttInfo != null)
                {
                    _CurrentPttSemaphore.WaitOne();
                        _SrcPtts.Remove(pttInfo);
                    LogTrace<RdFrecuency>(_Frecuency + "RemoveFromRtxGroup:Remove " + pttInfo.SrcId + ",srcPtts.Count " + _SrcPtts.Count.ToString());
                        if ((_CurrentSrcPtt != null) && (_CurrentSrcPtt.SrcType == PttSource.Avion))
                        {
                            NextPtt();
                        }
                    _CurrentPttSemaphore.Release();
                }

                if (rtxGroupFr.Count == 1)
                {
                    rtxGroupFr[0].RemoveFromRtxGroup(true);
                    Debug.Assert(rtxGroupFr.Count == 0);
                }
                else if (rtxGroupFr.Count == 0)
                {
                    _RtxGroups.Remove(owner);
                }

                _FrRs.RtxGroupId = 0;
                _FrRs.RtxGroupOwner = null;

                if (publish)
                {
                    RdRegistry.Publish(_Frecuency, _FrRs);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool NextPtt(string topId = null)
        {
            bool changed = true;
            PttInfo oldPtt = _CurrentSrcPtt;

            if (_CurrentSrcPtt != null)
            {
                //EDU 08/06/2017
                /*
                foreach (int sipTxCall in _SipTxCalls.Keys)
                {
                    SipAgent.PttOff(sipTxCall);
                }
                    */
                //EDU 08/06/2017
                foreach (KeyValuePair<int, RdResource> p in _SipTxCalls)
                {
                    LogTrace<RdFrecuency>("PttOff_1 " + p.Value.Frecuency + ",srcPtts.Count " + _SrcPtts.Count.ToString());
                    SipAgent.PttOff(p.Key);

                    //Al receptor del mismo emplazamiento y frecuencia se le envia el mismo PTT off
                    foreach (KeyValuePair<int, RdResource> q in _SipRxCalls)
                    {
                        if ((q.Value.Site == p.Value.Site) && (q.Value.Frecuency == p.Value.Frecuency))
                        {
                            LogTrace<RdFrecuency>("PttOff_2 " + p.Value.Frecuency);
                            SipAgent.PttOff(q.Key);
                            break;
                        }
                    }
                }

                RdMixer.Unlink(_CurrentSrcPtt.SrcType, _CurrentSrcPtt.SrcPorts, _SipTxCalls.Keys);
                _CurrentSrcPtt = null;

                //ReceivePtt("Rtx_" + _FrRs.RtxGroupId + "_" + _Frecuency, PttSource.NoPtt, null);
                //ReceivePtt("Rtx_" + _FrRs.RtxGroupId + "_" + _Frecuency, PttSource.Avion, _SipRxCalls.Keys);
            }

            if (_SrcPtts.Count > 0)
            {
                _CurrentSrcPtt = new PttInfo(_SrcPtts[0]);
                LogTrace<RdFrecuency>("NextPtt _srcPtt[0]" + _SrcPtts[0].SrcId + " srcPtts.Count " + _SrcPtts.Count.ToString());

                RdMixer.Link(_CurrentSrcPtt.SrcType, _CurrentSrcPtt.SrcPorts, _SipTxCalls.Keys);

                foreach (KeyValuePair<int, RdResource> p in _SipTxCalls)
                {
                    // JCAM. 27/02/2017
                    // Todos los recursos asociados a la frecuencia en transmisi�n hacen PTT
                    LogTrace<RdFrecuency>("PttOn_1 " + p.Value.Frecuency + "," + _CurrentSrcPtt.Type.ToString() + ",srcPtts.Count " + _SrcPtts.Count.ToString());
                    SipAgent.PttOn(p.Key, p.Value.PttId, _CurrentSrcPtt.Type, p.Value.PttMute);

                    //EDU 08/06/2017
                    //Al receptor del mismo emplazamiento y frecuencia se le envia el PTT type
                    if (p.Value.Type != RdRsType.RxTx)
                    {
                        foreach (KeyValuePair<int, RdResource> q in _SipRxCalls)
                        {
                            if ((q.Value.Site == p.Value.Site) && (q.Value.Frecuency == p.Value.Frecuency))
                            {
                                LogTrace<RdFrecuency>("PttOn_2 " + p.Value.Frecuency + "," + _CurrentSrcPtt.Type.ToString());
                                SipAgent.PttOn(q.Key, q.Value.PttId, _CurrentSrcPtt.Type, q.Value.PttMute);
                                break;
                            }
                        }
                    }

                    if (p.Value.Ptt == RdRsPttType.OwnedPtt)
                    {
                        _FrRs.PttSrcId = _CurrentSrcPtt.SrcId;
                        changed = true;
                    }
                }
            }

            // Creo que esto no tiene sentido ya al incluir el semaforo y 
            // adem�s no est� bien implementado (changed siempre true). Igual se puede quitar (Blanca 28/06/17)
            if (oldPtt != _CurrentSrcPtt)
            {
                if (_PostPtt != null)
                {
                    _PostPtt.Enabled = false;
                    _PostPtt.Dispose();
                }

                if (!changed)
                {
                    RdRegistry.DisablePublish(_Frecuency);

                    _PostPtt = new Timer(_CurrentSrcPtt != null ? 200 : 750);
                    _PostPtt.AutoReset = false;
                    _PostPtt.Elapsed += OnTimerElapsed;
                    _PostPtt.Enabled = true;
                }
            }

            return changed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        private void ActualizePtt(PttInfo p)
        {
            _CurrentSrcPtt.Type = p.Type;

            List<int> currentPorts = new List<int>(_CurrentSrcPtt.SrcPorts);
            _CurrentSrcPtt.SrcPorts.Clear();

            foreach (int srcPort in p.SrcPorts)
            {
                if (!currentPorts.Contains(srcPort))
                {
                    RdMixer.Link(_CurrentSrcPtt.SrcType, srcPort, _SipTxCalls.Keys);
                }

                _CurrentSrcPtt.SrcPorts.Add(srcPort);
            }

            if (_CurrentSrcPtt.SrcType != p.SrcType)
            {
                Debug.Assert((_CurrentSrcPtt.SrcType != PttSource.Avion) && (p.SrcType != PttSource.Avion), "1");
                Debug.Assert(_CurrentSrcPtt.SrcPorts.Count == 1, "2");

                if (p.SrcType == PttSource.Hmi)
                {
                    if (_CurrentSrcPtt.SrcType == PttSource.Instructor)
                    {
                        RdMixer.Link(PttSource.Alumn, _CurrentSrcPtt.SrcPorts, _SipTxCalls.Keys);
                    }
                    else
                    {
                        Debug.Assert(_CurrentSrcPtt.SrcType == PttSource.Alumn);
                        RdMixer.Link(PttSource.Instructor, _CurrentSrcPtt.SrcPorts, _SipTxCalls.Keys);
                    }
                }
                else
                {
                    Debug.Assert(p.SrcType == PttSource.Instructor, "Ptt nuevo: " + p.SrcType.ToString());
                    Debug.Assert(_CurrentSrcPtt.SrcType == PttSource.Alumn, "Ptt anterior: " + _CurrentSrcPtt.SrcType);

                    RdMixer.Unlink(PttSource.Alumn, _CurrentSrcPtt.SrcPorts, _SipTxCalls.Keys);
                    RdMixer.Link(PttSource.Instructor, _CurrentSrcPtt.SrcPorts, _SipTxCalls.Keys);
                }

                _CurrentSrcPtt.SrcType = p.SrcType;
            }
        }

        /// <summary>
        /// Se usa para realizar/quitar la retransmision del SQ recibido por la frecuencia, 
        /// a las frecuencias del grupo
        /// </summary>
        /// <param name="pttOn">true si ptt on, false si ptt off</param>
        /// <param name="force"></param>
        private void SendPttToRtxGroup(bool pttOn, bool force)
        {
            //Establece una secci�n cr�tica para corregir el desorden de eventos de Ptt de rtx
            lock (_RtxGroups)
            {
                List<RdFrecuency> rtxGroupFr = _RtxGroups[_FrRs.RtxGroupOwner.ToUpper() + _FrRs.RtxGroupId];
                LogTrace<RdFrecuency>(_Frecuency + " SendPttToRtxGroup on " + pttOn.ToString() + ",force " + force.ToString() + 
                    " RtxGr " + _RtxGroups.Count().ToString()+ "_sendingPtt:"+_SendingPttToRtxGroup.ToString());
 
               if (pttOn && (!_SendingPttToRtxGroup || force))
                {
                    Debug.Assert(_FrRs.RtxGroupId > 0);
                    //Debug.Assert(_FrRs.Squelch != RdSrvFrRs.SquelchType.NoSquelch);

                    Debug.Assert(rtxGroupFr.Count > 1);

                   //Solo envio los PTT coupling si nadie en el grupo est� ya retransmitiendo o es forzado 
                    if ((IsGroupActive(rtxGroupFr) == false) || force)
                    {
                        _SendingPttToRtxGroup = true;

                        foreach (RdFrecuency rdFr in rtxGroupFr)
                        {
                            if (rdFr != this)
                            {
                                rdFr.ReceivePtt("Rtx_" + _FrRs.RtxGroupId + "_" + _Frecuency, PttSource.Avion, _SipRxCalls.Keys);
                            }
                        }
                    }
                }
                else if (!pttOn && (_SendingPttToRtxGroup || force))
                {
                    Debug.Assert(_FrRs.RtxGroupId > 0);

                    _SendingPttToRtxGroup = false;

                    foreach (RdFrecuency rdFr in rtxGroupFr)
                    {
                        if (rdFr != this)
                        {
                            rdFr.ReceivePtt("Rtx_" + _FrRs.RtxGroupId + "_" + _Frecuency, PttSource.NoPtt, null);
                        }
                    }
                }
            }
        }
        //Devuelve true si alguna frecuencia del grupo ya est� retransmitiendo
        private bool IsGroupActive (List<RdFrecuency> rtxGroupFr)
        {
            bool isGroupActive = false;
            foreach (RdFrecuency rdFr in rtxGroupFr)
            {
                if (rdFr != this)
                {
                    isGroupActive |= rdFr.SendingPttToRtxGroup;
                }
            }
            return isGroupActive;
        }
        /// <summary>
        /// 
        /// </summary>
        private void Reset(bool temporizacion)
        {
            if (_FrRs != null)
            {
                // Solo afectaria a reset si est� temporizado
                if (temporizacion)
                {
                    if (_DisableFrequencyTimer != null && _DisableFrequencyTimer.Enabled)
                        return;

                    if (!_flag)
                    {
                        _flag = true;
                        if (_DisableFrequencyTimer != null)
                            _DisableFrequencyTimer.Enabled = true;
                        return;
                    }
                }
                else
                {
                    if (_DisableFrequencyTimer != null && _DisableFrequencyTimer.Enabled)
                        _DisableFrequencyTimer.Enabled = false;
                }


                if (_PostPtt != null)
                {
                    _PostPtt.Enabled = false;
                    _PostPtt.Dispose();
                    _PostPtt = null;
                }
                if ((_CurrentSrcPtt != null) && (_SipTxCalls.Count > 0))
                {
                    foreach (int sipTxCall in _SipTxCalls.Keys)
                    {
                        SipAgent.PttOff(sipTxCall);

                        //EDU 08/06/2017
                        RdResource rdRs = _SipTxCalls[sipTxCall];   //EDU 08/06/2017
                        //Al receptor del mismo emplazamiento y frecuencia se le envia el PTT off
                        foreach (KeyValuePair<int, RdResource> q in _SipRxCalls)
                        {
                            if ((q.Value.Site == rdRs.Site) && (q.Value.Frecuency == rdRs.Frecuency))
                            {
                                SipAgent.PttOff(q.Key);
                                break;
                            }
                        }

                    }

                    RdMixer.Unlink(_CurrentSrcPtt.SrcType, _CurrentSrcPtt.SrcPorts, _SipTxCalls.Keys);
                }

                RemoveFromRtxGroup(false);
                _CurrentPttSemaphore.WaitOne();

                _CurrentSrcPtt = null;

                LogTrace<RdFrecuency>(_Frecuency + "Reset:Clear " + " srcPtts.Count " + _SrcPtts.Count.ToString());
                _SrcPtts.Clear();
                _PttTypes.Clear();
                _RxIds.Clear();
                _TxIds.Clear();
                _FrRs = null;

                RdRegistry.Publish(_Frecuency, null);
                _CurrentPttSemaphore.Release();
            }
        }

        /// <summary>
        /// Tiempo que tiene que transcurrir para que una frecuencia se considere
        /// como ca�da. (Puede haber sido un cambio M->N o vv.)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnDisableFrequencyElapsed(object sender, ElapsedEventArgs e)
        {
            RdService.evQueueRd.Enqueue("OnDisableFrequencyElapsed", delegate()
            {
                if (_DisableFrequencyTimer != null)
                    _DisableFrequencyTimer.Enabled = false;
                _flag = false;

                Reset(false);
                LogInfo<RdFrecuency>("ASPAS enviadas en " + Frecuency);
            });
        }


        /// <summary>
        /// AGL. ???. Que timer es este???
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTimerSuperviser(object sender, ElapsedEventArgs e)
        {
            LogTrace<RdFrecuency>(_Frecuency + " OnTimerSuperviser ");
            SendPttToRtxGroup(false, false);

            _WaitingForSuperviser.Enabled = false;
            _WaitingForSuperviser.Dispose();
            _WaitingForSuperviser = null;

            _FrRs.PttSrcId = "NO_CARRIER";

            RdRegistry.Publish(_Frecuency, _FrRs);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            General.SafeLaunchEvent(TimerElapsed, sender, this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rdFrToRemove"></param>
        /// <returns></returns>
        private static List<RdFrecuency> RemoveFromRtxGroupWithSquelch(List<RdFrecuency> rdFrToRemove)
        {
            List<RdFrecuency> sinSquelch = new List<RdFrecuency>();

            foreach (RdFrecuency rdFr in rdFrToRemove)
            {
                if (rdFr._FrRs.Squelch != RdSrvFrRs.SquelchType.NoSquelch)
                {
                    rdFr.RemoveFromRtxGroup(true);
                }
                else
                    sinSquelch.Add(rdFr);
            }

            return sinSquelch;
        }

        /// <summary>
        /// 
        /// </summary>
        private void ConfirmaPtt()
        {
            _FrRs.PttSrcId = _CurrentSrcPtt.SrcId;

            if (_PostPtt != null)
            {
                RdRegistry.EnablePublish(_Frecuency, false);

                _PostPtt.Enabled = false;
                _PostPtt.Dispose();
                _PostPtt = null;
            }
        }

        /// <summary>
        /// Actualiza los nuevos par�metros de la frecuencia con los recibidos en la configuraci�n
        /// Retorna true si ha habido cambios con respecto a los anteriores configurados
        /// </summary>
        /// <param name="cfg"></param>
        private bool ResetNewParams(CfgEnlaceExterno cfg)
        {
            bool hayCambios = this.new_params.CLDCalculateMethod != (CORESIP_CLD_CALCULATE_METHOD)cfg.MetodoCalculoClimax ||
                                this.new_params.BssWindows != cfg.VentanaSeleccionBss ||
                                this.new_params.AudioSync != cfg.SincronizaGrupoClimax ||
                                this.new_params.NotUnassignable != cfg.FrecuenciaNoDesasignable ||
                                this.new_params.AudioInBssWindow != cfg.AudioPrimerSqBss ||
                                this.new_params.Priority != (CORESIP_Priority)cfg.PrioridadSesionSip ||
                                this.new_params.cld_supervision_time != cfg.CldSupervisionTime ||
                                this.new_params.MetodosBssOfrecidos != cfg.MetodosBssOfrecidos;

            this.new_params.CLDCalculateMethod = (CORESIP_CLD_CALCULATE_METHOD)cfg.MetodoCalculoClimax;
            this.new_params.BssWindows = cfg.VentanaSeleccionBss;
            this.new_params.AudioSync = cfg.SincronizaGrupoClimax;
            // Convertir los tipos de frecuencia a los esperados por el m�dulo de tratamiento BSS-CLIMAX
            switch (cfg.TipoFrecuencia)
            {
                case Tipo_Frecuencia.DUAL:
                    hayCambios |= this.new_params.FrequencyType != CORESIP_FREQUENCY_TYPE.Dual;
                    this.new_params.FrequencyType = CORESIP_FREQUENCY_TYPE.Dual;
                    break;
                case Tipo_Frecuencia.FD:
                    hayCambios |= this.new_params.FrequencyType != CORESIP_FREQUENCY_TYPE.FD;
                    this.new_params.FrequencyType = CORESIP_FREQUENCY_TYPE.FD;
                    break;
                case Tipo_Frecuencia.ME:
                    hayCambios |= this.new_params.FrequencyType != CORESIP_FREQUENCY_TYPE.ME;
                    this.new_params.FrequencyType = CORESIP_FREQUENCY_TYPE.ME;
                    break;
                default:
                    hayCambios |= this.new_params.FrequencyType != CORESIP_FREQUENCY_TYPE.Simple;
                    this.new_params.FrequencyType = CORESIP_FREQUENCY_TYPE.Simple;
                    break;
            }

            this.new_params.NotUnassignable = cfg.FrecuenciaNoDesasignable;
            this.new_params.AudioInBssWindow = cfg.AudioPrimerSqBss;
            this.new_params.Priority = (CORESIP_Priority)cfg.PrioridadSesionSip;
            this.new_params.cld_supervision_time = cfg.CldSupervisionTime;
            this.new_params.MetodosBssOfrecidos = cfg.MetodosBssOfrecidos;

            return hayCambios;
        }


        private void SendLogNewStatus(RdSrvFrRs.FrequencyStatusType oldStatus)
        {
            if (oldStatus != _Status)
                LogInfo<RdService>("FS Status. Frequency ID: " + this.Frecuency + ". Status: " + _Status,
                U5kiIncidencias.U5kiIncidencia.IGRL_U5KI_NBX_INFO, "RdService",
                CTranslate.translateResource("FS Status. Frequency ID: " + this.Frecuency + " Status: " + _Status.ToString()));
        }

        /// <summary>
        /// Gestiona los cambios en el metodo de transmision recibido en la configuraci�n
        /// Actualiza el valor TxSelected inicial de los recursos de Tx seg�n corresponda a lo configurado
        /// Los cambios de configuracion relacionados con el modo de transmisi�n tienen efecto en el
        /// siguiente Ptt, no son instantaneos
        /// </summary>
        /// <param name="cfg"></param>
        private void ConfiguraModoTransmision(CfgEnlaceExterno cfg)
        {

            if (cfg.TipoFrecuencia == Tipo_Frecuencia.FD)
            {
                if (_ModoTransmision != cfg.ModoTransmision)
                {
                    _ModoTransmision = cfg.ModoTransmision;
                    //S�lo a los TX o TXRX
                    foreach (RdResource rdRs in _RdRs.Values)
                        if (rdRs.isTx)
                            rdRs.TxMute = _ModoTransmision == Tipo_ModoTransmision.UltimoReceptor ? true : false;
                    _TxRsSelected = null;
                }                
                EvaluaTxMute();
            }
            else
            {
                if (_ModoTransmision != cfg.ModoTransmision)
                {
                    foreach (RdResource rdRs in _RdRs.Values)
                        if (rdRs.isTx)
                            rdRs.TxMute = false;
                    _TxRsSelected = null;
                    _LastSQSite = "";
                }
                _ModoTransmision = Tipo_ModoTransmision.Ninguno;
            }
        }

        /// <summary>
        /// Selecciona un transmisor de entre los conectados con estos criterios:
        /// -El que tenga el emplazamiento guardado en _LastSQSite siempre tiene preferencia
        /// -si no hay uno ya seleccionado previamente, cualquiera conectado
        /// Si no hay ninguna radio conectada, deselecciona el TX, si lo hay
        /// Si no tiene valor _LastSQSite, se le pone el del seleccionado (para poder conservar M+N)
        /// Si hay un cambio de seleccion de TX, con Ptt en curso, se actualizan los Ptt 
        /// para mantenerlo en el aire
        /// </summary>
        /// <param name="cfg"></param>
        private void EvaluaTxMute()
        {
            bool hayTxSeleccionado = false;
            bool hayTxEnSite = false;
            RdResource txSelected = null;
            RdResource txConnected = null;
            
            if (_ModoTransmision == Tipo_ModoTransmision.UltimoReceptor)
            {
                //Para optimizar busquedas se usa _TxRsSelected
                if ((_TxRsSelected != null) && 
                    (_TxRsSelected.Connected) &&
                    (_TxRsSelected.Site == _LastSQSite))
                    return;
                //Se busca el seleccionado entre los recursos
                foreach (RdResource rdRs in _RdRs.Values)
                    if (rdRs.isTx)
                    {
                        hayTxSeleccionado |= !rdRs.TxMute;
                        if (rdRs.Connected)
                        {
                            if (rdRs.Site == _LastSQSite)
                            {
                                hayTxEnSite = true;
                                txConnected = rdRs;
                            }
                            else if (!hayTxEnSite)
                                txConnected = rdRs;
                        }
                        if (!rdRs.TxMute) txSelected = rdRs;
                    }
                //Deselecciona
                if (hayTxSeleccionado && 
                    (!txSelected.Connected || hayTxEnSite))
                {
                    txSelected.TxMute = true;
                    hayTxSeleccionado = false;
                    _TxRsSelected = null;
                    //Cambio din�mico del PTT por cambio de TX seleccionado 
                    if (!string.IsNullOrEmpty(PttSrc))
                        SipAgent.PttOn(txSelected.SipCallId, txSelected.PttId, _CurrentSrcPtt.Type, txSelected.PttMute);
                }
                //Selecciona
                if (!hayTxSeleccionado && (txConnected != null))
                {
                    txConnected.TxMute = false;
                    _TxRsSelected = txConnected;
                    //Cambio din�mico del PTT por cambio de TX seleccionado 
                    if (!string.IsNullOrEmpty(PttSrc))
                        SipAgent.PttOn(txConnected.SipCallId, txConnected.PttId, _CurrentSrcPtt.Type, txConnected.PttMute);
                    //Para evitar cambios por M+N o por sectorizaci�n, si no ha habido ningun SQ antes.
                    if (_LastSQSite == "")
                        _LastSQSite = _TxRsSelected.Site;
                }
            }
        }

        #endregion

        #region Datos para mostrar en WEB para DEBUG

        public object PrivateData
        {
            get
            {
                return new
                {
                    SanityCheckCallsFailures = sanityCheckCallsFailures,
                    ModoTransmision = _ModoTransmision,
                    TxRsSelected = _TxRsSelected,
                    LastSQSite = _LastSQSite,
                    Flag = _flag,
                    RtxGroups = from g in _RtxGroups select new { key = g.Key, grp = from f in g.Value select new { id = f.Frecuency } },
                    FrRs = _FrRs,
                    CurrentSrcPtt = _CurrentSrcPtt,
                    SrcPtts = from s in _SrcPtts select new { PttInfo = s },
                    TxIds = from t in _TxIds select new { id = t },
                    RxIds = from t in _RxIds select new { id = t },
                    SipTxCalls = from c in _SipTxCalls select new { key = c.Key, val = c.Value },
                    SipRxCalls = from c in _SipRxCalls select new { key = c.Key, val = c.Value },
                    PttTypes = from p in _PttTypes select new { Ptt = p },
                    RdRs = from r in _RdRs select new { key = r.Key, val = r.Value },
                    SendingPttToRtxGroup = SendingPttToRtxGroup,
                };
            }
        }

        public object PublicData
        {
            get
            {
                return new
                {
                    SupervisionPortadora = SupervisionPortadora,
                    PttSrc = PttSrc,
                    Picts = from pict in Picts select new { key = pict.Key, val = pict.Value },
                    SelectedSite = SelectedSite,
                    SelectedSiteQidx = SelectedSiteQidx,
                    SelectedTxSiteString = SelectedTxSiteString,
                    TipoDeFrecuencia = TipoDeFrecuencia,
                    FrecuenciaSintonizada = FrecuenciaSintonizada
                };           
            }
        }

        #endregion
    }
}
