# Implementation Roadmap — Spiritual Gifts Survey Modernization

**Status:** PLANNING COMPLETE — Ready for Execution  
**Last Updated:** January 2025  
**Total Estimated Effort:** 11-16 days to production-ready on .NET 10  
**Current State:** .NET 9 MAUI with 3 CRITICAL blockers

---

## Executive Summary

This roadmap sequences all stabilization, migration, and modernization work to minimize risk and avoid duplicated effort.

**Key Decisions:**
1. **Fix BEFORE Migrating** — Resolve CRITICAL blockers on .NET 9 before attempting .NET 10
2. **Test Between Phases** — Validate each phase before proceeding
3. **Sequence Dependencies** — Async fixes before lifecycle fixes before testing
4. **Physical Device Testing** — Required for Release validation

**Timeline:**
- **Phase 1 (PRE-MIGRATION):** 7-8 days — Stabilization on .NET 9
- **Phase 2 (DURING-MIGRATION):** 1-2 days — .NET 10 upgrade
- **Phase 3 (POST-MIGRATION):** 3-6 days — Architectural improvements

---

## Phase Definitions

### PRE-MIGRATION
**Goal:** Achieve production-ready stability on .NET 9  
**Why:** Fix correctness bugs before framework upgrade  
**Exit Criteria:** All CRITICAL/HIGH issues resolved, Release builds validated on devices

### DURING-MIGRATION
**Goal:** Upgrade to .NET 10 with minimal disruption  
**Why:** Framework upgrade with stable baseline  
**Exit Criteria:** .NET 10 builds successfully, regression tests pass

### POST-MIGRATION
**Goal:** Architectural cleanup and modernization  
**Why:** Improve maintainability after stability achieved  
**Exit Criteria:** Code quality improvements, no regressions

---

## PHASE 1: PRE-MIGRATION Stabilization (7-8 days)

### Week 1, Day 1 — Parallel Track A & B

#### Track A: Trimming Configuration (0.5 day)
**Task ID:** task-001  
**Priority:** CRITICAL  
**Dependencies:** None  
**Owner:** Platform Engineer

**Deliverables:**
1. Update SpiritualGiftsSurvey.csproj:
   - Add PublishTrimmed=true
   - Add TrimMode=partial
   - Add platform-specific linker settings
   - Enable trimming analyzers

2. Build Release configurations:
   `ash
   dotnet clean
   dotnet build -c Release -f net9.0-android
   dotnet build -c Release -f net9.0-ios
   `

3. Address trimming analyzer warnings (if any)

**Validation:**
- [ ] Release build succeeds for Android
- [ ] Release build succeeds for iOS
- [ ] No critical trimming warnings
- [ ] App still runs (basic smoke test)

**Blockers:** None  
**Estimated Effort:** 4 hours

---

#### Track B: Fix Async Void (0.5 day)
**Task ID:** task-003  
**Priority:** CRITICAL  
**Dependencies:** None  
**Owner:** Backend Engineer

**Files to Modify:**
- Views/Splash/SplashPage.xaml.cs
- Views/Shared/BasePage.cs (if needed)

**Changes:**
`csharp
// SplashPage.xaml.cs
protected override async void OnAppearing()
{
    base.OnAppearing();
    
    try
    {
        await LoadDataAsync();
    }
    catch (Exception ex)
    {
        Analytics.TrackEvent("SplashLoadFailure", 
            new Dictionary<string, string> { { "Error", ex.Message } });
        // Navigate to error page or retry
    }
}

private async Task LoadDataAsync()
{
    await Task.Delay(4000);
    await ViewModel.LoadFirebaseDataAsync();
    await Shell.Current.GoToAsync($"///{Routes.WelcomePage}");
}
`

**Validation:**
- [ ] App launches successfully
- [ ] 4-second delay still works
- [ ] Navigation to WelcomePage works
- [ ] No unobserved exceptions

**Blockers:** None  
**Estimated Effort:** 4 hours

---

### Week 1, Day 2 — Parallel Track C & D

#### Track C: JSON Source Generation (1 day)
**Task ID:** task-002  
**Priority:** CRITICAL  
**Dependencies:** task-001 (trimming config)  
**Owner:** Backend Engineer

