using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using ECommons.ChatMethods;

namespace SomethingNeedDoing.Utils;

public static class ChatExtensions
{
    public static void PrintMessage(this IChatGui chat, string message)
        => chat.Print(new XivChatEntry
        {
            Type = C.ChatType,
            Message = $"[{P.Prefix}] {message}"
        });

    public static void PrintErrorMsg(this IChatGui chat, string message)
        => chat.Print(new XivChatEntry
        {
            Type = C.ErrorChatType,
            Message = $"[{P.Prefix}] {message}"
        });

    public static void PrintColor(this IChatGui chat, string message, UIColor color)
        => chat.Print(new XivChatEntry
        {
            Type = C.ChatType,
            Message = new SeString(
                new UIForegroundPayload((ushort)color),
                new TextPayload($"[{P.Prefix}] {message}"),
                UIForegroundPayload.UIForegroundOff)
        });
}
