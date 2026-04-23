# MiniJiraAspire.Server

## Local Database Setup (Docker)

1. Create a `.env` file in the same directory as `docker-compose.yml`:

```
POSTGRES_USER=<user>
POSTGRES_PASSWORD=<password>
POSTGRES_DB=<db-name>
ConnectionStrings__DefaultConnection=Host=localhost;Port=<port>;Database=<db-name>;Username=<user>;Password=<password>
```

> Replace `<user>`,`<password>`,`<port>`,`<db-name>` with your values. The connection string must match the Postgres credentials above.

2. Start the PostgreSQL container (you have to be in the directory of `docker-compose.yml`):

```bash
docker compose up -d
```

3. Create a migration if required (optional):

```bash
dotnet ef migrations add <name>
```

4. Apply migrations:

```bash
dotnet ef database update
```

5. Stop the container:

```bash
docker compose down
```

> The `.env` file is listed in `.gitignore` and will not be committed to the repository.
