using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FbSpamDb.Models {
    //Controllerで、スパム判定に、毎回一覧を取得するとコストが高いので、このクラスを使用する
    public class SpamDb2 {

        private readonly List<DbSpam> _ar;
        //private Repository _repository;
        public SpamDb2(Repository repository) {
            //repository = repository;
            _ar = repository.List(Order.None);
        }

        public bool IsSpam(long id) {
            return _ar.Any(a => a.Id == id);
        }

        //public bool IsSpam(long id,String url) {
        //    if (_ar.Any(a => a.Id == id)){
        //        return true;
        //    }

        //    var filename = _repository.GetJpgFileName(id);
        //    //画像のダウンロード
        //    Utils.Download(url, filename);
        //    var jpgValue = new JpgValue(filename);
        //    foreach (var a in _ar){
        //        if (a.Brightness == jpgValue.Brightness){
        //            if (a.ColorInfo == jpgValue.ColorInfo)
        //                return true;
        //        }
        //    }
        //    return false;
        //}

    }
}