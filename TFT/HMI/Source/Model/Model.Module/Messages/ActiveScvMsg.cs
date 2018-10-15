using System;
using System.Collections.Generic;
using System.Text;

namespace HMI.Model.Module.Messages
{
	public sealed class ActiveScvMsg : EventArgs
	{
		public readonly int Scv;

		public ActiveScvMsg(int scv)
		{
			Scv = scv;
		}

		public override string ToString()
		{
			return string.Format("[Scv={0}]", Scv);
		}
	}
}
