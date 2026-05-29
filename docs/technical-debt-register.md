# Technical Debt Register

**Project:** Spiritual Gifts Survey (MAUI)  
**Last Updated:** 2026-05-28  
**Owner:** Development Team

---

## Overview

This document tracks known technical debt, architectural smells, and improvement opportunities. Items are categorized by severity and impact on production readiness.

**Status Legend:**
- 🔴 **CRITICAL** — Blocks production release
- 🟠 **HIGH** — Should fix before release
- 🟡 **MEDIUM** — Recommended improvement
- 🟢 **LOW** — Nice-to-have, future enhancement

---

## Critical Debt (Blocks Release)

### 🔴 TD-001: No Trimming/AOT Configuration

**Category:** Configuration  
**Severity:** CRITICAL  
**Impact:** App crashes in Release builds  
**Effort:** 1 day

**Description:**  
Project has no `PublishTrimmed`, `TrimMode`, or `ILLink` settings. Release builds use aggressive default trimming which removes types used via reflection.

**Consequences:**
- JSON deserialization fails at runtime
- App crashes on startup or during database refresh
- Silent data corruption possible

**Remediation:**
- Add trimming configuration to `.csproj` (see `docs/release-build-findings.md` CRITICAL-1)
- OR: Switch to source-generated JSON serialization (recommended)

**Files Affected:**
- `SpiritualGiftsSurvey.csproj`

**Related Issues:** TD-002

---

### 🔴 TD-002: Reflection-Based JSON Serialization

**Category:** Serialization  
**Severity:** CRITICAL  
**Impact:** Database sync fails in Release  
**Effort:** 2 days

**Description:**  
`UrlService` and `EmailService` use `System.Text.Json` without source generation. Relies on reflection to serialize/deserialize `RootModel` and `List<string>`.

**Consequences:**
- Fails under trimming (iOS AOT, Android linker)
- Firebase database refresh returns null or throws `JsonException`
- No questions load, app unusable

**Remediation:**
```csharp
// Add source generator
[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(RootModel))]
[JsonSerializable(typeof(List<string>))]
internal partial class AppJsonContext : JsonSerializerContext { }

// Use in UrlService
var model = JsonSerializer.Deserialize(json, AppJsonContext.Default.RootModel);
```

**Files Affected:**
- `Services/UrlService.cs`
- `Services/EmailService.cs`
- All `Models/*.cs` (must be compatible with source gen)

**Related Issues:** TD-001

---

### 🔴 TD-003: Async Void in Page Lifecycle

**Category:** Async Patterns  
**Severity:** CRITICAL  
**Impact:** Unobservable exceptions, navigation race  
**Effort:** 4 hours

**Description:**  
`SplashPage.OnAppearing()` uses `async void`, violating async best practices.

**Consequences:**
- Exceptions after first `await` are swallowed
- App crashes without stack trace
- Navigation can occur after page disposed
- Duplicate navigation if page reappears

**Remediation:**
- Convert to `async Task` fire-and-forget with proper exception handling
- Add cancellation token to prevent navigation after disposal
- See `docs/release-build-findings.md` CRITICAL-3 for full code

**Files Affected:**
- `Views/Splash/SplashPage.xaml.cs`

**Related Issues:** TD-007

---

## High Priority Debt (Should Fix)

### 🟠 TD-004: Fire-and-Forget Ranking

**Category:** Async Patterns  
**Severity:** HIGH  
**Impact:** Race condition in results display  
**Effort:** 2 hours

**Description:**  
`ResultsViewModel` and `EmailService` fire-and-forget `RankGiftsAsync()` then immediately read results. Ranking may not complete in time.

**Consequences:**
- UI shows unranked gifts
- Email contains incorrect data
- Timing-dependent (more likely in Release)

**Remediation:**
```csharp
// BEFORE
if (!value.IsRanked)
{
    _ = value.RankGiftsAsync();  // Fire-and-forget
}
_ = LoadUserGiftResultAsync(value);  // Race!

// AFTER
if (!value.IsRanked)
{
    await value.RankGiftsAsync();  // AWAIT
}
await LoadUserGiftResultAsync(value);
```

