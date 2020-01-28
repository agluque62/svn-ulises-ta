using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HMI.Model.Module.BusinessEntities;

using Utilities;
using HMI.Model.Module.Messages;
using NLog;
using HMI.CD40.Module.Properties;
using U5ki.Infrastructure;
using System.Collections;
using System.Xml;
using System.Xml.Linq;
namespace HMI.CD40.Module.BusinessEntities
{
    public class TlfPickUp
    {
        public class DialogData
        {
            public string callId;
            public string state;
            public string remoteId;
            public string toTag;
            public string fromTag;
            public string display;
        }

        public event GenericEventHandler<ListenPickUpMsg> PickUpChanged;
        public event GenericEventHandler<SnmpStringMsg<string, string>> SetSnmpString;
        public event GenericEventHandler<string> SipMessageReceived;
        public event GenericEventHandler<string> PickUpError;
        public FunctionState State
        {
            get { return _State; }
            private set
            {
                if (value != _State)
                {
                    _State = value;

                    if (_State == FunctionState.Executing)
                    {
                        General.SafeLaunchEvent(PickUpChanged, this, new ListenPickUpMsg(_State, _Target.Literal));
                    }
                    else
                    {
                        General.SafeLaunchEvent(PickUpChanged, this, new ListenPickUpMsg(_State));
                    }
                }
            }
        }
        public TlfPickUp()
        {
            Top.Sip.NotifyDialog += OnNotifyDialog;
            Top.Sip.SipMessage += OnSipMessageReceived;
            Top.Cfg.ProxyStateChangeCfg += OnProxyStateChange;
        }
        /// <summary>
        /// Constructor used only for testing
        /// </summary>
        /// <param name="forTesting"></param>
        public TlfPickUp(bool forTesting)
        {
            
        }
        /// <summary>
        /// Do the preparatives to execute the pick up with AD
        /// makes the subscription to get information about destination to be pickUp'd
        /// </summary>
        /// <param name="id">AD destination to be pickUp'd</param>
        /// <returns></returns>
        public void Prepare(int id)
        {
            if (id < Tlf.NumDestinations)
            {
                _Target = Top.Tlf[id];
                if (_Target != null)
                    PrepareCommon();
                else 
                    _Logger.Error(String.Format("TlfPickUp:Prepare AD error target is null"));
            }
            else
                _Logger.Error(String.Format("TlfPickUp:Prepare AD out of range: {0}", id));
        }


