using Microsoft.AspNetCore.Mvc;
using LearnGamify.ResponseBaseDto;
using LearnGamify.Services.Base;


namespace LearnGamify.Controllers.Base
{
    /// <summary>
    /// 基礎控制器，提供通用功能
    /// </summary>
    /// <remarks>
    /// 🎯 權限控制使用指南：
    /// 
    /// ✅ 推薦做法 - 使用內建 [Authorize] 屬性：
    /// [HttpPost]
    /// [Authorize(Roles = "admin")]
    /// public async Task&lt;IActionResult&gt; SomeAction() { ... }
    /// 
    /// ⚠️  舊做法 - 手動檢查（已棄用但保留）：
    /// var result = ValidateAdminPermission();
    /// if (result != null) return result;
    /// 
    /// 💡 其他用法：
    /// [Authorize(Roles = "admin,manager")]     // 多個角色
    /// [Authorize(Policy = "CustomPolicy")]    // 自定義策略
    /// </remarks>
    public class ControllerBase_Auth : ControllerBase
    {
        protected readonly ServiceBase_Authorization _authorization;

        public ControllerBase_Auth(ServiceBase_Authorization authorization)
        {
            _authorization = authorization;
        }

        /// <summary>
        /// 檢查用戶是否有指定角色權限
        /// </summary>
        /// <param name="requiredRole">需要的角色，預設為 admin</param>
        /// <returns>權限驗證結果</returns>
        /// <remarks>
        /// ⚠️  已棄用：建議使用 [Authorize(Roles = "admin")] 屬性替代
        /// 保留此方法僅供參考或特殊情況使用
        /// </remarks>
        [Obsolete("建議使用 [Authorize(Roles = \"admin\")] 屬性替代此手動檢查方法")]
        protected Responser CheckRolePermission(string requiredRole = "admin")
        {
            Responser _responser = new Responser();
            
            // 先驗證 Token 是否有效
            IdCard _token = _authorization.Token_Slip(Request.Headers);
            if (!_token.result)
            {
                _responser.result = false;
                _responser.statusCode = 401;
                _responser.msg = _token.msg; // Token 錯誤訊息 (如過期等)
                return _responser;
            }

            // 驗證角色權限
            if (string.IsNullOrEmpty(_token.role) || _token.role != requiredRole)
            {
                _responser.result = false;
                _responser.statusCode = 403;
                _responser.msg = $"權限不足，需要 {requiredRole} 角色才能執行此操作";
                return _responser;
            }

            // 權限驗證通過
            _responser.result = true;
            _responser.statusCode = 200;
            _responser.msg = $"權限驗證通過，用戶角色：{_token.role}";
            return _responser;
        }

        /// <summary>
        /// 檢查管理員權限的方法
        /// </summary>
        /// <returns>權限驗證結果</returns>
        /// <remarks>
        /// ⚠️  已棄用：建議使用 [Authorize(Roles = "admin")] 屬性替代
        /// 保留此方法僅供參考或特殊情況使用
        /// </remarks>
        [Obsolete("建議使用 [Authorize(Roles = \"admin\")] 屬性替代此手動檢查方法")]
        protected Responser CheckAdminPermission()
        {
            return CheckRolePermission("admin");
        }

        /// <summary>
        /// 取得當前用戶資訊
        /// </summary>
        /// <returns>用戶資訊</returns>
        protected IdCard GetCurrentUser()
        {
            return _authorization.Token_Slip(Request.Headers);
        }

        /// <summary>
        /// 執行權限檢查，如果沒有權限直接回傳錯誤回應
        /// </summary>
        /// <param name="requiredRole">需要的角色，預設為 admin</param>
        /// <returns>如果沒有權限回傳 IActionResult，有權限回傳 null</returns>
        /// <remarks>
        /// ⚠️  已棄用：建議使用 [Authorize(Roles = "admin")] 屬性替代
        /// 保留此方法僅供參考或特殊情況使用
        /// </remarks>
        [Obsolete("建議使用 [Authorize(Roles = \"admin\")] 屬性替代此手動檢查方法")]
        protected IActionResult? ValidatePermission(string requiredRole = "admin")
        {
            var permissionCheck = CheckRolePermission(requiredRole);
            if (!permissionCheck.result)
            {
                return StatusCode(permissionCheck.statusCode, permissionCheck);
            }
            return null; // 權限驗證通過，回傳 null 表示可以繼續執行
        }

        /// <summary>
        /// 執行管理員權限檢查的方法
        /// </summary>
        /// <returns>如果沒有權限回傳 IActionResult，有權限回傳 null</returns>
        /// <remarks>
        /// ⚠️  已棄用：建議使用 [Authorize(Roles = "admin")] 屬性替代
        /// 保留此方法僅供參考或特殊情況使用
        /// </remarks>
        [Obsolete("建議使用 [Authorize(Roles = \"admin\")] 屬性替代此手動檢查方法")]
        protected IActionResult? ValidateAdminPermission()
        {
            return ValidatePermission("admin");
        }
    }
}