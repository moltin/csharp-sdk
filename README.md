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

There are a number of ways to utilize this SDK, an example would be to create a BaseApiController which exposes some properties and methods. It could look something like this:

``` c#
    /// <summary>
    /// Our base controller for setting up global access to moltin api
    /// </summary>
    public class BaseController : ApiController
    {
        // Private properties
        private OAuthHandler oauthHandler;
        private ModelFactory factory;
        private string publicKey;
        private string secretKey;

        // Public propertiers
        public string ApiUrl { get; private set; }
        public OAuthHandler OAuthHandler { get { return this.oauthHandler ?? (this.oauthHandler = new OAuthHandler(publicKey, secretKey)); } }
        public ModelFactory Factory { get { return this.factory ?? (this.factory = new ModelFactory()); } }

        /// <summary>
        /// Default constructor
        /// </summary>
        public BaseController()
        {
            // Get our private variables from our configuration file
            var baseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"].ToString();
            var version = ConfigurationManager.AppSettings["ApiVersion"].ToString();

            // Set our keys up
            this.publicKey = ConfigurationManager.AppSettings["MoltinApiKey"].ToString();
            this.secretKey = ConfigurationManager.AppSettings["MoltinSecretKey"].ToString();

            // Create our Urls and AccessToken
            this.ApiUrl = baseUrl + version + "/";
        }

        /// <summary>
        /// Gets the access token
        /// </summary>
        /// <param name="authUrl">The authorisation url</param>
        /// <returns>A string representing the access token</returns>
        public async Task<string> GetAccessToken()
        {

            // Authorization URL
            var authUrl = ConfigurationManager.AppSettings["ApiAuthUrl"].ToString();

            // Create our cache
            var cache = MemoryCache.Default;
            var policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(55) };
            var accessToken = (string)cache.Get("AccessToken");

            // If we have no cached access token
            if (!string.IsNullOrEmpty(accessToken))
                return accessToken;

            // Get our access token
            accessToken = await this.OAuthHandler.GetAccessTokenAsync(authUrl);

            // Add the token to the cache
            cache.Add(new CacheItem("AccessToken", accessToken), policy);

            // Return our access token
            return accessToken;
        }
    }
```

In your other controllers, you could then do this:

``` c#
    [Authorize]
    [RoutePrefix("api/carts")]
    public class CartsController : BaseController
    {
        /// <summary>
        /// Gets a list of carts from the Moltin API
        /// </summary>
        /// <returns>A list of carts</returns>
        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> Get()
        {
            return Ok(await this.OAuthHandler.QueryApiAsync(await this.GetAccessToken(), this.ApiUrl + "carts"));
        }

        /// <summary>
        /// Inserts an item into the cart
        /// </summary>
        /// <param name="model">The CartItem model containing the item id and quantity</param>
        /// <returns></returns>
        [HttpPost]
        [Route("insert")]
        public async Task<IHttpActionResult> Insert(CartItemBindingModel model)
        {

            // If our model is invalid, return the errors
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Perform our post
            await this.OAuthHandler.QueryApiAsync(await this.GetAccessToken(), this.ApiUrl + "carts", Moltin.HttpMethod.POST, model);

            // Return Ok
            return Ok();
        }
    }
```

## Contributing

Please see [CONTRIBUTING](https://github.com/moltin/charp-sdk/blob/master/CONTRIBUTING.md) for details.


## Credits

- [Moltin](https://github.com/moltin)
- [All Contributors](https://github.com/moltin/charp-sdk/contributors)


## License

Please see [License File](https://github.com/moltin/charp-sdk/blob/master/LICENSE) for more information.
