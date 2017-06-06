using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Lausanne20Km.Repositories;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Lausanne20Km.Models;
using Lausanne20Km.Business;

namespace Lausanne20Km.Tests
{
    [TestClass]
    public class PerformanceVsHalfRaceSpeedTests
    {
        private const string BasePath = @"..\..\..\..\..\data\";
        
        private const string CsvDataFile  = BasePath + "data.csv";

        public PerformanceVsHalfRaceSpeedTests()
        {
            Directory.SetCurrentDirectory(AppContext.BaseDirectory);
        }
        
        //[Ignore]
        [TestMethod]
        public void CsvIsLoaded_CsvFile_AnyResults()
        {
            var raceResults = RaceResultCsvRepository.GetAll(CsvDataFile);

            var xyResultsMen = PerformanceVsHalfRaceSpeedAnalyzer.GetXYResults(raceResults, distance: 20, minDataSize: 200, gender: Gender.Male);
            var xyResultsWomen = PerformanceVsHalfRaceSpeedAnalyzer.GetXYResults(raceResults, distance: 20, minDataSize: 200, gender: Gender.Female);

            var menResults2017 = xyResultsMen
                .Where(x => x.Key.year == "2017" && x.Value.AgeComparativePerformanceRatio < 1)
                .ToDictionary(x => x.Key, x => x.Value );
            var womenResults2017 = xyResultsWomen
                .Where(x => x.Key.year == "2017" && x.Value.AgeComparativePerformanceRatio < 1)
                .ToDictionary(x => x.Key, x => x.Value);

            var str1 = PerformanceVsHalfRaceSpeedAnalyzer.ParseToXYColumnStr(menResults2017.Values.ToList());
            var str2 = PerformanceVsHalfRaceSpeedAnalyzer.ParseToXYColumnStr(menResults2017.Values.ToList());

            var str = str1 + str2;

            var selectedQuadrantResults = menResults2017
                .Where(x => x.Value.AgeComparativePerformanceRatio < 0.65)
                .Select(x => x.Key)
                .ToList();

            //var selectedQuadrantResults = xyResultsMen
            //    .Where(x => x.Value.AgeComparativePerformanceRatio < 0.7 && x.Value.FirstHalfVsTotalTimeSpeedRatio > 0.55)
            //    .Select(x => GetResultAsStr(x.Key))
            //    .ToList();
        }

        private static string GetResultAsStr(RaceResult raceResult)
            => $"age:{raceResult.age}, time:{raceResult.GetTotalTimeAsTimeSpan()}";

    }
}
