using SpiritualGiftsSurvey.Enums;
using SpiritualGiftsSurvey.Models;
using System.Text.Json.Serialization;

namespace SpiritualGiftsSurvey.Services;

/// <summary>
/// JSON source generation context for Release-safe, linker-safe, AOT-compatible JSON serialization.
/// This eliminates reflection-based serialization that fails in iOS NativeAOT and Android Release builds.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    Converters = [typeof(JsonStringEnumConverter)])]
[JsonSerializable(typeof(RootModel))]
[JsonSerializable(typeof(DatabaseInfo))]
[JsonSerializable(typeof(Translation))]
[JsonSerializable(typeof(AppString))]
[JsonSerializable(typeof(LanguageOption))]
[JsonSerializable(typeof(Question))]
[JsonSerializable(typeof(GiftDescription))]
[JsonSerializable(typeof(Verse))]
[JsonSerializable(typeof(Reflection))]
[JsonSerializable(typeof(Gifts))]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(List<Translation>))]
[JsonSerializable(typeof(List<AppString>))]
[JsonSerializable(typeof(List<LanguageOption>))]
[JsonSerializable(typeof(List<Question>))]
[JsonSerializable(typeof(List<GiftDescription>))]
[JsonSerializable(typeof(List<Verse>))]
[JsonSerializable(typeof(List<Reflection>))]
internal partial class AppJsonContext : JsonSerializerContext
{
}
