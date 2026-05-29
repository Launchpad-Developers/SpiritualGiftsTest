# Release-Safe Patterns — .NET MAUI Production Guidelines

**Purpose:** Define patterns that work reliably in Release builds with trimming, linking, and AOT compilation.

**Target Platform:** .NET 9/10 MAUI (iOS NativeAOT, Android Linker)  
**Status:** DRAFT — Standards to be adopted during stabilization  
**Last Updated:** January 2025

---

## Executive Summary

**The Problem:**
- 71% of identified bugs appear ONLY in Release builds
- Reflection-based code fails under trimming/AOT
- Async timing changes under optimization
- Linker removes types/methods assumed to exist at runtime

**The Solution:**
- Use source generation instead of reflection
- Configure trimming explicitly
- Avoid dynamic type loading
- Test Release builds early and often

---

## Core Principles

1. **Assume aggressive trimming** — If you don't reference it statically, it may be removed
2. **Prefer source generation** — Reflection fails under AOT/trimming
3. **Test Release builds** — Debug builds hide problems
4. **Annotate carefully** — Use `[DynamicallyAccessedMembers]` when reflection is unavoidable
5. **Favor static over dynamic** — Compile-time analysis beats runtime discovery

---

## Critical Release-Only Failures

### Issue 1: Reflection-Based JSON Serialization ❌
**Problem:**
```csharp
// Services/UrlService.cs:52-56 — FAILS in Release/AOT
var options = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
    Converters = { new JsonStringEnumConverter() } // ❌ Reflection-based
};
var rootModel = JsonSerializer.Deserialize<RootModel>(json, options); // ❌ Reflection-based
```

**Why It Fails:**
- iOS NativeAOT: Reflection-based deserialization throws at runtime
- Android Linker: May trim properties, causing missing JSON fields
- Trimmer: Removes types not statically referenced

**Symptoms:**
- `rootModel` deserializes as `null`
- Properties missing or default values
- "Failed to parse database" errors
- Only happens in Release builds on device

**Solution: JSON Source Generation ✅**
```csharp
// Services/AppJsonContext.cs
using System.Text.Json.Serialization;

[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(RootModel))]
[JsonSerializable(typeof(List<string>))]
internal partial class AppJsonContext : JsonSerializerContext
{
}

// Services/UrlService.cs — Use source-generated context
var rootModel = JsonSerializer.Deserialize(json, AppJsonContext.Default.RootModel);

// Services/EmailService.cs
var list = JsonSerializer.Deserialize(json, AppJsonContext.Default.ListString);
var json = JsonSerializer.Serialize(list, AppJsonContext.Default.ListString);
```

**Testing:**
- MUST test on physical iOS device (Release build)
- MUST test on physical Android device (Release build)
- Verify database sync from Firebase works
- Verify email generation includes all data

---

### Issue 2: No Trimming Configuration ❌
**Problem:**
```xml
<!-- SpiritualGiftsSurvey.csproj — Missing configuration -->
<PropertyGroup>
  <TargetFrameworks>net9.0-android;net9.0-ios</TargetFrameworks>
  <!-- ❌ No PublishTrimmed -->
  <!-- ❌ No TrimMode -->
  <!-- ❌ No linker config -->
</PropertyGroup>
```

**Why It Fails:**
- Default trimming behavior is unpredictable
- .NET 10 may use more aggressive defaults
- No control over what gets trimmed

**Solution: Explicit Trimming Configuration ✅**
```xml
<PropertyGroup>
  <TargetFrameworks>net9.0-android;net9.0-ios</TargetFrameworks>
  
  <!-- Trimming Configuration -->
  <PublishTrimmed>true</PublishTrimmed>
  <TrimMode>partial</TrimMode>
  <TrimmerDefaultAction>link</TrimmerDefaultAction>
  
  <!-- Enable trimming analyzers -->
  <EnableTrimAnalyzer>true</EnableTrimAnalyzer>
  <SuppressTrimAnalysisWarnings>false</SuppressTrimAnalysisWarnings>
  
  <!-- iOS Linker Settings -->
  <MtouchLink Condition="'$(TargetFramework)' == 'net9.0-ios'">SdkOnly</MtouchLink>
  
  <!-- Android Linker Settings -->
  <AndroidLinkMode Condition="'$(TargetFramework)' == 'net9.0-android'">SdkOnly</AndroidLinkMode>
</PropertyGroup>
```

