using Pl.Sas.Core;
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
    }
}