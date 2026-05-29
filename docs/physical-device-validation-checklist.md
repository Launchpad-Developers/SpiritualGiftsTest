# Physical Device Release Validation Checklist

## PRE-MIGRATION VALIDATION PHASE

This checklist must be completed on **physical devices** before proceeding to .NET 10 migration.

**IMPORTANT:** Emulators/simulators are NOT sufficient for Release validation. Many Release-only issues only appear on physical devices due to:
- Stricter AOT compilation (iOS)
- More aggressive linker behavior
- Real-world network conditions
- Actual device permissions
- True app lifecycle timing
- Device-specific performance characteristics

---

## ANDROID PHYSICAL DEVICE VALIDATION

### Device Requirements
- [ ] Physical Android device (API 23+)
- [ ] Installed via Release APK (not Debug)
- [ ] Device NOT connected to debugger

### Build Validation
- [ ] Release APK builds successfully
- [ ] APK installs on device without errors
- [ ] App launches successfully
- [ ] No immediate crashes on startup
- [ ] Loading screen displays correctly

### Firebase Sync
- [ ] Database version check succeeds
- [ ] Local database is created/updated
- [ ] Questions load successfully
- [ ] Translations load successfully
- [ ] Gift descriptions load successfully

### Survey Flow
- [ ] All 250+ questions display correctly
- [ ] Question navigation works (Next/Previous)
- [ ] Answer selection persists
- [ ] Survey progress indicator updates
- [ ] Survey completes successfully
- [ ] No crashes during question flow
- [ ] No UI freezes during survey

### Ranking Logic
- [ ] Survey results calculate correctly
- [ ] RankGiftsAsync completes without crash
- [ ] Primary gifts assigned correctly
- [ ] Secondary gifts assigned correctly
- [ ] Ties handled correctly
- [ ] No ranking corruption under rapid completion

### Email Generation
- [ ] Email generation succeeds
- [ ] Ranked gifts appear in email body
- [ ] Email includes all selected gifts
- [ ] Email sharing works correctly
- [ ] No crashes during email generation

### Localization
- [ ] Language switching works
- [ ] All UI strings translate correctly
- [ ] RTL layout works (Arabic)
- [ ] Questions display in selected language
- [ ] Gift descriptions display in selected language
- [ ] No missing translation keys

### App Lifecycle
- [ ] Suspend/resume works correctly
- [ ] App state persists across suspend
- [ ] No crashes on resume
- [ ] Background/foreground transitions smooth
- [ ] No memory leaks on repeated suspend/resume

### Performance
- [ ] App starts within acceptable time (<3s)
- [ ] UI remains responsive during survey
- [ ] No noticeable lag during language switching
- [ ] Ranking completes quickly (<2s)
- [ ] Email generation completes quickly (<1s)

### Release-Specific Checks
- [ ] JSON deserialization works (CRITICAL-2 fix)
- [ ] Ranking does not corrupt state (HIGH-5 fix)
- [ ] No async void crashes (CRITICAL-3 fix)
- [ ] Language switching deterministic (HIGH-2 fix)
- [ ] No lifecycle overlap races (HIGH-3 fix)

### Known Issues
Document any issues found:
```
ISSUE: [Description]
SEVERITY: [Critical | High | Medium | Low]
REPRO STEPS: [1. ..., 2. ...]
```

---

## iOS PHYSICAL DEVICE VALIDATION

### Device Requirements
- [ ] Physical iOS device (iOS 15.0+)
- [ ] Installed via Release IPA or TestFlight
- [ ] Device NOT connected to Xcode debugger

### Build Validation
- [ ] Release build succeeds
- [ ] IPA installs on device without errors
- [ ] App launches successfully
- [ ] No immediate crashes on startup
- [ ] Loading screen displays correctly
- [ ] No codesigning issues

### Firebase Sync
- [ ] Database version check succeeds
- [ ] Local database is created/updated
- [ ] Questions load successfully
- [ ] Translations load successfully
- [ ] Gift descriptions load successfully

### Survey Flow
- [ ] All 250+ questions display correctly
- [ ] Question navigation works (Next/Previous)
- [ ] Answer selection persists
- [ ] Survey progress indicator updates
- [ ] Survey completes successfully
- [ ] No crashes during question flow
- [ ] No UI freezes during survey

