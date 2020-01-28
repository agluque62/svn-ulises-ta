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
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using Microsoft.Practices.CompositeUI;
using Microsoft.Practices.CompositeUI.SmartParts;
using Microsoft.Practices.CompositeUI.EventBroker;
using Microsoft.Practices.ObjectBuilder;
using HMI.Infrastructure.Interface;
using HMI.Model.Module.Services;
using HMI.Model.Module.Messages;
using HMI.Model.Module.UI;
using HMI.Model.Module.BusinessEntities;
using HMI.Presentation.Twr.Constants;
using HMI.Presentation.Twr.Properties;  // Miguel
using NLog;

namespace HMI.Presentation.Twr.Views
{
	[SmartPart]
	public partial class TlfFunctionsView : UserControl
	{
        // Miguel
        //private const string AD = "AD";
        //private const string AI = "AI";

		private static Logger _Logger = LogManager.GetCurrentClassLogger();

		private IModelCmdManagerService _CmdManager;
		private StateManagerService _StateManager;
		private Dictionary<HMIButton, Color> _SlowBlinkList;
		private Dictionary<HMIButton, Color> _FastBlinkList;
		private bool _SlowBlinkOn = true;
		private bool _FastBlinkOn = true;
        // Funci�n de telefon�a por altavoz habilitado seg�n configuraci�n teniendo en cuenta
        // el tipo de jack, con o sin auricular
        //private static bool _AltavozTlfHabilitado = Settings.Default.SpeakerTlfEnable && !Settings.Default.MicroMano;
        private Image _imagenTlfSpeakerBT;
        //Numero de p�gina de teclas de funci�n
        private int _FunctionsPage = Settings.Default.PageTlfFuntions;
		private bool _PriorityEnabled
		{
			get
			{
				return _StateManager.Tft.Enabled && _StateManager.Engine.Operative &&
					((_StateManager.Tlf.Priority.State != FunctionState.Idle) ||
					(((_StateManager.Permissions & Permissions.Priority) == Permissions.Priority) &&
					 _StateManager.Jacks.SomeJack &&
					(_StateManager.Tlf.Listen.State == FunctionState.Idle) &&
					(_StateManager.Tlf.Transfer.State == FunctionState.Idle) &&
                    (_StateManager.Tlf.PickUp.State == FunctionState.Idle) &&
					(_StateManager.Tlf[TlfState.Set] + _StateManager.Tlf[TlfState.Conf] +
					_StateManager.Tlf[TlfState.Out] + _StateManager.Tlf[TlfState.RemoteHold] == 0)));
			}
		}
        private bool _PickUpEnabled
        {
            get
            {
                return _StateManager.Tft.Enabled && _StateManager.Engine.Operative &&
                    ((_StateManager.Tlf.PickUp.State != FunctionState.Idle) ||
                    _StateManager.Jacks.SomeJack &&
                    (((_StateManager.Permissions & Permissions.Capture) == Permissions.Capture) &&
                    (_StateManager.Tlf.Priority.State == FunctionState.Idle) &&
                    (_StateManager.Tlf.Listen.State == FunctionState.Idle) &&
                    (_StateManager.Tlf.Transfer.State == FunctionState.Idle) &&
                    (_StateManager.Tlf.Forward.State == FunctionState.Idle) &&
                    (_StateManager.Tlf[TlfState.Congestion] + _StateManager.Tlf[TlfState.Busy] +
					_StateManager.Tlf[TlfState.Hold] + _StateManager.Tlf[TlfState.RemoteHold] +
					_StateManager.Tlf[TlfState.Set] + _StateManager.Tlf[TlfState.Conf] +
					_StateManager.Tlf[TlfState.Out] + _StateManager.Tlf[TlfState.RemoteIn] /*+
					_StateManager.Tlf[TlfState.In] + _StateManager.Tlf[TlfState.InPrio]*/ == 0)));
            }
        }
		private bool _ListenEnabled
		{
			get
			{
				return _StateManager.Tft.Enabled && _StateManager.Engine.Operative &&
					((_StateManager.Tlf.Listen.State != FunctionState.Idle) ||
					(((_StateManager.Permissions & Permissions.Listen) == Permissions.Listen) &&
					 _StateManager.Jacks.SomeJack &&
					(_StateManager.Tlf.Priority.State == FunctionState.Idle) &&
					(_StateManager.Tlf.Transfer.State == FunctionState.Idle) &&
                    (_StateManager.Tlf.PickUp.State == FunctionState.Idle) &&
                    (_StateManager.Tlf.Forward.State == FunctionState.Idle) &&
                    (_StateManager.Tlf[TlfState.Congestion] + _StateManager.Tlf[TlfState.Busy] +
					_StateManager.Tlf[TlfState.Hold] + _StateManager.Tlf[TlfState.RemoteHold] +
					_StateManager.Tlf[TlfState.Set] + _StateManager.Tlf[TlfState.Conf] +
					_StateManager.Tlf[TlfState.Out] + _StateManager.Tlf[TlfState.RemoteIn] +
					_StateManager.Tlf[TlfState.In] + _StateManager.Tlf[TlfState.InPrio] == 0)));
			}
		}
		private bool _HoldEnabled
		{
			get
			{
				return _StateManager.Tft.Enabled && _StateManager.Engine.Operative &&
					((_StateManager.Permissions & Permissions.Hold) == Permissions.Hold) &&
					 _StateManager.Jacks.SomeJack &&
					(_StateManager.Tlf.Priority.State == FunctionState.Idle) &&
					(_StateManager.Tlf.Listen.State == FunctionState.Idle) && 
					(_StateManager.Tlf.Transfer.State == FunctionState.Idle) &&
					(_StateManager.Tlf[TlfState.Set] + /* _StateManager.Tlf[TlfState.Conf] */ + _StateManager.Tlf[TlfState.RemoteHold] > 0) &&
                    (_StateManager.Tlf[TlfState.Conf] == 0);
			}
		}
		private bool _TransferEnabled
		{
			get
			{
				return _StateManager.Tft.Enabled && _StateManager.Engine.Operative &&
					((_StateManager.Tlf.Transfer.State != FunctionState.Idle) ||
					(((_StateManager.Permissions & Permissions.Transfer) == Permissions.Transfer) &&
					 _StateManager.Jacks.SomeJack &&
					(_StateManager.Tlf.Priority.State == FunctionState.Idle) &&
					(_StateManager.Tlf.Listen.State == FunctionState.Idle) &&
					(_StateManager.Tlf[TlfState.Set] /*+ _StateManager.Tlf[TlfState.Conf] */== 1) &&
                    //No se permiten transferencias si participa en una intrusi�n
                    (_StateManager.Tlf.IntrudedBy.By.Length == 0) &&
                    (_StateManager.Tlf.IntrudeTo.To.Length == 0)));
			}
		}
        private bool _ForwardEnabled
        {
            get
            {
                return _StateManager.Tft.Enabled && _StateManager.Engine.Operative &&
                    ((_StateManager.Tlf.Forward.State != FunctionState.Idle) ||
                    _StateManager.Jacks.SomeJack &&
                    (((_StateManager.Permissions & Permissions.Forward) == Permissions.Forward) &&
                    (_StateManager.Tlf.Priority.State == FunctionState.Idle) &&
                    (_StateManager.Tlf.Listen.State == FunctionState.Idle) &&
                    (_StateManager.Tlf.Transfer.State == FunctionState.Idle) &&
                    (_StateManager.Tlf.PickUp.State == FunctionState.Idle) &&
                    (_StateManager.Tlf[TlfState.Congestion] + _StateManager.Tlf[TlfState.Busy] +
                    _StateManager.Tlf[TlfState.Hold] + _StateManager.Tlf[TlfState.RemoteHold] +
                    _StateManager.Tlf[TlfState.Set] + _StateManager.Tlf[TlfState.Conf] +
                    _StateManager.Tlf[TlfState.Out] + _StateManager.Tlf[TlfState.RemoteIn] +
					_StateManager.Tlf[TlfState.In] + _StateManager.Tlf[TlfState.InPrio] == 0)));
            }
        }
        private bool _TlfViewEnabled
		{
			get 
			{
				return _StateManager.Tft.Enabled;
			}
		}
        private bool _MoreEnabled
        {
            get
            {
                return _StateManager.Tft.Enabled;
            }
        }
		private bool _CancelEnabled
		{
			get { return _StateManager.Tft.Enabled && _StateManager.Engine.Operative && (_StateManager.Tlf.Listen.State == FunctionState.Idle && !_StateManager.Tlf.ListenBy.IsListen); }
		}
        private bool _TlfSpeakerBtEnabled
        {
            get { return _StateManager.Tft.Enabled && _StateManager.Engine.Operative && _StateManager.Tlf.AltavozTlfHabilitado && _StateManager.LcSpeaker.Presencia && _StateManager.Jacks.SomeJack; }
        }
        private string _Prioridad   // Miguel
        {
            get { return Resources.Prioridad; }
        }
        private string _Retener // Miguel
        {
            get { return Resources.Retener; }
        }
        private string _Transferir // Miguel
        {
            get { return Resources.Transferir; }
        }
        private string _Escucha // Miguel
        {
            get { return Resources.Escucha; }
        }
        private string _More
        {
            get { return Resources.More; }
        }
        private string _PickUp
        {
            get { return Resources.PickUp; }
        }
        private string _Forward
        {
            get { return Resources.Forward; }
        }
        private string _AI // Miguel
        {
            get { return Resources.AI; }
        }
        private string _AD // Miguel
        {
            get { return Resources.AD; }
        }
        public Image ImagenTlfSpeakerBT
        {
            set { _imagenTlfSpeakerBT = value; }
            get { return _imagenTlfSpeakerBT; }
        }

