using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FbSpamDb.Models {
    public abstract class ViewUser {

        public string Name { get; private set; }
        public long Id { get; private set; }
        public string PictureUrl { get; private set; }　//画像へのリンク

        protected ViewUser(String name, long id, string pictureUrl)
        {
            Name = name;
            Id = id;
            PictureUrl = pictureUrl;
        }
    }
}