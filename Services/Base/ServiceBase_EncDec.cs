using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace LearnGamify.Services.Base
{
    public static class ServiceBase_EncDec
    {

        /// <summary>
        /// AES 加解密，參考: https://www.codeproject.com/Articles/769741/Csharp-AES-bits-Encryption-Library-with-Salt
        /// </summary>
        public static class AES
        {
            #region request、response AES 加解密
            internal static string EncryptAES(string plainText)
            {

                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = Encoding.UTF8.GetBytes(getKey()); // 32-byte key for AES-256
                    aesAlg.IV = Encoding.UTF8.GetBytes(getIv()); // 16-byte IV
                    aesAlg.Mode = CipherMode.CBC;
                    aesAlg.Padding = PaddingMode.PKCS7; // PKCS5 等同於 PKCS7

                    ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(plainText);
                            }
                            byte[] encrypted = msEncrypt.ToArray();
                            return Convert.ToBase64String(encrypted); // 輸出為 Base64 格式
                        }
                    }
                }
            }

            internal static bool DecryptAES(string _requestEncode, ref string _requestDecode)
            {
                int _iCircle = 0; // 記錄解密次數
                return DecryptAES_Circle(_requestEncode, _iCircle, ref _requestDecode);
            }

            internal static bool DecryptAES_Circle(string _requestEncode, int _iCircle, ref string _requestDecode)
            {
                #region
                /*
                因為網路傳遞資料時會，
                確保在 URL 中傳遞的資料不會破壞 URL 結構，也能正確解釋特殊字符。
                URL 中只能包含某些合法字符（如字母、數字和一些符號），
                而許多特殊字符（如空格、中文字符、或某些符號）在 URL 中是非法的或具有特殊意義。
                通過 encodeURIComponent()，可以將這些字符編碼成 URL 安全的格式。

                但開發階段，透過 Postman 來測試，傳遞值是不會加上 URL 編碼

                預設
                1.前台 requset 時，Url 加碼
                2.傳輸平台，自己自動 Url 加碼
                
                所以，用遞迴的方式
                第一次，不進行 Url 解碼，就解密
                第二次，進行 Url 解碼，再解密
                第三次，進行 Url 解碼，再解密

                這三次，只要有解密成功，就停止解密
                若三次都不成功，停止解密
                */
                #endregion

                bool bDecrypt = false;
                try
                {
                    using (Aes aesAlg = Aes.Create())
                    {
                        aesAlg.Key = Encoding.UTF8.GetBytes(getKey()); // 32-byte key for AES-256
                        aesAlg.IV = Encoding.UTF8.GetBytes(getIv()); // 16-byte IV
                        aesAlg.Mode = CipherMode.CBC;
                        aesAlg.Padding = PaddingMode.PKCS7;

                        ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                        using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(_requestEncode)))
                        {
                            using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                            {
                                using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                                {
                                    _requestDecode = srDecrypt.ReadToEnd(); // 解密後的明文
                                    bDecrypt = true;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    string errMsg = ex.Message;
                }

                _iCircle += 1;
                if (
                    _iCircle <= 3
                    && bDecrypt == false
                    )
                {
                    _requestEncode = System.Web.HttpUtility.UrlDecode(_requestEncode);
                    bDecrypt = DecryptAES_Circle(_requestEncode, _iCircle, ref _requestDecode);
                }

                return bDecrypt;
            }

            private static string getKey()
            {
                return "J8v4KO4br977dJfaPTaRKBxLo4iVqY89";
            }

            private static string getIv()
            {
                return "CqmsHbTWV3xoA+Bn";
            }
            #endregion


            #region "定義加密字串變數"
            private static readonly byte[] saltBytes = new byte[] { 156, 165, 69, 196, 56, 86, 53, 62 };
            /*Heap Inspection風險：不要將機密資料使用string紀錄，因為會存在記憶體中，還是有可能被檢索到，需改用SecureString*/
            private static SecureString password = GetPwdSecurity("7_wolf");
            #endregion

            #region "AES 基礎加密密方法"

            /// <summary>
            /// Encrypt Bytes with PasswordBytes
            /// </summary>
            /// <param name="bytesToBeEncrypted"></param>
            /// <param name="passwordBytes"></param>
            /// <returns></returns>
            private static byte[] Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
            {
                byte[]? encryptedBytes = null;

                // Set your salt here, change it to meet your flavor:
                // The salt bytes must be at least 8 bytes.

                using (MemoryStream ms = new MemoryStream())
                {
                    using (Aes AES = Aes.Create())
                    {
                        AES.KeySize = 256;
                        AES.BlockSize = 128;

                        var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                        AES.Key = key.GetBytes(AES.KeySize / 8);
                        AES.IV = key.GetBytes(AES.BlockSize / 8);
                        AES.Mode = CipherMode.CBC;

                        using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                            cs.Close();
                        }
                        encryptedBytes = ms.ToArray();
                    }
                }

                return encryptedBytes;
            }

            /// <summary>
            /// Decrypt encrypted bytes with PasswordBytes
            /// </summary>
            /// <param name="bytesToBeDecrypted"></param>
            /// <param name="passwordBytes"></param>
            /// <returns></returns>
            private static byte[] Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
            {
                byte[]? decryptedBytes = null;

                // Set your salt here, change it to meet your flavor:
                // The salt bytes must be at least 8 bytes.

                using (MemoryStream ms = new MemoryStream())
                {
                    using (Aes AES = Aes.Create())
                    {
                        AES.KeySize = 256;
                        AES.BlockSize = 128;

                        var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                        AES.Key = key.GetBytes(AES.KeySize / 8);
                        AES.IV = key.GetBytes(AES.BlockSize / 8);

                        AES.Mode = CipherMode.CBC;

                        using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                            cs.Close();
                        }
                        decryptedBytes = ms.ToArray();
                    }
                }

                return decryptedBytes;
            }

            private static byte[] GetRandomBytes()
            {
                int saltLength = GetSaltLength();
                byte[] ba = new byte[saltLength];
                RandomNumberGenerator.Create().GetBytes(ba);
                return ba;
            }

            private static int GetSaltLength()
            {
                return 8;
            }


            /// <summary>
            /// AES Encrypt(加密)
            /// </summary>
            /// <param name="input"></param>
            /// <param name="url_encode">是否要 url encode</param>
            /// <returns></returns>
            public static string Encrypt(string? input)
            {
                try
                {
                    return Encrypt(input, false);
                }
                catch
                {
                    return string.Empty;
                }
            }

            /// <summary>
            /// AES Encrypt(加密)
            /// </summary>
            /// <param name="input"></param>
            /// <param name="url_encode">是否要 url encode</param>
            /// <returns></returns>
            public static string Encrypt(string? input, bool url_encode = false)
            {
                try
                {
                    if (string.IsNullOrEmpty(input)) return string.Empty;

                    // Get the bytes of the string
                    byte[] bytesToBeEncrypted = Encoding.UTF8.GetBytes(input);
                    byte[] passwordBytes = Encoding.UTF8.GetBytes(SecureStringToString(password));

                    // Hash the password with SHA256
                    passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

                    byte[] bytesEncrypted = Encrypt(bytesToBeEncrypted, passwordBytes);

                    string result = BitConverter.ToString(bytesEncrypted.ToArray()).Replace("-", string.Empty);

                    if (url_encode)
                    {
                        result = WebUtility.UrlEncode(result);
                    }

                    return result;
                }
                catch
                {
                    return string.Empty;
                }
            }

            /// <summary>
            /// AES Decrypt(解密)
            /// </summary>
            /// <param name="input"></param>
            /// <returns></returns>
            public static string Decrypt(string? input)
            {
                try
                {
                    return Decrypt(input, false);
                }
                catch
                {
                    return string.Empty;
                }
            }


            /// <summary>
            /// AES Decrypt(解密)
            /// </summary>
            /// <param name="input"></param>
            /// <returns></returns>
            public static string Decrypt(string? input, bool url_decode = false)
            {
                try
                {
                    if (string.IsNullOrEmpty(input)) return string.Empty;
                    if (url_decode)
                    {
                        input = WebUtility.UrlDecode(input);
                    }

                    // Get the bytes of the string
                    byte[] bytesToBeDecrypted = new byte[input.Length / 2];
                    int j = 0;
                    for (int i = 0; i < input.Length / 2; i++)
                    {
                        bytesToBeDecrypted[i] = Byte.Parse(input[j].ToString() + input[j + 1].ToString(), System.Globalization.NumberStyles.HexNumber);
                        j += 2;
                    }

                    byte[] passwordBytes = Encoding.UTF8.GetBytes(SecureStringToString(password));
                    passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

                    byte[] bytesDecrypted = Decrypt(bytesToBeDecrypted, passwordBytes);

                    string result = Encoding.UTF8.GetString(bytesDecrypted);

                    return result;
                }
                catch
                {
                    return string.Empty;
                }
            }
            #endregion

            #region Base 64 AES 加解密

            /// <summary>
            /// Base 64 AES Encrypt(加密)
            /// </summary>
            /// <param name="input"></param>
            /// <param name="url_encode">是否要 url encode</param>
            /// <returns></returns>
            public static string Base64Encrypt(string? input, bool url_encode = false)
            {
                try
                {
                    if (string.IsNullOrEmpty(input)) return string.Empty;

                    // Get the bytes of the string
                    byte[] bytesToBeEncrypted = Encoding.UTF8.GetBytes(input);
                    byte[] passwordBytes = Encoding.UTF8.GetBytes(SecureStringToString(password));

                    // Hash the password with SHA256
                    passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

                    byte[] bytesEncrypted = Encrypt(bytesToBeEncrypted, passwordBytes);

                    string result = Convert.ToBase64String(bytesEncrypted);

                    if (url_encode)
                    {
                        result = WebUtility.UrlEncode(result);
                    }

                    return result;
                }
                catch
                {
                    return string.Empty;
                }
            }

            /// <summary>
            /// Base 64 AES Decrypt(解密)
            /// </summary>
            /// <param name="input"></param>
            /// <returns></returns>
            public static string Base64Decrypt(string? input, bool url_decode = false)
            {
                try
                {
                    if (string.IsNullOrEmpty(input)) return string.Empty;
                    if (url_decode)
                    {
                        input = WebUtility.UrlDecode(input);
                    }

                    // Get the bytes of the string
                    byte[] bytesToBeDecrypted = Convert.FromBase64String(input);
                    byte[] passwordBytes = Encoding.UTF8.GetBytes(SecureStringToString(password));
                    passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

                    byte[] bytesDecrypted = Decrypt(bytesToBeDecrypted, passwordBytes);

                    string result = Encoding.UTF8.GetString(bytesDecrypted);

                    return result;
                }
                catch
                {
                    return string.Empty;
                }

            }
            #endregion

            #region Base 64 AES 加解密
            /// <summary>
            /// 亂數 AES Encrypt(加密)
            /// </summary>
            /// <param name="text"></param>
            /// <param name="url_encode"></param>
            /// <returns></returns>
            public static string RandomizedEncrypt(string? text, bool url_encode = false)
            {
                try
                {
                    if (string.IsNullOrEmpty(text)) return string.Empty;

                    byte[] baPwd = Encoding.UTF8.GetBytes(SecureStringToString(password));

                    // Hash the password with SHA256
                    byte[] baPwdHash = SHA256.Create().ComputeHash(baPwd);

                    byte[] baText = Encoding.UTF8.GetBytes(text);

                    byte[] baSalt = GetRandomBytes();
                    byte[] baEncrypted = new byte[baSalt.Length + baText.Length];

                    // Combine Salt + Text
                    for (int i = 0; i < baSalt.Length; i++)
                        baEncrypted[i] = baSalt[i];
                    for (int i = 0; i < baText.Length; i++)
                        baEncrypted[i + baSalt.Length] = baText[i];

                    baEncrypted = Encrypt(baEncrypted, baPwdHash);

                    string result = BitConverter.ToString(baEncrypted.ToArray()).Replace("-", string.Empty);

                    if (url_encode)
                    {
                        result = WebUtility.UrlEncode(result);
                    }

                    return result;
                }
                catch
                {
                    return string.Empty;
                }
            }

            /// <summary>
            /// 亂數 AES Decrypt(解密)
            /// </summary>
            /// <param name="text"></param>
            /// <returns></returns>
            public static string RandomizedDecrypt(string? text, bool url_decode = false)
            {
                try
                {
                    if (string.IsNullOrEmpty(text)) return string.Empty;
                    if (url_decode)
                    {
                        text = WebUtility.UrlDecode(text);
                    }

                    byte[] baPwd = Encoding.UTF8.GetBytes(SecureStringToString(password));

                    // Hash the password with SHA256
                    byte[] baPwdHash = SHA256.Create().ComputeHash(baPwd);

                    byte[] baText = new byte[text.Length / 2];
                    int j = 0;
                    for (int i = 0; i < text.Length / 2; i++)
                    {
                        baText[i] = Byte.Parse(text[j].ToString() + text[j + 1].ToString(), System.Globalization.NumberStyles.HexNumber);
                        j += 2;
                    }

                    byte[] baDecrypted = Decrypt(baText, baPwdHash);

                    // Remove salt
                    int saltLength = GetSaltLength();
                    byte[] baResult = new byte[baDecrypted.Length - saltLength];
                    for (int i = 0; i < baResult.Length; i++)
                        baResult[i] = baDecrypted[i + saltLength];

                    string result = Encoding.UTF8.GetString(baResult);

                    return result;
                }
                catch
                {
                    return string.Empty;
                }

            }

            /// <summary>
            /// 亂數 Base 64 AES Encrypt(加密)
            /// </summary>
            /// <param name="text"></param>
            /// <param name="url_encode"></param>
            /// <returns></returns>
            public static string Randomized64Encrypt(string? text, bool url_encode = false)
            {
                try
                {
                    if (string.IsNullOrEmpty(text)) return string.Empty;

                    byte[] baPwd = Encoding.UTF8.GetBytes(SecureStringToString(password));

                    // Hash the password with SHA256
                    byte[] baPwdHash = SHA256.Create().ComputeHash(baPwd);

                    byte[] baText = Encoding.UTF8.GetBytes(text);

                    byte[] baSalt = GetRandomBytes();
                    byte[] baEncrypted = new byte[baSalt.Length + baText.Length];

                    // Combine Salt + Text
                    for (int i = 0; i < baSalt.Length; i++)
                        baEncrypted[i] = baSalt[i];
                    for (int i = 0; i < baText.Length; i++)
                        baEncrypted[i + baSalt.Length] = baText[i];

                    baEncrypted = Encrypt(baEncrypted, baPwdHash);

                    string result = Convert.ToBase64String(baEncrypted);

                    if (url_encode)
                    {
                        result = WebUtility.UrlEncode(result);
                    }

                    return result;
                }
                catch
                {
                    return string.Empty;
                }
            }

            /// <summary>
            /// 亂數 Base 64 AES Decrypt(解密)
            /// </summary>
            /// <param name="input"></param>
            /// <returns></returns>
            public static string Randomized64Decrypt(string? text, bool url_decode = false)
            {
                try
                {
                    if (string.IsNullOrEmpty(text)) return string.Empty;
                    if (url_decode)
                    {
                        text = WebUtility.UrlDecode(text);
                    }

                    byte[] baPwd = Encoding.UTF8.GetBytes(SecureStringToString(password));

                    // Hash the password with SHA256
                    byte[] baPwdHash = SHA256.Create().ComputeHash(baPwd);

                    byte[] baText = Convert.FromBase64String(text);

                    byte[] baDecrypted = Decrypt(baText, baPwdHash);

                    // Remove salt
                    int saltLength = GetSaltLength();
                    byte[] baResult = new byte[baDecrypted.Length - saltLength];
                    for (int i = 0; i < baResult.Length; i++)
                        baResult[i] = baDecrypted[i + saltLength];

                    string result = Encoding.UTF8.GetString(baResult);
                    return result;
                }
                catch
                {
                    return string.Empty;
                }

            }

            #endregion

            #region 檔案 加解密
            /// <summary>
            /// 加密
            /// </summary>
            /// <param name="original_file_path">欲加密檔案位置（實體路徑）</param>
            /// <param name="encrypted_file_path">加密後檔案位置（實體路徑）</param>
            public static void FileEncrypt(string original_file_path, string encrypted_file_path)
            {
                string file = original_file_path;

                byte[] bytesToBeEncrypted = File.ReadAllBytes(file);
                byte[] passwordBytes = Encoding.UTF8.GetBytes(SecureStringToString(password));

                // Hash the password with SHA256
                passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

                byte[] bytesEncrypted = Encrypt(bytesToBeEncrypted, passwordBytes);

                string fileEncrypted = encrypted_file_path;

                File.WriteAllBytes(fileEncrypted, bytesEncrypted);
            }

            /// <summary>
            /// 檔案解密
            /// </summary>
            /// <param name="encrypted_file_path">欲解密檔案位置（實體路徑）</param>
            /// <param name="decrypted_file_path">解密後檔案位置（實體路徑）</param>
            public static void FileDecrypt(string encrypted_file_path, string decrypted_file_path)
            {
                string fileEncrypted = encrypted_file_path;

                byte[] bytesToBeDecrypted = File.ReadAllBytes(fileEncrypted);
                byte[] passwordBytes = Encoding.UTF8.GetBytes(SecureStringToString(password));
                passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

                byte[] bytesDecrypted = Decrypt(bytesToBeDecrypted, passwordBytes);

                string file = decrypted_file_path;
                File.WriteAllBytes(file, bytesDecrypted);
            }
            #endregion

            #region 其他
            /// <summary>
            /// String to SecureString
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public static SecureString GetPwdSecurity(string value)
            {
                SecureString result = new SecureString();
                foreach (char c in value)
                {
                    result.AppendChar(c);
                }
                return result;
            }

            /// <summary>
            /// SecureString to String
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public static string SecureStringToString(SecureString value)
            {
                IntPtr valuePtr = IntPtr.Zero;
                try
                {
                    valuePtr = Marshal.SecureStringToGlobalAllocUnicode(value);
                    return Marshal.PtrToStringUni(valuePtr) ?? "";
                }
                finally
                {
                    Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
                }
            }
            #endregion
        }

        /// <summary>
        /// 電話號碼遮蔽
        /// </summary>
        public static class Mark
        {
            internal static string Phone(string _phone)
            {
                if (string.IsNullOrEmpty(_phone) || _phone.Length < 10)
                {
                    return _phone;
                }
                else
                {
                    // 使用正則表達式進行遮蔽，保留開頭 4 碼和結尾 3 碼}
                    return Regex.Replace(_phone, @"(\d{4})\d{3}(\d{3})", "$1***$2");
                }
            }
        }



    }
}
