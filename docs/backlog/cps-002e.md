# [CPS-002e] The author installs a signed package from CI

**Status**: Open
**Priority**: Medium
**Component**: `.github/workflows/`, `README.md`

**Summary**:
As the author, I want to install the extension on my Windows machine from a package CI built,
so that I can use it daily without a development setup there.

**Description**:
CI publishes an MSIX package for `win-x64` as a build artifact. It is signed with a
self-signed certificate for the author's machine; the README says how to trust the
certificate and install the package. Gallery or WinGet come only once others are to use the
extension.

The epic's acceptance criteria are checked on the installed package.

**Acceptance Criteria**:
- [ ] CI publishes a signed MSIX package for `win-x64` as an artifact
- [ ] The README describes trusting the certificate and installing the package
- [ ] The package is installed on the author's machine and the epic's acceptance criteria hold there
