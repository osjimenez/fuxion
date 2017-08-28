//using Fuxion.ComponentModel;
//using Fuxion.Net;
//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Http;
//using System.Text;
//using System.Threading.Tasks;

//namespace Fuxion.Web
//{
//    public class BearerAuthenticationWebApiProxy : Notifier<BearerAuthenticationWebApiProxy>, IWebApiProxy
//    {
//        public BearerAuthenticationWebApiProxy(IWebApiProxyProvider provider)
//        {
//            Provider = provider;
//            Provider.AddCallInterceptor((url, anonymous, useBaseUrl, httpClient) => {
//                if (!anonymous)
//                {
//                    var authHeader = GetAuthorizationHeader();
//                    httpClient.DefaultRequestHeaders.Add(authHeader.Key, authHeader.Value);
//                    //client = client.WithHeader(authHeader.Key, authHeader.Value);
//                }
//            });
//        }
//        public IWebApiProxyProvider Provider { get; }

//        public const string LOGIN_SEGMENT = "Token";
//        public bool LoginInProcess { get { return GetValue<bool>(); } private set { SetValue(value); } }
//        public bool IsLogged { get { return IsConnected && Identity.Current != null; } }
//        public string Token { get { return GetValue<string>(); } set { SetValue(value); } }
//        public TokenType TokenType { get { return GetValue<TokenType>(); } set { SetValue(value); } }
//        public ServiceProxyIdentity Identity { get; private set; }
//        public async Task Login(string username, string password)
//        {
//            RaisePropertyChanged(() => IsLogged, IsLogged, false);
//            TokenType = TokenType.Bearer;
//            Token = null;
//            LoginInProcess = true;
//            HttpResponseMessage response;
//            using (var client = new HttpClient())
//            {
//                response = await client.PostAsync(
//                    new Uri(new Uri(Provider.BaseUrl), LOGIN_SEGMENT),
//                    new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
//                    {
//                        new KeyValuePair<string, string>( "grant_type", "password" ),
//                        new KeyValuePair<string, string>( "username", username ),
//                        new KeyValuePair<string, string>( "password", password )
//                    }));
//            }
//            var responseContent = await response.Content.ReadAsStringAsync();
//            if (!response.IsSuccessStatusCode)
//            {
//                LoginInProcess = false;
//                throw new Exception(string.Format("Error: {0}", responseContent));
//            }
//            Dictionary<string, string> tokenDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);
//            Token = tokenDictionary["access_token"];
//            LoginInProcess = false;
//            RaisePropertyChanged(() => IsLogged, false, IsConnected && true);
//        }
//        //protected async override Task OnConnect()
//        //{
//        //    //var res = await Provider.Get<string>("Service/Version", true);
//        //    //var semVer = new SemVer.Version(res);
//        //    return Task.FromResult(0);
//        //}
//        //protected override Task OnDisconnect()
//        //{
//        //    Token = null;
//        //    Identity.Current = null;
//        //    return Task.FromResult(0);
//        //}
//        internal KeyValuePair<string, string> GetAuthorizationHeader()
//        {
//            var authoType = "";
//            //switch (TokenType)
//            //{
//            //    case TokenType.Basic:
//            //        authoType = "Basic";
//            //        break;
//            //    case TokenType.Bearer:
//                    authoType = "Bearer";
//            //        break;
//            //    default:
//            //        break;
//            //}
//            return new KeyValuePair<string, string>("Authorization", authoType + " " + Token);
//        }
//    }
//}
