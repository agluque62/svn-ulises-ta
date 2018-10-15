//----------------------------------------------------------------------------------------
// patterns & practices - Smart Client Software Factory - Guidance Package
//
// This file was generated by the "Add View" recipe.
//
// This class is the concrete implementation of a View in the Model-View-Presenter 
// pattern. Communication between the Presenter and this class is acheived through 
// an interface to facilitate separation and testability.
// Note that the Presenter generated by the same recipe, will automatically be created
// by CAB through [CreateNew] and bidirectional references will be added.
//
// For more information see:
// ms-help://MS.VSCC.v80/MS.VSIPCC.v80/ms.practices.scsf.2007may/SCSF/html/02-09-010-ModelViewPresenter_MVP.htm
//
// Latest version of this Guidance Package: http://go.microsoft.com/fwlink/?LinkId=62182
//----------------------------------------------------------------------------------------

using System;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Practices.CompositeUI.SmartParts;
using Microsoft.Practices.ObjectBuilder;
using Microsoft.Practices.CompositeUI;
using Microsoft.Practices.CompositeUI.EventBroker;
using HMI.Infrastructure.Interface;
using HMI.Model.Module.Services;
using HMI.Model.Module.UI;
using HMI.Model.Module.BusinessEntities;
using HMI.Model.Module.Messages;
using HMI.Presentation.Urr.Constants;
using HMI.Presentation.Urr.UI;
using HMI.Presentation.Urr.Properties;
using Utilities;
using NLog;

namespace HMI.Presentation.Urr.Views
{
    [SmartPart]
    public partial class TlfIaView : UserControl
    {
        private static Logger _Logger = LogManager.GetCurrentClassLogger();

        private WorkItem _WorkItem;
        private IModelCmdManagerService _CmdManager;
        private StateManagerService _StateManager;
        private Keypad _Keypad;
        private MemUC _Mem;
        private Dictionary<HMIButton, Color> _SlowBlinkList;
        private bool _SlowBlinkOn = true;
        private bool _IsCurrentView = false;
        private int? _CallTimestamp = null;
        private Number[] _Historic = new Number[4];
        private TlfDst _SetCall = null;

        private bool _KeypadEnabled
        {
            get
            {
                if (_StateManager.Tlf.Unhang.State != UnhangState.Idle)
                {
                    return false;
                }
                if (!_StateManager.Tft.Enabled || !_StateManager.Engine.Operative ||
                    (_StateManager.Tlf.Priority.State == PriorityState.Error) ||
                    (_StateManager.Tlf.Listen.State == ListenState.Executing) || (_StateManager.Tlf.Listen.State == ListenState.Error) ||
                    (_StateManager.Tlf.Transfer.State == TransferState.Executing) || (_StateManager.Tlf.Transfer.State == TransferState.Error))
                {
                    return false;
                }
                if ((_StateManager.Tlf.Listen.State == ListenState.Ready) ||
                    (_StateManager.Tlf.Transfer.State == TransferState.Ready) ||
                    (_StateManager.Tlf[TlfState.Set] + _StateManager.Tlf[TlfState.Conf] == 1))
                {
                    return true;
                }
                if (//(_StateManager.Tlf.Unhang.State != UnhangState.Idle) ||
                    (_StateManager.Tlf[Tlf.IaMappedPosition].State == TlfState.In) ||
                    (_StateManager.Tlf[Tlf.IaMappedPosition].State == TlfState.InPrio) ||
                    (_StateManager.Tlf[Tlf.IaMappedPosition].State == TlfState.RemoteIn))
                {
                    return false;
                }

                return true;
            }
        }
        private bool _CallEnabled
        {
            get
            {
                if (!_StateManager.Tft.Enabled || !_StateManager.Engine.Operative ||
                    (_StateManager.Tlf.Priority.State == PriorityState.Error) ||
                    (_StateManager.Tlf.Listen.State == ListenState.Executing) || (_StateManager.Tlf.Listen.State == ListenState.Error) ||
                    (_StateManager.Tlf.Transfer.State == TransferState.Executing) || (_StateManager.Tlf.Transfer.State == TransferState.Error))
                {
                    return false;
                }
                if ((_StateManager.Tlf.Listen.State == ListenState.Ready) ||
                    (_StateManager.Tlf.Transfer.State == TransferState.Ready))
                {
                    return Tlf.ValidateNumber(_Keypad.Digits);
                }
                if (_StateManager.Tlf.Unhang.State != UnhangState.Idle)
                {
                    return true;
                }
                if (//(_StateManager.Tlf[TlfState.Set] + _StateManager.Tlf[TlfState.Conf] == 1) ||
                    (_StateManager.Tlf[Tlf.IaMappedPosition].State == TlfState.In) ||
                    (_StateManager.Tlf[Tlf.IaMappedPosition].State == TlfState.InPrio) ||
                    (_StateManager.Tlf[Tlf.IaMappedPosition].State == TlfState.RemoteIn))
                {
                    return false;
                }

                return Tlf.ValidateNumber(_Keypad.Digits);
            }
        }
        private bool _MemEnabled
        {
            get { return _StateManager.Tft.Enabled; }
        }
        private bool _NumEnabled
        {
            get
            {
                return _StateManager.Tft.Enabled && _StateManager.Engine.Operative &&
                    (_StateManager.Tlf.Priority.State != PriorityState.Error) &&
                    (_StateManager.Tlf.Listen.State != ListenState.Executing) &&
                    (_StateManager.Tlf.Listen.State != ListenState.Error) &&
                    (_StateManager.Tlf.Transfer.State != TransferState.Error) &&
                    (_StateManager.Tlf.Transfer.State != TransferState.Executing) &&
                    (_StateManager.Tlf[Tlf.IaMappedPosition].State != TlfState.In) &&
                    (_StateManager.Tlf[Tlf.IaMappedPosition].State != TlfState.InPrio) &&
                    (_StateManager.Tlf[Tlf.IaMappedPosition].State != TlfState.RemoteIn);
            }
        }

