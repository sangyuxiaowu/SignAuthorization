# SignAuthorization

[![NuGet version (Sang.AspNetCore.SignAuthorization)](https://img.shields.io/nuget/v/Sang.AspNetCore.SignAuthorization.svg?style=flat-square)](https://www.nuget.org/packages/Sang.AspNetCore.SignAuthorization/)

A simple API URL signature verification middleware to validate requests through straightforward URL parameters.

English | [简体中文](./README_ZH.md)

## How It Works

1. Sort the `token`, `timestamp`, and `nonce` parameters in lexicographic order.
2. Concatenate the three parameters into a single string and encrypt it using SHA1.
3. Developers can then compare the obtained encrypted string with the `signature`.

## Instructions

### Step 1: Add the Package

```bash
Install-Package Sang.AspNetCore.SignAuthorization
```

or

```bash
dotnet add package Sang.AspNetCore.SignAuthorization
```

### Step 2: Enable the Middleware

Enable this middleware before `app.MapControllers();`.

```csharp
app.UseSignAuthorization(opt => {
    opt.sToken = "your-api-token";
});
```

### Step 3: Use `SignAuthorizeAttribute`

Add `SignAuthorizeAttribute` where signing is required.

Example:

```csharp
app.MapGet("/weatherforecast", () =>
{
    // your code
}).WithMetadata(new SignAuthorizeAttribute());
```

or:

```csharp
[HttpGet]
[SignAuthorize]
public IEnumerable<WeatherForecast> Get()
{
    // your code
}
```

## Settings

### SignAuthorizationOptions

| Parameter          | Default Value                                                    | Description                                             |
|--------------------|------------------------------------------------------------------|---------------------------------------------------------|
| UnauthorizedBack   | {"success":false,"status":10000,"msg":"Unauthorized"}            | JSON return content after validation failure            |
| sToken             | SignAuthorizationMiddleware                                      | API token for signing                                   |
| WithPath           | false                                                            | Include the requested path in the signature, starting with '/' |
| Expire             | 5                                                                | Signature expiration time (unit: seconds)               |
| nTimeStamp         | timestamp                                                        | GET parameter name for timestamp                        |
| nNonce             | nonce                                                            | GET parameter name for the random number                |
| nSign              | signature                                                        | GET parameter name for the signature                    |
| nExtra             |                                                                  | Extra GET parameter name                                |
| UseHeader          | false                                                            | Use the header to pass the signature                    |

## Examples

### PHP Example

```php
$sToken = "your-api-token";
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

### .Net Example

```csharp
var unixTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
var sNonce = Guid.NewGuid().ToString();

ArrayList AL = new ArrayList();
AL.Add("your-api-token");
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

### Use MakeSignAuthorization

Make sign authorization string.

```csharp
var unixTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
var sNonce = Guid.NewGuid().ToString("N");
var sToken = "your-api-token";
var sPath = "/weatherforecast";
var sExtra = "1"; // extra parameter: extra=1
string sign = MakeSignAuthorization.MakeSign(sToken, unixTimestamp, sNonce, sPath, sExtra);
```

Make sign URL.

```csharp
var url = MakeSignAuthorization.MakeSignUrl("http://localhost:5177",  new SignAuthorizationOptions());
```

## Cookie Authorization

Use Cookie-based authorization for simple user scenarios (for example `root`, `admin`, `init`). The cookie stores `username|timestamp|signature`.

### Enable Cookie Middleware

```csharp
var cookieOptions = new CookieAuthorizationOptions
{
    sToken = "your-api-token",
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

### Issue Cookie

```csharp
app.MapGet("/login/{user}", (string user, HttpContext context) =>
{
    var timeStamp = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
    var cookieValue = MakeSignAuthorization.MakeCookieValue(
        "your-api-token",
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

### Protect Endpoint

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

| Parameter          | Default Value            | Description                                         |
|--------------------|--------------------------|-----------------------------------------------------|
| UnauthorizedBack   | {"success":false,"status":10000,"msg":"Unauthorized"} | JSON return content after validation failure        |
| sToken             | CookieAuthorizationMiddleware | Token used to sign cookie values                |
| CookieName         | SignAuthorization         | Cookie name                                         |
| CookieSeparator    | |                        | Separator for cookie values                         |
| Expire             | 3600                      | Cookie expiration time (unit: seconds)              |
| ReuseExpire        | true                      | Refresh cookie timestamp on successful validation   |
| CookieOptions      | HttpOnly/IsEssential set  | Cookie options to use when refreshing               |
| UserNameClaimType  | ClaimTypes.Name           | Claim type for user name                            |
| UserNameItemKey    | SignAuthorizationUserName | Key for storing user name in HttpContext.Items      |
| AllowedUsers       | empty                     | Allowed user list (empty for no restriction)        |