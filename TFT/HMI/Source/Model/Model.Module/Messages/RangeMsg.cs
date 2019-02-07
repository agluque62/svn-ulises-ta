using System;
using System.Collections.Generic;
using System.Text;
using HMI.Model.Module.BusinessEntities;

namespace HMI.Model.Module.Messages
{
	public sealed class LcDestination
	{
		public readonly string Dst = "";
		public readonly int Group = 0;

		public LcDestination(string dst, int group)
		{
			Dst = dst;
			Group = group;
		}

		public override string ToString()
		{
			return string.Format("[Dst={0}] [Group={1}]", Dst, Group); 
		}
	}

	public sealed class LcState
	{
		public LcRxState Rx;
		public LcTxState Tx;

		public LcState() 
			: this(LcRxState.Idle, LcTxState.Idle)
		{
		}

		public LcState(LcRxState rx, LcTxState tx)
		{
			Rx = rx;
			Tx = tx;
		}

		public override string ToString()
		{
			return string.Format("[Rx={0}] [Tx={1}]", Rx, Tx);
		}
	}

	public sealed class LcInfo
	{
		public readonly string Dst;
		public readonly LcRxState Rx;
		public readonly LcTxState Tx;
		public readonly int Group;

		public LcInfo(string dst, LcRxState rx, LcTxState tx, int group)
		{
			Dst = dst;
			Rx = rx;
			Tx = tx;
			Group = group;
		}

		public override string ToString()
		{
			return string.Format("[Dst={0}] [Rx={1}] [Tx={2}] [Group={3}]", Dst, Rx, Tx, Group); 
		}
	}

	public sealed class TlfDestination
	{
		public readonly string Dst;

		public TlfDestination(string dst)
		{
			Dst = dst;
		}

		public override string ToString()
		{
			return string.Format("[Dst={0}]", Dst);
		}
	}

	public sealed class TlfInfo
	{
		public readonly string Dst;
		public readonly TlfState St;
        public readonly bool _PriorityAllowed;

		public TlfInfo(string dst, TlfState st, bool priorityAllowed)
		{
			Dst = dst;
			St = st;
            _PriorityAllowed = priorityAllowed;
		}

		public override string ToString()
		{
            return string.Format("[Dst={0}] [State={1}] [PrioAllowed={2}]", Dst, St, _PriorityAllowed);
		}
	}

	public sealed class RdDestination
	{
		public readonly string Dst;
		public readonly string Alias;

		public RdDestination(string dst, string alias)
		{
			Dst = dst;
			Alias = alias;
		}

		public override string ToString()
		{
			return string.Format("[Dst={0}] [Alias={1}]", Dst, Alias);
		}
	}

	public sealed class RdAsignState
	{
		public bool Tx;
		public bool Rx;
		public RdRxAudioVia AudioVia;

		public RdAsignState()
			: this(false, false, RdRxAudioVia.NoAudio)
		{
		}

		public RdAsignState(bool tx, bool rx, RdRxAudioVia audio)
		{
			Tx = tx;
			Rx = rx;
			AudioVia = audio;
		}

		public override string ToString()
		{
			return string.Format("[Rx={0}] [Tx={1}] [AudioVia={2}]", Rx, Tx, AudioVia);
		}
	}

	public sealed class RdRtxGroup
	{
		public readonly int RtxGroup;

		public RdRtxGroup(int rtxGroup)
		{
			RtxGroup = rtxGroup;
		}

		public override string ToString()
		{
			return string.Format("[RtxGroup={0}]", RtxGroup);
		}
	}

	public sealed class RdInfo
	{
        // Informacion para poder pintar la poscion de una radio.
        // Incluye el estado de la frecuencia (degradada, disponible o no).
		public readonly string Dst;
        public readonly string Alias;
		public readonly bool Tx;
		public readonly bool Rx;
		public readonly PttState Ptt;
		public readonly SquelchState Squelch;
		public readonly RdRxAudioVia AudioVia;
		public readonly int RtxGroup;
        public readonly TipoFrecuencia_t TipoFrecuencia;
        public readonly bool Monitoring;
        public readonly FrequencyState Estado;
        public readonly bool RxOnly;
        /** 20180321. AGL. ALIAS a mostrar en la tecla... */
        public string KeyAlias { get; set; }

