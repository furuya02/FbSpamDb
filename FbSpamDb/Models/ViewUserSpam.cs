using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FbSpamDb.Models {
    public class ViewUserSpam : ViewUser {

        public bool IsPublic { get; private set; } //公開・非公開
        public bool IsMine { get; private set; } //自分で登録したデータかどうか
        public long OwnerId { get; private set; } //投稿者ID
        public DateTime CreateAt { get; private set; } //登録日時
        //public int Brightness { get; private set; } //画像情報（全体彩度）
        //public int Colorinfo { get; private set; } //画像情報（部分色度）

        //public ViewUserSpam(string name, long id, string pictureUrl,long ownerId, bool isPublic, bool isMine, DateTime createAt, int brightness, int colorinfo)
        public ViewUserSpam(string name, long id, string pictureUrl,long ownerId, bool isPublic, bool isMine, DateTime createAt)
            : base(name, id,pictureUrl) {
            OwnerId = ownerId;
            IsPublic = isPublic;
            IsMine = isMine;
            CreateAt = createAt;
            //Brightness = brightness;
            //Colorinfo = colorinfo;
        }
    }
}