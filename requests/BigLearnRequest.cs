using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using ServiceStack;

namespace XHRTools.requests
{
    public abstract class BigLearnRequest<TR> where TR : BigLearnResult
    {
        private const string DefaultUseragent = "Mozilla/4.0 (compatible; MSIE 6.0;) JavaAjax/1.0";
        private readonly Uri _requestUri;
        private readonly string _method;

        protected BigLearnRequest(Uri requestUri, string method)
        {
            _requestUri = requestUri;
            this._method = method;
        }

        protected abstract TR ParseResult(string response);

        protected virtual bool CheckResult(TR result)
        {
            // Console.WriteLine(jObject["msg"].Value<string>());
            return result.IsSuccess();
        }

        protected virtual string GetRequestBody()
        {
            return "";
        }

        public TR PostRequest(BigLearnClient client)
        {
            var httpWebRequest = WebRequest.CreateHttp(_requestUri);
            httpWebRequest.Method = _method;
            httpWebRequest.CookieContainer = client.CookieContainer;
            SetHeaders(httpWebRequest);
            var requestBody = GetRequestBody();
            if (!requestBody.IsEmpty())
            {
                SetRequestBody(httpWebRequest, requestBody);
            }
            
            var responseContent = ReadResponse(httpWebRequest.GetResponse());
            var result = ParseResult(responseContent);
            if (CheckResult(result))
            {
                return result;
            }
            throw new WebException(GetErrorMsg(result));
        }

        protected virtual string GetErrorMsg(TR bigLearnResult)
        {
            return "请求失败!";
        }

        protected virtual void SetHeaders(HttpWebRequest request)
        {
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");
            request.Headers.Add("UserAgent", DefaultUseragent);
            request.ContentType = "";
            request.Referer = "http://qndxx.bestcood.com/nanning/daxuexi";
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.Accept = "text/javascript, text/html, application/xml, application/json, text/xml, */*";
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            request.Headers.Remove("Accept-Language");
            request.Headers.Add("Upgrade-Insecure-Requests", @"1");
            request.Expect = "";
        }


        
        private static string ReadResponse(WebResponse response)
        {
            return new StreamReader(response.GetResponseStream() ?? throw new InvalidOperationException())
                .ReadToEnd();
        }
        
        private static void SetRequestBody(WebRequest request, string content)
        {
            var escapeDataString = Encoding.UTF8.GetBytes(Uri.EscapeUriString(content));
            using var requestStream = request.GetRequestStream();
            requestStream.Write(escapeDataString,0, escapeDataString.Length);
            requestStream.Close();
        }
    }
}