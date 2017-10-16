using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using Fuxion.Net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Web
{
    public class WebApiProxyProvider : ConnectableNotifier<WebApiProxyProvider>, IWebApiProxyProvider
    {
        public WebApiProxyProvider(string baseUrl) { BaseUrl = baseUrl; }
        public string BaseUrl { get; }
        public const string API_SEGMENT = "api";

        public List<KeyValuePair<string, string>> Headers { get; } = new Dictionary<string, string>
        {
            {"Accept", "application/json"},
            {"Accept-Language", System.Globalization.CultureInfo.CurrentCulture.ToString() },
        }.ToList();

        List<Action<string, bool, bool, FlurlHttpSettings>> callInterceptors = new List<Action<string, bool, bool, FlurlHttpSettings>>();
        public void AddCallInterceptor(Action<string, bool, bool, FlurlHttpSettings> interceptionCallAction)
        {
            callInterceptors.Add(interceptionCallAction);
        }

        #region Common methods
        internal IFlurlRequest GetClient(Url pathSegment = null, bool anonymous = false, bool useBaseUrl = true)
        {
            var url = useBaseUrl ? BaseUrl
                .AppendPathSegment(API_SEGMENT)
                .AppendPathSegment(pathSegment.Path)
                .SetQueryParams(pathSegment.QueryParams) : pathSegment;
            IFlurlRequest client = new FlurlRequest(url);
            foreach (var hea in Headers)
                client = client.WithHeader(hea.Key, hea.Value);
            //var client = url
            //    //.WithHeader("Content-Type", "application/json")
            //    .WithHeader("Accept", "application/json")
            //    //.WithHeader("User-Agent", Platform + "|" + AppName + "|" + GetType().GetTypeInfo().Assembly.GetName().Version)
            //    .WithHeader("Accept-Language", System.Globalization.CultureInfo.CurrentCulture.ToString());

            client = client.WithHeader("ClientVersion", "1.0.0");
            foreach (var inter in callInterceptors)
                client.ConfigureRequest(httpClient =>
                {
                    inter(pathSegment, anonymous, useBaseUrl, httpClient);
                });
            //if (!anonymous)
            //{
            //    var authHeader = GetAuthorizationHeader();
            //    client = client.WithHeader(authHeader.Key, authHeader.Value);
            //}
            return client;
        }
        internal TResult Run<TResult>(Func<TResult> function)
        {
            try
            {
                return function();
            }
            catch (FlurlHttpException ex)
            {
                if (!string.IsNullOrEmpty(ex.Call.ErrorResponseBody))
                {
                    try
                    {
                        var response = JsonConvert.DeserializeObject<DebugErrorResponse>(ex.Call.ErrorResponseBody);
                        if (response.ExceptionStackTrace == null)
                            throw new ServiceException(JsonConvert.DeserializeObject<ErrorResponse>(ex.Call.ErrorResponseBody));
                        throw new ServiceException(response);
                    }
                    catch (Exception ex2)
                    {
                        throw new ServiceException("Unexpected error", new List<string>() { ex2.Message });
                    }
                }
                throw new ServiceException("Unexpected error", new[] { ex.Call.Exception.Message });
            }
            catch (Exception ex)
            {
                throw new ServiceException("Unexpected error", new List<string>() { ex.Message });
            }
        }
        public Task<T> Put<T>(T entity, string path, bool anonymous = false, bool useBaseUrl = true)
        {
            return Run(() =>
                GetClient(path, anonymous, useBaseUrl)
                    .PutJsonAsync(entity)
                    .ReceiveJson<T>());
        }
        public Task<T> Post<T>(string path, bool anonymous = false, bool useBaseUrl = true)
        {
            return Run(() =>
                GetClient(path, anonymous, useBaseUrl)
                    .PostAsync(null)
                    .ReceiveJson<T>());
        }
        public Task<T> Post<T>(T entity, string path, bool anonymous = false, bool useBaseUrl = true)
        {
            return Run(() =>
                GetClient(path, anonymous, useBaseUrl)
                    .PostJsonAsync(entity)
                    .ReceiveJson<T>());
        }
        public Task<TResponse> Post<TRequest, TResponse>(TRequest request, string path, bool anonymous = false, bool useBaseUrl = true)
        {
            return Run(() =>
                GetClient(path, anonymous, useBaseUrl)
                    .PostJsonAsync(request)
                    .ReceiveJson<TResponse>());
        }
        public Task<T> Patch<T>(string path, dynamic patch, bool anonymous = false, bool useBaseUrl = true) where T : class
        {
            var sss = ((object)patch).ToJson();
            return Run(() =>
                GetClient(path, anonymous, useBaseUrl)
                    .PatchJsonAsync((object)patch)
                    .ReceiveJson<T>());
        }
        public Task<T> Delete<T>(string path)
        {
            return Run(() =>
                GetClient(path)
                    .DeleteAsync()
                    .ReceiveJson<T>());
        }
        public Task Get(string path, bool anonymous = false, bool useBaseUrl = true)
        {
            return Run(() =>
                GetClient(path, anonymous, useBaseUrl)
                    .GetAsync());
        }
        public Task<T> Get<T>(string path, bool anonymous = false, bool useBaseUrl = true)
        {
            return Run(() =>
                GetClient(path, anonymous, useBaseUrl)
                    .GetJsonAsync<T>());
        }
        public Task<List<T>> GetList<T>(string path, bool anonymous = false, bool useBaseUrl = true)
        {
            return Run(() =>
                GetClient(path, anonymous, useBaseUrl)
                    .GetJsonAsync<List<T>>());
        }
        #endregion
        protected override Task OnConnect()
        {
            throw new NotImplementedException();
        }
        protected override Task OnDisconnect()
        {
            throw new NotImplementedException();
        }
    }
}
