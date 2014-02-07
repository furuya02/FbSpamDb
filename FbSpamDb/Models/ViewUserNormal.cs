using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FbSpamDb.Models {
    public class ViewUserNormal : ViewUser {


        public bool IsSpam { get; private set; }　//スパムにヒットするかどうか

        public ViewUserNormal(string name, long id, string pictureUrl, bool isSpam)
            : base(name, id,pictureUrl) {
            IsSpam = isSpam;
        }
    }
}