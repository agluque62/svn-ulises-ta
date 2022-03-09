using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.Diagnostics;
using Microsoft.Practices.ObjectBuilder;
using Microsoft.Practices.CompositeUI.EventBroker;
using HMI.Model.Module.Messages;
using HMI.Model.Module.Constants;
using HMI.Model.Module.Properties;
using Utilities;
using HMI.Infrastructure.Interface;

namespace HMI.Model.Module.BusinessEntities
{
	// Los valores están ordenados según prioridad
	// y empezando desde cero incrementandose de uno en uno (debido a _NumDstInState)
	public enum TlfState
	{
		UnChanged,
		Unavailable,
		Idle,
		PaPBusy,
		RemoteMem,
		Mem,
		NotAllowed,
		Hold,
		RemoteHold,
		Out,
		Set,
		Conf,
		Congestion,
		Busy,
		RemoteIn,
		In,
		InPrio,
		OutOfService, 
        Inactive,
        InProcess, //Telephony function in process (i.e. selected target for pickUp)
		offhook //#2855
	}

	public enum TlfType
	{
		Unknown,
		Ad,
		Ai,
        Md
	}

    public enum FunctionState
    {
        Idle,
        Ready,
        Executing,
        Error
    }

	public enum ConfState
	{
		Idle,
		Hold,
		Executing
	}

	public enum UnhangState
	{
		Idle = TlfState.Idle,
		RemoteHold = TlfState.RemoteHold,
		Hold = TlfState.Hold,
		Congestion = TlfState.Congestion,
		Busy = TlfState.Busy,
		Set = TlfState.Set,
		Conf = TlfState.Conf,
		Out = TlfState.Out,
		OutOfService = TlfState.OutOfService,
		NotAllowed = TlfState.NotAllowed,
		Descolgado = TlfState.offhook //#2855
	}

    /// <summary>
    /// Clase base que gestiona la parte de los mensajes de descripcion que usan
    /// las funciones de telefonía y los jacks
    /// </summary>
    public class Description
    {
        private string _StateDescription = "";
        private string _PreviusStateDescription = "";
        public string StateDescription
        {
            get { return _StateDescription; }
            set {
                _PreviusStateDescription = _StateDescription;
                if (value.Length > 0)
                    _StateDescription = value + Environment.NewLine;
                else
                    _StateDescription = value;
            }
        }

        public string PreviusStateDescription
        {
            get { return _PreviusStateDescription; }
            set
            {
                if (value.Length > 0)
                    _PreviusStateDescription = value + Environment.NewLine;
                else
                    _PreviusStateDescription = value;
            }
        }
        public void ResetDescription()
        {
            _PreviusStateDescription = _StateDescription;
            _StateDescription = "";
        }
    }

    public interface IPublishingState
    {
        event EventHandler StateChangeEvent;
        FunctionState State { get; set; }
        void Reset();
        void Reset(ListenPickUpMsg msg);
    }
	public sealed class TlfDst :Description
	{
		private int _Id;
		private string _Dst = "";
		private string _Number = "";
		private string _Digits = "";
		private TlfState _State = TlfState.Idle;
		private TlfState _PrevState = TlfState.Idle;
		private UiTimer _MemNotifTimer = null;
		private UiTimer _IaTimer = null;
        private bool _PriorityAllowed = true;
        private bool _IsTop = true;
        private bool _AllowsForward = false;
        private TlfType _Type = TlfType.Unknown;
		//LALM 211005 
		//#2629 Presentar via utilizada en llamada saliente.
		private string _recused = "";

		public event GenericEventHandler StChanged;

		public int Id
		{
			get { return _Id; }
		}

		public string Dst
		{
			get { return _Dst; }
		}

		public string Number
		{
			get { return _Number; }
		}

		public string Digits
		{
			get { return _Digits; }
			set { _Digits = value; }
		}

		public TlfState State
		{
			get { return _State; }
		}

		public TlfState PrevState
		{
			get { return _PrevState; }
		}

		public bool IsConfigurated
		{
			get { return _Dst != null && _Dst.Length > 0; }
		}

		public bool Unavailable
		{
			get { return (_State == TlfState.Unavailable); }
		}

        public bool PriorityAllowed
        {
            get { return _PriorityAllowed; }
        }
        public TlfType Type
        {
            get { return _Type; }
        }
        public bool IsTop
        {
            get { return _IsTop; }
        }

        public bool ForwardAllowed
        {
            get { return _AllowsForward; }
        }

		//LALM 211006
		//#2629 Presentar via utilizada en llamada saliente.
		public string recused
		{
			get { return _recused; }
			set
			{
				_recused = value;
			}
		}

