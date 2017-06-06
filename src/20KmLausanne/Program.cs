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
            Directory.SetCurrentDirectory(AppContext.BaseDirectory);

            try
            {
                var baseDataPath = @"..\..\..\..\..\data";
                var csvDataFileFullPath = Path.Combine(baseDataPath, "data.csv");

                //// Web acquisition:
                //var results = RaceResultWebRepository.GetAll();
                //RaceResultCsvRepository.SaveAll(csvDataFileFullPath, results);

                // Csv reader:
                var raceResults = RaceResultCsvRepository.GetAll(csvDataFileFullPath);                
                var participantRepository = new ParticipantRepository(raceResults);

                var participantsResults9CompletedRaces = participantRepository.GetAllCompletedNRaces(distance: 20, minNumberOfCompletedRaces: 9);
                var participantsResults5CompletedRaces = participantRepository.GetAllCompletedNRaces(distance: 20, minNumberOfCompletedRaces: 5);

                var confidenceIntervalTimeByAgeForMen = Analyzers.GetConfidenceIntervalTimeByAgeForMen(raceResults, distance: 20, minDataSize: 100);
                var progressionDispersion = Analyzers.GetProgressionDispersion(participantsResults5CompletedRaces, distance: 20);
                var progressionSumary20km = Analyzers.GetProgressionSummary(participantsResults9CompletedRaces, distance: 20);
                var ageGenderParticipation20km = Analyzers.GetAgeGenderParticipation(raceResults, distance: 20);
                var ageGenderAverageTime20km = Analyzers.GetAgeGenderAverageTime(raceResults, distance: 20, minDataSize: 30);
                var part1VsPart2Dispersion = PerformanceVsHalfRaceSpeedAnalyzer.GetXYResults(raceResults, distance: 20, minDataSize: 100);

                SaveResultsToFile(baseDataPath, "confidenceIntervalTimeByAgeForMen.csv", confidenceIntervalTimeByAgeForMen);
                SaveResultsToFile(baseDataPath, "progressionDispersion2.csv", progressionDispersion);
                SaveResultsToFile(baseDataPath, "progressionSumary20km.csv", progressionSumary20km);
                SaveResultsToFile(baseDataPath, "ageGenderParticipation20km.csv", ageGenderParticipation20km);
                SaveResultsToFile(baseDataPath, "ageGenderAverageTime20km.csv", ageGenderAverageTime20km);
                SaveResultsToFile(baseDataPath, "performanceVsHalfRaceSpeed.csv", part1VsPart2Dispersion);

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