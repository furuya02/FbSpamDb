using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Web.Mvc;
using Facebook;
using Microsoft.AspNet.Mvc.Facebook;
using FbSpamDb.Models;

namespace FbSpamDb.Controllers {
    public class HomeController : Controller {

        public ActionResult Friends(string userId, string accessToken) {

            const bool confirm = true; //有効期限検証
            var authDb = CreateAuthDb(userId, accessToken, confirm);
            if (authDb == null) {
                return RedirectToAction("OAuth", null, new { request = "Friends" });
            }

            //アクセスログ(1=Friends)
            AppnedLog(authDb, 1);

            return View();
        }

        public ActionResult Requests(string userId, string accessToken) {

            const bool confirm = true; //有効期限検証
            var authDb = CreateAuthDb(userId, accessToken, confirm);
            if (authDb == null) {
                return RedirectToAction("OAuth", null, new { request = "Requests" });
            }

            //アクセスログ(2=Requests)
            AppnedLog(authDb, 2);

            return View();
        }

        public ActionResult Spams(string userId, string accessToken) {
            var authDb = CreateAuthDb(userId, accessToken);
            if (authDb == null) {
                return RedirectToAction("OAuth", null, new { request = "Friends" });
            }

            //アクセスログ(3=Spams)
            AppnedLog(authDb, 3);

            return View();
        }

        public ActionResult About(string userId, string accessToken) {
            var authDb = CreateAuthDb(userId, accessToken);
            if (authDb == null) {
                return RedirectToAction("OAuth", null, new { request = "Friends" });
            }

            return View();
        }


        [FacebookAuthorize("user_friends","read_requests")]
        public ActionResult OAuth(FacebookContext context, String redirect){
            //有効期限２か月の長期トークンを取得
            var extendedToken = "";
            try {
                var client = new FacebookClient();
                dynamic result = client.Get("/oauth/access_token", new {
                    grant_type = "fb_exchange_token",
                    client_id = context.Client.AppId,
                    client_secret = context.Client.AppSecret,
                    fb_exchange_token = context.Client.AccessToken
                });
                extendedToken = result.access_token;
            } catch {
                extendedToken = context.Client.AccessToken;
            }

            var authDb = new AuthDb(context.UserId, extendedToken);
            Session["AuthDb"] = authDb;

            //Safari対応
            if (Utils.IsSafari(Request.UserAgent)){
                ViewBag.UserId = context.UserId;
                ViewBag.AccessToken = extendedToken;
            }
            
            //アクセスログ(0=Auth)
            AppnedLog(authDb, 0);

            if (redirect == null){
                redirect = "Friends";
            }
            return RedirectToAction(redirect, new { UserId = context.UserId, AccessToken = extendedToken });
        }

        private AuthDb CreateAuthDb(string userId, string accessToken,bool confirm = false){

            AuthDb authDb = null;

            //safari様式
            if (Utils.IsSafari(Request.UserAgent)) {
                Session["AuthDb"] = null; //Safari様式では、パラメータ渡しとなり、誤動作防止のため確実にこのセッションをnullに設定する
                if (accessToken != null && userId != null) {
                    //次のリクエストのために
                    ViewBag.UserId = userId;
                    ViewBag.AccessToken = accessToken;
                    authDb = new AuthDb(userId, accessToken);
                }
            }
            if (authDb == null){
                authDb = (AuthDb)Session["AuthDb"];
            }
            if (!confirm){
                return authDb;
            }
            //コストが高くなるので、Ajaxリクエスト時は以下の検証を行わない
            //万が一認証が切れている場合は、OAuthへ戻す
            try {
                //有効期限を取得するメソッド
                var client = new FacebookClient();
                dynamic result = client.Get("debug_token", new {
                    input_token = authDb.AccessToken,
                    access_token = authDb.AccessToken
                });

                var expires = result.data.expires_at;
            } catch (Exception) {
                return null;
            }

            var db = new Repository(authDb.UserId, Server.MapPath("~/SpamImage"));
            if (!db.LogSearch(authDb.UserId)){
                //新規ユーザの場合、メールを送る
                Mail.SendMe("FbSpamDb", "新規利用者", string.Format("http://www.facebook.com/{0}",authDb.UserId));
            }

            return authDb;
        }

        //アクセスログ
        private void AppnedLog(AuthDb authDb,int status){
            var db = new Repository(authDb.UserId, Server.MapPath("~/SpamImage"));
            db.AppenLog(authDb.UserId, Request.UserHostAddress, Request.UserHostName, Request.UserAgent, status);
        }