		public TlfDst(int id)
		{
			_Id = id;

			if (Settings.Default.TlfMemNotifSg > 0)
			{
				_MemNotifTimer = new UiTimer(Settings.Default.TlfMemNotifSg * 1000);
				_MemNotifTimer.AutoReset = false;
				_MemNotifTimer.Elapsed += OnMemNotifTimerElapsed;
			}

			if ((id == Tlf.IaMappedPosition) && (Settings.Default.IaMemTimerSg > 0))
			{
				_IaTimer = new UiTimer(Settings.Default.IaMemTimerSg * 1000);
				_IaTimer.AutoReset = false;
				_IaTimer.Elapsed += OnIaTimerElapsed;
			}
		}

		public void Reset()
		{
			if (_MemNotifTimer != null)
			{
				_MemNotifTimer.Enabled = false;
			}
			if (_IaTimer != null)
			{
				_IaTimer.Enabled = false;
			}

			_Dst = "";
			_Number = "";
			_Digits = "";
			_PrevState = _State;
            ResetDescription();
			_State = TlfState.Idle;
            _Type = TlfType.Unknown;
            _IsTop = true;
            _AllowsForward = false;
        }

		public void Reset(TlfInfo dst)
		{
			if (dst.Dst == "")
			{
				Reset();
			}
			else if (dst.Dst != _Dst)
			{
				_Dst = dst.Dst;
                _PriorityAllowed = dst._PriorityAllowed;
                _Type = dst._Type;
                _IsTop = dst._IsTop;
                _AllowsForward = dst._AllowsForward;
                ChangeState(dst.St);
				ChangeStateDescription();
			}
			else
			{
				Reset(dst.St);
			}
			//lalm 211007
			//#2629 Presentar via utilizada en llamada saliente.
			this.recused = dst._recused;
		}

		public void Reset(TlfDestination dst)
		{
			if (dst.Dst == "")
			{
				Reset();
			}
			else if (dst.Dst != _Dst)
			{
				_Dst = dst.Dst;
				ChangeState(TlfState.Idle);
				ChangeStateDescription();
			}
			else
			{
				// Después de llamarse a esta función se va a notificar el cambio
				// por lo que hay que actualizar _PreviusStateDescription
				_PrevState = _State;
				PreviusStateDescription = StateDescription;
			}
		}

		public void Reset(TlfState st)
		{
			if ((st != TlfState.Idle) || 
				((_State != TlfState.Mem) && (_State != TlfState.RemoteMem)) ||
				(st==TlfState.Idle && _State==TlfState.NotAllowed && _PrevState==TlfState.NotAllowed))
			{
				ChangeState(st);
				ChangeStateDescription();
			}
			else
			{
				_PrevState = _State;
				PreviusStateDescription = StateDescription;
			}
		}

		public void Reset(TlfIaDestination dst)
		{
			Debug.Assert(_Id >= Tlf.NumDestinations);

			_Dst = dst.Alias;
			_Number = dst.Number;
			//lalm 211008
			//#2629 Presentar via utilizada en llamada saliente.
			_recused = dst._recused;

			Reset(dst.State);

			if (_IaTimer != null)
			{
				_IaTimer.Enabled = ((_Number != "") && ((_State == TlfState.Idle) ||
					(_State == TlfState.PaPBusy) || (_State == TlfState.Mem) || (_State == TlfState.NotAllowed) || (_State == TlfState.RemoteMem)));
			}
		}

		public void ResetMem()
		{
			if ((_State == TlfState.Mem) || (_State == TlfState.NotAllowed) || (_State == TlfState.RemoteMem))
			{
				ChangeState(TlfState.Idle);
				ChangeStateDescription();
			}
			else
			{
				_PrevState = _State;
				PreviusStateDescription = StateDescription;
			}
		}

		private void ChangeState(TlfState st)
		{
			_PrevState = _State;

			if ((st != TlfState.UnChanged) && (st != _State))
			{
				_State = st;

				switch (_State)
				{
					case TlfState.Unavailable:
					case TlfState.PaPBusy:
					case TlfState.Idle:
					case TlfState.Mem:
					case TlfState.RemoteMem:
					case TlfState.In:
					case TlfState.InPrio:
					case TlfState.RemoteIn:
					case TlfState.NotAllowed:
						_Digits = "";
						break;
				}

				if (_MemNotifTimer != null)
				{
					_MemNotifTimer.Enabled = (_State == TlfState.Mem) || (_State == TlfState.NotAllowed) || (_State == TlfState.RemoteMem);
				}
			}
		}

		private void ChangeStateDescription()
		{
			switch (_State)
			{
				case TlfState.Set:
				case TlfState.Conf:
                    if (_Type == TlfType.Md)
                        StateDescription = Resources.MultidestinationCall + " " + _Dst;
					else
					    StateDescription = Resources.TalkTlfStateDescription + " " + _Dst+recused;
					break;
				case TlfState.Hold:
					StateDescription = Resources.HoldToTlfStateDescription + " " + _Dst;
					break;
				case TlfState.RemoteHold:
					StateDescription = Resources.HoldByTlfStateDescription + " " + _Dst;
					break;
				//case TlfState.InPrio:
				//    _StateDescription = Resources.IntrudedByDescription + " " + _Dst;
				//    break;
				default:
					StateDescription = "";
					break;
			}
		}

