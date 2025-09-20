"""Collection of tools exposed by the MCP server."""

from __future__ import annotations

from datetime import datetime
from random import choice
from typing import Any, Dict
from uuid import uuid4


def echo_tool(payload: Dict[str, Any]) -> Dict[str, Any]:
    """Echo back the supplied payload.

    Parameters
    ----------
    payload:
        Arbitrary JSON-compatible dictionary supplied by the caller.

    Returns
    -------
    dict
        A dictionary containing the original payload and the size of the
        message body, which can be useful for debugging round-trip latency.
    """

    message = payload.get("message", "")
    return {
        "message": message,
        "character_count": len(message),
        "input": payload,
    }


def timestamp_tool(_: Dict[str, Any]) -> Dict[str, Any]:
    """Return the current UTC timestamp."""

    now = datetime.utcnow()
    return {
        "timestamp": now.isoformat() + "Z",
        "epoch_seconds": now.timestamp(),
    }


_QUOTES = [
    "Perfection is achieved not when there is nothing more to add, but when there is nothing left to take away.",
    "In the middle of difficulty lies opportunity.",
    "Simplicity is the soul of efficiency.",
    "The best way to predict the future is to invent it.",
]


def inspiration_tool(_: Dict[str, Any]) -> Dict[str, Any]:
    """Return a random inspirational quote with a unique identifier."""

    quote = choice(_QUOTES)
    return {
        "quote": quote,
        "id": str(uuid4()),
    }


TOOLS = {
    "echo": {
        "function": echo_tool,
        "description": "Echoes the provided message and reports its character length.",
    },
    "timestamp": {
        "function": timestamp_tool,
        "description": "Returns the current UTC time in ISO format and epoch seconds.",
    },
    "inspiration": {
        "function": inspiration_tool,
        "description": "Delivers a random inspirational quote with a unique identifier.",
    },
}


def list_tools() -> Dict[str, Dict[str, Any]]:
    """Return the registry of available tools."""

    return TOOLS


def execute_tool(name: str, payload: Dict[str, Any]) -> Dict[str, Any]:
    """Execute a tool by name.

    Raises
    ------
    KeyError
        If a tool with the supplied ``name`` does not exist.
    """

    if name not in TOOLS:
        raise KeyError(f"Unknown tool: {name}")

    tool_func = TOOLS[name]["function"]
    return tool_func(payload)
