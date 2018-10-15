using System;
using System.Collections.Generic;
using System.Text;

namespace HMI.Model.Module.Messages
{
	public sealed class LevelMsg<T> : EventArgs
	{
		public readonly int Level;

		public LevelMsg(int level)
		{
			Level = level;
		}

		public override string ToString()
		{
			return string.Format("[Level={0}]", Level);
		}
	}
}
