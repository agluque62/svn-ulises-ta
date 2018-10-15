using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.Diagnostics;
using Microsoft.Practices.CompositeUI.EventBroker;
using HMI.Model.Module.Messages;
using HMI.Model.Module.Constants;
using HMI.Model.Module.Properties;
using Utilities;

namespace HMI.Model.Module.BusinessEntities
{
	public enum LcRxState
	{
		Idle,
		Rx,
		RxNotif,
		Mem,
		Unavailable
	}

	public enum LcTxState
	{
		Idle,
		Out,
		Tx,
		Congestion,
		Busy,
		Unavailable
	}

	public sealed class LcDst
	{
		private int _Id;
		private string _Dst = "";
		private LcRxState _Rx = LcRxState.Idle;
		private LcTxState _Tx = LcTxState.Idle;
        private int _Group = 0;//17_01_13
		private UiTimer _RxNotifTimer = null;
		private UiTimer _MemNotifTimer = null;

		public event GenericEventHandler StChanged;

		public int Id
		{
			get { return _Id; }
		}

		public string Dst
		{
			get { return _Dst; }
		}

		public LcRxState Rx
		{
			get { return _Rx; }
		}

		public LcTxState Tx
		{
			get { return _Tx; }
		}

		public int Group
		{
			get { return _Group; }
		}

		public bool IsConfigurated
		{
			get { return _Dst.Length > 0; }
		}

		public bool Unavailable
		{
			get { return (_Rx == LcRxState.Unavailable) || (_Tx == LcTxState.Unavailable); }
		}

		public LcDst(int id)
		{
			_Id = id;

			_RxNotifTimer = new UiTimer(Settings.Default.LcRxNotifSg * 1000);
			_RxNotifTimer.AutoReset = false;
			_RxNotifTimer.Elapsed += OnRxNotifTimerElapsed;

			if (Settings.Default.LcMemNotifSg > 0)
			{
				_MemNotifTimer = new UiTimer(Settings.Default.LcMemNotifSg * 1000);
				_MemNotifTimer.AutoReset = false;
				_MemNotifTimer.Elapsed += OnMemNotifTimerElapsed;
			}
		}

		public void Reset()
		{
			_RxNotifTimer.Enabled = false;
			if (_MemNotifTimer != null)
			{
				_MemNotifTimer.Enabled = false;
			}

			_Dst = "";
			_Rx = LcRxState.Idle;
			_Tx = LcTxState.Idle;
            _Group = 0; //17_01_13
		}

		public void Reset(LcInfo dst)
		{
			if (dst.Dst == "")
			{
				Reset();
			}
			else if (dst.Dst != _Dst)
			{
				_Dst = dst.Dst;
				_Group = dst.Group;

				ChangeTxState(dst.Tx);
				ChangeRxState(dst.Rx, false);
			}
			else
			{
				_Group = dst.Group;

				Reset(new LcState(dst.Rx, dst.Tx));
			}
		}

		public void Reset(LcDestination dst)
		{
			if (dst.Dst == "")
			{
				Reset();
			}
			else if (dst.Dst != _Dst)
			{
				_Dst = dst.Dst;
				_Group = dst.Group;

				ChangeTxState(LcTxState.Idle);
				ChangeRxState(LcRxState.Idle, false);
			}
			else
			{
				_Group = dst.Group;
			}
		}

		public void Reset(LcState st)
		{
			ChangeTxState(st.Tx);

			if ((st.Rx != LcRxState.Idle) || (_Rx != LcRxState.Mem))
			{
				ChangeRxState(st.Rx);
			}
		}

		public void ResetMem()
		{
			if (_Rx == LcRxState.Mem)
			{
				ChangeRxState(LcRxState.Idle);
			}
		}

		private void ChangeTxState(LcTxState st)
		{
			if (_Tx != st)
			{
				if (_RxNotifTimer.Enabled && (_Rx == LcRxState.RxNotif))
				{
					_RxNotifTimer.Enabled = false;
					_Rx = LcRxState.Idle;
				}
				if ((_MemNotifTimer != null) && _MemNotifTimer.Enabled && (_Rx == LcRxState.Mem))
				{
					_MemNotifTimer.Enabled = false;
					_Rx = LcRxState.Idle;
				}

				_Tx = st;
			}
		}

		private void ChangeRxState(LcRxState st, bool passToMem = true)
		{
			if (_Rx != st)
			{
                if ((_Rx == LcRxState.Rx) && (st == LcRxState.Idle) && (_Tx == LcTxState.Idle) && passToMem)
				{
					_RxNotifTimer.Enabled = true;
					_Rx = LcRxState.RxNotif;
				}
				else
				{
					_Rx = st;
					_RxNotifTimer.Enabled = false;

					if (_MemNotifTimer != null)
					{
						_MemNotifTimer.Enabled = (_Rx == LcRxState.Mem && passToMem);
					}
				}
			}
		}

		private void OnRxNotifTimerElapsed(object sender, ElapsedEventArgs e)
		{
			if (_Rx == LcRxState.RxNotif)
			{
				_Rx = LcRxState.Idle;
				General.SafeLaunchEvent(StChanged, this);
			}
		}

		private void OnMemNotifTimerElapsed(object sender, ElapsedEventArgs e)
		{
			if (_Rx == LcRxState.Mem)
			{
				_Rx = LcRxState.Idle;
				General.SafeLaunchEvent(StChanged, this);
			}
		}
	}

