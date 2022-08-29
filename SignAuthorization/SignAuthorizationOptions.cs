namespace Sang.AspNetCore.SignAuthorization
{
    /// <summary>
    /// SignAuthorization 中间件配置参数
    /// </summary>
    public class SignAuthorizationOptions
    {
        /// <summary>
        /// 认证失败的返回
        /// </summary>
        public object UnauthorizedBack { get; set; } = new
        {
            success = false,
            status = 10000,
            msg = "Unauthorized"
        };

        /// <summary>
        /// 组件的Token
        /// </summary>
        public string sToken { get; set; } = "SignAuthorizationMiddleware";

        /// <summary>
        /// 签名时需要包含请求的路径，以 '/' 开头
        /// </summary>
        public bool WithPath { get; set; } = false;

        /// <summary>
        /// 超时时间，超过该时间认为签名过期（单位：秒）
        /// </summary>
        public double TimeOut { get; set; } = 5;

        /// <summary>
        /// 时间戳的GET参数名
        /// </summary>
        public string nTimeStamp { get; set; } = "timestamp";

        /// <summary>
        /// 随机数的GET参数名
        /// </summary>
        public string nNonce { get; set; } = "nonce";

        /// <summary>
        /// 签名的GET参数名
        /// </summary>
        public string nSign { get; set; } = "signature";
    }

}