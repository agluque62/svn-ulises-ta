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
    /** 20190205 */
    public sealed class TxInProgressErrorCode : EventArgs
    {
        public readonly int IdEvent;            /* 0: TxConfirmation timeout, 1: TxCarrierDetect Timeout, 2: RTX Supervision Error. */

        public TxInProgressErrorCode(int idEvent)
        {
            IdEvent = idEvent;
        }

        public override string ToString()
        {
            return string.Format("[IdEvent={0}]", IdEvent);
        }
    }
}
