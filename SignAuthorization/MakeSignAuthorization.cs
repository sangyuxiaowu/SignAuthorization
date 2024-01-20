using System.Collections;
using System.Text;
using System.Security.Cryptography;
using System.Web;

namespace Sang.AspNetCore.SignAuthorization
{
    /// <summary>
    /// 生成签名
    /// </summary>
    public class MakeSignAuthorization
    {
        /// <summary>
        /// 生成签名
        /// </summary>
        /// <param name="sToken">Token</param>
        /// <param name="sTimeStamp">时间戳</param>
        /// <param name="sNonce">随机字符串</param>
        /// <param name="sExtra">额外参数内容</param>
        /// <param name="sPath">路径信息</param>
        /// <returns></returns>
        public static string MakeSign(string sToken, string sTimeStamp, string sNonce, string sExtra = "", string sPath = "")
        {
            var parameterList = new ArrayList
            {
                sToken,
                sTimeStamp,
                sNonce
            };
            if (!string.IsNullOrEmpty(sExtra))
            {
                parameterList.Add(sExtra);
            }
            if (!string.IsNullOrEmpty(sPath))
            {
                parameterList.Add(sPath);
            }
            // 字典排序
            parameterList.Sort(StringComparer.Ordinal);
            // 计算 SHA1
            var raw = string.Join("", parameterList.ToArray());
            using SHA1 sha1 = SHA1.Create();
            byte[] encry = sha1.ComputeHash(Encoding.UTF8.GetBytes(raw));
            string sign = string.Join("", encry.Select(b => string.Format("{0:x2}", b)).ToArray()).ToLower();
            return sign;
        }

        /// <summary>
        /// 为URL生成签名
        /// </summary>
        /// <param name="uri">URL</param>
        /// <param name="options">签名设置信息</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string MakeSignUrl(Uri uri, SignAuthorizationOptions options)
        {
            if (uri == null)
            {
                   throw new ArgumentNullException(nameof(uri));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            var sTimeStamp = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
            var sNonce = Guid.NewGuid().ToString("N");
            var sQuery = uri.IsAbsoluteUri ? uri.Query.TrimStart('?') : (uri.OriginalString.Contains('?') ? uri.OriginalString.Split('?')[1] : "");
            // 获取额外参数
            var sExtra = "";
            if (!string.IsNullOrEmpty(options.nExtra))
            {
                //获取URL中的指定参数的第一个值
                var queryDictionary = HttpUtility.ParseQueryString(sQuery);
                sExtra = queryDictionary[options.nExtra] ?? "";
                if(sExtra == "")
                {
                    // 在开启额外参数选项时，传入的签名URL信息必须包含额外参数名，否则将导致签名错误，远端无法验证
                    throw new ArgumentException("If the nExtra parameter in SignAuthorizationOptions is set, the signature URL must contain the corresponding extra parameter name. Failure to do so will result in a signature error, preventing remote verification.");
                }
            }
            // 获取路径信息
            var urlPath = uri.IsAbsoluteUri ? uri.AbsolutePath : uri.OriginalString.Split('?')[0];
            // 签名包含路径
            var sPath = options.WithPath ? urlPath : "";

            var sSign = MakeSign(options.sToken, sTimeStamp, sNonce, sExtra, sPath);

            // 非绝对路径时，需要修正协议、主机、端口
            var uriBuilder = new UriBuilder
            {
                Scheme = uri.IsAbsoluteUri ? uri.Scheme : "",
                Host = uri.IsAbsoluteUri ? uri.Host : "",
                Port = uri.IsAbsoluteUri ? uri.Port : 80,
                Path = urlPath,
                Query = sQuery
            };

            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query[options.nTimeStamp] = sTimeStamp;
            query[options.nNonce] = sNonce;
            query[options.nSign] = sSign;
            uriBuilder.Query = query.ToString();
            return uriBuilder.ToString();
        }

        /// <summary>
        /// 为URL生成签名
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="options">签名设置信息</param>
        /// <returns></returns>
        public static string MakeSignUrl(string url, SignAuthorizationOptions options)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            // 尝试创建一个 Uri 对象
            if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out Uri uri))
            {
                throw new ArgumentException("Invalid URL format.", nameof(url));
            }
            return MakeSignUrl(uri, options);
        }
    }
}
