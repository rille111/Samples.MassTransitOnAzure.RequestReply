using System.IO;
using Newtonsoft.Json.Linq;

namespace RequestReply.Shared.Tools
{
    public class JsonConfigFileReader
    {
        private readonly string _jsonFilePath;

        public JsonConfigFileReader(string jsonFilePath = "./config.json")
        {
            _jsonFilePath = jsonFilePath;
        }

        /// <summary>
        /// Scans jsonfile that is expected to be in the following example: {"SomeKey": "SomeValue", "SomeOtherKey":"SomeOtherValue"}
        /// And returns the value. Exception if not found.
        /// </summary>
        public string GetValue(string key)
        {
            // Make sure config.json exists, is set to 'Content' and 'Copy Always'.
            var config = JObject.Parse(File.ReadAllText(_jsonFilePath));
            var value = (string)config[key];
            return value;
        }

    }
}
