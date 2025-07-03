using System;
namespace SpiritualGiftsTest.Utilities;

public class TitledException : Exception
{
    public string ExceptionTitle { get; set; }

    public TitledException(string title, string message) : base(message)
    {
        ExceptionTitle = title;
    }
}
