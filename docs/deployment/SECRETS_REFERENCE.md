# GitHub Secrets Configuration Reference

This document provides a quick reference for all GitHub Secrets needed for the deployment workflows.

## How to Add Secrets

1. Go to your GitHub repository
2. Navigate to **Settings** → **Secrets and variables** → **Actions**
3. Click **New repository secret**
4. Enter the secret name (from tables below) and value
5. Click **Add secret**

## Android Secrets (4 Required)

| Secret Name | Description | Example Value | How to Get |
|-------------|-------------|---------------|------------|
| `ANDROID_KEYSTORE_BASE64` | Base64-encoded release keystore file | `MIIKgQIBAzCCCk...` (very long) | Convert keystore: `base64 -i release.keystore` |
| `ANDROID_KEYSTORE_PASSWORD` | Password for the keystore file | `MySecurePassword123!` | Password you set when creating keystore |
| `ANDROID_KEY_ALIAS` | Alias of the key in keystore | `spiritualgifts` | Alias you specified in `keytool` command |
| `ANDROID_KEY_PASSWORD` | Password for the specific key | `MyKeyPassword456!` | Password you set for the key alias |

## iOS Secrets (5 Required + 2 Optional)

### Required Secrets

| Secret Name | Description | Example Value | How to Get |
|-------------|-------------|---------------|------------|
| `IOS_DISTRIBUTION_CERT_P12_BASE64` | Base64-encoded distribution certificate | `MIIQJAIBAzCC...` (very long) | Export cert from Keychain → `base64 -i cert.p12` |
| `IOS_DISTRIBUTION_CERT_PASSWORD` | Password for the .p12 certificate | `CertPassword789!` | Password you set when exporting from Keychain |
| `IOS_PROVISIONING_PROFILE_BASE64` | Base64-encoded provisioning profile | `MIIOmAYJKoZ...` (very long) | Download from Apple Developer → `base64 -i profile.mobileprovision` |
| `IOS_PROVISIONING_PROFILE_NAME` | Name of the provisioning profile | `Spiritual Gifts Survey App Store` | Name you set when creating profile |
| `IOS_CODESIGN_KEY_NAME` | Code signing identity name | `Apple Distribution: John Doe (ABC123)` | Run: `security find-identity -v -p codesigning` |

### Optional Secrets (for Automatic TestFlight Upload)

| Secret Name | Description | How to Get |
|-------------|-------------|------------|
| `APP_STORE_CONNECT_API_KEY_ID` | App Store Connect API Key ID | App Store Connect → Users and Access → Keys |
| `APP_STORE_CONNECT_ISSUER_ID` | App Store Connect Issuer ID | App Store Connect → Users and Access → Keys |

## Verification Checklist

### Android Secrets Verification
```bash
# After adding secrets, verify workflow can access them by running the workflow
# Check the workflow logs - you should see:
# ✅ "Decode Keystore" step succeeds
# ✅ "Build Android AAB" step succeeds
# ✅ AAB file is created and uploaded as artifact
```

### iOS Secrets Verification
```bash
# After adding secrets, verify workflow can access them by running the workflow
# Check the workflow logs - you should see:
# ✅ "Import Code-Signing Certificates" step succeeds
# ✅ "Download Provisioning Profile" step succeeds
# ✅ "Build iOS IPA" step succeeds
# ✅ IPA file is created and uploaded as artifact
```

## Common Secret Issues

### Android

**Problem:** Keystore base64 string contains newlines
```bash
# ❌ Wrong (contains newlines every 76 characters)
base64 release.keystore

# ✅ Correct (single line)
# Windows PowerShell:
[Convert]::ToBase64String([IO.File]::ReadAllBytes("release.keystore"))

# macOS/Linux:
base64 -i release.keystore | tr -d '\n'
```

**Problem:** Wrong password
- **Symptom:** Build fails with "Keystore was tampered with, or password was incorrect"
- **Solution:** Double-check `ANDROID_KEYSTORE_PASSWORD` matches what you set

**Problem:** Wrong alias
- **Symptom:** Build fails with "Alias not found"
- **Solution:** List aliases with `keytool -list -keystore release.keystore`

### iOS

