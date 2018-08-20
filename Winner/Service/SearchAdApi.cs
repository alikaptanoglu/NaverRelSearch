using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Naver.SearchAd
{
    class SearchAdApi
    {
        private readonly string BaseUrl = Winner.Properties.Settings.Default.BASE_URL;
        private readonly string ApiKey = Winner.Properties.Settings.Default.API_KEY;
        private readonly string SecretKey = Winner.Properties.Settings.Default.SECRET_KEY;
        private readonly long managerCustomerId = long.Parse(Winner.Properties.Settings.Default.CUSTOMER_ID);
        private readonly HMACSHA256 HMAC;


        public SearchAdApi()
        {            
            this.HMAC = new HMACSHA256(Encoding.UTF8.GetBytes(SecretKey));            
        }

        public List<T> Execute<T>(RestRequest request, string root) where T : new()
        {

            try
            {
                var client = new RestClient(BaseUrl);

                var timestamp = getTimestamp().ToString();
                var signature = generateSignature(timestamp, request.Method.ToString(), request.Resource);

                request.AddHeader("X-API-KEY", ApiKey);
                request.AddHeader("X-Customer", managerCustomerId.ToString());
                request.AddHeader("X-Timestamp", timestamp);
                request.AddHeader("X-Signature", signature);

                var response = client.Execute<T>(request);

                var list = JObject.Parse(response.Content).SelectToken(root).ToObject<List<T>>();

                return list;
            }
            catch
            {
                return null;
            }
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