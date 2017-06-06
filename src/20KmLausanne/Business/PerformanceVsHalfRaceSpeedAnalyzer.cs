using Lausanne20Km.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lausanne20Km.Business
{
    public static class PerformanceVsHalfRaceSpeedAnalyzer
    {
        public static string GetXYResults(List<RaceResult> raceResults, int distance, int minDataSize)
        {
            var xyResultsMen = GetXYResults(raceResults, distance, minDataSize, Gender.Male);
            var xyResultsWomen = GetXYResults(raceResults, distance, minDataSize, Gender.Female);

            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("(personalPerformance/avgAgePerformance) ratio,(firstHalfRaceTime/totalRaceTime) ratio, ");
            stringBuilder.Append(ParseToXYColumnStr(xyResultsMen.Values.ToList()));
            stringBuilder.Append(ParseToXYColumnStr(xyResultsWomen.Values.ToList()));
            
            return stringBuilder.ToString();
        }

        public static string ParseToXYColumnStr(List<ComparativePerformanceResult> comparativePerformanceResults)
        {
            var stringBuilder = new StringBuilder();
            foreach (var xy in comparativePerformanceResults)
                stringBuilder.AppendLine($"{xy.AgeComparativePerformanceRatio},{xy.FirstHalfVsTotalTimeSpeedRatio}");
            return stringBuilder.ToString();
        }

        public static Dictionary<RaceResult, ComparativePerformanceResult> GetXYResults(List<RaceResult> raceResults, int distance, int minDataSize, Gender gender)
        {
            var genderFilteredResults = Shared.GetFilteredResults(raceResults, distance, gender);

            var genderFilteredHistogram = genderFilteredResults
                .GroupBy(x => x.age)
                .Where(x => x.Count() > minDataSize)
                .ToDictionary(
                        x => x.Key,
                        x => x.Select(y => y.GetTotalTimeAsTimeSpan().Value.TotalMilliseconds).Average()
                    );

            var xyResults = GetXYResults(genderFilteredResults, genderFilteredHistogram);
            return xyResults;
        }

        private static Dictionary<RaceResult, ComparativePerformanceResult> GetXYResults(List<RaceResult> raceResults, Dictionary<string, double> genderHistorgram)
        {
            var xyResults = new Dictionary<RaceResult, ComparativePerformanceResult>();
            foreach (var raceResult in raceResults)
            {
                var tempsPartiel1 = RaceResult.ParseAsTimeSpan(raceResult.temps_partiel_1);
                var tempsPartiel2 = RaceResult.ParseAsTimeSpan(raceResult.temps_partiel_2);

                if (genderHistorgram.ContainsKey(raceResult.age)
                    && tempsPartiel1.HasValue
                    && tempsPartiel2.HasValue)
                {
                    var totalTime = raceResult.GetTotalTimeAsTimeSpan();
                    var ageAverageTimeInMilliseconds = genderHistorgram[raceResult.age];
                    var ageAverageTime = TimeSpan.FromMilliseconds(ageAverageTimeInMilliseconds);
                    
                    var firstHalfVsTotalTimeSpeedRatio = tempsPartiel1.Value.TotalMilliseconds / totalTime.Value.TotalMilliseconds;
                    var ageComparativePerformanceRatio = totalTime.Value.TotalMilliseconds / ageAverageTime.TotalMilliseconds;

                    var xyResult = new ComparativePerformanceResult(ageComparativePerformanceRatio, firstHalfVsTotalTimeSpeedRatio);
                    xyResults.Add(raceResult, xyResult);
                }
            }
            return xyResults;
        }
    }

    public class ComparativePerformanceResult
    {
        public double AgeComparativePerformanceRatio { get; set; }
        public double FirstHalfVsTotalTimeSpeedRatio { get; set; }

        public ComparativePerformanceResult(double ageComparativePerformanceRatio, double secondHalfVsTotalTimeSpeedRatio)
        {
            this.AgeComparativePerformanceRatio = ageComparativePerformanceRatio;
            this.FirstHalfVsTotalTimeSpeedRatio = secondHalfVsTotalTimeSpeedRatio;
        }
    }
}
