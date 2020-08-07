using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace h0peBot_
{

	public class Program
	{
		static void Main(string[] args)
		{
			TwitchIRCClient client = new TwitchIRCClient();
			client.Connect();
			Task.Run(() => client.CheckCommand());
			client.Chat();
		}
	}
}