        private string _Num1    // Miguel
        {
            get { return Resources.Num1; }
        }
        private string _Num2    // Miguel
        {
            get { return Resources.Num2; }
        }
        private string _Num3    // Miguel
        {
            get { return Resources.Num3; }
        }
        private string _Num4    // Miguel
        {
            get { return Resources.Num4; }
        }
        private string _MEM    // Miguel
        {
            get { return Resources.Mem; }
        }

        public TlfIaView([ServiceDependency] WorkItem workItem, [ServiceDependency] IModelCmdManagerService cmdManager, [ServiceDependency] StateManagerService stateManager)
        {
            InitializeComponent();
            _IaToolsWS.Name = WorkspaceNames.IaToolsWorkspace;

            _WorkItem = workItem;
            _CmdManager = cmdManager;
            _StateManager = stateManager;
            _SlowBlinkList = new Dictionary<HMIButton, Color>();

            int pos = 0;
            foreach (string num in Settings.Default.Historic)
            {
                string[] numAlias = num.Split(',');
                _Historic[pos++] = new Number(numAlias[0], numAlias[1]);
            }

            _Keypad = _WorkItem.SmartParts.AddNew<Keypad>(ViewNames.KeypadView);
            _Mem = _WorkItem.SmartParts.AddNew<MemUC>(ViewNames.MemView);

            _Keypad.NewKey += OnKeypadNewKey;
            _Keypad.ClearClick += OnKeypadClear;
            _Mem.OkClick += OnMemOkClick;
            _Mem.CancelClick += OnMemCancelClick;

            _Num1BT.Tag = 0;
            _Num2BT.Tag = 1;
            _Num3BT.Tag = 2;
            _Num4BT.Tag = 3;

            // Miguel
            _Num1BT.Text = _Historic[0] != null ? _Historic[0].Alias : _Num1;
            _Num2BT.Text = _Historic[1] != null ? _Historic[1].Alias : _Num2;
            _Num3BT.Text = _Historic[2] != null ? _Historic[2].Alias : _Num3;
            _Num4BT.Text = _Historic[3] != null ? _Historic[3].Alias : _Num4;

            _MemBT.Text = _MEM; // Miguel
        }

