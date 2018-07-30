using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;
using NCS.DSS.Subscriptions.Models;

namespace NCS.DSS.Subscriptions.Helpers
{
    public class HttpRequestMessageHelper : IHttpRequestMessageHelper
    {
        public async Task<T> GetSubscriptionsFromRequest<T>(HttpRequestMessage req)
        {
            req.Content.Headers.ContentType.MediaType = "application/json";
            return await req.Content.ReadAsAsync<T>();
        }
    }
}
