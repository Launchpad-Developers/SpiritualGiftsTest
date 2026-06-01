# Android Deployment Setup Guide

## Prerequisites
- A Google Play Developer account ($25 one-time fee)
- Access to your repository's GitHub Settings

## Step-by-Step Setup Instructions

### Part 1: Create a Release Keystore

You need a keystore to sign your Android app. **Keep this file and passwords secure!**

#### Option A: Create Keystore on Windows (Using PowerShell)

```powershell
# Navigate to a secure location (do NOT commit this to git)
cd C:\secure-location

# Create the keystore (replace values as needed)
keytool -genkey -v -keystore release.keystore -alias spiritualgifts -keyalg RSA -keysize 2048 -validity 10000

# You will be prompted for:
# - Keystore password (save this as ANDROID_KEYSTORE_PASSWORD)
# - Key password (save this as ANDROID_KEY_PASSWORD)
# - Your name, organization, etc.
```

#### Option B: Create Keystore on macOS/Linux

```bash
# Navigate to a secure location
cd ~/secure-location

# Create the keystore
keytool -genkey -v -keystore release.keystore -alias spiritualgifts -keyalg RSA -keysize 2048 -validity 10000
```

**Important Notes:**
- The alias (spiritualgifts) is your `ANDROID_KEY_ALIAS`
- Store the keystore file in a secure location (NOT in your repository)
- Back up this keystore - if you lose it, you cannot update your app!

### Part 2: Convert Keystore to Base64

The GitHub workflow needs the keystore as a base64 string.

#### On Windows (PowerShell)
```powershell
$bytes = [System.IO.File]::ReadAllBytes("C:\secure-location\release.keystore")
$base64 = [System.Convert]::ToBase64String($bytes)
$base64 | Out-File -FilePath keystore_base64.txt -Encoding ASCII
Write-Host "Base64 saved to keystore_base64.txt"
```

#### On macOS/Linux
```bash
base64 -i release.keystore -o keystore_base64.txt
```

### Part 3: Add Secrets to GitHub

1. Go to your GitHub repository
2. Navigate to **Settings** → **Secrets and variables** → **Actions**
3. Click **New repository secret** for each of these:

| Secret Name | Value | Example |
|-------------|-------|---------|
| `ANDROID_KEYSTORE_BASE64` | Contents of `keystore_base64.txt` | (long base64 string) |
| `ANDROID_KEYSTORE_PASSWORD` | Keystore password you set | `MySecurePass123!` |
| `ANDROID_KEY_ALIAS` | Alias you used | `spiritualgifts` |
| `ANDROID_KEY_PASSWORD` | Key password you set | `MyKeyPass456!` |

### Part 4: Google Play Console Setup

#### 1. Create App in Google Play Console

1. Go to [Google Play Console](https://play.google.com/console)
2. Click **Create app**
3. Fill in:
   - **App name:** Spiritual Gifts Survey
   - **Default language:** English (United States)
   - **App or game:** App
   - **Free or paid:** Free
4. Accept declarations and click **Create app**

#### 2. Complete Store Listing

Navigate to **Store presence** → **Main store listing**:

- **App name:** Spiritual Gifts Survey
- **Short description:** Discover your spiritual gifts through an interactive survey
- **Full description:** (Write a detailed description)
- **App icon:** 512x512 PNG (from your Resources/AppIcon)
- **Feature graphic:** 1024x500 PNG (you'll need to create this)
- **Screenshots:** At least 2 phone screenshots, recommended tablet screenshots
- **App category:** Lifestyle or Education
- **Contact email:** Your support email

#### 3. Set Up Content Rating

Navigate to **Policy** → **App content** → **Content rating**:

1. Click **Start questionnaire**
2. Enter your email
3. Select **Utility, Productivity, Communication, or Other**
4. Answer questions (should be all "No" for this app)
5. Submit

#### 4. Set Target Audience

Navigate to **Policy** → **App content** → **Target audience**:

1. Select target age groups (e.g., 13+)
2. Answer follow-up questions
3. Save

#### 5. Create Internal Testing Track (Recommended)

Before releasing to production:

1. Navigate to **Release** → **Testing** → **Internal testing**
2. Click **Create new release**
3. Upload your AAB file (from GitHub Actions artifacts)
4. Add release notes
5. Save and review
6. Click **Start rollout to internal testing**

Add testers:
1. Go to **Testers** tab
2. Create an email list of testers
3. Share the opt-in URL with them

#### 6. Production Release (When Ready)

1. Navigate to **Release** → **Production**
2. Click **Create new release**
3. Upload your signed AAB file
4. Add release notes
5. Set rollout percentage (start with 20% for safety)
6. Save and review
7. Click **Start rollout to production**

### Part 5: Run the GitHub Action

#### Manual Trigger
1. Go to GitHub **Actions** tab
2. Select **Android Release Build**
3. Click **Run workflow**
4. Enter:
   - Version name: `1.0.5`
   - Version code: `5`
5. Click **Run workflow**

#### Tag-Based Trigger
```bash
git tag v1.0.5
git push origin v1.0.5
```

### Part 6: Download and Upload AAB

1. After workflow completes, go to **Actions** → Click the workflow run
2. Download the AAB artifact
3. In Google Play Console, upload the AAB to your release

## Important Notes

### Version Management
- **Version Code:** Must increment for every release (integer: 1, 2, 3...)
- **Version Name:** User-facing version (string: 1.0, 1.0.1, 1.1.0)
- Update both in `AndroidManifest.xml` (workflow does this automatically)

### App Signing by Google Play
- Recommended: Enroll in **App Signing by Google Play**
- Google will manage your upload key and signing key
- Provides additional security and key management

### Testing Checklist Before Production
- [ ] Test on multiple Android devices/emulators
- [ ] Test on different screen sizes
- [ ] Test offline functionality
- [ ] Test all survey flows
- [ ] Verify translations work correctly
- [ ] Test email sending functionality
- [ ] Check that results display correctly

### Common Issues

**Issue:** Build fails with signing error
- **Solution:** Verify all 4 GitHub secrets are set correctly
- Check that base64 encoding is complete (no line breaks in middle)

**Issue:** Google Play rejects AAB
- **Solution:** Ensure version code is higher than previous uploads
- Check that target SDK version is recent (currently 35)

**Issue:** App crashes on startup
- **Solution:** Test the APK artifact before uploading AAB
- Check Application Insights for crash logs

## Useful Commands

```bash
# Test build locally (without signing)
dotnet build -f net9.0-android -c Release

# View keystore details
keytool -list -v -keystore release.keystore -alias spiritualgifts

# Check AAB contents
bundletool dump manifest --bundle=path/to/app.aab
```

## Resources

- [Google Play Console](https://play.google.com/console)
- [Android App Bundle Documentation](https://developer.android.com/guide/app-bundle)
- [Play Console Help](https://support.google.com/googleplay/android-developer)