        [EventSubscription(EventTopicNames.TftEnabledChanged, ThreadOption.Publisher)]
        [EventSubscription(EventTopicNames.EngineStateChanged, ThreadOption.Publisher)]
        public void OnTftEngineChanged(object sender, EventArgs e)
        {
            if (_IsCurrentView)
            {
                _Keypad.Enabled = _KeypadEnabled;
                _CallBT.Enabled = _CallEnabled;
                _MemBT.Enabled = _MemEnabled;

                bool numEnabled = _NumEnabled;
                _Num1BT.Enabled = numEnabled && (_Historic[0] != null);
                _Num2BT.Enabled = numEnabled && (_Historic[1] != null);
                _Num3BT.Enabled = numEnabled && (_Historic[2] != null);
                _Num4BT.Enabled = numEnabled && (_Historic[3] != null);
            }
        }

        [EventSubscription(EventTopicNames.ActiveViewChanging, ThreadOption.Publisher)]
        public void OnActiveViewChanging(object sender, EventArgs<string> e)
        {
            if (e.Data == ViewNames.TlfIa)
            {
                _IsCurrentView = true;

                if ((_StateManager.Tlf.Transfer.State == TransferState.Idle) &&
                    (_StateManager.Tlf[TlfState.Set] + _StateManager.Tlf[TlfState.Conf] == 1))
                {
                    int id = _StateManager.Tlf.GetFirstInState(TlfState.Set, TlfState.Conf);
                    if (Tlf.IaMappedPosition == id)
                    {
                        _SetCall = _StateManager.Tlf[id];

                        // JCAM: 03/01/2017
                        // #2431
                        _Keypad.Dst = ""; // !string.IsNullOrEmpty(_SetCall.Dst) ? _SetCall.Dst : _SetCall.Number;
                        _Keypad.Digits = ""; // _SetCall.Digits;
                    }
                    else
                    {
                        _Keypad.Digits = "";
                        _Keypad.Dst = "";
                    }
                }
                else
                {
                    _SetCall = null;
                    _Keypad.Digits = "";
                    _Keypad.Dst = "";
                }


                if (_SetCall != null && _SetCall.State == TlfState.Set && !string.IsNullOrEmpty(_Keypad.Dst))  //FGB
                {   // al volver de AD a AI _StateManager.Tlf.Unhang.State vale Idle en vez de Set
                    // cuando se ha hecho una llamada por AI 
                    _Keypad.Enabled = false;
                    _CallBT.Enabled = true;
                    _SlowBlinkTimer.Enabled = true;
                    _SlowBlinkOn = true;
                    _CallTimestamp = Environment.TickCount;

                    _CallBT.ButtonColor = GetStateColor(_CallBT, UnhangState.Set);
                }
                else
                {
                    _Keypad.Enabled = _KeypadEnabled;
                    _CallBT.Enabled = _CallEnabled;
                    _CallBT.ButtonColor = GetStateColor(_CallBT, _StateManager.Tlf.Unhang.State);
                }

                _MemBT.Enabled = _MemEnabled;

                bool numEnabled = _NumEnabled;
                _Num1BT.Enabled = numEnabled && (_Historic[0] != null);
                _Num2BT.Enabled = numEnabled && (_Historic[1] != null);
                _Num3BT.Enabled = numEnabled && (_Historic[2] != null);
                _Num4BT.Enabled = numEnabled && (_Historic[3] != null);

                ShowAgenda(false);


            }
            else if (_IsCurrentView)
            {
                _IsCurrentView = false;
                _CallTimestamp = null;

                if (_StateManager.Tlf.Unhang.AssociatePosition != -1)
                {
                    _StateManager.Tlf.Unhang.Reset();

                    if (_SlowBlinkList.Remove(_CallBT) && (_SlowBlinkList.Count == 0))
                    {
                        _SlowBlinkTimer.Enabled = false;
                        _SlowBlinkOn = true;
                    }
                }
            }
        }

