# iOS Deployment Setup Guide

## Prerequisites
- An Apple Developer account ($99/year)
- A Mac computer (for certificate creation and Xcode)
- Xcode installed (from Mac App Store)
- Access to your repository's GitHub Settings

## Step-by-Step Setup Instructions

### Part 1: Apple Developer Account Setup

#### 1. Enroll in Apple Developer Program
1. Go to [Apple Developer](https://developer.apple.com/programs/)
2. Sign in with your Apple ID
3. Enroll in the Apple Developer Program ($99/year)
4. Complete enrollment (may take 24-48 hours for approval)

#### 2. Create App ID
1. Go to [Apple Developer Portal](https://developer.apple.com/account/)
2. Navigate to **Certificates, Identifiers & Profiles**
3. Click **Identifiers** → **+** (plus button)
4. Select **App IDs** → Click **Continue**
5. Select **App** → Click **Continue**
6. Fill in:
   - **Description:** Spiritual Gifts Survey
   - **Bundle ID:** `com.launchpaddevs.spiritualgiftssurvey` (Explicit)
   - **Capabilities:** Select any needed (likely just default)
7. Click **Continue** → **Register**

### Part 2: Create Distribution Certificate

**You MUST do this on a Mac with Xcode installed.**

#### 1. Create Certificate Signing Request (CSR)

1. Open **Keychain Access** (in Applications/Utilities)
2. Menu: **Keychain Access** → **Certificate Assistant** → **Request a Certificate from a Certificate Authority**
3. Fill in:
   - **User Email Address:** your@email.com
   - **Common Name:** Your Name
   - **Request:** Saved to disk
   - **Let me specify key pair information:** UNCHECKED
4. Click **Continue** → Save the CSR file

#### 2. Create Distribution Certificate in Apple Developer Portal

1. Go to [Certificates, Identifiers & Profiles](https://developer.apple.com/account/resources/certificates/list)
2. Click **Certificates** → **+** (plus button)
3. Select **Apple Distribution** → Click **Continue**
4. Upload the CSR file you created
5. Click **Continue** → **Download** the certificate
6. Double-click the downloaded certificate to install it in Keychain Access

#### 3. Export Certificate as .p12

1. Open **Keychain Access**
2. In left sidebar, select **My Certificates**
3. Find your **Apple Distribution** certificate
4. Right-click → **Export "Apple Distribution..."**
5. Save as: `distribution_cert.p12`
6. **Set a password** (this is your `IOS_DISTRIBUTION_CERT_PASSWORD`)
7. Click **Save**
8. You may be prompted for your Mac password

#### 4. Convert Certificate to Base64

```bash
# In Terminal
base64 -i distribution_cert.p12 -o cert_base64.txt
```

### Part 3: Create Provisioning Profile

#### 1. Register Devices (for Testing - Optional)

If you want to test on specific devices first:
1. Go to **Devices** → **+** (plus button)
2. Enter device UDID and name
3. Register

#### 2. Create App Store Provisioning Profile

1. Go to **Profiles** → **+** (plus button)
2. Select **App Store** under Distribution → Click **Continue**
3. Select your App ID: `com.launchpaddevs.spiritualgiftssurvey`
4. Click **Continue**
5. Select your **Distribution Certificate**
6. Click **Continue**
7. Enter Profile Name: `Spiritual Gifts Survey App Store`
8. Click **Generate** → **Download**

#### 3. Convert Provisioning Profile to Base64

```bash
# In Terminal
base64 -i YourProfile.mobileprovision -o profile_base64.txt
```

#### 4. Get Provisioning Profile Details

```bash
# Extract and view profile details
security cms -D -i YourProfile.mobileprovision

# Look for:
# - <key>Name</key><string>YOUR_PROFILE_NAME</string>
# - <key>UUID</key><string>PROFILE_UUID</string>
```

### Part 4: Get Code Signing Identity Name

```bash
# In Terminal on your Mac
security find-identity -v -p codesigning

# Look for output like:
# 1) XXXXX "Apple Distribution: Your Name (TEAM_ID)"
# The quoted string is your IOS_CODESIGN_KEY_NAME
```

### Part 5: Add Secrets to GitHub

Navigate to **Settings** → **Secrets and variables** → **Actions**

Add these secrets:

| Secret Name | Value | How to Get |
|-------------|-------|------------|
| `IOS_DISTRIBUTION_CERT_P12_BASE64` | Contents of `cert_base64.txt` | Step 2.4 above |
| `IOS_DISTRIBUTION_CERT_PASSWORD` | Password you set in Step 2.3 | Your password |
| `IOS_PROVISIONING_PROFILE_BASE64` | Contents of `profile_base64.txt` | Step 3.3 above |
| `IOS_PROVISIONING_PROFILE_NAME` | Name of provisioning profile | Step 3.2 - e.g., "Spiritual Gifts Survey App Store" |
| `IOS_CODESIGN_KEY_NAME` | Code signing identity | Step 4 - e.g., "Apple Distribution: John Doe (ABC123XYZ)" |

### Part 6: App Store Connect Setup

#### 1. Create App in App Store Connect

1. Go to [App Store Connect](https://appstoreconnect.apple.com/)
2. Click **My Apps** → **+** (plus button) → **New App**
3. Fill in:
   - **Platforms:** iOS
   - **Name:** Spiritual Gifts Survey
   - **Primary Language:** English (U.S.)
   - **Bundle ID:** Select `com.launchpaddevs.spiritualgiftssurvey`
   - **SKU:** `spiritualgiftssurvey001` (unique identifier)
   - **User Access:** Full Access
4. Click **Create**

#### 2. Complete App Information

**App Information:**
- **Privacy Policy URL:** (Required - your privacy policy URL)
- **Category:** Primary: Lifestyle or Education
- **Content Rights:** Choose appropriate option

**Pricing and Availability:**
- **Price:** Free
- **Availability:** All countries or select specific ones

**App Privacy:**
1. Click **Get Started**
2. Answer data collection questions
3. For this app, likely: Spiritual information, possibly email
4. Submit

#### 3. Prepare App Store Listing

Under your app version (e.g., 1.0):

**Screenshots Required:**
- 6.7" Display (iPhone 15 Pro Max): At least 1 screenshot
- 6.5" Display (iPhone 11 Pro Max): At least 1 screenshot
- 5.5" Display (iPhone 8 Plus): At least 1 screenshot
- 12.9" iPad Pro: At least 1 screenshot (recommended)

**App Description:**
- **Promotional Text:** (Optional, can update anytime)
- **Description:** Full description of your app
- **Keywords:** Comma-separated (max 100 characters)
- **Support URL:** Your support website
- **Marketing URL:** (Optional)

**App Icon:**
- 1024x1024 PNG (no transparency, no rounded corners)

**Copyright:**
- `2026 LaunchPad Developers` (or your organization)

**Age Rating:**
- Click **Edit** → Answer questionnaire
- Should be 4+ for this app

### Part 7: Run the GitHub Action

#### Manual Trigger
1. Go to GitHub **Actions** tab
2. Select **iOS Release Build**
3. Click **Run workflow**
4. Enter:
   - Version name: `1.0.5`
   - Build number: `5`
5. Click **Run workflow**

#### Tag-Based Trigger
```bash
git tag v1.0.5
git push origin v1.0.5
```

### Part 8: Upload IPA to App Store

#### Option A: Using Transporter App (Recommended)

1. Download [Transporter](https://apps.apple.com/us/app/transporter/id1450874784) from Mac App Store
2. Sign in with your Apple Developer account
3. Drag and drop the IPA file from GitHub Actions artifacts
4. Click **Deliver**

#### Option B: Using Xcode

1. Open Xcode
2. Menu: **Window** → **Organizer**
3. Drag IPA to Organizer or click **Distribute App**
4. Follow the wizard to upload

#### Option C: Using Command Line (xcrun altool)

```bash
xcrun altool --upload-app \
  --type ios \
  --file YourApp.ipa \
  --username "your@email.com" \
  --password "@keychain:AC_PASSWORD"
```

### Part 9: Submit for Review

1. After upload completes (may take 5-30 minutes to process)
2. Go to App Store Connect → Your App → Version
3. The build should appear under **Build** section
4. Click **+** to select the build
5. Complete all required fields:
   - Screenshots
   - Description
   - Keywords
   - Support URL
   - Privacy Policy URL
6. **Save** changes
7. Click **Submit for Review**
8. Answer additional questions
9. Click **Submit**

## Part 10: (Optional) Enable Automatic TestFlight Upload

To have the GitHub Action automatically upload to TestFlight:

### 1. Create App Store Connect API Key

1. Go to [App Store Connect](https://appstoreconnect.apple.com/)
2. Navigate to **Users and Access** → **Keys** tab
3. Click **+** (Generate API Key)
4. **Name:** GitHub Actions
5. **Access:** App Manager or Developer
6. Click **Generate**
7. **Download** the API Key file (`.p8`) - you can only download once!
8. Note the **Key ID** and **Issuer ID**

### 2. Add Additional GitHub Secrets

| Secret Name | Value |
|-------------|-------|
| `APP_STORE_CONNECT_API_KEY_ID` | The Key ID from App Store Connect |
| `APP_STORE_CONNECT_ISSUER_ID` | The Issuer ID from App Store Connect |
| `APP_STORE_CONNECT_API_KEY_P8_BASE64` | Base64 of the .p8 file |

Convert .p8 to base64:
```bash
base64 -i AuthKey_XXXXX.p8 -o api_key_base64.txt
```

### 3. Update Workflow

The workflow already has TestFlight upload commented in - you can enable it once the above secrets are set.

## Important Notes

### Version Management
- **CFBundleShortVersionString (Version):** User-facing (e.g., 1.0.5)
- **CFBundleVersion (Build Number):** Must increment for each upload (e.g., 1, 2, 3)
- Each build number can only be used once per version

### Testing Before Submission
- [ ] Test on physical iPhone devices
- [ ] Test on iPad (if targeting iPad)
- [ ] Test all orientations
- [ ] Test offline functionality
- [ ] Verify all survey flows work
- [ ] Test email functionality
- [ ] Verify translations and RTL layout (Arabic)

### App Review Guidelines
Apple is strict about:
- Privacy policy must be accessible
- All features must work as described
- No crashes or broken functionality
- Proper handling of user data
- Compliance with Human Interface Guidelines

### Common Issues

**Issue:** "No code signing identity found"
- **Solution:** Verify certificate is installed in Keychain
- Check that `IOS_CODESIGN_KEY_NAME` exactly matches certificate name

**Issue:** "Provisioning profile doesn't match bundle ID"
- **Solution:** Verify provisioning profile was created for correct App ID
- Check bundle ID in `Info.plist` matches exactly

**Issue:** "Build rejected - missing compliance"
- **Solution:** If app uses encryption, you need to answer export compliance questions
- For this app, should answer "No" (standard HTTPS doesn't count)

**Issue:** Upload fails with authentication error
- **Solution:** Use app-specific password, not your regular Apple ID password
- Create at [appleid.apple.com](https://appleid.apple.com) → Security → App-Specific Passwords

## Useful Commands

```bash
# Check installed certificates
security find-identity -v -p codesigning

# View provisioning profile details
security cms -D -i YourProfile.mobileprovision

# Check IPA contents
unzip -l YourApp.ipa

# Validate IPA before upload
xcrun altool --validate-app --type ios --file YourApp.ipa

# List installed provisioning profiles
ls ~/Library/MobileDevice/Provisioning\ Profiles/
```

## Resources

- [App Store Connect](https://appstoreconnect.apple.com/)
- [Apple Developer Portal](https://developer.apple.com/account/)
- [App Store Review Guidelines](https://developer.apple.com/app-store/review/guidelines/)
- [Human Interface Guidelines](https://developer.apple.com/design/human-interface-guidelines/)
- [TestFlight Documentation](https://developer.apple.com/testflight/)