		private void OnMemNotifTimerElapsed(object sender, ElapsedEventArgs e)
		{
			if ((_State == TlfState.Mem) || (_State == TlfState.NotAllowed) || (_State == TlfState.RemoteMem))
			{
				_PrevState = _State;
				PreviusStateDescription = StateDescription;

				_State = TlfState.Idle;

				General.SafeLaunchEvent(StChanged, this);
			}
		}

		private void OnIaTimerElapsed(object sender, ElapsedEventArgs e)
		{
			if ((_Number != "") && ((_State == TlfState.Idle) ||
				(_State == TlfState.PaPBusy) || (_State == TlfState.Mem) || (_State == TlfState.NotAllowed) || (_State == TlfState.RemoteMem)))
			{
				Reset();

				General.SafeLaunchEvent(StChanged, this);
			}
		}
	}

	public sealed class Priority
	{
		private int _AssociatePosition = -1;
		private FunctionState _State = FunctionState.Idle;

		[EventPublication(EventTopicNames.TlfPriorityChanged, PublicationScope.Global)]
		public event EventHandler TlfPriorityChanged;

		public FunctionState State
		{
			get { return _State; }
			private set
			{
				if (_State != value)
				{
					_State = value;
					General.SafeLaunchEvent(TlfPriorityChanged, this);
				}
			}
		}

		public int AssociatePosition
		{
			get { return _AssociatePosition; }
		}

		public void Reset()
		{
			_AssociatePosition = -1;
			State = FunctionState.Idle;
		}

		public void Reset(int associatePosition)
		{
			_AssociatePosition = associatePosition;
			State = FunctionState.Ready;
		}

		public void Reset(FunctionState st)
		{
			if ((_State == FunctionState.Ready) || (_State == FunctionState.Executing))
			{
				_AssociatePosition = -1;
				State = st;
			}
		}

		public bool NewCall(int id)
		{
			if ((_State == FunctionState.Ready) && (_AssociatePosition == -1))
			{
				_AssociatePosition = id;
				return true;
			}

			_AssociatePosition = -1;
			State = FunctionState.Idle;

			return false;
		}

        public void RedirectCall(int id)
        { 
            if ((_State == FunctionState.Executing) && (_AssociatePosition != -1))
                _AssociatePosition = id;
        }
		public void CheckTlfStChanged(TlfDst dst)
		{
			if (_AssociatePosition == dst.Id)
			{
				Debug.Assert((_State == FunctionState.Ready) || (_State == FunctionState.Executing));

				switch (dst.State)
				{
                    //Cambio el case Idle hacer que funcione el caso de desvío de una llamada con prioridad
                    //No entiendo para qué servía esto (B. Santamaria)
                    case TlfState.Idle:
                        break;
					case TlfState.Unavailable:
                    case TlfState.PaPBusy:
                        if (_State == FunctionState.Executing)
                        {
                            // Puede ocurrir que antes realizar la llamada por acceso indirecto
                            // tengamos que colgar una ya existente. De ahí la comprobación
                            _AssociatePosition = -1;
                            State = FunctionState.Error;
                        }
						break;
					case TlfState.Out:
						State = FunctionState.Executing;
						break;
					case TlfState.Congestion:
					case TlfState.OutOfService:
					case TlfState.Busy:
						_AssociatePosition = -1;
						State = FunctionState.Error;
						break;
					default:
						_AssociatePosition = -1;
						State = FunctionState.Idle;
						break;
				}
			}
			else if ((_State == FunctionState.Ready) && (_AssociatePosition >= Tlf.NumDestinations) && (dst.Id < Tlf.NumDestinations))
			{
				// Puede ocurrir que una llamada por acceso indirecto coincida con una tecla de acceso directo
				switch (dst.State)
				{
					case TlfState.Out:
						_AssociatePosition = dst.Id;
						State = FunctionState.Executing;
						break;
					case TlfState.Congestion:
					case TlfState.OutOfService:
					case TlfState.Busy:
						_AssociatePosition = -1;
						State = FunctionState.Error;
						break;
				}
			}
		}
	}

	public sealed class IntrudedBy :Description
	{
		private string _By = "";

		[EventPublication(EventTopicNames.TlfIntrudedByChanged, PublicationScope.Global)]
		public event EventHandler TlfIntrudedByChanged;

        public string By
        {
            get { return _By; }
        }

		public void Reset()
		{
			if (_By != "")
			{
				_By = "";
                ResetDescription();

				General.SafeLaunchEvent(TlfIntrudedByChanged, this);
			}
		}

		public void Reset(StateMsg<string> msg)
		{
			if (_By != msg.State)
			{
				_By = msg.State;
				StateDescription = _By.Length > 0 ? Resources.IntrudedByDescription + " " + _By : "";

				General.SafeLaunchEvent(TlfIntrudedByChanged, this);
			}
		}
	}

	public sealed class IntrudeTo : Description
	{
		private string _To = "";

