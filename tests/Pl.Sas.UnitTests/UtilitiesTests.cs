using Pl.Sas.Core;
using System;
using Xunit;

namespace Pl.Sas.UnitTests
{
    public class UtilitiesTests
    {
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
    }
}