        //interval件の友達一覧を取得する
        [HttpPost]
        public async Task<ActionResult> GetFriend(string userId,string accessToken,int max, int start, int interval,int spam) {
            
            if (Request.IsAjaxRequest()) {
                if (max != -1 && start >= max) {
                    return new HttpStatusCodeResult(500, "complete");
                }

                var authDb = CreateAuthDb(userId, accessToken);
                if (authDb == null){
                    return Content("許可されないアクセスです");
                }
                var db = new Repository(authDb.UserId, Server.MapPath("~/SpamImage"));

                const string title = "すべての友達";

                //すべての友達の取得
                var dat = await GraphRequest.GetFriends(authDb.AccessToken);
                max = dat.Count;

                //ViewUserの作成
                var isComplete = false;
                var ar = CreateViewUserNormal(dat, start, interval, db.List(Order.None), ref spam, ref isComplete);

                ViewBag.Max = max;
                ViewBag.Spam = spam;
                ViewBag.Alert = CreateAlertString(title, isComplete, max, start + ar.Count, spam);

                return PartialView("GetFriend", ar);
                
            }
            return Content("0$0$許可されないアクセスです");
        }

        //interval件の友達一覧を取得する
        [HttpPost]
        public async Task<ActionResult> GetRequest(string userId,string accessToken,int max,int start, int interval, int spam) {

            if (Request.IsAjaxRequest()) {
                if (max!=-1 && start >= max) {
                    return new HttpStatusCodeResult(500, "complete");
                }
                var authDb = CreateAuthDb(userId, accessToken);
                if (authDb == null) {
                    return Content("許可されないアクセスです");
                }
                var db = new Repository(authDb.UserId, Server.MapPath("~/SpamImage"));

                const string title = "友達リクエスト";

                //友達リクエストの取得
                var dat = await GraphRequest.GetRequests(authDb.AccessToken);
                if (max == -1){
                    max = dat.Count;
                }

                //ViewUserの作成
                var isComplete = false;
                var ar = CreateViewUserNormal(dat, start, interval, db.List(Order.None), ref spam, ref isComplete);

                ViewBag.Max = max;
                ViewBag.Spam = spam;
                ViewBag.Alert = CreateAlertString(title,isComplete, max, start + ar.Count, spam);

                return PartialView("GetFriend",ar);
            }
            return Content("許可されないアクセスです");
        }

        //interval件のスパム一覧を取得する
        [HttpPost]
        public ActionResult GetSpam(string userId,string accessToken,int max, int start, int interval,Order order) {

            if (Request.IsAjaxRequest()) {
                if (max != -1 && start >= max) {
                    return new HttpStatusCodeResult(500, "complete");
                }
                var authDb = CreateAuthDb(userId, accessToken);
                if (authDb == null) {
                    return Content("許可されないアクセスです");
                }
                var db = new Repository(authDb.UserId, Server.MapPath("~/SpamImage"));

                const string title = "スパムDB";

                //スパムの取得
                var dat = db.List(order);
                if (max == -1) {
                    max = dat.Count;
                }

                //ViewUserの作成
                var isComplete = false;
                var ar = CreateViewUserSpam(dat, start, interval, ref isComplete,authDb.UserId);

                ViewBag.Max = max;
                ViewBag.Spam = 0;
                ViewBag.Alert = CreateAlertString(title, isComplete, max, start + ar.Count, -1);

                ViewBag.IsManager = db.IsManager;

                return PartialView("GetSpam", ar);
            }
            return Content("許可されないアクセスです");
        }

        //ViewUserSpam生成
        List<ViewUserSpam> CreateViewUserSpam(IList<DbSpam> dat, int start, int interval, ref bool isComplete,long userId) {
            var ar = new List<ViewUserSpam>();
            for (var i = start; i < start + interval; i++) {
                if (i >= dat.Count) {
                    isComplete = true;
                    break;
                }
                var pictureUrl = string.Format("../SpamImage/{0}.jpg",dat[i].ImgId);

               ar.Add(new ViewUserSpam(dat[i].Name, dat[i].Id, pictureUrl, dat[i].OwnerId, dat[i].IsPublic, (dat[i].OwnerId == userId), dat[i].CreateAt));
            }
            return ar;
        }
        //ViewUserNormal生成
        List<ViewUserNormal> CreateViewUserNormal(List<QueryFriend> dat, int start, int interval, List<DbSpam> spamList, ref int spam, ref bool isComplete) {
            var ar = new List<ViewUserNormal>();
            for (var i = start; i < start + interval; i++) {
                if (i >= dat.Count) {
                    isComplete = true;
                    break;
                }
                var id = Convert.ToInt64(dat[i].Id);
                var isSpam = false;
                if (spamList.Any(a => a.Id == id)) {
                    spam++;
                    isSpam = true;

                }
                ar.Add(new ViewUserNormal(dat[i].Name, id, dat[i].Picture.Data.Url, isSpam));
            }
            return ar;
        }