		[EventPublication(EventTopicNames.TlfIntrudeToChanged, PublicationScope.Global)]
		public event EventHandler TlfIntrudeToChanged;

		public bool IsIntrudingTo
		{
			get { return (_To.Length > 0); }
		}

		public string To
		{
			get { return _To; }
		}

		public void Reset()
		{
			if (_To != "")
			{
				_To = "";
                ResetDescription();
				General.SafeLaunchEvent(TlfIntrudeToChanged, this);
			}
		}

		public void Reset(StateMsg<string> msg)
		{
			if (_To != msg.State)
			{
				_To = msg.State;
				StateDescription = _To.Length > 0 ? Resources.IntrudeToDescription + " " + _To : "";

				General.SafeLaunchEvent(TlfIntrudeToChanged, this);
			}
		}
	}

	public sealed class InterruptedBy :Description
	{
		private string _By = "";

		[EventPublication(EventTopicNames.TlfInterruptedByChanged, PublicationScope.Global)]
		public event EventHandler TlfInterruptedByChanged;

		public void Reset()
		{
			if (_By != "")
			{
				_By = "";
                ResetDescription();
				General.SafeLaunchEvent(TlfInterruptedByChanged, this);
			}
		}

		public void Reset(StateMsg<string> msg)
		{
			if (_By != msg.State)
			{
				_By = msg.State;
				StateDescription = _By.Length > 0 ? Resources.InterruptedByDescription + " " + _By : "";

				General.SafeLaunchEvent(TlfInterruptedByChanged, this);
			}
		}
	}

	public sealed class ListenBy :Description
	{
		private Dictionary<int, string> _By = new Dictionary<int, string>();

		[EventPublication(EventTopicNames.TlfListenByChanged, PublicationScope.Global)]
		public event EventHandler TlfListenByChanged;

		public bool IsListen
		{
			get { return _By.Count > 0; }
		}

		public void Reset()
		{
			Dictionary<int, string> listeners = new Dictionary<int, string>(_By);
			_By.Clear();

			foreach (string by in listeners.Values)
			{
                ResetDescription();
				General.SafeLaunchEvent(TlfListenByChanged, this);
			}
		}

		public void Reset(ListenPickUpMsg msg)
		{
			if (msg.State == FunctionState.Executing)
			{
				_By[msg.Id] = msg.Dst;
				StateDescription = Resources.ListenByStateDescription + " " + msg.Dst;

				General.SafeLaunchEvent(TlfListenByChanged, this);
			}
			else
			{
				Debug.Assert(msg.State == FunctionState.Idle);
				string by;

				if (_By.TryGetValue(msg.Id, out by))
				{
					_By.Remove(msg.Id);
                    ResetDescription();

					General.SafeLaunchEvent(TlfListenByChanged, this);
				}
			}
		}
	}

	public sealed class ConfList :Description
	{
		private List<string> _Participants = new List<string>();

		[EventPublication(EventTopicNames.TlfConfListChanged, PublicationScope.Global)]
		public event EventHandler TlfConfListChanged;

		public void Reset()
		{
			foreach (string p in _Participants)
			{
                ResetDescription();
				General.SafeLaunchEvent(TlfConfListChanged, this);
			}

			_Participants.Clear();
		}

		public void Reset(RangeMsg<string> msg)
		{
			List<string> newParticipants = new List<string>(msg.Info);
			_Participants.RemoveAll(delegate(string p)
			{
				if (!newParticipants.Contains(p))
				{
                    ResetDescription();
                    General.SafeLaunchEvent(TlfConfListChanged, this);
					return true;
				}

				return false;
			});
			newParticipants.ForEach(delegate(string p)
			{
				if (!_Participants.Contains(p))
				{
					_Participants.Add(p);

					StateDescription = Resources.TalkTlfStateDescription + " " + p;

					General.SafeLaunchEvent(TlfConfListChanged, this);
				}
			});
		}
	}

	public sealed class Listen: Description, IPublishingState
	{
		private FunctionState _State = FunctionState.Idle;

		[EventPublication(EventTopicNames.TlfListenChanged, PublicationScope.Global)]
        public event EventHandler StateChangeEvent;

		public FunctionState State
		{
			get { return _State; }
			set
			{
				if (value != _State)
				{
					_State = value;
                    General.SafeLaunchEvent(StateChangeEvent, this);
				}
			}
		}

		public void Reset()
		{
			if (_State != FunctionState.Idle)
			{
                ResetDescription();
                State = FunctionState.Idle;
			}
		}

		public void Reset(ListenPickUpMsg msg)
		{
			if (_State != msg.State)
			{
				StateDescription = msg.State == FunctionState.Executing ? Resources.ListenToStateDescription + " " + msg.Dst : "";
				State = msg.State;
			}
		}

    }
    public sealed class PickUp : Description, IPublishingState
	{
        private FunctionState _State = FunctionState.Idle;

        [EventPublication(EventTopicNames.TlfPickUpChanged, PublicationScope.Global)]
        public event EventHandler StateChangeEvent;

