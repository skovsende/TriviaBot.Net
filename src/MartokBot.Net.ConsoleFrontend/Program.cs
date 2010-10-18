using System;

namespace MartokBot.Net.ConsoleFrontend
{
	class Program
	{
		static void Main()
		{
			using (var bot = new Bot())
			{
				bot.AddPlugin(new TriviaPlugin());

				bot.Connect("irc.quakenet.org", "GinnTrivia", "GinnTrivia", "GinnTrivia");
				Console.WriteLine("Connected...");

				bot.Join("#ginnunga");
				bot.Message +=
					(channel, sender, text) => Console.WriteLine(string.Format("{0}/{1}: {2}", channel.Name, sender, text));

				Console.ReadKey();
			}
		}
	}
}
