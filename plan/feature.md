# Feature: Docs site and package icon

## Goal
Match the pattern established by Tharga.Test and Tharga.Mcp: a public DocFX-generated docs site (`crawler.tharga.net`) and an updated NuGet package icon URL pointing at `thargelion.net/assets/`.

## Scope
1. Update `<PackageIconUrl>` in `Tharga.Crawler.csproj` to `https://thargelion.net/assets/component-crawler.png`.
2. Add a DocFX site under `docs/`:
   - `CNAME` → `crawler.tharga.net`
   - `docfx.json` configured for `Tharga.Crawler`
   - Landing `index.md`
   - Articles: overview, getting-started, configuration, custom-services
   - Custom `templates/thg/` (mirrors Tharga.Test — navbar logo sizing + absolute-URL favicon/logo fix)
   - `toc.yml` for top-level and articles
3. Add `docs` + `docs-deploy` jobs to `.github/workflows/build.yml` plus `pages: write` and `id-token: write` permissions.
4. Update `README.md` with a link to the new docs site.

## Out of scope
- DNS / CNAME setup at the DNS provider (manual, outside the repo).
- Verifying `component-crawler.png` exists at `thargelion.net/assets/` (asset side, outside the repo).
- Migration to `<PackageIcon>` (Tharga.Test still uses the deprecated `<PackageIconUrl>` — keep consistent).

## Acceptance criteria
- `dotnet build -c Release` passes with 0 warnings.
- `dotnet test -c Release` passes (44/44 tests).
- `docfx docs/docfx.json` (run locally or in CI) generates `docs/_site/` without errors.
- The workflow's `docs` and `docs-deploy` jobs are present and gated on `release` success + master push.
- `README.md` points at `https://crawler.tharga.net`.

## Done condition
- All acceptance criteria met.
- PR opened from `feature/docs-site-and-package-icon` → `master`.
- After merge, GitHub Pages serves the site at `crawler.tharga.net`.