	public sealed class Lc
	{
		public static int NumDestinations = Settings.Default.NumLcDestinations;

		private LcDst[] _Dst = new LcDst[NumDestinations];
		private Dictionary<int, List<int>> _Groups = new Dictionary<int, List<int>>();

		[EventPublication(EventTopicNames.LcChanged, PublicationScope.Global)]
		public event EventHandler<RangeMsg> LcChanged;

		public LcDst this[int i]
		{
			get { return _Dst[i]; }
		}

		public IEnumerable<LcDst> Destinations
		{
			get { return _Dst; }
		}

		public Lc()
		{
			for (int i = 0; i < NumDestinations; i++)
			{
				_Dst[i] = new LcDst(i);
				_Dst[i].StChanged += OnLcStChanged;
			}
		}

		public int HeaderLc(int group)
		{
			Debug.Assert(_Groups.ContainsKey(group));
			return _Groups[group][0];
		}

		public LcDst ActiveLc(int group, int preferred)
		{
			Debug.Assert(_Groups.ContainsKey(group));
			List<int> members = _Groups[group];
			LcDst activeLc = _Dst[preferred].Unavailable ? null : _Dst[preferred];

			foreach (int id in members)
			{
				LcDst dst = _Dst[id];

				if (!dst.Unavailable)
				{
					if (dst.Tx != LcTxState.Idle)
					{
						activeLc = dst;
						break;
					}
					else if (dst.Rx == LcRxState.Rx)
					{
						activeLc = dst;
						break;
					}
					else if (dst.Rx == LcRxState.RxNotif)
					{
						if ((activeLc == null) || (activeLc.Rx == LcRxState.Idle) || (activeLc.Rx == LcRxState.Mem))
						{
							activeLc = dst;
						}
					}
					else if (dst.Rx == LcRxState.Mem)
					{
						if ((activeLc == null) || (activeLc.Rx == LcRxState.Idle))
						{
							activeLc = dst;
						}
					}
					else if (dst.Rx == LcRxState.Idle)
					{
						if (activeLc == null)
						{
							activeLc = dst;
						}
					}
				}
			}

			return (activeLc != null ? activeLc : _Dst[members[0]]);
		}

		public void Reset()
		{
			_Groups.Clear();

			for (int i = 0; i < NumDestinations; i++)
			{
				_Dst[i].Reset();
			}

			General.SafeLaunchEvent(LcChanged, this, new RangeMsg(0, NumDestinations));
		}

		public void Reset(RangeMsg<LcInfo> msg)
		{
			Debug.Assert(msg.From + msg.Count <= NumDestinations);

			for (int i = 0; i < msg.Count; i++)
			{
				LcDst dst = _Dst[i + msg.From];
				LcInfo info = msg.Info[i];

				if (dst.Group != info.Group)
				{
					if (dst.Group != 0)//17_01_13
					{
						Debug.Assert(_Groups.ContainsKey(dst.Group));

						List<int> members = _Groups[dst.Group];
						members.Remove(dst.Id);
						if (members.Count == 0)
						{
							_Groups.Remove(dst.Group);
						}
					}
                    if (info.Group != 0)//17_01_13
					{
						List<int> members = null;
						if (!_Groups.TryGetValue(info.Group, out members))
						{
							members = new List<int>();
							_Groups[info.Group] = members;
						}

						members.Add(i + msg.From);
						members.Sort();
					}
				}

				dst.Reset(info);
			}

			General.SafeLaunchEvent(LcChanged, this, (RangeMsg)msg);
		}

		public void Reset(RangeMsg<LcDestination> msg)
		{
			Debug.Assert(msg.From + msg.Count <= NumDestinations);

			for (int i = 0; i < msg.Count; i++)
			{
				LcDst dst = _Dst[i + msg.From];
				LcDestination info = msg.Info[i];

				if (dst.Group != info.Group)
				{
                    if (dst.Group != 0) //17_01_13
					{
						Debug.Assert(_Groups.ContainsKey(dst.Group));

						List<int> members = _Groups[dst.Group];
						members.Remove(dst.Id);
						if (members.Count == 0)
						{
							_Groups.Remove(dst.Group);
						}
					}
                    if (info.Group != 0)//17_01_13
					{
						List<int> members = null;
						if (!_Groups.TryGetValue(info.Group, out members))
						{
							members = new List<int>();
							_Groups[info.Group] = members;
						}

						members.Add(i + msg.From);
						members.Sort();
					}
				}

				dst.Reset(info);
			}

			General.SafeLaunchEvent(LcChanged, this, (RangeMsg)msg);
		}

		public void Reset(RangeMsg<LcState> msg)
		{
			Debug.Assert(msg.From + msg.Count <= NumDestinations);

			for (int i = 0; i < msg.Count; i++)
			{
				_Dst[i + msg.From].Reset(msg.Info[i]);
			}

			General.SafeLaunchEvent(LcChanged, this, (RangeMsg)msg);
		}

		public void ResetMem(int id)
		{
			Debug.Assert(id < NumDestinations);

			_Dst[id].ResetMem();

			General.SafeLaunchEvent(LcChanged, this, new RangeMsg(id, 1));
		}

		private void OnLcStChanged(object sender)
		{
			General.SafeLaunchEvent(LcChanged, this, new RangeMsg(((LcDst)sender).Id, 1));
		}
	}
}