        /// <summary>
        /// Do the preparatives to execute the pick up with AID:
        /// Only allows captures from prefix 0 (TA users)
        /// makes the subscription to get information about destination to be pickUp'd
        /// </summary>
        /// <param name="id">destination data to be pickUp'd</param>
        /// <returns></returns>
        public void Prepare(uint prefix, string dst, string number, string lit)
        {
            if (_State == FunctionState.Idle)
            {
                if (prefix != 0)
                {
                    State = FunctionState.Error;
                    _Logger.Error(String.Format("TlfPickUp:Prepare AID error target is prefix {0}", prefix));
                }
                else
                {
                    _Target = Top.Tlf.SearchTlfPosition(prefix, dst, number, lit, true);
                    if (_Target != null)
                        PrepareCommon();
                    else
                        _Logger.Error(String.Format("TlfPickUp:Prepare AID error target is null"));
                }
            }
            else
                _Logger.Error(String.Format("TlfPickUp:Prepare AID error state: {0}", _State));
        }
        /// <summary>
        /// Parsea los datos relevantes que llegan en el info del notify 
        /// </summary>
        /// <param name="xml"></param>
        /// <returns> lista de dialogData</returns>
        public List<DialogData> NotifyDialogParse(string xml, out string source)
        {
            List<DialogData> dialogList = new List<DialogData>();
            source = null;
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                XmlNodeList nodes = doc.GetElementsByTagName("dialog-info");
                source = (nodes.Item(0).Attributes["entity"] != null) ? nodes.Item(0).Attributes.GetNamedItem("entity").Value : null;
                XmlNodeList dialogs = doc.GetElementsByTagName("dialog");
                foreach (XmlNode dialog in dialogs)
                {
                    XmlNodeList dialogData = dialog.ChildNodes;

                    DialogData dlg = new DialogData();
                    dlg.callId = (dialog.Attributes["call-id"] != null) ? dialog.Attributes.GetNamedItem("call-id").Value : null;
                    dlg.toTag = (dialog.Attributes["local-tag"] != null) ? dialog.Attributes.GetNamedItem("local-tag").Value : null;
                    dlg.fromTag = (dialog.Attributes["remote-tag"] != null) ? dialog.Attributes.GetNamedItem("remote-tag").Value : null;
                    foreach (XmlNode node in dialogData)
                    {
                        if (node.Name == "state")
                        {
                            dlg.state = node.InnerText;
                        }
                        else if (node.Name == "remote")
                        {
                            XmlNodeList remoteNode = node.ChildNodes;
                            foreach (XmlNode remoteData in remoteNode)
                            {
                                if (remoteData.Name == "identity")
                                {
                                    dlg.remoteId = remoteData.InnerText;
                                    dlg.display = (remoteData.Attributes["display"] != null) ? remoteData.Attributes["display"].Value : null;
                                }
                            }
                        }
                    }
                    //Hay que descartar los dialogos early de llamadas salientes
                    string direction = (dialog.Attributes["direction"] != null) ? dialog.Attributes.GetNamedItem("direction").Value : null;
                    if ((direction != "initiator") || (dlg.state != "early"))
                        dialogList.Add(dlg);
                }
            }
            catch (Exception exc)
            {
                _Logger.Error("NotifyDialog Parse error {0} en {1}", exc.Message, xml);
                dialogList.Clear();
            }
            return dialogList;
        }

        /// <summary>
        /// Finaliza la captura de la llamada enviando un Invite con replaces y los datos del dialogo
        /// Se cancela la suscripcion
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Capture(int id)
        {
            if (State == FunctionState.Idle)
                return false;
            if (id < Tlf.NumDestinations+Tlf.NumIaDestinations)
            {                
                _Captured = Top.Tlf[id];
                DialogData dialog;
                if (_Ringing.TryGetValue(id, out dialog))
                {
                    //TODO que pasa si falla una captura, intento las otras?
                    if (_Captured.CallPickUp(dialog))
                    {
                        _IgnoreNotify = true;
                        if (_TargetUris != null)
                            foreach (string uri in _TargetUris)
                                SipAgent.DestroyDialogSubscription(uri);
                        _Captured.TlfPosStateChanged += OnCapturedCallStateChanged;
                        return true;
                    }
                    else
                    {
                        State = FunctionState.Error;
                        _Logger.Warn(String.Format("TlfPickUp:Capture pickUp failed: {0}", _Captured.Literal));
                        return true;
                    }
                }
                else
                    _Logger.Error(String.Format("TlfPickUp:Capture not ringing in captured destination: {0}", _Captured.Literal));
            }
            else
                _Logger.Error(String.Format("TlfPickUp:Capture out of range: {0}", id));
            return false;
        }
        public bool Cancel(int id)
        {
            if (State != FunctionState.Idle)
            {
                if ((_Target == null) || ((_Target.Pos != id) && (_Ringing.ContainsKey(id) == false)))
                    return false;
                else
                    return Cancel(false);
            }
            else
                return false;
        }
        /// <summary>
        /// Cancel the pickUp process
        /// If this cancellation is due to changes in _target, then its state is not changed
        /// otherwise is set back to Idle.
        /// </summary>
        /// <param name="changeTargetState"></param>
        /// <returns></returns>
        public bool Cancel(bool changeTargetState = true)
        {
            if (State != FunctionState.Idle)
            {
                State = FunctionState.Idle;
                if (_Target != null)
                {
                    _Target.TlfPosStateChanged -= OnTargetCallStateChanged;
                    if (_TargetUris != null)
                        foreach (string uri in _TargetUris)
                            SipAgent.DestroyDialogSubscription(uri);
                    if ((changeTargetState) || (_Target.State == TlfState.InProcess))
                        _Target.State = TlfState.Idle;
                     _Target = null;
                     _TargetUris = null;
                }
                if (_Captured != null)
                {
                    _Captured = null;
                }
                foreach (int pos in _Ringing.Keys)
                {
                    Top.Tlf[pos].State = TlfState.Idle;
                }
                _Ringing.Clear();
                return true;
            }
            return false;
        }