**Linker Configuration (if needed):**
Create `TrimmerRoots.xml`:
```xml
<linker>
  <!-- Preserve all models (used for serialization) -->
  <assembly fullname="SpiritualGiftsSurvey">
    <type fullname="SpiritualGiftsSurvey.Models.*" preserve="all" />
    <type fullname="SpiritualGiftsSurvey.Enums.*" preserve="all" />
  </assembly>
</linker>
```

**Testing:**
- Build Release config: `dotnet build -c Release`
- Check build warnings for trimming issues
- Deploy to device and test all features

---

### Issue 3: TranslateExtension Uses Reflection ⚠️
**Problem:**
```csharp
// Extensions/TranslateExtension.cs (if exists) — Reflection risk
public object ProvideValue(IServiceProvider serviceProvider)
{
    // ❌ May use reflection to lookup resources
    var resourceManager = new ResourceManager(...);
    return resourceManager.GetString(Key);
}
```

**Why It's Risky:**
- Trimmer may remove resource strings
- Reflection-based lookup may fail in AOT

**Solution: Database-Backed Translations ✅**
The app already uses database-backed translations via `TranslationService`, which is trim-safe:
```csharp
// Services/TranslationService.cs — Already trim-safe
public string GetString(string key, string defaultValue = "")
{
    // Direct database lookup, no reflection
    var translation = _databaseService.GetAppString(key);
    return translation ?? defaultValue;
}
```

**No action needed** — current implementation is trim-safe.

---

## Trimming & AOT Patterns

### ✅ SAFE: Static Type References
```csharp
// Compiler knows about these types
var model = new RootModel();
var list = new List<Translation>();
var gift = Gifts.Teaching;

// Source-generated serialization
var json = JsonSerializer.Serialize(model, AppJsonContext.Default.RootModel);
```

### ✅ SAFE: DI with Concrete Types
```csharp
// MauiProgram.cs — Statically referenced
services.AddSingleton<IDatabaseService, DatabaseService>();
services.AddSingleton<INavigationService, NavigationService>();

// Injected types are preserved
public class SurveyViewModel : BaseViewModel
{
    private readonly IDatabaseService _db;
    
    public SurveyViewModel(IDatabaseService db)
    {
        _db = db; // ✅ Safe
    }
}
```

### ✅ SAFE: XAML Compiled Bindings
```xml
<!-- SurveyPage.xaml — Compiled bindings are trim-safe -->
<ContentPage xmlns:vm="clr-namespace:SpiritualGiftsSurvey.Views.Survey"
             x:DataType="vm:SurveyViewModel">
    
    <!-- ✅ Property resolved at compile time -->
    <Label Text="{Binding Title}" />
    
    <!-- ✅ Command resolved at compile time -->
    <Button Command="{Binding NextCommand}" />
</ContentPage>
```

### ⚠️ RISKY: Reflection-Based Serialization
```csharp
// ❌ Fails in Release/AOT
var json = JsonSerializer.Serialize(obj); // Uses reflection

// ✅ Use source generation
var json = JsonSerializer.Serialize(obj, AppJsonContext.Default.MyType);
```

### ⚠️ RISKY: Type.GetType() or Assembly.Load()
```csharp
// ❌ Type may be trimmed
var type = Type.GetType("SpiritualGiftsSurvey.Models.RootModel");
var instance = Activator.CreateInstance(type);

// ✅ Use static references
var instance = new RootModel();
```

### ⚠️ RISKY: Reflection on Properties
```csharp
// ❌ Properties may be trimmed
var props = typeof(Translation).GetProperties();
foreach (var prop in props)
{
    var value = prop.GetValue(translation);
}

// ✅ Access properties directly
var code = translation.Code;
var flowDirection = translation.FlowDirection;
```

### ❌ UNSAFE: Dynamic Assembly Loading
```csharp
// ❌ Fails in AOT (no assembly loading)
var asm = Assembly.LoadFrom("Plugin.dll");
var type = asm.GetType("Plugin.MyType");

// ✅ Use compile-time plugin system or source generation
```

