using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Practices.CompositeUI.EventBroker;
using HMI.Model.Module.Constants;
using HMI.Model.Module.Properties;
using Utilities;

namespace HMI.Model.Module.BusinessEntities
{
	public sealed class TlfHeadPhones
	{
		private int _Level = 0;

		[EventPublication(EventTopicNames.TlfHeadPhonesLevelChanged, PublicationScope.Global)]
		public event EventHandler TlfHeadPhonesLevelChanged;

		public int Level
		{
			get { return _Level; }
			set 
			{ 
				if (_Level != value)
				{
					_Level = value;

					Settings.Default.TlfHeadPhonesLevel = value;
					Settings.Default.Save();

					General.SafeLaunchEvent(TlfHeadPhonesLevelChanged, this);
				}
			}
		}

		public TlfHeadPhones()
		{
			_Level = Settings.Default.TlfHeadPhonesLevel;
		}
	}
}
