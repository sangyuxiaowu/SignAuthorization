namespace Sang.AspNetCore.SignAuthorization
{
    /// <summary>
    /// 签名授权的属性，含有该属性的资源在访问时需要进行URL签名
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class SignAuthorizeAttribute : Attribute
    {
    }
}