		public TlfFunctionsView([ServiceDependency] IModelCmdManagerService cmdManager, [ServiceDependency] StateManagerService stateManager)
		{

			InitializeComponent();
            _CmdManager = cmdManager;
            _StateManager = stateManager;
            _SlowBlinkList = new Dictionary<HMIButton, Color>();
            _FastBlinkList = new Dictionary<HMIButton, Color>();

            // Si esta habilitado la funcion de telefonia por altavoz, hay que redimensionar el boton para anular
            // porque comparte espacio con el bot�n de selecci�n de audio de telefon�a.
            if (_StateManager.Tlf.AltavozTlfHabilitado)
            {
                this._CancelBT.Location = new System.Drawing.Point(336, 2);
                this._CancelBT.Margin = new System.Windows.Forms.Padding(2);
                this._CancelBT.Size = new System.Drawing.Size(91, 60);
                this._CancelBT.TabIndex = 5;
                this._TlfFunctionsTLP.SetRowSpan(this._CancelBT, 1);
                this._CancelBT.ImageNormal = global::HMI.Presentation.Twr.Properties.Resources.AnularPeq;                
            }

			_PriorityBT.Enabled = _PriorityEnabled;
			_ListenBT.Enabled = _ListenEnabled;
			_HoldBT.Enabled = _HoldEnabled;
			_TransferBT.Enabled = _TransferEnabled;
			_TlfViewBT.Enabled = _TlfViewEnabled;
			_CancelBT.Enabled = _CancelEnabled;
            _TlfSpeakerBT.Enabled = _TlfSpeakerBtEnabled;
            _MoreBT.Enabled = _MoreEnabled;
            _PickUpBT.Enabled = _PickUpEnabled;
            _ForwardBT.Enabled = _ForwardEnabled;

            this._TlfSpeakerBT.Visible = _StateManager.Tlf.AltavozTlfHabilitado;
            if (_StateManager.Tlf.AltavozTlfEstado)
                this._TlfSpeakerBT.ImageNormal = global::HMI.Presentation.Twr.Properties.Resources.SpeakerTlf;
            else
                this._TlfSpeakerBT.ImageNormal = global::HMI.Presentation.Twr.Properties.Resources.HeadPhonesTlf;

            // Miguel
            _PriorityBT.Text = _Prioridad;
            _ListenBT.Text = _Escucha;
            _TransferBT.Text = _Transferir;
            _HoldBT.Text = _Retener;
            _TlfViewBT.Text = _AI;
            _MoreBT.Text = _More;
            _PickUpBT.Text = _PickUp;
            _ForwardBT.Text = _Forward;
            if (_FunctionsPage == 1)
            {
                _FunctionsPage = 0;
                ChangeFunctionsTlfPage();
            }
		}

