using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PokemonGoFlairExtract
{
	public class Program
	{
		private static void Main(string[] args)
		{
			using (WebClient client = new WebClient())
			{
				string teamName = "plain";//also possible instinct,mystic,valor
				string url = "https://www.reddit.com/r/pokemongo/wiki/flair/" + teamName;

				string htmlCode = client.DownloadString(url);
				string splitAfter = "**HINT: USE CTRL+F TO SEARCH FOR YOUR FAVORITE POKEMON!**";

				string pokemonNames = htmlCode.Split(new string[] { splitAfter }, StringSplitOptions.RemoveEmptyEntries).Last();

				List<string> rows = pokemonNames.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();

				int currentGeneration = 0;

				string pokemonList = "";
				foreach (var row in rows)
				{
					if (row.StartsWith("#"))
					{
						//Generation number

						//Get the number out of the string
						var genNumber = Regex.Match(row, @"\d+").Value;

						bool canParse = int.TryParse(genNumber, out currentGeneration);
						if (canParse == true)
						{
							if (!string.IsNullOrWhiteSpace(pokemonList))
							{
								pokemonList = pokemonList.TrimEnd(',');
								pokemonList += "]},";//End Last Generation block
							}

							pokemonList += "{";
							pokemonList += "name:" + "\"Generation " + genNumber + "\"" + ",";
							pokemonList += "id:" + genNumber + ",";
							pokemonList += "pokemonList:[";
						}
					}
					else if (row.StartsWith("["))
					{
						//Pokemon Name
						string pokemonName = row.Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries).First();

						//Example pokemon row: [Bulbasaur](https://www.reddit.com/message/compose/?to=PokemonGoFlairs&amp;subject=plainflair-1&amp;message=Flair%20Text)
						string pokemonCssIdentifier = row.Split(new string[] { teamName + "flair-", "&" }, StringSplitOptions.RemoveEmptyEntries)[2];

						bool isSpecial = false;//Needed for Json

						pokemonList += "{";
						pokemonList += "name:" + "\"" + pokemonName + "\"" + ",";
						pokemonList += "css:" + "\"" + pokemonCssIdentifier + "\"" + ",";
						pokemonList += "gen:" + currentGeneration + ",";
						pokemonList += "special:" + isSpecial.ToString().ToLower();
						pokemonList += "},";
					}
				}

				pokemonList = pokemonList.TrimEnd(',');
				pokemonList += "]}";//End Last Generation block

				File.WriteAllText(Environment.CurrentDirectory + "\\PokemonGoList.txt", pokemonList);

				Console.WriteLine("Press ENTER to end.");
				Console.ReadLine();

			}
		}
	}
}
