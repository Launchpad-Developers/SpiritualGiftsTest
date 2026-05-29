# .NET 10 Migration Assessment — Spiritual Gifts Survey

**Status:** NOT READY FOR MIGRATION  
**Assessment Date:** January 2025  
**Current Framework:** .NET 9.0 (net9.0-android, net9.0-ios)  
**Current MAUI:** 9.0.81  
**Target Framework:** .NET 10.0 (net10.0-android, net10.0-ios)  
**Target MAUI:** 10.x (TBD)

---

## Executive Summary

**The application is NOT ready to migrate to .NET 10.**

Three CRITICAL blockers must be resolved BEFORE migration:
1. **No trimming configuration** — Release builds will fail under .NET 10's more aggressive default trimming
2. **Reflection-based JSON serialization** — Will break under iOS AOT and Android linker
3. **Async void lifecycle** — Timing issues will worsen under .NET 10 runtime optimizations

**Recommendation:** Fix all PRE-MIGRATION issues (7-8 days effort) before attempting .NET 10 upgrade.

---

## Migration Readiness Assessment

### ✅ What's Ready
- **Platform Targets:** iOS 15.0+ and Android API 23+ are compatible with .NET 10/MAUI 10
- **CommunityToolkit:** Both Maui 12.1.0 and Mvvm 8.4.0 are conceptually compatible
- **SQLite:** sqlite-net-pcl 1.7.335 is .NET Standard 2.0, should work without changes
- **Core Architecture:** MVVM pattern, Shell navigation, DI container are all compatible

### ❌ What's NOT Ready
- **Trimming Configuration:** Missing entirely — .NET 10 will use aggressive defaults
- **Serialization:** Reflection-based JSON will fail under stricter trimming/AOT
- **Async Patterns:** Fire-and-forget and async void will be more fragile
- **Thread Safety:** Race conditions may worsen under optimized runtime
- **Package Versions:** Some need updates (see Package Compatibility Matrix)

---

## .NET 10 / MAUI 10 Changes

### Trimming & AOT
- **.NET 10 Default Trimming:** More aggressive than .NET 9
- **iOS NativeAOT:** Stricter enforcement of AOT-safe patterns
- **Analyzers:** Better detection of trim-unsafe code (good for us if we fix issues first)
- **Source Generation:** Preferred/required for serialization, logging, etc.

### MAUI 10 Expected Changes
- **Platform Updates:** Likely bumps to newer iOS/Android SDKs (verify release notes)
- **Breaking Changes:** API changes possible (review migration guide when available)
- **Performance Improvements:** Better startup, smaller app size with proper trimming
- **Shell Navigation:** May have timing/lifecycle improvements (or regressions)

### System.Text.Json
- **Source Generation:** Remains the recommended path
- **Trimming Safety:** Reflection-based serialization increasingly discouraged
- **Performance:** Source-gen has lower overhead, especially on AOT platforms

---

## Critical Migration Blockers

### MR-001: JSON Source Generation Required [CRITICAL]
**Current State:**
- `Services/UrlService.cs:52-56` uses `JsonSerializer.Deserialize<RootModel>()` with reflection
- `Services/EmailService.cs:126-153` serializes `List<string>` with reflection
- No `JsonSerializerContext` defined
- Uses `JsonStringEnumConverter` (reflection-based)

**Why It's a Blocker:**
- Release builds on iOS can fail deserialization entirely (AOT limitation)
- Android linker may trim properties, causing missing JSON fields
- .NET 10 trimming will be MORE aggressive, increasing failure likelihood

**Impact on Migration:**
- HIGH — Migration will make the problem worse
- Fix BEFORE migration to avoid debugging on two fronts

**Remediation:** (PRE-MIGRATION)
1. Create `Services/AppJsonContext.cs`:
```csharp
[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(RootModel))]
[JsonSerializable(typeof(List<string>))]
internal partial class AppJsonContext : JsonSerializerContext { }
```
2. Replace `UrlService.cs` deserialization:
```csharp
// Before
var rootModel = JsonSerializer.Deserialize<RootModel>(json, options);

// After
var rootModel = JsonSerializer.Deserialize(json, AppJsonContext.Default.RootModel);
```
3. Replace `EmailService.cs` serialization:
```csharp
// Before
var json = JsonSerializer.Serialize(list);

// After  
var json = JsonSerializer.Serialize(list, AppJsonContext.Default.ListString);
```
4. Test Release builds on both platforms

