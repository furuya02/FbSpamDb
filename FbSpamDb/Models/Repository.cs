using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace FbSpamDb.Models{
    public class Repository{
        private readonly FbSpamDbContext _db = new FbSpamDbContext();
        private readonly String _imagePath;
        private readonly long _userId; //アクセス者
        private readonly bool _isManager; //管理者かどうか

        public Repository(long userId, String imagePath){
            _userId = userId;
            _imagePath = imagePath;

            //管理者かどうかの確認
            if (_userId == XXXXX){
                _isManager = true;
            }
        }

        public bool IsManager{
            get{
                return _isManager;
            }
        }

        //スパム一覧の取得
        public List<DbSpam> List(Order order){

            var ar = new List<DbSpam>();
            foreach (var d in _db.DbSpams){
                //「自分の登録したデータ」及び「公開データ」が検索対象になる
                if (_isManager || d.OwnerId == _userId || d.IsPublic){
                    ar.Add(d);
                }
            }

            if (order != Order.None)
            {
                //ソートの指定がある場合

                ar.Sort((a, b) =>
                {
                    switch (order)
                    {
                        case Order.CreateAt:
                            //return b.CreateAt.CompareTo(a.CreateAt);
                            return b.CreateAt.Ticks.CompareTo(a.CreateAt.Ticks);
                        case Order.Id:
                            return b.Id.CompareTo(a.Id);
                        case Order.Name:
                            return b.Name.CompareTo(a.Name);
                        case Order.Gender:
                            var ret = a.Gender.CompareTo(b.Gender);
                            if (ret != 0){
                                return ret;
                            }
                            return a.ImgId.CompareTo(b.ImgId); //JPG
                        case Order.Jpg:
                            return a.ImgId.CompareTo(b.ImgId);
                    }
                    return 0;
                });
            }

            return ar;
        }

        //Idによる検索
        public DbSpam Find(long id){
            return _db.DbSpams.ToList().FirstOrDefault(a => a.Id == id);
        }

        //画像情報からDB内の同一画像を検索する
        long Fnid(int brightness, int colorInfo1, int colorInfo2, int colorInfo3) {
            foreach (var a in _db.DbSpams) {
                if (a.Brightness == brightness && a.ColorInfo1 == colorInfo1 && a.ColorInfo2 == colorInfo2 && a.ColorInfo3 == colorInfo3) {
                    return a.ImgId;
                }
            }
            return -1;
        }

        //追加
        public async Task<bool> Create(long ownerId, long id, String name, String pictureUrl, bool gender, DateTime createAt, bool isPublic){

            return await Task.Run(() =>{
                try{
                    var filename = GetJpgFileName(id);
                    //画像のダウンロード
                    Utils.Download(pictureUrl, filename);
                    var jpgValue = new JpgValue(filename);
                    
                    //既に、同じ画像が存在する場合は、そのIDとする
                    var imgId = Fnid(jpgValue.Brightness, jpgValue.ColorInfo1, jpgValue.ColorInfo2, jpgValue.ColorInfo3);
                    if (imgId == -1){
                        imgId = id; //画像ファイルは、デフォルトで対象ID名とする
                    } else{
                        if (imgId != id){
                            //重複ファイルは削除する
                            File.Delete(filename);
                        }
                    }
                    _db.DbSpams.Add(new DbSpam() { Id = id, Name = name, OwnerId = ownerId, IsPublic = isPublic, CreateAt = createAt, Gender = gender, Brightness = jpgValue.Brightness, ColorInfo1 = jpgValue.ColorInfo1, ColorInfo2 = jpgValue.ColorInfo2, ColorInfo3 = jpgValue.ColorInfo3,ImgId = imgId});
                    _db.SaveChanges();
                    return true;
                } catch (Exception ){
                    return false;
                }

            });
        }

        //削除
        public bool Delete(long id){
            foreach (var d in _db.DbSpams){
                if (d.Id == id){
                    _db.DbSpams.Remove(d);
                    _db.SaveChanges();

                    bool isUse = false;
                    foreach (var a in _db.DbSpams){
                        if (a.ImgId == d.ImgId){
                            //まだ使用されいている
                            isUse = true;
                            break;
                        }
                    
                    }
                    if (!isUse){
                        try {
                            System.IO.File.Delete(GetJpgFileName(d.ImgId));
                        } catch (Exception) {
                            ;
                        }
                    }
                    return true;
                }
            }



            return false;
        }

        //画像ファイル名
        public String GetJpgFileName(long id){
            return string.Format("{0}/{1}.jpg", _imagePath, id);
        }

        //データ整備用
        public async Task<int> Restore(long ownerId, long spamId, DateTime createAt, bool isPublic, String accessToken){

            if (_db.DbSpams.ToList().Any(a => a.Id == spamId)){
                return 0;
            }


            //unameからSpamUser型でデータ取得
            var queryUser = await GraphRequest.GetUser(spamId.ToString(), accessToken);
            if (queryUser.Id != null){

                //新規登録
                await Create(ownerId, spamId, queryUser.Name, queryUser.Picture.Data.Url, (queryUser.Gender == "男性"), createAt, isPublic);

                //全てのデータを公開に変更する
                foreach (var a in _db.DbSpams.ToList()){
                    a.IsPublic = true;
                    _db.Entry(a).State = EntityState.Modified;
                }
                _db.SaveChanges();
            }
            return 0;
        }

        //ログ追加
        public bool AppenLog(long userId, String addr,String host,String userAgent,int status){
            var dt = DateTime.Now;
            var utc = dt.AddHours(9);
            //var utc = TimeZoneInfo.ConvertTime(dt, TimeZoneInfo.Utc);
            //var utcTime = TimeZoneInfo.ConvertTimeToUtc(utc);//UTCに変換
            //var localTime = TimeZoneInfo.ConvertTimeFromUtc(utc, TimeZoneInfo.Local);//ローカル時刻に変換

            var dbLog = new DbLog(){CreateAt = utc, UserId = userId, Host=host,Addr=addr,UserAgent = userAgent, Status = status};
            _db.DbLogs.Add(dbLog);
            _db.SaveChanges();
            return true;
        }
        //ログ一覧の取得
        public async Task<List<DbLog>> LogListAsync() {

            return await _db.DbLogs.OrderByDescending(x => x.CreateAt).ToListAsync();
        }

        //既にログに存在するかどうかの検索
        public bool LogSearch(long id){
            foreach (var a in _db.DbLogs){
                if (a.UserId == id){
                    return true;
                }
            }
            return false;

        }

        //公開・非公開設定
        public void SetPublic(long id,bool isPublic){
            var o = Find(id);
            if (o != null){
                o.IsPublic = isPublic;
                _db.SaveChanges();
            }
        }

        //ログ削除（30件だけのこす）
        public void LogDelete(){
            var count = _db.DbLogs.Count();
            foreach (var a in _db.DbLogs) {
                _db.DbLogs.Remove(a);
                count--;
                if (count < 30) {
                    break;
                }
            }
            _db.SaveChanges();
        }
    }

}