# [CPF-008d] A trace gate fails CI when a requirement has no test

**Status**: Open
**Priority**: Medium
**Component**: `.github/workflows/ci.yml`, `docs/`

**Summary**:
As the author, I want CI to fail when a requirement has no test, so that traceability does not decay.

**Description**:
A check over the needs data (sphinx-needs' JSON export or a filter) that fails when a `req` has no linked `test`.

**Acceptance Criteria**:
- [ ] CI fails on a requirement without a linked test, shown by a test case
- [ ] CI passes on the current requirements