**Estimated Effort:** 1 day

---

### MR-002: Trimming Configuration Required [CRITICAL]
**Current State:**
- `SpiritualGiftsSurvey.csproj` has NO trimming configuration
- No `PublishTrimmed`, `TrimMode`, or `TrimmerDefaultAction` settings
- No linker configuration files

**Why It's a Blocker:**
- .NET 10 will use aggressive default trimming
- Without explicit configuration, Release builds are unpredictable
- Can cause runtime crashes from missing types/methods

**Impact on Migration:**
- HIGH — .NET 10 defaults may differ from .NET 9
- Must establish baseline trimming behavior before migration

**Remediation:** (PRE-MIGRATION)
Add to `SpiritualGiftsSurvey.csproj` inside `<PropertyGroup>`:
```xml
<!-- Trimming Configuration -->
<PublishTrimmed>true</PublishTrimmed>
<TrimMode>partial</TrimMode>
<TrimmerDefaultAction>link</TrimmerDefaultAction>

<!-- iOS Specific -->
<MtouchLink Condition="'$(TargetFramework)' == 'net9.0-ios'">SdkOnly</MtouchLink>

<!-- Android Specific -->
<AndroidLinkMode Condition="'$(TargetFramework)' == 'net9.0-android'">SdkOnly</AndroidLinkMode>
```

Then create `TrimmerRoots.xml`:
```xml
<linker>
  <assembly fullname="SpiritualGiftsSurvey">
    <type fullname="SpiritualGiftsSurvey.Models.*" preserve="all" />
  </assembly>
</linker>
```

**Estimated Effort:** 0.5 day (includes Release build testing)

---

### MR-003: Async Void Lifecycle [CRITICAL]
**Current State:**
- `Views/Splash/SplashPage.xaml.cs:21-26` — `async void OnAppearing()`
- `Views/Shared/BasePage.cs:22-27` — calls `ViewModel.InitAsync()` without await
- `Views/Shared/BaseViewModel.cs:37-40` — calls `InitAsync()` in message handler without await

**Why It's a Blocker:**
- Exceptions are unobservable
- Navigation timing is unpredictable
- .NET 10 runtime optimizations may change async timing, exposing hidden races
- Fire-and-forget patterns become MORE fragile under optimization

**Impact on Migration:**
- HIGH — Optimized runtime will likely expose more timing bugs
- These are correctness issues regardless of .NET version

**Remediation:** (PRE-MIGRATION)
Fix lifecycle initialization:
```csharp
// BasePage.xaml.cs
protected override async void OnNavigatedTo(NavigatedToEventArgs args)
{
    base.OnNavigatedTo(args);
    
    if (ViewModel != null)
    {
        await ViewModel.InitAsync(); // Await properly
    }
}

// SplashPage.xaml.cs  
protected override async void OnAppearing()
{
    base.OnAppearing();
    await LoadDataAsync(); // Extract to awaitable method
}

private async Task LoadDataAsync()
{
    await Task.Delay(4000);
    // ... rest of logic
}
```

**Estimated Effort:** 0.5 day

---

### MR-004: Fire-and-Forget Async Patterns [HIGH]
**Current State:**
- `_ = value.RankGiftsAsync();` in ResultsViewModel.cs:28-34
- `_ = LoadUserGiftResultAsync(value);` in ResultsViewModel.cs:28-34
- `_ = TranslationService.SetLanguageByCodeAsync()` in SettingsViewModel.cs:256-266
- `_ = LoadGiftDetailsAsync(value);` in GiftDescriptionViewModel.cs:26-33
- `_ = result.RankGiftsAsync();` in EmailService.cs:90-94

**Why It's a Blocker:**
- Unobserved exceptions silently fail
- Race conditions with UI state
- No error handling
- .NET 10 optimizations may change execution order/timing

