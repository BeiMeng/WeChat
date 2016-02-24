using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace BeiDream.WeChat.Controllers
{
    public class HomeController : Controller
    {
        public string Token = "BeiDream";
        //public ActionResult Index()
        //{
        //    return View();
        //}

        /// <summary>
        /// 微信后台验证地址（使用Get），微信后台的“接口配置信息”的Url
        /// </summary>
        /// <param name="signature"></param>
        /// <param name="timestamp"></param>
        /// <param name="nonce"></param>
        /// <param name="echostr"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index(string signature, string timestamp, string nonce, string echostr)
        {
            if (string.IsNullOrEmpty(Token)) return Content("请先设置Token！");

            var ent = "";
            if (!CheckSignature(signature, timestamp, nonce, Token, out ent))
            {
                return Content("参数错误！");
            }
            return Content(echostr); //返回随机字符串则表示验证通过

        }


        private bool CheckSignature(string signature, string timestamp, string nonce, string token, out string ent)
        {
            var arr = new[] { token, timestamp, nonce }.OrderBy(z => z).ToArray();
            var arrString = string.Join("", arr);
            var sha1 = System.Security.Cryptography.SHA1.Create();
            var sha1Arr = sha1.ComputeHash(Encoding.UTF8.GetBytes(arrString));
            StringBuilder enText = new StringBuilder();
            foreach (var b in sha1Arr)
            {
                enText.AppendFormat("{0:x2}", b);
            }
            ent = enText.ToString();
            return signature == enText.ToString();
        }



        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}