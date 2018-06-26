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

namespace NCS.DSS.Subscriptions.PutSubscriptionHttpTrigger
{ 
    public static class PutSubscriptionHttpTrigger
    {
        [FunctionName("PUT")]
        [Response(HttpStatusCode = (int)HttpStatusCode.Created, Description = "Subscriptions Replaced", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Unable to Replace Subscriptions", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient Access To This Resource", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "Unauthorised", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Resource Does Not Exist", ShowSchema = false)]
        [ResponseType(typeof(Models.Subscription))]
        [Display(Name = "Put", Description = "Ability to replace a session object for a given customer.")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "customers/{customerId}/subscriptions/{subscriptionid}")]HttpRequestMessage req, TraceWriter log, string customerId, string subscriptionid)
        {
            log.Info("C# HTTP trigger function Put Subscription processed a request.");
            
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject("Put successful for subscription with id: " + subscriptionid),
                    System.Text.Encoding.UTF8, "application/json")
            };
        }

    }

}