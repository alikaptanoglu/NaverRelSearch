using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Naver.SearchAd
{
    class SearchAdApi
    {
        private readonly string BaseUrl;
        private readonly string ApiKey;
        private readonly string SecretKey;
        private readonly HMACSHA256 HMAC;

        public SearchAdApi(string baseUrl, string apiKey, string secretKey)
        {
            this.BaseUrl = baseUrl;
            this.ApiKey = apiKey;
            this.SecretKey = secretKey;
            this.HMAC = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
        }

        public List<T> Execute<T>(RestRequest request, long customerId, string root) where T : new()
        {

            try
            {
                var client = new RestClient(BaseUrl);

                var timestamp = getTimestamp().ToString();
                var signature = generateSignature(timestamp, request.Method.ToString(), request.Resource);

                request.AddHeader("X-API-KEY", ApiKey);
                request.AddHeader("X-Customer", customerId.ToString());
                request.AddHeader("X-Timestamp", timestamp);
                request.AddHeader("X-Signature", signature);

                var response = client.Execute<T>(request);

                if (response.ErrorException != null)
                {
                    throw new ApplicationException("Error retrieving response. Check inner details for more info.", response.ErrorException);
                }

                var list = JObject.Parse(response.Content).SelectToken(root).ToObject<List<T>>();

                return list;
            }
            catch
            {
                // Error
            }

            return null;
        }

        private static IEnumerable<JToken> AllChildren(JToken json)
        {
            foreach (var c in json.Children())
            {
                yield return c;
                foreach (var cc in AllChildren(c))
                {
                    yield return cc;
                }
            }
        }

        private long getTimestamp()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }

        private string generateSignature(string timestamp, string method, string resource)
        {
            return Convert.ToBase64String(HMAC.ComputeHash(Encoding.UTF8.GetBytes(timestamp + "." + method + "." + resource)));
        }

        private static readonly Func<Parameter, bool> IS_HEADER = param => param.Type == ParameterType.HttpHeader;
        private static readonly Func<Parameter, bool> IS_QUERYPARAM = param => param.Type == ParameterType.QueryString;

    }
}