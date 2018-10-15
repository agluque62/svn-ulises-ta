using System;
using System.Collections.Generic;
using System.Text;

namespace HMI.Model.Module.Messages
{
    public sealed class RdFrAsignedToOtherMsg : EventArgs
    {
        public readonly int RdId;
        public readonly string Owner;

        public RdFrAsignedToOtherMsg(int rdId, string owner)
        {
            RdId = rdId;
            Owner = owner;
        }

        public override string ToString()
        {
            return string.Format("[RdId={0}] [Owner={1}]", RdId, Owner);
        }
    }
}