        public RdInfo(string dst, string alias, bool tx, bool rx, PttState ptt, SquelchState squelch,
            RdRxAudioVia audioVia, int rtxGroup, TipoFrecuencia_t tipoFrecuencia, bool monitoring, FrequencyState estado, bool rxOnly)
		{
			Dst = dst;
			Alias = alias;
			Tx = tx;
			Rx = rx;
			Ptt = ptt;
			Squelch = squelch;
			AudioVia = audioVia;
			RtxGroup = rtxGroup;
            TipoFrecuencia = tipoFrecuencia;
            Monitoring = monitoring;
            Estado = estado;
            RxOnly = rxOnly;
        }
		public override string ToString()
		{
            return string.Format("[Dst={0}] [Alias={1}] [Rx={2}] [Tx={3}] [Ptt={4}] [Squelch={5}] [AudioVia={6}] [RxtGroup={7}] [TipoFrecuencia={8}] [Monitoring={9}] [Estado={10}] [RxOnly={11}]", Dst, Alias, Rx, Tx, Ptt, Squelch, AudioVia, RtxGroup, TipoFrecuencia, Monitoring, Estado, RxOnly);
		}
	}

	public sealed class RdState
	{
		public readonly bool Tx;
		public readonly bool Rx;
		public readonly PttState Ptt;
		public readonly SquelchState Squelch;
		public readonly RdRxAudioVia AudioVia;
		public readonly int RtxGroup;
        public readonly FrequencyState State;
        // BSS Information
        public readonly string QidxMethod;
        public readonly uint QidxValue;
        public readonly string QidxResource;
        /** 20190205 RTX Information */
        public readonly string PttSrcId;

		public RdState(bool tx, bool rx, string pttSrcId, PttState ptt, SquelchState squelch, RdRxAudioVia audioVia, int rtxGroup, FrequencyState state,
            string qidxMethod, uint qidxValue, string qidxResource)
		{
			Tx = tx;
			Rx = rx;
			Ptt = ptt;
			Squelch = squelch;
			AudioVia = audioVia;
			RtxGroup = rtxGroup;
            State = state;
		    // BSS Information
            QidxMethod = qidxMethod;
            QidxValue = qidxValue;
            QidxResource = qidxResource;

            PttSrcId = pttSrcId;
        }

		public override string ToString()
		{
			return string.Format("[Rx={0}] [Tx={1}] [Ptt={2}] [Squelch={3}] [AudioVia={4}] [RxtGroup={5}] [FrequencyState={6}]", Rx, Tx, Ptt, Squelch, AudioVia, RtxGroup, State);
		}
	}

	public sealed class TlfIaDestination
	{
		public string Alias;
		public string Number;
		public TlfState State;

		public TlfIaDestination(string alias, string number, TlfState state)
		{
			Alias = alias;
			Number = number;
			State = state;
		}

		public override string ToString()
		{
			return string.Format("[Alias={0}] [Number={1}] [State={2}]", Alias, Number, State);
		}
	}

	public class RangeMsg : EventArgs
	{
		public readonly int From;
		public readonly int Count;

		public RangeMsg(int from, int count)
		{
			From = from;
			Count = count;
		}

		public override string ToString()
		{
			return string.Format("[From={0}] [Count={1}]", From, Count);
		}
	}

	public sealed class RangeMsg<T> : RangeMsg
	{
		public readonly T[] Info;

		public RangeMsg(int from, int count) : base(from, count)
		{
			Info = new T[count];
		}

		public RangeMsg(int id, T info) : base(id, 1)
		{
			Info = new T[1] { info };
		}

		public RangeMsg(int from, int count, T info) : base(from, count)
		{
			Info = new T[count];

			for (int i = 0; i < count; i++)
			{
				Info[i] = info;
			}
		}

		public RangeMsg(T[] info) : base(0, info.Length)
		{
			Info = info;
		}

		public RangeMsg(int from, int count, IEnumerable<T> info) : base(from, count)
		{
			Info = new T[count];

			int i = 0;
			foreach (T el in info)
			{
				Info[i++] = el;
			}
		}

		public override string ToString()
		{
			if (Count == 1)
			{
				return string.Format("[Id={0}] {1}", From, Info[0]);
			}
			else
			{
				StringBuilder str = new StringBuilder(base.ToString());

				for (int i = 0; i < Count; i++)
				{
					str.AppendFormat("{0}\t[{1}] {2}", Environment.NewLine, i + From, Info[i]);
				}
				return str.ToString();
			}
		}
	}
}