**Files Affected:**
- `Views/Results/ResultsViewModel.cs:28-34`
- `Services/EmailService.cs:91-94`
- `Views/GiftDescription/GiftDescriptionViewModel.cs:32`

**Related Issues:** TD-010

---

### 🟠 TD-005: Page Lifecycle Double-Init

**Category:** Lifecycle  
**Severity:** HIGH  
**Impact:** Concurrent operations, state corruption  
**Effort:** 3 hours

**Description:**  
`BasePage` calls both `RefreshViewModel()` (OnAppearing) and `InitAsync()` (OnNavigatedTo) without awaiting `InitAsync()`. Both can run concurrently.

**Consequences:**
- Overlapping async operations
- State corruption if `RefreshViewModel()` clears while `InitAsync()` loads
- Timing-dependent bugs

**Remediation:**
- Call `InitAsync()` properly with `await` wrapper
- Ensure mutual exclusion or sequential execution
- See `docs/release-build-findings.md` HIGH-3

**Files Affected:**
- `Views/Shared/BasePage.cs:15-27`

**Related Issues:** TD-006

---

### 🟠 TD-006: Language Change Fire-and-Forget

**Category:** Async Patterns  
**Severity:** HIGH  
**Impact:** Stale UI, concurrent init calls  
**Effort:** 1 hour

**Description:**  
`SettingsViewModel.OnSelectedLanguageChanged` fires-and-forgets `SetLanguageByCodeAsync()`, then reads state before init completes.

**Consequences:**
- UI shows old language strings
- All ViewModels re-init concurrently (state corruption)
- Unobserved exceptions

**Remediation:**
```csharp
partial void OnSelectedLanguageChanged(LanguageOption? value)
{
    if (value == null) return;
    
    _ = RunWithLoading(async () =>
    {
        await TranslationService.SetLanguageByCodeAsync(value.CodeOption);
        LanguageOptions = TranslationService.GetLanguageOptions();
        ShowLanguagePicker = false;
    });
}
```

**Files Affected:**
- `Views/Settings/SettingsViewModel.cs:261`

**Related Issues:** TD-007

---

### 🟠 TD-007: BaseViewModel Language Message Handler

**Category:** Messaging  
**Severity:** HIGH  
**Impact:** Unobserved exceptions, state corruption  
**Effort:** 1 hour

**Description:**  
`WeakReferenceMessenger` handler in `BaseViewModel` calls `InitAsync()` without awaiting.

**Consequences:**
- Exceptions swallowed
- Concurrent `InitAsync()` calls if rapid language switching
- State corruption

**Remediation:**
```csharp
WeakReferenceMessenger.Default.Register<LanguageChangedMessage>(this, (r, m) =>
{
    _ = HandleLanguageChangeAsync();
});

private async Task HandleLanguageChangeAsync()
{
    try { await InitAsync(); }
    catch (Exception ex) { Debug.WriteLine($"Init failed: {ex}"); }
}
```

**Files Affected:**
- `Views/Shared/BaseViewModel.cs:37-40`

**Related Issues:** TD-006

---

### 🟠 TD-008: Task.Run Mutating Shared State

**Category:** Threading  
**Severity:** HIGH  
**Impact:** Race condition, collection modification  
**Effort:** 2 hours

**Description:**  
`SurveyResult.RankGiftsAsync()` uses `Task.Run()` to mutate `Scores` collection on background thread without synchronization.

**Consequences:**
- Race if UI reads `Scores` during ranking
- `InvalidOperationException: Collection was modified`
- No thread safety

**Remediation:**
- Work on a copy, assign atomically
- Or: Use lock for synchronization
- See `docs/release-build-findings.md` HIGH-5

**Files Affected:**
- `Models/SurveyResult.cs:35-91`

**Related Issues:** TD-004

---

## Medium Priority Debt (Recommended)

### 🟡 TD-009: Service Locator Anti-Pattern

