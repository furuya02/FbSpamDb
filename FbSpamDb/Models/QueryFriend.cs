using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Mvc.Facebook;

namespace FbSpamDb.Models {
    public class QueryFriend {
        public string Name { get; set; }
        public string Id { get; set; }
        [FacebookFieldModifier("height(50).width(50)")] // 画像の高さと幅を 50px に設定
        public FacebookConnection<QueryPicture> Picture { get; set; }
    }
}