        public FunctionState State
        {
            get { return _State; }
            set
            {
                if (value != _State)
                {
                    _State = value;
                    //Envia evento a la vista para cambiar el color
                    General.SafeLaunchEvent(StateChangeEvent, this);
                }
            }
        }

		public void Reset()
		{
           if (_State != FunctionState.Idle)
			{
                ResetDescription();
                State = FunctionState.Idle;
			}
		}

		public void Reset(ListenPickUpMsg msg)
		{
			if (_State != msg.State)
			{
                StateDescription = (msg.State == FunctionState.Ready|| msg.State == FunctionState.Executing) ? 
                    Resources.PickUpDescription + " " + msg.Dst : "";
                State = msg.State;
			}
		}
	}
    public sealed class Forward : Description, IPublishingState
    {
        private FunctionState _State = FunctionState.Idle;

        [EventPublication(EventTopicNames.TlfForwardChanged, PublicationScope.Global)]
        public event EventHandler StateChangeEvent;

        public FunctionState State
        {
            get { return _State; }
            set
            {
                if (value != _State)
                {
                    _State = value;
                }
                //Envia evento a la vista para cambiar el color
                General.SafeLaunchEvent(StateChangeEvent, this);
            }
        }

        public void Reset()
        {
            if (_State != FunctionState.Idle)
            {
                ResetDescription();
                State = FunctionState.Idle;
            }
        }

        public void Reset(ListenPickUpMsg msg)
        {
            //if (_State != msg.State)
            {
                if (msg.State != FunctionState.Error)
                {
                    switch (msg.State)
                    {
                        case FunctionState.Executing:
                            if (string.IsNullOrEmpty(msg.OtherDst))
                                StateDescription =Resources.ForwardDescription + " " + msg.Dst;
                            else
                                StateDescription = Resources.ForwardDescription + " " + msg.OtherDst + " " +
                                    Resources.From + " " + msg.Dst;
                            break;
                        case FunctionState.Idle:
                        case FunctionState.Ready:
                            if (!StateDescription.Contains(Resources.RemoteForwardDescription))
                                StateDescription = "";
                            break;
                    }
                }
                State = msg.State;
            }
        }
        public void Reset(string remoteName)
        {
            if (!String.IsNullOrEmpty(remoteName))
                StateDescription = Resources.RemoteForwardDescription + " " + remoteName;
            else
                ResetDescription();
            //Para publicar el mensaje del desvio remoto
            General.SafeLaunchEvent(StateChangeEvent, this);
        }
    }

    public sealed class Transfer
	{
		private bool _Direct = false;
		private FunctionState _State = FunctionState.Idle;

		[EventPublication(EventTopicNames.TlfTransferChanged, PublicationScope.Global)]
		public event EventHandler TlfTransferChanged;

		public FunctionState State
		{
			get { return _State; }
			set
			{
				if (_State != value)
				{
					_State = value;
					General.SafeLaunchEvent(TlfTransferChanged, this);
				}
			}
		}

		public bool Direct
		{
			get { return _Direct; }
			set { _Direct = value; }
		}

		public void Reset()
		{
			State = FunctionState.Idle;
		}

		public void Reset(StateMsg<FunctionState> msg)
		{
			State = msg.State;
		}
	}

	public sealed class HangTone
	{
		private bool _On = false;

		[EventPublication(EventTopicNames.TlfHangToneChanged, PublicationScope.Global)]
		public event EventHandler TlfHangToneChanged;

		public bool On
		{
			get { return _On; }
			set
			{
				if (_On != value)
				{
					_On = value;
					General.SafeLaunchEvent(TlfHangToneChanged, this);
				}
			}
		}

		public void Reset()
		{
			On = false;
		}

		public void Reset(StateMsg<bool> msg)
		{
			On = msg.State;
		}
	}

	public sealed class Unhang
	{
		private int _AssociatePosition = -1;
		private UnhangState _State = UnhangState.Idle;

		[EventPublication(EventTopicNames.TlfUnhangChanged, PublicationScope.Global)]
		public event EventHandler TlfUnhangChanged;

		public int AssociatePosition
		{
			get { return _AssociatePosition; }
		}

		public UnhangState State
		{
			get { return _State; }
			private set
			{
				if (_State != value)
				{
					_State = value;
					General.SafeLaunchEvent(TlfUnhangChanged, this);
				}
			}
		}

		//#2855
		public void Descuelga(int id)
		{
			State = UnhangState.Descolgado;
			this._AssociatePosition = id;
		}

		public void Cuelga()
		{
			State = UnhangState.Idle;
			this._AssociatePosition = -1;
		}

		public void Reset()
		{
			_AssociatePosition = -1;
			State = UnhangState.Idle;
		}

		public void NewCall(bool ia)
		{
			_AssociatePosition = ia ? Tlf.IaMappedPosition : -1;
			State = UnhangState.Idle;
		}

