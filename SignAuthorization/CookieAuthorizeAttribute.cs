namespace Sang.AspNetCore.SignAuthorization
{
    /// <summary>
    /// Cookie授权的属性，含有该属性的资源在访问时需要进行Cookie鉴权
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class CookieAuthorizeAttribute : Attribute
    {
        /// <summary>
        /// 允许访问的用户名列表
        /// </summary>
        public string[] Users { get; }

        /// <summary>
        /// 初始化 CookieAuthorizeAttribute 的新实例。
        /// </summary>
        /// <param name="users">允许访问的用户名列表</param>
        public CookieAuthorizeAttribute(params string[] users)
        {
            Users = users ?? Array.Empty<string>();
        }
    }
}
