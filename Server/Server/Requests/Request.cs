﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Server.Requests
{
    public class Request
    {
        [JsonProperty("data")]
        public string Data { get; set; } = string.Empty;
    }
}
