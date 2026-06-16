# Mini Jira AI Assistant Local Setup

This project uses LM Studio to run the Mini Jira AI Assistant locally.
The assistant is used from the Mini Jira web app and sends requests to the backend endpoint:

```text
POST /api/chats
```

The endpoint requires a logged-in user, so the frontend must send the normal Mini Jira bearer token.

## 1. Install LM Studio

Download and install LM Studio from:

```text
https://lmstudio.ai/
```

## 2. Download a Local Model

Open LM Studio and download a chat/instruct model.

The model used during development was:

```text
qwen2.5-3b-instruct
```

Other instruct models can work too, but the model name in the backend configuration must match the model name shown by LM Studio.

## 3. Start the LM Studio Local Server

In LM Studio:

1. Open the Developer or Local Server section.
2. Load the model.
3. Start the local server.
4. Use this base URL:

```text
http://localhost:1234/v1
```

For local development, keep LM Studio authentication disabled unless the backend is updated to send an LM Studio API token.

## 4. Configure the Backend Locally

Do not commit personal settings or local connection strings to `appsettings.json`.

For local development, set the LM Studio values with environment variables before starting the server.

PowerShell example:

```powershell
$env:LmStudio__Enabled = "true"
$env:LmStudio__BaseUrl = "http://localhost:1234/v1"
$env:LmStudio__Model = "qwen2.5-3b-instruct"
$env:LmStudio__MaxTokens = "900"
```

Then start the Mini Jira backend normally.

## 5. Run Mini Jira

Start the backend and frontend as usual.

Then:

1. Register or log in.
2. Open a project.
3. Go to the Board page.
4. Use the AI Assistant chat window.

The assistant can answer general Mini Jira questions and some user-specific questions, such as projects, assigned tasks, epics, and project summaries.

## 6. Test with Postman or PowerShell

Because the chatbot endpoint requires login, first log in and copy the JWT token from the login response.

Example request:

```http
POST http://localhost:5413/api/chats
Authorization: Bearer YOUR_JWT_TOKEN
Content-Type: application/json

{
  "message": "What projects do I have?"
}
```

PowerShell example:

```powershell
$body = @{
  message = "What projects do I have?"
} | ConvertTo-Json

Invoke-RestMethod `
  -Uri "http://localhost:5413/api/chats" `
  -Method Post `
  -ContentType "application/json" `
  -Headers @{ Authorization = "Bearer YOUR_JWT_TOKEN" } `
  -Body $body
```

## Troubleshooting

If the assistant does not answer:

- Check that LM Studio is running.
- Check that the model is loaded.
- Check that the LM Studio server is using port `1234`.
- Check that LM Studio authentication is disabled.
- Check that the backend model name matches the LM Studio model name.
- Check that the Mini Jira request includes a valid bearer token.

If LM Studio returns an empty answer or reasoning text only, use a non-reasoning instruct model such as `qwen2.5-3b-instruct`.
