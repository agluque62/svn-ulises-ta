using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Practices.CompositeUI.EventBroker;
using HMI.Model.Module.Constants;
using HMI.Model.Module.Properties;
using Utilities;

namespace HMI.Model.Module.BusinessEntities
{
	public sealed class Jacks :Description
	{
		private bool _LeftJack = false;
		private bool _RightJack = false;

		[EventPublication(EventTopicNames.JacksChanged, PublicationScope.Global)]
		public event EventHandler JacksChanged;

		public bool LeftJack
		{
			get { return _LeftJack; }
		}

		public bool RightJack
		{
			get { return _RightJack; }
		}

		public bool SomeJack
		{
			get { return (_LeftJack || _RightJack); }
		}

		public void Reset(bool leftJack, bool rightJack)
		{
			if ((leftJack != _LeftJack) || (rightJack != _RightJack))
			{
				_LeftJack = leftJack;
				_RightJack = rightJack;

				StateDescription = SomeJack ? "" : Resources.NoJacksStateDescription;

				General.SafeLaunchEvent(JacksChanged, this);
			}
		}
	}
}
