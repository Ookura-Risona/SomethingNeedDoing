namespace SomethingNeedDoing.Utils;

public static class ObjectExtensions
{
    public static int ToInt(this object obj)
    {
        if (obj is null || string.IsNullOrWhiteSpace(obj.ToString())) return default;
        return int.TryParse(obj.ToString(), out var result) ? result : default;
    }

    public static float ToFloat(this object obj)
    {
        if (obj is null || string.IsNullOrWhiteSpace(obj.ToString())) return default;
        return float.TryParse(obj.ToString(), out var result) ? result : default;
    }

    public static bool ToBool(this object obj)
    {
        if (obj is null || string.IsNullOrWhiteSpace(obj.ToString())) return default;
        return bool.TryParse(obj.ToString(), out var result) && result;
    }
}