**Files to Create:**
- Services/AppJsonContext.cs

**Files to Modify:**
- Services/UrlService.cs
- Services/EmailService.cs

**Implementation:**
`csharp
// Services/AppJsonContext.cs
using System.Text.Json.Serialization;

namespace SpiritualGiftsSurvey.Services;

[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    WriteIndented = false)]
[JsonSerializable(typeof(Models.RootModel))]
[JsonSerializable(typeof(List<string>))]
internal partial class AppJsonContext : JsonSerializerContext
{
}

// Services/UrlService.cs:52-56
// Replace:
var options = new JsonSerializerOptions { ... };
var rootModel = JsonSerializer.Deserialize<RootModel>(json, options);

// With:
var rootModel = JsonSerializer.Deserialize(json, AppJsonContext.Default.RootModel);

// Services/EmailService.cs:126-153
// Replace all Serialize/Deserialize calls with source-gen overloads
`

**Validation:**
- [ ] Database sync from Firebase works
- [ ] RootModel deserializes correctly
- [ ] Email generation includes all fields
- [ ] Release build succeeds
- [ ] iOS device test (Release) succeeds
- [ ] Android device test (Release) succeeds

**Blockers:** Requires task-001 (trimming config) complete  
**Estimated Effort:** 8 hours

---

#### Track D: NavigationService MainThread Safety (0.5 day)
**Task ID:** task-006  
**Priority:** MEDIUM  
**Dependencies:** None  
**Owner:** MAUI Engineer

**Files to Modify:**
- Services/NavigationService.cs

**Changes:**
`csharp
// Services/NavigationService.cs
public async Task NavigateAsync(string route, Dictionary<string, object>? parameters = null)
{
    await MainThread.InvokeOnMainThreadAsync(async () =>
    {
        try
        {
            if (parameters != null)
                await Shell.Current.GoToAsync(route, parameters);
            else
                await Shell.Current.GoToAsync(route);
        }
        catch (Exception ex)
        {
            Analytics.TrackEvent("NavigationFailure", 
                new Dictionary<string, string> 
                { 
                    { "Route", route },
                    { "Error", ex.Message } 
                });
            throw;
        }
    });
}
`

**Validation:**
- [ ] All navigation still works
- [ ] No threading exceptions
- [ ] Test navigation from background threads (if any)

**Blockers:** None  
**Estimated Effort:** 4 hours

---

### Week 1, Day 3 — Fire-and-Forget Fixes (1 day)
**Task ID:** task-004  
**Priority:** HIGH  
**Dependencies:** task-003 (async void fixed)  
**Owner:** Backend Engineer

**Files to Modify:**
- Views/Results/ResultsViewModel.cs
- Views/Settings/SettingsViewModel.cs
- Views/GiftDescription/GiftDescriptionViewModel.cs
- Services/EmailService.cs

**Pattern:**
`csharp
// ResultsViewModel.cs — Replace fire-and-forget
// Before:
public SurveyResult? UserGiftResult
{
    get => _userGiftResult;
    set
    {
        _userGiftResult = value;
        OnPropertyChanged();
        _ = value.RankGiftsAsync(); // ❌
        _ = LoadUserGiftResultAsync(value); // ❌
    }
}

// After:
[QueryProperty(nameof(UserGiftResult), "UserGiftResult")]
public SurveyResult? UserGiftResult
{
    get => _userGiftResult;
    set
    {
        _userGiftResult = value;
        OnPropertyChanged();
        
        if (value != null)
        {
            // Don't fire-and-forget; InitAsync will handle loading
            RequiresInitialization = true;
        }
    }
}

public override async Task InitCoreAsync(CancellationToken ct)
{
    if (UserGiftResult != null)
    {
        await RunWithLoading(async () =>
        {
            await UserGiftResult.RankGiftsAsync();
            await LoadUserGiftResultAsync(UserGiftResult);
        });
    }
}
`

Apply similar pattern to all fire-and-forget sites.

**Validation:**
- [ ] Survey completion → Results page works
- [ ] Language switching works
- [ ] Gift description navigation works
- [ ] Email generation works
- [ ] No unobserved exceptions

**Blockers:** Requires task-003 (async void fixed)  
**Estimated Effort:** 8 hours

---

