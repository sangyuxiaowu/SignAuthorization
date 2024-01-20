# SignAuthorization

[![NuGet version (Sang.AspNetCore.SignAuthorization)](https://img.shields.io/nuget/v/Sang.AspNetCore.SignAuthorization.svg?style=flat-square)](https://www.nuget.org/packages/Sang.AspNetCore.SignAuthorization/)

简易的 API url 签名验证中间件，通过简单的url参数验证请求是否合法。

1. 将token、timestamp、nonce三个参数进行字典序排序 
1. 将三个参数字符串拼接成一个字符串进行sha1加密
1. 开发者获得加密后的字符串可与 signature 对比

<hr>

Simple API url signature verification middleware.Verify that the request is legitimate with a simple url parameter.

1. Sort the three parameters of token, timestamp and nonce in lexicographic order
1. Concatenate the three parameter strings into one string for sha1 encryption
1. The developer obtains the encrypted string which can be compared with the signature

## Instructions:

##### Step 1 

Add this package.

```bash
Install-Package Sang.AspNetCore.SignAuthorization
```

or

```bash
dotnet add package Sang.AspNetCore.SignAuthorization
```

##### Step 2 

Enable this middleware before `app.MapControllers();`.

在  `app.MapControllers();` 前启用这个中间件。

```
app.UseSignAuthorization(opt => {
    opt.sToken = "you-api-token";
});
```

##### Step 3

Add `SignAuthorizeAttribute` where signing is required.

在需要签名的地方添加 `SignAuthorizeAttribute`。

like this:

```
app.MapGet("/weatherforecast", () =>
{
    // your code
}).WithMetadata(new SignAuthorizeAttribute());
```

or:

```
[HttpGet]
[SignAuthorize]
public IEnumerable<WeatherForecast> Get()
{
    // your code
}
```

## Setting

### SignAuthorizationOptions


| 参数 | default | 说明|
| --- | --- | --- |
| UnauthorizedBack | {"success":false,"status":10000,"msg":"Unauthorized"} |  json return content after validation failure <br> 验证失败后的 json 返回 |
| sToken | SignAuthorizationMiddleware | API token for sign <br> API签名使用的token |
| WithPath | false |  Need to include the requested path when signing, starting with '/' <br> 签名时需要包含请求的路径，以 '/' 开头 |
| Expire |  5 | Signature expiration time (unit: second) <br> 签名过期时间（单位:秒） |
| nTimeStamp | timestamp  |  GET parameter name for timestamp <br> 时间戳的GET参数名 |
| nNonce | nonce  | GET parameter name of random number <br> 随机数的GET参数名 |
| nSign | signature | Sign GET parameter name <br> 签名的GET参数名 |
| nExtra | | Extra GET parameter name <br> 额外参数的GET参数名 |


## PHP example

```php
$sToken = "you-api-token";
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

## .Net example

```csharp
var unixTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
var sNonce = Guid.NewGuid().ToString();

ArrayList AL = new ArrayList();
AL.Add("you-api-token");
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

## Use MakeSignAuthorization

Make sign authorization string.

```csharp
var unixTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
var sNonce = Guid.NewGuid().ToString("N");
var sToken = "you-api-token";
var sPath = "/weatherforecast";
var sExtra = "1"; // extra parameter: extra=1
string sign = MakeSignAuthorization.MakeSign(sToken, unixTimestamp, sNonce, sPath, sExtra);
```

Make sign URL.

```csharp
var url = MakeSignAuthorization.MakeSignUrl("http://localhost:5177",  new SignAuthorizationOptions());
```