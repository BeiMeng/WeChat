using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using Senparc.Weixin.MP.Entities.Request;
using Senparc.Weixin.MP.MvcExtension;
using weixin_api;

namespace BeiDream.WeChat.Controllers
{
    public class InPutMsgObject
    {
        /// <summary>
        /// 本公众账号
        /// </summary>
        public string ToUserName { get; set; }
        /// <summary>
        /// 用户账号
        /// </summary>
        public string FromUserName { get; set; }
        /// <summary>
        /// 发送时间戳
        /// </summary>
        public string CreateTime { get; set; }
        /// <summary>
        /// 发送的文本内容
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 消息的类型
        /// </summary>
        public string MsgType { get; set; }
        /// <summary>
        /// 事件名称
        /// </summary>
        public string EventName { get; set; } 
    }

    public class HomeController : Controller
    {
        public string Token = "BeiDream";
        public string AppId = "wx6ab6f2dab9e0bedf";
        public string EncodingAESKey = "";
        //public ActionResult Index()
        //{
        //    return View();
        //}

        /// <summary>
        /// 微信后台验证地址（使用Get），微信后台的“接口配置信息”的Url
        /// </summary>
        /// <param name="signature">微信加密签名，signature结合了开发者填写的token参数和请求中的timestamp参数、nonce参数</param>
        /// <param name="timestamp">时间戳</param>
        /// <param name="nonce">随机数</param>
        /// <param name="echostr">随机字符串</param>
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
        [HttpPost]
        [ActionName("Index")]
        public ActionResult Post(string signature, string timestamp, string nonce, string echostr)
        {
            XmlDocument inPutXml = GetMsgXml();
            string outPutMsg = string.Empty;
            XmlElement root = inPutXml.DocumentElement;
            InPutMsgObject inPutMsgObject = GetInPutMsgObject(root);
            string messageType = inPutMsgObject.MsgType;//获取收到的消息类型。文本(text)，图片(image)，语音等。

            switch (messageType)
            {
                //当消息为文本时
                case "text":
                    outPutMsg = TextCase(inPutMsgObject);
                    break;
                case "event":
                    if (!string.IsNullOrEmpty(inPutMsgObject.EventName) && inPutMsgObject.EventName.Trim() == "subscribe")
                    {
                        //刚关注时的时间，用于欢迎词  
                        int nowtime = ConvertDateTimeInt(DateTime.Now);
                        string msg = "你要关注我，我有什么办法。随便发点什么试试吧~~~";
                        string resxml = "<xml>" +
                                        "<ToUserName><![CDATA[" + inPutMsgObject.FromUserName + "]]></ToUserName>" +
                                        "<FromUserName><![CDATA[" + inPutMsgObject.ToUserName + "]]></FromUserName>" +
                                        "<CreateTime>" + nowtime + "</CreateTime>" +
                                        "<MsgType><![CDATA[text]]></MsgType>" +
                                        "<Content><![CDATA[" + msg + "]]></Content>" +
                                        "</xml>";
                        outPutMsg = resxml;
                    }
                    break;
                case "image":
                    break;
                case "voice":
                    break;
                case "vedio":
                    break;
                case "location":
                    break;
                case "link":
                    break;
                default:
                    break;
            }

            return Content(outPutMsg);

        }

        #region 操作文本消息 + void TextCase(XmlElement root)
        private string getText(InPutMsgObject xmlMsg)
        {
            string con = xmlMsg.Content.Trim();

            System.Text.StringBuilder retsb = new StringBuilder(200);
            retsb.Append("这里放你的业务逻辑");
            retsb.Append("接收到的消息：" + xmlMsg.Content);
            retsb.Append("用户的OPEANID：" + xmlMsg.FromUserName);

            return retsb.ToString();
        }
        private string TextCase(InPutMsgObject xmlMsg)
        {
            int nowtime = ConvertDateTimeInt(DateTime.Now);
            string msg = "";
            msg = getText(xmlMsg);
            string resxml = "<xml>" +
                            "<ToUserName><![CDATA[" + xmlMsg.FromUserName + "]]></ToUserName>" +
                            "<FromUserName><![CDATA[" + xmlMsg.ToUserName + "]]></FromUserName>" +
                            "<CreateTime>" + nowtime + "</CreateTime>" +
                            "<MsgType><![CDATA[text]]></MsgType>" +
                            "<Content><![CDATA[" + msg + "]]></Content>" +
                            "</xml>";
            return resxml;

        }
        #endregion

        #region 将datetime.now 转换为 int类型的秒
        /// <summary>
        /// datetime转换为unixtime
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private int ConvertDateTimeInt(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (int)(time - startTime).TotalSeconds;
        }
        /// <summary>
        /// unix时间转换为datetime
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        private DateTime UnixTimeToTime(string timeStamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp + "0000000");
            TimeSpan toNow = new TimeSpan(lTime);
            return dtStart.Add(toNow);
        }
        #endregion

        #region 获取接收事件推送的XML结构
        /// <summary>
        /// 获取接收事件推送的XML结构  例如：用户发送的文本消息，我们就会得到上述列举的XML结构
        /// </summary>
        /// <returns></returns>
        private XmlDocument GetMsgXml()
        {
            Stream stream = System.Web.HttpContext.Current.Request.InputStream;
            byte[] byteArray = new byte[stream.Length];
            stream.Read(byteArray, 0, (int)stream.Length);
            string postXmlStr = System.Text.Encoding.UTF8.GetString(byteArray);
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(postXmlStr);
            return xmldoc;
        }
        #endregion

        /// <summary>
        /// 获取回复的消息
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        private InPutMsgObject GetInPutMsgObject(XmlElement root)
        {
            InPutMsgObject xmlMsg = new InPutMsgObject()
            {
                FromUserName = root.SelectSingleNode("FromUserName").InnerText,
                ToUserName = root.SelectSingleNode("ToUserName").InnerText,
                CreateTime = root.SelectSingleNode("CreateTime").InnerText,
                MsgType = root.SelectSingleNode("MsgType").InnerText,
            };
            if (xmlMsg.MsgType.Trim().ToLower() == "text")
            {
                xmlMsg.Content = root.SelectSingleNode("Content").InnerText;
            }
            else if (xmlMsg.MsgType.Trim().ToLower() == "event")
            {
                xmlMsg.EventName = root.SelectSingleNode("Event").InnerText;
            }
            return xmlMsg;
        }

        private ActionResult ResponseMsg(string weixin) // 服务器响应微信请求
        {
            return Content(weixin);
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