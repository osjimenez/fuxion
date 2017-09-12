using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Json
{
    public class JsonContainer<TKey>
    {
        protected JsonContainer() { }

        public string Name { get; set; }
        public TKey Key { get; set; }
        [JsonProperty(PropertyName = "Value")]
        public JRaw RawValue { get; set; }

        public static JsonContainer<TKey> Create(object value, TKey key, string name = null)
            => new JsonContainer<TKey>
            {
                Key = key,
                Name = name ?? value.GetType().Name,
                RawValue = new JRaw(value.ToJson())
            };
        public static TContainer Create<TContainer>(object value, TKey key, string name = null) where TContainer : JsonContainer<TKey>, new()
            => new TContainer
            {
                Key = key,
                Name = name ?? value.GetType().Name,
                RawValue = new JRaw(value.ToJson())
            };
        public T As<T>() => RawValue.Value.ToString().FromJson<T>();
        public object As(Type type) => RawValue.Value.ToString().FromJson(type);
        public bool Is<T>() => Is(typeof(T));
        public bool Is(Type type)
        {
            try
            {
                RawValue.Value.ToString().FromJson(type);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("" + ex.Message);
                return false;
            }
        }
    }
}
