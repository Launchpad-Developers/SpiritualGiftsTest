# Async and Threading Guidelines — Spiritual Gifts Survey

**Purpose:** Define safe, consistent async/threading patterns to prevent race conditions, deadlocks, and unobserved exceptions.

**Status:** DRAFT — Standards to be adopted during PRE-MIGRATION stabilization  
**Last Updated:** January 2025

---

## Executive Summary

**Current State:**
- 1 async void method (SplashPage.OnAppearing)
- 4+ fire-and-forget patterns (`_ = SomeAsync()`)
- 2+ async calls without await
- 1 Task.Run mutating shared state
- 0 CancellationToken usage in app code
- Fragile lifecycle initialization (BasePage, BaseViewModel)

**Impact:**
- 71% of production bugs are Release-only
- Async timing issues worsen under optimization
- Unobserved exceptions cause silent failures
- Race conditions in state management

**Goal:**
- Eliminate all async anti-patterns
- Establish deterministic initialization
- Add cancellation support
- Ensure MainThread safety
- Enable predictable testing

---

## Core Principles

1. **Never use `async void`** (except event handlers, and even then, wrap with try/catch)
2. **Never fire-and-forget** (`_ = SomeAsync()`) — always await or manage the task
3. **Always use `CancellationToken`** for long-running or lifecycle-bound operations
4. **Always dispatch to MainThread** when needed (don't assume caller's context)
5. **Always observe exceptions** — no silent failures
6. **Prefer async all the way** — don't block async with `.Wait()` or `.Result`

---

## Anti-Patterns to Eliminate

### ❌ ANTI-PATTERN 1: Async Void
**What:** Methods that return `void` but are marked `async`

**Current Violations:**
```csharp
// Views/Splash/SplashPage.xaml.cs:21-26
protected override async void OnAppearing()
{
    base.OnAppearing();
    await Task.Delay(4000);
    // ... navigation
}
```

**Why It's Bad:**
- Exceptions are unobservable (cannot be caught by caller)
- Cannot await the operation
- Timing issues in tests
- Silent failures in production

**Correct Pattern:**
```csharp
// Option 1: Async event handler with try/catch
protected override async void OnAppearing()
{
    base.OnAppearing();
    
    try
    {
        await LoadDataAsync();
    }
    catch (Exception ex)
    {
        Analytics.TrackEvent("OnAppearingFailure", 
            new Dictionary<string, string> { { "Error", ex.Message } });
        // Show error to user or navigate to error page
    }
}

private async Task LoadDataAsync()
{
    await Task.Delay(4000);
    // ... rest of logic
}

// Option 2: Synchronous event handler dispatching async work
protected override void OnAppearing()
{
    base.OnAppearing();
    
    // Dispatch async work, observe exceptions
    MainThread.BeginInvokeOnMainThread(async () =>
    {
        try
        {
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            Analytics.TrackEvent("OnAppearingFailure", 
                new Dictionary<string, string> { { "Error", ex.Message } });
        }
    });
}
```

**Migration Priority:** CRITICAL

---

### ❌ ANTI-PATTERN 2: Fire-and-Forget
**What:** Starting async operations without awaiting or tracking them

**Current Violations:**
```csharp
// Views/Results/ResultsViewModel.cs:28-34
[QueryProperty(nameof(UserGiftResult), "UserGiftResult")]
public SurveyResult? UserGiftResult
{
    get => _userGiftResult;
    set
    {
        _userGiftResult = value;
        OnPropertyChanged();
        _ = value.RankGiftsAsync(); // ❌ Fire-and-forget
        _ = LoadUserGiftResultAsync(value); // ❌ Fire-and-forget
    }
}

// Views/Settings/SettingsViewModel.cs:256-266
_ = TranslationService.SetLanguageByCodeAsync(code); // ❌ Fire-and-forget

// Views/GiftDescription/GiftDescriptionViewModel.cs:26-33
_ = LoadGiftDetailsAsync(value); // ❌ Fire-and-forget

// Services/EmailService.cs:90-94
_ = result.RankGiftsAsync(); // ❌ Fire-and-forget
```

**Why It's Bad:**
- Unobserved exceptions
- No error handling
- Race conditions with UI state
- Cannot track completion
- Cannot cancel
- Unpredictable timing in tests

**Correct Pattern:**

