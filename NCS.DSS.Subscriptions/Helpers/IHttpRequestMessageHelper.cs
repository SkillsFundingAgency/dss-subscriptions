using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.Subscriptions.Helpers
{
    public interface IHttpRequestMessageHelper
    {
        Task<T> GetSubscriptionsFromRequest<T>(HttpRequestMessage req);
        string GetTouchpointId(HttpRequestMessage req);
    }
}