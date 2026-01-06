**共通：Bearerトークンを付けて外部APIを呼ぶ基本（HttpClient）**
- `Authorization: Bearer <token>` をヘッダーに付けます。
- `HttpClient` はDI（`IHttpClientFactory`）で管理するのが定番です（ただし最小例として直生成でも可）。

---

**1) JSON形式のリクエスト（`application/json`）**

**最小例（tokenを付けてPOST JSON）**
```csharp
using System.Net.Http.Headers;
using System.Net.Http.Json;

var token = "<access token>";
var url = "https://api.example.com/messages";

using var client = new HttpClient();

client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);

var payload = new
{
    title = "hello",
    body = "world",
    to = new[] { "user1", "user2" },
};

using var response = await client.PostAsJsonAsync(url, payload);
response.EnsureSuccessStatusCode();

var json = await response.Content.ReadAsStringAsync();
```

**ポイント**
- `PostAsJsonAsync` は `Content-Type: application/json` を付けてくれます。
- レスポンスを型で受けるなら `ReadFromJsonAsync<T>()` を使えます。

---

**2) ファイルを含む form-data（`multipart/form-data`）**

**最小例（tokenを付けて title + file を送る）**
```csharp
using System.Net.Http.Headers;
using System.Text;

var token = "<access token>";
var url = "https://api.example.com/upload";

using var client = new HttpClient();
client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);

using var form = new MultipartFormDataContent();

// text項目
form.Add(new StringContent("my title", Encoding.UTF8), "Title");

// file項目（byte[]から作る例）
var fileBytes = await File.ReadAllBytesAsync(@"C:\path\to\sample.pdf");
using var fileContent = new ByteArrayContent(fileBytes);
fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

// 第3引数はファイル名（IFormFile.FileName相当）
form.Add(fileContent, "File", "sample.pdf");

using var response = await client.PostAsync(url, form);
response.EnsureSuccessStatusCode();

var body = await response.Content.ReadAsStringAsync();
```

**ポイント**
- `MultipartFormDataContent` を使うと `multipart/form-data; boundary=...` を自動で組み立てます（自分で Content-Type を手で設定しないのが基本）。
- `"Title"` / `"File"` のキー名は、相手APIの受け取り側（フォームフィールド名）に合わせます。
- ファイルは `StreamContent` でもOK（大きいファイルなら `FileStream` を使う方がメモリに優しい）。

---

**応用：JSON + file を multipart に同梱する場合**
相手が「JSON + file」を1リクエストで求めるときは、JSONを文字列として1パーツに入れます。

```csharp
using System.Net.Http.Headers;

var token = "<access token>";
var url = "https://api.example.com/upload";

using var client = new HttpClient();
client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);

using var form = new MultipartFormDataContent();

var json = """{"title":"hello","to":["u1","u2"]}""";
form.Add(new StringContent(json), "metadata"); // ←相手のフィールド名に合わせる

await using var stream = File.OpenRead(@"C:\path\to\hello.txt");
using var streamContent = new StreamContent(stream);
streamContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
form.Add(streamContent, "file", "hello.txt");

using var response = await client.PostAsync(url, form);
response.EnsureSuccessStatusCode();
```

---

相手の外部API仕様で、フィールド名が `Title/File` なのか `title/file` なのか、またレスポンスがJSONかどうかが変わるので、もしAPI仕様（URL、必要なキー名、期待レスポンス）が分かれば、それに合わせた完成版のコードに寄せられます。