        [EventSubscription(EventTopicNames.TlfUnhangChanged, ThreadOption.Publisher)]
        public void OnTlfUnhangChanged(object sender, EventArgs e)
        {
            if (_IsCurrentView)
            {
                if (_StateManager.Tlf.Unhang.State == UnhangState.Idle)
                {
                    _Keypad.Digits = "";
                    _Keypad.Dst = "";
                }

                _Keypad.Enabled = _KeypadEnabled;
                _CallBT.Enabled = _CallEnabled;

                if (_SlowBlinkList.Remove(_CallBT) && (_SlowBlinkList.Count == 0))
                {
                    _SlowBlinkTimer.Enabled = false;
                    _SlowBlinkOn = true;
                }

                _CallBT.ButtonColor = GetStateColor(_CallBT, _StateManager.Tlf.Unhang.State);
            }
        }

        [EventSubscription(EventTopicNames.TlfPriorityChanged, ThreadOption.Publisher)]
        public void OnFacilityChanged(object sender, EventArgs e)
        {
            if (_IsCurrentView)
            {
                _Keypad.Enabled = _KeypadEnabled;
                _CallBT.Enabled = _CallEnabled;

                bool numEnabled = _NumEnabled;
                _Num1BT.Enabled = numEnabled && (_Historic[0] != null);
                _Num2BT.Enabled = numEnabled && (_Historic[1] != null);
                _Num3BT.Enabled = numEnabled && (_Historic[2] != null);
                _Num4BT.Enabled = numEnabled && (_Historic[3] != null);
            }
        }

        [EventSubscription(EventTopicNames.TlfListenChanged, ThreadOption.Publisher)]
        public void OnListenChanged(object sender, EventArgs e)
        {
            if (_IsCurrentView)
            {
                Debug.Assert(_SetCall == null);

                if (_StateManager.Tlf.Listen.State == ListenState.Idle)
                {
                    _Keypad.Digits = "";
                    _Keypad.Dst = "";
                }

                OnFacilityChanged(sender, e);
            }
        }