```csharp
// Option 1: Await in async context (preferred)
public async Task SetUserGiftResultAsync(SurveyResult? value)
{
    _userGiftResult = value;
    OnPropertyChanged(nameof(UserGiftResult));
    
    if (value != null)
    {
        await RunWithLoading(async () =>
        {
            await value.RankGiftsAsync();
            await LoadUserGiftResultAsync(value);
        });
    }
}

// Option 2: Background task with observation (if can't await)
private Task? _backgroundTask;

public SurveyResult? UserGiftResult
{
    get => _userGiftResult;
    set
    {
        _userGiftResult = value;
        OnPropertyChanged();
        
        if (value != null)
        {
            _backgroundTask = Task.Run(async () =>
            {
                try
                {
                    await value.RankGiftsAsync();
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await LoadUserGiftResultAsync(value);
                    });
                }
                catch (Exception ex)
                {
                    Analytics.TrackEvent("RankGiftsFailure", 
                        new Dictionary<string, string> { { "Error", ex.Message } });
                }
            });
        }
    }
}

// Option 3: Event-based async command (best for user interactions)
[RelayCommand]
private async Task LoadResults(SurveyResult result)
{
    await RunWithLoading(async () =>
    {
        await result.RankGiftsAsync();
        await LoadUserGiftResultAsync(result);
    });
}
```

**Migration Priority:** HIGH

---

### ❌ ANTI-PATTERN 3: Async Without Await
**What:** Calling async methods without awaiting them

**Current Violations:**
```csharp
// Views/Shared/BasePage.cs:22-27
protected override void OnNavigatedTo(NavigatedToEventArgs args)
{
    base.OnNavigatedTo(args);
    
    if (ViewModel != null)
        ViewModel.InitAsync(); // ❌ Not awaited
}

// Views/Shared/BaseViewModel.cs:37-40
WeakReferenceMessenger.Default.Register<LanguageChangedMessage>(this, (r, m) =>
{
    InitAsync(); // ❌ Not awaited
    RefreshViewModel();
});
```

**Why It's Bad:**
- Same issues as fire-and-forget
- Lifecycle timing becomes unpredictable
- QueryProperty may race with InitAsync
- RefreshViewModel may run before InitAsync completes

**Correct Pattern:**

```csharp
// BasePage.cs — Await async initialization
protected override async void OnNavigatedTo(NavigatedToEventArgs args)
{
    base.OnNavigatedTo(args);
    
    if (ViewModel != null)
    {
        try
        {
            await ViewModel.InitAsync();
        }
        catch (Exception ex)
        {
            Analytics.TrackEvent("InitAsyncFailure", 
                new Dictionary<string, string> { { "Error", ex.Message } });
        }
    }
}

// BaseViewModel.cs — Dispatch and await in message handler
WeakReferenceMessenger.Default.Register<LanguageChangedMessage>(this, async (r, m) =>
{
    try
    {
        await InitAsync();
        RefreshViewModel();
    }
    catch (Exception ex)
    {
        Analytics.TrackEvent("LanguageChangedFailure", 
            new Dictionary<string, string> { { "Error", ex.Message } });
    }
});
```

**Migration Priority:** CRITICAL (part of lifecycle fix)

---

### ❌ ANTI-PATTERN 4: Task.Run with Shared State
**What:** Using `Task.Run` to mutate shared state without synchronization

**Current Violations:**
```csharp
// Models/SurveyResult.cs:35-91
public async Task RankGiftsAsync()
{
    if (Scores == null || !Scores.Any() || IsRanked)
        return;

    await Task.Run(() =>
    {
        // Mutates Scores collection from background thread
        var ordered = Scores.OrderByDescending(x => x.Score).ToList();
        // ...
        foreach (var score in ordered)
        {
            score.GiftRank = GiftRank.Primary; // ❌ Mutating shared state
        }
    });

    IsRanked = true;
}
```

**Why It's Bad:**
- Race condition: UI may read `Scores` while background thread writes
- No synchronization
- May crash on some platforms (e.g., ObservableCollection access from non-UI thread)
- Unpredictable behavior under optimization

**Correct Pattern:**

```csharp
// Option 1: Synchronous ranking (fast enough, no need for Task.Run)
public void RankGifts()
{
    if (Scores == null || !Scores.Any() || IsRanked)
        return;

    var ordered = Scores.OrderByDescending(x => x.Score).ToList();
    var scoreGroups = ordered
        .GroupBy(x => x.Score)
        .OrderByDescending(g => g.Key)
        .ToList();

    // ... ranking logic

    foreach (var score in ordered)
    {
        // Safe: single-threaded access
        if (primary.Contains(score))
            score.GiftRank = GiftRank.Primary;
        else if (secondary.Contains(score))
            score.GiftRank = GiftRank.Secondary;
        else
            score.GiftRank = GiftRank.None;
    }

    IsRanked = true;
}

// Option 2: Async with immutable data flow (if ranking is slow)
public async Task RankGiftsAsync()
{
    if (Scores == null || !Scores.Any() || IsRanked)
        return;

    // Copy state before background work
    var scoresCopy = Scores.ToList();
    
    var rankedScores = await Task.Run(() =>
    {
        var ordered = scoresCopy.OrderByDescending(x => x.Score).ToList();
        // ... ranking logic (working on copy)
        return ordered; // Return new ranked list
    });

    // Update shared state on caller thread (likely MainThread)
    Scores.Clear();
    foreach (var score in rankedScores)
        Scores.Add(score);

    IsRanked = true;
}
```

