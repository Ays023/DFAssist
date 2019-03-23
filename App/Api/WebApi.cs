﻿using System;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace App
{
    internal static class WebApi
    {
        internal static void customHttpRequest(string status, string name)
        {
            Task.Factory.StartNew(() =>
            {
                var url = "http://" + Settings.customHttpUrl;
                string data = "{ \"status\": \""+status+"\", \"name\" : \""+name+"\" }";

                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "POST";
                    request.ContentType = "application/json";
                    request.Timeout = 30 * 1000;

                    // POST할 데이터를 Request Stream에 쓴다
                    byte[] bytes = Encoding.UTF8.GetBytes(data);
                    request.ContentLength = bytes.Length; // 바이트수 지정

                    using (Stream reqStream = request.GetRequestStream())
                    {
                        reqStream.Write(bytes, 0, bytes.Length);
                    }

                    Log.I("l-POST-send", url, data);

                    // Response 처리
                    string responseText = string.Empty;
                    using (WebResponse resp = request.GetResponse())
                    {
                        Stream respStream = resp.GetResponseStream();
                        using (StreamReader sr = new StreamReader(respStream))
                        {
                            responseText = sr.ReadToEnd();
                            if(responseText == null)
                            {
                                Log.E("l-POST-no-response");
                            }
                            else
                            {
                                Log.S("l-POST-response",responseText);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Ex(ex, "l-POST-failed");
                }
            });
        }

        internal static string Request(string urlfmt, params object[] args)
        {
            try
            {
                var url = string.Format(urlfmt, args);

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                ServicePointManager.DefaultConnectionLimit = 9999;

                var request = (HttpWebRequest)WebRequest.Create(url);
                request.UserAgent = "DFA";
                request.Timeout = 10000;
                request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    var encoding = Encoding.GetEncoding(response.CharacterSet);

                    using (var responseStream = response.GetResponseStream())
                    using (var reader = new StreamReader(responseStream, encoding))
                        return reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                Log.Ex(ex, "l-web-request-failed");
            }

            return null;
        }

        private static string GetMD5Hash(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            var textToHash = Encoding.UTF8.GetBytes(text);
            byte[] result;
            using (MD5 md5 = new MD5CryptoServiceProvider())
                result = md5.ComputeHash(textToHash);

            return BitConverter.ToString(result).Replace("-", "").ToLower();
        }
    }
}
