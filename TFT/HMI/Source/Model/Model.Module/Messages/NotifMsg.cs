using System;
using System.Collections.Generic;
using System.Text;
using HMI.Model.Module.BusinessEntities;

namespace HMI.Model.Module.Messages
{
	public enum MessageButtons
	{
		None,
		Ok,
		OkCancel
	}

	public enum MessageType
	{
		Error,
		Warning,
		Information,
		Processing
	}

	public enum NotifMsgResponse
	{
		Ok,
		Cancel,
		Timeout
	}

	public sealed class NotifMsg : EventArgs
	{
		public readonly string Id;
		public readonly string Caption;
		public readonly string Text;
		public readonly int TimeoutMs;
		public readonly MessageType Type;
		public readonly MessageButtons Buttons;
		public int Height;
		public int Width;
		public object Info;

		public NotifMsg(string id, string caption, string text, int timeOutMs, MessageType type, MessageButtons buttons)
			: this(id, caption, text, timeOutMs, type, buttons, null)
		{
		}

		public NotifMsg(string id, string caption, string text, int timeOutMs, MessageType type, MessageButtons buttons, object info)
		{
			Id = id;
			Caption = caption;
			Text = text;
			TimeoutMs = timeOutMs;
			Type = type;
			Buttons = buttons;
			Height = 110;
			Width = 400;
			Info = info;
		}

		public override string ToString()
		{
			return string.Format("[Id={0}] [Caption={1}] [Text={2}] [TimeoutMs={3}] [Type={4}] [Buttons={5}]", Id, Caption, Text, TimeoutMs, Type, Buttons);
		}
	}
}
