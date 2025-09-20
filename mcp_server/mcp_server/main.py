"""Entry point and API routes for the MCP server."""

from __future__ import annotations

from typing import List

from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware

from .models import Tool, ToolExecutionRequest, ToolExecutionResponse
from .tools import execute_tool, list_tools


def create_app() -> FastAPI:
    """Create and configure the FastAPI application instance."""

    app = FastAPI(
        title="Model Context Protocol Server",
        description="A minimal MCP server exposing three utility tools.",
        version="0.1.0",
    )

    app.add_middleware(
        CORSMiddleware,
        allow_origins=["*"],
        allow_credentials=True,
        allow_methods=["*"],
        allow_headers=["*"],
    )

    @app.get("/health", summary="Health check", tags=["system"])
    async def health_check() -> dict:
        return {"status": "ok"}

    @app.get("/tools", response_model=List[Tool], summary="List available tools", tags=["tools"])
    async def get_tools() -> List[Tool]:
        registry = list_tools()
        return [Tool(name=name, description=entry["description"]) for name, entry in registry.items()]

    @app.post(
        "/tools/{tool_name}",
        response_model=ToolExecutionResponse,
        summary="Execute a tool",
        tags=["tools"],
    )
    async def run_tool(tool_name: str, request: ToolExecutionRequest) -> ToolExecutionResponse:
        try:
            result = execute_tool(tool_name, request.input)
        except KeyError as exc:  # pragma: no cover - simple error branch
            raise HTTPException(status_code=404, detail=str(exc)) from exc

        return ToolExecutionResponse(tool=tool_name, output=result)

    return app


def run() -> None:
    """Run the MCP server using ``uvicorn``."""

    import uvicorn

    uvicorn.run("mcp_server.main:create_app", host="0.0.0.0", port=8000, factory=True)


app = create_app()
