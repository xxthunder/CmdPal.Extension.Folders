# [CPS-008a] A Sphinx documentation with sphinx-needs builds in CI

**Status**: Open
**Priority**: Medium
**Component**: `docs/`, `pyproject.toml`, `.github/workflows/ci.yml`

**Summary**:
As the author, I want a Sphinx documentation with sphinx-needs that CI builds, so that requirements have a home.

**Description**:
A `docs/` folder with Sphinx and sphinx-needs, built with `uv` on the Linux runner; warnings fail the build. The README links to it.

**Acceptance Criteria**:
- [ ] `docs/` builds with sphinx-needs in CI, warnings as errors
- [ ] The build runs locally with one command, named in the README
