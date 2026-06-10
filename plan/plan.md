# Plan: Docs site and package icon

## Steps

- [x] **Step 1** — Update `<PackageIconUrl>` in `Tharga.Crawler/Tharga.Crawler.csproj` to `https://thargelion.net/assets/component-crawler.png`.
- [x] **Step 2** — Create `docs/CNAME` with `crawler.tharga.net`.
- [x] **Step 3** — Create `docs/docfx.json` based on Tharga.Test's, but with `Tharga.Crawler` paths, names, logo, and repo URL.
- [x] **Step 4** — Create `docs/index.md` (landing page) and `docs/toc.yml`.
- [x] **Step 5** — Create `docs/articles/` with `toc.yml`, `index.md`, `getting-started.md`, `configuration.md`, `custom-services.md`.
- [x] **Step 6** — Copy `templates/thg/layout/_master.tmpl` from Tharga.Test verbatim and write a Crawler-specific `templates/thg/public/main.css`.
- [x] **Step 7** — Update `.github/workflows/build.yml`:
  - Added `pages: write` and `id-token: write` to permissions
  - Added `docs` job (DocFX build + upload artifact, `needs: release`)
  - Added `docs-deploy` job (deploy to GitHub Pages)
- [x] **Step 8** — Update `README.md` with a link to `https://crawler.tharga.net`.
- [x] **Step 9** — Verified build (`dotnet build -c Release`) — 0 warnings.
- [x] **Step 10** — Verified tests — 44/44 pass.
- [x] **Step 11** — Verified DocFX build locally — 0 warnings, 0 errors, 42 HTML files generated.
- [x] **Step 12** — Added `/docs/_site/` and `/docs/api/` to `.gitignore`.
- [~] **Step 13** — Commit and push the feature branch, then open PR.

## Last session
Implementation complete. Build/test/DocFX all clean. Ready to commit + push + PR.
