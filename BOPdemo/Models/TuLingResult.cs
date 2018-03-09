using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BOPdemo.Models
{
    public class TuLingResult
    {
        /// <summary>
        /// The top answer found in the QnA Service.
        /// </summary>
        [JsonProperty(PropertyName = "code")]
        public int Code { get; set; }

        /// <summary>
        /// The score in range [0, 100] corresponding to the top answer found in the QnA    Service.
        /// </summary>
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }
    }
}