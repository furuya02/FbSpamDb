using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Facebook;
using Microsoft.AspNet.Mvc.Facebook;
using Microsoft.AspNet.Mvc.Facebook.Client;

namespace FbSpamDb.Models {
    public class Utils {

        //画像のダウンロード
        public static bool Download(String url, String fileName) {
            try {
                var wc = new System.Net.WebClient();
                wc.DownloadFile(url, fileName);
                wc.Dispose();
                return false;
            } catch (Exception) {
                ;
            }
            return false;
        }

        //Safariの判定
        public static bool IsSafari(String userAgent) {
            if (userAgent.IndexOf("Safari") >= 0){
                if (userAgent.IndexOf("Chrome") < 0){
                    return true;
                }
            }
            return false;
            //chrome  : Mozilla/5.0 (Windows NT 6.1) AppleWebKit/534.24 (KHTML, like Gecko) Chrome/11.0.696.65 Safari/534.24
        }  

        //ドロップされたURLからユーザ名を取得する
        public static String GetKeyOrUName(String str) {
            if (str == null) {
                return null;
            }
            if (str.IndexOf("https://www.facebook.com/") != -1) {
                var tmp = str.Substring(25);

                var i = tmp.IndexOf("profile.php?id");
                if (i != -1) {

                    tmp = tmp.Substring(i + 15);
                    i = tmp.IndexOf("&");
                    if (i != -1) {
                        tmp = tmp.Substring(0, i);
                    }
                    return tmp;
                }

                i = tmp.IndexOf("photo.php?fbid=");
                if (i != -1) {
                    tmp = tmp.Substring(i + 15);

                    i = tmp.IndexOf("&set=");
                    if (i != -1) {
                        tmp = tmp.Substring(i + 5);
                        i = tmp.IndexOf("&");
                        if (i != -1) {
                            tmp = tmp.Substring(0, i);
                            var d = tmp.Split('.');
                            if (d.Count() > 0) {
                                return d[d.Count() - 1];
                            }
                        }
                    }

                    return "";
                }


                i = tmp.IndexOf("/");
                if (i != -1) {
                    tmp = tmp.Substring(0, i);
                    return tmp;
                }
                i = tmp.IndexOf("&");
                if (i != -1) {
                    tmp = tmp.Substring(0, i);
                }
                i = tmp.IndexOf("?");
                if (i != -1) {
                    tmp = tmp.Substring(0, i);
                }
                return tmp;

            }
            return null;
        }

    }
}