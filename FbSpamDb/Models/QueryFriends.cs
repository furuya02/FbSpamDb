using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Mvc.Facebook;

namespace FbSpamDb.Models {
    public class QueryFriends {
        public FacebookGroupConnection<QueryFriend> Friends { get; set; }
    }
}