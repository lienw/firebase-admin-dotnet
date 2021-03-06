// Copyright 2018, Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Http;
using Google.Apis.Json;
using Xunit;

namespace FirebaseAdmin.Tests
{
    internal class MockHttpClientFactory : HttpClientFactory
    {
        private HttpMessageHandler Handler { get; set; }

        public MockHttpClientFactory(HttpMessageHandler handler)
        {
            Handler = handler;
        }

        protected override HttpMessageHandler CreateHandler(CreateHttpClientArgs args)
        {
            return Handler;
        }
    }

    /// <summary>
    /// An <see cref="HttpMessageHandler"/> implementation that counts the number of requests
    /// processed.
    /// </summary>
    internal abstract class CountableMessageHandler : HttpMessageHandler
    {
        private int _calls;

        public int Calls
        {
            get { return _calls; }
        }

        sealed protected override Task<HttpResponseMessage> SendAsync(
          HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref _calls);
            return SendAsyncCore(request, cancellationToken);
        }

        protected abstract Task<HttpResponseMessage> SendAsyncCore(
            HttpRequestMessage request, CancellationToken cancellationToken);
    }

    /// <summary>
    /// An <see cref="HttpMessageHandler"/> implementation that counts the number of requests
    /// and facilitates mocking HTTP interactions locally.
    /// </summary>
    internal class MockMessageHandler : CountableMessageHandler
    {
        public string Request { get; private set; }
        
        public HttpStatusCode StatusCode { get; set; }
        public Object Response { get; set; }

        public delegate void SetHeaders(HttpResponseHeaders header);

        public SetHeaders ApplyHeaders { get; set; }

        public MockMessageHandler()
        {
            StatusCode = HttpStatusCode.OK;
        }

        protected override async Task<HttpResponseMessage> SendAsyncCore(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Content != null)
            {
                Request = await request.Content.ReadAsStringAsync();
            }
            else
            {
                Request = null;
            }            
            var resp = new HttpResponseMessage();
            string json;
            if (Response is byte[])
            {
                json = Encoding.UTF8.GetString(Response as byte[]);
            }
            else if (Response is string)
            {
                json = Response as string;
            }
            else
            {
                json = NewtonsoftJsonSerializer.Instance.Serialize(Response);
            }               
            resp.StatusCode = StatusCode;            
            if (ApplyHeaders != null)
            {
                ApplyHeaders(resp.Headers);
            }
            resp.Content = new StringContent(json, Encoding.UTF8, "application/json");
            var tcs = new TaskCompletionSource<HttpResponseMessage>();
            tcs.SetResult(resp);
            return await tcs.Task;
        }
    }
}
