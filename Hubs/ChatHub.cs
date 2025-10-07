using Microsoft.AspNetCore.SignalR;
using System.Net.Http.Json;
using System.Text.Json;
using Azure.Core;
using Azure.Identity;

namespace SampleRemoteMcpServer.Hubs
{
    public class ChatHub : Hub
    {
        private readonly HttpClient _httpClient;

        public ChatHub(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task SendMessage(string user, string message, string sessionId, string toolList)
        {
            // Use provided sessionId directly
            if (string.IsNullOrEmpty(sessionId))
            {
                await Clients.Caller.SendAsync("ReceiveMessage", "MCP Bot", "No sessionId provided.");
                return;
            }


            // GPT decides whether to call a tool or reply normally
            var gptPrompt = $@"
        You are an assistant. Decide if a tool call is needed.
        User: ""{user}""
        Message: ""{message}""
        Available tools: ""{toolList}""
        Reply ONLY in JSON: 
        - If tool is needed: {{""tool"":""tool_name"",""args"":{{}}}}
        - If no tool needed: {{""tool"":null,""reply"":""Your text reply to user""}}";

            await Clients.Caller.SendAsync("ReceiveMessage", "MCP Bot", "Processing your message...");
            var gptResponse = await CallGptAzureAsync(gptPrompt);
            Console.WriteLine("GPT Response: " + gptResponse);

            // 4️⃣ Parse GPT response
            JsonElement gptJson;
            try
            {
                gptJson = JsonDocument.Parse(gptResponse).RootElement;
            }
            catch
            {
                await Clients.Caller.SendAsync("ReceiveMessage", "MCP Bot", "GPT returned invalid JSON.");
                return;
            }

            string? toolName = gptJson.TryGetProperty("tool", out var toolProp) ? toolProp.GetString() : null;

            // 5️⃣ Case 1: GPT just wants to reply
            if (string.IsNullOrEmpty(toolName) || toolName.ToLower() == "null")
            {
                string reply = gptJson.TryGetProperty("reply", out var r) ? r.GetString() : "GPT did not provide a reply.";
                await Clients.Caller.SendAsync("ReceiveMessage", "GPT", reply);
                return;
            }

            // 6️⃣ Case 2: GPT wants to call a tool → handle args
            JsonElement argsElement;
            if (gptJson.TryGetProperty("args", out var temp) && temp.ValueKind == JsonValueKind.Object)
            {
                argsElement = temp;
            }
            else
            {
                argsElement = JsonDocument.Parse("{}").RootElement;
            }

            // 7️⃣ Call MCP tool
            var payload = new
            {
                jsonrpc = "2.0",
                method = "tools/call",
                @params = new
                {
                    name = toolName,
                    arguments = argsElement
                },
                id = 2
            };

            Console.WriteLine("MCP Payload: " + JsonSerializer.Serialize(payload));

            var response = await _httpClient.PostAsJsonAsync(
                $"http://localhost:5251/message?sessionId={sessionId}",
                payload
            );

            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("MCP tool call failed: " + responseContent);
                await Clients.Caller.SendAsync("ReceiveMessage", "MCP Bot", "Tool call failed: " + responseContent);
                return;
            }

            // 8️⃣ Notify client that result will be posted to SSE
            await Clients.Caller.SendAsync("ReceiveMessage", "MCP Bot", "Tool call sent. Check SSE for result.");
        }
        // 8️⃣ Call Azure OpenAI using Azure.Identity (az login / managed identity)
        private async Task<string> CallGptAzureAsync(string prompt)
        {
            string? endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
            string? deployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT");
            string? apiVersion = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_VERSION") ?? "2024-12-01-preview";

            if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(deployment))
                throw new Exception("Azure OpenAI environment variables are not set.");

            var credential = new DefaultAzureCredential();
            var tokenRequest = new TokenRequestContext(new[] { "https://cognitiveservices.azure.com/.default" });
            AccessToken token = await credential.GetTokenAsync(tokenRequest);

            var url = $"{endpoint}openai/deployments/{deployment}/chat/completions?api-version={apiVersion}";

            var request = new
            {
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                temperature = 0
            };

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(request)
            };
            requestMessage.Headers.Add("Authorization", $"Bearer {token.Token}");

            var response = await _httpClient.SendAsync(requestMessage);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            return json.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";
        }
    }
}
