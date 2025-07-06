namespace SpiritualGiftsTest.Models;

public class RootModel
{
    public DatabaseInfo Database { get; set; } = new();
    public List<Translation> Translations { get; set; } = new();
}
