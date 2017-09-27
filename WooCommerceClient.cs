using log4net;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Flexinets.WooCommerce
{
    public class WooCommerceClient
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(WooCommerceClient));
        private readonly String _signatureSecret;
        private readonly String _baseUrl;
        private readonly String _consumerKey;
        private readonly String _consumerSecret;


        /// <summary>
        /// Woocommerce client wrapper
        /// </summary>
        /// <param name="consumerKey"></param>
        /// <param name="consumerSecret"></param>
        /// <param name="signatureSecret"></param>
        /// <param name="baseUrl"></param>
        public WooCommerceClient(String consumerKey, String consumerSecret, String signatureSecret, String baseUrl)
        {
            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;
            _signatureSecret = signatureSecret;
            _baseUrl = baseUrl;
        }


        /// <summary>
        /// Get an order by order id
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public async Task<WooCommerceOrderModel> GetOrderAsync(Int64 orderId)
        {
            using (var client = new WebClient())
            {
                var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_consumerKey}:{_consumerSecret}"));
                client.Headers[HttpRequestHeader.Authorization] = $"Basic {credentials}";
                var orderJson = await client.DownloadStringTaskAsync($"{_baseUrl}/wp-json/wc/v1/orders/{orderId}");
                return JsonConvert.DeserializeObject<WooCommerceOrderModel>(orderJson);
            }
        }


        /// <summary>
        /// Verifies the token from the header in an incoming woocommerce request
        /// </summary>
        /// <param name="content"></param>
        /// <param name="hash"></param>
        /// <returns></returns>
        public Boolean VerifyToken(String content, String hash)
        {
            var sha = new HMACSHA256(Encoding.UTF8.GetBytes(_signatureSecret));
            var hashbytes = sha.ComputeHash(Encoding.UTF8.GetBytes(content));
            var hashstring = Convert.ToBase64String(hashbytes);
            return hash == hashstring;
        }
    }
}