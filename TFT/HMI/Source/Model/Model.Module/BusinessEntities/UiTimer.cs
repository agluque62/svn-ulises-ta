using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.Diagnostics;

namespace HMI.Model.Module.BusinessEntities
{
	public class UiTimer : Timer
	{
		public new bool Enabled
		{
			get { return base.Enabled; }
			set
			{
				if (value && (SynchronizingObject == null))
				{
					SynchronizingObject = System.Windows.Forms.Control.FromHandle(Process.GetCurrentProcess().MainWindowHandle);
				}

				base.Enabled = value;
			}
		}

		public UiTimer() : base() {}
		public UiTimer(double tout) : base(tout) {}
	}
}