### Week 1, Day 4-5 — Lifecycle Overhaul (2 days)
**Task ID:** task-005  
**Priority:** HIGH  
**Dependencies:** task-003 (async void), task-004 (fire-and-forget)  
**Owner:** MAUI Engineer + Backend Engineer

**Files to Modify:**
- Views/Shared/BasePage.cs
- Views/Shared/BaseViewModel.cs
- All derived ViewModels (8 files)

**Implementation:**
`csharp
// BasePage.cs
protected override async void OnNavigatedTo(NavigatedToEventArgs args)
{
    base.OnNavigatedTo(args);
    
    if (ViewModel != null)
    {
        try
        {
            // QueryProperty is already set by Shell
            await ViewModel.InitAsync();
        }
        catch (Exception ex)
        {
            Analytics.TrackEvent("PageInitFailure", 
                new Dictionary<string, string> 
                { 
                    { "Page", GetType().Name },
                    { "Error", ex.Message } 
                });
        }
    }
}

protected override void OnAppearing()
{
    base.OnAppearing();
    
    // RefreshViewModel only if already initialized
    if (ViewModel != null && !ViewModel.RequiresInitialization)
    {
        ViewModel.RefreshViewModel();
    }
}

protected override void OnDisappearing()
{
    ViewModel?.CancelOngoingOperations();
    base.OnDisappearing();
}

// BaseViewModel.cs
private CancellationTokenSource? _operationsCts;
protected bool RequiresInitialization { get; set; } = true;

public async Task InitAsync()
{
    if (!RequiresInitialization)
        return;

    _operationsCts?.Cancel();
    _operationsCts = new CancellationTokenSource();

    try
    {
        await InitCoreAsync(_operationsCts.Token);
        RequiresInitialization = false;
    }
    catch (OperationCanceledException)
    {
        // Expected during navigation away
    }
}

protected abstract Task InitCoreAsync(CancellationToken cancellationToken);

public void CancelOngoingOperations()
{
    _operationsCts?.Cancel();
}
`

**Update All ViewModels:**
- Convert InitAsync() → InitCoreAsync(CancellationToken)
- Add cancellation checks for long operations
- Test each ViewModel's initialization

**Validation:**
- [ ] All pages initialize correctly
- [ ] QueryProperty → InitAsync sequence works
- [ ] Back navigation works
- [ ] Language switching works
- [ ] No initialization races
- [ ] Cancellation works on navigation away

**Blockers:** Requires task-003 and task-004  
**Estimated Effort:** 16 hours

---

### Week 2, Day 1-1.5 — Unit Testing (1.5 days)
**Task ID:** task-007  
**Priority:** HIGH  
**Dependencies:** task-002 (JSON source-gen)  
**Owner:** Backend Engineer

**Test Coverage Required:**
1. **Ranking Algorithm** (5 tests)
   - Primary gifts (top 3)
   - Secondary gifts (next 3)
   - Tie handling
   - Edge cases (all same score, all different)
   - Empty scores

2. **JSON Serialization** (2 tests)
   - RootModel deserialization
   - List<string> serialization/deserialization

3. **Email Generation** (3 tests)
   - Email body includes all gifts
   - Email body includes descriptions
   - Email format is valid

4. **Database** (2 tests)
   - Translation loading
   - Question loading

**Test Project Setup:**
`xml
<!-- SpiritualGiftsSurvey.Tests/SpiritualGiftsSurvey.Tests.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="xunit" Version="2.6.0" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.0" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\SpiritualGiftsSurvey\SpiritualGiftsSurvey.csproj" />
  </ItemGroup>
</Project>
`

**Sample Test:**
`csharp
// Tests/SurveyResultTests.cs
public class SurveyResultTests
{
    [Fact]
    public async Task RankGiftsAsync_Top3Scores_MarkedAsPrimary()
    {
        // Arrange
        var result = new SurveyResult
        {
            Scores = new List<UserGiftScore>
            {
                new() { Gift = Gifts.Teaching, Score = 50, MaxScore = 50 },
                new() { Gift = Gifts.Faith, Score = 48, MaxScore = 50 },
                new() { Gift = Gifts.Mercy, Score = 46, MaxScore = 50 },
                new() { Gift = Gifts.Giving, Score = 40, MaxScore = 50 }
            }
        };

        // Act
        await result.RankGiftsAsync();

        // Assert
        Assert.True(result.IsRanked);
        Assert.Equal(3, result.Scores.Count(s => s.GiftRank == GiftRank.Primary));
    }
}
`

