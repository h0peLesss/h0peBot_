using System;
using System.IO;
using System.Collections.Generic;

namespace h0peBot_
{

	public class Program
	{
		static void Main(string[] args)
		{
			TwitchIRCClient client = new TwitchIRCClient();
			client.Connect();
			client.Chat();
		}
	}
}