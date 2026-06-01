# CI/CD Pipelines Documentation

This directory contains GitHub Actions workflows for building and deploying the Spiritual Gifts Survey app to both iOS App Store and Google Play Store.

## Workflows

### 1. `android-release.yml` - Android Release Build
Builds a signed AAB (Android App Bundle) and APK for Google Play Store distribution.

**Triggers:**
- Manual workflow dispatch with version input
- Git tags matching `v*.*.*` pattern

**Outputs:**
- Signed AAB file (for Play Store submission)
- Signed APK file (for manual testing)

### 2. `ios-release.yml` - iOS Release Build
Builds a signed IPA for App Store distribution.

**Triggers:**
- Manual workflow dispatch with version input
- Git tags matching `v*.*.*` pattern

**Outputs:**
- Signed IPA file (for App Store submission)
- Optional: Automatic TestFlight upload

## Required GitHub Secrets

### Android Secrets

| Secret Name | Description | How to Obtain |
|-------------|-------------|---------------|
| `ANDROID_KEYSTORE_BASE64` | Base64-encoded release keystore file | See Android setup instructions below |
| `ANDROID_KEYSTORE_PASSWORD` | Keystore password | Password you set when creating the keystore |
| `ANDROID_KEY_ALIAS` | Key alias name | Alias you set when creating the keystore |
| `ANDROID_KEY_PASSWORD` | Key password | Password for the key alias |

### iOS Secrets

| Secret Name | Description | How to Obtain |
|-------------|-------------|---------------|
| `IOS_DISTRIBUTION_CERT_P12_BASE64` | Base64-encoded distribution certificate (.p12) | See iOS setup instructions below |
| `IOS_DISTRIBUTION_CERT_PASSWORD` | Certificate password | Password you set when exporting the certificate |
| `IOS_PROVISIONING_PROFILE_BASE64` | Base64-encoded provisioning profile | See iOS setup instructions below |
| `IOS_PROVISIONING_PROFILE_NAME` | Provisioning profile name | Name of your provisioning profile |
| `IOS_CODESIGN_KEY_NAME` | Code signing identity name | Usually "iPhone Distribution: Your Name (Team ID)" |
| `APP_STORE_CONNECT_API_KEY_ID` | (Optional) App Store Connect API Key ID | For automatic TestFlight upload |
| `APP_STORE_CONNECT_ISSUER_ID` | (Optional) App Store Connect Issuer ID | For automatic TestFlight upload |

## How to Add Secrets to GitHub

1. Go to your GitHub repository
2. Navigate to **Settings** → **Secrets and variables** → **Actions**
3. Click **New repository secret**
4. Enter the secret name and value
5. Click **Add secret**

**Important:** GitHub Secrets are similar to Azure DevOps variable groups - they are encrypted and only exposed to workflow runs. They are never displayed in logs.

## Running the Workflows

### Manual Trigger (Recommended for initial releases)

1. Go to **Actions** tab in your GitHub repository
2. Select the workflow (Android or iOS)
3. Click **Run workflow**
4. Enter the version name and version/build number
5. Click **Run workflow**

### Automatic Trigger via Git Tags

```bash
# Create and push a version tag
git tag v1.0.5
git push origin v1.0.5
```

This will automatically trigger both iOS and Android builds.

## Downloading Build Artifacts

After a successful workflow run:
1. Go to the **Actions** tab
2. Click on the completed workflow run
3. Scroll to the **Artifacts** section at the bottom
4. Download the AAB/IPA files

## Deployment Process

The workflows build the packages but **do not automatically deploy** to the stores. You'll need to manually upload:

- **Android:** Upload the AAB file to Google Play Console
- **iOS:** Upload the IPA file via Transporter app or Xcode, or enable automatic TestFlight upload

See the detailed setup instructions in the root `docs/deployment` folder.
