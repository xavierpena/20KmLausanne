using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Lausanne20Km.Repositories;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Lausanne20Km.Models;

namespace Lausanne20Km.Tests
{
    [TestClass]
    public class ParseTests
    {
        private const string BasePath = @"..\..\..\..\..\data\";
        
        private const string CsvDataFile  = BasePath + "data.csv";
        private const string RawWebDataDirectory = BasePath + "raw-web-data";

        private const string DataLine1 = "20-H30       2. Abay Girma                          1986 Petit-Lancy                 1:00.35,1      0.04,0      (9) diplôme foto video      3.01 ¦   30.03     1.¦  30.31     2.¦";

        public ParseTests()
        {
            Directory.SetCurrentDirectory(AppContext.BaseDirectory);
        }

        [TestMethod]
        public void DataFileExists_IOFile_Exists()
        {
            Assert.IsTrue(File.Exists(CsvDataFile));
        }

        [TestMethod]
        public void CsvIsLoaded_CsvFile_AnyResults()
        {
            var results = RaceResultCsvRepository.GetAll(CsvDataFile);
            Assert.IsTrue(results.Any());
        }

        [TestMethod]
        public void WebIsDownloaded_WebUrl_AnyResults()
        {
            var webpageResponseStr = RaceResultWebRepository.DownloadWebpageStr(2017, 'A');
            Assert.IsNotNull(webpageResponseStr);
        }

        [TestMethod]
        public void HtmlContainsData_LocalHtmlFile_AnyResults()
        {
            var filePath = GetWebResponseLocalDataFilePath(2017, 'A');
            var webpageResponseStr = File.ReadAllText(filePath);
            var results = RaceResultWebRepository.GetDataLinesFromWebResponse(webpageResponseStr);
            Assert.IsTrue(results.Any());
        }

        [TestMethod]
        public void HtmlIsParsed_LocalHtmlFile_AnyResults()
        {
            var filePath = GetWebResponseLocalDataFilePath(2017, 'A');
            var webpageResponseStr = File.ReadAllText(filePath);
            var dataLines = RaceResultWebRepository.GetDataLinesFromWebResponse(webpageResponseStr);
            var parsedResults = RaceResultWebRepository.ParseLines(2017, dataLines);
            Assert.IsTrue(dataLines.Any());
        }

        [TestMethod]
        public void AllHtmlsAreParsed_LocalHtmlFiles_AnyResults()
        {
            var allWebResponsesByYear = GetAllWebResponsesByYear();
            var raceResults = new List<RaceResult>();
            foreach(var pair in allWebResponsesByYear)
            {
                var year = pair.Key;
                foreach(var webpageResponseStr in pair.Value)
                {
                    var partialDataLines = RaceResultWebRepository.GetDataLinesFromWebResponse(webpageResponseStr);
                    var parsedResults = RaceResultWebRepository.ParseLines(year, partialDataLines);
                    raceResults.AddRange(parsedResults);
                }
            }
            Assert.IsTrue(raceResults.Any());
        }
        
        [TestMethod]
        public void LineIsParse_LocalHtmlFile_AnyResults()
        {
            var raceResult = RaceResultWebRepository.ParseToRaceResultWithPartialRaceTimes(DataLine1, 2017);
            Assert.IsNotNull(raceResult);
        }
        
        #region "Helpers"

        private static string GetWebResponseLocalDataFilePath(int year, char letter)
            => Path.Combine(RawWebDataDirectory, year.ToString(), $"{letter}.html");

        private Dictionary<int, List<string>> GetAllWebResponsesByYear()
        {
            var results = new Dictionary<int, List<string>>();
            for (var year = 2017; year >= 2009; year--)
            {
                results.Add(year, new List<string>());
                for (var letter = 'A'; letter <= 'Z'; letter++)
                {             
                    var filePath = GetWebResponseLocalDataFilePath(year, letter);
                    var webpageResponseStr = File.ReadAllText(filePath);
                    results[year].Add(webpageResponseStr);
                }
            }
            return results;
        }

        [Ignore]
        [TestMethod]
        public void DownloadAllResultPages()
        {
            for (var year = 2017; year >= 2009; year--)
            {
                Console.WriteLine(year);
                Directory.CreateDirectory(Path.Combine(RawWebDataDirectory, year.ToString()));
                for (var letter = 'A'; letter <= 'Z'; letter++)
                {
                    Console.WriteLine(letter);
                    var webpageResponseStr = RaceResultWebRepository.DownloadWebpageStr(year, letter);
                    var filePath = Path.Combine(RawWebDataDirectory, year.ToString(), $"{letter}.html");
                    File.WriteAllText(filePath, webpageResponseStr);
                }
            }
        }

        //[Ignore]
        [TestMethod]
        public void ParseAllLocalResultsAndSaveToCsv()
        {
            var allWebResponsesByYear = GetAllWebResponsesByYear();
            var raceResults = new List<RaceResult>();
            foreach (var pair in allWebResponsesByYear)
            {
                var year = pair.Key;
                foreach (var webpageResponseStr in pair.Value)
                {
                    var partialDataLines = RaceResultWebRepository.GetDataLinesFromWebResponse(webpageResponseStr);
                    var parsedResults = RaceResultWebRepository.ParseLines(year, partialDataLines);
                    raceResults.AddRange(parsedResults);
                }
            }            
            RaceResultCsvRepository.SaveAll(CsvDataFile, raceResults);
        }

        #endregion

    }
}
