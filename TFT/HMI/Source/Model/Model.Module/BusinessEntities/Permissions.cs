using System;
using System.Collections.Generic;
using System.Text;

namespace HMI.Model.Module.BusinessEntities
{
	[Flags]
	public enum Permissions
	{
		None = 0,
		Priority = 0x1,
		Listen = 0x2,
		Hold = 0x4,
		Transfer = 0x8,
		Intruded = 0x10,
        Conference = 0x20,
        Replay = 0x40
    }
}