        //アラート生成
        String CreateAlertString(string title, bool isComplete, int max, int pos, int spam) {
            if (spam == -1){
                return string.Format("現在、{0}件のスパムアカウントがデータベースに登録されています。",max);
            }

            if (isComplete) {
                if (spam == 0) {
                    return string.Format("{0} {1} スパムは見つかりませんでした", title, max);
                }
                return string.Format("{0} {1} スパムが{2}件見つかりました", title, max, spam);
            }
            if (spam == 0) {
                return string.Format("処理中.... {0}/{1}", pos, max);
            }
            return string.Format("処理中.... {0}/{1} スパムが{2}件見つかりました", pos, max, spam);
        }

        //新規登録
        [HttpPost]
        public async Task<ActionResult> Create(string userId,string accessToken,string url, Order order) {
            Response.StatusCode = 400;
            if (Request.IsAjaxRequest()) {
                if (url == "") {
                    return Content("URLが指定されていません"); 
                }

                var uname = Utils.GetKeyOrUName(url);
                if (uname == null) {
                    return Content("URLが無効です");
                }

                var authDb = CreateAuthDb(userId, accessToken); 
                if (authDb == null) {
                    return Content("許可されないアクセスです");
                }
                var db = new Repository(authDb.UserId, Server.MapPath("~/SpamImage"));

                var queryUser = await GraphRequest.GetUser(uname, authDb.AccessToken);
                if (queryUser.Id == null) {
                    return Content("ID検索に失敗しました");
                }
                var uid = Convert.ToInt64(queryUser.Id);
                if (null != db.Find(uid)) {
                    return Content("既に登録されています");
                }
                if (!await db.Create(authDb.UserId, uid, queryUser.Name, queryUser.Picture.Data.Url, (queryUser.Gender == "男性"), DateTime.Now, false)) {
                    return Content("DBへの登録に失敗しました");
                }
                Response.StatusCode = 200;

                //ViewUserの作成
                var isComplete = false;
                var ar = CreateViewUserSpam(db.List(order),0,30,ref isComplete, authDb.UserId);

                //メールを送る
                Mail.SendMe("FbSpamDb", "新規スパム登録", string.Format("新規に登録されました。\r\nhttp://www.facebook.com/{0}\r\n\r\n報告者\r\nhttp://www.facebook.com/{1}", uid, authDb.UserId));
                return Content("登録しました");
            }
            return Content("許可されないアクセスです");
        }
        
        //通報
        public ActionResult Report(string userId, string accessToken, long id) {
            Response.StatusCode = 400;
            if (Request.IsAjaxRequest()) {

                var authDb = CreateAuthDb(userId, accessToken);
                if (authDb == null) {
                    return Content("許可されないアクセスです");
                }
                var db = new Repository(authDb.UserId, Server.MapPath("~/SpamImage"));
                //メールを送る
                if (Mail.SendMe("FbSpamDb", "非スパム通報", string.Format("以下のアカウントはスパムではありません。\r\nhttp://www.facebook.com/{0}", id))){
                    Response.StatusCode = 200;
                    return Content("通報しました");
                }
                return Content("メール送信に失敗しました");
            }
            return Content("許可されないアクセスです");
        }

        //データベースから削除
        [HttpPost]
        public ActionResult Delete(string userId,string accessToken,long id, Order order) {
            Response.StatusCode = 400;
            if (Request.IsAjaxRequest()){

                var authDb = CreateAuthDb(userId, accessToken);
                if (authDb == null) {
                    return Content("許可されないアクセスです");
                }
                var db = new Repository(authDb.UserId, Server.MapPath("~/SpamImage"));
                db.Delete(id);

                //メールを送る
                Mail.SendMe("FbSpamDb", "スパム削除", string.Format("スパムが削除されました。\r\nhttp://www.facebook.com/{0}\r\n\r\n報告者\r\nhttp://www.facebook.com/{1}", id, authDb.UserId));
                
                Response.StatusCode = 200;
                return Content("登録しました");

            }
            return Content("許可されないアクセスです");
        }