### Ranking Logic
- [ ] Survey results calculate correctly
- [ ] RankGiftsAsync completes without crash
- [ ] Primary gifts assigned correctly
- [ ] Secondary gifts assigned correctly
- [ ] Ties handled correctly
- [ ] No ranking corruption under rapid completion

### Email Generation
- [ ] Email generation succeeds
- [ ] Ranked gifts appear in email body
- [ ] Email includes all selected gifts
- [ ] Email sharing works correctly
- [ ] No crashes during email generation

### Localization
- [ ] Language switching works
- [ ] All UI strings translate correctly
- [ ] RTL layout works (Arabic)
- [ ] Questions display in selected language
- [ ] Gift descriptions display in selected language
- [ ] No missing translation keys

### App Lifecycle
- [ ] Suspend/resume works correctly
- [ ] App state persists across suspend
- [ ] No crashes on resume
- [ ] Background/foreground transitions smooth
- [ ] No memory leaks on repeated suspend/resume

### Performance
- [ ] App starts within acceptable time (<3s)
- [ ] UI remains responsive during survey
- [ ] No noticeable lag during language switching
- [ ] Ranking completes quickly (<2s)
- [ ] Email generation completes quickly (<1s)

### iOS-Specific Checks
- [ ] NativeAOT compilation succeeds
- [ ] JSON source generation works correctly (no reflection)
- [ ] No linker/trim errors at runtime
- [ ] Async lifecycle behavior deterministic
- [ ] No MainThread dispatch deadlocks

### Release-Specific Checks
- [ ] JSON deserialization works (CRITICAL-2 fix)
- [ ] Ranking does not corrupt state (HIGH-5 fix)
- [ ] No async void crashes (CRITICAL-3 fix)
- [ ] Language switching deterministic (HIGH-2 fix)
- [ ] No lifecycle overlap races (HIGH-3 fix)

### Known Issues
Document any issues found:
```
ISSUE: [Description]
SEVERITY: [Critical | High | Medium | Low]
REPRO STEPS: [1. ..., 2. ...]
```

---

## VALIDATION SIGN-OFF

### Android Release
- **Tested by:** ___________________________
- **Device Model:** ___________________________
- **Android Version:** ___________________________
- **Date:** ___________________________
- **Result:** [ ] PASS | [ ] FAIL | [ ] BLOCKED

### iOS Release
- **Tested by:** ___________________________
- **Device Model:** ___________________________
- **iOS Version:** ___________________________
- **Date:** ___________________________
- **Result:** [ ] PASS | [ ] FAIL | [ ] BLOCKED

---

## PRE-MIGRATION EXIT CRITERIA STATUS

ONLY proceed to .NET 10 migration when ALL criteria are met:

- [ ] ✅ Release builds stable on Android (physical device validation PASS)
- [ ] ✅ Release builds stable on iOS (physical device validation PASS)
- [ ] ✅ JSON source generation implemented (CRITICAL-2)
- [ ] ✅ Trimming configuration validated (CRITICAL-1)
- [ ] ✅ Async lifecycle races stabilized (HIGH-2/3/4)
- [ ] ✅ Fire-and-forget patterns eliminated (HIGH-1)
- [ ] ✅ No known Release-only crashes
- [ ] ✅ Critical regression tests passing
- [ ] ✅ Physical-device validation completed

**If ANY criterion is not met, DO NOT proceed to migration.**

---

## POST-VALIDATION NEXT STEPS

Once ALL validation criteria pass:

1. **Document findings** in `docs/release-build-findings.md`
2. **Update risk register** in `docs/technical-debt-register.md`
3. **Create checkpoint** summarizing PRE-MIGRATION stabilization completion
4. **Obtain approval** to proceed to Phase 5B (.NET 10 migration)

---

## ROLLBACK PLAN

If critical issues are discovered during physical device validation:

1. **HALT migration planning**
2. **Document issue** with full repro steps
3. **Classify root cause** (trimming, AOT, async, lifecycle, etc.)
4. **Implement targeted fix** following release-safe patterns
5. **Add regression test** to prevent recurrence
6. **Re-validate** on physical devices
7. **Only then** consider migration

**DO NOT proceed to migration with known Release blockers.**
