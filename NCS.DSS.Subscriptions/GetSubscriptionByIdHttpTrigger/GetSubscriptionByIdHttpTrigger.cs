﻿using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System;
using System.Web.Http.Description;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.Subscriptions.Annotations;

namespace NCS.DSS.Subscriptions.GetSubscriptionByIdHttpTrigger
{ 
    public static class GetSubscriptionByIdHttpTrigger
    {
        [FunctionName("GETByID")]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Subscriptions Retrieved", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Resource Does Not Exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Get request is malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API Key unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient Access To This Resource", ShowSchema = false)]
        [ResponseType(typeof(Models.Subscription))]
        [Display(Name = "Get", Description = "Ability to get a session object for a given customer.")]
        [Disable]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "customers/{customerId}/subscriptions/{subscriptionid}")]HttpRequestMessage req, TraceWriter log, string customerId, string subscriptionid)
        {
            log.Info("C# HTTP trigger function Get Subscription By Id processed a request.");

            if (!Guid.TryParse(subscriptionid, out var subscriptionGuid))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(subscriptionid),
                        System.Text.Encoding.UTF8, "application/json")
                };
            }

            var service = new GetSubscriptionByIdHttpTriggerService();
            var values = await service.GetSubscriptions(subscriptionGuid);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(values),
                    System.Text.Encoding.UTF8, "application/json")
            };
        }

    }

}