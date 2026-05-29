# Architecture Overview

**Application:** Spiritual Gifts Survey  
**Platform:** .NET MAUI 9  
**Targets:** Android (API 23+), iOS (15.0+)  
**Last Updated:** 2026-05-28

---

## System Overview

The Spiritual Gifts Survey is a cross-platform mobile application that helps users identify their spiritual gifts through a comprehensive survey. Users answer 250+ questions, receive ranked results, and can email the results with Biblical descriptions and scripture references.

```
┌─────────────┐
│   User      │
└──────┬──────┘
       │
┌──────▼──────────────────────────────────────┐
│          MAUI Application                    │
│  ┌────────────────────────────────────────┐ │
│  │  UI Layer (XAML + ViewModels)          │ │
│  │  - Shell Navigation                     │ │
│  │  - MVVM Pattern (CommunityToolkit)     │ │
│  │  - Compiled Bindings                    │ │
│  └────────────┬───────────────────────────┘ │
│               │                              │
│  ┌────────────▼───────────────────────────┐ │
│  │  Business Logic Layer                   │ │
│  │  - Services (Navigation, Translation)   │ │
│  │  - Scoring/Ranking Algorithm            │ │
│  │  - Email Generation                     │ │
│  └────────────┬───────────────────────────┘ │
│               │                              │
│  ┌────────────▼───────────────────────────┐ │
│  │  Data Layer                             │ │
│  │  - SQLite (Local Cache)                 │ │
│  │  - DatabaseService                      │ │
│  └────────────┬───────────────────────────┘ │
└───────────────┼──────────────────────────────┘
                │
    ┌───────────┴───────────┐
    │                       │
┌───▼──────────┐    ┌──────▼────────┐
│ Firebase     │    │ Device APIs   │
│ Realtime DB  │    │ - Email       │
│ (Content)    │    │ - Filesystem  │
└──────────────┘    │ - Preferences │
                    └───────────────┘
```

---

## Solution Structure

### Project: `SpiritualGiftsSurvey.csproj`

Single-project MAUI application targeting:
- `net9.0-android` (min SDK 23, target SDK 35)
- `net9.0-ios` (min iOS 15.0)

**No test projects** — see `docs/testing-assessment.md` for critical gap analysis.

---

## Directory Structure

```
SpiritualGiftsSurvey/
├── Views/                     # XAML pages and ViewModels
│   ├── Shared/                # BasePage, BaseViewModel
│   ├── Controls/              # Reusable UI components
│   ├── Welcome/               # Welcome screen
│   ├── Survey/                # Main survey flow
│   ├── Results/               # Results display
│   ├── GiftDescription/       # Individual gift details
│   ├── Send/                  # Email results
│   ├── Settings/              # App settings
│   ├── Reporting/             # Admin email configuration
│   ├── AppInfo/               # About page
│   └── Splash/                # Splash screen
│
├── Services/                  # Business logic and infrastructure
│   ├── IDatabaseService       # SQLite CRUD operations
│   ├── ITranslationService    # i18n and localization
│   ├── INavigationService     # Shell navigation wrapper
│   ├── IUrlService            # Firebase HTTP client
│   ├── IEmailService          # Email composition
│   ├── IAnalyticsService      # AppInsights telemetry
│   ├── IDeviceStorageService  # File system access
│   ├── IAppInfoService        # App metadata
│   └── IAggregatedServices    # Service locator facade
│
├── Models/                    # Data models (SQLite entities)
│   ├── Translation            # Language metadata
│   ├── Question               # Survey question
│   ├── GiftDescription        # Gift details and verses
│   ├── SurveyResult           # User results
│   ├── UserGiftScore          # Individual gift score
│   ├── AppString              # UI translations
│   ├── LanguageOption         # Available languages
│   └── ...
│
├── Routing/                   # Navigation route constants
├── Messages/                  # WeakReferenceMessenger messages
├── Helpers/                   # Utility classes (DebugHelper, PageHelper)
├── Utilities/                 # Constants, exceptions, handlers
├── Converters/                # XAML value converters
├── Enums/                     # Shared enumerations
├── il8n/                      # XAML markup extensions for i18n
├── Resources/                 # Fonts, images, styles
├── Platforms/                 # Platform-specific code
│   ├── Android/               # MainActivity, AndroidManifest.xml
│   └── iOS/                   # AppDelegate, Info.plist
│
├── App.xaml[.cs]              # Application entry point
├── AppShell.xaml[.cs]         # Shell navigation structure
├── MauiProgram.cs             # DI registration and configuration
└── SpiritualGiftsSurvey.csproj
```