        #region Private Members
        private const string PickUpTag="-PickUp-";
        private static readonly Logger _Logger = LogManager.GetCurrentClassLogger();
        private bool _UseProxy = false;
        /// <summary>
        /// Target position whose calls I want to capture
        /// </summary>
        private TlfPosition _Target = null;
        /// <summary>
        /// Position to be captured
        /// </summary>
        private TlfPosition _Captured = null;
        /// <summary>
        /// Positions that are able to be captured
        /// </summary>
        private Dictionary<int, DialogData> _Ringing = new Dictionary<int, DialogData>();
        ///State of pickUp procedure:
        ///Idle: No pickUp in progress
        ///Ready: PickUp initiated, waiting target to be selected
        ///Executing: Target selected, waiting to select the call to be captured
        ///Error: Capture process failed
        private FunctionState _State = FunctionState.Idle;
        ArrayList _TargetUris = null;
        /// <summary>
        /// Once the capture has begun, the notifies are ignored because may be caused by the capture itself
        /// </summary>
        private bool _IgnoreNotify = false;

        private void PrepareCommon()
        {
            _TargetUris = _Target.GetUris();
            if (_TargetUris != null)
            {
                _IgnoreNotify = false;
                _Target.State = TlfState.InProcess;
                bool error = false;
                foreach (string uri in _TargetUris)
                    error &= SipAgent.CreateDialogSubscription(_Target.Channels[0].AccId, uri, _UseProxy);
                    if (!error)
                    {
                    State = FunctionState.Executing;
                    _Target.TlfPosStateChanged += OnTargetCallStateChanged;

                    Top.WorkingThread.Enqueue("SetSnmp", delegate()
                    {
                        string snmpString = Top.Cfg.PositionId + "_" + "CAPTURE" + "_"+_Target.Literal;
                        General.SafeLaunchEvent(SetSnmpString, this, new SnmpStringMsg<string, string>(Settings.Default.TlfFacilityOid, snmpString));
                    });
                }
                else
                {
                    State = FunctionState.Error;
                    _Logger.Error("TlfPickUp::PrepareCommon CreateDialogSubscription error [0]", _Target.Literal);
                }
                }
                else
                {
                    State = FunctionState.Error;
                _Logger.Error("TlfPickUp::PrepareCommon target [0] with no path", _Target.Literal);
            }
        }

        /// <summary>
        /// Si el objetivo sobre el que capturamos cambia de estado, damos por errónea la captura 
        /// </summary>
        /// <param name="sender"></param>
        private void OnTargetCallStateChanged(object sender)
        {
            TlfPosition tlf = (TlfPosition)sender; 
            if ((tlf.OldState != TlfState.InProcess) || (tlf.State != TlfState.Idle))
                Cancel(false);
        }

