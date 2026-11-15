namespace PersonalFinance.Services;

public sealed record ThemePreset(string Name,string Mode,string BackgroundColor,string TextColor,string AccentColor,string FontChoice)
{
    public static IReadOnlyList<ThemePreset> All {get;} =
    [
        new("Obsidiana","Oscuro","#183050","#F5F7FF","#5D86F1","Space Grotesk"),
        new("Perla","Claro","#C9D8EB","#202B3E","#4778C7","Space Grotesk"),
        new("Amatista","Oscuro","#5E427C","#FAF5FF","#A477DB","Space Grotesk"),
        new("Ámbar","Claro","#D8B897","#30231C","#AD6A3C","Georgia")
    ];
}
