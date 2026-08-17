# menou-store License Auth

menou-store がBOOTH等で販売するUnityツール共通の、パスワード認証Editor拡張です。

## 含まれるパッケージ

| パッケージID | 内容 |
|---|---|
| `com.menoustore.license` | 購入者向けパスワード認証ウィンドウ・認証状態API |

## 仕組み

- `auth.json`（このリポジトリのルート）に、配布用パスワードのハッシュ値（ソルト付きSHA256）を保管します。実際のパスワードそのものは保管しません。
- 各販売ツールは Editor起動時などに `MenouStore.License.LicenseAuth.IsAuthenticated()` で認証済みかを確認し、未認証なら `LicenseAuth.OpenAuthWindow()` を呼んで認証ウィンドウを開きます。
- ユーザーが入力したパスワードは `auth.json` から取得したハッシュと照合され、一致すればそのUnityプロジェクト内で認証済み状態が保存されます（`EditorPrefs`）。
- パスワードのローテーションは `auth.json` を書き換えて `main` へpushするだけで完了します（購入者側の再インストールは不要）。

## パスワードの更新方法

`auth.json` の該当エントリの `salt` / `hash` を書き換えて push してください。`hash` は `SHA256(salt + password)` の16進文字列です。