**Problem:** Certificate not found in keychain
- **Symptom:** "No matching certificate found"
- **Solution:** Verify the .p12 file contains the private key (exported from Keychain with "Export Items")

**Problem:** Provisioning profile UUID not extracted
- **Symptom:** "No provisioning profile found"
- **Solution:** Verify profile is for App Store distribution (not Development or Ad Hoc)

**Problem:** Code signing identity name doesn't match
- **Symptom:** "Code signing identity not found"
- **Solution:** Run `security find-identity -v -p codesigning` and copy exact string including quotes

## Security Best Practices

### Storage
- ✅ **DO** store original files (keystore, .p12, profiles) in a secure password manager (1Password, LastPass, etc.)
- ✅ **DO** keep offline encrypted backups of signing materials
- ❌ **DON'T** store signing materials in git, even in private repos
- ❌ **DON'T** email signing materials or passwords

### Access Control
- ✅ **DO** limit repository access to trusted team members
- ✅ **DO** use separate secrets for development and production if needed
- ✅ **DO** rotate secrets if compromised
- ❌ **DON'T** share secrets via chat or unencrypted channels

### GitHub Secrets Security
- ✅ Secrets are encrypted at rest
- ✅ Secrets are masked in workflow logs
- ✅ Secrets are only exposed to workflow runs
- ✅ Secrets cannot be viewed after creation (only updated)
- ✅ Audit logs track secret usage

## Updating Secrets

If you need to update a secret (e.g., new certificate, password change):

1. Go to **Settings** → **Secrets and variables** → **Actions**
2. Click the secret name
3. Click **Update secret**
4. Enter new value
5. Click **Update secret**

**Note:** You cannot view the current value - only update or delete.

## When to Rotate Secrets

### Android
- **Keystore:** NEVER (losing this means you can't update your app!)
- **Passwords:** If compromised or annually for security

### iOS
- **Distribution Certificate:** Before expiration (1 year) or if compromised
- **Provisioning Profile:** When certificate changes or if expired
- **Passwords:** If compromised

## Backup Strategy

Create an encrypted backup containing:

### Android Backup
```
android-signing/
  ├── release.keystore          # The actual keystore file
  ├── credentials.txt           # Encrypted document with:
  │                             #   - Keystore password
  │                             #   - Key alias
  │                             #   - Key password
  └── backup-info.txt           # Date created, version info
```

### iOS Backup
```
ios-signing/
  ├── distribution_cert.p12     # Distribution certificate
  ├── profile.mobileprovision   # Provisioning profile
  ├── credentials.txt           # Encrypted document with:
  │                             #   - Certificate password
  │                             #   - Profile name
  │                             #   - Signing identity name
  │                             #   - Apple ID used
  └── backup-info.txt           # Date created, expiration dates
```

Store encrypted backups in:
- Encrypted cloud storage (iCloud, OneDrive with encryption)
- Password manager vault
- Encrypted external drive in secure location

## Quick Reference: Secret Extraction Commands

### Android
```bash
# Create keystore
keytool -genkey -v -keystore release.keystore -alias spiritualgifts -keyalg RSA -keysize 2048 -validity 10000

# Convert to base64 (macOS/Linux)
base64 -i release.keystore | tr -d '\n' > keystore_base64.txt

# Convert to base64 (Windows PowerShell)
[Convert]::ToBase64String([IO.File]::ReadAllBytes("release.keystore")) | Out-File keystore_base64.txt

# List keystore details
keytool -list -v -keystore release.keystore
```

### iOS
```bash
# Convert certificate to base64
base64 -i distribution_cert.p12 | tr -d '\n' > cert_base64.txt

# Convert provisioning profile to base64
base64 -i YourProfile.mobileprovision | tr -d '\n' > profile_base64.txt

# Get signing identity
security find-identity -v -p codesigning

# View provisioning profile details
security cms -D -i YourProfile.mobileprovision

# Extract profile UUID
security cms -D -i YourProfile.mobileprovision | grep -A 1 UUID
```

## Support

If secrets are not working:
1. Check the workflow logs for specific error messages
2. Verify base64 encoding is correct (no newlines)
3. Confirm passwords are correct
4. Review the detailed setup guides:
   - Android: `docs/deployment/ANDROID_SETUP.md`
   - iOS: `docs/deployment/IOS_SETUP.md`
