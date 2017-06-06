using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lausanne20Km.Models;
using Lausanne20Km.Repositories;

namespace Lausanne20Km.Business
{
    public static class Analyzers
    {
        private const string TimeSpanStrFormat = "hh\\:mm\\:ss";

        public static string GetAgeGenderParticipation(List<RaceResult> raceResults, int distance)
        {
            var menResults = Shared.GetFilteredResults(raceResults, distance, Gender.Male);
            var womenResults = Shared.GetFilteredResults(raceResults, distance, Gender.Female);

            var histogramMen = menResults
                .GroupBy(x => x.age)
                .ToDictionary(x => x.Key, x => x.Count());

            var histogramWomen = womenResults
                .GroupBy(x => x.age)
                .ToDictionary(x => x.Key, x => x.Count());

            var mergedKeys = Shared.GetMergedKeys(histogramMen, histogramWomen);

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"age,num. of men,num. of women");
            foreach (var age in mergedKeys)
            {
                var key = age;
                var menValue = histogramMen.ContainsKey(key) ? histogramMen[key] : 0;
                var womenValue = histogramWomen.ContainsKey(key) ? histogramWomen[key] : 0;
                stringBuilder.AppendLine($"{key},{menValue},{womenValue}");
            }

            return stringBuilder.ToString();
        }

        public static string GetAgeGenderAverageTime(List<RaceResult> raceResults, int distance, int minDataSize)
        {
            var menResults = Shared.GetFilteredResults(raceResults, distance, Gender.Male);
            var womenResults = Shared.GetFilteredResults(raceResults, distance, Gender.Female);

            var histogramMen = menResults
                .GroupBy(x => x.age)
                .Where(x => x.Count() > minDataSize)
                .ToDictionary(
                        x => x.Key, 
                        x => x.Select(y => y.GetTotalTimeAsTimeSpan().Value.TotalMilliseconds).Average()
                    );

            var histogramWomen = womenResults
                .GroupBy(x => x.age)
                .Where(x => x.Count() > minDataSize)
                .ToDictionary(
                        x => x.Key,
                        x => x.Select(y => y.GetTotalTimeAsTimeSpan().Value.TotalMilliseconds).Average()
                    );

            var mergedKeys = Shared.GetMergedKeys(histogramMen, histogramWomen);

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"age,men,women");
            foreach (var age in mergedKeys)
            {
                var key = age;
                var menValue = histogramMen.ContainsKey(key) ? TimeSpan.FromMilliseconds(histogramMen[key]).ToString(TimeSpanStrFormat) : "";
                var womenValue = histogramWomen.ContainsKey(key) ? TimeSpan.FromMilliseconds(histogramWomen[key]).ToString(TimeSpanStrFormat) : "";
                stringBuilder.AppendLine($"{key},{menValue},{womenValue}");
            }