		[EventSubscription(EventTopicNames.TftEnabledChanged, ThreadOption.Publisher)]
		[EventSubscription(EventTopicNames.EngineStateChanged, ThreadOption.Publisher)]
		public void OnTftEngineChanged(object sender, EventArgs e)
		{
			_PriorityBT.Enabled = _PriorityEnabled;
			_ListenBT.Enabled = _ListenEnabled;
			_HoldBT.Enabled = _HoldEnabled;
			_TransferBT.Enabled = _TransferEnabled;
			_TlfViewBT.Enabled = _TlfViewEnabled;
			_CancelBT.Enabled = _CancelEnabled;
            _TlfSpeakerBT.Enabled = _TlfSpeakerBtEnabled;
            _PickUpBT.Enabled = _PickUpEnabled;
            _ForwardBT.Enabled = _ForwardEnabled;
            _MoreBT.Enabled = _MoreEnabled;
        }

		[EventSubscription(EventTopicNames.ActiveViewChanging, ThreadOption.Publisher)]
		public void OnActiveViewChanging(object sender, EventArgs<string> e)
		{
			if (e.Data == ViewNames.TlfDa)
			{
				_TlfViewBT.Text = _AI;
				ResetTlfViewBt(_AI);
			}
			else if (e.Data == ViewNames.TlfIa)
			{
				_TlfViewBT.Text = _AD;
				ResetTlfViewBt(_AD);
			}
		}

		[EventSubscription(EventTopicNames.JacksChanged, ThreadOption.Publisher)]
		public void OnJacksChanged(object sender, EventArgs e)
		{
			_PriorityBT.Enabled = _PriorityEnabled;
			_ListenBT.Enabled = _ListenEnabled;
			_HoldBT.Enabled = _HoldEnabled;
			_TransferBT.Enabled = _TransferEnabled;
            _PickUpBT.Enabled = _PickUpEnabled;
            _ForwardBT.Enabled = _ForwardEnabled;
            if (_TlfSpeakerBT.Enabled && !_TlfSpeakerBtEnabled && !_StateManager.Tlf.AltavozTlfEstado)
                try
                {
                    _CmdManager.SpeakerTlfClick();
                }
                catch (Exception ex)
                {
                    _Logger.Error("ERROR activando cascos", ex);
                }

            _TlfSpeakerBT.Enabled = _TlfSpeakerBtEnabled;

		}

