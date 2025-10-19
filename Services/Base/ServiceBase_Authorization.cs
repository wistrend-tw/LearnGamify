using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LearnGamify.ResponseBaseDto;
using LearnGamify.MemberDto;

namespace LearnGamify.Services.Base
{
    public partial class ServiceBase_Authorization
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// 建構
        /// </summary>
        /// <param name="objContext"></param>
        public ServiceBase_Authorization(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        internal ResponserT<Response_Auth> Get_Auth(RequestDto.GetAuth request)
        {
            ResponserT<Response_Auth> _responser = new ResponserT<Response_Auth>();
            _responser.data = new Response_Auth();
            _responser.data.access = GenerateToken(enumToken.AccessToken, request.id, request.role);
            _responser.data.refreshToken = GenerateToken(enumToken.RefreshToken, request.id, request.role);

            return _responser;
        }

        internal Responser Check_Auth(IHeaderDictionary _headers)
        {
            Responser _responser = new Responser();
            IdCard _token = Token_Slip(_headers);
            if (!_token.result)
            {
                _responser.result = false;
                _responser.statusCode = 401;
                _responser.msg = _token.msg;
                return _responser;
            }

            _responser.msg = $"取得會員ID = {_token.sub}, 角色 = {_token.role}";
            return _responser;
        }


        internal IdCard Token_Slip(IHeaderDictionary _headers)
        {
            IdCard registeredClaims = new IdCard();
            try
            {
                // 判斷 Authorization 必須存在且不為空
                if (!(
                   _headers.ContainsKey("Authorization")
                    && !string.IsNullOrEmpty(_headers["Authorization"])
                    ))
                {
                    registeredClaims.result = false;
                    registeredClaims.msg = "Token 必須存在";
                    return registeredClaims;
                }

                // 解出 JWT
                string serializeToken = _headers["Authorization"].ToString();
                if (_headers["Authorization"].ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    // 由 Postman Requset：authorization header 傳入
                    serializeToken = _headers["Authorization"].ToString().Substring("Bearer ".Length).Trim();
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var securityToken = tokenHandler.ReadToken(serializeToken) as JwtSecurityToken;
                if (securityToken == null)
                {
                    registeredClaims.result = false;
                    registeredClaims.msg = "Token 無法解出";
                    return registeredClaims;
                }

                // 解出 JWT 的屬性
                registeredClaims.issuer = securityToken.Issuer; // 取得發行者
                registeredClaims.audience = securityToken.Audiences.FirstOrDefault(); // 取得受眾
                
                foreach (var claim in securityToken.Claims) // 取得 sub 和 role
                {
                    if (claim.Type == "sub")
                    {
                        registeredClaims.sub = ServiceBase_EncDec.AES.Decrypt(claim.Value);
                    }
                    else if (claim.Type == ClaimTypes.Role || claim.Type == "role") // 支援標準和舊格式
                    {
                        registeredClaims.role = claim.Value;
                    }
                }
                if (string.IsNullOrWhiteSpace(registeredClaims.sub))
                {
                    registeredClaims.result = false;
                    registeredClaims.msg = "Token 無法解出 sub";
                    return registeredClaims;
                }

                // 驗證，sub (對 sub 做拆解 & 判斷)
                {
                }

                // 驗證，過期時間
                DateTime expires = securityToken.ValidTo; // Token 過期時間為 (取得的為 UTC 時間)
                if (DateTime.Now.AddHours(-8) > securityToken.ValidTo)
                {
                    registeredClaims.result = false;
                    registeredClaims.msg = "Token 失效";
                    return registeredClaims;
                }

                if (string.IsNullOrEmpty(registeredClaims.sub))
                {
                    registeredClaims.result = false;
                    registeredClaims.msg = "Token 不合法";
                    return registeredClaims;
                }
            }
            catch (SecurityTokenExpiredException ex)
            {
                registeredClaims.result = false;
                registeredClaims.msg = ex.Message;
                return registeredClaims;
            }
            catch (Exception ex)
            {
                registeredClaims.result = false;
                registeredClaims.msg = ex.Message;
                return registeredClaims;
            }

            return registeredClaims;
        }

        internal string GenerateToken(enumToken tokenType, string userID, string role = "")
        {
            var issuer = _configuration.GetValue<string>("JwtSettings:Issuer"); //發行者
            var audience = _configuration.GetValue<string>("JwtSettings:Audience"); //接收者
            var secretKey = _configuration.GetValue<string>("JwtSettings:SecretKey");   //簽章金鑰
            var encryptkey = _configuration.GetValue<string>("JwtSettings:Encryptkey"); //加密金鑰
            var expireMinutes = ExpireMinutes(tokenType);  // 設定過期時間(單位：分鐘)

            // 建立使用者的 Claims 聲明，這會是 JWT Payload 的一部分
            var claims = new List<Claim>();
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, ServiceBase_EncDec.AES.Encrypt(userID))); // 有加密的 UserID
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            
            // 加入 role claim (如果有提供的話)
            if (!string.IsNullOrEmpty(role))
            {
                // claims.Add(new Claim("role", role)); // 舊的自定義 role claim，已改用標準方式
                claims.Add(new Claim(ClaimTypes.Role, role)); // 標準角色 Claim 供 [Authorize] 使用
            }
            
            var claimsIdentity = new ClaimsIdentity(claims);

            // 取得對稱式加密 JWT Signature 的金鑰
            var symmetricSecurityKey_secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey_secretKey, SecurityAlgorithms.HmacSha256Signature);

