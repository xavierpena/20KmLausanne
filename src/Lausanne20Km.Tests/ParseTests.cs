using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Lausanne20Km.Repositories;
using System.IO;
using System.Linq;

namespace Lausanne20Km.Tests
{
    [TestClass]
    public class ParseTests
    {
        private const string BasePath = @"..\..\..\..\..\data\";
        
        private const string CsvDataFile  = BasePath + "data.txt";
        private const string RawWebDataDirectory = BasePath + "raw-web-data";

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
            var filePath = GetTestWebResponseDataFile();
            var webpageResponseStr = File.ReadAllText(filePath);
            var results = RaceResultWebRepository.GetDataLinesFromWebResponse(webpageResponseStr);
            Assert.IsTrue(results.Any());
        }

        [TestMethod]
        public void HtmlIsParsed_LocalHtmlFile_AnyResults()
        {
            var filePath = GetTestWebResponseDataFile();
            var webpageResponseStr = File.ReadAllText(filePath);
            var dataLines = RaceResultWebRepository.GetDataLinesFromWebResponse(webpageResponseStr);
            var parsedResults = RaceResultWebRepository.ParseLines(2017, dataLines);
            Assert.IsTrue(dataLines.Any());
        }        

#region "Helpers"

        private static string GetTestWebResponseDataFile()
            => Path.Combine(RawWebDataDirectory, "2017", $"A.html");

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

#endregion

    }
}
