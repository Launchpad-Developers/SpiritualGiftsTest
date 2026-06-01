# Deployment Quick Start Guide

This guide provides a high-level overview of deploying the Spiritual Gifts Survey app to both app stores using GitHub Actions.

## Overview

✅ **GitHub Actions workflows created** for both iOS and Android  
✅ **GitHub Secrets** used for secure credential storage (similar to Azure DevOps)  
✅ **Manual deployment** to stores (workflows build packages, you upload)

## What You Need

### For Android
- [ ] Google Play Developer Account ($25 one-time)
- [ ] Android release keystore file
- [ ] 4 GitHub Secrets configured

### For iOS
- [ ] Apple Developer Account ($99/year)
- [ ] Mac computer with Xcode
- [ ] Distribution certificate and provisioning profile
- [ ] 5-7 GitHub Secrets configured

## Deployment Process Flow

```
1. Make code changes
2. Run GitHub Action (manual or via git tag)
3. Workflow builds signed package
4. Download artifact from GitHub
5. Upload to App Store / Play Store manually
6. Submit for review
```

## GitHub Secrets vs Azure DevOps

If you're familiar with Azure DevOps:

| Azure DevOps | GitHub Actions | Purpose |
|--------------|----------------|---------|
| Variable Groups | Repository Secrets | Store sensitive values |
| Secure Files | Base64-encoded secrets | Store certificates/keystores |
| Service Connections | GitHub Secrets | API authentication |
| Pipeline YAML | Workflow YAML | Build definition |

**How to access GitHub Secrets:**
- Repository → Settings → Secrets and variables → Actions
- Values are encrypted and never shown in logs
- Only exposed to workflow runs

## Step-by-Step Setup (Summary)

### Android Setup (Detailed: `ANDROID_SETUP.md`)

1. **Create release keystore** (one-time)
   ```bash
   keytool -genkey -v -keystore release.keystore -alias spiritualgifts -keyalg RSA -keysize 2048 -validity 10000
   ```

2. **Convert to base64 and add 4 secrets to GitHub:**
   - `ANDROID_KEYSTORE_BASE64`
   - `ANDROID_KEYSTORE_PASSWORD`
   - `ANDROID_KEY_ALIAS`
   - `ANDROID_KEY_PASSWORD`

3. **Set up Google Play Console:**
   - Create app listing
   - Add screenshots, descriptions
   - Set content rating
   - Create internal testing track (recommended)

4. **Run workflow** → Download AAB → Upload to Play Console

**Time estimate:** 2-3 hours for first-time setup

### iOS Setup (Detailed: `IOS_SETUP.md`)

1. **Create Apple Developer assets** (requires Mac):
   - App ID in Developer Portal
   - Distribution certificate (export as .p12)
   - App Store provisioning profile

2. **Convert to base64 and add 5 secrets to GitHub:**
   - `IOS_DISTRIBUTION_CERT_P12_BASE64`
   - `IOS_DISTRIBUTION_CERT_PASSWORD`
   - `IOS_PROVISIONING_PROFILE_BASE64`
   - `IOS_PROVISIONING_PROFILE_NAME`
   - `IOS_CODESIGN_KEY_NAME`

3. **Set up App Store Connect:**
   - Create app listing
   - Add screenshots (multiple sizes required)
   - Complete app privacy questionnaire
   - Set pricing and availability

4. **Run workflow** → Download IPA → Upload via Transporter → Submit for review

**Time estimate:** 3-5 hours for first-time setup

## Running the Workflows

### Option 1: Manual Trigger (Recommended)

1. Go to **Actions** tab in GitHub
2. Select workflow (Android or iOS)
3. Click **Run workflow**
4. Enter version name (e.g., `1.0.5`) and version/build number (e.g., `5`)
5. Click **Run workflow**

### Option 2: Automatic via Git Tag

```bash
git tag v1.0.5
git push origin v1.0.5
```

Both workflows will run automatically.

## Workflow Features

### Android Workflow (`android-release.yml`)
- ✅ Builds signed AAB (for Play Store)
- ✅ Builds signed APK (for testing)
- ✅ Automatically updates version in AndroidManifest.xml
- ✅ Uploads artifacts to GitHub
- ✅ Creates GitHub release (if triggered by tag)

### iOS Workflow (`ios-release.yml`)
- ✅ Builds signed IPA (for App Store)
- ✅ Automatically updates version in Info.plist
- ✅ Imports certificates and provisioning profiles
- ✅ Uploads artifact to GitHub
- ✅ Optional: Automatic TestFlight upload
- ✅ Creates GitHub release (if triggered by tag)

## Version Management

