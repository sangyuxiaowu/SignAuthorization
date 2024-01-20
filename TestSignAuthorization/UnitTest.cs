using Sang.AspNetCore.SignAuthorization;

namespace TestSignAuthorization
{
    [TestClass]
    public class UnitTest
    {
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

        [TestMethod]
        public void MakeSignUrl()
        {
            var url = MakeSignAuthorization.MakeSignUrl("http://localhost:5177", new SignAuthorizationOptions());
            Console.WriteLine(url);
            Assert.IsTrue(url.StartsWith("http://localhost:5177/?"));
        }

        [TestMethod]
        public void MakeSignUrl_Path1()
        {
            var url = MakeSignAuthorization.MakeSignUrl("/", new SignAuthorizationOptions());
            Console.WriteLine(url);
            Assert.IsTrue(url.StartsWith("/?"));
        }

        [TestMethod]
        public void MakeSignUrl_Path2()
        {
            var url = MakeSignAuthorization.MakeSignUrl("/abc", new SignAuthorizationOptions());
            Console.WriteLine(url);
            Assert.IsTrue(url.StartsWith("/abc?"));
        }

        [TestMethod]
        public void MakeSignUrl_Path3()
        {
            var url = MakeSignAuthorization.MakeSignUrl("/?ext=1", new SignAuthorizationOptions() { nExtra = "ext" });
            Console.WriteLine(url);
            Assert.IsTrue(url.StartsWith("/?ext=1&"));
        }

        [TestMethod]
        public void MakeSignUrl_Path4()
        {
            var url = MakeSignAuthorization.MakeSignUrl("/abc?ext=1", new SignAuthorizationOptions() { nExtra = "ext" });
            Console.WriteLine(url);
            Assert.IsTrue(url.StartsWith("/abc?ext=1&"));
        }

        [TestMethod]
        public void MakeSignUrl_Path5()
        {
            try
            {
                var url = MakeSignAuthorization.MakeSignUrl("/?#ext=1", new SignAuthorizationOptions() { nExtra = "ext" });
                Assert.IsTrue(false);
            }
            catch
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void MakeSignUrl_Path6()
        {
            var url = MakeSignAuthorization.MakeSignUrl("abc/?ext=1", new SignAuthorizationOptions() { nExtra = "ext" });
            Console.WriteLine(url);
            Assert.IsTrue(url.StartsWith("abc/?ext=1"));
        }
    }
}