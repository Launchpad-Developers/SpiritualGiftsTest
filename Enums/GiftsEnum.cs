using SpiritualGiftsSurvey.Services;

namespace SpiritualGiftsSurvey.Enums;

public enum Gifts
{
    Unknown = -1,
    Prophecy = 0,
    Pastor = 1,
    Teaching = 2,
    Wisdom = 3,
    Knowledge = 4,
    Exhortation = 5,
    Discernment = 6,
    Giving = 7,
    Helps = 8,
    Mercy = 9,
    Missionary = 10,
    Evangelist = 11,
    Hospitality = 12,
    Faith = 13,
    Leadership = 14,
    Administration = 15,
    Miracles = 16,
    Healing = 17,
    Tongues = 18,
    InterpretationOfTongues = 19,
    Intercession = 20,
    Service = 21
}

public static class GiftExtensions
{
    public static Gifts ToGiftEnum(this string giftName)
    {
        return Enum.TryParse<Gifts>(giftName, out var giftEnum)
            ? giftEnum
            : Gifts.Unknown;
    }
}

