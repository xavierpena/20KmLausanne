using Lausanne20Km.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Lausanne20Km.Repositories
{
    public static class RaceResultWebRepository
    {
        public static List<RaceResult> GetAll()
        {
            var results = new List<RaceResult>();
            for(var year = 2017; year >= 2009; year--)
            {
                Console.WriteLine(year);
                for (var letter = 'A'; letter <= 'Z'; letter++)
                {
                    Console.WriteLine(letter);
                    var partialResults = GetResultsFromWebPage(year, letter);                    
                    foreach(var line in partialResults.Skip(2))
                    {
                        var raceResult = ParseToRaceResult(line, year);
                        results.Add(raceResult);
                    }
                }
            }
            return results;
        }

        /// <summary>
        /// Decodes the line from the website into a RaceResultModel.
        /// </summary>
        private static RaceResult ParseToRaceResult(string line, int year)
        {
            var model = new RaceResult();

            model.year = year.ToString();

            model.categorie = line.Substring(0, 10).Trim();
            model.rang = line.Substring(11, 5).Trim().Replace(".", "");
            var nom = line.Substring(16, 35).Trim();
            var an = line.Substring(52, 4).Trim();
            model.equipe_ou_lieu = line.Substring(57, 27).Trim();
            model.temps = line.Substring(85, 8).Trim();

            // Remove last comma from "race time":
            model.temps = model.temps.Substring(0, model.temps.Length - 1);

            model.participant = new Participant(nom, an, model.GetGender());

            if (an != "????")
                model.age = (int.Parse(model.year) - int.Parse(an)).ToString();

            return model;
        }

        /// <summary>
        /// Given a year and a letter, builds the URL and downloads the result list from the datasport website.
        /// </summary>
        private static string[] GetResultsFromWebPage(int year, char letter)
        {
            var url = $"https://services.datasport.com/{year}/lauf/km20/alfa{letter}.htm";
            var response = DownloadPageAsync(url).Result;

            var index1 = response.IndexOf("<font size=\"2\">catégorie");
            var substring = response.Substring(index1, response.Length - index1);

            var substringWithoutHtml = StripHTML(substring);

            var lines = substringWithoutHtml.Split(new string[] { "\r\n" }, StringSplitOptions.None);

            var str = string.Join("\r\n", lines);

            //var dataLines = lines.Skip(2).Take(lines.Length - 2 - 5).ToArray();
            var dataLines = lines.Take(lines.Length - 5).ToArray();

            return dataLines;
        }

        /// <summary>
        /// Removes all html tags from the text.
        /// </summary>
        private static string StripHTML(string input)
            => Regex.Replace(input, "<.*?>", String.Empty);            

        /// <summary>
        /// Downloads the source code of a website as a string.
        /// </summary>
        private static async Task<string> DownloadPageAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage response = await client.GetAsync(url))
            using (HttpContent content = response.Content)
            {
                // ... Read the string.
                var result = await content.ReadAsStringAsync();
                return result;
            }
        }
    }
}
