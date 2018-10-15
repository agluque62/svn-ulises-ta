using System;
using System.Collections.Generic;
using System.Text;
using HMI.Model.Module.BusinessEntities;

namespace HMI.Model.Module.Messages
{
	public sealed class ListenMsg : EventArgs
	{
		public readonly ListenState State;
		public readonly int Id = -1;
		public readonly string Dst = "";

		public ListenMsg(ListenState st)
		{
			State = st;
		}

		public ListenMsg(ListenState st, string dst)
		{
			State = st;
			Dst = dst;
		}

		public ListenMsg(ListenState st, string dst, int id)
		{
			State = st;
			Dst = dst;
			Id = id;
		}

		public override string ToString()
		{
			return string.Format("[ListenState={0}] [Dst={2}] [Id={1}]", State, Id, Dst);
		}
	}
}