        // #2429
        /*
		[EventSubscription(EventTopicNames.TlfTransferChanged, ThreadOption.Publisher)]
		public void OnTransferChanged(object sender, EventArgs e)
		{
			if (_IsCurrentView)
			{
				if (_StateManager.Tlf.Transfer.State == TransferState.Idle)
				{
					if ((_StateManager.Tlf[TlfState.Set] + _StateManager.Tlf[TlfState.Conf]) == 1)
					{
						int id = _StateManager.Tlf.GetFirstInState(TlfState.Set, TlfState.Conf);

						_SetCall = _StateManager.Tlf[id];
						_Keypad.Dst = string.IsNullOrEmpty(_SetCall.Number) ? _SetCall.Dst : _SetCall.Number;
						_Keypad.Digits = _SetCall.Digits;
					}
					else
					{
						_Keypad.Digits = "";
						_Keypad.Dst = "";
					}
				}
				else if (_StateManager.Tlf.Transfer.State == TransferState.Ready)
				{
					_SetCall = null;
					_Keypad.Digits = "";
					_Keypad.Dst = "";
				}

				OnFacilityChanged(sender, e);
			}
		}
        */
        [EventSubscription(EventTopicNames.TlfChanged, ThreadOption.Publisher)]
        public void OnTlfChanged(object sender, RangeMsg e)
        {
            //if (_IsCurrentView)
            {
                if ((e.Count == 1) && (e.From < Tlf.NumDestinations) && !General.TimeElapsed(_CallTimestamp, 1000))
                {
                    bool changeView = false;
                    TlfDst dst = _StateManager.Tlf[e.From];

                    switch (dst.PrevState)
                    {
                        case TlfState.Unavailable:
                        case TlfState.Idle:
                        case TlfState.PaPBusy:
                        case TlfState.RemoteMem:
                        case TlfState.Mem:
                        case TlfState.NotAllowed:
                            changeView = (dst.State == TlfState.NotAllowed) || (dst.State == TlfState.Out) || (dst.State == TlfState.Congestion);
                            break;
                        case TlfState.Hold:
                        case TlfState.RemoteIn:
                        case TlfState.In:
                        case TlfState.InPrio:
                            changeView = (dst.State == TlfState.Set) || (dst.State == TlfState.Conf);
                            break;
                    }

                    if (changeView)
                    {
                        _CmdManager.SwitchTlfView(ViewNames.TlfDa);
                        _CmdManager.TlfLoadDaPage(e.From / ((Settings.Default.TlfRows * Settings.Default.TlfColumns) - 1));

                        return;
                    }
                }

                if ((_StateManager.Tlf.Listen.State == ListenState.Idle) &&
                    (_StateManager.Tlf.Transfer.State == TransferState.Idle))
                {
                    int numSet = _StateManager.Tlf[TlfState.Set] + _StateManager.Tlf[TlfState.Conf];

                    if (numSet == 1)
                    {
                        if ((_SetCall == null) || ((_SetCall.State != TlfState.Set) && (_SetCall.State != TlfState.Conf)))
                        {
                            int id = _StateManager.Tlf.GetFirstInState(TlfState.Set, TlfState.Conf);
                            _SetCall = _StateManager.Tlf[id];

                            _Keypad.Dst = string.Empty; // = !string.IsNullOrEmpty(_SetCall.Dst) ? _SetCall.Dst : _SetCall.Number;
                            _Keypad.Digits = _SetCall.Digits;
                        }
                    }
                    else if ((numSet == 0) && (_SetCall != null))
                    {
                        _SetCall = null;

                        if (_StateManager.Tlf.Unhang.State == UnhangState.Idle)
                        {
                            _Keypad.Digits = "";
                            _Keypad.Dst = "";
                        }
                    }

                    _Keypad.Enabled = _KeypadEnabled;
                    _CallBT.Enabled = _CallEnabled;
                }
            }

            if ((e.From + e.Count) > Tlf.IaMappedPosition)
            {
                TlfDst dst = _StateManager.Tlf[Tlf.IaMappedPosition];

                switch (dst.State)
                {
                    case TlfState.In:
                    case TlfState.InPrio:
                    case TlfState.RemoteIn:
                    case TlfState.Congestion:
                    case TlfState.Set:
                    case TlfState.Conf:
                    case TlfState.Out:
                    case TlfState.Busy:
                        if (!string.IsNullOrEmpty(dst.Number))
                        {
                            AddToHistoric(new Number(dst.Number, dst.Dst));
                        }
                        break;
                }

                if (_IsCurrentView)
                {
                    bool numEnabled = _NumEnabled;
                    _Num1BT.Enabled = numEnabled && (_Historic[0] != null);
                    _Num2BT.Enabled = numEnabled && (_Historic[1] != null);
                    _Num3BT.Enabled = numEnabled && (_Historic[2] != null);
                    _Num4BT.Enabled = numEnabled && (_Historic[3] != null);
                }
            }
        }

        [EventSubscription(EventTopicNames.AgendaChanged, ThreadOption.Publisher)]
        public void OnAgendaChanged(object sender, EventArgs e)
        {
            _Mem.Reset(_StateManager.Agenda.Numbers);
        }

        [EventSubscription(EventTopicNames.DependencesNumberCalled, ThreadOption.Publisher)]
        public void OnDependencesNumberCalled(object sender, EventArgs<string> e)
        {
            string num = "03" + e.Data;
            _CmdManager.SwitchTlfView(ViewNames.TlfIa);

            _CmdManager.TlfClick(num);

            _Keypad.Digits = num;
            _CallBT.Enabled = _CallEnabled;
            _CallTimestamp = Environment.TickCount;
        }

        private void ShowAgenda(bool show)
        {
            bool memShowed = _WorkItem.Workspaces[WorkspaceNames.IaToolsWorkspace].ActiveSmartPart == _Mem;
            bool keypadShowed = _WorkItem.Workspaces[WorkspaceNames.IaToolsWorkspace].ActiveSmartPart == _Keypad;

            if (show && !memShowed)
            {
                _Mem.Reset();
                _MemBT.ButtonColor = HMI.Presentation.Urr.UI.VisualStyle.Colors.Yellow;
                _WorkItem.Workspaces[WorkspaceNames.IaToolsWorkspace].Show(_Mem);
            }
            else if (!show && !keypadShowed)
            {
                _MemBT.ButtonColor = HMI.Presentation.Urr.UI.VisualStyle.ButtonColor;
                _WorkItem.Workspaces[WorkspaceNames.IaToolsWorkspace].Show(_Keypad);
            }
        }

