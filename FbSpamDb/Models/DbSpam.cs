using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FbSpamDb.Models{
    public class DbSpam{
        [Key]
        public int DbSpamId { get; set; }

        [Required]
        public long Id { get; set; }

        [Required]
        public String Name { get; set; }

        [Required]
        public long OwnerId { get; set; } //登録者のId

        [Required]
        public bool Gender { get; set; } //性別 男性=true

        [Required]
        public bool IsPublic { get; set; } //公開されているかどうかのフラグ

        public DateTime CreateAt { get; set; } //登録日時

        //画像の高速比較のために、作成時に画像情報を2つの数値で記録する
        public int Brightness { get; set; } //彩度
        public int ColorInfo1 { get; set; } //色情報
        public int ColorInfo2 { get; set; } //色情報
        public int ColorInfo3 { get; set; } //色情報

        [Required]
        public long ImgId { get; set; } //画像ファイルのID番号


    }
}