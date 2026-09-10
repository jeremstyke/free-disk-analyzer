# Security Policy

## Supported versions

Only the latest published release on the [Releases](../../releases) page is supported with security fixes.

## Reporting a vulnerability

Please do not open a public issue for security vulnerabilities.

Instead, email **juryjeremy@gmail.com** with:
- A description of the vulnerability
- Steps to reproduce
- Potential impact

You should receive an acknowledgment within a few days. Once a fix is available, a new release will be published and the reporter credited (unless anonymity is requested).

## Design principles relevant to security

- PurgeCore does not require administrator privileges for normal disk analysis
- The app never executes, deletes, or modifies scanned files in V1
- All file system paths are validated before use
- Every release is published with a `SHA256SUMS.txt` checksum file so downloads can be verified independently of Windows SmartScreen