        private Color GetStateColor(HMIButton bt, UnhangState st)
        {
            Color backColor = HMI.Presentation.Urr.UI.VisualStyle.ButtonColor;

            switch (st)
            {
                case UnhangState.Out:
                    backColor = HMI.Presentation.Urr.UI.VisualStyle.Colors.Blue;
                    break;
                case UnhangState.Set:
                case UnhangState.Conf:
                    backColor = HMI.Presentation.Urr.UI.VisualStyle.Colors.Green;
                    break;
                case UnhangState.Hold:
                case UnhangState.RemoteHold:
                    backColor = _SlowBlinkOn ? HMI.Presentation.Urr.UI.VisualStyle.Colors.Green : HMI.Presentation.Urr.UI.VisualStyle.ButtonColor;
                    _SlowBlinkList[bt] = HMI.Presentation.Urr.UI.VisualStyle.Colors.Green;
                    _SlowBlinkTimer.Enabled = true;
                    break;
                case UnhangState.Busy:
                    backColor = HMI.Presentation.Urr.UI.VisualStyle.Colors.Red;
                    break;
                case UnhangState.Congestion:
                case UnhangState.OutOfService:
                    backColor = _SlowBlinkOn ? HMI.Presentation.Urr.UI.VisualStyle.Colors.Red : HMI.Presentation.Urr.UI.VisualStyle.ButtonColor;
                    _SlowBlinkList[bt] = HMI.Presentation.Urr.UI.VisualStyle.Colors.Red;
                    _SlowBlinkTimer.Enabled = true;
                    break;
                case UnhangState.NotAllowed:
                    backColor = _SlowBlinkOn ? HMI.Presentation.Urr.UI.VisualStyle.Colors.Yellow : HMI.Presentation.Urr.UI.VisualStyle.ButtonColor;
                    _SlowBlinkList[bt] = HMI.Presentation.Urr.UI.VisualStyle.Colors.Yellow;
                    _SlowBlinkTimer.Enabled = true;
                    break;
            }

            return backColor;
        }

        private bool NumberEqualToHistoric(Number a, int hIndex)
        {
            Number b = _Historic[hIndex];

            return (b != null) && (a.Digits == b.Digits);
        }

        private void AddToHistoric(Number number)
        {
            if (NumberEqualToHistoric(number, 2))
            {
                _Historic[2] = _Historic[1];
                _Historic[1] = _Historic[0];
            }
            else if (NumberEqualToHistoric(number, 1))
            {
                _Historic[1] = _Historic[0];
            }
            else if (NumberEqualToHistoric(number, 0))
            {
            }
            else
            {
                _Historic[3] = _Historic[2];
                _Historic[2] = _Historic[1];
                _Historic[1] = _Historic[0];
            }

            _Historic[0] = number;

            _Num1BT.Text = _Historic[0] != null ? _Historic[0].Alias : "Num. 1";
            _Num2BT.Text = _Historic[1] != null ? _Historic[1].Alias : "Num. 2";
            _Num3BT.Text = _Historic[2] != null ? _Historic[2].Alias : "Num. 3";
            _Num4BT.Text = _Historic[3] != null ? _Historic[3].Alias : "Num. 4";

            Settings.Default.Historic.Clear();
            for (int i = 0; (i < _Historic.Length) && (_Historic[i] != null); i++)
            {
                Settings.Default.Historic.Add(_Historic[i].Digits + "," + _Historic[i].Alias);
            }
            Settings.Default.Save();
        }

