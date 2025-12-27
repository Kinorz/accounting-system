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
