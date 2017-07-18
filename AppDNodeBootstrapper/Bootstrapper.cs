using Apprenda.API.Extension.Bootstrapping;
using Apprenda.API.Extension.CustomProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using AppDNodeBootstrapper.Models;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Apprenda.Services.Logging;

namespace AppDNodeBootstrapper
{
    public class Bootstrapper : BootstrapperBase
    {
        private static readonly ILogger log =
                LogManager.Instance().GetLogger(typeof(Bootstrapper));
        public override BootstrappingResult Bootstrap(BootstrappingRequest request)
        {
            try
            {
                log.Info("AppD Bootstrapper starting");
                
                var requestAPM  = request.Properties.First(p => p.Name == CustomProperties.APMEnable).Values.First();

                if (requestAPM != "Yes")
                {
                    log.Info("AppD instrumentation not requested");
                    return BootstrappingResult.Success();
                }

                var yamlFilePaths = Directory.EnumerateFiles(request.ComponentPath, "*.yml")
                    .Concat(Directory.EnumerateFiles(request.ComponentPath, "*.yaml"));

                foreach (var yamlFilePath in yamlFilePaths)
                {
                    var obj = ReadYaml(yamlFilePath);
                    var spec = GetOrCreateYamlNode(obj, "spec");
                    var template = GetOrCreateYamlNode(spec, "template");
                    var contSpec = GetOrCreateYamlNode(template, "spec");
                    var containers = GetOrCreateYamlNodeList(contSpec, "containers");

                    int i = 0;
                    foreach(var c in containers)
                    {
                        if (i == 0)
                        {
                            var env = GetOrCreateYamlNodeList(((Dictionary<object, object>)c), "env");
                            AddEnvVariables(ref env, request);
                            break;
                        }
                    }

                    WriteYaml(yamlFilePath, obj);
                }

                log.Info("AppD instrumentation is complete");

                //var jsonFilePaths = Directory.EnumerateFiles(request.ComponentPath, "*.json");

                //foreach (var jsonFilePath in jsonFilePaths)
                //{
                //    var obj = (JObject)JsonConvert.DeserializeObject(File.ReadAllText(jsonFilePath));
                //    var spec = GetOrCreateJsonNode(obj, "spec");
                //    var template = GetOrCreateJsonNode(spec, "template");
                //    var contSpec = GetOrCreateJsonNode(template, "spec");
                //    var containers = GetOrCreateJsonNode(contSpec, "containers");


                //    File.WriteAllText(jsonFilePath, JsonConvert.SerializeObject(obj, Formatting.Indented));
                //}
            }
            catch (Exception ex)
            {
                log.Error("Error instrumenting perf monitoring for the app" + ex.Message);
                return BootstrappingResult.Failure(new[] { $"Error instrumenting perf monitoring for the app. {ex}" });
            }

            return BootstrappingResult.Success();
        }

        private static void AddEnvVariables(ref List<object> env, BootstrappingRequest request)
        {
            string appname = string.Empty;
            string apptier = string.Empty;
            string url = string.Empty;
            string account = string.Empty;
            string key = string.Empty;

            //AppD Controller URL
            try
            {
                url = request.Properties.First(p => p.Name == CustomProperties.AppdController).Values.First();
                env.Add(BuildEnvVar(AppDVariables.AppdController, url));
            }
            catch (Exception ex)
            {
                throw new Exception("AppD Controller URL is not configured. Contact Platform Operator", ex);
            }


            //AppD Account
            try
            {
                account = request.Properties.First(p => p.Name == CustomProperties.AppdAccount).Values.First();
                env.Add(BuildEnvVar(AppDVariables.AppdAccount, account));
            }
            catch (Exception ex)
            {
                throw new Exception("AppD Account is not configured. Contact Platform Operator", ex);
            }

            //AppD Account Key
            try
            {
                key = request.Properties.First(p => p.Name == CustomProperties.AppdKey).Values.First();
                env.Add(BuildEnvVar(AppDVariables.AppdKey, key));
            }
            catch (Exception ex)
            {
                throw new Exception("AppD Account Key is not configured. Contact Platform Operator", ex);
            }

            //App Tier.
            try
            {
                apptier = request.Properties.First(p => p.Name == CustomProperties.AppdAppTier).Values.First();
                env.Add(BuildEnvVar(AppDVariables.AppdAppTier, apptier));
            }
            catch (Exception ex)
            {
                throw new Exception("AppD App Tier is not configured. Contact Platform Operator", ex);
            }


            //Optional App Name. If not set, derived from the App Alias
            try
            {
                appname = request.Properties.First(p => p.Name == CustomProperties.AppdAppName).Values.First();
            }
            catch(Exception)
            {

            }
            if (string.IsNullOrEmpty(appname))
            {
                appname = request.ApplicationAlias.ToUpper();
            }
            env.Add(BuildEnvVar(AppDVariables.AppdAppName, appname));
        }


        private static Dictionary<object, object> BuildEnvVar(string name, string val)
        {
            Dictionary<object, object> ev = new Dictionary<object, object>();
            ev.Add("name", name);
            ev.Add("value", val);
            return ev;
        }

       
        private static Dictionary<object, object> GetOrCreateYamlNode(Dictionary<object, object> parent, string child)
        {
            object result;

            if (parent.TryGetValue(child, out result))
            {
                return (Dictionary<object, object>)result;
            }

            var created = new Dictionary<object, object>();

            parent[child] = created;

            return created;
        }

        private static List<object> GetOrCreateYamlNodeList(Dictionary<object, object> parent, string child)
        {
            object result;

            if (parent.TryGetValue(child, out result))
            {
               return (List<object>)result;      
            }

            var  created = new List<object>();
            parent[child] = created;

            return created;
        }

        private static JObject GetOrCreateJsonNode(JObject parent, string child)
        {
            JToken result;

            if (parent.TryGetValue(child, out result))
            {
                return (JObject)result;
            }

            var created = new JObject();

            parent[child] = created;

            return created;
        }

        private static void WriteYaml(string yamlFilePath, Dictionary<object, object> obj)
        {
            var builder = new SerializerBuilder();
            var serializer = builder.WithNamingConvention(new CamelCaseNamingConvention()).Build();

            using (var writer = new StreamWriter(yamlFilePath))
            {
                serializer.Serialize(writer, obj);
            }
        }

        private static Dictionary<object, object> ReadYaml(string filePath)
        {
            var builder = new DeserializerBuilder();
            var deserializer = builder.IgnoreUnmatchedProperties()
                                      .WithNamingConvention(new CamelCaseNamingConvention())
                                      .Build();

            using (var reader = File.OpenText(filePath))
            {
                return deserializer.Deserialize<Dictionary<object, object>>(reader);
            }
        }
    }
}
