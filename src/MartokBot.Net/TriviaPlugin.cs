using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using IrcDotNet;

namespace MartokBot.Net
{
	public class TriviaPlugin : IPlugin
	{
		private Bot _bot;
		private readonly List<Question> _questions;
		private readonly List<Score> _scores;
		private readonly Random _random;

		private Question _currentQuestion;

		public TriviaPlugin()
		{
			_questions = new List<Question>();
			_scores = new List<Score>();

			_random = new Random();
		}

		public void InitPlugin(Bot bot)
		{
			_bot = bot;
			bot.Message += MessageReceived;

			using (var sr = new StreamReader(@"c:\Users\Administrator\Documents\source\MartokBot.Net\questions.txt"))
			{
				string line;
				while ((line = sr.ReadLine()) != null)
				{
					_questions.Add(
						new Question
							{
								Text = line.Substring(0, line.IndexOf("*")),
								Answers = line.Substring(line.IndexOf("*")+1).Split('*').Select(x => x.ToLower()),
							});
				}
			}
		}

		private void MessageReceived(IrcChannel channel, string source, string text)
		{
			if(text.StartsWith("!scores"))
				ShowScores(channel);

			if(text.StartsWith("!trivia") && _currentQuestion == null)
				StartQuestion(channel);

			if(_currentQuestion != null)
				CheckForCorrectAnswer(text, channel, source);
		}

		private void ShowScores(IrcChannel channel)
		{
			foreach(var score in _scores.OrderBy(s => s.Points).Reverse().Take(5))
			{
				_bot.Say(channel.Name, string.Format("{0}: {1} points", score.Nickname, score.Points));
			}
		}

		private void CheckForCorrectAnswer(string text, IrcChannel channel, string source)
		{
			if (_currentQuestion.Answers.Contains(text.ToLower()))
			{
				_bot.Say(channel.Name, source + " rocks!");
				_currentQuestion = null;

				AddToScores(source);
				
				if(_scores.Sum(x => x.Points) == 10)
				{
					_bot.Say(channel.Name, _scores.OrderBy(s => s.Points).Reverse().First().Nickname + " is the winner");
					ShowScores(channel);
					_scores.Clear();
				}
				else
				{
					StartQuestion(channel);
				}
			}
		}

		private void StartQuestion(IrcChannel channel)
		{
			var question = _questions[_random.Next(0, _questions.Count)];
			_bot.Say(channel.Name, question.Text);
			_currentQuestion = question;

			Action<Question, IrcChannel> delayedAction =
				delegate(Question currentQuestion, IrcChannel c)
					{
						Thread.Sleep(30000);

						if (_currentQuestion == currentQuestion)
						{
							_bot.Say(channel.Name, "Noone knew the answer - point for me!");
							StartQuestion(c);
						}
					};

			delayedAction.BeginInvoke(_currentQuestion, channel, null, null);
		}

		private void AddToScores(string nickname)
		{
			if(_scores.Any(s => s.Nickname.ToLower() == nickname.ToLower()))
				_scores.First(s => s.Nickname.ToLower() == nickname.ToLower()).Points++;
			else
				_scores.Add(new Score{Nickname = nickname, Points = 1});
		}

		private class Question
		{
			public string Text { get; set; }
			public IEnumerable<string> Answers { get; set; }
		}

		private class Score
		{
			public string Nickname { get; set; }
			public int Points { get; set; }
		}
	}
}