**IMPORTANT:** Version numbers must increment for each release.

### Android
- **Version Name:** User-facing (e.g., "1.0.5")
- **Version Code:** Integer that must increment (1, 2, 3, 4, 5...)

### iOS
- **Version:** User-facing (e.g., "1.0.5")
- **Build Number:** Integer that must increment (1, 2, 3, 4, 5...)

The workflows automatically update these values based on your input.

## Security Best Practices

### DO:
✅ Store keystores/certificates in a secure location (NOT in git)  
✅ Use strong passwords for keystores and certificates  
✅ Back up your Android keystore (losing it means you can't update the app!)  
✅ Use different keystores for debug and release  
✅ Limit access to GitHub Secrets to necessary team members  

### DON'T:
❌ Commit keystores, certificates, or passwords to git  
❌ Share keystore passwords via email or chat  
❌ Use the same password for all secrets  
❌ Skip testing before uploading to stores  

## Testing Before Production

### Android Testing Checklist
- [ ] Test APK on physical device before uploading AAB
- [ ] Test on different Android versions (API 23+)
- [ ] Use Internal Testing track before production
- [ ] Test offline functionality
- [ ] Verify all survey flows
- [ ] Check translations and RTL (Arabic)

### iOS Testing Checklist
- [ ] Test IPA on physical iPhone/iPad
- [ ] Test on different iOS versions (15.0+)
- [ ] Use TestFlight for beta testing
- [ ] Test offline functionality
- [ ] Verify all survey flows
- [ ] Check translations and RTL (Arabic)

## Common Issues and Solutions

### Android

**Issue:** Build fails with "Keystore not found"
- **Solution:** Check `ANDROID_KEYSTORE_BASE64` is set correctly
- Ensure base64 encoding didn't add line breaks

**Issue:** "Version code X has already been used"
- **Solution:** Increment version code to a number higher than any previous release

### iOS

**Issue:** "No code signing identity found"
- **Solution:** Verify certificate is correctly base64 encoded
- Check `IOS_CODESIGN_KEY_NAME` exactly matches certificate

**Issue:** "Provisioning profile doesn't include signing certificate"
- **Solution:** Regenerate provisioning profile with correct certificate
- Update `IOS_PROVISIONING_PROFILE_BASE64` secret

### Both Platforms

**Issue:** Workflow runs but artifacts are empty
- **Solution:** Check workflow logs for build errors
- Ensure .NET MAUI workload installed correctly

**Issue:** App crashes after deployment
- **Solution:** Test locally in Release configuration first
- Check Application Insights for crash logs

## Next Steps After First Deployment

1. **Monitor reviews** - Respond to user feedback
2. **Set up analytics** - Track usage via Application Insights
3. **Plan updates** - Bug fixes and new features
4. **Beta testing** - Use TestFlight (iOS) and Internal Testing (Android)
5. **Localization** - Add more languages as needed

## Getting Help

### Documentation
- [Detailed Android Setup](./ANDROID_SETUP.md)
- [Detailed iOS Setup](./IOS_SETUP.md)
- [Workflow README](../../.github/workflows/README.md)

### External Resources
- [Google Play Console Help](https://support.google.com/googleplay/android-developer)
- [App Store Connect Help](https://developer.apple.com/support/app-store-connect/)
- [GitHub Actions Documentation](https://docs.github.com/en/actions)

### Troubleshooting
1. Check workflow logs in GitHub Actions tab
2. Review detailed setup guides for your platform
3. Search GitHub Issues in .NET MAUI repository
4. Check Apple/Google developer forums

## Estimated Timeline

| Task | Time Required |
|------|---------------|
| Google Play Developer account setup | 1 hour |
| Android keystore creation & GitHub setup | 30 minutes |
| Google Play Console app listing | 1-2 hours |
| Apple Developer account enrollment | 24-48 hours (approval) |
| iOS certificates & provisioning (on Mac) | 1-2 hours |
| App Store Connect app listing | 2-3 hours |
| First Android workflow run | 10-15 minutes |
| First iOS workflow run | 15-20 minutes |
| **Total (excluding Apple approval)** | **6-9 hours** |

## Summary

You now have:
1. ✅ Two GitHub Actions workflows ready to use
2. ✅ GitHub Secrets for secure credential storage (like Azure DevOps)
3. ✅ Comprehensive setup guides for both platforms
4. ✅ Automatic version management
5. ✅ Build artifacts ready to upload to stores

**Next Action:** Follow the detailed setup guide for your target platform:
- **Android:** See `docs/deployment/ANDROID_SETUP.md`
- **iOS:** See `docs/deployment/IOS_SETUP.md`
