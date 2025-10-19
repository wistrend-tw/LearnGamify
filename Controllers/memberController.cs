using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using LearnGamify.Services;
using LearnGamify.Services.Base;
using LearnGamify.Controllers.Base;
using LearnGamify.MemberDto;
using LearnGamify.ResponseBaseDto;

namespace Sample_API.Controllers.CRUD_Sample
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class memberController : ControllerBase_Auth
    {
        private readonly Service_Member _member;

        public memberController(
            Service_Member member,
            ServiceBase_Authorization authorization  
        ) : base(authorization)
        {
            _member = member;
        }

        /// <summary>
        /// 取得認證 Token
        /// </summary>
        /// <param name="_request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult getAuth([FromBody] RequestDto.GetAuth? _request = null)
        {
            ResponserT<Response_Auth> _responser = new ResponserT<Response_Auth>();
            ///
            if (string.IsNullOrWhiteSpace(_request.id)) { return Ok("必填"); }
            _responser = _authorization.Get_Auth(_request);

            return StatusCode(_responser.statusCode, _responser);
        }

        /// <summary>
        /// 確認會員
        /// </summary>
        /// <param name="_request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult checkAuth()
        {
            Responser _responser = new Responser();

            _responser = _authorization.Check_Auth(Request.Headers);

            return StatusCode(_responser.statusCode, _responser);
        }

        /// <summary>
        /// 新增會員 (需要管理員權限)
        /// </summary>
        /// <param name="_request"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> create([FromBody] RequestDto.Create? _request = null)
        {
            // 驗證參數 (權限檢查由 [Authorize] 自動處理)
            if (string.IsNullOrWhiteSpace(_request.name)) 
            { 
                return Ok("必填"); 
            }

            Responser _responser = new Responser();
            _responser = await _member.Create(_request);

            return StatusCode(_responser.statusCode, _responser);
        }

        /// <summary>
        /// 讀取 會員清單 (需要管理員權限)
        /// </summary>
        /// <param name="_request"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> read_list([FromBody] RequestDto.ReadList? _request = null)
        {
            ///
            if (_request != null)
                if (_request.id <= 0) { return Ok("ID 有誤"); }

            ResponserT<List<ResponseDto.ReadList>> _responser = new ResponserT<List<ResponseDto.ReadList>>();

            _responser = await _member.ReadList(_request);

            return StatusCode(_responser.statusCode, _responser);
        }

        /// <summary>
        /// 修改會員 (需要管理員權限)
        /// </summary>
        /// <param name="_request"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> update([FromBody] RequestDto.Update? _request = null)
        {
            // 驗證參數 (權限檢查由 [Authorize] 自動處理)
            if (_request == null) 
            { 
                return StatusCode(400, new Responser { result = false, statusCode = 400, msg = "ID 必填" }); 
            }
            if (_request.id <= 0) 
            { 
                return StatusCode(400, new Responser { result = false, statusCode = 400, msg = "ID 有誤" }); 
            }

            Responser _responser = new Responser();
            _responser = await _member.Update(_request);

            return StatusCode(_responser.statusCode, _responser);
        }

        /// <summary>
        /// 刪除會員 (需要管理員權限)
        /// </summary>
        /// <param name="_request"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> delete([FromBody] RequestDto.Delete? _request = null)
        {
            // 驗證參數 (權限檢查由 [Authorize] 自動處理)
            if (_request == null) 
            { 
                return StatusCode(400, new Responser { result = false, statusCode = 400, msg = "ID 必填" }); 
            }
            if (_request.id <= 0) 
            { 
                return StatusCode(400, new Responser { result = false, statusCode = 400, msg = "ID 有誤" }); 
            }

            Responser _responser = new Responser();
            _responser = await _member.Delete(_request);

            return StatusCode(_responser.statusCode, _responser);
        }
    }

}
