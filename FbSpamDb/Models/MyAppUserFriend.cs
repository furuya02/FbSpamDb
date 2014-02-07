using Microsoft.AspNet.Mvc.Facebook;

// 各ユーザーに関して保存する必要があるフィールドを追加し、Facebook から返信された JSON でフィールド名を指定します
// http://go.microsoft.com/fwlink/?LinkId=301877

namespace FbSpamDb.Models {
    public class MyAppUserFriend {
        public string Name { get; set; }
        public string Link { get; set; }

        [FacebookFieldModifier("height(100).width(100)")] // これにより、画像の高さと幅が 100px に設定されます。
        public FacebookConnection<FacebookPicture> Picture { get; set; }
    }
}
