using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Sang.AspNetCore.SignAuthorization
{
    /// <summary>
    /// CookieAuthorization 中间件配置参数
    /// </summary>
    public class CookieAuthorizationOptions
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
        public string sToken { get; set; } = "CookieAuthorizationMiddleware";

        /// <summary>
        /// Cookie的字段名
        /// </summary>
        public string CookieName { get; set; } = "SignAuthorization";

        /// <summary>
        /// Cookie 值分隔符
        /// </summary>
        public string CookieSeparator { get; set; } = "|";

        /// <summary>
        /// 超时时间，超过该时间认为签名过期（单位：秒）
        /// </summary>
        public double Expire { get; set; } = 3600;

        /// <summary>
        /// 是否复用有效期，启用后每次验证成功都会刷新Cookie时间戳
        /// </summary>
        public bool ReuseExpire { get; set; } = true;

        /// <summary>
        /// Cookie选项配置
        /// </summary>
        public CookieOptions CookieOptions { get; set; } = new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true
        };

        /// <summary>
        /// UserName 的 Claim 类型
        /// </summary>
        public string UserNameClaimType { get; set; } = ClaimTypes.Name;

        /// <summary>
        /// UserName 的存储字段名
        /// </summary>
        public string UserNameItemKey { get; set; } = "SignAuthorizationUserName";

        /// <summary>
        /// 允许访问的用户名列表（为空时不限制）
        /// </summary>
        public ISet<string> AllowedUsers { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

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

            if (string.IsNullOrEmpty(CookieName))
            {
                throw new ArgumentException("CookieName cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(CookieSeparator))
            {
                throw new ArgumentException("CookieSeparator cannot be null or empty.");
            }

            if (Expire <= 0)
            {
                throw new ArgumentException("Expire must be greater than 0.");
            }

            if (string.IsNullOrWhiteSpace(UserNameItemKey))
            {
                throw new ArgumentException("UserNameItemKey cannot be null or empty.");
            }
        }
    }
}
