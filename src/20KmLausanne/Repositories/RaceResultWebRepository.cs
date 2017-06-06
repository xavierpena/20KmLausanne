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
            var webResponsesByYear = GetAllWebResponsesByYear();
            var results = ParseAllWebResponses(webResponsesByYear);
            return results;
        }

        private static List<RaceResult> ParseAllWebResponses(Dictionary<int, List<string>> webResponsesByYear)
        {
            var results = new List<RaceResult>();
            foreach (var pair in webResponsesByYear)
            {
                var year = pair.Key;
                foreach (var webpageResponseStr in pair.Value)
                {
                    var partialResults = GetDataLinesFromWebResponse(webpageResponseStr);
                    List<RaceResult> partialRaceResults = ParseLines(year, partialResults);
                    results.AddRange(partialRaceResults);
                }
            }

            return results;
        }

        public static Dictionary<int, List<string>> GetAllWebResponsesByYear()
        {
            var results = new Dictionary<int, List<string>>();
            for (var year = 2017; year >= 2009; year--)
            {
                Console.WriteLine(year);
                results.Add(year, new List<string>());
                for (var letter = 'A'; letter <= 'Z'; letter++)
                {
                    Console.WriteLine(letter);
                    var webpageResponseStr = DownloadWebpageStr(year, letter);
                    results[year].Add(webpageResponseStr);
                }
            }
            return results;
        }

        public static List<RaceResult> ParseLines(int year, string[] partialResults)
        {
            var partialRaceResults = new List<RaceResult>();
            foreach (var line in partialResults.Skip(2))
            {
                var raceResult = ParseToRaceResultWithPartialRaceTimes(line, year);
                partialRaceResults.Add(raceResult);
            }

            return partialRaceResults;
        }

        /// <summary>
        /// Decodes the line from the website into a RaceResultModel.
        /// </summary>
        public static RaceResult ParseToRaceResultWithPartialRaceTimes(string line, int year)
        {
            var model = new RaceResult();

            model.year = year.ToString();

            model.categorie = line.Substring(0, 10).Trim();
            model.rang = line.Substring(11, 5).Trim().Replace(".", "");
            var nom = line.Substring(16, 35).Trim();
            var an = line.Substring(52, 4).Trim();
            model.equipe_ou_lieu = line.Substring(57, 27).Trim();
            model.temps = line.Substring(85, 8).Trim();

            model.temps_partiel_1 = "----";
            model.temps_partiel_2 = "----";

            var parts = line.Split('¦');
            if (parts.Length == 4)
            {
                model.temps_partiel_1 = parts[1].TrimStart().Split(' ')[0].Trim();
                model.temps_partiel_2 = parts[2].TrimStart().Split(' ')[0].Trim();
            }

            //if (model.temps_partiel_1 == "0:----" || model.temps_partiel_2 == "0:----")
            //    model.temps_partiel_2 = model.temps_partiel_2;

            // Remove last comma from "race time":
            model.temps = model.temps.Substring(0, model.temps.Length - 1);

            model.participant = new Participant(nom, an, model.GetGender());

            if (an != "????")
                model.age = (int.Parse(model.year) - int.Parse(an)).ToString();

            return model;
        }

        public static string DownloadWebpageStr(int year, char letter)
        {
            var url = $"https://services.datasport.com/{year}/lauf/km20/alfa{letter}.htm";
            var response = DownloadPageAsync(url).Result;
            return response;
        }

        /// <summary>
        /// Given a year and a letter, builds the URL and downloads the result list from the datasport website.
        /// </summary>
        public static string[] GetDataLinesFromWebResponse(string webResponseStr)
        {
            var index1 = webResponseStr.IndexOf("<font size=\"2\">catégorie");
            var substring = webResponseStr.Substring(index1, webResponseStr.Length - index1);

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
