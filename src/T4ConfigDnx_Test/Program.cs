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
                var list = array[appSettingSectionArray].ToList();

                // interface builder
                Console.WriteLine("public interface I{0} {{", appSettingSectionArray);

                foreach (var item in list)
                {
                    Console.WriteLine(GetInterfacePropertyString(((JProperty)item).Name, ((JProperty)item).Value.ToString()));
                }


                Console.WriteLine("}");

                // class builder
                Console.WriteLine("public class {0} : I{0} {{", appSettingSectionArray);
                foreach (var item in list)
                {
                    Console.WriteLine(GetClassPrivatePropertyString(((JProperty)item).Name, ((JProperty)item).Value.ToString()));
                    Console.WriteLine(GetClassPublicPropertyString(((JProperty)item).Name, ((JProperty)item).Value.ToString()));
                }

                Console.WriteLine(GetSettingMethod());

                Console.WriteLine("}");

            }

            Console.ReadLine();
        }

        private static string GetInterfacePropertyString(string key, string value)
        {
            var type = GetTypeString(value);
            return string.Format(@"{0} {1} {{ get; }}", type, key);
        }

        private static string GetClassPrivatePropertyString(string key, string value)
        {

            var type = GetTypeString(value);
            return $@"private static readonly Lazy<{type}> _{LowerFirst(key)} = new Lazy<{type}>(() => {GetConversion(type, key)});";
        }

        private static string GetClassPublicPropertyString(string key, string value)
        {
            var type = GetTypeString(value);
            return $"public virtual {type} {key} =>  _{LowerFirst(key)}.Value;";
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

        public static string GetSettingMethod()
        {
            return @"public static string GetSetting(string key) {{ return key; }}";
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
