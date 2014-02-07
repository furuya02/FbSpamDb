using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Facebook;
using Microsoft.AspNet.Mvc.Facebook.Client;
using Microsoft.AspNet.Mvc.Facebook;

namespace FbSpamDb.Models {
    public class GraphRequest {

        //FacebookClientは、プロパイダ経由で作成しないとSerializer等が初期化されない
        static FacebookClient CreateFacebookClient(String accessToken) {
            var client = GlobalFacebookConfiguration.Configuration.ClientProvider.CreateClient();
            client.AccessToken = accessToken;
            return client;
        }

        //GET発行
        async static Task<T> GetAsync<T>(String uname, String accessToken) {
            var client = CreateFacebookClient(accessToken);
            var path = uname + FacebookQueryHelper.GetFields(typeof(T)) + "&locale=ja_JP";
            return await client.GetTaskAsync<T>(path);
        }
        //FQL発行
        async static Task<dynamic> FqlAsync(string query, string accessToken) {
            var client = CreateFacebookClient(accessToken);
            dynamic result = await client.GetTaskAsync("fql", new { q = query });
            return result.data;
        }

        //すべての友達
        static public async Task<List<QueryFriend>> GetFriends(string accessToken){
            var data = await GetAsync<QueryFriends>("me", accessToken);
            return data.Friends.Data.ToList();
        }
        ////友達リクエスト
        //static public async Task<List<QueryUser>> GetRequests(string accessToken) {
        //    var data = await FqlAsync("select uid_from from friend_request where uid_to=me()", accessToken);

        //    var ar = new List<QueryUser>();
        //    foreach (var d in data) {
        //        var queryUser = await GetAsync<QueryUser>((string)d.uid_from, accessToken);
        //        ar.Add(queryUser);
        //    }
        //    return ar;
        //}

        //友達リクエスト
        static public async Task<List<QueryFriend>> GetRequests(string accessToken) {
            var data = await FqlAsync("select uid_from from friend_request where uid_to=me()", accessToken);

            var ar = new List<QueryFriend>();
            foreach (var d in data) {
                var u = await GetAsync<QueryUser>((string)d.uid_from, accessToken);
                ar.Add(new QueryFriend(){Id = u.Id,Name = u.Name,Picture = u.Picture});
            }
            return ar;
        }
        //ユーザ情報
        public static async Task<QueryUser> GetUser(string uname, string accessToken){
            return await GetAsync<QueryUser>(uname, accessToken);
        }

    }
}