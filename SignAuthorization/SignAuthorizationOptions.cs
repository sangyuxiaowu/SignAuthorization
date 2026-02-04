namespace Sang.AspNetCore.SignAuthorization
{
    /// <summary>
    /// SignAuthorization 中间件配置参数
    /// </summary>
    public class SignAuthorizationOptions
    {
        /// <summary>
        /// 认证失败的返回（对象形式）
        /// </summary>
        public object UnauthorizedBack { get; set; } = new
        {
            success = false,
            status = 10000,
            msg = "Unauthorized"
        };

        /// <summary>
        /// 认证失败的返回（已序列化的 JSON 字符串）。
        /// 设置后将直接写入响应，避免运行时序列化以支持 AOT。
        /// </summary>
        public string? UnauthorizedBackJson { get; set; }

        /// <summary>
        /// 认证失败时返回的状态码，默认 401。
        /// </summary>
        public int UnauthorizedStatusCode { get; set; } = 401;

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
        public double Expire { get; set; } = 5;

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

        /// <summary>
        /// 额外的参数名，签名时将额外添加该参数名的参数值
        /// </summary>
        public string nExtra { get; set; } = "";

        /// <summary>
        /// 是否使用HTTP头的参数来进行鉴权，默认为false
        /// </summary>
        public bool UseHeader { get; set; } = false;

        /// <summary>
        /// 验证配置信息
        /// </summary>
        /// <exception cref="ArgumentException">配置错误信息</exception>
        public void Validate()
        {
            if (string.IsNullOrEmpty(sToken))
            {
                throw new ArgumentException("sToken cannot be null or empty.");
            }

            if (Expire <= 0)
            {
                throw new ArgumentException("Expire must be greater than 0.");
            }
        }
    }

}