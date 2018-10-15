using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMI.Model.Module.Messages
{
    public class RdHfFrAssigned : EventArgs
    {
        public readonly int Id;
        public readonly uint HfEstado;

        public RdHfFrAssigned(int id)
        {
            Id = id;
        }

        public RdHfFrAssigned(int id, uint estado)
        {
            Id = id;
            HfEstado = estado;
        }

        public override string ToString()
		{
			return string.Format("[PositionId={0}]", Id);
		}
    }
}
