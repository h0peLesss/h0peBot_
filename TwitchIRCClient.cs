using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

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
		private Dictionary<string, DateTime> cooldownDict = new Dictionary<string, DateTime> { };
		private List<string> chatBuff = new List<string>();
		private Dictionary<string, string> commands = new Dictionary<string, string>
        
        //если разместить "!тест члена" после "!тест" в ,
        //то при вызове первой команды будет срабатывать вторая
        //из-за того, что команда считывает ввод через функцию .Contains()
        //решения этой проблемы я (пока) не нашел
        //моё решение - не пользоваться этим говном 4Голова
        
        { {"!тест члена", "хуятый хуй, опизденные яйца"}, {"!тест", "тест кого? я тут один"},
	{"!мамб(рес|ерс) маниф[юе]р", "Ничего нет лушего выпить теплого, свежего камшота TPFufun"} };

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

		public void CheckCommand()
		{
			while (true)
			{
				if (chatBuff.Count == 0)
				{
					continue;
				}
				string msg = chatBuff.First();
				chatBuff.RemoveAt(0);
				string reply = null;
				if (msg.Contains("PRIVMSG"))
				{
					msg = msg.Split(":", 2, StringSplitOptions.RemoveEmptyEntries)[1];
				}
				else
				{
					continue;
				}
				if (RegexMatch(msg, "^!ра си я"))
				{
					string[] rus = { "РА", "СИ", "Я" };
					SendMessage(rus, 1000);
					continue;
				}
				else if (RegexMatch(msg, "^!слава украине"))
				{
					string[] ukr = {
					"/color DodgerBlue", "ГЕРОЯМ",
					"/color GoldenRod", "СЛАВА",
					"/color BlueViolet"};
					SendMessage(ukr);
					continue;
				}
				else if (RegexMatch(msg, "^!ебаный задрот"))
				{
					if (!CooldownElapsed("rules", 30))
					{
						continue;
					}
					string[] rules = {
					"В ЧАТЕ ЗАПРЕЩЕНЫ:",
					"НЕГРЫ", "ПИДАРАСЫ",
					"ЧИН-ЧОНГИ", "Ж*НЩИНЫ",
					"УКРОПЫ","ЖИДЫ",
					"ФУРРИ", "НАЗАРЫ" };
					SendMessage(rules, 1000);
					cooldownDict["rules"] = DateTime.Now;
					continue;
				}
				foreach (var pair in commands)
				{
					if (RegexMatch(msg, "^" + pair.Key))
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
		}

		public void Chat()
		{
			while (true)
			{
				string message = reader.ReadLine();
				if (message != null)
				{
					Console.WriteLine(message);
					chatBuff.Add(message);
					if (message == "PING :tmi.twitch.tv")
					{
						SendCommand("PONG", ":tmi.twitch.tv");
					}
				}
			}
		}

		private bool RegexMatch(string msg, string reg)
		{
			Regex regex = new Regex("(?i)" + reg); //(?i) игнорирует регистр букв
			if (regex.IsMatch(msg))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		private bool CooldownElapsed(string key, int cooldown, string cooldownMsg = "Данная команда будет доступна через {0} сек")
		{
			if (cooldownDict.ContainsKey(key))
			{
				var i = (DateTime.Now - cooldownDict[key]).TotalSeconds;
				if (i < cooldown)
				{
					SendMessage(string.Format(cooldownMsg, Math.Round(cooldown - i, 1)));
					return false;
				}
				else
				{
					return true;
				}
			}
			else
			{
				cooldownDict.Add(key, DateTime.Now);
				return true;
			}
		}

		private void SendMessage(string message)
		{
			SendCommand("PRIVMSG", string.Format("#{0} :{1}", TwitchInit.ChannelName, message));
		}

		private void SendMessage(string[] messages, int delay = 0)
		{
			for (int i = 0; i < messages.Length; i++)
			{
				SendCommand("PRIVMSG", string.Format("#{0} :{1}", TwitchInit.ChannelName, messages[i]));
				Thread.Sleep(delay);
			}
		}

		private void SendCommand(string cmd, string param)
		{
			writer.WriteLine(cmd + " " + param);
		}
	}
}
