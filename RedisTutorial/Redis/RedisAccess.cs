using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using StackExchange.Redis;
using System.Reflection;
using Newtonsoft.Json;
using System.Text;
using System.Security.Cryptography;
using RedisTutorial.Models;
namespace RedisTutorial.Redis
{
    public class RedisAccess
    {
        private static string CONNECTION_STRING = "localhost:6379";
        private static int DBID = 1;
        private static readonly Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(CONNECTION_STRING));
        private static IDatabase cache = lazyConnection.Value.GetDatabase(DBID);
        private static IServer serverCache = lazyConnection.Value.GetServer(CONNECTION_STRING);

        public static List<T> Get<T>()
        {
            List<T> result = new List<T>();

            if (cache.KeyExists(typeof(T).Name + "Table"))
                result = JsonConvert.DeserializeObject<List<T>>(cache.StringGet(typeof(T).Name + "Table"));

            return result;
        }

        public static void Set<T>(T item)
        {
            var res = Get<T>();
            res.Add(item);
            cache.StringSetAsync(typeof(T).Name + "Table", JsonConvert.SerializeObject(res));
        }

        private static int GetMaxHashIndex(string name)
        {
            return serverCache.Keys(DBID, name + ":*").Count();
        }

        private static IEnumerable<string> GetAllHashKeys(string name)
        {
            return serverCache.Keys(DBID, name + ":*").Select(x => x.ToString());
        }

        public static List<Human> HGet()
        {
            List<Human> result = new List<Human>();

            var keys = GetAllHashKeys(typeof(Human).Name);
            foreach (var key in keys) {
                var item = cache.HashGetAll(key);
                result.Add(new Human() { Name = item[0].Value, DOB = Convert.ToDateTime(item[1].Value), Address = item[2].Value, Email = item[3].Value, Password = item[4].Value });
            }
            return result;
        }

        public static void HSet<T>(T item)
        {
            var index = GetMaxHashIndex(typeof(T).Name);
            cache.HashSetAsync(typeof(T).Name + ":" + index, item.ToHashEntries());
        }
    }

    static class Encryption
    {
        public static string Crypt(this string text)
        {
            return Convert.ToBase64String(
                    Encoding.Unicode.GetBytes(text));
        }

        public static string Derypt(this string text)
        {
            return Encoding.Unicode.GetString(
                     Convert.FromBase64String(text));
        }
    }

    static class HashConverter
    {
        public static HashEntry[] ToHashEntries(this object obj)
        {
            PropertyInfo[] properties = obj.GetType().GetProperties();
            return properties
                .Where(x => x.GetValue(obj) != null) // <-- PREVENT NullReferenceException
                .Select
                (
                      property =>
                      {
                          object propertyValue = property.GetValue(obj);

                          // This will detect if given property value is 
                          // enumerable, which is a good reason to serialize it
                          // as JSON!
                          if (propertyValue is IEnumerable<object>)
                          {
                              // So you use JSON.NET to serialize the property
                              // value as JSON
                              return new HashEntry(property.Name, JsonConvert.SerializeObject(propertyValue));
                          }
                          else if (propertyValue is int)
                          {
                              return new HashEntry(property.Name, (int)propertyValue);
                          }
                          else
                          {
                              return new HashEntry(property.Name, propertyValue.ToString());
                          }

                      }
                )
                .ToArray();
        }
    }
}