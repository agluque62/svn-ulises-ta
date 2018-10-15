using System;
using System.Collections.Generic;
using System.Text;

namespace HMI.Model.Module.Messages
{
	public sealed class RdMaxPositionMsg : EventArgs
	{
		public readonly int Pos;

		public RdMaxPositionMsg(int numRd)
		{
			Pos = numRd;
		}

		public override string ToString()
		{
			return string.Format("[RdMaxPositions={0}]", Pos);
		}
	}
}
