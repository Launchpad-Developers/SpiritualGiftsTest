# Package Compatibility Matrix — .NET 10 Migration

**Assessment Date:** January 2025  
**Current Target:** .NET 9.0 (net9.0-android, net9.0-ios)  
**Migration Target:** .NET 10.0 (net10.0-android, net10.0-ios)

---

## Summary

| Status | Count | Packages |
|--------|-------|----------|
| ✅ Compatible (no changes) | 3 | CommunityToolkit.Mvvm, sqlite-net-pcl, SQLitePCLRaw |
| 🔄 Update Required | 5 | MAUI, Microsoft.Extensions.*, ApplicationInsights |
| ❌ Remove | 1 | Newtonsoft.Json (unused) |

---

## Package Details

### Microsoft.Maui.Controls — 9.0.81
**Status:** 🔄 Update Required  
**Target Version:** 10.x (exact version TBD)  
**Risk Level:** MEDIUM  
**.NET 10 Compatible:** Yes

**Migration Impact:**
- Major version bump (9.x → 10.x)
- Expect API changes, deprecations, and breaking changes
- Review MAUI 10 migration guide when released
- Shell navigation may have lifecycle improvements or regressions
- Platform handlers may change

**Action Required:**
1. Monitor MAUI 10 stable release
2. Review migration guide and breaking changes
3. Update package reference:
   ```xml
   <PackageReference Include="Microsoft.Maui.Controls" Version="10.x.x" />
   ```
4. Update workloads: `dotnet workload update`
5. Test all platform-specific code
6. Test Shell navigation timing
7. Regression test critical paths

**Testing Priority:** CRITICAL  
**Estimated Effort:** 0.5 day (update + testing)

---

### CommunityToolkit.Maui — 12.1.0
**Status:** 🔄 Minor Update Recommended  
**Target Version:** Latest 12.x (check for updates)  
**Risk Level:** LOW  
**.NET 10 Compatible:** Yes

**Migration Impact:**
- CommunityToolkit.Maui is MAUI-version-aware
- May release 13.x for MAUI 10 compatibility
- Current patterns (behaviors, popups, extensions) should remain compatible
- No known breaking changes

**Action Required:**
1. Check for latest version compatible with MAUI 10
2. Update if new version available
3. Test Popup usage (if any)
4. Test behaviors and extensions

**Testing Priority:** MEDIUM  
**Estimated Effort:** 0.25 day

---

### CommunityToolkit.Mvvm — 8.4.0
**Status:** ✅ No Update Required  
**Target Version:** 8.4.0 (or latest 8.x)  
**Risk Level:** LOW  
**.NET 10 Compatible:** Yes

**Migration Impact:**
- Framework-agnostic library
- Works with .NET Standard 2.0+
- ObservableObject, RelayCommand, Messaging patterns unchanged
- No expected breaking changes

**Action Required:**
- None required (may optionally update to latest 8.x for bug fixes)

**Testing Priority:** LOW  
**Estimated Effort:** 0 days

---

### sqlite-net-pcl — 1.7.335
**Status:** ✅ No Update Required  
**Target Version:** 1.7.335 (or latest)  
**Risk Level:** LOW  
**.NET 10 Compatible:** Yes

**Migration Impact:**
- .NET Standard 2.0 library
- Stable, mature package
- No .NET version dependencies
- No expected breaking changes

**Action Required:**
- None required (may optionally update for bug fixes)

**Testing Priority:** LOW  
**Estimated Effort:** 0 days

---

### SQLitePCLRaw.bundle_e_sqlite3 — 2.1.11
**Status:** ✅ Likely Compatible  
**Target Version:** 2.1.11 (or latest 2.x)  
**Risk Level:** LOW  
**.NET 10 Compatible:** Likely Yes

**Migration Impact:**
- Native SQLite bindings
- May need update for new platform versions (if MAUI 10 bumps iOS/Android minimums)
- Otherwise stable

**Action Required:**
1. Check for updates after MAUI 10 release
2. Test database operations on both platforms

**Testing Priority:** MEDIUM  
**Estimated Effort:** 0.25 day

---

### Microsoft.ApplicationInsights — 2.23.0
**Status:** 🔄 Update Required (or Replace)  
**Target Version:** Latest 2.x (or remove)  
**Risk Level:** MEDIUM  
**.NET 10 Compatible:** TBD