		public void CheckTlfStChanged(TlfDst dst)
		{
			if (_AssociatePosition == dst.Id)
			{
				switch (dst.State)
				{
					case TlfState.Unavailable:
					case TlfState.Idle:
					case TlfState.PaPBusy:
						if (_State != UnhangState.Idle)
						{
							// Puede ocurrir que antes realizar la llamada por acceso indirecto
							// tengamos que colgar una ya existente. De ahí la comprobación
							_AssociatePosition = -1;
							State = UnhangState.Idle;
						}
						break;
					case TlfState.Out:
						State = UnhangState.Out;
						break;
					case TlfState.Set:
						State = UnhangState.Set;
						break;
					case TlfState.Conf:
						State = UnhangState.Conf;
						break;
					case TlfState.Busy:
						State = UnhangState.Busy;
						break;
					case TlfState.RemoteHold:
						State = UnhangState.RemoteHold;
						break;
					case TlfState.Hold:
						State = UnhangState.Hold;
						break;
					case TlfState.Congestion:
						State = UnhangState.Congestion;
						break;
					case TlfState.OutOfService:
						State = UnhangState.OutOfService;
						break;
					case TlfState.NotAllowed:
						State = UnhangState.NotAllowed;
						break;
					default:
						_AssociatePosition = -1;
						State = UnhangState.Idle;
						break;
				}
			}
		}
	}

	public sealed class Tlf
	{
        //Peticiones #3638
		public static int NumIaDestinations = 1;//LALM 210923 , anulada, solo puede existir una lina de AI.
		public static int NumDestinations = Settings.Default.NumTlfDestinations;
		public static int IaMappedPosition = NumDestinations;

		private TlfDst[] _Dst = new TlfDst[NumDestinations + NumIaDestinations];
		private int[] _NumDstInState = new int[Enum.GetValues(typeof(TlfState)).Length];
		private Priority _Priority;
		private IntrudedBy _IntrudedBy;
		private InterruptedBy _InterruptedBy;
		private IntrudeTo _IntrudeTo;
		private Listen _Listen;
        private PickUp _PickUp;
        private Forward _Forward;
        private ListenBy _ListenBy;
		private Transfer _Transfer;
		private HangTone _HangTone;
		private Unhang _Unhang;
		private ConfList _ConfList;
        private bool _AltavozTlfEstado = Settings.Default.TlfSpeaker;
        private bool _AltavozTlfEnable = Settings.Default.SpeakerTlfEnable && !Settings.Default.OnlySpeakerMode;
        private bool _SoloAltavoces = Settings.Default.OnlySpeakerMode;

		[EventPublication(EventTopicNames.TlfChanged, PublicationScope.Global)]
		public event EventHandler<RangeMsg> TlfChanged;

        [EventPublication(EventTopicNames.ChangeTlfSpeaker, PublicationScope.Global)]
        public event EventHandler<EventArgs<bool>> ChangeTlfSpeaker;

		[CreateNew]
		public Priority Priority
		{
			get { return _Priority; }
			set { _Priority = value; }
		}

		[CreateNew]
		public IntrudedBy IntrudedBy
		{
			get { return _IntrudedBy; }
			set { _IntrudedBy = value; }
		}

		[CreateNew]
		public InterruptedBy InterruptedBy
		{
			get { return _InterruptedBy; }
			set { _InterruptedBy = value; }
		}

		[CreateNew]
		public IntrudeTo IntrudeTo
		{
			get { return _IntrudeTo; }
			set { _IntrudeTo = value; }
		}

		[CreateNew]
		public Listen Listen
		{
			get { return _Listen; }
			set { _Listen = value; }
		}

		[CreateNew]
		public ListenBy ListenBy
		{
			get { return _ListenBy; }
			set { _ListenBy = value; }
		}

        [CreateNew]
        public PickUp PickUp
        {
            get { return _PickUp; }
            set { _PickUp = value; }
        }

        [CreateNew]
        public Forward Forward
        {
            get { return _Forward; }
            set { _Forward = value; }
        }

        [CreateNew]
		public Transfer Transfer
		{
			get { return _Transfer; }
			set { _Transfer = value; }
		}

		[CreateNew]
		public HangTone HangTone
		{
			get { return _HangTone; }
			set { _HangTone = value; }
		}

		[CreateNew]
		public Unhang Unhang
		{
			get { return _Unhang; }
			set { _Unhang = value; }
		}

		[CreateNew]
		public ConfList ConfList
		{
			get { return _ConfList; }
			set { _ConfList = value; }
		}

		public TlfDst this[int i]
		{
			get { return _Dst[i]; }
		}

		public int this[TlfState st]
		{
			get { return _NumDstInState[(int)st]; }
		}

        public bool AltavozTlfEstado
        {
            get { return _AltavozTlfEstado; }
            set {
                bool oldState = _AltavozTlfEstado;

                _AltavozTlfEstado = value;
                Settings.Default.TlfSpeaker = value;
                Settings.Default.Save();
                if (oldState != _AltavozTlfEstado)
                    General.SafeLaunchEvent(ChangeTlfSpeaker, this, new EventArgs<bool>(_AltavozTlfEstado));
            }
        }