**Category:** Architecture  
**Severity:** MEDIUM  
**Impact:** Hard to test, hidden dependencies  
**Effort:** 5 days (major refactor)

**Description:**  
`IAggregatedServices` acts as a service locator, hiding dependencies and complicating testing.

**Consequences:**
- ViewModels appear to have zero dependencies
- Dependency graph invisible
- Hard to mock services for unit tests
- Violates Dependency Inversion Principle

**Remediation:**
- **Option 1:** Direct constructor injection (recommended but tedious)
  ```csharp
  public SurveyViewModel(
      IDatabaseService db,
      ITranslationService trans,
      INavigationService nav,
      IPreferences prefs)
  ```
- **Option 2:** Keep pattern but add `IServiceProvider` wrapper for testability
- **Option 3:** Use facade pattern with clear interface

**Files Affected:**
- `Services/AggregatedServices.cs`
- `Views/Shared/BaseViewModel.cs`
- All ViewModels (14 files)

**Related Issues:** None (architectural smell)

---

### 🟡 TD-010: Singleton ViewModels

**Category:** DI Configuration  
**Severity:** MEDIUM  
**Impact:** State persistence, hard to test  
**Effort:** 2 hours + testing

**Description:**  
All ViewModels registered as `Singleton` in `MauiProgram.cs`. State persists between navigations.

**Consequences:**
- Stale data if user navigates back
- Hard to isolate for unit tests
- Memory usage increases over time

**Remediation:**
- Change ViewModels to `Transient` or `Scoped`
- Ensure services can handle multiple instances
- Test state cleanup between navigations

**Files Affected:**
- `MauiProgram.cs:87-112`

**Related Issues:** TD-009

---

### 🟡 TD-011: Mixed Preferences.Default vs IPreferences

**Category:** Inconsistency  
**Severity:** MEDIUM  
**Impact:** Hard to test, timing issues  
**Effort:** 1 hour

**Description:**  
`TranslationService` uses `Preferences.Default` directly in properties while also injecting `IPreferences`.

**Consequences:**
- Inconsistent pattern
- Hard to mock for unit tests
- Can cause initialization timing issues

**Remediation:**
```csharp
// BEFORE
public string CurrentLanguageCode
{
    get => Preferences.Default.Get(LangCodeKey, DefaultLangCode);
    set => Preferences.Default.Set(LangCodeKey, value);
}

// AFTER
public string CurrentLanguageCode
{
    get => _prefs.Get(LangCodeKey, DefaultLangCode);
    set => _prefs.Set(LangCodeKey, value);
}
```

**Files Affected:**
- `Services/TranslationService.cs:53, 58, 66, 71`

**Related Issues:** None

---

### 🟡 TD-012: NavigationService Missing Main Thread Check

**Category:** Threading  
**Severity:** MEDIUM  
**Impact:** Crash if called from background  
**Effort:** 1 hour

**Description:**  
`NavigationService` calls `Shell.Current.GoToAsync()` without main thread dispatch or null check.

**Consequences:**
- Crash if called from background thread
- Crash if Shell not initialized

**Remediation:**
```csharp
public async Task<bool> NavigateAsync(string route)
{
    if (!MainThread.IsMainThread)
        return await MainThread.InvokeOnMainThreadAsync(() => NavigateAsync(route));
    
    if (Shell.Current == null)
    {
        Debug.WriteLine("Shell not ready");
        return false;
    }
    
    await Shell.Current.GoToAsync($"///{route}");
    return true;
}
```

**Files Affected:**
- `Services/NavigationService.cs:15-69`

**Related Issues:** None

---

### 🟡 TD-013: TranslateExtension Reflection

**Category:** Localization  
**Severity:** MEDIUM  
**Impact:** Resources trimmed in Release  
**Effort:** 2 hours

**Description:**  
`TranslateExtension` uses `typeof().GetTypeInfo().Assembly` to locate resources. Assembly can be trimmed.

**Consequences:**
- Resource lookup fails
- XAML shows translation keys instead of values
- Silent failure (fallback to key)

