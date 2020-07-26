using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Text.RegularExpressions;

//основа кода "позаимствована" у юзера AmbushedRaccoon, большое ему за это спасiбi
//https://github.com/AmbushedRaccoon/TwitchChatBot

namespace h0peBot_
{
	public class TwitchInit
	{
		public const string Host = "irc.twitch.tv";
		public const int port = 6667;
		public const string BotNick = "h0peBot_";
		public const string ChannelName = "h0pelesss_";
	}

	public class TwitchIRCClient
	{
		private TcpClient client;
		private StreamReader reader;
		private StreamWriter writer;
		private string OAuthToken;
		private Dictionary<string, string> commands = new Dictionary<string, string>
        
        //если разместить "!тест члена" после "!тест" в ,
        //то при вызове первой команды будет срабатывать вторая
        //из-за того, что команда считывает ввод через функцию .Contains()
        //решения этой проблемы я (пока) не нашел
        //моё решение - не пользоваться этим говном 4Голова
        
        {
			{"!тест члена", "хуятый хуй, опизденные яйца"}, {"!тест", "тест кого? я тут один"},
		};

		public TwitchIRCClient()
		{
			client = new TcpClient(TwitchInit.Host, TwitchInit.port);
			reader = new StreamReader(client.GetStream());
			writer = new StreamWriter(client.GetStream());
			writer.AutoFlush = true;
			OAuthToken = File.ReadAllText("OAuthToken.ps");
		}

		public void Connect()
		{
			SendCommand("PASS", OAuthToken);
			SendCommand("USER", string.Format("{0} * 0 * {0}", TwitchInit.BotNick));
			SendCommand("NICK", TwitchInit.BotNick);
			SendCommand("JOIN", "#" + TwitchInit.ChannelName);
		}

		public void CheckCommand(string msg)
		{
			string reply = null;
			foreach (var pair in commands)
			{
				if (msg.Contains(pair.Key)) //детекцию команд нужно переписывать, 
											//иначе он замечает команды посреди сообщения, 
											//а не только в начале
				{
					reply = pair.Value;
					break;
				}
			}
			if (reply != null)
			{
				SendMessage(reply);
			}
		}

		public bool RegexCheck(string msg, string reg)
		{
			Regex regex = new Regex(reg);
			if (regex.IsMatch(msg))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public void Chat()
		{
			while (true)
			{
				string message = reader.ReadLine();
				if (message != null)
				{
					System.Console.Out.WriteLine(message);
					CheckCommand(message);
					if (message == "PING :tmi.twitch.tv")
					{
						SendCommand("PONG", ":tmi.twitch.tv");
					}
					if (RegexCheck(message, "!мамб(рес|ерс) маниф[юе]р"))
					{
						SendMessage("Ничего нет лушего выпить теплого, свежего камшота TPFufun");
					}
				}
			}
		}

		private void SendMessage(string message)
		{
			SendCommand("PRIVMSG", string.Format("#{0} :{1}", TwitchInit.ChannelName, message));
		}

		private void SendCommand(string cmd, string param)
		{
			writer.WriteLine(cmd + " " + param);
		}
	}
}