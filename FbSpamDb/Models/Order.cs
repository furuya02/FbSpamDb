using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FbSpamDb.Models {
    public enum Order {
        None = 0, //ソートなし
        CreateAt = 1,  //登録順
        Name = 2, //名前順
        Id = 3,
        Gender = 4, //性別
        Jpg = 5 //画像
    }
}