---

## Architecture Patterns

### 1. MVVM (Model-View-ViewModel)

**Implementation:** CommunityToolkit.Mvvm 8.4.0

#### Base Classes

- **`BasePage`** (`Views/Shared/BasePage.cs`)
  - Sets `BindingContext = viewModel`
  - Calls `ViewModel.RefreshViewModel()` on `OnAppearing()`
  - Calls `ViewModel.InitAsync()` on `OnNavigatedTo()`
  
- **`BaseViewModel`** (`Views/Shared/BaseViewModel.cs`)
  - Inherits `ObservableObject` from CommunityToolkit
  - Exposes all services via protected properties
  - Abstract methods: `InitAsync()`, `RefreshViewModel()`
  - Common properties: `IsLoading`, `FlowDirection`, `Title`, `PageTopic`
  - Uses `[ObservableProperty]` and `[RelayCommand]` attributes

#### Lifecycle

```
Navigation → OnNavigatedTo() → InitAsync()
          ↓
      OnAppearing() → RefreshViewModel()
```

**⚠️ Issue:** `InitAsync()` called without `await` — see `docs/release-build-findings.md` (HIGH-3)

---

### 2. Dependency Injection

**Container:** Microsoft.Extensions.DependencyInjection (built into MAUI)

#### Service Registration (`MauiProgram.cs`)

**Services:**
- `Singleton`: Translation, Navigation, Analytics, AppInfo, Email, AggregatedServices, IPreferences
- `Transient`: DatabaseService, DeviceStorageService
- `HttpClient`: UrlService (typed client with base URL)

**ViewModels & Pages:**
- All registered as `Singleton` (⚠️ potential state persistence issue)

#### Service Locator Pattern

`IAggregatedServices` acts as a **service locator**:

```csharp
public interface IAggregatedServices
{
    IDatabaseService DatabaseService { get; }
    ITranslationService TranslationService { get; }
    INavigationService NavigationService { get; }
    // ... 8 services total
}
```

**Usage in ViewModels:**
```csharp
public abstract class BaseViewModel
{
    protected IDatabaseService DatabaseService => AggregatedServices.DatabaseService;
    protected ITranslationService TranslationService => AggregatedServices.TranslationService;
    // ...
}
```

**⚠️ Trade-off:** Service locator hides dependencies and complicates testing, but reduces constructor complexity.

---

### 3. Navigation Architecture

**Framework:** MAUI Shell

#### Route Definition

All routes defined as `static string` properties in `Routing/Routes.cs`:

```csharp
public static class Routes
{
    public static string WelcomePage => nameof(WelcomePage);
    public static string SurveyPage => nameof(SurveyPage);
    // ... 9 routes total
}
```

#### Navigation Service

`INavigationService` wraps `Shell.Current.GoToAsync()`:

```csharp
// Absolute navigation (clears back stack)
await NavigationService.NavigateAsync(Routes.ResultsPage);

// With parameters
await NavigationService.NavigateAsync(Routes.ResultsPage, new Dictionary<string, object>
{
    ["UserGiftResult"] = result
});

// Back navigation (preserves stack)
await NavigationService.GoBackAsync(Routes.WelcomePage);
```

#### Parameter Passing

Uses Shell's `QueryProperty` attribute:

```csharp
[QueryProperty(nameof(UserGiftResult), "UserGiftResult")]
public partial class ResultsViewModel : BaseViewModel
{
    [ObservableProperty] private SurveyResult? userGiftResult;
    
    partial void OnUserGiftResultChanged(SurveyResult? value)
    {
        // Handle parameter arrival (may occur before or after InitAsync)
    }
}
```

**⚠️ Timing fragility:** Parameters may arrive before/after `InitAsync()` completes.

---

### 4. Data Layer

#### Architecture

```
Firebase RTDB → UrlService → DatabaseService → SQLite → ViewModels → UI
     ↓                ↓              ↓
   JSON          HTTP Client    CRUD Ops
```

#### Firebase Integration

**Environment-Specific Base URLs:**
```csharp
#if DEBUG
    var baseUrl = "https://sgt-dev-b29c8-default-rtdb.firebaseio.com/";
#else
    var baseUrl = "https://sgt-prod-691ce-default-rtdb.firebaseio.com/";
#endif
```

**Data Sync Process:**
1. `SplashViewModel` compares local DB version to Firebase version
2. If remote is newer, calls `DatabaseService.RefreshDatabaseAsync()`
3. Downloads full JSON from `/.json` endpoint
4. Deserializes into `RootModel` (⚠️ **CRITICAL:** fails in Release without source gen)
5. Drops content tables, rebuilds from Firebase data
6. Preserves user result tables

#### SQLite Schema

**Content Tables (refreshed from Firebase):**
- `DatabaseInfo` — version metadata
- `Translation` — language configurations
- `AppString` — UI translations (keyed)
- `LanguageOption` — available languages
- `Question` — survey questions
- `GiftDescription` — gift details and descriptions
- `Verse` — Biblical references
- `Reflection` — reflection questions

**User Data Tables (preserved):**
- `SurveyResult` — completed survey results
- `UserGiftScore` — individual gift scores per result

#### Database Access Pattern

```csharp
// Synchronous (rare)
using var conn = new SQLiteConnection(DatabasePath);
var items = conn.Table<Question>().Where(q => q.TranslationGuid == guid).ToList();

// Asynchronous (preferred)
var conn = GetAsyncConnection();
var items = await conn.Table<Question>()
                      .Where(q => q.TranslationGuid == guid)
                      .ToListAsync();
```

**⚠️ Pattern:** New connection per call (no connection pooling). Acceptable for SQLite but verbose.

---

### 5. Localization / i18n

#### Translation Service

**Responsibilities:**
- Load UI strings from SQLite (`AppString` table)
- Manage current language selection
- Provide RTL/LTR flow direction
- Broadcast language changes

**Language Switching:**
```csharp
await TranslationService.SetLanguageByCodeAsync("AR");
// ↓
// 1. Update Preferences
// 2. Reload AppStrings dictionary
// 3. Send LanguageChangedMessage
// ↓
// All ViewModels receive message and call InitAsync() to reload UI
```

**⚠️ Issue:** `TranslationService` uses `Preferences.Default` directly instead of injected `IPreferences` (inconsistent).

#### RTL Support

- `Translation.FlowDirection` field stores "RTL" or "LTR"
- Parsed to `FlowDirection` enum
- Set on `BaseViewModel.FlowDirection`
- Bound to all pages: `FlowDirection="{Binding FlowDirection}"`

#### XAML Markup Extension

```xml
<!-- Old pattern (not used much) -->
<Label Text="{i18n:Translate Welcome}" />

<!-- Current pattern (preferred) -->
<Label Text="{Binding WelcomeText}" />
<!-- ViewModel loads: WelcomeText = TranslationService.GetString("Welcome", "Welcome"); -->
```

---

### 6. Messaging

**Framework:** `CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger`

**Messages:**
- `LanguageChangedMessage` — broadcast when user changes language
  - **Subscribers:** All ViewModels (via `BaseViewModel`)
  - **Action:** Re-initialize UI strings
