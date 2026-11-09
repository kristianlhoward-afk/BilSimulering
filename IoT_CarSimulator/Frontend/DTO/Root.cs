using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;   

namespace Frontend.DTO
{
    public class Root
    {
        [JsonPropertyName("channel")]
        public Channel? Channel { get; set; }
        [JsonPropertyName("feeds")]
        public List<ReadCarDTO>? Feeds { get; set; }
    }

}