		[EventSubscription(EventTopicNames.TlfPriorityChanged, ThreadOption.Publisher)]
		public void OnTlfPriorityChanged(object sender, EventArgs e)
		{
			_PriorityBT.Enabled = _PriorityEnabled;
			_ListenBT.Enabled = _ListenEnabled;
			_HoldBT.Enabled = _HoldEnabled;
			_TransferBT.Enabled = _TransferEnabled;
            _PickUpBT.Enabled = _PickUpEnabled;
            _ForwardBT.Enabled = _ForwardEnabled;
            ChangeColorMore();
			switch (_StateManager.Tlf.Priority.State)
			{
				case FunctionState.Idle:
					if (_SlowBlinkList.Remove(_PriorityBT) && (_SlowBlinkList.Count == 0))
					{
						_SlowBlinkTimer.Enabled = false;
						_SlowBlinkOn = true;
					}
					_PriorityBT.ButtonColor = VisualStyle.ButtonColor;
					break;
				case FunctionState.Ready:
					_PriorityBT.ButtonColor = _SlowBlinkOn ? VisualStyle.Colors.Yellow : VisualStyle.ButtonColor;
					_SlowBlinkList[_PriorityBT] = VisualStyle.Colors.Yellow;
					_SlowBlinkTimer.Enabled = true;
					break;
				case FunctionState.Error:
					if (_SlowBlinkList.Remove(_PriorityBT) && (_SlowBlinkList.Count == 0))
					{
						_SlowBlinkTimer.Enabled = false;
						_SlowBlinkOn = true;
					}
					_PriorityBT.ButtonColor = VisualStyle.Colors.Red;
					break;
			}
		}
        [EventSubscription(EventTopicNames.TlfPickUpChanged, ThreadOption.Publisher)]
        public void OnTlfPickUpChanged(object sender, EventArgs e)
        {
            _PriorityBT.Enabled = _PriorityEnabled;
            _ListenBT.Enabled = _ListenEnabled;
            _HoldBT.Enabled = _HoldEnabled;
            _TransferBT.Enabled = _TransferEnabled;
            _PickUpBT.Enabled = _PickUpEnabled;
            _ForwardBT.Enabled = _ForwardEnabled;
            ChangeColorMore();
            switch (_StateManager.Tlf.PickUp.State)
            {
                case FunctionState.Idle:
                    if (_SlowBlinkList.Remove(_PickUpBT) && (_SlowBlinkList.Count == 0))
                    {
                        _SlowBlinkTimer.Enabled = false;
                        _SlowBlinkOn = true;
                    }
                    _PickUpBT.ButtonColor = VisualStyle.ButtonColor;
                    break;
                case FunctionState.Ready:
                    _PickUpBT.ButtonColor = _SlowBlinkOn ? VisualStyle.Colors.Yellow : VisualStyle.ButtonColor;
                    _SlowBlinkList[_PickUpBT] = VisualStyle.Colors.Yellow;
                    _SlowBlinkTimer.Enabled = true;
                    break;
                case FunctionState.Executing:
                    if (_SlowBlinkList.Remove(_PickUpBT) && (_SlowBlinkList.Count == 0))
                    {
                        _SlowBlinkTimer.Enabled = false;
                        _SlowBlinkOn = true;
                    }
                    _PickUpBT.ButtonColor = VisualStyle.Colors.Yellow;
                    break;
                case FunctionState.Error:
                    if (_SlowBlinkList.Remove(_PickUpBT) && (_SlowBlinkList.Count == 0))
                    {
                        _SlowBlinkTimer.Enabled = false;
                        _SlowBlinkOn = true;
                    }
                    _PickUpBT.ButtonColor = VisualStyle.Colors.Red;
                    break;
            }
        }
		[EventSubscription(EventTopicNames.TlfListenChanged, ThreadOption.Publisher)]
        public void OnTlfListenChanged(object sender, EventArgs e)
		{
			_PriorityBT.Enabled = _PriorityEnabled;
			_ListenBT.Enabled = _ListenEnabled;
			_HoldBT.Enabled = _HoldEnabled;
			_TransferBT.Enabled = _TransferEnabled;
            _PickUpBT.Enabled = _PickUpEnabled;
            //La tecla cancel est� deshabilitada mientras se realiza una escucha, para evitar cancelar la escucha.
            //La escucha es una llamada que se cancela pulsando de nuevo la tecla escucha.
            _CancelBT.Enabled = _CancelEnabled;
            _TlfSpeakerBT.Enabled = _TlfSpeakerBtEnabled;
            _ForwardBT.Enabled = _ForwardEnabled;
            ChangeColorMore();
			switch (_StateManager.Tlf.Listen.State)
			{
				case FunctionState.Idle:
					if (_SlowBlinkList.Remove(_ListenBT) && (_SlowBlinkList.Count == 0))
					{
						_SlowBlinkTimer.Enabled = false;
						_SlowBlinkOn = true;
					}
					_ListenBT.ButtonColor = VisualStyle.ButtonColor;
					break;
				case FunctionState.Ready:
					_ListenBT.ButtonColor = _SlowBlinkOn ? VisualStyle.Colors.Yellow : VisualStyle.ButtonColor;
					_SlowBlinkList[_ListenBT] = VisualStyle.Colors.Yellow;
					_SlowBlinkTimer.Enabled = true;
					break;
				case FunctionState.Executing:
					if (_SlowBlinkList.Remove(_ListenBT) && (_SlowBlinkList.Count == 0))
					{
						_SlowBlinkTimer.Enabled = false;
						_SlowBlinkOn = true;
					}
					_ListenBT.ButtonColor = VisualStyle.Colors.Yellow;
					break;
				case FunctionState.Error:
					if (_SlowBlinkList.Remove(_ListenBT) && (_SlowBlinkList.Count == 0))
					{
						_SlowBlinkTimer.Enabled = false;
						_SlowBlinkOn = true;
					}
					_ListenBT.ButtonColor = VisualStyle.Colors.Red;
					break;
			}
		}

