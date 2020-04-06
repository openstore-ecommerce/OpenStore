using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Text;

namespace Nevoweb.DNN.NBrightBuy.Components
{
    public class CacheUtils
    {

        #region "cache"

        public static object GetCache(string cacheKey, string groupid = "")
        {
            cacheKey = GetMd5Hash(cacheKey) + "_groupid:" + groupid;

            ObjectCache cache = MemoryCache.Default;
            if (cache.GetCacheItem(cacheKey) == null)
            {
                return null;
            }
            return cache.GetCacheItem(cacheKey).Value;
        }

        public static void SetCache(string cacheKey, object objObject, string groupid = "")
        {
            if (objObject != null)
            {
                RemoveCache(cacheKey, groupid);

                cacheKey = GetMd5Hash(cacheKey) + "_groupid:" + groupid;

                ObjectCache cache = MemoryCache.Default;
                CacheItemPolicy policy = new CacheItemPolicy();
                var cacheData = new CacheItem(cacheKey, objObject);
                cache.Set(cacheData, policy);

            }
        }

        public static void RemoveCache(string cacheKey, string groupid = "")
        {
            cacheKey = GetMd5Hash(cacheKey) + "_groupid:" + groupid;

            ObjectCache cache = MemoryCache.Default;
            cache.Remove(cacheKey);
        }

        public static void ClearAllCache(string groupid = "")
        {
            try
            {
                ObjectCache cache = MemoryCache.Default;
                List<string> cacheKeys = cache.Select(kvp => kvp.Key).ToList();
                foreach (string cacheKey in cacheKeys)
                {
                    if (groupid == "" || cacheKey.EndsWith("_groupid:" + groupid))
                    {
                        cache.Remove(cacheKey);
                    }
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
        }

        private static string GetMd5Hash(string input)
        {
            var md5 = MD5.Create();
            var inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            var hash = md5.ComputeHash(inputBytes);
            var sb = new StringBuilder();
            foreach (byte t in hash)
            {
                sb.Append(t.ToString("X2"));
            }
            return sb.ToString();
        }


        #endregion

    }
}
