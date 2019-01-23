using Flurl.Http.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.AspNet
{
    public interface IWebApiProxyProvider
    {
        string BaseUrl { get; }

        List<KeyValuePair<string, string>> Headers { get; }

        Task<T> Put<T>(T entity, string path, bool anonymous = false, bool useBaseUrl = true);
        Task<T> Post<T>(string path, bool anonymous = false, bool useBaseUrl = true);
        Task<T> Post<T>(T entity, string path, bool anonymous = false, bool useBaseUrl = true);
        Task<TResponse> Post<TRequest, TResponse>(TRequest request, string path, bool anonymous = false, bool useBaseUrl = true);
        Task<T> Patch<T>(string path, dynamic patch, bool anonymous = false, bool useBaseUrl = true) where T : class;
        Task<T> Delete<T>(string path);
        Task Get(string path, bool anonymous = false, bool useBaseUrl = true);
        Task<T> Get<T>(string path, bool anonymous = false, bool useBaseUrl = true);
        Task<List<T>> GetList<T>(string path, bool anonymous = false, bool useBaseUrl = true);

        void AddCallInterceptor(Action<string, bool, bool, FlurlHttpSettings> interceptionCallAction);
    }
}