        //メンテナンス(公開・非公開設定)
        public ActionResult SetPublic(string userId, string accessToken, long id,int isPublic) {
            var authDb = CreateAuthDb(userId, accessToken);
            if (authDb == null) {
                return Content("許可されないアクセスです");
            }

            var db = new Repository(authDb.UserId, Server.MapPath("~/SpamImage"));
            if (db.IsManager){
                db.SetPublic(id, (isPublic == 1));
            }
            return RedirectToAction("Spams");
        }

        //メンテナンス(管理者用メニュー)
        public ActionResult Admin() {
            var authDb = (AuthDb)Session["AuthDb"];
            if (authDb == null) {
                return Content("許可されないアクセスです");
            }
            var db = new Repository(authDb.UserId, Server.MapPath("~/SpamImage"));

            if (!db.IsManager) {
                return RedirectToAction("Error", null, new { msg = "管理者以外は、このメニューを利用することができません。" });
            }
            return View();
        }


        //メンテナンス(アクセス履歴)
        [HttpPost]
        public async Task<ActionResult> Log() {

            if (Request.IsAjaxRequest()) {

                var authDb = (AuthDb)Session["AuthDb"];
                if (authDb == null) {
                    return Content("許可されないアクセスです");
                }
                var db = new Repository(authDb.UserId, Server.MapPath("~/SpamImage"));
                if (db.IsManager) {
                    return PartialView(await db.LogListAsync());
                }
            }
            return Content("許可されないアクセスです");
        }

        //メンテナンス(ログ削除」)
        [HttpPost]
        public async Task<ActionResult> LogDelete() {

            if (Request.IsAjaxRequest()) {

                var authDb = (AuthDb)Session["AuthDb"];
                if (authDb == null) {
                    return Content("許可されないアクセスです");
                }
                var db = new Repository(authDb.UserId, Server.MapPath("~/SpamImage"));
                if (db.IsManager){

                    db.LogDelete();
                    return PartialView("Log",await db.LogListAsync());
                }
            }
            return Content("許可されないアクセスです");
        }

        //メンテナンス(バックアップ)
        [HttpPost]
        public ActionResult Backup() {

            if (Request.IsAjaxRequest()) {

                var authDb = (AuthDb)Session["AuthDb"];
                if (authDb == null) {
                    return Content("許可されないアクセスです");
                }
                var db = new Repository(authDb.UserId, Server.MapPath("~/SpamImage"));
                if (db.IsManager){
                    var ar = new List<DbSpam>();
                    foreach (var a in db.List(Order.None)) {
                        //何か変換の必要がある場合は、ここで処理する
                        //if (a.OwnerId == 0){
                        //    a.OwnerId = 100002002711624;
                        //}
                        ar.Add(a);
                    }
                    return PartialView(ar);
                }
            }
            return Content("許可されないアクセスです");
        }



        //メンテナンス(リストア)
        [HttpPost]
        public async Task<ActionResult> Restore(String backup){

            var authDb = (AuthDb) Session["AuthDb"];
            if (authDb == null){
                return Content("許可されないアクセスです");
            }
            var db = new Repository(authDb.UserId, Server.MapPath("~/SpamImage"));
            if (db.IsManager){
                var lines = backup.Split('\n');
                foreach (var line in lines){
                    var tmp = line.Split(',');
                    if (tmp.Length == 4){
                        var spamId = Int64.Parse(tmp[0]);
                        var ownerId = Int64.Parse(tmp[1]);
                        var createAt = DateTime.Parse(tmp[2]);
                        var isPublic = bool.Parse(tmp[3]);
                        await db.Restore(ownerId, spamId, createAt, isPublic, authDb.AccessToken);
                    }
                }
                return RedirectToAction("Admin");

            }
            return Content("許可されないアクセスです");
        }

        //メンテナンス(スパム追加)
        public async Task<ActionResult> AddSpam(long id, long ownerId, string accessToken){

            var db = new Repository(ownerId, Server.MapPath("~/SpamImage"));

            var queryUser = await GraphRequest.GetUser(id.ToString(), accessToken);
            if (queryUser.Id == null){
                return Content("ERROR ID検索に失敗しました");
            }
            var uid = Convert.ToInt64(queryUser.Id);
            if (null != db.Find(uid)){
                return Content("ERROR 既に登録されています");
            }
            if (!await db.Create(ownerId, uid, queryUser.Name, queryUser.Picture.Data.Url, (queryUser.Gender == "男性"), DateTime.Now, false)){
                return Content("DBへの登録に失敗しました");
            }

            return Content("SUCCESS");
        }
    }
}
