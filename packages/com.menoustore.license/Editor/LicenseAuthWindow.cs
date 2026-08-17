using UnityEditor;
using UnityEngine;

namespace MenouStore.License
{
    public class LicenseAuthWindow : EditorWindow
    {
        private string _productId = "default";
        private string _password = "";
        private string _status = "";
        private MessageType _statusType = MessageType.None;
        private bool _busy;

        [MenuItem("menou-store/License Authentication")]
        private static void OpenFromMenu()
        {
            Open("default");
        }

        [MenuItem("menou-store/Reset License Authentication")]
        private static void ResetFromMenu()
        {
            LicenseAuth.ResetAuthentication("default");
            EditorUtility.DisplayDialog("menou-store 認証", "認証状態をリセットしました。", "OK");
        }

        public static void Open(string productId)
        {
            var window = GetWindow<LicenseAuthWindow>(true, "menou-store 認証", true);
            window._productId = productId;
            window.minSize = new Vector2(380, 160);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("購入時に配布されたパスワードを入力してください。", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space(4);

            GUI.enabled = !_busy;
            EditorGUI.BeginChangeCheck();
            var newPassword = EditorGUILayout.PasswordField("パスワード", _password);
            if (EditorGUI.EndChangeCheck())
            {
                _password = newPassword;
            }

            EditorGUILayout.Space(6);
            if (GUILayout.Button(_busy ? "認証中..." : "認証"))
            {
                Submit();
            }
            GUI.enabled = true;

            if (!string.IsNullOrEmpty(_status))
            {
                EditorGUILayout.Space(6);
                EditorGUILayout.HelpBox(_status, _statusType);
            }
        }

        private void Submit()
        {
            _busy = true;
            _status = "";
            LicenseAuth.Verify(_productId, _password, (success, reason) =>
            {
                _busy = false;

                if (success)
                {
                    _status = "認証に成功しました。";
                    _statusType = MessageType.Info;
                    Repaint();
                    Close();
                    return;
                }

                switch (reason)
                {
                    case LicenseAuth.VerifyFailReason.Network:
                        _status = "通信に失敗しました。ファイアウォールでUnityの通信がブロックされていないか確認してください。";
                        _statusType = MessageType.Warning;
                        break;
                    case LicenseAuth.VerifyFailReason.ProductNotFound:
                        _status = "認証情報の取得に失敗しました。時間をおいて再度お試しください。";
                        _statusType = MessageType.Warning;
                        break;
                    default:
                        _status = "パスワードが違います。購入時にお渡しした内容をご確認ください。";
                        _statusType = MessageType.Error;
                        break;
                }
                Repaint();
            });
        }
    }
}
