using System;
using System.Collections.Generic;
using System.Text;

namespace HMI.Model.Module.Messages
{
	public sealed class JacksStateMsg : EventArgs
	{
		public readonly bool LeftJack;  // Tambi�n se va a utilizar para altavoz radio, y altavoz HF
        public readonly bool RightJack; // Tambi�n se va a utilizar para altavoz l�nea caliente y cable grabaci�n

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
