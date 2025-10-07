# SampleRemoteMcpServer

## Introduction
SampleRemoteMcpServer is a sample implementation of a Model Context Protocol (MCP) server built with .NET. It demonstrates how to expose tools and endpoints for communication between clients and the server using HTTP and Server-Sent Events (SSE). 

The purpose of this project is to:
- Showcase how to build and run an MCP server using .NET.
- Demonstrate integration with VS Code and the MCP extension.
- Provide sample endpoints and tools for client-server interaction.
- Serve as a starting point for developers building their own MCP-compatible servers.

# Start the Server
Under this project folder `SampleRemoteMpcServer`
Start the server
```
dotnet run
```

# Interact with MCP Server
## Option 1 with UI
Go to http://localhost:5251/index.html

## Option 2 with Bash 
1. Go to `http://localhost:5251/sse?sessionId=<string>` Page
    Will see 
    ```
    event: endpoint
    data: /message?sessionId=<sessionId>
    ```

2. Use the `sessionId` to POST and can see the result on `http://localhost:5251/sse?sessionId=<string>` Page
    - List tools
        ```bash
        curl -X POST "http://localhost:5251/message?sessionId=<sessionId>" \
            -H "Content-Type: application/json" \
            -d '{
                "jsonrpc": "2.0",
                "method": "tools/list",
                "params": {},
                "id": 1
                }'
        ```

    - Use tool
        ```bash
        curl -X POST "http://localhost:5251/message?sessionId=<sessionId>" \
            -H "Content-Type: application/json" \
            -d '{
                "jsonrpc": "2.0",
                "method": "tools/call",
                "params": {
                    "name": "echo",
                    "arguments": {
                    "username": "<username>"
                    }
                },
                "id": 2
                }'
        ```

## Option 3 with Github Copliot in VS code
1. Use Ctrl+Shift+P to open VS Code’s command palette and select `> MCP: Add Server...`.  
    ![mcp_add_server](Images\readme\mcp_add_server.png)
2. Next select `HTTP (HTTP or Server-Sent Events)`.  
    ![select_http](Images\readme\select_http.png)
3. Enter `http://localhost:5251/` as sever url.  
    ![input_url](Images\readme\input_url.png)
4. Enter a custom Server ID or use the generated ID  
    ![enter_server_id](Images\readme\enter_server_id.png)
5. Pick Workspace.  
    ![pick_workspace](Images\readme\pick_workspace.png)
6. MCP configs would be added into `settings.json`.
    ```json
        "mcp": {
        "servers": {
            "my-mcp-server-4193fcbc": {
                "url": "http://localhost:5251/"
            }
        }
    }
    ```
7. Open VS Code Copilot with Agent mode  
    ![agent_mode](Images\readme\agent_mode.png)
8. Check the toolbox icon and can find this MCP server we just set up  
    ![toolbox_icon](Images\readme\toolbox_icon.png)
    ![mcp_server_in_toolbox](Images\readme\mcp_server_in_toolbox.png)