using System;
using System.Collections.Generic;
using System.Text;
using HMI.Model.Module.BusinessEntities;

namespace HMI.Model.Module.Messages
{
	public sealed class ListenPickUpMsg : EventArgs
	{
		public readonly FunctionState State;
		public readonly int Id = -1;
		public readonly string Dst = "";
        public readonly string OtherDst = "";

        public ListenPickUpMsg(FunctionState st)
		{
			State = st;
		}

		public ListenPickUpMsg(FunctionState st, string dst)
		{
			State = st;
			Dst = dst;
		}
        public ListenPickUpMsg(FunctionState st, string dst, string otherDst)
        {
            State = st;
            Dst = dst;
            OtherDst = otherDst;
        }

        public ListenPickUpMsg(FunctionState st, string dst, int id)
		{
			State = st;
			Dst = dst;
			Id = id;
		}

		public override string ToString()
		{
            return string.Format("[State={0}] [Dst={2}] [Id={1}] [OtherDst={3}]", State, Id, Dst, OtherDst);
        }
    }
}
