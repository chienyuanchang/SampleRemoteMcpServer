using ModelContextProtocol.Server;
using System.ComponentModel;

namespace Tools;

[McpServerToolType]
public sealed class GreetingTools
{
    [McpServerTool, Description("Says Hello to a user")]
    public static string Echo(string username)
    {
        return "Hello " + username + " from GreetingTool!";
    }
    [McpServerTool, Description("Echoes in reverse the message sent by the client.")]
    public static string ReverseEcho(string message) => new string(message.Reverse().ToArray());
}
