﻿using System;
using System.Net.Http;
using System.Text;
using Azure.Health.DataServices.Pipelines;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AzureHealth.DataServices.Tests.Proxy
{
    [TestClass]
    public class OperationContextTests
    {
        [TestMethod]
        public void OperationContext_UpdateUri_Test()
        {
            string content = "content";
            string uriString = "https://example.org/fhir/Patient/1";
            string expectedUriString = "https://example.org/fhir/Patient/2";
            HttpMethod expectedMethod = HttpMethod.Post;

            HttpRequestMessage request = new(HttpMethod.Get, new Uri(uriString));
            OperationContext context = new(request);
            context.UpdateFhirRequestUri(expectedMethod, "fhir", "Patient", "2");
            context.ContentString = content;

            Assert.AreEqual(expectedUriString, request.RequestUri.ToString(), "Uri mismatch.");
            Assert.AreEqual(content, Encoding.UTF8.GetString(context.Content), "Content mismatch");
            Assert.AreEqual(expectedMethod, request.Method, "Http method mismatch");
        }

        [TestMethod]
        public void OperationContext_Properties_Test()
        {
            string propKey = "test";
            string propValue = "value";
            string uriString = "https://example.org/fhir/Patient/1";

            HttpRequestMessage request = new(HttpMethod.Get, new Uri(uriString));
            OperationContext context = new(request);
            context.Properties.Add(propKey, propValue);
            string actualValue = context.Properties[propKey];
            Assert.AreEqual(propValue, actualValue, "Property mismatch.");
        }
    }
}
