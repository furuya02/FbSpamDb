using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FbSpamDb.Models {
    public class AuthDb{
        public long UserId { get; private set; }
        public String AccessToken { get; private set; }
        public AuthDb(String userId,String accessToken){
            UserId = Convert.ToInt64(userId);
            AccessToken = accessToken;
        }
    }
}