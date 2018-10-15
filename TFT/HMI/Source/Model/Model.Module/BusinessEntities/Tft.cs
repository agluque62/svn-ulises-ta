using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Practices.CompositeUI.EventBroker;
using HMI.Model.Module.Messages;
using HMI.Model.Module.Constants;
using Utilities;

namespace HMI.Model.Module.BusinessEntities
{
	public sealed class Tft
	{
        private bool _Enabled = true;

        private bool _Briefing = false;
        private bool _Playing = false;

        [EventPublication(EventTopicNames.TftEnabledChanged, PublicationScope.Global)]
        public event EventHandler TftEnabledChanged;
        [EventPublication(EventTopicNames.BriefingChanged, PublicationScope.Global)]
        public event EventHandler BriefingChanged;
        [EventPublication(EventTopicNames.PlayingChanged, PublicationScope.Global)]
        public event EventHandler PlayingChanged;

		public bool Enabled
		{
			get { return _Enabled; }
			set 
			{ 
				if (_Enabled != value)
				{
					_Enabled = value;
					General.SafeLaunchEvent(TftEnabledChanged, this);
				}
			}
		}

        public bool Briefing
        {
            get { return _Briefing; }
            set
            {
                if (_Briefing != value) 
                {
                    _Briefing = value;

                    General.SafeLaunchEvent(BriefingChanged, this);
                }
            }
        }

        public bool Playing
        {
            get { return _Playing; }
            set
            {
                if (_Playing != value)
                {
                    _Playing = value;

                    General.SafeLaunchEvent(PlayingChanged, this);
                }
            }
        }
    }
}