        [EventSubscription(EventTopicNames.TlfListenByChanged, ThreadOption.Publisher)]
        public void OnTlfListenByChanged(object sender, EventArgs e)
        {
            //La tecla cancel est� deshabilitada mientras se est� siendo escuchado para evitar colgar la
            //llamada de escucha. Este evento se recibe al inicio o fin de ser objeto de escucha
            _CancelBT.Enabled = _CancelEnabled;
        }

        [EventSubscription(EventTopicNames.TlfTransferChanged, ThreadOption.Publisher)]
		public void OnTlfTransferChanged(object sender, EventArgs e)
		{
			_PriorityBT.Enabled = _PriorityEnabled;
			_ListenBT.Enabled = _ListenEnabled;
			_HoldBT.Enabled = _HoldEnabled;
			_TransferBT.Enabled = _TransferEnabled;
            _PickUpBT.Enabled = _PickUpEnabled;
            _ForwardBT.Enabled = _ForwardEnabled;
            ChangeColorMore();
			switch (_StateManager.Tlf.Transfer.State)
			{
				case FunctionState.Idle:
					if (_SlowBlinkList.Remove(_TransferBT) && (_SlowBlinkList.Count == 0))
					{
						_SlowBlinkTimer.Enabled = false;
						_SlowBlinkOn = true;
					}
					_TransferBT.ButtonColor = VisualStyle.ButtonColor;
					break;
				case FunctionState.Ready:
				case FunctionState.Executing:
					_TransferBT.ButtonColor = _SlowBlinkOn ? VisualStyle.Colors.Yellow : VisualStyle.ButtonColor;
					_SlowBlinkList[_TransferBT] = VisualStyle.Colors.Yellow;
					_SlowBlinkTimer.Enabled = true;
					break;
				case FunctionState.Error:
					if (_SlowBlinkList.Remove(_TransferBT) && (_SlowBlinkList.Count == 0))
					{
						_SlowBlinkTimer.Enabled = false;
						_SlowBlinkOn = true;
					}
					_TransferBT.ButtonColor = VisualStyle.Colors.Red;
					break;
			}
		}

        [EventSubscription(EventTopicNames.TlfForwardChanged, ThreadOption.Publisher)]
        public void OnTlfForwardChanged(object sender, EventArgs e)
        {
            _PriorityBT.Enabled = _PriorityEnabled;
            _ListenBT.Enabled = _ListenEnabled;
            _HoldBT.Enabled = _HoldEnabled;
            _TransferBT.Enabled = _TransferEnabled;
            _PickUpBT.Enabled = _PickUpEnabled;
            _ForwardBT.Enabled = _ForwardEnabled;
            ChangeColorMore();
            switch (_StateManager.Tlf.Forward.State)
            {
                case FunctionState.Idle:
                    if (_SlowBlinkList.Remove(_ForwardBT) && (_SlowBlinkList.Count == 0))
                    {
                        _SlowBlinkTimer.Enabled = false;
                        _SlowBlinkOn = true;
                    }
                    _ForwardBT.ButtonColor = VisualStyle.ButtonColor;
                    break;
                case FunctionState.Ready:
                    _ForwardBT.ButtonColor = _SlowBlinkOn ? VisualStyle.Colors.Yellow : VisualStyle.ButtonColor;
                    _SlowBlinkList[_ForwardBT] = VisualStyle.Colors.Yellow;
                    _SlowBlinkTimer.Enabled = true;
                    break;
                case FunctionState.Executing:
                    if (_SlowBlinkList.Remove(_ForwardBT) && (_SlowBlinkList.Count == 0))
                    {
                        _SlowBlinkTimer.Enabled = false;
                        _SlowBlinkOn = true;
                    }
                    _ForwardBT.ButtonColor = VisualStyle.Colors.Yellow;
                    break;
                case FunctionState.Error:
                    if (_SlowBlinkList.Remove(_ForwardBT) && (_SlowBlinkList.Count == 0))
                    {
                        _SlowBlinkTimer.Enabled = false;
                        _SlowBlinkOn = true;
                    }
                    _ForwardBT.ButtonColor = VisualStyle.Colors.Red;
                    break;
            }
        }

