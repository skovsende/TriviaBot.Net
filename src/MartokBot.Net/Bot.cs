using System;
using System.Threading;
using IrcDotNet;

namespace MartokBot.Net
{
	public class Bot : IDisposable
	{
		public event Action<IrcChannel, string, string> Message;

		private readonly IrcClient _client;
		
		public Bot()
		{
			_client = new IrcClient();
		}

		public void AddPlugin(IPlugin plugin)
		{
			plugin.InitPlugin(this);
		}

		public void Connect(string ircHostname, string nickname, string username, string realname)
		{
			BlockingConnect(ircHostname, nickname, username, realname);
			SetChannelMessageHandler();
		}

		public void Join(string channelName)
		{
			_client.Channels.Join(channelName);
		}

		public void Say(string channel, string text)
		{
			_client.LocalUser.SendMessage(channel, text);
		}

		public void Dispose()
		{
			_client.Dispose();
		}

		private void BlockingConnect(string ircHostname, string nickname, string username, string realname)
		{
			using (var connectionEvent = new ManualResetEvent(false))
			{
				EventHandler<EventArgs> connected = (o, e) => connectionEvent.Set();

				_client.Registered += connected;

				_client.Connect(ircHostname,
								new IrcUserRegistrationInfo
								{
									NickName = nickname,
									UserName = username,
									RealName = realname
								});

				if (!connectionEvent.WaitOne(20000))
					throw new ApplicationException("Timeout when connecting");
			}
		}

		private void SetChannelMessageHandler()
		{
			_client.LocalUser.JoinedChannel += (o, e) =>
				e.Channel.MessageReceived +=
					delegate(object oChannel, IrcMessageEventArgs eChannel)
					{
						if (Message != null)
							Message(oChannel as IrcChannel, eChannel.Source.Name, eChannel.Text);
					};
		}
	}
}