**Validation:**
- [ ] Minimum 10 tests written
- [ ] All tests pass
- [ ] Code coverage >= 60% on critical paths
- [ ] Tests run in CI (manual for now, automated later)

**Blockers:** Requires task-002 (JSON source-gen for serialization tests)  
**Estimated Effort:** 12 hours

---

### Week 2, Day 1.5-2.5 — Release Build Device Testing (1 day)
**Task ID:** task-008  
**Priority:** CRITICAL  
**Dependencies:** task-001 (trimming), task-002 (JSON), task-003 (async void)  
**Owner:** QA / Platform Engineer

**Testing Checklist:**

#### iOS Physical Device (Release Build)
- [ ] Build Release: dotnet build -c Release -f net9.0-ios
- [ ] Deploy to physical iOS device (NOT simulator)
- [ ] Launch app
- [ ] Database sync from Firebase (verify new install)
- [ ] Navigate: Welcome → Survey
- [ ] Complete full survey (all 250+ questions)
- [ ] Verify ranking algorithm (check top 3 gifts)
- [ ] View gift description
- [ ] Generate email
- [ ] Send email (test email client integration)
- [ ] Switch language: English → Arabic → English
- [ ] Verify RTL layout in Arabic
- [ ] Settings: clear database, restore
- [ ] Monitor console for errors/warnings
- [ ] Performance: startup time < 5 seconds

#### Android Physical Device (Release Build)
- [ ] Build Release: dotnet build -c Release -f net9.0-android
- [ ] Deploy to physical Android device (NOT emulator)
- [ ] Repeat all iOS tests above

#### Critical Validation
- [ ] No deserialization failures (check logs)
- [ ] No missing JSON properties
- [ ] No navigation crashes
- [ ] No unobserved exceptions
- [ ] Email body includes all ranked gifts
- [ ] Database sync works (check version number)

**Blockers:** Requires task-001, task-002, task-003  
**Estimated Effort:** 8 hours

---

### PRE-MIGRATION Phase Exit Criteria
- [ ] All CRITICAL tasks complete (task-001, task-002, task-003, task-008)
- [ ] All HIGH tasks complete (task-004, task-005, task-007)
- [ ] Release builds validated on iOS AND Android physical devices
- [ ] No known Release-only crashes
- [ ] Regression testing passed
- [ ] Team approval to proceed to migration

**Total PRE-MIGRATION Effort:** 7-8 days

---

## PHASE 2: DURING-MIGRATION (.NET 10 Upgrade) (1-2 days)

### Prerequisites
- [ ] .NET 10 stable release available
- [ ] MAUI 10 stable release available
- [ ] Migration guide reviewed
- [ ] PRE-MIGRATION phase 100% complete

---

### Day 1 — Framework Migration (1 day)
**Task ID:** task-009  
**Priority:** HIGH  
**Dependencies:** ALL PRE-MIGRATION tasks  
**Owner:** Platform Engineer

#### Step 1: Update .csproj (30 min)
`xml
<!-- Before -->
<TargetFrameworks>net9.0-android;net9.0-ios</TargetFrameworks>
<PackageReference Include="Microsoft.Maui.Controls" Version="9.0.81" />

<!-- After -->
<TargetFrameworks>net10.0-android;net10.0-ios</TargetFrameworks>
<PackageReference Include="Microsoft.Maui.Controls" Version="10.x.x" />
<PackageReference Include="Microsoft.Extensions.Http" Version="10.0.*" />
<PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="10.0.*" />
<PackageReference Include="Microsoft.Extensions.Logging.Debug" Version="10.0.*" />
`

#### Step 2: Update Workloads (15 min)
`ash
dotnet workload update
dotnet workload install maui
`

#### Step 3: Clean and Rebuild (15 min)
`ash
dotnet clean
rm -rf bin/ obj/
dotnet build -c Debug
`

