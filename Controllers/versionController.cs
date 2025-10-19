
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace LearnGamify.Controllers
{

    [Route("[controller]/[action]")]
    [ApiController]
    public class versionController : ControllerBase
    {

        ///
        public versionController(
            )
        {
        }

        /// <summary>
        /// 顯示版本
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult list([FromBody] JsonElement? requestJson = null)
        {
            return Ok("V250715_1353");
        }
    }

}
