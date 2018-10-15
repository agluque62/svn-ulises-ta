using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Practices.CompositeUI.EventBroker;
using HMI.Model.Module.Constants;
using Utilities;

namespace HMI.Model.Module.BusinessEntities
{
	public sealed class ScreenSaver
	{
		private bool _On;

		[EventPublication(EventTopicNames.ScreenSaverChanged, PublicationScope.Global)]
		public event EventHandler ScreenSaverChanged;

		public bool On
		{
			get { return _On; }
			set 
			{ 
				if (_On != value)
				{
					_On = value;
                    try
                    {
                        General.SafeLaunchEvent(ScreenSaverChanged, this);
                    }
                    catch (Microsoft.Practices.CompositeUI.EventBroker.EventTopicException )
                    {
                    }
				}
			}
		}
	}
}