#### Step 4: Address Breaking Changes (2 hours)
- Review MAUI 10 migration guide
- Fix any deprecated API usage
- Update namespace imports if changed

#### Step 5: Build Release Configurations (1 hour)
`ash
dotnet build -c Release -f net10.0-android
dotnet build -c Release -f net10.0-ios
`

#### Step 6: Regression Testing (3 hours)
Run same tests as task-008:
- [ ] iOS Release build on device
- [ ] Android Release build on device
- [ ] Full critical path testing
- [ ] Performance baseline (compare to .NET 9)

**Validation:**
- [ ] Builds succeed for both platforms
- [ ] No new trimming warnings
- [ ] All PRE-MIGRATION tests still pass
- [ ] No performance regressions
- [ ] No new crashes

**Blockers:** Requires ALL PRE-MIGRATION tasks  
**Estimated Effort:** 8 hours

---

### Day 2 — Cleanup Tasks (0.5 day)

#### Task: Remove Newtonsoft.Json (0.25 day)
**Task ID:** task-010  
**Priority:** LOW  
**Dependencies:** task-009  
**Owner:** Backend Engineer

**Changes:**
`xml
<!-- SpiritualGiftsSurvey.csproj — Remove this line -->
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
`

`csharp
// Converters/ItemMarginConverter.cs — Remove unused using
using Newtonsoft.Json.Linq; // ❌ Delete
`

**Validation:**
- [ ] Build succeeds
- [ ] App runs
- [ ] No runtime errors

**Estimated Effort:** 2 hours

---

#### Task: Fix SurveyResult Thread Safety (1 day)
**Task ID:** task-011  
**Priority:** MEDIUM  
**Dependencies:** task-009  
**Owner:** Backend Engineer

**Changes:**
`csharp
// Models/SurveyResult.cs
// Option 1: Remove Task.Run (ranking is fast enough)
public void RankGifts()
{
    if (Scores == null || !Scores.Any() || IsRanked)
        return;

    var ordered = Scores.OrderByDescending(x => x.Score).ToList();
    // ... ranking logic (synchronous)

    IsRanked = true;
}

// Option 2: Immutable data flow (if async needed)
public async Task RankGiftsAsync()
{
    if (Scores == null || !Scores.Any() || IsRanked)
        return;

    var scoresCopy = Scores.ToList();
    
    var rankedScores = await Task.Run(() =>
    {
        var ordered = scoresCopy.OrderByDescending(x => x.Score).ToList();
        // ... work on copy
        return ordered;
    });

    // Update on caller thread
    Scores.Clear();
    foreach (var score in rankedScores)
        Scores.Add(score);

    IsRanked = true;
}
`

**Validation:**
- [ ] Ranking still works
- [ ] No race conditions
- [ ] Unit tests pass
- [ ] Survey completion workflow works

**Estimated Effort:** 8 hours

---

### DURING-MIGRATION Phase Exit Criteria
- [ ] .NET 10 upgrade complete (task-009)
- [ ] Newtonsoft.Json removed (task-010)
- [ ] Thread safety fixed (task-011)
- [ ] Release builds validated on both platforms
- [ ] All regression tests pass
- [ ] Performance acceptable

**Total DURING-MIGRATION Effort:** 1-2 days

---

## PHASE 3: POST-MIGRATION Improvements (3-6 days)

**Goal:** Architectural cleanup after stability achieved  
**Priority:** MEDIUM/LOW  
**Timeline:** Can be done incrementally

---

### Task: Replace Service Locator (2 days)
**Task ID:** task-012  
**Priority:** MEDIUM  
**Dependencies:** task-009 (migration complete)  
**Owner:** Backend Engineer

**Changes:**
- Remove Services/IAggregatedServices.cs
- Remove Services/AggregatedServices.cs
- Update BaseViewModel to inject services directly
- Update all ViewModel constructors

**Before:**
`csharp
public abstract class BaseViewModel : ObservableObject
{
    protected IAggregatedServices Services { get; }
    
    protected BaseViewModel(IAggregatedServices services)
    {
        Services = services;
    }
    
    // Usage: Services.DatabaseService.GetData()
}
`

