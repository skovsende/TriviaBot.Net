using System;
using System.Collections.Generic;
using System.IO;

namespace MartokBot.Net.ConsoleFrontend
{
	class Program
	{
		static void Main()
		{
			using (var bot = new Bot())
			{
				var trivia = new TriviaPlugin(bot);
				trivia.Init();

				bot.Connect("irc.quakenet.org", "ErnestoRules", "ErnestoRules", "ErnestoRules");
				Console.WriteLine("Connected...");

				bot.Join("#ginnunga");
				bot.Message +=
					(channel, sender, text) => Console.WriteLine(string.Format("{0}/{1}: {2}", channel.Name, sender, text));

				Console.ReadKey();
			}
		}
	}
}
