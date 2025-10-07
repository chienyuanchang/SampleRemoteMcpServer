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
