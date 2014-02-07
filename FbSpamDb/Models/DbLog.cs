using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls.Expressions;

namespace FbSpamDb.Models {
    public class DbLog {
        [Key]
        public int Id { get; set; }
        public DateTime CreateAt { get; set; }
        public String UserAgent { get; set; }
        public String Host { get; set; }
        public String Addr { get; set; }
        public long UserId { get; set; }
        public int Status { get; set; } //0.ログイン
    }
}