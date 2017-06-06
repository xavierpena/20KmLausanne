using Lausanne20Km.Business;
using Lausanne20Km.Repositories;
using System;
using System.IO;

namespace Lausanne20Km
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var baseDataPath = args[0];
                var csvDataFileFullPath = Path.Combine(baseDataPath, "data.csv");

                //// Web acquisition:
                //var results = RaceResultWebRepository.GetAll();
                //RaceResultCsvRepository.SaveAll(csvDataFileFullPath, results);

                // Csv reader:
                var results = RaceResultCsvRepository.GetAll(csvDataFileFullPath);
                var analyzer = new Analyzer(results);

                var confidenceIntervalTimeByAgeForMen = analyzer.GetConfidenceIntervalTimeByAgeForMen(distance: 20, minDataSize: 100);
                var progressionDispersion = analyzer.GetProgressionDispersion(distance: 20, minNumberOfCompletedRaces: 5);
                var progressionSumary20km = analyzer.GetProgressionSummary(distance: 20);
                var ageGenderParticipation20km = analyzer.GetAgeGenderParticipation(distance: 20);
                var ageGenderAverageTime20km = analyzer.GetAgeGenderAverageTime(distance: 20, minDataSize: 30);

                SaveResultsToFile(baseDataPath, "confidenceIntervalTimeByAgeForMen.csv", confidenceIntervalTimeByAgeForMen);
                SaveResultsToFile(baseDataPath, "progressionDispersion2.csv", progressionDispersion);
                SaveResultsToFile(baseDataPath, "progressionSumary20km.csv", progressionSumary20km);
                SaveResultsToFile(baseDataPath, "ageGenderParticipation20km.csv", ageGenderParticipation20km);
                SaveResultsToFile(baseDataPath, "ageGenderAverageTime20km.csv", ageGenderAverageTime20km);

                Console.WriteLine("Finished.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Console.ReadKey();
        }

        private static void SaveResultsToFile(string basePath, string fileName, string fullText)
        {
            var resultFilePath = Path.Combine(basePath, "results", fileName);
            File.WriteAllText(resultFilePath, fullText);
        }
    }
}