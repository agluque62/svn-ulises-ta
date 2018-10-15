using System;
using System.Collections.Generic;
using System.Text;

using Utilities;

namespace HMI.CD40.Module.BusinessEntities
{
	abstract class Resource
	{
		public event GenericEventHandler Changed;
		public event GenericEventHandler<short> NewMsg;
        public event GenericEventHandler<short> SelCalMsg;
        public event GenericEventHandler<short> SiteChanged;

		public string Id
		{
			get { return _Id; }
		}

		public object Content
		{
			get { return _Content; }
		}

		public bool IsValid
		{
			get { return (_Content != null); }
		}

		public bool IsUnreferenced
		{
			get { return ((_ContainerId == null) && (Changed == null) && (NewMsg == null)); }
		}

		public Resource(string id)
		{
			_Id = id;
		}

		public void ResetSubscribers()
		{
			Changed = null;
			NewMsg = null;
            SelCalMsg = null;
		}

		public void Reset(string containerId, object content)
		{
			object oldContent = _Content;

			if (containerId == _ContainerId)
			{
				_Content = null;
			    _ContainerId = null;
			}

            //Actualizo siempre para evitar que se pierdan cambios (recursos que nacen con rs = null) y no cambian
//			if (content != null)
			{
				_Content = content;
				_ContainerId = containerId;
			}

			if (oldContent != _Content)
			{
				General.SafeLaunchEvent(Changed, this);
			}
		}

        public void NotifNewMsg(short type, object msg)
        {
            General.SafeLaunchEvent(NewMsg, msg, type);
        }

        public void NotifSelCal(short type, object msg)
        {
            General.SafeLaunchEvent(SelCalMsg, msg, type);
        }

        public void NotifSiteChanged(short type, object resultado)
        {
            General.SafeLaunchEvent(SiteChanged, resultado, type);
        }

        #region Protected Members

		protected string _Id;
		protected string _ContainerId;
		protected object _Content;

		#endregion
	}

	class Rs<T> : Resource where T : class
	{
		public T Info
		{
			get { return (T)_Content; }
		}

		public Rs(string id) : base(id)
		{
		}
	}
}