            //// 加密JWT的金鑰 ( 這裡照著 will 的 API 是註解的 )
            //var symmetricSecurityKey_encryptkey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(encryptkey));
            //var encryptingCredentials = new EncryptingCredentials(symmetricSecurityKey_encryptkey, SecurityAlgorithms.Aes128KW, SecurityAlgorithms.Aes128CbcHmacSha256);

            // 建立 JWT TokenHandler 以及用於描述 JWT 的 TokenDescriptor
            var tokenHandler = new JwtSecurityTokenHandler();
            DateTime expires = DateTime.Now.AddMinutes(expireMinutes);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = issuer,
                Audience = audience,
                Subject = claimsIdentity,
                Expires = expires, // Mingo 為了測試，暫時把時效限制取消
                SigningCredentials = signingCredentials,
                //EncryptingCredentials = encryptingCredentials,
            };

            // 產出所需要的 JWT Token 物件
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            // 產出序列化的 JWT Token 字串
            var serializeToken = tokenHandler.WriteToken(securityToken);

            return serializeToken;
        }

        internal double ExpireMinutes(enumToken tokenType)
        {
            var _look = tokenType == enumToken.AccessToken ? //過期時間(單位：分鐘);
             _configuration.GetValue<int>("JwtSettings:ExpireMinutes_accessToken")
             : _configuration.GetValue<int>("JwtSettings:ExpireMinutes_refreshToken")
             ;
            return tokenType == enumToken.AccessToken ? //過期時間(單位：分鐘);
             _configuration.GetValue<int>("JwtSettings:ExpireMinutes_accessToken")
             : _configuration.GetValue<int>("JwtSettings:ExpireMinutes_refreshToken")
             ;
        }
    }

    public class IdCard
    {
        // iss (Issuer): 發行者
        public string issuer { get; set; }
        // aud (Audience): 受眾，接收 JWT 的一方
        public string audience { get; set; }

        // sub (Subject): 主題，通常是用戶 ID
        public string sub { get; set; }
        
        // role: 用戶角色
        public string role { get; set; }

        public bool result { get; set; } = true;
        public string msg { get; set; }
    }
    public class Response_Auth
    {
        public string? access { get; set; }
        public string? refreshToken { get; set; }
        public string? msg { get; set; }
    }
    public enum enumToken
    {
        AccessToken = 1,
        RefreshToken = 2
    }
}