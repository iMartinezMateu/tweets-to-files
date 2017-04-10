using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using Microsoft.Extensions.CommandLineUtils;
using Tweetinvi;

namespace TweetsToFiles
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var app = new CommandLineApplication();
			app.Name = "Tweets to Files";
			app.HelpOption("-?|-h|--help");

			app.OnExecute(() =>
				{
					Console.WriteLine("Tweets to Files 1.0.0");
					Console.WriteLine("Available commands:");
					Console.WriteLine("\tprompt\t\tGet the tweet text by providing its ID as an argument and save it to a file");
					Console.WriteLine("\tfile\t\tGet the tweet text by providing its ID stored in a CSV file and save it to a file");
					Console.WriteLine("Common options:");
					Console.WriteLine("\t-?|-h|--help\tShow help of the typed command");
					return 1;
				});

			app.Command("prompt", (command) =>
				{

					command.Description = "Get the tweet text by providing its ID as an argument and save it to a file";
					command.HelpOption("-?|-h|--help");

					CommandOption consumerKeyOption = command.Option("--consumer-key <consumer-key>",
																	 "To make API calls, you need a consumer key from the Twitter Applications Platform (https://apps.twitter.com).",
											  CommandOptionType.SingleValue);

					CommandOption consumerSecretOption = command.Option("--consumer-secret <consumer-secret>",
										  "To make API calls, you need a consumer secret key from the Twitter Applications Platform (https://apps.twitter.com).",
										  CommandOptionType.SingleValue);

					CommandOption accessTokenOption = command.Option("--access-token <access-token>",
										  "To make API calls, you need an access token from the Twitter Applications Platform (https://apps.twitter.com).",
										  CommandOptionType.SingleValue);

					CommandOption accessTokenSecretOption = command.Option("--access-token-secret <access-token-secret>",
										  "To make API calls, you need an access token secret key from the Twitter Applications Platform (https://apps.twitter.com).",
										  CommandOptionType.SingleValue);

					CommandOption tweetIdOption = command.Option("--tweet-id <tweet-id>",
																  "Tweet ID that is going to be issued to the API in order to get its text.",
																  CommandOptionType.MultipleValue);
				
					CommandOption timeOutOption = command.Option("--timeout", "Time in milliseconds to wait between requests.", CommandOptionType.SingleValue);

					CommandOption onlyPrintOption = command.Option("--only-print", "Only print the tweets, avoid saving them into files.", CommandOptionType.NoValue);

					command.OnExecute(() =>
						{
							if (consumerKeyOption.HasValue() == false || consumerSecretOption.HasValue() == false || accessTokenOption.HasValue() == false || accessTokenSecretOption.HasValue() == false || tweetIdOption.HasValue() == false)
							{
								Console.WriteLine(command.GetHelpText());
								return 0;
							}
							else
							{
								Auth.SetUserCredentials(consumerKeyOption.Value(), consumerSecretOption.Value(), accessTokenOption.Value(), accessTokenSecretOption.Value());
								if (onlyPrintOption.HasValue() == true)
								{
									foreach (string tweetId in tweetIdOption.Values)
									{
										var tweet = Tweet.GetTweet(Convert.ToInt64(tweetId));
										Console.WriteLine(tweet.Text);
										if (timeOutOption.HasValue() && Convert.ToInt32(timeOutOption.Value()) > 0)
										{
											Thread.Sleep(Convert.ToInt32(timeOutOption.Value()));
										}
									}
								}
								else
								{
									foreach (string tweetId in tweetIdOption.Values)
									{
										var tweet = Tweet.GetTweet(Convert.ToInt64(tweetId));
										var workingDir = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/" + tweetId + ".txt";
										using (var writer = File.CreateText(workingDir))
										{
											writer.WriteLine(tweet);
											if (timeOutOption.HasValue() && Convert.ToInt32(timeOutOption.Value()) > 0)
											{
												Thread.Sleep(Convert.ToInt32(timeOutOption.Value()));
											}
										}
										Console.WriteLine("Tweet " + tweetId + " stored successfully at " + workingDir);
									}
								}

								return 1;
							}
						});
				});


			app.Command("file", (command) =>
				{

					command.Description = "Get the tweet text by providing its ID stored in a CSV file and save it to a file";
					command.HelpOption("-?|-h|--help");

					CommandOption consumerKeyOption = command.Option("--consumer-key <consumer-key>",
																	 "To make API calls, you need a consumer key from the Twitter Applications Platform (https://apps.twitter.com).",
											  CommandOptionType.SingleValue);

					CommandOption consumerSecretOption = command.Option("--consumer-secret <consumer-secret>",
										  "To make API calls, you need a consumer secret key from the Twitter Applications Platform (https://apps.twitter.com).",
										  CommandOptionType.SingleValue);

					CommandOption accessTokenOption = command.Option("--access-token <access-token>",
										  "To make API calls, you need an access token from the Twitter Applications Platform (https://apps.twitter.com).",
										  CommandOptionType.SingleValue);

					CommandOption accessTokenSecretOption = command.Option("--access-token-secret <access-token-secret>",
										  "To make API calls, you need an access token secret key from the Twitter Applications Platform (https://apps.twitter.com).",
										  CommandOptionType.SingleValue);

					CommandOption pathOption = command.Option("--path <path>",
																  "Path of the CSV file to be processed",
																  CommandOptionType.SingleValue);

					CommandOption delimiterOption = command.Option("--delimiter <delimiter>",
												  "Column delimiter char of the CSV file",
												  CommandOptionType.SingleValue);

					CommandOption indexOption = command.Option("--index <index>",
															   "Column index of the CSV file where the tweets IDs are stored (starting from 0 if it is the first column and so on)",
															   CommandOptionType.SingleValue);

					CommandOption timeOutOption = command.Option("--timeout", "Time in milliseconds to wait between requests.", CommandOptionType.SingleValue);

					CommandOption onlyPrintOption = command.Option("--only-print", "Only print the tweets, avoid saving them into files.", CommandOptionType.NoValue);

					command.OnExecute(() =>
						{
							if (consumerKeyOption.HasValue() == false || consumerSecretOption.HasValue() == false || accessTokenOption.HasValue() == false || accessTokenSecretOption.HasValue() == false || pathOption.HasValue() == false || delimiterOption.HasValue() == false || indexOption.HasValue() == false)
							{
								Console.WriteLine(command.GetHelpText());
								return 0;
							}
							else
							{
								if (File.Exists(pathOption.Value()))
								{
									List<string> tweetIdsList = new List<string>();
									foreach (string line in File.ReadLines(pathOption.Value())) {
										char delimiter = Convert.ToChar(delimiterOption.Value());
										int index = Convert.ToInt32(indexOption.Value());
										tweetIdsList.Add(line.Split(delimiter)[index]);
									}
									if (tweetIdsList.Count > 0)
									{
										Auth.SetUserCredentials(consumerKeyOption.Value(), consumerSecretOption.Value(), accessTokenOption.Value(), accessTokenSecretOption.Value());
										if (onlyPrintOption.HasValue() == true)
										{
											foreach (string tweetId in tweetIdsList)
											{
												var tweet = Tweet.GetTweet(Convert.ToInt64(tweetId));
												if (tweet != null)
												{
													Console.WriteLine(tweet.Text);
													if (timeOutOption.HasValue() && Convert.ToInt32(timeOutOption.Value()) > 0)
													{
														Thread.Sleep(Convert.ToInt32(timeOutOption.Value()));
													}
												}
											}
										}
										else
										{
											foreach (string tweetId in tweetIdsList)
											{
												var tweet = Tweet.GetTweet(Convert.ToInt64(tweetId));
												if (tweet != null)
												{
													var workingDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/" + tweetId + ".txt";
													using (var writer = File.CreateText(workingDir))
													{
														writer.WriteLine(tweet);
														if (timeOutOption.HasValue() && Convert.ToInt32(timeOutOption.Value()) > 0)
														{
															Thread.Sleep(Convert.ToInt32(timeOutOption.Value()));
														}
													}
													Console.WriteLine("Tweet " + tweetId + " stored successfully at " + workingDir);
												}
											}
										}
										return 1;
									}
									else
									{
										Console.WriteLine("No tweets IDs were found at provided file " + pathOption.Value());
										return 0;
									}
									
								}
								else
								{
									Console.WriteLine("File " + pathOption.Value() + " not found!");
									return 0;
								}
							}
						});
				});

			app.Execute(args);
		}
	}
}
