using Microsoft.AspNetCore.Mvc;

namespace LearnGamify.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        #region Notify 的參數
        internal readonly String _clientId = "K7DEZMJK2GEDTVSSMQfsgl"; // 供應商開發平台的 Client ID
        internal readonly string _client_secret = "3gskZrHln6yW1d8H1hpH8zpNGLR4G1Y16fJyIpIetoi"; // 供應商開發平台的 Client Secret
        internal readonly string _redirectUri = "https://localhost:7102/callback/line/getcodess"; // 供應者 API 用來接收 Notify 的 Ccode
        internal readonly string _state = "AAAA"; // 內容自訂，用來當 _redirectUri 收到 Notify 傳入 Ccode & state 後，開發者驗證 state = _state 用，不等於就可能遭惡意攻擊

        #endregion

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        #region 原生頁面，產生天氣與氣溫

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        #endregion


        #region 取得 notify code，前提是 notify 開發者該設定的都設好

        //[HttpGet]
        //public IActionResult Get()
        //{
        //    var authUrl = $"https://notify-bot.line.me/oauth/authorize?response_type=code&client_id={_clientId}&redirect_uri={_redirectUri}&scope=notify&state={_state}";
        //    // 用這個  authUrl 開啟瀏覽器，貼在網址列，就會得到 Code

        //    return Redirect(authUrl);
        //}

        #endregion


        #region 取得 notify access token，前提是 取得 notify code


        //[HttpGet]
        //public IActionResult Get()
        //{
        //    string responseString = string.Empty;
        //    try
        //    {
        //        using (WebClient wc = new WebClient())
        //        {
        //            //取回 accessToken
        //            var uri = new Uri("https://notify-bot.line.me/oauth/token");
        //            string _code = "KiHtUyFUBtNLTPttbmFk2V"; // 用戶允許後取回來的 code

        //            var parameters = new NameValueCollection
        //            {
        //                { "grant_type", "authorization_code" },
        //                { "code", _code },
        //                { "redirect_uri", "https://localhost:7102/callback/line/getcodess" },
        //                { "client_id", _clientId },
        //                { "client_secret", _client_secret }
        //            };

        //            byte[] responseBytes = wc.UploadValues(uri, "POST", parameters);
        //            //{
        //            //"status" : 200,
        //            //"message" : "access_token is issued",
        //            //"access_token" : "IOtXTkEQjalNteb0UCWNcT41XQOJPuJ4011NkWwbbif"     
        //            //}
        //            responseString = Encoding.UTF8.GetString(responseBytes);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        string msg = ex.Message;
        //    }

        //    return Content(responseString);
        //}

        #endregion


        #region 用 notify access token，發 notify


        //[HttpGet]
        //public IActionResult Get()
        //{
        //    using (WebClient wc = new WebClient())
        //    {
        //        // 設置請求的授權標頭
        //        wc.Headers[HttpRequestHeader.Authorization] = "Bearer " + "umJSmqsvLm1bx1W1rUQFM2nAUHVajEKVQ74qBpVW5uV";

        //        // 設置請求的內容類型
        //        wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";

        //        // 準備 POST 請求的參數
        //        var values = new NameValueCollection
        //        {
        //            { "message", "恭喜發送成功" }
        //        };

        //        try
        //        {
        //            // 發送 POST 請求並獲取回應
        //            byte[] responseBytes = wc.UploadValues("https://notify-api.line.me/api/notify", "POST", values);
        //            string responseString = Encoding.UTF8.GetString(responseBytes);
        //            // {"status":200,"message":"ok"}
        //        }
        //        catch (WebException ex)
        //        {
        //            // 處理例外情況，例如無效的 accessToken 或網路問題
        //            using (var reader = new System.IO.StreamReader(ex.Response.GetResponseStream()))
        //            {
        //                string errorResponse = reader.ReadToEnd();
        //            }
        //        }
        //    }
        //    return Redirect(string.Empty);
        //}

        #endregion


        #region 用 notify access token，註銷 Notify


        //[HttpGet]
        //public IActionResult Get()
        //{
        //    string responseString = string.Empty;
        //    using (WebClient wc = new WebClient())
        //    {
        //        // 設置請求的授權標頭
        //        wc.Headers[HttpRequestHeader.Authorization] = "Bearer " + "PVORc2OzC9mnvosG1m56vGRhBcmMGLEHxfo4dI14atR";

        //        // 設置請求的內容類型
        //        wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";

        //        try
        //        {
        //            // 發送 POST 請求並獲取回應
        //            byte[] responseBytes = wc.UploadData("https://notify-api.line.me/api/revoke", "POST", new byte[0]);
        //            responseString = Encoding.UTF8.GetString(responseBytes);
        //            // {"status":200,"message":"ok"}
        //        }
        //        catch (WebException ex)
        //        {
        //            // 處理例外情況，例如無效的 accessToken 或網路問題
        //            using (var reader = new System.IO.StreamReader(ex.Response.GetResponseStream()))
        //            {
        //                string errorResponse = reader.ReadToEnd();
        //            }
        //        }
        //    }
        //    return Content(responseString);
        //}

        #endregion
    }
}
