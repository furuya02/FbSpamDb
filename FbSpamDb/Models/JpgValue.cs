using System.Drawing;
using System.IO;

namespace FbSpamDb.Models {

    //JPG画像を数値化するクラス
    public class JpgValue{
        public int Brightness { get; private set; } //鮮度
        public int ColorInfo1 { get; private set; } //色情報1
        public int ColorInfo2 { get; private set; } //色情報2
        public int ColorInfo3 { get; private set; } //色情報3


        //未解釈データ用のコンストラクタ
        public JpgValue(string fileName){

            // この方がmew Bitmap(fileName)より速い
            var s = File.OpenRead(fileName);
            var img = Image.FromStream(s, false, false);
            var bitmap = new Bitmap(img);
            s.Close();
            
            //全体の鮮度及び４か所の色情報で画像を特定する
            //全体の鮮度
            Brightness = GetBrightness(bitmap);
            //特定位置の色成分
            ColorInfo1 = GetColorInfo(bitmap, 15, 15);
            ColorInfo2 = GetColorInfo(bitmap, 25, 25);
            ColorInfo3 = GetColorInfo(bitmap, 45, 45);
        }

        //解釈済みデータ用のコンストラクタ
        public JpgValue(int brightness, int colorInfo1, int colorInfo2, int colorInfo3){
            Brightness = brightness;
            ColorInfo1 = colorInfo1;
            ColorInfo2 = colorInfo2;
            ColorInfo3 = colorInfo3;
        }

        //画像一覧のソート用
        //可能かなぎり似ている画像を同一として処理している
        //【画像の一致検証に使用してはならない】
        //public int CompareTo(JpgValue p){
        //    var b1 = Brightness/5; //近似値ソート用
        //    var b2 = p.Brightness/5;//近似値ソート用
        //    var c1 = Brightness2;
        //    var c2 = p.Brightness2;

        //    if (b1 == b2){
        //        if (c1 == c2){
        //            return 0;
        //        }
        //        return c1.CompareTo(c2);
        //    }
        //    return (b1+c1).CompareTo(b2 + c2);
        //}

        //画像一覧のソート用
        //完全一致に仕様変更
        public int CompareTo(JpgValue p){
            var b1 = Brightness;
            var b2 = p.Brightness;
            var c1 = ColorInfo1;
            var c2 = p.ColorInfo1;
            var d1 = ColorInfo2;
            var d2 = p.ColorInfo2;
            var e1 = ColorInfo3;
            var e2 = p.ColorInfo3;

            if (b1 == b2){
                if (c1 == c2){
                    if (d1 == d2){
                        if (e1 == e2){
                            return 0;
                        }
                        return e1.CompareTo(e2);
                    }
                    return d1.CompareTo(d2);
                }
                return c1.CompareTo(c2);
            }
            return (b1 + c1 + d1 + e1).CompareTo(b2 + c2 + d1 + e1);
        }



        //特定位置の色成分
        private static int GetColorInfo(Bitmap bitmap, int x, int y){
            int count = 0;
            count += bitmap.GetPixel(x, y).R;
            count += bitmap.GetPixel(x + 1, y).R;
            count += bitmap.GetPixel(x - 1, y).R;
            count += bitmap.GetPixel(x, y + 1).R;
            count += bitmap.GetPixel(x, y - 1).R;

            count += bitmap.GetPixel(x, y).B;
            count += bitmap.GetPixel(x + 1, y).B;
            count += bitmap.GetPixel(x - 1, y).B;
            count += bitmap.GetPixel(x, y + 1).B;
            count += bitmap.GetPixel(x, y - 1).B;

            count += bitmap.GetPixel(x, y).G;
            count += bitmap.GetPixel(x + 1, y).G;
            count += bitmap.GetPixel(x - 1, y).G;
            count += bitmap.GetPixel(x, y + 1).G;
            count += bitmap.GetPixel(x, y - 1).G;

            return count;
        }

        //全体の鮮度
        private static int GetBrightness(Bitmap bitmap){

            var count = 0;
            for (var y = 0; y < bitmap.Height; y++){
                for (var x = 0; x < bitmap.Width; x++){
                    var brightness = bitmap.GetPixel(x, y).GetBrightness();
                    if (brightness > 0.5){
                        count++;
                    }
                }
            }
            return count;
        }

    }
}