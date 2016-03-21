using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T4ConfigDnx_Test
{
    using System.IO;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    class Program
    {
        static void Main(string[] args)
        {
            var appSettingSectionArray = "AppSettings";       // A list of json setting sections you whish to create classes for
            var settingsFile = "appSettings.json";                      // in case you have a custom setting json file name.
            //var createObjectInterface = true;                           // The use of a custom interface, or set to false if useing IOptions<T>

            var path = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory) ?? "";
            var settingsFilePath = Path.Combine(path, settingsFile);


            using (StreamReader file = File.OpenText(settingsFilePath))
            {
                string json = file.ReadToEnd();
                var array = (JObject)JsonConvert.DeserializeObject(json);


                foreach (var item in array[appSettingSectionArray].ToList())
                {
                    Console.WriteLine(GetPropertyString(((JProperty)item).Name, ((JProperty)item).Value.ToString()));
                }



            }

            Console.ReadLine();
        }



        private static string GetPropertyString(string key, string value)
        {

            var type = GetTypeString(value);

            // {0} = type
            // {1} = property lower first
            // {2} = conversion func call
             
            return string.Format("private static readonly Lazy<{0}> _{1} = new Lazy<{0}>({2});", type, LowerFirst(key), GetConversion(type, key));
        }

        private static string GetConversion(string type, string key)
        {
            switch (type)
            {
                case "Guid":
                    return $@"new Guid(GetSetting(""{key}""))";
                case "int":
                    return $@"Convert.ToInt32(GetSetting(""{key}""))";
                case "bool":
                    return $@"Convert.ToBoolean(GetSetting(""{key}""))";
                case "decimal":
                    return $@"Convert.ToDecimal(GetSetting(""{key}""))";
                default:
                    return $@"GetSetting(""{key}"")";
            }
        }

        private static string GetTypeString(object value)
        {
            Guid guid;
            if (Guid.TryParse(value.ToString(), out guid))
            {
                return "Guid";
            }

            int i;
            if (int.TryParse(value.ToString(), out i))
            {
                return "int";
            }

            decimal d;
            if (decimal.TryParse(value.ToString(), out d))
            {
                return "decimal";
            }

            bool b;
            if (bool.TryParse(value.ToString(), out b))
            {
                return "bool";
            }

            return "string";
        }
        public static string LowerFirst(string text)
        {
            return char.ToLower(text[0]) + text.Substring(1);
        }

    }
}
