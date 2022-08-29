# SignAuthorization

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

Enable this middleware before `app.MapControllers();`.

在  `app.MapControllers();` 前启用这个中间件。

```
app.UseSignAuthorization(opt => {
    opt.sToken = "you-api-token";
});
```

##### Step 2

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
| TimeOut |  5 | Signature expiration time <br> 签名过期时间 |
| nTimeStamp | timestamp  |  GET parameter name for timestamp <br> 时间戳的GET参数名 |
| nNonce | nonce  | GET parameter name of random number <br> 随机数的GET参数名 |
| nSign | signature | Sign GET parameter name <br> 签名的GET参数名 |


## PHP example

```
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