        public bool AltavozTlfHabilitado
        {
            get { return _AltavozTlfEnable; }
        }

        public bool SoloAltavoces
        {
            get { return _SoloAltavoces; }
        }

        public Tlf()
		{
			for (int i = 0; i < NumDestinations + NumIaDestinations; i++)
			{
				_Dst[i] = new TlfDst(i);
				_Dst[i].StChanged += OnTlfStChanged;
			}

            if (SoloAltavoces)
                _AltavozTlfEstado = true;
            else if (AltavozTlfHabilitado)
                // Se inicializa con el estado guardado 
                _AltavozTlfEstado = Settings.Default.TlfSpeaker;
            else
                _AltavozTlfEstado = false;
			_NumDstInState[(int)TlfState.Idle] = NumDestinations + NumIaDestinations;
		}

		public void Reset()
		{
			for (int i = 0; i < _NumDstInState.Length; i++)
			{
				_NumDstInState[i] = 0;
			}
			_NumDstInState[(int)TlfState.Idle] = NumDestinations + NumIaDestinations;

			_Priority.Reset();
			_IntrudedBy.Reset();
			_IntrudeTo.Reset();
			_InterruptedBy.Reset();
			_Listen.Reset();
			_ListenBy.Reset();
			_Transfer.Reset();
			_HangTone.Reset();
			_Unhang.Reset();
			_ConfList.Reset();
            _PickUp.Reset();

			for (int i = 0; i < NumDestinations + NumIaDestinations; i++)
			{
				_Dst[i].Reset();
			}

			General.SafeLaunchEvent(TlfChanged, this, new RangeMsg(0, NumDestinations + NumIaDestinations));
		}

		public void Reset(RangeMsg<TlfInfo> msg)
		{
			Debug.Assert(msg.From + msg.Count <= NumDestinations);

			for (int i = 0; i < msg.Count; i++)
			{
				TlfDst dst = _Dst[i + msg.From];
				dst.Reset(msg.Info[i]);

				if (dst.PrevState != dst.State)
				{
					_NumDstInState[(int)dst.PrevState]--;
					_NumDstInState[(int)dst.State]++;

					Debug.Assert(_NumDstInState[(int)dst.PrevState] >= 0);
					Debug.Assert(_NumDstInState[(int)dst.State] >= 0);

					_Priority.CheckTlfStChanged(dst);
				}
			}

			General.SafeLaunchEvent(TlfChanged, this, (RangeMsg)msg);

			if ((_Transfer.State == FunctionState.Ready) && 
				(_NumDstInState[(int)TlfState.Set] + _NumDstInState[(int)TlfState.Conf] != 1))
			{
				_Transfer.State = FunctionState.Idle;
			}
		}

		public void Reset(RangeMsg<TlfDestination> msg)
		{
			Debug.Assert(msg.From + msg.Count <= NumDestinations);

			for (int i = 0; i < msg.Count; i++)
			{
				TlfDst dst = _Dst[i + msg.From];
				dst.Reset(msg.Info[i]);

				if (dst.PrevState != dst.State)
				{
					_NumDstInState[(int)dst.PrevState]--;
					_NumDstInState[(int)dst.State]++;

					Debug.Assert(_NumDstInState[(int)dst.PrevState] >= 0);
					Debug.Assert(_NumDstInState[(int)dst.State] >= 0);

					_Priority.CheckTlfStChanged(dst);
				}
			}

			General.SafeLaunchEvent(TlfChanged, this, (RangeMsg)msg);

			if ((_Transfer.State == FunctionState.Ready) &&
				(_NumDstInState[(int)TlfState.Set] + _NumDstInState[(int)TlfState.Conf] != 1))
			{
				_Transfer.State = FunctionState.Idle;
			}
		}

		public void Reset(RangeMsg<TlfState> msg)
		{
			for (int i = 0; i < msg.Count; i++)
			{
				TlfDst dst = _Dst[i + msg.From];
				dst.Reset(msg.Info[i]);


				if (msg.Info[i] == TlfState.Idle && IntrudeTo.IsIntrudingTo && IntrudeTo.To == dst.Dst)
				{
					IntrudeTo.Reset();
				}

				if (i + msg.From == IaMappedPosition)
					_Unhang.CheckTlfStChanged(dst);

				if (dst.PrevState != dst.State)
				{
					_NumDstInState[(int)dst.PrevState]--;
					_NumDstInState[(int)dst.State]++;

					Debug.Assert(_NumDstInState[(int)dst.PrevState] >= 0);
					Debug.Assert(_NumDstInState[(int)dst.State] >= 0);

					_Priority.CheckTlfStChanged(dst);
				}
			}

			General.SafeLaunchEvent(TlfChanged, this, (RangeMsg)msg);

			if ((_Transfer.State == FunctionState.Ready) &&
				(_NumDstInState[(int)TlfState.Set] + _NumDstInState[(int)TlfState.Conf] != 1))
			{
				_Transfer.State = FunctionState.Idle;
			}
		}

