using System;
using System.Collections.Generic;
using System.Text;

namespace HMI.Model.Module.Messages
{
	public sealed class EngineConnectionStateMsg : EventArgs
	{
		public readonly bool Connected;

		public EngineConnectionStateMsg(bool connected)
		{
			Connected = connected;
		}

		public override string ToString()
		{
			return string.Format("[EngineConnected={0}]", Connected);
		}
	}

	public sealed class EngineIsolatedStateMsg : EventArgs
	{
		public readonly bool Isolated;

		public EngineIsolatedStateMsg(bool isolated)
		{
			Isolated = isolated;
		}

		public override string ToString()
		{
			return string.Format("[EngineIsolated={0}]", Isolated);
		}
	}
}