**Remediation:**
- Use direct `ResourceManager` creation
- OR: Add `[DynamicallyAccessedMembers]` attribute
- OR: Disable trimming for resources

**Files Affected:**
- `il8n/TranslateExtension.cs:13`

**Related Issues:** TD-001, TD-002

---

### 🟡 TD-014: Control Event Subscription Leaks

**Category:** Memory Management  
**Severity:** MEDIUM  
**Impact:** Memory leaks if controls reused  
**Effort:** 30 minutes per control

**Description:**  
Custom controls (`GiftScoreView`, etc.) subscribe to events (`SizeChanged`, `Tapped`) without unsubscribing.

**Consequences:**
- Memory leaks if controls reused
- Closures capture `BindingContext` references
- Handlers execute on disposed objects

**Remediation:**
```csharp
private TapGestureRecognizer? _tapGesture;

protected override void OnHandlerChanged()
{
    base.OnHandlerChanged();
    if (Handler != null)
    {
        _tapGesture = new TapGestureRecognizer();
        _tapGesture.Tapped += OnTapped;
        GestureRecognizers.Add(_tapGesture);
    }
}

protected override void OnHandlerChanging(HandlerChangingEventArgs args)
{
    base.OnHandlerChanging(args);
    if (args.OldHandler != null && _tapGesture != null)
    {
        _tapGesture.Tapped -= OnTapped;
    }
}
```

**Files Affected:**
- `Views/Controls/GiftScoreView.xaml.cs`
- `Views/Controls/QuestionView.xaml.cs`
- Others (grep for event subscriptions)

**Related Issues:** None

---

### 🟡 TD-015: No ConfigureAwait Usage

**Category:** Async Patterns  
**Severity:** MEDIUM  
**Impact:** Unnecessary context capture overhead  
**Effort:** 2 hours (solution-wide)

**Description:**  
No `ConfigureAwait(false)` usage anywhere in the codebase.

**Consequences:**
- Unnecessary UI context capture in background work
- Minor performance overhead
- In MAUI generally safe (UI context needed) but inefficient

**Remediation:**
- Add `.ConfigureAwait(false)` to non-UI async calls:
  - Database operations
  - HTTP requests
  - File I/O
  - JSON serialization

**Files Affected:**
- Solution-wide (all `await` calls)

**Related Issues:** None

---

## Low Priority Debt (Nice-to-Have)

### 🟢 TD-016: WeakReferenceMessenger No Explicit Unregister

**Category:** Messaging  
**Severity:** LOW  
**Impact:** Callbacks on disposed objects  
**Effort:** 30 minutes per ViewModel

**Description:**  
ViewModels register message handlers but never explicitly unregister.

**Consequences:**
- WeakReference prevents hard leaks
- But callbacks can still execute on disposed objects
- Difficult to debug lifecycle issues

**Remediation:**
```csharp
public override void Dispose()
{
    WeakReferenceMessenger.Default.Unregister<LanguageChangedMessage>(this);
    base.Dispose();
}
```

**Files Affected:**
- `Views/Shared/BaseViewModel.cs`
- All ViewModels (14 files)

**Related Issues:** None

---

### 🟢 TD-017: QueryProperty Timing Fragility

**Category:** Navigation  
**Severity:** LOW  
**Impact:** Fragile initialization order  
**Effort:** 3 hours

**Description:**  
`QueryProperty` parameters may arrive before or after `InitAsync()` completes. Currently handled implicitly.

**Consequences:**
- Fragile timing assumptions
- Partial state if parameter arrives late
- Repeated loads if parameter arrives early

**Remediation:**
- Standardize pattern: always trigger load in `OnPropertyChanged`
- OR: Wait for both `InitAsync` and property in coordinator method

**Files Affected:**
- `Views/Results/ResultsViewModel.cs`
- `Views/Send/SendViewModel.cs`
- `Views/GiftDescription/GiftDescriptionViewModel.cs`

**Related Issues:** None

---

### 🟢 TD-018: Mixed Sync/Async SQLite Access