		public void Reset(RangeMsg<TlfIaDestination> msg)
		{
			Debug.Assert((msg.From >= NumDestinations) && (msg.From + msg.Count <= NumDestinations + NumIaDestinations));

			for (int i = 0; i < msg.Count; i++)
			{
				TlfIaDestination info = msg.Info[i];
				TlfDst dst = _Dst[i + msg.From];

				dst.Reset(info);

                // 29112016. JCAM.  Poder intruir una llamada prioritaria desde AI
                if (info.State ==  TlfState.Idle && IntrudeTo.IsIntrudingTo && IntrudeTo.To == dst.Dst)
                {
                    IntrudeTo.Reset();
                }

				if (dst.PrevState != dst.State)
				{
					_NumDstInState[(int)dst.PrevState]--;
					_NumDstInState[(int)dst.State]++;

					Debug.Assert(_NumDstInState[(int)dst.PrevState] >= 0);
					Debug.Assert(_NumDstInState[(int)dst.State] >= 0);

					_Unhang.CheckTlfStChanged(dst);
					_Priority.CheckTlfStChanged(dst);
				}
			}

			General.SafeLaunchEvent(TlfChanged, this, (RangeMsg)msg);

			if ((_Transfer.State == FunctionState.Ready) &&
				(_NumDstInState[(int)TlfState.Set] + _NumDstInState[(int)TlfState.Conf] != 1))
			{
				_Transfer.State = FunctionState.Idle;
			}
		}

		public void ResetMem(int id)
		{
			Debug.Assert(id < NumDestinations + NumIaDestinations);

			TlfDst dst = _Dst[id];
			dst.ResetMem();

			if (dst.PrevState != dst.State)
			{
				_NumDstInState[(int)dst.PrevState]--;
				_NumDstInState[(int)dst.State]++;

				Debug.Assert(_NumDstInState[(int)dst.PrevState] >= 0);
				Debug.Assert(_NumDstInState[(int)dst.State] >= 0);

				if (id == IaMappedPosition)
					_Unhang.CheckTlfStChanged(dst);

				General.SafeLaunchEvent(TlfChanged, this, new RangeMsg(id, 1));
			}
		}

		public TlfState GetTlfState(int from, int count)
		{
			Debug.Assert(from + count <= NumDestinations + NumIaDestinations);
			TlfState st = TlfState.Idle;

			if ((from == 0) && (count == NumDestinations + NumIaDestinations))
			{
				for (int i = _NumDstInState.Length - 1; i >= 0; i--)
				{
					if (_NumDstInState[i] > 0)
					{
						st = (TlfState)i;
						break;
					}
				}
			}
			else
			{
				for (int i = 0; i < count; i++)
				{
					TlfDst dst = _Dst[i + from];
					st = (TlfState)Math.Max((int)st, (int)dst.State);
				}
			}

			return st;
		}

		public int GetFirstInState(params TlfState[] st)
		{
			for (int i = 0; i < NumDestinations + NumIaDestinations; i++)
			{
				if (Array.IndexOf(st, _Dst[i].State) >= 0)
				{
					return i;
				}
			}

			return -1;
		}

		public static bool ValidateNumber(string number)
		{
			string[] atsDigit = Settings.Default.ATSNetFirstDigit.Split(',');

            if (number.Length < 2)
                return false;

            string foundAtsDigit = Array.Find(atsDigit, s => s.Equals(number[0].ToString()));


            //if ((number.Length < 2) ||
            //    ((atsDigit != '0') && (number[0] == atsDigit) && (number.Length != 6)) ||
            //    (number.StartsWith("03") && (number.Length != 8)))
            if (((foundAtsDigit != null) && (number.Length != 6)) ||
                (number.StartsWith("03") && (number.Length != 8)))
            {
				return false;
			}

			return true;
		}

		public static string NumberToEngine(string number)
		{
			string[] atsDigit = Settings.Default.ATSNetFirstDigit.Split(',');
			Debug.Assert(number.Length >= 2);

            string foundAtsDigit = Array.Find(atsDigit, s => s.Equals(number[0].ToString()));

            if ((foundAtsDigit != null) && (number.Length == 6))
			{
                return "03" + number;
                //return foundAtsDigit + number;
            }

			return number;
		}

		public static string NumberToPresentation(string number)
		{
			ulong num;
			if ((number.Length == 8) && number.StartsWith("03") && ulong.TryParse(number, out num))
			{
				return number.Substring(2);
			}
            else if (number.StartsWith("02"))
            {
                return number.Split('@')[0];
            }

			return number;
		}

		private void OnTlfStChanged(object sender)
		{
			TlfDst dst = (TlfDst)sender;

			_NumDstInState[(int)dst.PrevState]--;
			_NumDstInState[(int)dst.State]++;

			General.SafeLaunchEvent(TlfChanged, this, new RangeMsg(dst.Id, 1));
		}
	}
}
