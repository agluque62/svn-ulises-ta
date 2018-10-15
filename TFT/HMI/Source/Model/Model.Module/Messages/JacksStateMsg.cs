using System;
using System.Collections.Generic;
using System.Text;

namespace HMI.Model.Module.Messages
{
	public sealed class JacksStateMsg : EventArgs
	{
		public readonly bool LeftJack;  // También se va a utilizar para altavoz radio, y altavoz HF
        public readonly bool RightJack; // También se va a utilizar para altavoz línea caliente y cable grabación

		public JacksStateMsg(bool leftJack, bool rightJack)
		{
			LeftJack = leftJack;
			RightJack = rightJack;
		}

		public override string ToString()
		{
			return string.Format("[LeftJack={0}] [RightJack={1}]", LeftJack, RightJack);
		}
	}
}