            return stringBuilder.ToString();
        }

        public static string GetConfidenceIntervalTimeByAgeForMen(List<RaceResult> raceResults, int distance, int minDataSize)
        {
            var menResults = Shared.GetFilteredResults(raceResults, distance, Gender.Male);

            var confidenceIntervals = new double[] { 0.05, 0.5, 0.95 };

            var allResults = new SortedDictionary<double, Dictionary<int, double>>();
            foreach(var confidenceInterval in confidenceIntervals)
            {
                var intervalResults = menResults
                    .GroupBy(x => int.Parse(x.age))
                    .Where(x => x.Count() > minDataSize)
                    .ToDictionary(
                            x => x.Key,
                            x => GetConfidenceValueAsMilliseconds(
                                    x.Select(y => y.GetTotalTimeAsTimeSpan().Value).ToList(),
                                    confidenceInterval
                                )
                        );
                allResults.Add(confidenceInterval, intervalResults);
            }

            var ages = allResults.First().Value.Keys.OrderBy(x => x);

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"age," + string.Join(",", confidenceIntervals));            
            foreach (var age in ages)
            {
                var key = age;
                stringBuilder.Append($"{age},");
                foreach (var pair in allResults)
                {
                    var intervalResults = pair.Value;
                    var intervalValue = intervalResults.ContainsKey(key) ? TimeSpan.FromMilliseconds(intervalResults[key]).ToString(TimeSpanStrFormat) : "";
                    stringBuilder.Append($"{intervalValue},");
                }
                stringBuilder.Length--; // remove last trailing comma
                stringBuilder.AppendLine();
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// For example: get the time of the participant that beat the lower 20% of all participants
        /// </summary>
        private static double GetConfidenceValueAsMilliseconds(List<TimeSpan> times, double confidenceInterval)
        {
            times.Sort();
            var length = times.Count;
            var percentilIndex = (int) Math.Truncate(confidenceInterval * (length - 1));
            return times[percentilIndex].TotalMilliseconds;
        }

        /// <summary>
        /// Progression (classified by age) for participants that have completed all races.
        /// </summary>
        public static string GetProgressionSummary(Dictionary<Participant, List<RaceResult>> participantsResults, int distance)
        {        
            var summaryPerParticipant = new Dictionary<Participant, Tuple<TimeSpan, TimeSpan>>();
            foreach (var pair in participantsResults)
            {
                var participant = pair.Key;
                var minTime = pair.Value.Min(x => x.GetTotalTimeAsTimeSpan().Value);
                var maxTime = pair.Value.Max(x => x.GetTotalTimeAsTimeSpan().Value);

                summaryPerParticipant.Add(participant, new Tuple<TimeSpan, TimeSpan>(minTime, maxTime));             
            }

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Year of birth,Time between worst and best times");
            foreach (var pair in summaryPerParticipant)
            {
                var participant = pair.Key;
                var minMaxTimes = pair.Value;
                var maxMinusMin = minMaxTimes.Item2.Subtract(minMaxTimes.Item1);
                stringBuilder.AppendLine($"{participant.YearOfBirth},{maxMinusMin.ToString()}");
            }

            return stringBuilder.ToString();
        }

        public static string GetProgressionDispersion(Dictionary<Participant, List<RaceResult>> participantsResults, int distance)
        {
            var summaryPerParticipant = new List<Tuple<TimeSpan, TimeSpan>>();
            foreach (var pair in participantsResults)
            {
                var participant = pair.Key;
                var minTime = pair.Value.Min(x => x.GetTotalTimeAsTimeSpan().Value);
                var maxTime = pair.Value.Max(x => x.GetTotalTimeAsTimeSpan().Value);

                if (minTime.TotalMinutes < 60)
                    continue;
                
                var maxMinusMin = maxTime.Subtract(minTime);

                summaryPerParticipant.Add(new Tuple<TimeSpan, TimeSpan>(maxTime, maxMinusMin));
            }

            summaryPerParticipant = summaryPerParticipant.OrderBy(x => x.Item1).ToList();

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Worst participant time,Time diff between worst and best results");
            foreach (var tuple in summaryPerParticipant)
            {
                var averageTime = tuple.Item1;
                var maxMinusMin = tuple.Item2;
                
                stringBuilder.AppendLine($"{averageTime.ToString(TimeSpanStrFormat)},{maxMinusMin.ToString(TimeSpanStrFormat)}");
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Progression (classified by age) for participants that have completed all races.
        /// </summary>
        public static string GetProgressionDetails(Dictionary<Participant, List<RaceResult>> participantsResults, int distance)
        {            
            var resultsPerAgeAndParticipant = new SortedDictionary<int, Dictionary<Participant, TimeSpan>>();
            foreach(var pair in participantsResults)
            {
                var participant = pair.Key;
                foreach(var result in pair.Value)
                {
                    var age = int.Parse(result.age);
                    var year = int.Parse(result.year);
                    var time = result.GetTotalTimeAsTimeSpan().Value;

                    if (!resultsPerAgeAndParticipant.ContainsKey(age))
                        resultsPerAgeAndParticipant.Add(age, new Dictionary<Participant, TimeSpan>());

                    resultsPerAgeAndParticipant[age].Add(participant, time);
                }
            }

            var stringBuilder = new StringBuilder();
            foreach (var pair in resultsPerAgeAndParticipant)
            {
                var age = pair.Key;
                stringBuilder.Append(age + ",");
                foreach (var participant in participantsResults.Keys)
                {
                    if(pair.Value.ContainsKey(participant))
                    {
                        var participantTime = pair.Value[participant].ToString();
                        stringBuilder.Append(participantTime);
                    }                        
                    else
                    {
                        stringBuilder.Append("");
                    }
                    stringBuilder.Append(",");
                }
                stringBuilder.AppendLine();
            }
            
            return stringBuilder.ToString();
        }

    }
}
