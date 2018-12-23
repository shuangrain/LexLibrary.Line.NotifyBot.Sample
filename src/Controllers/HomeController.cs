using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LexLibrary.Line.NotifyBot.Sample.Models;
using LexLibrary.Line.NotifyBot.Models;
using LexLibrary.Line.NotifyBot;
using Newtonsoft.Json;

namespace LexLibrary.Line.NotifyBot.Sample.Controllers
{
    public class HomeController : Controller
    {
        private readonly LineNotifyBotApi _lineNotifyBotApi = null;

        public HomeController(LineNotifyBotApi lineNotifyBotApi)
        {
            _lineNotifyBotApi = lineNotifyBotApi;
        }

        public async Task<IActionResult> Index(string code)
        {
            string callBackUrl = string.Format("{0}://{1}{2}", Request.Scheme, Request.Host, Url.Action("CallBack"));

            if (!string.IsNullOrWhiteSpace(code))
            {
                var apiResult = await _lineNotifyBotApi.Token(new TokenRequestDTO
                {
                    Code = code,
                    RedirectUri = callBackUrl
                });

                ViewBag.AccessToken = apiResult.Access_Token;
            }

            ViewBag.ClientAuthorizeUrl = _lineNotifyBotApi.GenerateAuthorizeUrl(callBackUrl, "1234");

            return View();
        }

        public IActionResult CallBack(AuthorizeCallbackDTO callbackDTO)
        {
            // 可能是第三方攻擊
            if (callbackDTO.State != "1234")
            {
                return new UnauthorizedResult();
            }

            return RedirectToAction("Index", new { callbackDTO.Code });
        }

        public async Task<IActionResult> Notify(string accessToken, string message)
        {
            var apiResult = await _lineNotifyBotApi.Notify(new NotifyRequestDTO
            {
                AccessToken = accessToken,
                Message = message
            });

            ViewBag.ResultJson = JsonConvert.SerializeObject(apiResult, Formatting.Indented);

            return PartialView("Notify", accessToken);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
