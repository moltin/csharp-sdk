# C# SDK Alpha

* [Website](http://molt.in)
* [License](https://github.com/moltin/csharp-sdk/master/LICENSE)
* Version: dev

The Moltin csharp-sdk is a simple to use interface for the API to help you get off the ground quickly and efficiently.

## Installation
Download and compile the DLL using the included project or use the pre-built version found in Moltin/bin/Release

Add the DLL as a reference in your project and in the top of your main class add:

``` c#
using Moltin;
```

## Usage

There are a number of ways to utilize this SDK, I believe the best way is to create a provider (which can be injected into controllers using a DI solution like autofac):

``` c#
public class MoltinProvider
{
    private readonly OAuthHandler _authHandler;
    private readonly string _moltinUrl;

    public MoltinProvider(IConfig config)
    {
        var publicKey = config.MoltinPublicKey;
        var secretKey = config.MoltinSecretKey;
        var authUrl = config.MoltinAuthUrl;

        _authHandler = new OAuthHandler(publicKey, secretKey, authUrl);
        _moltinUrl = config.MoltinBaseUrl + config.MoltinVersion + "/";
    }

    public async Task<JToken> Get(string path) =>
        await _authHandler.QueryApiAsync(_moltinUrl + path);

    public async Task<JToken> Post(string path, object data) =>
        await _authHandler.QueryApiAsync(_moltinUrl + path, HttpMethod.POST, data);

    public async Task<JToken> Put(string path, object data) =>
        await _authHandler.QueryApiAsync(_moltinUrl + path, HttpMethod.PUT, data);

    public async Task<JToken> Delete(string path) =>
        await _authHandler.QueryApiAsync(_moltinUrl + path, HttpMethod.DELETE);
}
```

Now you are able to use the provider in your controllers like this:

``` c#
[Authorize]
[RoutePrefix("carts")]
public class CartsController : ApiController
{
    private readonly MoltinProvider _moltinProvider;
    public CartsController(MoltinProvider moltinProvider) => _moltinProvider = moltinProvider;

    /// <summary>
    ///     Gets a list of carts from the Moltin API
    /// </summary>
    /// <returns>A list of carts</returns>
    [HttpGet]
    [Route("")]
    public async Task<IHttpActionResult> Get()
    {
        // Get our models
        var models = await _moltinProvider.Get("carts");

        // Return our result
        return Ok(models.SelectToken("result"));
    }

    /// <summary>
    ///     Gets aa item from the cart based on it's id
    /// </summary>
    /// <param name="cartId">The id of the cart</param>
    /// <returns>A list of carts</returns>
    [HttpGet]
    [Route("")]
    public async Task<IHttpActionResult> Get(string cartId)
    {
        // Get our model
        var model = await _moltinProvider.Get("carts/" + cartId);

        // Return our result
        return Ok(model.SelectToken("result"));
    }

    /// <summary>
    ///     Checks if an item is in the cart.
    /// </summary>
    /// <param name="cartId">The id of the cart</param>
    /// <param name="itemId">The id of the item to check</param>
    /// <returns>A boolean value</returns>
    [HttpGet]
    [Route("incart")]
    public async Task<IHttpActionResult> InCart(string cartId, string itemId)
    {
        var model = await _moltinProvider.Get("carts/" + cartId + "/has/" + itemId);

        // Return our result
        return Ok(model.SelectToken("result"));
    }

    /// <summary>
    ///     Creates the cart an insert the item into it.
    /// </summary>
    /// <param name="model">The CartItem model containing the item id and quantity</param>
    /// <returns></returns>
    [HttpPost]
    [Route("")]
    public async Task<IHttpActionResult> Create(CartItemBindingModel model)
    {
        // If our model is invalid, return the errors
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // If we don't have a cart id, create one
        if (string.IsNullOrEmpty(model.CartId)) model.CartId = Guid.NewGuid().ToString().Replace(" - ", "");

        // Get our response
        var response = await _moltinProvider.Post("carts/" + model.CartId, model);

        // Return Ok
        return Ok(response.SelectToken("result"));
    }

    [HttpPost]
    [Route("checkout")]
    public async Task<IHttpActionResult> Checkout(CheckoutBindingModel model)
    {
        // If our model is invalid, return the errors
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // Get our response
        var response = await _moltinProvider.Post("carts/" + model.CartId + "/checkout", model);

        // Return Ok
        return Ok(response.SelectToken("result"));
    }

    /// <summary>
    ///     Updates the item in the cart
    /// </summary>
    /// <param name="model">The CartItem model containing the item id and quantity</param>
    /// <returns></returns>
    [HttpPut]
    [Route("")]
    public async Task<IHttpActionResult> Update(CartItemBindingModel model)
    {
        // If our model is invalid, return the errors
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // Get our response
        var response = await _moltinProvider.Put("carts/" + model.CartId + "/item/" + model.Id, model);

        // Return Ok
        return Ok(response.SelectToken("result"));
    }

    /// <summary>
    ///     Deletes an item from the cart
    /// </summary>
    /// <param name="cartId">The cart id</param>
    /// <param name="itemId">The id of the item to delete</param>
    /// <returns></returns>
    [HttpDelete]
    [Route("")]
    public async Task<IHttpActionResult> Delete(string cartId, string itemId)
    {
        // Get our response
        var response = await _moltinProvider.Delete("carts/" + cartId + "/item/" + itemId);

        // Return Ok
        return Ok(response.SelectToken("result"));
    }
}
```

It is worth noting that you do not need to worry about authentication; the SDK will take care of getting your access token and caching it for 55 minutes (so that each request is not authenticating). If you need to perform a request without using the access token, there is an optional parameter on the `OAuthHandler.QueryApi` method:

``` c#
bool requiresAuthentication = true
```

Setting this to false will disable the Authentication header for that request.

## Contributing

Please see [CONTRIBUTING](https://github.com/moltin/charp-sdk/blob/master/CONTRIBUTING.md) for details.


## Credits

- [Moltin](https://github.com/moltin)
- [All Contributors](https://github.com/moltin/charp-sdk/contributors)


## License

Please see [License File](https://github.com/moltin/charp-sdk/blob/master/LICENSE) for more information.