**Impact on Migration:**
- MEDIUM — Will get worse under optimization but not immediate blocker
- Recommend fixing PRE-migration for stability

**Remediation:** (PRE-MIGRATION)
Replace with awaited calls or proper background task management:
```csharp
// Bad
_ = value.RankGiftsAsync();

// Good  
await RunWithLoading(async () => await value.RankGiftsAsync());
```

**Estimated Effort:** 1 day

---

### MR-005: Task.Run State Mutation [MEDIUM]
**Current State:**
- `Models/SurveyResult.cs:35-91` — `RankGiftsAsync()` uses `Task.Run()` and mutates `Scores` collection
- No synchronization
- Shared state accessed from background thread

**Why It's an Issue:**
- Race condition with UI access to `Scores`
- No thread safety guarantees
- May fail intermittently under optimization

**Impact on Migration:**
- MEDIUM — Should fix but not blocking migration
- Can be addressed DURING or POST migration

**Remediation:** (DURING-MIGRATION)
Option 1: Remove Task.Run (ranking is fast, run on calling thread)
Option 2: Copy state before background work, update atomically

**Estimated Effort:** 1 day

---

## Package Compatibility Matrix

| Package | Current | .NET 10 Compatible | Upgrade Required | Risk Level |
|---------|---------|-------------------|------------------|------------|
| Microsoft.Maui.Controls | 9.0.81 | Yes (→ 10.x) | **Yes** | MEDIUM |
| CommunityToolkit.Maui | 12.1.0 | Yes | Minor update | LOW |
| CommunityToolkit.Mvvm | 8.4.0 | Yes | No | LOW |
| sqlite-net-pcl | 1.7.335 | Yes | No | LOW |
| SQLitePCLRaw.bundle_e_sqlite3 | 2.1.11 | Yes | Possibly | LOW |
| Microsoft.ApplicationInsights | 2.23.0 | TBD | Likely | MEDIUM |
| Microsoft.Extensions.Http | 9.0.6 | Yes (→ 10.x) | **Yes** | LOW |
| Microsoft.Extensions.Configuration.Json | 9.0.6 | Yes (→ 10.x) | **Yes** | LOW |
| Microsoft.Extensions.Logging.Debug | 9.0.6 | Yes (→ 10.x) | **Yes** | LOW |
| Newtonsoft.Json | 13.0.3 | Yes (not used) | **Remove** | LOW |

### Notes:
- **MAUI 10.x:** Major version bump expected, review migration guide when available
- **Microsoft.Extensions.*:** Will update to 10.0.x series automatically with .NET 10
- **ApplicationInsights:** Check compatibility or replace with MAUI-native telemetry
- **Newtonsoft.Json:** Currently installed but NOT used — safe to remove

---

## Migration Risk Register

See `docs/migration-risk-register.md` for complete risk tracking.

**Summary:**
- **8 migration risks** identified
- **3 CRITICAL** blockers (MR-001, MR-002, MR-003)
- **2 HIGH** priority (MR-004, MR-005)
- **3 MEDIUM/LOW** (MR-006, MR-007, MR-008)

---

## Sequencing Recommendation

### Phase 1: PRE-MIGRATION Stabilization (7-8 days)
**Must be completed BEFORE .NET 10 migration**

| Order | Task | Effort | Blocker |
|-------|------|--------|---------|
| 1 | Add trimming configuration | 0.5 day | ✅ Yes |
| 1 | Fix async void lifecycle | 0.5 day | ✅ Yes |
| 2 | Implement JSON source generation | 1 day | ✅ Yes |
| 2 | Fix NavigationService MainThread safety | 0.5 day | No |
| 3 | Fix fire-and-forget patterns | 1 day | Recommended |
| 4 | Fix BasePage/BaseViewModel lifecycle | 2 days | Recommended |
| 5 | Add critical business logic tests | 1.5 days | Recommended |
| 6 | Release build device testing | 1 day | ✅ Yes |

**Why PRE-migration:**
- These are correctness bugs regardless of .NET version
- .NET 10 will make them WORSE, not better
- Debugging on .NET 9 is easier than .NET 10 (more mature tooling)
- Establishes stable baseline for migration