**Category:** Data Access  
**Severity:** LOW  
**Impact:** Inconsistent patterns, minor perf  
**Effort:** 2 days (refactor)

**Description:**  
`DatabaseService` uses both `SQLiteConnection` (sync) and `SQLiteAsyncConnection` (async) inconsistently.

**Consequences:**
- Inconsistent patterns hard to follow
- Minor performance differences
- Some methods unnecessarily block

**Remediation:**
- Standardize on async everywhere
- Remove sync `GetAppStrings`, `GetLanguageOptions`
- Use `await` consistently

**Files Affected:**
- `Services/DatabaseService.cs` (entire file)

**Related Issues:** None

---

### 🟢 TD-019: DatabaseService Violates SRP

**Category:** Architecture  
**Severity:** LOW  
**Impact:** Hard to test, God object  
**Effort:** 3 days (refactor)

**Description:**  
`DatabaseService` acts as repository + sync engine + cache manager + schema migrator.

**Consequences:**
- Single Responsibility Principle violated
- Hard to test individual concerns
- Hard to replace parts (e.g., swap Firebase for REST API)

**Remediation:**
- Split into:
  - `IQuestionRepository`
  - `IGiftDescriptionRepository`
  - `ISurveyResultRepository`
  - `IDatabaseMigrationService`
  - `IFirebaseSyncService`

**Files Affected:**
- `Services/DatabaseService.cs`
- All service consumers

**Related Issues:** TD-009

---

### 🟢 TD-020: No Structured Logging

**Category:** Observability  
**Severity:** LOW  
**Impact:** Hard to debug production issues  
**Effort:** 1 day

**Description:**  
Uses `Debug.WriteLine()` throughout. No structured logging, no log levels, no crash reporting.

**Consequences:**
- Can't diagnose production issues
- No telemetry on errors
- Debug output not available in Release

**Remediation:**
- Add `Microsoft.Extensions.Logging`
- OR: Use Serilog or similar
- Integrate with AppInsights
- Add crash reporting (e.g., AppCenter)

**Files Affected:**
- Solution-wide (all `Debug.WriteLine` calls)

**Related Issues:** None

---

### 🟢 TD-021: Splash Screen 4-Second Hardcoded Delay

**Category:** UX  
**Severity:** LOW  
**Impact:** Slow startup perception  
**Effort:** 1 hour

**Description:**  
`SplashPage` uses `Task.Delay(4000)` hardcoded delay before navigation.

**Consequences:**
- Feels slow on fast devices
- Users may tap multiple times
- Doesn't reflect actual loading state

**Remediation:**
- Show loading indicator
- Navigate when database check completes
- OR: Reduce delay to 2 seconds

**Files Affected:**
- `Views/Splash/SplashPage.xaml.cs:24`

**Related Issues:** TD-003

---

### 🟢 TD-022: No Delta Sync for Firebase

**Category:** Performance  
**Severity:** LOW  
**Impact:** Bandwidth waste, slow refresh  
**Effort:** 5 days (feature work)

**Description:**  
Database refresh downloads entire Firebase JSON (~500KB-1MB) even if only questions changed.

**Consequences:**
- Wastes bandwidth
- Slow on poor connections
- Full table rebuild every time

**Remediation:**
- Implement delta sync (last modified timestamp)
- Download only changed entities
- Incremental updates instead of full rebuild

**Files Affected:**
- `Services/UrlService.cs`
- `Services/DatabaseService.cs`

**Related Issues:** None

---

### 🟢 TD-023: No User Result Cleanup

**Category:** Storage  
**Severity:** LOW  
**Impact:** Unbounded storage growth  
**Effort:** 4 hours

**Description:**  
User survey results stored indefinitely. No archival or cleanup strategy.

**Consequences:**
- Database size grows unbounded
- Old results never deleted
- Privacy concern (data retention)

**Remediation:**
- Add "Clear History" feature
- OR: Auto-delete results older than X days
- OR: Limit to N most recent results

**Files Affected:**
- `Services/DatabaseService.cs`
- `Views/Settings/SettingsPage.xaml` (add UI)

