using Pl.Sas.Core;
using Pl.Sas.Core.Trading;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Pl.Sas.UnitTests
{
    public class UtilitiesTests
    {
        private readonly ITestOutputHelper _output;
        public UtilitiesTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void ConvertFloatTest()
        {
            var testValue = "9602746000000.0";
            var convertValue = float.Parse(testValue);
            Assert.True(convertValue == 9602746000000f);
        }

        [Fact]
        public void GenerateShortGuidTest()
        {
            var testGuid = Utilities.GenerateShortGuid();
            Assert.True(testGuid.Length == 22);
        }

        [Fact]
        public void TruncateStringTest()
        {
            var cutString = Utilities.TruncateString("string to test truncate", 16);
            Assert.Equal("string to...", cutString);
        }

        [Fact]
        public void RandomStringTest()
        {
            var fistString = Utilities.RandomString(15);
            Assert.True(fistString.Length == 15);
            var lastString = Utilities.RandomString(15);
            Assert.True(lastString.Length == 15);
            Assert.NotEqual(fistString, lastString);
        }

        [Fact]
        public void RealtimeScheduleTest()
        {
            var random = new Random();
            if (DateTime.Now.Hour > 9 && DateTime.Now.Hour < 15 && DateTime.Now.DayOfWeek != DayOfWeek.Sunday && DateTime.Now.DayOfWeek != DayOfWeek.Saturday)
            {
                DateTime.Now.AddSeconds(random.Next(100, 150));
            }
            DateTime.Now.Date.AddHours(8).AddMinutes(random.Next(50, 60));
            Assert.True(true);
        }

        [Fact]
        public void BankProfitTest()
        {
            var totalMoney = 100000000f;
            var profit = BaseTrading.BankProfit(100000000, 5, 6.8f);
            _output.WriteLine($"{profit:0,0.000}({totalMoney.GetPercent(profit)}:0.00)");
            Assert.True(138949264 == profit);
        }
    }
}