using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Tests.NubeSync.Client
{
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        public MockHttpMessageHandler()
        {
            Results = new List<TestItem>
            {
                new TestItem { Id = "123", Name = "Name1" },
                new TestItem { Id = "456", Name = "Name2" },
            };
        }

        public bool HttpRequestFails { get; set; }

        public bool HttpRequestThrows { get; set; }

        public HttpRequestMessage LastRequest { get; set; }

        public string Response { get; set; }

        public List<TestItem> Results { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;

            if (HttpRequestThrows)
            {
                throw new Exception();
            }

            if (HttpRequestFails)
            {
                return _FailingResult();
            }

            return _DefaultResult();
        }

        public void UserLargeResultSet(int size = 150)
        {
            Results = new List<TestItem>();

            for (int i=1; i<=size; i++)
            {
                Results.Add(new TestItem { Id = "i", Name = $"Name{i}" });
            }
        }

        private Task<HttpResponseMessage> _DefaultResult()
        {
            return Task.FromResult(new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(Results,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }))
            });
        }

        private Task<HttpResponseMessage> _FailingResult()
        {
            return Task.FromResult(new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Content = new StringContent("some message")
            });
        }
    }
}