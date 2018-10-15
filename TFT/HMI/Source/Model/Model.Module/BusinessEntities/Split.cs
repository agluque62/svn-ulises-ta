using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Practices.CompositeUI.EventBroker;
using HMI.Model.Module.Constants;
using HMI.Model.Module.Properties;
using Utilities;

namespace HMI.Model.Module.BusinessEntities
{
	public enum SplitMode
	{
		Off = 0,
		LcTf,
		RdLc
	}

	public sealed class Split
	{
		private SplitMode _Mode = SplitMode.Off;

		[EventPublication(EventTopicNames.SplitModeChanged, PublicationScope.Global)]
		public event EventHandler SplitModeChanged;

		public SplitMode Mode
		{
			get { return _Mode; }
			set 
			{ 
				if (_Mode != value)
				{
					_Mode = value;

					Settings.Default.SplitMode = (int)value;
					Settings.Default.Save();

					General.SafeLaunchEvent(SplitModeChanged, this);
				}
			}
		}

		public Split()
		{
			_Mode = (SplitMode)Settings.Default.SplitMode;
		}
	}
}
