# iOS Workflow - GitHub Runner Update Notice

## Current Status (as of May 31, 2026)

The iOS workflow is **temporarily unable to build** due to a version mismatch between:
- **.NET 10 iOS SDK** (requires Xcode 27.x)
- **GitHub Actions macOS runners** (currently have Xcode 26.4)

## Resolution Timeline

**GitHub Actions will update macOS 26 runners to Xcode 26.5+ starting June 8, 2026.**

- **Start Date**: Monday, June 8, 2026
- **Completion**: 2-4 days (expected June 8-12, 2026)
- **Affected Runners**: `macos-26`, `macos-26-arm64`, `macos-14` (if updated)

### Source
GitHub Actions announcement: [Runner Images Issue #14108](https://github.com/actions/runner-images/issues/14108)

## What to Do

### Option 1: Wait for Runner Update (Recommended)
1. Wait until **June 12, 2026** for the rollout to complete
2. Run the iOS workflow - it should work without changes
3. Builds will produce signed IPA files ready for Transporter upload

### Option 2: Build Locally
Until the runners are updated, you can:
1. Build iOS locally on your Mac with Xcode 26.5+ installed
2. Use the same signing certificates and provisioning profiles
3. Upload the IPA via Transporter manually

```bash
# Build locally on Mac
dotnet publish SpiritualGiftsSurvey.csproj \
  -f net10.0-ios \
  -c Release \
  -p:ArchiveOnBuild=true \
  -p:RuntimeIdentifier=ios-arm64 \
  -p:CodesignKey="Apple Distribution: Your Name (TEAM_ID)" \
  -p:CodesignProvision="Your Provisioning Profile Name"
```

## After June 12, 2026

The iOS workflow will work normally:
1. ✅ Run unit tests
2. ✅ Build signed IPA for production
3. ✅ Upload IPA to GitHub Actions artifacts
4. ✅ Download and upload to TestFlight via Transporter

No changes to the workflow will be needed - the runner update will resolve the issue automatically.

## Current Workaround Status

We tried several workarounds:
- ❌ Pinning to older .NET versions (SDK mismatch)
- ❌ Using workload manifest rollbacks (manifest URLs changed)
- ❌ Switching between macos-14 and macos-15 runners (neither has Xcode 26.5+)

**Conclusion**: Waiting for the runner update is the cleanest solution.

## Notes

- **Android workflow**: ✅ Working perfectly - no issues
- **Project upgraded to .NET 10**: ✅ Complete
- **iOS workflow ready**: ✅ Just needs GitHub runner update
