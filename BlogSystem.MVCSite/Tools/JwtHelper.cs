using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlogSystem.MVCSite.Tools
{
    public class JwtHelper
    {
        //私钥  web.config中配置
        //"GQDstcKsx0NHjPOuXOYg5MbeJ1XT0uFiwDVvVBrk";
        private static string secret = "xiaowo";

        /// <summary>
        /// 生成JwtToken
        /// </summary>
        /// <returns></returns>
        public static string SetJwtEncode(string userId, int second)
        {

            //格式如下
            IDateTimeProvider provider = new UtcDateTimeProvider();
            var now = provider.GetNow();
            var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            //过期时间
            var secondsSinceEpoch = Math.Round((now - unixEpoch).TotalSeconds);

            var payload = new Dictionary<string, object>
           {
               { "exp", secondsSinceEpoch+second },  //86400秒后过期 一天
               { "userId",userId }
           };

            IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
            IJsonSerializer serializer = new JsonNetSerializer();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

            var token = encoder.Encode(payload, secret);
            return token;
        }

        /// <summary>
        /// 根据jwtToken  获取实体
        /// </summary>
        /// <param name="token">jwtToken</param>
        /// <returns></returns>
        public static bool GetJwtDecode(string token, out string userId, out string message)
        {
            try
            {
                IJsonSerializer serializer = new JsonNetSerializer();
                IDateTimeProvider provider = new UtcDateTimeProvider();
                IJwtValidator validator = new JwtValidator(serializer, provider);
                IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
                IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder);
                //token为之前生成的字符串
                var userInfo = decoder.DecodeToObject(token, secret, verify: true);
                //此处json为IDictionary<string, object> 类型
                userId = userInfo["userId"].ToString();  //可获取当前id
                message = "成功";
                return true;

                //double exp = double.Parse(userInfo["exp"].ToString());  //可获取过期时间
                //var now = provider.GetNow();
                //var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                ////过期时间
                //var secondsSinceEpoch = Math.Round((now - unixEpoch).TotalSeconds);
                //if (secondsSinceEpoch >= exp)//当前时间大于过期时间 已经过期
                //{
                //    userId = null;
                //    message = "token已过期";
                //    return false;
                //}
                //else
                //{
                //    message = "成功";
                //    return true;
                //}
            }
            catch (TokenExpiredException)
            {
                message = "token已过期";
            }
            catch (SignatureVerificationException)
            {
                message = "token签名无效";
            }
            catch (Exception)
            {
                message = "token不可为空";
            }
            userId = null;
            return false;
        }
    }
}