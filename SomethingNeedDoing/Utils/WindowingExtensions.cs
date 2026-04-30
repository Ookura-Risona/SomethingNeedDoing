using Dalamud.Interface.Windowing;

namespace SomethingNeedDoing.Utils;

public static class WindowingExtensions
{
    public static Window? GetWindow<T>(this WindowSystem ws) where T : Window => ws.Windows.OfType<T>().FirstOrDefault();
    public static void Toggle<T>(this WindowSystem ws) where T : Window => GetWindow<T>(ws)?.IsOpen ^= true;
}