        [EventSubscription(EventTopicNames.TlfHangToneChanged, ThreadOption.Publisher)]
		public void OnTlfHangToneChanged(object sender, EventArgs e)
		{
			_CancelBT.ButtonColor = _StateManager.Tlf.HangTone.On ? VisualStyle.Colors.Yellow : VisualStyle.ButtonColor;
		}

		[EventSubscription(EventTopicNames.PermissionsChanged, ThreadOption.Publisher)]
		public void OnPermissionsChanged(object sender, EventArgs e)
		{
			_PriorityBT.Enabled = _PriorityEnabled;
			_ListenBT.Enabled = _ListenEnabled;
			_HoldBT.Enabled = _HoldEnabled;
			_TransferBT.Enabled = _TransferEnabled;
            _PickUpBT.Enabled = _PickUpEnabled;
            _ForwardBT.Enabled = _ForwardEnabled;
		}

		[EventSubscription(EventTopicNames.TlfChanged, ThreadOption.Publisher)]
		public void OnTlfChanged(object sender, RangeMsg e)
		{
			_PriorityBT.Enabled = _PriorityEnabled;
			_HoldBT.Enabled = _HoldEnabled;
			_TransferBT.Enabled = _TransferEnabled;
			_ListenBT.Enabled = _ListenEnabled;
            _PickUpBT.Enabled = _PickUpEnabled;
            _ForwardBT.Enabled = _ForwardEnabled;
            _HoldBT.ButtonColor = _StateManager.Tlf[TlfState.Hold] == 0 ? VisualStyle.ButtonColor : VisualStyle.Colors.Yellow;

			ResetTlfViewBt(_AD);
		}

        /// <summary>
        /// Recibe evento para cambiar el bot�n de telefon�a por altavoz
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [EventSubscription(EventTopicNames.ChangeTlfSpeaker, ThreadOption.Publisher)]
        public void OnChangeTlfSpeaker(object sender, EventArgs<bool> e)
        {
            if (e.Data == true)
            {
                this._TlfSpeakerBT.ImageNormal = global::HMI.Presentation.Twr.Properties.Resources.SpeakerTlf;
            }
            else
            {
                this._TlfSpeakerBT.ImageNormal = global::HMI.Presentation.Twr.Properties.Resources.HeadPhonesTlf;
            }
        }
        /// <summary>
        /// Este evento llega cuando hay un cambio en la presencia del altavoz
        /// se usa habilitar o no el bot�n de seleci�n de altavoz de telefon�a
        /// y para conmutar automaticamente a cascos si es posible y se pierde el altavoz
        /// </summary>
        /// <param name="sender">no se usa</param>
        /// <param name="e">no se usa</param>
        [EventSubscription(EventTopicNames.SpeakerChanged, ThreadOption.Publisher)]
        public void OnSpeakerChanged(object sender, EventArgs e)
        {
            if (sender.GetType() != typeof(LcSpeaker))
                return;
            if (_TlfSpeakerBT.Enabled && !_TlfSpeakerBtEnabled && _StateManager.Tlf.AltavozTlfEstado )
                try
                {
                    _CmdManager.SpeakerTlfClick();
                }
                catch (Exception ex)
                {
                    _Logger.Error("ERROR activando cascos", ex);
                }

            _TlfSpeakerBT.Enabled = _TlfSpeakerBtEnabled;
        }
        [EventSubscription(EventTopicNames.TlfIntrudedByChanged, ThreadOption.Publisher)]
        public void OnTlfIntrudedByChanged(object sender, EventArgs e)
        {
            _TransferBT.Enabled = _TransferEnabled;
        }

        [EventSubscription(EventTopicNames.TlfIntrudeToChanged, ThreadOption.Publisher)]
        public void OnTlfIntrudeToChanged(object sender, EventArgs e)
        {
            _TransferBT.Enabled = _TransferEnabled;
        }

 		private void ResetTlfViewBt(string text)
		{
			if (_TlfViewBT.Text == text)
			{
				if (_FastBlinkList.Remove(_TlfViewBT) && (_FastBlinkList.Count == 0))
				{
					_FastBlinkTimer.Enabled = false;
					_FastBlinkOn = true;
				}
				if (_SlowBlinkList.Remove(_TlfViewBT) && (_SlowBlinkList.Count == 0))
				{
					_SlowBlinkTimer.Enabled = false;
					_SlowBlinkOn = true;
				}

				if (text == _AD)
				{
					TlfState st = _StateManager.Tlf.GetTlfState(0, Tlf.NumDestinations);

					if (_StateManager.Tlf.Unhang.AssociatePosition != Tlf.IaMappedPosition)
					{
						st = (TlfState)Math.Max((int)st, (int)_StateManager.Tlf[Tlf.IaMappedPosition].State);
					}

					_TlfViewBT.ButtonColor = GetStateColor(_TlfViewBT, st);
				}
				else
				{
					_TlfViewBT.ButtonColor = VisualStyle.ButtonColor;
				}
			}
		}