**Related Issues:** None

---

### 🟢 TD-024: iOS Status Bar Hiding Uses Deprecated API

**Category:** Platform Code  
**Severity:** LOW  
**Impact:** Warning, may break in future iOS  
**Effort:** 30 minutes

**Description:**  
`UIApplication.SharedApplication.StatusBarHidden = true` is deprecated.

**Consequences:**
- Compiler warning
- May not work in future iOS versions

**Remediation:**
```csharp
// Use UIViewController preference instead
public override bool PrefersStatusBarHidden => true;
```

**Files Affected:**
- `Platforms/iOS/AppDelegate.cs:14`

**Related Issues:** None

---

## Summary Statistics

| Severity | Count | Estimated Effort |
|----------|-------|------------------|
| 🔴 Critical | 3 | 3.5 days |
| 🟠 High | 5 | 1.5 days |
| 🟡 Medium | 7 | 5 days |
| 🟢 Low | 9 | 10 days |
| **Total** | **24** | **20 days** |

### Production Release Blockers

**Must Fix Before Release:**
- TD-001: Trimming configuration (1 day)
- TD-002: JSON source generation (2 days)
- TD-003: Async void fix (4 hours)

**Total Critical Path:** 3.5 days

**Should Fix Before Release:**
- TD-004 through TD-008 (High priority items)

**Total Recommended Path:** 5 days

---

## Prioritization Matrix

### Impact vs Effort

```
HIGH IMPACT, LOW EFFORT (Do First)
├─ TD-003: Async void fix (4h)
├─ TD-004: Fire-and-forget ranking (2h)
├─ TD-006: Language change fix (1h)
├─ TD-007: Message handler fix (1h)
└─ TD-011: Preferences consistency (1h)

HIGH IMPACT, HIGH EFFORT (Do After Critical)
├─ TD-001: Trimming config (1d)
├─ TD-002: JSON source gen (2d)
└─ TD-005: Lifecycle fix (3h)

LOW IMPACT, LOW EFFORT (Quick Wins)
├─ TD-012: Navigation thread safety (1h)
├─ TD-016: Message unregister (30m)
└─ TD-024: iOS status bar (30m)

LOW IMPACT, HIGH EFFORT (Defer)
├─ TD-009: Service locator refactor (5d)
├─ TD-019: DatabaseService SRP (3d)
└─ TD-022: Delta sync (5d)
```

---

## Recommended Fix Order

### Sprint 1: Production Blockers (Week 1)
1. TD-001: Add trimming config
2. TD-002: JSON source generation
3. TD-003: Fix async void
4. TD-004: Fix fire-and-forget ranking
5. TD-006: Fix language change
6. TD-007: Fix message handler

**Deliverable:** Release-ready build

### Sprint 2: Stability & Quality (Week 2)
7. TD-005: Fix lifecycle race
8. TD-008: Fix ranking thread safety
9. TD-010: Change ViewModels to Transient
10. TD-011: Fix Preferences consistency
11. TD-012: Add navigation thread safety
12. Add unit tests (see `docs/testing-assessment.md`)

**Deliverable:** Stable, tested app

### Sprint 3: Code Quality (Week 3+)
13. TD-009: Refactor service locator (if testability poor)
14. TD-013: Fix TranslateExtension
15. TD-014: Fix control event leaks
16. TD-015: Add ConfigureAwait
17. TD-020: Add structured logging

**Deliverable:** Maintainable codebase

### Backlog: Future Improvements
- TD-016 through TD-024 (low priority items)

---

## Change Log

| Date | Author | Changes |
|------|--------|---------|
| 2026-05-28 | Senior Staff Engineer | Initial technical debt assessment |

---

## Related Documentation

- **[docs/release-build-findings.md](release-build-findings.md)** — Detailed analysis of release-specific issues
- **[docs/testing-assessment.md](testing-assessment.md)** — Test coverage gaps
- **[docs/architecture-overview.md](architecture-overview.md)** — System architecture and patterns
