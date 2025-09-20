"""Pydantic models for the MCP server."""

from datetime import datetime
from typing import Any, Dict

from pydantic import BaseModel, Field


class Tool(BaseModel):
    """Metadata describing a tool available from the server."""

    name: str = Field(..., description="Unique identifier of the tool.")
    description: str = Field(..., description="Human readable summary of the tool's purpose.")


class ToolExecutionRequest(BaseModel):
    """Request body used to execute a tool."""

    input: Dict[str, Any] = Field(
        default_factory=dict,
        description="Arbitrary JSON payload that parameterizes the tool execution.",
    )


class ToolExecutionResponse(BaseModel):
    """Standard response returned after running a tool."""

    tool: str = Field(..., description="Identifier of the tool that was invoked.")
    output: Dict[str, Any] = Field(
        default_factory=dict,
        description="Structured result payload produced by the tool.",
    )
    executed_at: datetime = Field(
        default_factory=datetime.utcnow,
        description="Timestamp of when the server finished running the tool.",
    )
