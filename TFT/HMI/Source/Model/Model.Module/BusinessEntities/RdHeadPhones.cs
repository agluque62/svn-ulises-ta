using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Practices.CompositeUI.EventBroker;
using HMI.Model.Module.Constants;
using HMI.Model.Module.Properties;
using Utilities;

namespace HMI.Model.Module.BusinessEntities
{
	public sealed class RdHeadPhones
	{
		private int _Level = 0;

		[EventPublication(EventTopicNames.RdHeadPhonesLevelChanged, PublicationScope.Global)]
		public event EventHandler RdHeadPhonesLevelChanged;

		public int Level
		{
			get { return _Level; }
			set 
			{ 
				if (_Level != value)
				{
					_Level = value;

					Settings.Default.RdHeadPhonesLevel = value;
					Settings.Default.Save();

					General.SafeLaunchEvent(RdHeadPhonesLevelChanged, this);
				}
			}
		}

		public RdHeadPhones()
		{
			_Level = Settings.Default.RdHeadPhonesLevel;
		}
	}
}