### Phase 2: DURING-MIGRATION (1-2 days)
**Execute alongside .NET 10 upgrade**

1. Update TFM to `net10.0-android;net10.0-ios`
2. Update MAUI to 10.x (exact version TBD)
3. Update Microsoft.Extensions.* to 10.x
4. Remove Newtonsoft.Json package
5. Fix SurveyResult thread safety (if not done pre-migration)
6. Test Release builds on both platforms
7. Regression testing

### Phase 3: POST-MIGRATION (3-6 days)
**Architectural improvements after stability achieved**

1. Replace Service Locator with proper DI
2. Convert ViewModels to Transient lifetime
3. Split DatabaseService responsibilities
4. Expand test coverage
5. Performance optimization

---

## Platform-Specific Concerns

### iOS
- **Current:** iOS 15.0+ (`SpiritualGiftsSurvey.csproj:34`)
- **MAUI 10:** Verify minimum iOS version (likely 15.0 or 15.2)
- **AOT:** Most sensitive to reflection/trimming issues
- **Testing:** MUST test on physical device in Release mode

### Android
- **Current:** API 23+ (Android 6.0)
- **MAUI 10:** Verify minimum API level (may bump to 24+)
- **Linker:** Aggressive but more forgiving than iOS AOT
- **Testing:** Test on physical device in Release mode

---

## Testing Strategy

### Pre-Migration Testing
1. **Release Build Smoke Test** (after trimming config + JSON source-gen)
   - iOS: Physical device, Release build
   - Android: Physical device, Release build
   - Test: Welcome → Survey → Results → Email → Settings

2. **Critical Path Validation**
   - Database sync from Firebase
   - Survey completion (all 250+ questions)
   - Ranking algorithm
   - Email generation
   - Language switching

3. **Regression Testing**
   - Verify no behavior changes from PRE-migration fixes

### During-Migration Testing
1. **Framework Upgrade Validation**
   - Clean rebuild
   - Release builds on both platforms
   - Same critical path tests as pre-migration

2. **Performance Baseline**
   - Startup time
   - Survey navigation performance
   - Database load time

### Post-Migration Testing
1. **Extended Soak Testing**
   - Multiple survey completions
   - Language switching stress test
   - Memory leak detection

---

## Remaining Questions

1. **MAUI 10 Release Timeline**
   - When is MAUI 10 stable release?
   - What's the migration guide?
   - Any breaking changes in Shell navigation?

2. **ApplicationInsights Compatibility**
   - Is Microsoft.ApplicationInsights 2.23.0 compatible with .NET 10?
   - Should we migrate to MAUI-native telemetry?

3. **Minimum Platform Versions**
   - Does MAUI 10 bump iOS minimum to 15.2 or 16.0?
   - Does MAUI 10 bump Android minimum to API 24?

4. **Trimming Behavior Changes**
   - How does .NET 10 default trimming differ from .NET 9?
   - Are there new trimming analyzers?

---

## Decision: DO NOT MIGRATE YET

**Verdict:** Complete all PRE-MIGRATION work before attempting .NET 10 upgrade.

**Rationale:**
1. Three CRITICAL blockers will worsen under .NET 10
2. Debugging .NET 9 issues on .NET 9 is easier than debugging .NET 9 legacy issues on .NET 10
3. Establishes stable, tested baseline
4. Reduces migration risk
5. Allows focused validation of .NET 10-specific changes

**Timeline:**
- **PRE-MIGRATION:** 7-8 days
- **MIGRATION:** 1-2 days  
- **POST-MIGRATION:** 3-6 days
- **Total:** 11-16 days to production-ready on .NET 10

---

## Next Steps

1. Review and approve this assessment
2. Execute PRE-MIGRATION roadmap (see `docs/implementation-roadmap.md`)
3. Complete Release build validation
4. Monitor MAUI 10 release announcements
5. Plan migration window after PRE-migration complete

---

**Document Owner:** Architecture & Modernization Initiative  
**Last Updated:** January 2025  
**Next Review:** After PRE-MIGRATION phase completion
