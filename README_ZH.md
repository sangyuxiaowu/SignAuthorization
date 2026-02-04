# SignAuthorization

[![NuGet版本(Sang.AspNetCore.SignAuthorization)](https://img.shields.io/nuget/v/Sang.AspNetCore.SignAuthorization.svg?style=flat-square)](https://www.nuget.org/packages/Sang.AspNetCore.SignAuthorization/)

简易的API URL签名验证中间件，通过简单的URL参数验证请求是否合法。

[English](./README.md) | 简体中文

## 工作原理

1. 将`token`、`timestamp`、`nonce`三个参数进行字典序排序。
2. 将三个参数字符串拼接成一个字符串进行SHA1加密。
3. 开发者获得加密后的字符串可与`signature`对比。

## 使用说明

### 第一步：添加包

```bash
Install-Package Sang.AspNetCore.SignAuthorization
```

或者

```bash
dotnet add package Sang.AspNetCore.SignAuthorization
```

### 第二步：启用中间件

在`app.MapControllers();`前启用这个中间件。

```csharp
app.UseSignAuthorization(opt => {
    opt.sToken = "你的api-token";
});
```

### 第三步：使用`SignAuthorizeAttribute`

在需要签名的地方添加`SignAuthorizeAttribute`。

例如：

```csharp
app.MapGet("/weatherforecast", () =>
{
    // 你的代码
}).WithMetadata(new SignAuthorizeAttribute());
```

或者：

```csharp
[HttpGet]
[SignAuthorize]
public IEnumerable<WeatherForecast> Get()
{
    // 你的代码
}
```

## 设置

### SignAuthorizationOptions

| 参数               | 默认值                                                          | 说明                                                   |
|-------------------|---------------------------------------------------------------|-------------------------------------------------------|
| UnauthorizedBack  | {"success":false,"status":10000,"msg":"Unauthorized"}         | 验证失败后的json返回内容                               |
| UnauthorizedBackJson | null                                                        | 预序列化 JSON 字符串，用于 AOT 场景                     |
| UnauthorizedStatusCode | 401                                                     | 验证失败时返回的 HTTP 状态码                            |
| sToken            | SignAuthorizationMiddleware                                   | API签名使用的token                                    |
| WithPath          | false                                                         | 签名时需要包含请求的路径，以'/'开头                    |
| Expire            | 5                                                             | 签名过期时间（单位：秒）                               |
| nTimeStamp        | timestamp                                                     | 时间戳的GET参数名                                      |
| nNonce            | nonce                                                         | 随机数的GET参数名                                      |
| nSign             | signature                                                     | 签名的GET参数名                                        |
| nExtra            |                                                               | 额外参数的GET参数名                                    |
| UseHeader         | false                                                         | 是否使用header传递签名                                 |

## 示例

### PHP示例

```php
$sToken = "你的api-token";
$sReqTimeStamp = time();
$sReqNonce = getNonce();
$tmpArr = array($sToken, $sReqTimeStamp, $sReqNonce);
sort($tmpArr, SORT_STRING);
$sign = sha1(implode($tmpArr));
$url = "http://localhost:5177/weatherforecast?timestamp=$sReqTimeStamp&nonce=$sReqNonce&signature=$sign";
echo "$url\n";
echo file_get_contents($url);

function getNonce(){
    $str = '1234567890abcdefghijklmnopqrstuvwxyz';
    $t1='';
    for($i=0;$i<30;$i++){
        $j=rand(0,35);
        $t1 .= $str[$j];
    }
    return $t1;
}
```

### .Net示例

```csharp
var unixTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
var sNonce = Guid.NewGuid().ToString();

ArrayList AL = new ArrayList();
AL.Add("你的api-token");
AL.Add(unixTimestamp.ToString());
AL.Add(sNonce);
AL.Sort(StringComparer.Ordinal);

var raw = string.Join("", AL.ToArray());
using System.Security.Cryptography.SHA1 sha1 = System.Security.Cryptography.SHA1.Create();
byte[] encry = sha1.ComputeHash(Encoding.UTF8.GetBytes(raw));
string sign = string.Join("", encry.Select(b => string.Format("{0:x2}", b)).ToArray()).ToLower();

var client = new HttpClient();
string jsoninfo = await client.GetStringAsync($"http://localhost:5177/weatherforecast?timestamp={unixTimestamp}&nonce={sNonce}&signature={sign}");
```

### 使用MakeSignAuthorization

生成签名授权字符串。

```csharp
var unixTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
var sNonce = Guid.NewGuid().ToString("N");
var sToken = "你的api-token";
var sPath = "/weatherforecast";
var sExtra = "1"; // 额外参数：extra=1
string sign = MakeSignAuthorization.MakeSign(sToken, unixTimestamp, sNonce, sPath, sExtra);
```

生成签名URL。

```csharp
var url = MakeSignAuthorization.MakeSignUrl("http://localhost:5177",  new SignAuthorizationOptions());
```

## Cookie 鉴权

适用于简单用户管理场景（例如 `root`、`admin`、`init`）。Cookie 的内容格式为 `用户名|时间戳|签名`。

### 启用 Cookie 中间件

```csharp
var cookieOptions = new CookieAuthorizationOptions
{
    sToken = "你的api-token",
    CookieName = "SignAuthorization",
    Expire = 3600,
    ReuseExpire = true
};

cookieOptions.AllowedUsers.UnionWith(new[] { "root", "admin", "init" });

app.UseCookieAuthorization(opt =>
{
    opt.sToken = cookieOptions.sToken;
    opt.CookieName = cookieOptions.CookieName;
    opt.Expire = cookieOptions.Expire;
    opt.ReuseExpire = cookieOptions.ReuseExpire;
    opt.CookieSeparator = cookieOptions.CookieSeparator;
    opt.AllowedUsers.UnionWith(cookieOptions.AllowedUsers);
});
```

### 生成并写入 Cookie

```csharp
app.MapGet("/login/{user}", (string user, HttpContext context) =>
{
    var timeStamp = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
    var cookieValue = MakeSignAuthorization.MakeCookieValue(
        "你的api-token",
        user,
        timeStamp,
        "|");

    context.Response.Cookies.Append(
        "SignAuthorization",
        cookieValue,
        new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            Expires = DateTimeOffset.Now.AddSeconds(3600)
        });

    return Results.Ok(new { success = true, user });
});
```

### 保护接口

```csharp
app.MapGet("/secure", (HttpContext context) =>
{
    return Results.Ok(new
    {
        user = context.User.Identity?.Name,
        item = context.Items["SignAuthorizationUserName"]
    });
}).WithMetadata(new CookieAuthorizeAttribute("root", "admin"));
```

### CookieAuthorizationOptions

| 参数               | 默认值                     | 说明                                      |
|-------------------|----------------------------|-------------------------------------------|
| UnauthorizedBack  | {"success":false,"status":10000,"msg":"Unauthorized"} | 验证失败后的json返回内容                 |
| UnauthorizedBackJson | null                   | 预序列化 JSON 字符串，用于 AOT 场景       |
| UnauthorizedStatusCode | 401                | 验证失败时返回的 HTTP 状态码              |
| sToken            | CookieAuthorizationMiddleware | Cookie 签名使用的 token                |
| CookieName        | SignAuthorization           | Cookie 字段名                              |
| CookieSeparator   | |                          | Cookie 内容分隔符                          |
| Expire            | 3600                        | Cookie 过期时间（单位：秒）                |
| ReuseExpire       | true                        | 验证成功后刷新时间戳（滑动过期）          |
| CookieOptions     | HttpOnly/IsEssential set    | 刷新时使用的 Cookie 选项                   |
| UserNameClaimType | ClaimTypes.Name             | 用户名 Claim 类型                          |
| UserNameItemKey   | SignAuthorizationUserName   | HttpContext.Items 存储用户的字段名        |
| AllowedUsers      | 空                          | 允许访问的用户名列表（为空不限制）        |