---

## Source Generation Patterns

### JSON Serialization
```csharp
// AppJsonContext.cs
[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    WriteIndented = false, // Reduce size
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(RootModel))]
[JsonSerializable(typeof(DatabaseInfo))]
[JsonSerializable(typeof(Translation))]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(Dictionary<string, object>))]
internal partial class AppJsonContext : JsonSerializerContext
{
}

// Usage
var model = JsonSerializer.Deserialize(json, AppJsonContext.Default.RootModel);
var json = JsonSerializer.Serialize(model, AppJsonContext.Default.RootModel);
```

### Future: Logging Source Generation (Optional)
```csharp
// If using Microsoft.Extensions.Logging
[LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Failed to load database: {Error}")]
private partial void LogDatabaseError(string error);

// Usage
LogDatabaseError(ex.Message);
```

---

## Trimming Annotations (When Reflection is Unavoidable)

### [DynamicallyAccessedMembers]
Use when you MUST use reflection:
```csharp
using System.Diagnostics.CodeAnalysis;

public class ReflectionHelper
{
    // Tell trimmer: preserve all public properties of T
    public static T CreateInstance<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>()
        where T : new()
    {
        var instance = new T();
        var props = typeof(T).GetProperties(); // ⚠️ Reflection
        return instance;
    }
}
```

### [RequiresUnreferencedCode]
Mark methods that use reflection and may fail when trimmed:
```csharp
[RequiresUnreferencedCode("Uses reflection for JSON deserialization")]
public static T DeserializeLegacy<T>(string json)
{
    // ⚠️ Reflection-based (only for legacy code)
    return JsonSerializer.Deserialize<T>(json);
}
```

**Note:** Prefer fixing to source generation over using these attributes.

---

## Platform-Specific Considerations

### iOS NativeAOT
**Strictest Environment** — Test here first!

**Restrictions:**
- ❌ No `Assembly.Load()` or `Assembly.LoadFrom()`
- ❌ No `Reflection.Emit`
- ❌ No dynamic code generation at runtime
- ⚠️ Reflection-based serialization fails
- ⚠️ `Type.GetType()` may return null

**Mitigations:**
- Use JSON source generation
- Use compiled bindings in XAML
- Avoid reflection entirely
- Test on physical device (simulator is NOT AOT)

### Android Linker
**Less Strict** — But still aggressive

**Restrictions:**
- ⚠️ Unused types/methods removed
- ⚠️ Reflection-based code may fail
- ✅ Dynamic loading sometimes works (but avoid)

**Mitigations:**
- Same as iOS (source generation, static references)
- Test on physical device in Release mode
- Use `AndroidLinkMode=SdkOnly` to reduce risk

---

## Testing Strategy

### Release Build Testing Checklist
**Before Every Release:**

- [ ] Build Release configuration for Android
- [ ] Build Release configuration for iOS
- [ ] Deploy to Android physical device (not emulator)
- [ ] Deploy to iOS physical device (not simulator)
- [ ] Test database sync from Firebase
- [ ] Complete full survey (250+ questions)
- [ ] Test ranking algorithm
- [ ] Generate and send email
- [ ] Switch languages (English → Arabic → English)
- [ ] Test all navigation paths
- [ ] Monitor logs for errors/warnings
- [ ] Performance check (startup time, memory)

### Debug vs Release Differences to Watch

| Feature | Debug | Release |
|---------|-------|---------|
| Trimming | Off | On (aggressive) |
| AOT (iOS) | Partial | Full NativeAOT |
| Linker (Android) | Minimal | SdkOnly or Full |
| Optimization | Off | On (may change timing) |
| Symbols | Full | Minimal |
| Reflection | Works | May fail |

**⚠️ Critical:** Issues that appear ONLY in Release builds are much harder to debug. Test early, test often.

---

## Configuration Checklist