**Migration Priority:** MEDIUM (can be done during or post-migration)

---

### ❌ ANTI-PATTERN 5: No CancellationToken
**What:** Long-running operations without cancellation support

**Current Violations:**
- `InitAsync()` — no cancellation when user navigates away
- `LoadUserGiftResultAsync()` — no cancellation
- `UrlService.FetchFromFirebase()` — no cancellation
- Database operations — no cancellation

**Why It's Bad:**
- Wasted work if user navigates away
- Memory leaks (tasks keep references alive)
- Cannot interrupt long operations
- Poor user experience (can't cancel slow loads)

**Correct Pattern:**

```csharp
// BaseViewModel.cs — Add cancellation support
private CancellationTokenSource? _initCts;

public async Task InitAsync()
{
    // Cancel previous initialization if still running
    _initCts?.Cancel();
    _initCts = new CancellationTokenSource();
    
    try
    {
        await InitCoreAsync(_initCts.Token);
    }
    catch (OperationCanceledException)
    {
        // Expected when navigating away
    }
}

protected abstract Task InitCoreAsync(CancellationToken cancellationToken);

// In derived ViewModels
protected override async Task InitCoreAsync(CancellationToken cancellationToken)
{
    var data = await DatabaseService.GetDataAsync(cancellationToken);
    cancellationToken.ThrowIfCancellationRequested();
    
    // Update UI
    Items = data;
}

// Cleanup on dispose
public void Dispose()
{
    _initCts?.Cancel();
    _initCts?.Dispose();
}
```

**Migration Priority:** MEDIUM (recommended but not blocking)

---

## Lifecycle Initialization Pattern

### Current Problem
```
OnNavigatedTo → InitAsync() ❌ Not awaited
              → QueryProperty set → may call async without await
OnAppearing → RefreshViewModel() (sync)

RESULT: Race conditions, unpredictable timing
```

### Correct Pattern
```
OnNavigatedTo → await InitAsync(ct)
              → QueryProperty set BEFORE InitAsync
RefreshViewModel AFTER InitAsync completes
OnDisappearing → Cancel ongoing operations
```

### Implementation

```csharp
// BasePage.cs
public abstract class BasePage : ContentPage
{
    protected BaseViewModel? ViewModel { get; private set; }
    
    protected BasePage(BaseViewModel viewModel)
    {
        ViewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        
        if (ViewModel != null)
        {
            try
            {
                // QueryProperty is already set by Shell at this point
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
                // Show error page or retry
            }
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        // Refresh only happens AFTER Init completes
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
}

// BaseViewModel.cs
public abstract class BaseViewModel : ObservableObject, IDisposable
{
    private CancellationTokenSource? _operationsCts;
    protected bool RequiresInitialization { get; set; } = true;

    public async Task InitAsync()
    {
        if (!RequiresInitialization)
            return;

        // Cancel previous operations
        _operationsCts?.Cancel();
        _operationsCts = new CancellationTokenSource();

        try
        {
            await InitCoreAsync(_operationsCts.Token);
            RequiresInitialization = false;
        }
        catch (OperationCanceledException)
        {
            // Expected during navigation
        }
    }

    protected abstract Task InitCoreAsync(CancellationToken cancellationToken);
    public abstract void RefreshViewModel();

    public void CancelOngoingOperations()
    {
        _operationsCts?.Cancel();
    }

    public void Dispose()
    {
        _operationsCts?.Cancel();
        _operationsCts?.Dispose();
        GC.SuppressFinalize(this);
    }
}
```

---

## MainThread Safety

### Rule
**Never assume you're on the MainThread.** Always dispatch if needed.

### Current Problem
```csharp
// Services/NavigationService.cs:15-68
public async Task NavigateAsync(string route, Dictionary<string, object>? parameters = null)
{
    // ❌ Assumes caller is on MainThread
    await Shell.Current.GoToAsync(route, parameters);
}
```

### Correct Pattern

```csharp
// NavigationService.cs
public async Task NavigateAsync(string route, Dictionary<string, object>? parameters = null)
{
    if (MainThread.IsMainThread)
    {
        await Shell.Current.GoToAsync(route, parameters);
    }
    else
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await Shell.Current.GoToAsync(route, parameters);
        });
    }
}

// Or simpler (always dispatch):
public async Task NavigateAsync(string route, Dictionary<string, object>? parameters = null)
{
    await MainThread.InvokeOnMainThreadAsync(async () =>
    {
        await Shell.Current.GoToAsync(route, parameters);
    });
}
```

---

## Exception Handling

### Rule
**All async operations MUST handle exceptions.** No silent failures.

### Current Problem
```csharp
_ = LoadDataAsync(); // ❌ Unobserved exception
```

### Correct Pattern

```csharp
// Option 1: Await and handle
try
{
    await LoadDataAsync();
}
catch (Exception ex)
{
    Analytics.TrackEvent("LoadDataFailure", 
        new Dictionary<string, string> { { "Error", ex.Message } });
    await NotifyUserAsync("Error", "Failed to load data", "OK");
}

// Option 2: Background task with observation
private async void StartBackgroundLoad()
{
    try
    {
        await LoadDataAsync();
    }
    catch (Exception ex)
    {
        Analytics.TrackEvent("BackgroundLoadFailure", 
            new Dictionary<string, string> { { "Error", ex.Message } });
    }
}
```

---

## Testing Strategy

### Unit Testing Async Code
```csharp
[Fact]
public async Task RankGiftsAsync_ShouldRankCorrectly()
{
    // Arrange
    var result = new SurveyResult
    {
        Scores = new List<UserGiftScore>
        {
            new() { Gift = Gifts.Teaching, Score = 50 },
            new() { Gift = Gifts.Faith, Score = 48 },
            new() { Gift = Gifts.Mercy, Score = 45 }
        }
    };

    // Act
    await result.RankGiftsAsync();

    // Assert
    Assert.True(result.IsRanked);
    Assert.Equal(GiftRank.Primary, result.Scores[0].GiftRank);
    Assert.Equal(GiftRank.Primary, result.Scores[1].GiftRank);
    Assert.Equal(GiftRank.Primary, result.Scores[2].GiftRank);
}

[Fact]
public async Task InitAsync_ShouldCancelPreviousOperation()
{
    // Arrange
    var vm = new TestViewModel();

    // Act
    var task1 = vm.InitAsync(); // Start first init
    var task2 = vm.InitAsync(); // Start second init (should cancel first)

    // Assert
    await Assert.ThrowsAsync<OperationCanceledException>(() => task1);
    await task2; // Should complete successfully
}
```

---

## Migration Checklist

### Phase 1: Fix Async Void (0.5 day)
- [ ] Fix `SplashPage.OnAppearing()`
- [ ] Add try/catch to any remaining async void handlers
- [ ] Test splash screen navigation

### Phase 2: Fix Fire-and-Forget (1 day)
- [ ] Fix `ResultsViewModel.UserGiftResult` setter
- [ ] Fix `SettingsViewModel` language change
- [ ] Fix `GiftDescriptionViewModel.UserGiftDescription` setter
- [ ] Fix `EmailService.GenerateEmailBodyAsync()`
- [ ] Test all affected workflows

### Phase 3: Fix Lifecycle (2 days)
- [ ] Update `BasePage.OnNavigatedTo()` to await InitAsync
- [ ] Update `BaseViewModel` message handler to await InitAsync
- [ ] Add cancellation support to BaseViewModel
- [ ] Update all derived ViewModels to implement InitCoreAsync
- [ ] Test page navigation and back navigation
- [ ] Test language switching

### Phase 4: Fix MainThread Safety (0.5 day)
- [ ] Update `NavigationService.NavigateAsync()`
- [ ] Test navigation from background threads (if any)

### Phase 5: Fix Task.Run (1 day)
- [ ] Refactor `SurveyResult.RankGiftsAsync()`
- [ ] Add unit tests for ranking algorithm
- [ ] Test survey completion workflow

### Phase 6: Add Cancellation (Optional, 1 day)
- [ ] Add CancellationToken to long-running operations
- [ ] Add dispose/cleanup to ViewModels
- [ ] Test navigation away during loading

---

## Standards Summary

| Pattern | Status | Priority |
|---------|--------|----------|
| No async void (except wrapped event handlers) | ❌ Violated | CRITICAL |
| No fire-and-forget | ❌ Violated | HIGH |
| Always await or observe async | ❌ Violated | CRITICAL |
| Always handle exceptions | ❌ Violated | HIGH |
| MainThread safety | ❌ Violated | MEDIUM |
| CancellationToken for long operations | ❌ Missing | MEDIUM |
| No Task.Run with shared state | ❌ Violated | MEDIUM |
| Deterministic lifecycle | ❌ Fragile | CRITICAL |

---

**Document Owner:** Architecture & Modernization Initiative  
**Last Updated:** January 2025  
**Next Review:** After PRE-MIGRATION async fixes complete
