# [CPF-008c] Code comments link to the requirements through sphinx-codelinks

**Status**: Open
**Priority**: Medium
**Component**: `src/`, `docs/conf.py`

**Summary**:
As the author, I want comments in the code to name the requirement they implement, so that the documentation links from a requirement to its code.

**Description**:
sphinx-codelinks reads markers in C# comments. First check that the released version supports C# (`FE_CSHARP`); if not, this substory waits for the release or uses a pinned version.

**Acceptance Criteria**:
- [ ] The code that implements each requirement carries a marker the documentation shows
- [ ] The sphinx-codelinks version used supports C# — release or pin named in the item
