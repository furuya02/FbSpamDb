using System;
using System.Web.Mvc;
using Microsoft.AspNet.Mvc.Facebook;
using Microsoft.AspNet.Mvc.Facebook.Authorization;

namespace FbSpamDb {
    public static class FacebookConfig {
        public static void Register(FacebookConfiguration configuration) {
            // 次のアプリケーション設定キーを使用して web.config から設定を読み込みます:
            // Facebook:AppId, Facebook:AppSecret, Facebook:AppNamespace
            configuration.LoadFromAppSettings();

            // Facebook の署名済み要求とアクセス許可を確認するための認証フィルターの追加
            GlobalFilters.Filters.Add(new FacebookAuthorizeFilter(configuration));
        }
    }
}
