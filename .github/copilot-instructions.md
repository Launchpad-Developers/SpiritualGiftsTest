# Copilot Instructions — SpiritualGiftsSurvey

## What This App Does

A cross-platform mobile spiritual gifts assessment app built with **.NET MAUI 9** targeting Android (API 23+) and iOS (15.0+). Users answer 250+ survey questions, which are scored and ranked to identify their top spiritual gifts. Results can be emailed with gift descriptions and Bible verses.

---

## Build Commands

```bash
# Build
dotnet build

# Build for a specific platform
dotnet build -f net9.0-android
dotnet build -f net9.0-ios
```

There are **no automated tests** and no CI/CD workflows. Manual device/emulator testing is the norm.

---

## Architecture

### MVVM with BasePage / BaseViewModel

Every page inherits `BasePage` and every ViewModel inherits `BaseViewModel`:

- **`BasePage`** (`Views/Shared/BasePage.cs`): Sets `BindingContext = viewModel`, calls `ViewModel.RefreshViewModel()` on `OnAppearing`, and calls `ViewModel.InitAsync()` on `OnNavigatedTo`.
- **`BaseViewModel`** (`Views/Shared/BaseViewModel.cs`): Provides all injected services via protected properties, exposes `IsLoading`, `FlowDirection`, and abstract methods `InitAsync()` + `RefreshViewModel()`. Uses `CommunityToolkit.Mvvm` (`ObservableObject`, `[ObservableProperty]`, `[RelayCommand]`).

```csharp
// Page constructor pattern — always passes ViewModel to base
public partial class WelcomePage : BasePage
{
    public WelcomePage(WelcomeViewModel vm) : base(vm)
    {
        InitializeComponent();
        On<iOS>().SetUseSafeArea(false);
    }
}

// ViewModel pattern
public partial class WelcomeViewModel : BaseViewModel
{
    public override async Task InitAsync() { /* called on navigation */ }
    public override void RefreshViewModel() { /* called on re-appear */ }
}
```

### Dependency Injection (MauiProgram.cs)

All pages and ViewModels are registered as **singletons** in `MauiProgram.RegisterViewModels()`. Services are registered in `MauiProgram.RegisterAppServices()`. When adding a new page, register both the ViewModel and Page as singletons — the MAUI DI container resolves the constructor chain automatically.

### Navigation

- Routes are defined as `static string` properties in `Routing/Routes.cs` using `nameof()` — always use `Routes.XxxPage` constants, never raw strings.
- Navigation uses `Shell.Current.GoToAsync($"///{route}")` (absolute routes, no back-stack).
- Pass data via `Shell.Current.GoToAsync(route, parameters)` and receive with `[QueryProperty]` on the ViewModel.
- All Shell routes are declared in `AppShell.xaml` using `<ShellContent>`.

```csharp
// Sending navigation params
await NavigationService.NavigateAsync(Routes.ResultsPage, new Dictionary<string, object>
{
    ["UserGiftResult"] = result
});

// Receiving params
[QueryProperty(nameof(UserGiftResult), "UserGiftResult")]
public partial class ResultsViewModel : BaseViewModel { ... }
```

### Data Layer

- **SQLite** (sqlite-net-pcl) at `FileSystem.AppDataDirectory/SpiritualGiftsSurvey.sqlite`.
- All DB access goes through `IDatabaseService` / `DatabaseService`.
- Content data (questions, gift descriptions, translations) is fetched from **Firebase Realtime Database** and cached locally. On startup, `SplashViewModel` compares local vs. remote version and calls `RefreshDatabaseAsync()` if the remote is newer.
- The JSON files at the repo root (`071425.json`, `080125.json`) are dated developer snapshots of the Firebase data structure — they are **not** loaded at runtime.
- Model classes are in `Models/` (e.g., `Question.cs`, `GiftDescription.cs`, `Translation.cs`, `UserGiftScore.cs`).

### Localization / i18n

- All UI strings, questions, and gift descriptions are translation-keyed and stored in the local SQLite DB (populated from Firebase).
- Use `TranslationService.GetString("Key", "Default")` in ViewModels — never hardcode visible strings.
- Language changes broadcast `LanguageChangedMessage` via `WeakReferenceMessenger`, which causes every registered ViewModel to call `InitAsync()`.
- RTL layout (Arabic) is handled automatically: `FlowDirection = TranslationService.FlowDirection` is set in `BaseViewModel` and bound in every XAML page.

