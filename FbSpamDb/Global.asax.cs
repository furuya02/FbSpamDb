using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Microsoft.AspNet.Mvc.Facebook;
using FbSpamDb.Models;

namespace FbSpamDb {
    public class MvcApplication : System.Web.HttpApplication {
        protected void Application_Start() {
            //DBファイルの存在するディレクトリが存在しない時は作成する
            var path = Server.MapPath("~/App_Data");
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
            path = Server.MapPath("~/SpamImage");
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<FbSpamDbContext>());

            AreaRegistration.RegisterAllAreas();
            FacebookConfig.Register(GlobalFacebookConfiguration.Configuration);
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}