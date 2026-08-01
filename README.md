# 🛡️ Aegis

> **Military-grade, zero-compromise file encryption for maximum privacy.**
>
> **Aegis** is a lightweight desktop application designed to encrypt and obfuscate your files with standard-setting cryptography — protecting your data without complex setups or bloated software.

<p align="center">
    <a href="LICENSE"><img src="https://img.shields.io/badge/License-MIT-yellow.svg"></a>
    <img src="https://img.shields.io/badge/C%23-.NET%20Windows-blue">
    <img src="https://img.shields.io/badge/Security-AES--256--GCM-success">
    <img src="https://img.shields.io/badge/Status-Active-success">
</p>

---

## What is this?

**Aegis** is a high-security file encryption and decryption utility built with AES-256-GCM authenticated encryption. It shields your sensitive files and obfuscates their underlying metadata, making them unreadable to unauthorized users.

Unlike heavy or overly complicated security tools, Aegis was designed around a clear goal:

> **Maximum protection. Zero hassle. Select, lock, done.**

Perfect for users who need true cryptographic security, metadata privacy, and clean file obfuscation.

---

## Features

✔ **AES-256-GCM Authenticated Encryption** (Tamper-proof protection)

✔ **PBKDF2 Key Derivation** (100,000 iterations + 128-bit Random Salt)

✔ **Filename & Extension Obfuscation** (Converts files into `.txt` with random numbers)

✔ **Encrypted Metadata Embedding** (Restores original names/extensions securely)

✔ **Chinese Character Visual Masking** (Hides raw binary data under Unicode text)

✔ **Fixed Aegis Protection Header** (`Aegis protection ` header tag)

✔ **Password Protection** (Derives keys directly from user passwords)

✔ **Built-in UI** (Standalone Windows Forms interface)

---

## Technical Specifications

| Security Layer | Implementation Detail |
|----------------|-----------------------|
| Encryption Algorithm | AES-256-GCM (Galois/Counter Mode) |
| Key Derivation (KDF) | PBKDF2 with HMAC-SHA256 (100,000 iterations) |
| Salt Size | 16 bytes (128 bits) |
| Nonce Size | 12 bytes (96 bits) |
| Auth Tag Size | 16 bytes (128 bits) |
| Metadata Storage | Encrypted inside the payload |
| Output Encoding | Mapped to CJK Unicode block (`U+4E00`+) |

Whenever encrypted files are opened in text editors, raw binaries are safely masked into visual Chinese characters preceded by:

Aegis protection ䷀䷁䷂䷃...


---

# ⚠ Important

Aegis is designed with **authenticated encryption**.

If a file is edited, modified, or corrupted by a single byte, decryption will fail automatically to prevent data tampering attacks.

Please remember:

- Keep your passwords safe (Keys are not stored anywhere).
- Incorrect passwords will fail authentication.
- Always keep backups of original critical files.

---

# Quick Usage

## Option 1 — Pre-built Installer (Recommended)

1. Download the latest installer from [Releases](https://github.com/Meresye/Aegis/releases)
2. Run `AegisSetup.exe`
3. Launch **Aegis** from your desktop shortcut

---

## Option 2 — Building from Source

1. Clone this repository:
   ```bash
   git clone [https://github.com/Meresye/Aegis.git](https://github.com/Meresye/Aegis.git)
Open Aegis.sln in Visual Studio

Select Release mode and press Build (Ctrl + Shift + B)

License
MIT
