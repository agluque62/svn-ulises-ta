using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.OracleClient;
using System.Text;
using System.Net;
using System.Threading;
using Microsoft.Practices.CompositeUI.EventBroker;
using HMI.OPE.Module.Constants;
using HMI.OPE.Module.Properties;
using HMI.Model.Module.BusinessEntities;
using HMI.Model.Module.Messages;
using Utilities;
using NLog;

namespace HMI.OPE.Module.Services
{
	class ServerCommunicationsService
	{
		private delegate void ReceivedHandler();

		private static Logger _Logger = LogManager.GetCurrentClassLogger();

		private bool _Stop;
		private string _NumUser;
		private UdpSocket _Comm;
		private ReceivedHandler[] _Handlers;
		private object _Sync = new object();
		private byte[][] _Buffers = new byte[4][];
		private Dictionary<string, string> _LongNames = new Dictionary<string, string>();
		private Dictionary<string, string> _Alias = new Dictionary<string, string>();
		private Dictionary<string, string> _R2Rtb = new Dictionary<string, string>();
		private Dictionary<string, string> _R2Alias = new Dictionary<string, string>();

		[EventPublication(EventTopicNames.AgendaChangedEngine, PublicationScope.Global)]
		public event EventHandler<RangeMsg<Number>> AgendaChangedEngine;

		[EventPublication(EventTopicNames.NumberBookChangedEngine, PublicationScope.Global)]
		public event EventHandler<RangeMsg<Area>> NumberBookChangedEngine;

		public void Run()
		{
			try
			{
				_Stop = false;
				_Handlers = new ReceivedHandler[4] { SetLongNames, SetAgenda, SetDependences, SetDependences };

				foreach (string str in Settings.Default.LongNames)
				{
					string[] shortLong = str.Split('=');
					_LongNames[shortLong[0].Trim()] = shortLong[1].Trim();
				}
				foreach (string str in Settings.Default.Alias)
				{
					string[] nameAlias = str.Split('=');
					_Alias[nameAlias[0].Trim()] = nameAlias[1].Trim();
				}

				if (Settings.Default.ListenPort != 0)
				{
					_Comm = new UdpSocket(Settings.Default.ListenPort);
					_Comm.MaxReceiveThreads = 1;
					_Comm.NewDataEvent += OnNewData;

					_Comm.BeginReceive();
				}
			}
			catch (Exception) {}
		}

		public void Stop()
		{
			_Stop = true;

			if (_Comm != null)
			{
				_Comm.Dispose();
				_Comm = null;
			}
		}

		public void AskServerCfg(int numUser)
		{
			_NumUser = numUser.ToString();
			SetAgenda();
		}

		public string GetEquivalentName(string name)
		{
			if (!string.IsNullOrEmpty(name))
			{
				return GetNameAlias(GetLongName(name));
			}

			return name;
		}

		public string GetLongName(string name)
		{
			string longName;

			lock (_Sync)
			{
				if (!_LongNames.TryGetValue(name, out longName))
				{
					longName = name;
				}
			}

			return longName;
		}

		public string GetNameAlias(string name)
		{
			string alias;
			
			if (!_Alias.TryGetValue(name, out alias))
			{
				alias = name;
			}

			return alias;
		}

		public string GetNameAlias(string name, string defValue)
		{
			string alias;

			if (!_Alias.TryGetValue(name, out alias))
			{
				alias = defValue;
			}

			return alias;
		}

		public string GetRtbNumber(string r2Number)
		{
			string rtbNumber;

			lock (_Sync)
			{
				Debug.Assert(r2Number.Length > 2);

				if (!_R2Rtb.TryGetValue(r2Number.Substring(2), out rtbNumber))
				{
					rtbNumber = null;
				}
			}

			return rtbNumber;
		}

		public string GetNumberAlias(string number)
		{
			string alias;

			lock (_Sync)
			{
				if ((number.Length <= 2) || !_R2Alias.TryGetValue(number.Substring(2), out alias))
				{
					alias = number;
				}
				else
				{
					alias = GetEquivalentName(alias);
				}
			}

			return alias;
		}

		void OnNewData(object sender, DataGram dg)
		{
			try
			{
				if ((dg.Data.Length == 510) && (dg.Data[0] == 1) && (dg.Data[1] < 4))
				{
					int type = dg.Data[1];
					int length = (dg.Data[2] | (dg.Data[3] << 8) | (dg.Data[4] << 16) | (dg.Data[5] << 24));
					int block = (dg.Data[8] | (dg.Data[9] << 8));
					int numBlocks = (length + 499) / 500;

					if ((_Buffers[type] == null) || !ArrayEquals(_Buffers[type], 0, dg.Data, 2, 6))
					{
						_Buffers[type] = new byte[6 + numBlocks + (numBlocks * 500)];
						Array.Copy(dg.Data, 2, _Buffers[type], 0, 6);
					}

					if (_Buffers[type][block + 6] != 1)
					{
						_Buffers[type][block + 6] = 1;
						Array.Copy(dg.Data, 10, _Buffers[type], 6 + numBlocks + (block * 500), 500);

						if (_Handlers[type] != null)
						{
							_Handlers[type]();
						}
					}
				}
			}
			catch (Exception ex)
			{
				if (!_Stop)
				{
					_Logger.Error(Resources.ServerError, ex);
				}
			}
		}

