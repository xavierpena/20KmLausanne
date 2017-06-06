using Lausanne20Km.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lausanne20Km.Business
{
    public static class Shared
    {
        public static List<T1> GetMergedKeys<T1, T2>(Dictionary<T1, T2> histogramMen, Dictionary<T1, T2> histogramWomen)
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

        public static List<RaceResult> GetFilteredResults(List<RaceResult> results, int distanceId, Gender gender)
            => results
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
