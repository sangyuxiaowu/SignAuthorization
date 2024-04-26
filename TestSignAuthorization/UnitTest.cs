using Sang.AspNetCore.SignAuthorization;

namespace TestSignAuthorization
{
    [TestClass]
    public class UnitTest
    {
        private void AssertUrlStartsWith(string url, string expectedStart)
        {
            Console.WriteLine(url);
            Assert.IsTrue(url.StartsWith(expectedStart));
        }

        [TestMethod]
        public void MakeSign()
        {
            var unixTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
            var sNonce = Guid.NewGuid().ToString("N");
            var sToken = "you-api-token";
            var sPath = "/weatherforecast";
            var sExtra = "1"; // extra parameter: extra=1
            string sign = MakeSignAuthorization.MakeSign(sToken, unixTimestamp, sNonce, sPath, sExtra);
            Console.WriteLine(sign);
            Assert.IsTrue(sign.Length == 40);
        }

        [DataTestMethod]
        [DataRow("http://localhost:5177", "http://localhost:5177/?", null)]
        [DataRow("/", "/?", null)]
        [DataRow("/abc", "/abc?", null)]
        [DataRow("/?ext=1", "/?ext=1&", "ext")]
        [DataRow("/abc?ext=1", "/abc?ext=1&", "ext")]
        [DataRow("abc/?ext=1", "abc/?ext=1", "ext")]
        public void MakeSignUrl(string inputUrl, string expectedStart, string extra)
        {
            var url = MakeSignAuthorization.MakeSignUrl(inputUrl, new SignAuthorizationOptions() { nExtra = extra });
            AssertUrlStartsWith(url, expectedStart);
        }

        [TestMethod]
        public void MakeSignUrl_InvalidUrl()
        {
            try
            {
                var url = MakeSignAuthorization.MakeSignUrl("/?#ext=1", new SignAuthorizationOptions() { nExtra = "ext" });
                Assert.Fail("Expected an exception to be thrown");
            }
            catch
            {
                Assert.IsTrue(true);
            }
        }
    }
}