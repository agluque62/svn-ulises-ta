using System;
using System.Collections.Generic;
using System.Text;

namespace HMI.Model.Module.Messages
{
	public sealed class PositionIdMsg : EventArgs
	{
		public readonly string Id;

		public PositionIdMsg(string id)
		{
			Id = id;
		}

		public override string ToString()
		{
			return string.Format("[PositionId={0}]", Id);
		}
	}
}
