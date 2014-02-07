using Microsoft.AspNet.Mvc.Facebook;
using Newtonsoft.Json;

// 各ユーザーに関して保存する必要があるフィールドを追加し、Facebook から返信された JSON でフィールド名を指定します
// http://go.microsoft.com/fwlink/?LinkId=301877

namespace FbSpamDb.Models {
    public class MyAppUser {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        [JsonProperty("picture")] // これにより、画像に対するプロパティの名前が変更されます。
        [FacebookFieldModifier("type(large)")] // これにより、画像のサイズが大に設定されます。
        public FacebookConnection<FacebookPicture> ProfilePicture { get; set; }

        [FacebookFieldModifier("limit(8)")] // これにより、フレンド リストのサイズが 8 に設定されます。削除すると、すべてのフレンドが対象となります。
        public FacebookGroupConnection<MyAppUserFriend> Friends { get; set; }

        [FacebookFieldModifier("limit(16)")] // これにより、写真リストのサイズが 16 に設定されます。削除すると、すべての写真が対象となります。
        public FacebookGroupConnection<FacebookPhoto> Photos { get; set; }
    }
}