**Migration Impact:**
- May not be officially supported on .NET 10 MAUI
- Consider migrating to MAUI-native telemetry or alternative
- Used in: `Services/Analytics.cs`, various ViewModels

**Action Required:**
1. Verify .NET 10 compatibility with Microsoft
2. **Option A:** Update to latest ApplicationInsights
3. **Option B:** Replace with MAUI-native telemetry (e.g., Microsoft.Extensions.Logging + Application Insights Exporter)
4. **Option C:** Replace with cross-platform analytics (e.g., Firebase Analytics, AppCenter)

**Recommendation:**
- **Short-term:** Update to latest 2.x if compatible
- **Long-term:** Consider MAUI-native telemetry solution

**Testing Priority:** MEDIUM  
**Estimated Effort:** 0.5-1 day (depending on option)

---

### Microsoft.Extensions.Http — 9.0.6
**Status:** 🔄 Update Required  
**Target Version:** 10.0.x  
**Risk Level:** LOW  
**.NET 10 Compatible:** Yes

**Migration Impact:**
- Microsoft.Extensions.* packages follow .NET versioning
- Will auto-update to 10.0.x with .NET 10
- No breaking changes expected (follows semver)

**Action Required:**
1. Update package reference:
   ```xml
   <PackageReference Include="Microsoft.Extensions.Http" Version="10.0.*" />
   ```
2. Verify HttpClient usage still works

**Testing Priority:** LOW  
**Estimated Effort:** 0.1 day

---

### Microsoft.Extensions.Configuration.Json — 9.0.6
**Status:** 🔄 Update Required  
**Target Version:** 10.0.x  
**Risk Level:** LOW  
**.NET 10 Compatible:** Yes

**Migration Impact:**
- Will auto-update to 10.0.x with .NET 10
- No breaking changes expected

**Action Required:**
1. Update package reference:
   ```xml
   <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="10.0.*" />
   ```

**Testing Priority:** LOW  
**Estimated Effort:** 0.1 day

---

### Microsoft.Extensions.Logging.Debug — 9.0.6
**Status:** 🔄 Update Required  
**Target Version:** 10.0.x  
**Risk Level:** LOW  
**.NET 10 Compatible:** Yes

**Migration Impact:**
- Will auto-update to 10.0.x with .NET 10
- No breaking changes expected

**Action Required:**
1. Update package reference:
   ```xml
   <PackageReference Include="Microsoft.Extensions.Logging.Debug" Version="10.0.*" />
   ```

**Testing Priority:** LOW  
**Estimated Effort:** 0.1 day

---

### Newtonsoft.Json — 13.0.3
**Status:** ❌ Remove (Unused)  
**Target Version:** N/A  
**Risk Level:** LOW  
**.NET 10 Compatible:** Yes (but not used)

**Migration Impact:**
- Package is installed but NOT used in the codebase
- Only reference found: unused `using Newtonsoft.Json.Linq;` in `Converters/ItemMarginConverter.cs:1`
- Application uses System.Text.Json instead
- Dead dependency

**Action Required:**
1. Remove package reference from `.csproj`:
   ```xml
   <!-- DELETE THIS LINE -->
   <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
   ```
2. Remove unused using statement from `ItemMarginConverter.cs`
3. Verify no runtime errors (should be none)

**Testing Priority:** LOW  
**Estimated Effort:** 0.1 day

**Recommendation:** Remove DURING migration to reduce package footprint

---

## Migration Checklist

### Before Migration
- [ ] Verify MAUI 10 stable release available
- [ ] Review MAUI 10 migration guide
- [ ] Review .NET 10 breaking changes
- [ ] Check ApplicationInsights .NET 10 compatibility

### During Migration
- [ ] Update `SpiritualGiftsSurvey.csproj` TFM:
  ```xml
  <TargetFrameworks>net10.0-android;net10.0-ios</TargetFrameworks>
  ```
- [ ] Update MAUI to 10.x:
  ```xml
  <PackageReference Include="Microsoft.Maui.Controls" Version="10.x.x" />
  ```
- [ ] Update Microsoft.Extensions.* to 10.0.*:
  ```xml
  <PackageReference Include="Microsoft.Extensions.Http" Version="10.0.*" />
  <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="10.0.*" />
  <PackageReference Include="Microsoft.Extensions.Logging.Debug" Version="10.0.*" />
  ```