- `ScrollToQuestionMessage` — DEBUG only, scroll survey to specific question

**Registration (in BaseViewModel):**
```csharp
WeakReferenceMessenger.Default.Register<LanguageChangedMessage>(this, (r, m) =>
{
    InitAsync();  // ⚠️ Called without await
});
```

**⚠️ Issue:** No explicit unregistration (WeakReference prevents hard leak, but callbacks can still fire on disposed objects).

---

## External Dependencies

### NuGet Packages

| Package | Version | Purpose |
|---------|---------|---------|
| CommunityToolkit.Mvvm | 8.4.0 | MVVM framework (ObservableObject, RelayCommand) |
| CommunityToolkit.Maui | 12.1.0 | UI toolkit extensions |
| sqlite-net-pcl | 1.7.335 | SQLite ORM |
| SQLitePCLRaw.bundle_e_sqlite3 | 2.1.11 | SQLite native bindings |
| Microsoft.Extensions.Http | 9.0.6 | HttpClient factory |
| Newtonsoft.Json | 13.0.3 | ⚠️ Installed but not used |
| System.Text.Json | (built-in) | JSON serialization (⚠️ **CRITICAL:** needs source gen) |
| Microsoft.ApplicationInsights | 2.23.0 | Telemetry/analytics |

### Platform APIs

| API | Purpose | Platform |
|-----|---------|----------|
| `Shell` | Navigation framework | MAUI |
| `Preferences` | Settings storage | MAUI |
| `FileSystem` | App data directory | MAUI |
| `Connectivity` | Network status check | MAUI |
| `Email` | Compose email with results | MAUI |
| `Launcher` | Open external URLs (planned) | MAUI |
| `DeviceInfo` | Detect tablet vs phone, idiom | MAUI |
| `AppInfo` | App version, name | MAUI |

---

## Data Flow Diagrams

### Startup Flow

```
App Launch
    ↓
MauiProgram.CreateMauiApp()
    ↓
App.CreateWindow() → AppShell
    ↓
SplashPage.OnAppearing()
    ↓
Task.Delay(4000)  ⚠️ Async void
    ↓
Navigate to WelcomePage
```

### Database Sync Flow

```
App Startup
    ↓
SplashViewModel.InitAsync() (empty in current code)
    ↓
[Manual refresh or version check]
    ↓
DatabaseService.EnsureDatabaseUpToDate()
    ↓
Compare local version to Firebase version
    ↓
If newer: RefreshDatabaseAsync()
    ├─→ UrlService.GetFullDatabaseAsync()
    ├─→ HTTP GET /.json from Firebase
    ├─→ JsonSerializer.Deserialize<RootModel>()  ⚠️ CRITICAL
    ├─→ Drop content tables
    ├─→ Recreate tables from RootModel
    └─→ Update DatabaseInfo.Version
```

### Survey Flow

```
WelcomePage
    ↓
[Start Survey] → NavigateAsync(SurveyPage)
    ↓
SurveyViewModel.InitAsync()
    ├─→ Load questions from SQLite
    ├─→ Shuffle randomly
    ├─→ Apply debug filters (if DEBUG)
    └─→ Create QuestionViewModel for each
    ↓
User answers questions
    ↓
[Submit] → SurveyViewModel.SubmitSurveyAsync()
    ├─→ Calculate scores per gift
    ├─→ Create SurveyResult
    ├─→ await RankGiftsAsync()  (should await!)
    ├─→ Save to SQLite
    └─→ NavigateAsync(ResultsPage, result)
    ↓
ResultsViewModel receives QueryProperty
    ├─→ OnUserGiftResultChanged()
    ├─→ _ = value.RankGiftsAsync()  ⚠️ Fire-and-forget
    └─→ _ = LoadUserGiftResultAsync()  ⚠️ Race
    ↓
Display ranked results
    ↓
[Continue] → NavigateAsync(SendPage, result)
    ↓
User enters name/email
    ↓
[Send] → EmailService.SendEmailAsync()
    ├─→ Check !result.IsRanked
    ├─→ _ = result.RankGiftsAsync()  ⚠️ Fire-and-forget
    ├─→ GenerateHtmlEmail()  (may read unranked data)
    └─→ Email.Default.ComposeAsync()
```