**After:**
`csharp
public abstract class BaseViewModel : ObservableObject
{
    protected IDatabaseService DatabaseService { get; }
    protected INavigationService NavigationService { get; }
    protected ITranslationService TranslationService { get; }
    // ... direct injection
    
    protected BaseViewModel(
        IDatabaseService databaseService,
        INavigationService navigationService,
        ITranslationService translationService)
    {
        DatabaseService = databaseService;
        NavigationService = navigationService;
        TranslationService = translationService;
    }
}
`

**Estimated Effort:** 16 hours

---

### Task: Convert ViewModels to Transient (1 day)
**Task ID:** task-013  
**Priority:** MEDIUM  
**Dependencies:** task-012  
**Owner:** Platform Engineer

**Changes:**
`csharp
// MauiProgram.cs
// Before:
services.AddSingleton<SurveyViewModel>();
services.AddSingleton<ResultsViewModel>();
// ... etc

// After:
services.AddTransient<SurveyViewModel>();
services.AddTransient<ResultsViewModel>();
// ... etc
`

**Testing:**
- [ ] Multiple navigations don't reuse old state
- [ ] Memory leaks resolved (use profiler)
- [ ] All pages still work

**Estimated Effort:** 8 hours

---

### Task: Split DatabaseService (3 days)
**Task ID:** task-014  
**Priority:** LOW  
**Dependencies:** task-009  
**Owner:** Backend Engineer

**Goal:** Separate concerns:
- IRepository — CRUD operations
- IDatabaseSyncService — Firebase sync
- ICacheService — In-memory caching
- IDatabaseMigrationService — Schema migrations

**Estimated Effort:** 24 hours

---

### POST-MIGRATION Phase Exit Criteria
- [ ] Service Locator removed (optional)
- [ ] ViewModels are Transient (optional)
- [ ] DatabaseService refactored (optional)
- [ ] Code quality improved
- [ ] No regressions

**Total POST-MIGRATION Effort:** 3-6 days (can be done incrementally)

---

## Risk Mitigation

### Rollback Plan
If migration fails at any step:
1. Revert .csproj TFM to net9.0
2. Revert package versions
3. Rebuild with .NET 9
4. Investigate failures on .NET 10

### Validation Checkpoints
After every task:
- [ ] Build succeeds
- [ ] Tests pass
- [ ] Manual smoke test passes

After every phase:
- [ ] Full regression testing
- [ ] Device testing (Release builds)
- [ ] Performance check
- [ ] Team sign-off

---

## Success Metrics

### Phase 1 Success
- ✅ 0 CRITICAL blockers remaining
- ✅ Release builds work on iOS AND Android
- ✅ 10+ unit tests passing
- ✅ All async anti-patterns eliminated

### Phase 2 Success
- ✅ .NET 10 builds successfully
- ✅ No new Release-only failures
- ✅ Performance within 10% of .NET 9 baseline

### Phase 3 Success
- ✅ Service Locator removed
- ✅ ViewModel state isolation improved
- ✅ Code quality metrics improved

---

## Timeline Summary

| Phase | Duration | Outcome |
|-------|----------|---------|
| PRE-MIGRATION | 7-8 days | Production-ready on .NET 9 |
| DURING-MIGRATION | 1-2 days | Stable on .NET 10 |
| POST-MIGRATION | 3-6 days | Modernized architecture |
| **TOTAL** | **11-16 days** | Production-ready .NET 10 app |

---

## Dependencies Graph

`
task-001 (Trimming) ─┬─> task-002 (JSON) ─┬─> task-007 (Tests) ─┐
                     │                     └─> task-008 (Device) ┤
                     └─────────────────────────> task-008         ├─> task-009 (Migration) ─┬─> task-010 (Cleanup)
task-003 (Async) ────┬─> task-004 (Fire&Forget) ──> task-005 (Lifecycle) ──> task-008 ──────┤                    │
                     └─────────────────────────────────────────────> task-008 ──────────────┘                    ├─> task-011 (Thread)
task-006 (MainThread) ───────────────────────────────────────────────────────────────────────> task-009 ─────────┤
                                                                                                                  ├─> task-012 (DI) ─> task-013 (Transient)
                                                                                                                  └─> task-014 (DatabaseService)
`

---

**Document Owner:** Architecture & Modernization Initiative  
**Last Updated:** January 2025  
**Next Review:** Weekly during execution
