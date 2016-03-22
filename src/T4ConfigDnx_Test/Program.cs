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
            var settingsFile = "appSettings.json";            // in case you have a custom setting json file name.
            //var createObjectInterface = true;               // The use of a custom interface, or set to false if useing IOptions<T>

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


        private static string BuildInterface(List<JToken> tokens, string sectionName, bool useIOptionInterface)
        {
            if (useIOptionInterface)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            sb.Append("\t").AppendLine("public interface I" + sectionName);
            sb.Append("\t").AppendLine("{");

            foreach (var item in tokens)
            {
                sb.Append("\t").Append("\t").AppendLine(GetInterfacePropertyString(((JProperty)item).Name, ((JProperty)item).Value.ToString()));
            }

            sb.Append("\t").AppendLine("}");
            return sb.ToString();
        }

        private static string BuildSettingClass(List<JToken> tokens, string sectionName, bool useIOptionInterface)
        {
            var sb = new StringBuilder();
            if (useIOptionInterface)
            {
                sb.Append("\t").AppendLine(string.Format("public class {0} ", sectionName));
            }
            else
            {
                sb.Append("\t").AppendLine(string.Format("public class {0} : I{0} ", sectionName));
            }
            sb.Append("\t").AppendLine("{");

            foreach (var item in tokens)
            {
                if (useIOptionInterface)
                {
                    sb.Append("\t").Append("\t").AppendLine(GetClassPublicAutoProperty(((JProperty)item).Name, ((JProperty)item).Value.ToString()));
                }
                else
                {
                    sb.Append("\t").Append("\t").AppendLine(GetClassPrivatePropertyString(((JProperty)item).Name, ((JProperty)item).Value.ToString()));
                    sb.Append("\t").Append("\t").AppendLine(GetClassPublicPropertyString(((JProperty)item).Name, ((JProperty)item).Value.ToString()));
                }
            }

            if (!useIOptionInterface)
            {
                sb.AppendLine(Environment.NewLine);
                sb.Append("\t").Append("\t").AppendLine(GetSettingMethod());
            }

            sb.Append("\t").AppendLine("}");
            return sb.ToString();
        }

        private static string BuildFileHeader(object ns)
        {
            var sb = new StringBuilder();
            sb.AppendLine("namespace " + ns);
            sb.AppendLine("{");
            sb.Append("\t").AppendLine("using System;");

            return sb.ToString();
        }

        private static string BuildFileClose()
        {
            return "}";
        }

        private static string GetInterfacePropertyString(string key, string value)
        {
            var type = GetTypeString(value);
            return string.Format(@"{0} {1} {{ get; }}", type, key);
        }

        private static string GetClassPrivatePropertyString(string key, string value)
        {

            var type = GetTypeString(value);
            return string.Format("private static readonly Lazy<{0}> _{1} = new Lazy<{0}>(() => {2});", type, LowerFirst(key), GetConversion(type, key));
        }

        private static string GetClassPublicPropertyString(string key, string value)
        {
            var type = GetTypeString(value);
            return string.Format("public virtual {0} {1} =>  _{2}.Value;", type, key, LowerFirst(key));
        }

        private static string GetClassPublicAutoProperty(string key, string value)
        {
            var type = GetTypeString(value);
            return string.Format("public {0} {1} {{ get; set; }}", type, key);
        }

        private static string GetConversion(string type, string key)
        {
            switch (type)
            {
                case "Guid":
                    return string.Format(@"new Guid(GetSetting(""{0}""))", key);
                case "int":
                    return string.Format(@"Convert.ToInt32(GetSetting(""{0}""))", key);
                case "bool":
                    return string.Format(@"Convert.ToBoolean(GetSetting(""{0}""))", key);
                case "decimal":
                    return string.Format(@"Convert.ToDecimal(GetSetting(""{0}""))", key);
                default:
                    return string.Format(@"GetSetting(""{0}"")", key);
            }
        }

        private static string GetSettingMethod()
        {
            return "public static string GetSetting(string key) {{ return key; }}";
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

        private static string LowerFirst(string text)
        {
            return char.ToLower(text[0]) + text.Substring(1);
        }
    }

    



}
