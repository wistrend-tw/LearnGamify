using Microsoft.AspNetCore.Mvc;

namespace LearnGamify.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        #region Notify ���Ѽ�
        internal readonly String _clientId = "K7DEZMJK2GEDTVSSMQfsgl"; // �����Ӷ}�o���x�� Client ID
        internal readonly string _client_secret = "3gskZrHln6yW1d8H1hpH8zpNGLR4G1Y16fJyIpIetoi"; // �����Ӷ}�o���x�� Client Secret
        internal readonly string _redirectUri = "https://localhost:7102/callback/line/getcodess"; // ������ API �Ψӱ��� Notify �� Ccode
        internal readonly string _state = "AAAA"; // ���e�ۭq�A�Ψӷ� _redirectUri ���� Notify �ǤJ Ccode & state ��A�}�o������ state = _state �ΡA������N�i��D�c�N����

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

        #region ��ͭ����A���ͤѮ�P���

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


        #region ���o notify code�A�e���O notify �}�o�̸ӳ]�w�����]�n

        //[HttpGet]
        //public IActionResult Get()
        //{
        //    var authUrl = $"https://notify-bot.line.me/oauth/authorize?response_type=code&client_id={_clientId}&redirect_uri={_redirectUri}&scope=notify&state={_state}";
        //    // �γo��  authUrl �}���s�����A�K�b���}�C�A�N�|�o�� Code

        //    return Redirect(authUrl);
        //}

        #endregion


        #region ���o notify access token�A�e���O ���o notify code


        //[HttpGet]
        //public IActionResult Get()
        //{
        //    string responseString = string.Empty;
        //    try
        //    {
        //        using (WebClient wc = new WebClient())
        //        {
        //            //���^ accessToken
        //            var uri = new Uri("https://notify-bot.line.me/oauth/token");
        //            string _code = "KiHtUyFUBtNLTPttbmFk2V"; // �Τ᤹�\����^�Ӫ� code

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


        #region �� notify access token�A�o notify


        //[HttpGet]
        //public IActionResult Get()
        //{
        //    using (WebClient wc = new WebClient())
        //    {
        //        // �]�m�ШD�����v���Y
        //        wc.Headers[HttpRequestHeader.Authorization] = "Bearer " + "umJSmqsvLm1bx1W1rUQFM2nAUHVajEKVQ74qBpVW5uV";

        //        // �]�m�ШD�����e����
        //        wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";

        //        // �ǳ� POST �ШD���Ѽ�
        //        var values = new NameValueCollection
        //        {
        //            { "message", "���ߵo�e���\" }
        //        };

        //        try
        //        {
        //            // �o�e POST �ШD������^��
        //            byte[] responseBytes = wc.UploadValues("https://notify-api.line.me/api/notify", "POST", values);
        //            string responseString = Encoding.UTF8.GetString(responseBytes);
        //            // {"status":200,"message":"ok"}
        //        }
        //        catch (WebException ex)
        //        {
        //            // �B�z�ҥ~���p�A�Ҧp�L�Ī� accessToken �κ������D
        //            using (var reader = new System.IO.StreamReader(ex.Response.GetResponseStream()))
        //            {
        //                string errorResponse = reader.ReadToEnd();
        //            }
        //        }
        //    }
        //    return Redirect(string.Empty);
        //}

        #endregion


        #region �� notify access token�A���P Notify


        //[HttpGet]
        //public IActionResult Get()
        //{
        //    string responseString = string.Empty;
        //    using (WebClient wc = new WebClient())
        //    {
        //        // �]�m�ШD�����v���Y
        //        wc.Headers[HttpRequestHeader.Authorization] = "Bearer " + "PVORc2OzC9mnvosG1m56vGRhBcmMGLEHxfo4dI14atR";

        //        // �]�m�ШD�����e����
        //        wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";

        //        try
        //        {
        //            // �o�e POST �ШD������^��
        //            byte[] responseBytes = wc.UploadData("https://notify-api.line.me/api/revoke", "POST", new byte[0]);
        //            responseString = Encoding.UTF8.GetString(responseBytes);
        //            // {"status":200,"message":"ok"}
        //        }
        //        catch (WebException ex)
        //        {
        //            // �B�z�ҥ~���p�A�Ҧp�L�Ī� accessToken �κ������D
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