		private Color GetStateColor(HMIButton bt, TlfState st)
		{
			Color backColor = VisualStyle.ButtonColor;

			switch (st)
			{
				case TlfState.Idle:
				case TlfState.PaPBusy:
					break;
				case TlfState.In:
					backColor = _SlowBlinkOn ? VisualStyle.Colors.Orange : VisualStyle.ButtonColor;
					_SlowBlinkList[bt] = VisualStyle.Colors.Orange;
					_SlowBlinkTimer.Enabled = true;
					break;
				case TlfState.Out:
					backColor = VisualStyle.Colors.Blue;
					break;
				case TlfState.Set:
				case TlfState.Conf:
					backColor = VisualStyle.Colors.Green;
					break;
				case TlfState.Busy:
					backColor = VisualStyle.Colors.Red;
					break;
				case TlfState.Mem:
					backColor = VisualStyle.Colors.Orange;
					break;
				case TlfState.RemoteMem:
					backColor = VisualStyle.Colors.DarkGray;
					break;
				case TlfState.Hold:
				case TlfState.RemoteHold:
					backColor = _SlowBlinkOn ? VisualStyle.Colors.Green : VisualStyle.ButtonColor;
					_SlowBlinkList[bt] = VisualStyle.Colors.Green;
					_SlowBlinkTimer.Enabled = true;
					break;
				case TlfState.RemoteIn:
					backColor = _SlowBlinkOn ? VisualStyle.Colors.DarkGray : VisualStyle.ButtonColor;
					_SlowBlinkList[bt] = VisualStyle.Colors.DarkGray;
					_SlowBlinkTimer.Enabled = true;
					break;
				case TlfState.Congestion:
					backColor = _SlowBlinkOn ? VisualStyle.Colors.Red : VisualStyle.ButtonColor;
					_SlowBlinkList[bt] = VisualStyle.Colors.Red;
					_SlowBlinkTimer.Enabled = true;
					break;
				case TlfState.InPrio:
					backColor = _FastBlinkOn ? VisualStyle.Colors.Orange : VisualStyle.ButtonColor;
					_FastBlinkList[bt] = VisualStyle.Colors.Orange;
					_FastBlinkTimer.Enabled = true;
					break;
				case TlfState.NotAllowed:
					backColor = _FastBlinkOn ? VisualStyle.Colors.Yellow : VisualStyle.ButtonColor;
					_FastBlinkList[bt] = VisualStyle.Colors.Yellow;
					_FastBlinkTimer.Enabled = true;
					break;
                case TlfState.InProcess:
                    backColor = VisualStyle.Colors.Yellow;
                    break;
            }

			return backColor;
		}

		private void _PriorityBT_Click(object sender, EventArgs e)
		{
			try
			{
				_CmdManager.PriorityClick();
			}
			catch (Exception ex)
			{
				_Logger.Error("ERROR pulsando tecla de prioridad", ex);
			}
		}

		private void _ListenBT_Click(object sender, EventArgs e)
		{
			try
			{
				_CmdManager.ListenClick();
			}
			catch (Exception ex)
			{
				_Logger.Error("ERROR pulsando tecla de escucha", ex);
			}
		}

		private void _HoldBT_Click(object sender, EventArgs e)
		{
			try
			{
				_CmdManager.HoldClick();
			}
			catch (Exception ex)
			{
				_Logger.Error("ERROR pulsando tecla de aparcar", ex);
			}
		}

		private void _TransferBT_Click(object sender, EventArgs e)
		{
			try
			{
				_CmdManager.TransferClick();
			}
			catch (Exception ex)
			{
				_Logger.Error("ERROR pulsando tecla de transferencia", ex);
			}
		}

		private void _TlfViewBT_Click(object sender, EventArgs e)
		{
			try
			{
				_CmdManager.SwitchTlfView(null);
			}
			catch (Exception ex)
			{
				_Logger.Error("ERROR solicitando cambio de pagina AD <--> AI", ex);
			}
		}

