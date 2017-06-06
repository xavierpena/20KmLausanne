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
    public static class RaceResultCsvRepository
    {
        private const char Separator = ';';

        public static List<RaceResult> GetAll(string fileFullPath)
        {
            var lines = File.ReadAllLines(fileFullPath);
            return lines
                .Select(x => ParseToRaceResult(x))
                .ToList();
        }

        public static void SaveAll(string fileFullPath, List<RaceResult> results)
        {
            var resultsStrArray = results.Select(x => ParseToCsvLine(x)).ToArray();
            var resultsStr = string.Join("\r\n", resultsStrArray);
            File.WriteAllText(fileFullPath, resultsStr);
        }

        private static string ParseToCsvLine(RaceResult m)
            => string.Join(
                    Separator.ToString(), 
                    new string[] 
                    {
                        m.year,
                        m.categorie,
                        m.rang,
                        m.participant.FullName,
                        m.participant.YearOfBirth,
                        m.age,
                        m.equipe_ou_lieu.Replace(Separator, '$'), // to avoid csv read-write misstakes 
                        m.temps,
                        m.temps_partiel_1,
                        m.temps_partiel_2
                    }
                );
        
        private static RaceResult ParseToRaceResult(string line)
        {
            var cells = line.Split(Separator);
            var raceResult = new RaceResult
            {
                year = cells[0],
                categorie = cells[1],
                rang = cells[2],                
                age = cells[5],
                equipe_ou_lieu = cells[6],
                temps = cells[7],
                temps_partiel_1 = cells[8],
                temps_partiel_2 = cells[9]
            };

            var nom = cells[3];
            var an = cells[4];

            raceResult.participant = new Participant(nom, an, raceResult.GetGender());

            return raceResult;
        }
        
    }
}
