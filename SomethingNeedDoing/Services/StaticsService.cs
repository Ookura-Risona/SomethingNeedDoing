using AutoRetainerAPI;

namespace SomethingNeedDoing.Services;

public static class StaticsService
{
    public static AutoRetainerApi AutoRetainerApi { get; private set; } = null!;
}
