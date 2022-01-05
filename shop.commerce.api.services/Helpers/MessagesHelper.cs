using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace shop.commerce.api.services.Helpers
{
    public class MessagesHelper
    {
        private Dictionary<string, string> _messages;

        public MessagesHelper(string fileJson = "messages.json")
        {
            _messages = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(fileJson));
        }

        public string GetMessageCode(string code)
        {
            return _messages?.ContainsKey(code) == true ? _messages[code] : "";
        }
    }
}
