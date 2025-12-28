# VPSデプロイ（Docker Compose）

このフォルダには「開発用（DBだけDocker）」と「VPS運用想定（caddy+api+db）」のComposeがあります。

## ローカル開発（VSでF5 + DBだけDocker）

APIはVSでF5起動し、DBだけDockerで用意します。

1. dev用のenvを作成

```bash
cd deploy
cp .env.example .env.dev
```

2. DBだけ起動（ホストから `localhost:5432` で接続できる）

```bash
docker compose -p accounting-devdb -f docker-compose.dev.yml up -d
```

3. VS（またはCLI）側の接続文字列を設定

- 推奨: User Secrets に `ConnectionStrings:Default`
- 接続先: `Host=localhost;Port=5432;Database=<POSTGRES_DB>;Username=<POSTGRES_USER>;Password=<POSTGRES_PASSWORD>`

User Secrets 設定例（1つずつ実行）:

```bash
cd src/AccountingSystem.Api
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=accounting;Username=accounting;Password=change-me;Pooling=true"
dotnet user-secrets set "Jwt:Issuer" "http://localhost"
dotnet user-secrets set "Jwt:Audience" "http://localhost"
dotnet user-secrets set "Jwt:SigningKey" "replace-with-a-long-random-string"
dotnet user-secrets set "Jwt:AccessTokenMinutes" "60"
```

### 代替（Developmentでも env で統一したい場合）

`deploy/.env.dev` に `CONNECTIONSTRINGS__DEFAULT` と `Jwt__*` を書いておけば、Development 実行時に API が自動で読み込みます。

- `.env.dev` は gitignore 済みなので、秘密情報をコミットしません
- User Secrets / 環境変数が設定されている場合は、そちらが優先されます（`.env.dev` は「足りないキーだけ」補います）

4. VSで `AccountingSystem.Api` をF5起動して、Postman/ブラウザから `http://localhost:<VSのポート>/...` を呼び出す

## テスト実行について

このリポジトリのAPI統合テストは **Testcontainers** を使って、テスト時に Postgres コンテナを自動で起動します。

- ローカルでテストを実行する場合は **Docker Desktop（またはDocker Engine）が起動している必要があります**
- CIでも同じ方式で動くため、ローカルとCIで挙動が揃います

停止:

```bash
cd deploy
docker compose -p accounting-devdb -f docker-compose.dev.yml down
```

## 使い方（VPS上）

事前準備:

- Docker / Docker Compose
- ドメインのDNS（A/AAAAをVPSへ）
- FWで `80/tcp` と `443/tcp`

1. `deploy/.env.example` をコピーして `.env.prod` を作成
2. `deploy/Caddyfile` のドメイン / `email` を自分のものに変更
3. `deploy/` で起動

```bash
cd deploy
cp .env.example .env.prod
# edit .env.prod and Caddyfile

docker compose -f docker-compose.prod.yml up -d --build
```

## 重要

- DBデータは `pg_data` ボリュームに保存されます。
- 本番相当ではバックアップを必ず別ストレージへ退避してください。

### `.env.prod` に秘密情報を入れてよいか

入れてOKです（むしろ Compose 運用では一般的です）。ただし **絶対に Git にコミットしない/共有しない** ことが前提です。

- このリポジトリでは `deploy/.env.prod` は gitignore されています
- VPS 上ではファイル権限を絞って管理してください（例: `chmod 600 deploy/.env.prod`）