---

## Platform-Specific Code

### Android

**File:** `Platforms/Android/MainActivity.cs`
```csharp
[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ...
)]
public class MainActivity : MauiAppCompatActivity { }
```

**Manifest:** `Platforms/Android/AndroidManifest.xml`
- Min SDK: 23 (Android 6.0)
- Target SDK: 35 (Android 15)
- Permissions: `INTERNET`, `ACCESS_NETWORK_STATE`

### iOS

**File:** `Platforms/iOS/AppDelegate.cs`
```csharp
public override bool FinishedLaunching(UIApplication app, NSDictionary options)
{
    UIApplication.SharedApplication.StatusBarHidden = true;  // ⚠️ Deprecated
    return base.FinishedLaunching(app, options);
}
```

**Info.plist:**
- Min iOS: 15.0
- Bundle ID: `com.launchpaddevs.spiritualgiftssurvey`
- Version: 1.0.4 (build 4)

---

## Configuration Management

### Debug vs Release

**Firebase Environment:**
```csharp
#if DEBUG
    var baseUrl = "https://sgt-dev-b29c8-default-rtdb.firebaseio.com/";
#else
    var baseUrl = "https://sgt-prod-691ce-default-rtdb.firebaseio.com/";
#endif
```

**Debug Helpers:**
- `DebugHelper.ApplyDebugQuestionFilters()` — limit questions for faster testing
- Debug preferences (see `AppConstants`):
  - `DebugTotalTopicsKey`
  - `DebugQuestionsPerTopicKey`
  - `DebugAllowUnansweredQuestionsKey`
  - `DebugTotalUnansweredQuestionsKey`
  - `DebugOptionsEnabledKey`

### App Configuration

**No `appsettings.json`** — configuration is compiled or stored in Preferences.

**Preferences Keys:**
- `CurrentLanguageCode` — selected language
- `CurrentLanguageDisplayName` — display name
- `ReportingEmailsKey` — BCC email list (JSON serialized)
- Debug keys (see above)

---

## Security Considerations

### Data Storage

- **SQLite:** Unencrypted local database in app data directory
  - ✅ Sandboxed per-app (OS-level protection)
  - ❌ No encryption at rest
  - ⚠️ User survey results stored indefinitely

- **Preferences:** Unencrypted key-value storage
  - User email addresses stored (BCC list)
  - Language preferences

### Network

- **Firebase RTDB:** HTTPS only
- **No authentication:** Public read access to Firebase database
  - ✅ Read-only from app
  - ⚠️ No rate limiting
  - ⚠️ Database URL exposed in APK/IPA

### Email

- Uses device email client (not direct SMTP)
  - ✅ User controls sending
  - ✅ User sees recipients (To/BCC)
  - ⚠️ BCC list visible in settings (transparency)

---

## Performance Characteristics

### Startup Time

- Splash screen: 4 seconds (hardcoded delay)
- Database check: < 1 second (if no refresh needed)
- **First launch:** 5-10 seconds (download + parse Firebase JSON)

### Survey Load Time

- 250 questions × shuffle × ViewModel creation
- Uses `await Task.Yield()` every 5 items to keep UI responsive
- **Load time:** 1-2 seconds on modern devices

### Ranking Algorithm

- Complexity: O(n log n) where n = number of gifts (typically 20-30)
- Runs on background thread via `Task.Run()`
- **⚠️ Issue:** Mutates shared state without synchronization

---

## Known Architectural Limitations

1. **Service Locator Anti-pattern**
   - `IAggregatedServices` hides dependencies
   - Hard to test ViewModels in isolation
   - Violates Dependency Inversion Principle

