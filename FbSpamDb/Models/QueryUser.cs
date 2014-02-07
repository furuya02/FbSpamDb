using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Mvc.Facebook;
using Newtonsoft.Json;

namespace FbSpamDb.Models {
    public class QueryUser {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        [FacebookFieldModifier("height(50).width(50)")]
        public FacebookConnection<QueryPicture> Picture { get; set; }
    }
}