        /// <summary>
        /// Al establecerse la llamada y terminar con exito la captura se envia el mensaje
        /// al target
        /// </summary>
        /// <param name="sender"></param>
        private void OnCapturedCallStateChanged(object sender)
        {
            TlfPosition tlf = (TlfPosition)sender;
            try
            {
            if (tlf.State == TlfState.Set)
            {
                _Captured.TlfPosStateChanged -= OnCapturedCallStateChanged;
                SipAgent.SendInstantMessage(_Target.Channels[0].AccId, _Target.Uri, PickUpTag + Top.Cfg.PositionId + Resources.HasCaptured + _Captured.Literal, _UseProxy);
                _Ringing.Remove(_Captured.Pos);
                foreach (int pos in _Ringing.Keys)
                    Top.Tlf[pos].State = TlfState.Idle;
                _Ringing.Clear();
                _Target.TlfPosStateChanged -= OnTargetCallStateChanged;
                    _Target.State = TlfState.Idle; // se pasa a reposo al terminar la captura, no al entrar los ringing
                _Target = null;
                _TargetUris = null;
                State = FunctionState.Idle;
                Top.WorkingThread.Enqueue("SetSnmp", delegate()
                {
                        string snmpString = Top.Cfg.PositionId + "_" + "CAPTURED" +"_"+ _Captured.Literal;
                    General.SafeLaunchEvent(SetSnmpString, this, new SnmpStringMsg<string, string>(Settings.Default.TlfFacilityOid, snmpString));
                });
            }
        }
            catch (Exception exc)
            {
                _Logger.Error("OnCapturedCallStateChanged exception {0}", exc.Message);
            }
        }

        private void OnProxyStateChange(object sender, bool state)		
        {
            _UseProxy = state;
        }
        /// <summary>
        /// Receive data of present dialogs.
        /// With early dialogs it signals ringing state
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="xml"></param>
        /// <param name="len"></param>
        private void OnNotifyDialog(object sender, string xml)
        {
            string source = null;
            if ((State != FunctionState.Executing) || _IgnoreNotify)
                return;
            List<DialogData> dialogs = NotifyDialogParse(xml, out source);
            
            if (_Target != null) 
            {
                try
                {
                    //it's for me
                    IEnumerable<DialogData> earlyDialogs = dialogs.Where<DialogData>(x => x.state == "early");
                    IEnumerable<DialogData> otherDialogs = dialogs.Where<DialogData>(x => x.state != "early");
                    foreach (DialogData dialog in otherDialogs)
                    {
                        TlfPosition tlf = Top.Tlf.GetTlfPosition(dialog.remoteId, dialog.display);
                        if ((tlf != null) && _Ringing.ContainsKey(tlf.Pos))
                        {
                            _Ringing.Remove(tlf.Pos);
                            //tlf.State = TlfState.Idle;
                            tlf.State = tlf.OldState;
                        }
                    }
                    foreach (DialogData dialog in earlyDialogs)
                    {
                        TlfPosition tlf = Top.Tlf.GetTlfPosition(dialog.remoteId, dialog.display);
                        if (tlf != null)
                        {
                            //No se tienen en cuenta las capturadas que ya estaban en entrantes
                            //en el sistema, tampoco se da por fallida la captura 
                            // ej: captura de una linea pto a punto que suena en todos los puestos
                            if (tlf.State != TlfState.In)
                            {
                                tlf.State = TlfState.In;
                                _Ringing.Add(tlf.Pos, dialog);
                            }
                        }
                        else
                        {
                            //No hay hueco para la entrante
                            State = FunctionState.Error;
                            foreach (int pos in _Ringing.Keys)
                                Top.Tlf[pos].State = TlfState.Idle;
                            _Ringing.Clear();
                            //_Target.State = TlfState.Idle; 
                            if (_TargetUris != null)
                                foreach (string uri in _TargetUris)
                                    SipAgent.DestroyDialogSubscription(uri);
                            General.SafeLaunchEvent(PickUpError, this, Resources.CaptureErrorCapacity);
                            break;
                        }
                    }
                    //if (_Ringing.Count > 0) 
                    //    _Target.State = TlfState.Idle;

                    //State = FunctionState.Idle;
                }
                catch (Exception exc)
                {
                    _Logger.Error("NotifyDialog exception {0}", exc.Message);
                }
            }
            else
                _Logger.Error("NotifyDialog not expected from {0}", source);

        }

        /// <summary>
        /// Receive a text message.
        /// Only implemented in pick Up scenarioss
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="textMessage"></param>
        private void OnSipMessageReceived(object sender, string textMessage)
        {
            if (textMessage.StartsWith(PickUpTag))
                //it's for me
                General.SafeLaunchEvent(SipMessageReceived, this, textMessage.Substring(PickUpTag.Length));
        }
        #endregion
    }
}
