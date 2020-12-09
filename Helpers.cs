using Newtonsoft.Json;

namespace PlansParser
{
    public static class ObjectExtentions
    {
        public static string ToJson<T>(this T o)
        {
            return JsonConvert.SerializeObject(o);
        }
    }
}
