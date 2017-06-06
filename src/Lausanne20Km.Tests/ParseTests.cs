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
        private const string WebResponseTestDataFile = @"..\..\..\..\..\data\web-response-2017-A.htm";
        private const string CsvDataFile  = @"..\..\..\..\..\data\data.txt";

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
            var webpageResponseStr = File.ReadAllText(WebResponseTestDataFile);
            var results = RaceResultWebRepository.GetDataLinesFromWebResponse(webpageResponseStr);
            Assert.IsTrue(results.Any());
        }

        [TestMethod]
        public void HtmlIsParsed_LocalHtmlFile_AnyResults()
        {
            var webpageResponseStr = File.ReadAllText(WebResponseTestDataFile);
            var dataLines = RaceResultWebRepository.GetDataLinesFromWebResponse(webpageResponseStr);
            var parsedResults = RaceResultWebRepository.ParseLines(2017, dataLines);
            Assert.IsTrue(dataLines.Any());
        }
    }
}
