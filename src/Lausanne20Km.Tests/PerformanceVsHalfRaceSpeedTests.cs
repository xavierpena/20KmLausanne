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
        
        [Ignore]
        [TestMethod]
        public void CsvIsLoaded_CsvFile_AnyResults()
        {
            var raceResults = RaceResultCsvRepository.GetAll(CsvDataFile);

            var xyResultsMen = PerformanceVsHalfRaceSpeedAnalyzer.GetXYResults(raceResults, distance: 20, minDataSize: 100, gender: Gender.Male);

            var str = PerformanceVsHalfRaceSpeedAnalyzer.ParseToXYColumnStr(xyResultsMen.Values.ToList());

            var selectedQuadrantResults = xyResultsMen
                .Where(x => x.Value.AgeComparativePerformanceRatio < 0.7 && x.Value.FirstHalfVsTotalTimeSpeedRatio > 0.55)
                .Select(x => GetResultAsStr(x.Key))
                .ToList();
        }

        private static string GetResultAsStr(RaceResult raceResult)
            => $"age:{raceResult.age}, time:{raceResult.GetTotalTimeAsTimeSpan()}";

    }
}
