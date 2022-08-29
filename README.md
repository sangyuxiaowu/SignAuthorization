# SignAuthorization

���׵� API url ǩ����֤�м����ͨ���򵥵�url������֤�����Ƿ�Ϸ���

1. ��token��timestamp��nonce�������������ֵ������� 
1. �����������ַ���ƴ�ӳ�һ���ַ�������sha1����
1. �����߻�ü��ܺ���ַ������� signature �Ա�

<hr>

Simple API url signature verification middleware.Verify that the request is legitimate with a simple url parameter.

1. Sort the three parameters of token, timestamp and nonce in lexicographic order
1. Concatenate the three parameter strings into one string for sha1 encryption
1. The developer obtains the encrypted string which can be compared with the signature

## Instructions:

##### Step 1 

Enable this middleware before `app.MapControllers();`.

��  `app.MapControllers();` ǰ��������м����

```
app.UseSignAuthorization(opt => {
    opt.sToken = "you-api-token";
});
```

##### Step 2

Add `SignAuthorizeAttribute` where signing is required.

����Ҫǩ���ĵط���� `SignAuthorizeAttribute`��

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


| ���� | default | ˵��|
| --- | --- | --- |
| UnauthorizedBack | {"success":false,"status":10000,"msg":"Unauthorized"} |  json return content after validation failure <br> ��֤ʧ�ܺ�� json ���� |
| sToken | SignAuthorizationMiddleware | API token for sign <br> APIǩ��ʹ�õ�token |
| WithPath | false |  Need to include the requested path when signing, starting with '/' <br> ǩ��ʱ��Ҫ���������·������ '/' ��ͷ |
| TimeOut |  5 | Signature expiration time <br> ǩ������ʱ�� |
| nTimeStamp | timestamp  |  GET parameter name for timestamp <br> ʱ�����GET������ |
| nNonce | nonce  | GET parameter name of random number <br> �������GET������ |
| nSign | signature | Sign GET parameter name <br> ǩ����GET������ |


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