		private void _CancelBT_Click(object sender, EventArgs e)
		{
			try
			{
				_CmdManager.CancelTlfClick();
			}
			catch (Exception ex)
			{
				_Logger.Error("ERROR pulsando tecla de anular", ex);
			}
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
						p.Key.ButtonColor = _SlowBlinkOn ? p.Value : VisualStyle.ButtonColor;
					}
				}
			}
			catch (Exception ex)
			{
				_Logger.Error("ERROR generando parpadeo lento para teclas de funcionalidad telefonica", ex);
			}
		}

		private void _FastBlinkTimer_Tick(object sender, EventArgs e)
		{
			try
			{
				if (_FastBlinkTimer.Enabled)
				{
					_FastBlinkOn = !_FastBlinkOn;
					foreach (KeyValuePair<HMIButton, Color> p in _FastBlinkList)
					{
						p.Key.ButtonColor = _FastBlinkOn ? p.Value : VisualStyle.ButtonColor;
					}
				}
			}
			catch (Exception ex)
			{
				_Logger.Error("ERROR generando parpadeo rapido para teclas de funcionalidad telefonica", ex);
			}
        }

        private void _SpeakerTlfBT_Click(object sender, EventArgs e)
        {
            try
            {
                _CmdManager.SpeakerTlfClick();
            }
            catch (Exception ex)
            {
                _Logger.Error("ERROR pulsando tecla modo altavoz telefon�a", ex);
            }
        }

        private void _MoreBT_Click(object sender, EventArgs e)
        {
            ChangeFunctionsTlfPage();

            Settings.Default.PageTlfFuntions = _FunctionsPage;
            Settings.Default.Save();
        }

        private void ChangeFunctionsTlfPage()
        {
            if (_FunctionsPage == 0)
            {
                this._TlfFunctionsTLP.Controls.Add(this._PriorityBT, 1, 2);
                this._TlfFunctionsTLP.Controls.Add(this._ListenBT, 0, 0);
                this._TlfFunctionsTLP.Controls.Add(this._HoldBT, 1, 3);
                this._TlfFunctionsTLP.Controls.Add(this._PickUpBT, 0, 1);
                this._TlfFunctionsTLP.Controls.Add(this._TransferBT, 1, 1);
                this._TlfFunctionsTLP.Controls.Add(this._ForwardBT, 1, 1);
                _FunctionsPage = 1;
            }
            else
            {
                this._TlfFunctionsTLP.Controls.Add(this._PriorityBT, 0, 0);
                this._TlfFunctionsTLP.Controls.Add(this._ListenBT, 1, 2);
                this._TlfFunctionsTLP.Controls.Add(this._HoldBT, 0, 1);
                this._TlfFunctionsTLP.Controls.Add(this._PickUpBT, 1, 3);
                this._TlfFunctionsTLP.Controls.Add(this._TransferBT, 1, 1);
                this._TlfFunctionsTLP.Controls.Add(this._ForwardBT, 2, 2);
                _FunctionsPage = 0;
            }
            //Visibles en pagina 0
            _PriorityBT.Visible = (_FunctionsPage == 0)  && (Settings.Default.EnablePriority);
            _HoldBT.Visible = (_FunctionsPage == 0) && (Settings.Default.EnableHold);
            _TransferBT.Visible = (_FunctionsPage == 0) && (Settings.Default.EnableTransfer);
            //Visibles en pagina 1
            _ListenBT.Visible = (_FunctionsPage == 1) && (Settings.Default.EnableListen);
            _PickUpBT.Visible = (_FunctionsPage == 1) && (Settings.Default.EnablePickUp);
            _ForwardBT.Visible = (_FunctionsPage == 1) && (Settings.Default.EnableForward);
            ChangeColorMore();
        }

        private void ChangeColorMore()
        {
            bool otherPageActive = ((_FunctionsPage == 0) &&
                ((_StateManager.Tlf.Listen.State != FunctionState.Idle) ||
                 (_StateManager.Tlf.PickUp.State != FunctionState.Idle) ||
                 (_StateManager.Tlf.Forward.State != FunctionState.Idle)));
            otherPageActive |= ((_FunctionsPage == 1) &&
                ((_StateManager.Tlf.Priority.State != FunctionState.Idle) ||
                 (_StateManager.Tlf.Transfer.State != FunctionState.Idle)));

            if (otherPageActive)
            {
                _MoreBT.ButtonColor = _SlowBlinkOn ? VisualStyle.Colors.Yellow : VisualStyle.ButtonColor;
                _SlowBlinkList[_MoreBT] = VisualStyle.Colors.Yellow;
                _SlowBlinkTimer.Enabled = true;
            }
            else
            {
                if (_SlowBlinkList.Remove(_MoreBT) && (_SlowBlinkList.Count == 0))
                {
                    _SlowBlinkTimer.Enabled = false;
                    _SlowBlinkOn = true;
                }
                _MoreBT.ButtonColor = VisualStyle.ButtonColor;
            }
        }

        private void _PickUpBT_Click(object sender, EventArgs e)
        {
            try
            {
                _CmdManager.PickUpClick();
            }
            catch (Exception ex)
            {
                _Logger.Error("ERROR pulsando tecla de captura", ex);
            }
        }

        private void _ForwardBT_Click(object sender, EventArgs e)
        {
            try
            {
                _CmdManager.ForwardClick();
            }
            catch (Exception ex)
            {
                _Logger.Error("ERROR pulsando tecla de captura", ex);
            }
        }
    }
}

