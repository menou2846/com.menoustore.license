using System;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace MenouStore.License
{
    /// <summary>
    /// menou-store販売ツール共通のパスワード認証API。
    /// 各ツールは起動時に IsAuthenticated() で確認し、未認証なら OpenAuthWindow() を呼ぶ。
    /// </summary>
    public static class LicenseAuth
    {
        private const string AuthUrl = "https://raw.githubusercontent.com/menou2846/com.menoustore.license/main/auth.json";
        private const string PrefsKeyPrefix = "MenouStore.License.Verified.";

        [Serializable]
        private class ProductEntry
        {
            public string id;
            public string salt;
            public string hash;
        }

        [Serializable]
        private class AuthFile
        {
            public int version;
            public ProductEntry[] products;
        }

        public static bool IsAuthenticated(string productId = "default")
        {
            return EditorPrefs.GetBool(PrefsKeyPrefix + productId, false);
        }

        public static void ResetAuthentication(string productId = "default")
        {
            EditorPrefs.DeleteKey(PrefsKeyPrefix + productId);
        }

        public static void OpenAuthWindow(string productId = "default")
        {
            LicenseAuthWindow.Open(productId);
        }

        internal enum VerifyFailReason
        {
            None,
            WrongPassword,
            Network,
            ProductNotFound,
        }

        internal static void Verify(string productId, string password, Action<bool, VerifyFailReason> onComplete)
        {
            var request = UnityWebRequest.Get(AuthUrl + "?t=" + DateTime.UtcNow.Ticks);
            var op = request.SendWebRequest();
            op.completed += _ =>
            {
                try
                {
#if UNITY_2020_1_OR_NEWER
                    bool ok = request.result == UnityWebRequest.Result.Success;
#else
                    bool ok = !request.isNetworkError && !request.isHttpError;
#endif
                    if (!ok)
                    {
                        onComplete(false, VerifyFailReason.Network);
                        return;
                    }

                    var data = JsonUtility.FromJson<AuthFile>(request.downloadHandler.text);
                    ProductEntry entry = null;
                    if (data?.products != null)
                    {
                        foreach (var p in data.products)
                        {
                            if (p.id == productId)
                            {
                                entry = p;
                                break;
                            }
                        }
                    }

                    if (entry == null)
                    {
                        onComplete(false, VerifyFailReason.ProductNotFound);
                        return;
                    }

                    var computed = ComputeHash(entry.salt, password);
                    if (string.Equals(computed, entry.hash, StringComparison.OrdinalIgnoreCase))
                    {
                        EditorPrefs.SetBool(PrefsKeyPrefix + productId, true);
                        onComplete(true, VerifyFailReason.None);
                    }
                    else
                    {
                        onComplete(false, VerifyFailReason.WrongPassword);
                    }
                }
                catch (Exception)
                {
                    onComplete(false, VerifyFailReason.Network);
                }
                finally
                {
                    request.Dispose();
                }
            };
        }

        private static string ComputeHash(string salt, string password)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(salt + password);
                var hashBytes = sha.ComputeHash(bytes);
                var sb = new StringBuilder(hashBytes.Length * 2);
                foreach (var b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}
