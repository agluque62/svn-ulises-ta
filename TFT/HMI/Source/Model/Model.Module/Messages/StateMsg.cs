using System;
using System.Collections.Generic;
using System.Text;

namespace HMI.Model.Module.Messages
{
	public sealed class StateMsg<T> : EventArgs
	{
		public readonly T State;

		public StateMsg(T st)
		{
			State = st;
		}

		public override string ToString()
		{
			return string.Format("[State={0}]", State);
		}
	}

	public sealed class SnmpStringMsg<T1, T2> : EventArgs
	{
		public readonly T1 Oid;
		public readonly T2 Value;

		public SnmpStringMsg(T1 oid, T2 value)
		{
			Oid = oid;
			Value = value;
		}

		public override string ToString()
		{
			return string.Format("[Oid={0}] | [Value={1}]", Oid, Value);
		}
	}

	public sealed class SnmpIntMsg<T1, T2> : EventArgs
	{
		public readonly T1 Oid;
		public readonly T2 Value;

		public SnmpIntMsg(T1 oid, T2 value)
		{
			Oid = oid;
			Value = value;
		}

		public override string ToString()
		{
			return string.Format("[Oid={0}] | [Value={1}]", Oid, Value);
		}
	}

}
