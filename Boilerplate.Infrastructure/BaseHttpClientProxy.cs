using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Boilerplate.Infrastructure
{
    public abstract class BaseHttpClientProxy
    {
        private HttpClient _client;
        private static readonly JsonSerializerOptions options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };

        public BaseHttpClientProxy(HttpClient client)
        {
            _client = client;
        }

        protected async Task<FinalClass> SendJson<IntermediateClass, FinalClass>(HttpMethod method, string route, Func<IntermediateClass, HttpStatusCode, FinalClass> processResponse, object body = null)
        {
            // may seem excessive, but it's necessary for high quality error logs
            // this has saved me from dozens of hours of debugging many times over
            // we want (for every error) to have (if available): full url, http status, response body, exception            

            var request = new HttpRequestMessage(method, _client.BaseAddress + route);

            if (body != null) request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            try
            {

                response = await _client.SendAsync(request);
            }
            catch (System.Exception ex)
            {
                throw new DownstreamException($"{GetEndpoint()} threw exception [{ex.Message}]", ex);
            }

            string contentBody = string.Empty;
            try
            {
                contentBody = await response.Content.ReadAsStringAsync();
            }
            catch (System.Exception ex)
            {
                ThrowDownstreamErrorWithResponseInfo(ex, "reading response body");
            }

            IntermediateClass result = default;
            try
            {
                result = JsonSerializer.Deserialize<IntermediateClass>(contentBody, options);
            }
            catch (System.Exception ex)
            {
                ThrowDownstreamErrorWithResponseInfo(ex, "parsing response to class");
            }

            try
            {
                return processResponse(result, response.StatusCode);
            }
            catch (System.Exception ex)
            {
                ThrowDownstreamErrorWithResponseInfo(ex, "processing response");
            }

            return default; // should never get here, but just to make compiler happy

            void ThrowDownstreamErrorWithResponseInfo(System.Exception ex, string action)
            {
                var message = $"{GetEndpoint()} had exception [{ex.Message}] while [{action}], response HttpStatus={(int)response.StatusCode} and body=[{contentBody}]";

                throw new DownstreamException(message, ex);
            }

            string GetEndpoint()
            {
                return $"{request.Method} {request.RequestUri}";
            }
        }
    }
}
