using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Practices.CompositeUI.EventBroker;
using HMI.Model.Module.Constants;
using Utilities;

namespace HMI.Model.Module.BusinessEntities
{
	public sealed class Engine
	{
		private bool _Connected = false;
		private bool _Isolated = true;

		[EventPublication(EventTopicNames.EngineStateChanged, PublicationScope.Global)]
		public event EventHandler EngineStateChanged;

		public bool Operative
		{
			get { return _Connected && !_Isolated; }
		}

		public bool Connected
		{
			get { return _Connected; }
			set 
			{
				if (_Connected != value)
				{
					_Connected = value;
					General.SafeLaunchEvent(EngineStateChanged, this);
				}
			}
		}

		public bool Isolated
		{
			get { return _Isolated; }
			set
			{
				if (_Isolated != value)
				{
					_Isolated = value;
					General.SafeLaunchEvent(EngineStateChanged, this);
				}
			}
		}
	}
}
