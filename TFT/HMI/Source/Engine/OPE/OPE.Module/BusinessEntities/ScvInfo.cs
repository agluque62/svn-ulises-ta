using System;
using System.Collections.Generic;
using System.Text;

namespace HMI.OPE.Module.BusinessEntities
{
	class ScvInfo
	{
		public int OpeId;
		public int IolId;
		public string OpeVersion = "";
		public string OpeDate = "";
		public string IolVersion = "";
		public string IolDate = "";
		public string DspVersion = "";
		public string DspDate = "";

		public void Reset(int opeId, int iolId)
		{
			OpeId = opeId;
			IolId = iolId;
			OpeVersion = "";
			OpeDate = "";
			IolVersion = "";
			IolDate = "";
			DspVersion = "";
			DspDate = "";
		}

		public override string ToString()
		{
			return string.Format("OpeId = {0, -6}OpeVersion = {1, -12}OpeDate = {2}" + Environment.NewLine +
				"IOLId  = {3, -6}IolVersion    = {4, -13}IolDate    = {5}" + Environment.NewLine +
				"                     DspVersion  = {6, -11}DspDate  = {7}",
				OpeId, OpeVersion, OpeDate, IolId, IolVersion, IolDate, DspVersion, DspDate);
		}
	}
}
