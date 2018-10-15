using System;
using System.Collections.Generic;
using System.Text;

namespace HMI.Model.Module.Messages
{
	public sealed class PageMsg : EventArgs
	{
		public readonly int Page;

		public PageMsg(int page)
		{
			Page = page;
		}

		public override string ToString()
		{
			return string.Format("[Page={0}]", Page);
		}
	}
}
