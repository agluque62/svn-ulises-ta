using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Practices.CompositeUI.EventBroker;
using HMI.Model.Module.Constants;
using HMI.Model.Module.Properties;
using Utilities;

namespace HMI.Model.Module.BusinessEntities
{
	public sealed class Jacks
	{
		private bool _LeftJack = false;
		private bool _RightJack = false;
		private string _StateDescription = "";
		private string _PreviusStateDescription = "";

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

		public string StateDescription
		{
			get { return _StateDescription; }
		}

		public string PreviusStateDescription
		{
			get { return _PreviusStateDescription; }
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

				_PreviusStateDescription = _StateDescription;
				_StateDescription = SomeJack ? "" : Resources.NoJacksStateDescription;

				General.SafeLaunchEvent(JacksChanged, this);
			}
		}
	}
}
