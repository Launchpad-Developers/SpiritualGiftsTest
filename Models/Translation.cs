using SQLite;

namespace SpiritualGiftsTest.Models;

[Table("Translation")]
public class Translation
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public Guid TranslationGuid { get; set; }
    public string Code { get; set; } = string.Empty;
    public string FlowDirection { get; set; } = "LTR";
    [Ignore] public List<AppString> AppStrings { get; set; } = new();
    [Ignore] public List<LanguageOption> LanguageOptions { get; set; } = new();
    [Ignore] public List<Question> Questions { get; set; } = new();
    [Ignore] public List<GiftDescription> GiftDescriptions { get; set; } = new();
    [Ignore] public List<Reflection> Reflections { get; set; } = new();
}

