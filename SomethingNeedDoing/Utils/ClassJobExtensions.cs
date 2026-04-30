using Lumina.Excel.Sheets;

namespace SomethingNeedDoing.Utils;

public static class ClassJobExtensions
{
    // from XIVDeck
    public static Role GetRole(this ClassJob row) => (Role)(row.UIPriority / 10);

    public enum Role
    {
        Tank = 0,
        Healer = 1,
        Melee = 2,
        PhysRanged = 3,
        Caster = 4,
        DoH = 10,
        DoL = 20
    }
}
