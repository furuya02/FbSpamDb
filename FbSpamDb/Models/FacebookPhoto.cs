using Newtonsoft.Json;

namespace FbSpamDb.Models {
    public class FacebookPhoto {
        [JsonProperty("picture")] // これにより、画像に対するプロパティの名前が変更されます。
        public string ThumbnailUrl { get; set; }

        public string Link { get; set; }
    }
}