- [ ] Remove Newtonsoft.Json:
  ```xml
  <!-- Remove this package reference -->
  ```
- [ ] Update/verify ApplicationInsights compatibility
- [ ] Update .NET workloads:
  ```bash
  dotnet workload update
  ```
- [ ] Clean and rebuild:
  ```bash
  dotnet clean
  dotnet build -c Release
  ```

### Testing After Migration
- [ ] Build succeeds for Android (Release)
- [ ] Build succeeds for iOS (Release)
- [ ] Deploy to Android physical device (Release)
- [ ] Deploy to iOS physical device (Release)
- [ ] Smoke test: Welcome → Survey → Results → Email
- [ ] Test database sync from Firebase
- [ ] Test language switching
- [ ] Test survey completion (all questions)
- [ ] Test ranking algorithm
- [ ] Test email generation
- [ ] Performance regression check (startup time)

---

## Risk Mitigation

### High-Risk Packages
1. **Microsoft.Maui.Controls** (MAUI 9 → 10)
   - **Mitigation:** Thorough regression testing, review migration guide
   - **Rollback:** Revert TFM and package versions if critical issues found

2. **Microsoft.ApplicationInsights**
   - **Mitigation:** Verify compatibility early, have replacement ready
   - **Rollback:** Remove telemetry temporarily if blocking

### Testing Strategy
- **iOS Release Build:** MUST test on physical device (AOT-specific issues)
- **Android Release Build:** MUST test on physical device (linker issues)
- **Critical Paths:** Survey completion, email, database sync
- **Performance:** Baseline startup time before/after migration

---

## Package Update Order

Execute in this sequence to minimize risk:

1. **Pre-Migration** (on .NET 9)
   - Remove Newtonsoft.Json (verify builds still work)
   - Update CommunityToolkit.Maui to latest 12.x (optional)

2. **Migration** (.NET 9 → .NET 10)
   - Update TFM to net10.0-android;net10.0-ios
   - Update MAUI to 10.x
   - Update Microsoft.Extensions.* to 10.0.*
   - Update/verify ApplicationInsights

3. **Post-Migration** (on .NET 10)
   - Update CommunityToolkit packages if new versions released
   - Update SQLitePCLRaw if needed for platform support

---

## Dependency Graph

```
SpiritualGiftsSurvey
├── Microsoft.Maui.Controls 9.0.81 → 10.x ⚠️ BREAKING
│   └── (brings in MAUI platform packages)
├── CommunityToolkit.Maui 12.1.0 → 12.x/13.x ℹ️ CHECK
├── CommunityToolkit.Mvvm 8.4.0 ✅ OK
├── sqlite-net-pcl 1.7.335 ✅ OK
│   └── SQLitePCLRaw.bundle_e_sqlite3 2.1.11 ✅ OK
├── Microsoft.ApplicationInsights 2.23.0 → ??? ⚠️ VERIFY
├── Microsoft.Extensions.Http 9.0.6 → 10.0.* ✅ AUTO
├── Microsoft.Extensions.Configuration.Json 9.0.6 → 10.0.* ✅ AUTO
├── Microsoft.Extensions.Logging.Debug 9.0.6 → 10.0.* ✅ AUTO
└── Newtonsoft.Json 13.0.3 ❌ REMOVE
```

**Legend:**
- ✅ OK — No changes needed
- ℹ️ CHECK — Verify latest version
- ⚠️ BREAKING — Review migration guide
- ⚠️ VERIFY — Check compatibility
- ❌ REMOVE — Unused dependency

---

## Recommendations

1. **DO NOT migrate until:**
   - MAUI 10 stable release available
   - Migration guide reviewed
   - PRE-MIGRATION blockers resolved (trimming, JSON source-gen, async fixes)

2. **Immediate Actions:**
   - Remove Newtonsoft.Json (can be done now on .NET 9)
   - Monitor MAUI 10 announcements
   - Plan ApplicationInsights replacement if needed

3. **Testing Strategy:**
   - MUST test Release builds on physical devices
   - Focus on iOS (most sensitive to trimming/AOT)
   - Regression test all critical paths

---

**Document Owner:** Architecture & Modernization Initiative  
**Last Updated:** January 2025  
**Next Review:** When MAUI 10 stable releases
