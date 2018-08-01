﻿using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.Subscriptions.Helpers
{
    public class HttpRequestMessageHelper : IHttpRequestMessageHelper
    {
        public async Task<T> GetSubscriptionsFromRequest<T>(HttpRequestMessage req)
        {
            req.Content.Headers.ContentType.MediaType = "application/json";
            return await req.Content.ReadAsAsync<T>();
        }

        public Guid? GetTouchpointId(HttpRequestMessage req)
        {
            if (req?.Headers == null)
                return null;

            if (!req.Headers.Contains("APIM-TouchpointId"))
                return null;

            var touchpointId = req.Headers.GetValues("APIM-TouchpointId").FirstOrDefault();

            if (!Guid.TryParse(touchpointId, out var touchpountGuid))
                return null;

            return touchpountGuid;
        }
    }
}