2. **Singleton ViewModels**
   - State persists between navigations
   - Hard to isolate for testing
   - Can cause stale data issues

3. **Async Lifecycle Mismatches**
   - `InitAsync()` called without await
   - Fire-and-forget patterns throughout
   - Race conditions between lifecycle methods

4. **No Abstraction for Static Dependencies**
   - `Shell.Current` used directly
   - `Preferences.Default` mixed with `IPreferences`
   - `WeakReferenceMessenger.Default` used directly

5. **Mixed Sync/Async Data Access**
   - Some methods use `SQLiteConnection` (sync)
   - Others use `SQLiteAsyncConnection` (async)
   - Inconsistent pattern

6. **No Separation of Concerns in DatabaseService**
   - Acts as repository + sync engine + cache manager + schema migrator
   - Single Responsibility Principle violated

---

## Deployment Architecture

### Build Outputs

- **Android:** APK (debug), AAB (release for Play Store)
- **iOS:** IPA (requires Mac build agent, code signing)

### Release Pipeline

**⚠️ No CI/CD configured** — manual builds only.

**Manual Steps:**
1. Update version numbers (csproj, AndroidManifest, Info.plist)
2. Switch Firebase URL to prod (compile-time flag)
3. Build Release configuration
4. Test on physical devices
5. Sign APK/IPA
6. Upload to stores

---

## Monitoring & Observability

### Analytics

- **Microsoft ApplicationInsights** (TelemetryClient)
- Events tracked:
  - `NavBackFailure` — navigation errors
  - (Limited telemetry currently implemented)

### Logging

- `System.Diagnostics.Debug.WriteLine()` throughout
- **⚠️ No structured logging**
- **⚠️ No crash reporting configured**

### Error Handling

- Most exceptions caught and logged to Debug output
- User-facing errors show generic "Error" message (from `TitledException`)
- **⚠️ No retry logic for network failures**

---

## Scalability Considerations

### Current Limits

- **Questions:** 250+ per language (reasonable, no pagination needed)
- **Languages:** 5-10 expected (no performance concern)
- **Results:** Stored indefinitely (⚠️ potential storage growth)
  - No cleanup/archival strategy

### Firebase Data Size

- Full database JSON: ~500KB-1MB (acceptable for mobile)
- **⚠️ Downloaded on every refresh** (no delta sync)

---

## Extensibility Points

### Adding a New Language

1. Add translation data to Firebase
2. App auto-detects on next refresh
3. No code changes required ✅

### Adding a New Question

1. Add to Firebase `Questions` collection
2. App auto-loads on refresh
3. No code changes required ✅

### Adding a New Gift

1. Add to `Gifts` enum ❌ Requires code change
2. Add questions + descriptions to Firebase
3. Recompile app

### Adding a New Page

1. Create `Views/{Feature}/{Feature}Page.xaml[.cs]`
2. Create `Views/{Feature}/{Feature}ViewModel.cs`
3. Add route to `Routing/Routes.cs`
4. Register in `MauiProgram.RegisterViewModels()`
5. Add `<ShellContent>` to `AppShell.xaml`

**⚠️ 5-step process** — well-documented in `.github/copilot-instructions.md`

---

## Related Documentation

- **[.github/copilot-instructions.md](../.github/copilot-instructions.md)** — Detailed MVVM patterns, conventions, examples
- **[docs/testing-assessment.md](testing-assessment.md)** — Test coverage gaps and recommendations
- **[docs/release-build-findings.md](release-build-findings.md)** — CRITICAL release-specific bugs and fixes
- **[docs/technical-debt-register.md](technical-debt-register.md)** — Known issues and improvement opportunities
- **[docs/existing-documentation-audit.md](existing-documentation-audit.md)** — Documentation inventory

---

## Revision History

| Date | Author | Changes |
|------|--------|---------|
| 2026-05-28 | Senior Staff Engineer | Initial architecture documentation |
