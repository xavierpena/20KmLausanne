using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lausanne20Km.Models;
using Lausanne20Km.Repositories;

namespace Lausanne20Km.Business
{
    public class Analyzer
    {
        private const string TimeSpanStrFormat = "hh\\:mm\\:ss";

        private List<RaceResult> _results;
        private ParticipantRepository _participantRepository;

        public Analyzer(List<RaceResult> results)
        {
            _results = results;
            _participantRepository = new ParticipantRepository(_results);
        }

        public string GetAgeGenderParticipation(int distance)
        {
            var menResults = GetFilteredResults(distance, Gender.Male);
            var womenResults = GetFilteredResults(distance, Gender.Female);

            var histogramMen = menResults
                .GroupBy(x => x.age)
                .ToDictionary(x => x.Key, x => x.Count());

            var histogramWomen = womenResults
                .GroupBy(x => x.age)
                .ToDictionary(x => x.Key, x => x.Count());

            var mergedKeys = GetMergedKeys(histogramMen, histogramWomen);

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

        public string GetAgeGenderAverageTime(int distance, int minDataSize)
        {
            var menResults = GetFilteredResults(distance, Gender.Male);
            var womenResults = GetFilteredResults(distance, Gender.Female);

            var histogramMen = menResults
                .GroupBy(x => x.age)
                .Where(x => x.Count() > minDataSize)
                .ToDictionary(
                        x => x.Key, 
                        x => x.Select(y => y.GetTimeSpan().Value.TotalMilliseconds).Average()
                    );

            var histogramWomen = womenResults
                .GroupBy(x => x.age)
                .Where(x => x.Count() > minDataSize)
                .ToDictionary(
                        x => x.Key,
                        x => x.Select(y => y.GetTimeSpan().Value.TotalMilliseconds).Average()
                    );

            var mergedKeys = GetMergedKeys(histogramMen, histogramWomen);

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

        public string GetConfidenceIntervalTimeByAgeForMen(int distance, int minDataSize)
        {
            var menResults = GetFilteredResults(distance, Gender.Male);

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
                                    x.Select(y => y.GetTimeSpan().Value).ToList(),
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
        private double GetConfidenceValueAsMilliseconds(List<TimeSpan> times, double confidenceInterval)
        {
            times.Sort();
            var length = times.Count;
            var percentilIndex = (int) Math.Truncate(confidenceInterval * (length - 1));
            return times[percentilIndex].TotalMilliseconds;
        }

        /// <summary>
        /// Progression (classified by age) for participants that have completed all races.
        /// </summary>
        public string GetProgressionSummary(int distance)
        {
            var participantsResults = _participantRepository.GetAllCompletedNRaces(distance, minNumberOfCompletedRaces: 9);

            var summaryPerParticipant = new Dictionary<Participant, Tuple<TimeSpan, TimeSpan>>();
            foreach (var pair in participantsResults)
            {
                var participant = pair.Key;
                var minTime = pair.Value.Min(x => x.GetTimeSpan().Value);
                var maxTime = pair.Value.Max(x => x.GetTimeSpan().Value);

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

        public string GetProgressionDispersion(int distance, int minNumberOfCompletedRaces)
        {
            var participantsResults = _participantRepository.GetAllCompletedNRaces(distance, minNumberOfCompletedRaces);

            var summaryPerParticipant = new List<Tuple<TimeSpan, TimeSpan>>();
            foreach (var pair in participantsResults)
            {
                var participant = pair.Key;
                var minTime = pair.Value.Min(x => x.GetTimeSpan().Value);
                var maxTime = pair.Value.Max(x => x.GetTimeSpan().Value);

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
        public string GetProgressionDetails(int distance)
        {
            var participantsResults = _participantRepository.GetAllCompletedNRaces(distance, minNumberOfCompletedRaces: 9);

            var resultsPerAgeAndParticipant = new SortedDictionary<int, Dictionary<Participant, TimeSpan>>();
            foreach(var pair in participantsResults)
            {
                var participant = pair.Key;
                foreach(var result in pair.Value)
                {
                    var age = int.Parse(result.age);
                    var year = int.Parse(result.year);
                    var time = result.GetTimeSpan().Value;

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

        private static List<T1> GetMergedKeys<T1, T2>(Dictionary<T1, T2> histogramMen, Dictionary<T1, T2> histogramWomen)
        {
            var menKeys = histogramMen.Keys.ToList();
            var womenKeys = histogramWomen.Keys.ToList();

            var totalKeys = menKeys;
            totalKeys.AddRange(womenKeys);
            totalKeys = totalKeys
                .Distinct()
                .OrderBy(x => x).ToList();

            return totalKeys;
        }

        private List<RaceResult> GetFilteredResults(int distanceId, Gender gender)
            => _results
                .Where( 
                    x => 
                        x.IsDistance(distanceId) 
                        && x.participant.Gender == gender 
                        && x.IsValidAge()
                        && x.IsValidTime()
                    )
                .ToList();

    }
}