        private void _SlowBlinkTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (_SlowBlinkTimer.Enabled)
                {
                    _SlowBlinkOn = !_SlowBlinkOn;
                    foreach (KeyValuePair<HMIButton, Color> p in _SlowBlinkList)
                    {
                        p.Key.ButtonColor = _SlowBlinkOn ? p.Value : HMI.Presentation.Urr.UI.VisualStyle.ButtonColor;
                    }
                }
            }
            catch (Exception ex)
            {
                _Logger.Error("ERROR generando parpadeo lento para teclas TlfAI", ex);
            }
        }

        private void _CallBT_Click(object sender, EventArgs e)
        {
            if ((_StateManager.Tlf.Unhang.State == UnhangState.Idle) && _Keypad.Enabled)
            {
                string dst = _Keypad.Display;

                try
                {
                    _CmdManager.TlfClick(_Keypad.Display);
                    _CallTimestamp = Environment.TickCount;
                }
                catch (Exception ex)
                {
                    _Logger.Error("ERROR llamando a " + dst, ex);
                }
            }
            else
            {
                try
                {
                    Debug.Assert(_StateManager.Tlf.Unhang.AssociatePosition == Tlf.IaMappedPosition);
                    _CmdManager.TlfClick(Tlf.IaMappedPosition);
                    _CallBT.ButtonColor = GetStateColor(_CallBT, UnhangState.Idle);
                }
                catch (Exception ex)
                {
                    _Logger.Error("ERROR pulsando tecla de llamada en pagina AI", ex);
                }
            }
        }

        private void _MemBT_Click(object sender, EventArgs e)
        {
            bool show = _MemBT.ButtonColor != HMI.Presentation.Urr.UI.VisualStyle.Colors.Yellow;

            try
            {
                ShowAgenda(show);
            }
            catch (Exception ex)
            {
                string msg = string.Format("ERROR {0} agenda tras pulsar boton Mem", show ? "mostrando" : "ocultando");
                _Logger.Error(msg, ex);
            }
        }

        private void _NumBT_Click(object sender, EventArgs e)
        {
            int id = (int)((HMIButton)sender).Tag;
            Debug.Assert(_Historic[id] != null);

            string num = _Historic[id].Digits;
            Debug.Assert(!string.IsNullOrEmpty(num));

            try
            {
                _CmdManager.TlfClick(num);

                //_Keypad.Digits = num;
                _Keypad.Digits = ((HMIButton)sender).Text;
                _CallBT.Enabled = _CallEnabled;
                _CallTimestamp = Environment.TickCount;
            }
            catch (Exception ex)
            {
                string msg = string.Format("ERROR llamando a historico [Historico={0}] [Num={1}]", id, num);
                _Logger.Error(msg, ex);
            }

            try
            {
                ShowAgenda(false);
            }
            catch (Exception ex)
            {
                _Logger.Error("ERROR ocultando agenda tras llamada a historico", ex);
            }
        }

        private void OnKeypadNewKey(object sender, char key)
        {
            /*
            if (_SetCall != null)
            {
                try
                {
                    _CmdManager.NewDigit(_SetCall.Id, key);
                }
                catch (Exception ex)
                {
                    string msg = string.Format("ERROR enviando digito a llamada [Tlf={0}] [Digito={1}]", _SetCall.Id, key);
                    _Logger.Error(msg, ex);
                }
            }
            else*/
            {
                try
                {
                    _CallBT.Enabled = _CallEnabled;
                }
                catch (Exception ex)
                {
                    _Logger.Error("ERROR tras pulsacion de digito", ex);
                }
            }
        }

        private void OnKeypadClear(object sender)
        {
            try
            {
                _CallBT.Enabled = _CallEnabled;
            }
            catch (Exception ex)
            {
                _Logger.Error("ERROR tras pulsacion de tecla de borrado", ex);
            }
        }

        private void OnMemOkClick(object sender, Number number)
        {
            string num = number.Digits;
            Debug.Assert(!string.IsNullOrEmpty(num));

            try
            {
                _CmdManager.TlfClick(num);

                _Keypad.Digits = num;
                _CallBT.Enabled = _CallEnabled;
                _CallTimestamp = Environment.TickCount;
            }
            catch (Exception ex)
            {
                string msg = string.Format("ERROR llamando a agenda [Num={0}]", num);
                _Logger.Error(msg, ex);
            }

            try
            {
                ShowAgenda(false);
            }
            catch (Exception ex)
            {
                _Logger.Error("ERROR ocultando agenda tras llamada a agenda", ex);
            }
        }

        private void OnMemCancelClick(object sender)
        {
            try
            {
                ShowAgenda(false);
            }
            catch (Exception ex)
            {
                _Logger.Error("ERROR ocultando agenda tras cancelar Mem", ex);
            }
        }
    }
}

