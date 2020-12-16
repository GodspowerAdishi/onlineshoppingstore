using PayPal.Api;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineShoppingStore
{
    public class PaypalConfiguration
    {
        public readonly static string clientId;
        public readonly static string clientSecret;


        static PaypalConfiguration()
        {
            var config = getconfig();
            clientId = "AZQkdI2EIusMmdkiv-kz-sbl2Xv8Nsa2wa1GR4W3dyMhCla7xtNpFsgGdiRoDBk9z4qjofu1AqqG_oRe";
            clientSecret = "EHq88FE1xW7lpeFZaItKh5_ESoZmZAm5Bu2UPcfAvXbdIFSfztyDGmeFpgAYlBt-nzsSR0JaEK6ubrBk";
        }

        public static Dictionary<string, string> getconfig()
        {
            return PayPal.Api.ConfigManager.Instance.GetProperties();
        }

        // Create acess token
        private static string GetAccessToken()
        {
            string accessToken = new OAuthTokenCredential(clientId, clientSecret, getconfig()).GetAccessToken();
            return accessToken;
        }

        // This will return APIContext object
        public static APIContext GetAPIContext()
        {
            APIContext apicontext = new APIContext(GetAccessToken());
            apicontext.Config = getconfig();
            return apicontext;
        }
    }
}