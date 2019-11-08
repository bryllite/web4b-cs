# Web4b Samples

## Game User's Private Key And Address

~~~c#
// user key
public string GetUserKey(string uid)
{
    return gamekey.CKD(uid);
}

// user address
public string GetUserAddress(string uid)
{
    return gamekey.CKD(uid).Address;
}
~~~
see [BrylliteApiService.GetUserKey()]() and [BrylliteApiService.GetUserAddress()]()

## User Balance

~~~c#
// get user balance
public async Task<ulong> GetBalanceAsync(string address)
{
    try
    {
        return await api.GetBalanceAsync(address, LATEST) ?? 0;
    }
    catch (Exception ex)
    {
        Log.Warning("exception! ex=", ex);
        return 0;
    }
}
~~~
see [BrylliteApiService.GetBalanceAsync()]()

## In-Game Transaction
~~~c#
// In-Game Tx
public async Task<string> TransferAsync(string signer, string to, decimal value, decimal gas, ulong? nonce = null)
{
    try
    {
        return await api.TransferAsync(signer, to, value, gas, nonce);
    }
    catch (Exception ex)
    {
        Log.Warning("exception! ex=", ex);
        return null;
    }
}
~~~
see [BrylliteApiService.TransferAsync()]()

## Ex-Game Transaction
~~~c#
// Ex-Game Tx
public async Task<string> PayoutAsync(string signer, string to, decimal value, decimal gas, ulong? nonce = null)
{
    try
    {
        return await api.PayoutAsync(signer, to, value, gas, nonce);
    }
    catch (Exception ex)
    {
        Log.Warning("exception! ex=", ex);
        return null;
    }
}
~~~
see [BrylliteApiService.TransferAsync()]()

## PoA

1. BridgeService request game client to prove attendance (with `hash`, `iv`)
1. game client request game server to aquire valid `accessToken`
1. game server publish `accessToken` to game client ( see [BrylliteApiService.GetPoAToken()]())
1. game client returns `accessToken` ( see [GameClient.OnPoARequest()]())

