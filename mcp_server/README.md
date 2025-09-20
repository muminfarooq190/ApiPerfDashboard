# MCP Server

This directory contains a minimal Model Context Protocol (MCP) server written in
Python. The server is implemented with [FastAPI](https://fastapi.tiangolo.com/)
and exposes three tools that demonstrate different capabilities you can plug
into an MCP ecosystem.

## Available tools

| Tool        | Description                                                                 |
|-------------|-----------------------------------------------------------------------------|
| `echo`      | Echoes a provided message and reports the total character count.            |
| `timestamp` | Returns the current UTC time in ISO 8601 format and raw epoch seconds.      |
| `inspiration` | Responds with a random inspirational quote, each tagged with a unique ID. |

## Running locally

1. **Install dependencies**

   ```bash
   cd mcp_server
   pip install -e .
   ```

2. **Start the server**

   ```bash
   mcp-server
   ```

   The server listens on `http://0.0.0.0:8000` by default. You can also run it
   via `uvicorn` directly if you need extra options:

   ```bash
   uvicorn mcp_server.main:create_app --factory --host 0.0.0.0 --port 8000
   ```

3. **Interact with the API**

   - List tools:

     ```bash
     curl http://localhost:8000/tools
     ```

   - Execute the echo tool:

     ```bash
     curl -X POST http://localhost:8000/tools/echo \
       -H "Content-Type: application/json" \
       -d '{"input": {"message": "Hello MCP"}}'
     ```

   - The automatic documentation is available at
     `http://localhost:8000/docs` for a Swagger UI view or
     `http://localhost:8000/redoc` for ReDoc.

## Project layout

```
mcp_server/
├── mcp_server/
│   ├── __init__.py
│   ├── main.py
│   ├── models.py
│   └── tools.py
├── pyproject.toml
└── README.md
```

## Notes on repository creation

This environment does not have access to GitHub, so a remote repository cannot
be created automatically. The project is organized locally so that you can push
it to a new GitHub repository with:

```bash
git init
git remote add origin <your-repo-url>
git add .
git commit -m "Initial commit"
git push -u origin main
```

Replace `<your-repo-url>` with the URL of the GitHub repository you create
through the GitHub web interface.