		bool ArrayEquals(byte[] a, int aIndex, byte[] b, int bIndex, int count)
		{
			for (int i = 0; i < count; i++)
			{
				if (a[aIndex + i] != b[bIndex + i])
				{
					return false;
				}
			}

			return true;
		}

		void SetLongNames()
		{
			if (_Buffers[0] != null)
			{
				byte[] data = _Buffers[0];
				int length = (data[0] | (data[1] << 8) | (data[2] << 16) | (data[3] << 24));
				int numBlocks = (length + 499) / 500;

				for (int i = 0; i < numBlocks; i++)
				{
					if (data[6 + i] != 1)
					{
						return;
					}
				}

				string str = Encoding.UTF8.GetString(data, 6 + numBlocks, length);
				string[] rows = str.Split(';');

				lock (_Sync)
				{
					_LongNames.Clear();

					foreach (string row in rows)
					{
						string[] colums = row.Split(',');

						if (colums.Length == 2)
						{
							_LongNames[colums[0]] = colums[1];
						}
					}
				}

				Settings.Default.LongNames.Clear();
				foreach (KeyValuePair<string, string> p in _LongNames)
				{
					Settings.Default.LongNames.Add(p.Key + "=" + p.Value);
				}

				Settings.Default.Save();
			}
		}

		void SetAgenda()
		{
			if (!string.IsNullOrEmpty(_NumUser) && (_Buffers[1] != null))
			{
				byte[] data = _Buffers[1];
				int length = (data[0] | (data[1] << 8) | (data[2] << 16) | (data[3] << 24));
				int numBlocks = (length + 499) / 500;

				for (int i = 0; i < numBlocks; i++)
				{
					if (data[6 + i] != 1)
					{
						return;
					}
				}

				string str = Encoding.UTF8.GetString(data, 6 + numBlocks, length);
				string[] rows = str.Split(';');

				List<Number> agenda = new List<Number>();

				foreach (string row in rows)
				{
					string[] columns = row.Split(',');

					if ((columns.Length == 3) && (columns[0] == _NumUser))
					{
						agenda.Add(new Number(columns[2], columns[1]));
					}
				}

				General.SafeLaunchEvent(AgendaChangedEngine, this, new RangeMsg<Number>(agenda.ToArray()));
			}
		}

		void SetDependences()
		{
			if ((_Buffers[2] != null) && (_Buffers[3] != null))
			{
				byte[] dData = _Buffers[2];
				int dLength = (dData[0] | (dData[1] << 8) | (dData[2] << 16) | (dData[3] << 24));
				int dNumBlocks = (dLength + 499) / 500;

				for (int i = 0; i < dNumBlocks; i++)
				{
					if (dData[6 + i] != 1)
					{
						return;
					}
				}

				byte[] uData = _Buffers[3];
				int uLength = (uData[0] | (uData[1] << 8) | (uData[2] << 16) | (uData[3] << 24));
				int uNumBlocks = (uLength + 499) / 500;

				for (int i = 0; i < uNumBlocks; i++)
				{
					if (uData[6 + i] != 1)
					{
						return;
					}
				}

				Dictionary<int, Depencence> dependences = new Dictionary<int, Depencence>();
				Dictionary<string, Area> numberBook = new Dictionary<string, Area>();

				string dStr = Encoding.UTF8.GetString(dData, 6 + dNumBlocks, dLength);
				string[] dRows = dStr.Split(';');

				foreach (string row in dRows)
				{
					string[] columns = row.Split(',');

					if (columns.Length == 5)
					{
						Area area;
						Fir fir;
						Depencence dependence;

						string areaName = columns[1];
						string firName = columns[2];
						string dependenceName = columns[3] + " (" + columns[4] + ")";

						if (!numberBook.TryGetValue(areaName, out area))
						{
							area = new Area(areaName);
							numberBook[areaName] = area;
						}
						if (!area.TryGetValue(firName, out fir))
						{
							fir = new Fir(firName);
							area[firName] = fir;
						}
						if (!fir.TryGetValue(dependenceName, out dependence))
						{
							dependence = new Depencence(dependenceName);
							fir[dependenceName] = dependence;
						}

						dependences[int.Parse(columns[0])] = dependence;
					}
				}

				lock (_Sync)
				{
					_R2Rtb.Clear();
					_R2Alias.Clear();

					string uStr = Encoding.UTF8.GetString(uData, 6 + uNumBlocks, uLength);
					string[] uRows = uStr.Split(';');

					foreach (string row in uRows)
					{
						string[] columns = row.Split(',');

						if (columns.Length == 6)
						{
							UserNumber user = new UserNumber(columns[1], columns[2], columns[3], columns[4], columns[5]);
							dependences[int.Parse(columns[0])][user.Name] = user;

							if (user.RtbNumber.Length > 2)
							{
								_R2Rtb[user.R2Number] = user.RtbNumber;
							}
							_R2Alias[user.R2Number] = user.Name;
						}
					}
				}

				General.SafeLaunchEvent(NumberBookChangedEngine, this, new RangeMsg<Area>(0, numberBook.Values.Count, numberBook.Values));
			}
		}
	}
}