### SpiritualGiftsSurvey.csproj
```xml
<PropertyGroup>
  <!-- ✅ Required -->
  <PublishTrimmed>true</PublishTrimmed>
  <TrimMode>partial</TrimMode>
  <TrimmerDefaultAction>link</TrimmerDefaultAction>
  
  <!-- ✅ Recommended -->
  <EnableTrimAnalyzer>true</EnableTrimAnalyzer>
  <SuppressTrimAnalysisWarnings>false</SuppressTrimAnalysisWarnings>
  
  <!-- ✅ Platform-specific -->
  <MtouchLink Condition="'$(TargetFramework)' == 'net9.0-ios'">SdkOnly</MtouchLink>
  <AndroidLinkMode Condition="'$(TargetFramework)' == 'net9.0-android'">SdkOnly</AndroidLinkMode>
  
  <!-- ✅ Performance (optional) -->
  <AndroidEnableProfiledAot>true</AndroidEnableProfiledAot>
  <RunAOTCompilation Condition="'$(TargetFramework)' == 'net9.0-ios'">true</RunAOTCompilation>
</PropertyGroup>
```

### Services/AppJsonContext.cs
```csharp
// ✅ Required for Release builds
[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(RootModel))]
[JsonSerializable(typeof(List<string>))]
internal partial class AppJsonContext : JsonSerializerContext { }
```

### All Serialization Call Sites
```csharp
// ✅ Replace all reflection-based calls
// Before: JsonSerializer.Deserialize<T>(json)
// After:  JsonSerializer.Deserialize(json, AppJsonContext.Default.T)
```

---

## Common Pitfalls

### Pitfall 1: "It Works in Debug" Syndrome
**Problem:** Relying on Debug builds for validation

**Solution:**
- Test Release builds weekly (minimum)
- CI/CD should build Release, not Debug
- Device testing MUST use Release builds

### Pitfall 2: Reflection Without Fallback
**Problem:**
```csharp
var type = Type.GetType(typeName); // ❌ May return null in Release
var instance = Activator.CreateInstance(type); // ❌ Throws
```

**Solution:**
```csharp
// Avoid reflection entirely, or:
var type = Type.GetType(typeName);
if (type == null)
{
    // Handle gracefully
    Logger.LogWarning("Type {TypeName} not found (trimmed?)", typeName);
    return null;
}
```

### Pitfall 3: Assuming Optimizer Won't Change Behavior
**Problem:** Async timing, race conditions masked in Debug

**Solution:**
- Fix async anti-patterns (see `async-and-threading-guidelines.md`)
- Avoid fire-and-forget
- Test Release builds

### Pitfall 4: Hardcoded Strings for Type Names
**Problem:**
```csharp
var type = Type.GetType("SpiritualGiftsSurvey.Models.RootModel"); // ❌
```

**Solution:**
```csharp
var type = typeof(RootModel); // ✅ Statically referenced
```

---

## Migration Priorities

| Priority | Task | Effort | Release Impact |
|----------|------|--------|----------------|
| CRITICAL | Add trimming config | 0.5 day | Blocks Release |
| CRITICAL | Implement JSON source generation | 1 day | Blocks Release |
| HIGH | Test Release builds on devices | 1 day | Validation |
| MEDIUM | Review XAML bindings (if any classic bindings exist) | 0.5 day | May fail in Release |
| LOW | Add trimming annotations (if reflection needed) | 0.5 day | Improve safety |

---

## Best Practices Summary

### ✅ DO
- Use JSON source generation for all serialization
- Configure trimming explicitly
- Test Release builds on physical devices
- Use compiled bindings in XAML
- Reference types statically
- Use DI with concrete type registrations

### ❌ DON'T
- Use reflection-based serialization
- Rely on Debug builds for validation
- Use `Type.GetType()` or `Assembly.Load()`
- Assume trimmer preserves unused code
- Use classic XAML bindings (prefer compiled)
- Test only on emulator/simulator

---

## Resources

- [.NET Trimming Documentation](https://learn.microsoft.com/en-us/dotnet/core/deploying/trimming/trim-self-contained)
- [JSON Source Generation](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/source-generation)
- [.NET NativeAOT](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/)
- [MAUI Trimming](https://learn.microsoft.com/en-us/dotnet/maui/deployment/overview#trimming)

---

**Document Owner:** Architecture & Modernization Initiative  
**Last Updated:** January 2025  
**Next Review:** After JSON source generation implementation
