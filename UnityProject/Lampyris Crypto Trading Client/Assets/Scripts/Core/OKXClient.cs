/*
 * Copyright (C) 2024 The Hong-Jin Investment Company.
 * This file is part of the OKX Trading Client.
 * File created at 2024-12-04
*/

using System;
using System.Collections;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using HongJinInvestment.OKX.Client.Data;
using HongJinInvestment.OKX.Client.Common;

namespace HongJinInvestment.OKX.Client.Core
{
    public class OKXClient:Singleton<OKXClient>
    {
        private LoginToken          m_usingLoginToken;
        private readonly HttpClient m_httpClient;
        private readonly int        m_timestampPaddingSeconds = 10;
        
        // 用于请求OKX的时间戳，需要添加到请求头部
        private readonly string     m_timestamp;

        // 欧意请求的网址基址
        private const string        c_baseUrl = "https://www.okex.com";

        public OKXClient()
        {
            m_httpClient = new HttpClient
            {
                BaseAddress = new Uri(c_baseUrl)
            };

            // 添加一个偏移m_timestampPaddingSeconds秒后 生成时间戳字符串
            m_timestamp = DateTimeOffset.UtcNow.AddSeconds(m_timestampPaddingSeconds).ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        }

        /// <summary>
        /// 本函数将得到OK-ACCESS-SIGN，需要对timestamp + method + requestPath + body字符串（+表示字符串连接），
        /// 以及SecretKey，使用HMAC SHA256方法加密，并通过Base-64编码输出
        /// </summary>
        /// <param name="timestamp">时间戳字符串</param>
        /// <param name="method">请求方法，字母全部大写：GET/POST</param>
        /// <param name="requestPath">请求接口路径。如：/api/v5/account/balance</param>
        /// <param name="secretKey">用户申请APIKey时所生成</param>
        /// <param name="body">请求主体的字符串（可省略），如：{"instId":"BTC-USDT","lever":"5","mgnMode":"isolated"}</param>
        /// <returns>字符串类型的APIKey</returns>
        private static string GenerateSignature(string timestamp, 
                                                string method, 
                                                string requestPath, 
                                                string secretKey,
                                                string body = "")
        {
            var preHashString = $"{timestamp}{method.ToUpper()}{requestPath}{body}";
            var secretBytes = Encoding.UTF8.GetBytes(secretKey);
            
            using (var hmac = new HMACSHA256(secretBytes))
            {
                var signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(preHashString));
                return Convert.ToBase64String(signatureBytes);
            }
        }

        public HttpRequestMessage CreateRequest(string method, string requestPath, string body = "")
        {
            if (m_usingLoginToken == null)
                return null;
            
            var signature = GenerateSignature(m_timestamp, method, requestPath, m_usingLoginToken.secretKey, body);

            var request = new HttpRequestMessage(new HttpMethod(method), requestPath);
            request.Headers.Add("OK-ACCESS-KEY", m_usingLoginToken.key);
            request.Headers.Add("OK-ACCESS-SIGN", signature);
            request.Headers.Add("OK-ACCESS-TIMESTAMP", m_timestamp);
            request.Headers.Add("OK-ACCESS-PASSPHRASE", m_usingLoginToken.passPhrase); 
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");

            return request;
        }

        public IEnumerator SendRequest(string url, System.Action<string> callback)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                // 发送请求并等待响应
                yield return webRequest.SendWebRequest();

                // 检查请求结果
                if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("Error: " + webRequest.error);
                    callback?.Invoke(null);
                }
                else
                {
                    // 获取响应内容
                    string responseContent = webRequest.downloadHandler.text;
                    callback?.Invoke(responseContent);
                }
            }
        }
    }
}