### Messaging (CommunityToolkit.Mvvm.Messaging)

- `LanguageChangedMessage` — broadcast on language switch; all ViewModels re-init.
- `ScrollToQuestionMessage` — DEBUG only; scrolls the survey to a specific question index.
- Registration uses `WeakReferenceMessenger.Default`.

---

## Key Conventions

### Adding a New Page

1. Create `Views/{Feature}/{Feature}Page.xaml` inheriting `views:BasePage` with `x:DataType="vm:{Feature}ViewModel"`.
2. Create `Views/{Feature}/{Feature}Page.xaml.cs` — constructor takes ViewModel, passes to `base(vm)`.
3. Create `Views/{Feature}/{Feature}ViewModel.cs` inheriting `BaseViewModel`, override `InitAsync` and `RefreshViewModel`.
4. Add `public static string {Feature}Page => nameof({Feature}Page);` to `Routing/Routes.cs`.
5. Register in `MauiProgram.RegisterViewModels()`: `AddSingleton<{Feature}ViewModel>()` + `AddSingleton<{Feature}Page>()`.
6. Add `<ShellContent>` to `AppShell.xaml`.

### Adding a New Service

1. Create `Services/I{Name}Service.cs` interface.
2. Create `Services/{Name}Service.cs` implementation.
3. Register in `MauiProgram.RegisterAppServices()` as `AddSingleton` or `AddTransient`.
4. If needed by most ViewModels, inject it into `AggregatedServices` and expose it via `BaseViewModel`.

### XAML Patterns

- All pages use `x:DataType` for **compiled bindings** — bindings are validated at compile time.
- Use `{StaticResource}` for colors, styles, and converters declared in `App.xaml`.
- Responsive sizing always uses `{OnIdiom Phone=..., Tablet=...}` and `{OnPlatform Android=..., iOS=...}`.
- Icons use `FontImageSource` with `FontFamily="FA6ProThin"` or `"FA6Solid"` and glyphs from `Utilities/IconFontGlyphs.cs`.
- Custom controls live in `Views/Controls/` (e.g., `QuestionView.xaml`, `LoadingOverlay.xaml`, `NavBar.xaml`).
- Value converters live in `Converters/` and must be registered in `App.xaml` as static resources.

### Async & UI Thread

- Wrap any loading operation with `await RunWithLoading(async () => { ... })` (defined in `BaseViewModel`).
- `RunWithLoading` automatically sets `IsLoading = true`, yields to render the overlay, executes the action, and sets `IsLoading = false` in a try/finally.
- Periodic `await Task.Yield()` inside long loops prevents UI jank (see `SurveyViewModel`).

### Naming

| Entity | Convention | Example |
|--------|------------|---------|
| Page | `{Feature}Page.xaml[.cs]` | `SurveyPage.xaml` |
| ViewModel | `{Feature}ViewModel.cs` | `SurveyViewModel.cs` |
| Custom Control | `{Name}View.xaml[.cs]` | `QuestionView.xaml` |
| Service | `I{Name}Service` / `{Name}Service` | `IDatabaseService` |
| Route constant | `{Feature}Page` (static string) | `Routes.ResultsPage` |
| Message | `{Name}Message.cs` | `LanguageChangedMessage.cs` |
| Async method | Suffix `Async` | `InitAsync()`, `SaveUserGiftResultAsync()` |

### Debug Mode

- `DebugHelper.cs` limits the question set for fast manual testing.
- Firebase URLs switch automatically via `#if DEBUG` in `MauiProgram.cs` (dev vs. prod database).
- Debug preference keys (defined in `AppConstants.cs`) can be set via `Preferences` to control test behavior:
  - `DebugTotalTopicsKey` — limit number of gift topics
  - `DebugQuestionsPerTopicKey` — limit questions per topic
  - `DebugAllowUnansweredQuestionsKey` / `DebugTotalUnansweredQuestionsKey` — simulate unanswered questions
  - `DebugOptionsEnabledKey` — enable debug UI

---

## Key Packages

| Purpose | Package |
|---------|---------|
| MVVM | `CommunityToolkit.Mvvm` 8.4.0 |
| UI Toolkit | `CommunityToolkit.Maui` 12.1.0 |
| SQLite | `sqlite-net-pcl` 1.7.335 |
| HTTP / Firebase | `Microsoft.Extensions.Http` 9.0.6 |
| JSON | `Newtonsoft.Json` 13.0.3 |
| Analytics | `Microsoft.ApplicationInsights` 2.23.0 |
