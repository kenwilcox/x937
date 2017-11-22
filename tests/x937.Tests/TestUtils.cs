using System;
using System.IO;
using Xunit;

namespace x937.Tests
{
    // https://xunit.github.io/docs/shared-context.html
    // specifically
    // https://xunit.github.io/docs/shared-context.html#class-fixture

    public class TestUtilsFixture : IDisposable
    {
        public TestUtilsFixture()
        {
            ValidICLFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"101Bank Of America20130218.ICL");
            NewICLFile = ValidICLFile.Replace(".ICL", "New.ICL");
            InvalidICLFile = ValidICLFile.Replace(".ICL", "Invalid.ICL");

            File.WriteAllBytes(ValidICLFile, GetTestICLFile());
        }

        public void Dispose()
        {
            File.Delete(ValidICLFile);
            File.Delete(NewICLFile);
            File.Delete(InvalidICLFile);
        }

        public string ValidICLFile { get; }
        public string NewICLFile { get; }
        public string InvalidICLFile { get; }

        private static byte[] GetTestICLFile()
        {
            var data = Properties.Resources.ResourceManager.GetString("ValidICLFile");
            return Convert.FromBase64String(data);
        }
    }

    [CollectionDefinition("ICL Files")]
    public class TestUtilsCollection : ICollectionFixture<TestUtilsFixture>
    {
        // No code, never created, placeholder for the attribute only
    }

    [Collection("ICL Files")]
    public class TestUtils
    {
        private readonly TestUtilsFixture _fixture;

        public TestUtils(TestUtilsFixture fixture)
        {
            _fixture = fixture;


            var recs = Parser.ParseX9File(_fixture.ValidICLFile);
            Program.WriteX9File(recs, _fixture.NewICLFile);
        }

        [Fact]
        public void Test_Something()
        {
            var recs = Parser.ParseX9File(_fixture.NewICLFile);
            // figure out what to remove...
            recs.Remove(0);
            Program.WriteX9File(recs, _fixture.InvalidICLFile);

            Assert.True(true);
        }

    }
}
