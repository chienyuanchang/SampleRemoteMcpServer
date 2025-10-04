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
}
