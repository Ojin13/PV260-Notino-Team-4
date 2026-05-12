---
name: release-pipeline
description: Documents Popocatepetl's two GitHub Actions workflows - the Nuxt site deploy on github-page and the Windows release build on main. Covers triggers, tag scheme, prerelease logic, the workflow_dispatch quirk, and where the download URL points. Use when changing the workflows or debugging a failed release.
argument-hint: "[workflow name, trigger, or release scenario]"
---

# Release Pipeline

Two workflows live in `.github/workflows/`, one per branch:

| Branch | Workflow | Job |
| --- | --- | --- |
| `github-page` | `deploy-pages.yml` | Generate the Nuxt site and publish to GitHub Pages |
| `main` | `release.yml` | Build the Windows .exe and attach it to a GitHub Release |

They never run on the same branch. Edits to the `.NET` source on `main`/`milestone-2` go through `release.yml`; edits to the Nuxt site on `github-page` go through `deploy-pages.yml`.

## Pages deploy — `deploy-pages.yml`

**Trigger.** Push to `github-page` or manual `workflow_dispatch`.

**Steps.**

1. Checkout
2. `actions/setup-node@v4` with Node 20
3. `npm ci || npm install`
4. `npx nuxt generate` — produces a fully static site in `.output/public/`. The `NUXT_APP_BASE_URL` env is set to `/<repo-name>/` so links and asset paths resolve under the project-pages prefix.
5. `actions/configure-pages@v5`
6. `actions/upload-pages-artifact@v3` with `path: .output/public`
7. `actions/deploy-pages@v4` in a separate job, against the `github-pages` environment

**Settings required once in the repo.**

1. **Settings → Pages → Build and deployment → Source** must be `GitHub Actions` (not "Deploy from a branch").
2. **Settings → Environments → `github-pages` → Deployment branches and tags** must allow `github-page` (default only allows the repo's default branch). Either set "All branches" or add `github-page` as an explicit deployment branch rule.

**URL.** `https://ojin13.github.io/PV260-Notino-Team-4/`. The deploy job logs the URL in its environment summary.

**Concurrency.** Group `pages` with `cancel-in-progress: false` — overlapping pushes queue instead of racing.

## Windows release — `release.yml`

**Trigger.**

```yaml
on:
  push:
    branches: [main]
  workflow_dispatch:
    inputs:
      prerelease:   # default true
      tag_suffix:   # default 'test'
```

Push to `main` → real release. Manual dispatch (any branch) → prerelease with a tag suffix.

**Steps.**

1. Checkout
2. `actions/setup-dotnet@v4` with `dotnet-version: '10.0.x'`
3. **Compute version & release flags** — PowerShell step computes:
   - Tag: `v{yyyy.MM.dd}.{github.run_number}` (e.g., `v2026.05.12.42`)
   - On `workflow_dispatch`: appends `-{tag_suffix}` (default `-test`) and respects the `prerelease` input
   - On push to `main`: `prerelease=false`
4. **Publish CLI** — `dotnet publish Popocatepetl.CLI/Popocatepetl.CLI.csproj` with:
   - `--configuration Release`
   - `--runtime win-x64`
   - `--self-contained true` (bundles .NET 10 runtime)
   - `-p:PublishSingleFile=true`
   - `-p:IncludeNativeLibrariesForSelfExtract=true` (critical for SQLite — without it `e_sqlite3.dll` sits outside the .exe)
   - `-p:DebugType=embedded` (no separate `.pdb`)
5. **Zip** `publish/*` into `Popocatepetl.CLI-win-x64.zip` via `Compress-Archive`
6. **Create release** with `softprops/action-gh-release@v2`:
   - `tag_name`, `name`, `target_commitish: github.sha`
   - `prerelease` from the computed flag
   - `generate_release_notes: true` (pulls from PR/commit titles since the previous tag)
   - `fail_on_unmatched_files: true` so a missing zip fails loudly
   - `files: Popocatepetl.CLI-win-x64.zip`

**Permissions.** The workflow needs `contents: write` to create the release and push the tag. The default `GITHUB_TOKEN` has it when the block is declared.

**Concurrency.** Group `release-main` with `cancel-in-progress: false` — never cancel a publish in flight.

## The download URL

The presentation site links to:

```
https://github.com/Ojin13/PV260-Notino-Team-4/releases/latest/download/Popocatepetl.CLI-win-x64.zip
```

GitHub resolves `/releases/latest/...` to the most recent **non-prerelease** release. That means:

- Prerelease builds (manual dispatch, current `milestone-2` test runs) never become the public download.
- Until a real push-to-main release exists, that URL **404s**. The site has a temporary `latestPrereleaseTag` override in `pages/index.vue` that points at a specific prerelease — clear it once a real release lands. See `pages/index.vue` for the override.

## The `workflow_dispatch` quirk

> A workflow's "Run workflow" button only appears once the workflow file exists on the repository's **default branch**.

That's a GitHub design choice, not a code issue. Until `release.yml` is on `main`, manual dispatch is unavailable even when the file is on every other branch. To test the workflow before a first real merge, **add `milestone-2` to the push trigger** temporarily and let the per-branch prerelease logic mark the build as a prerelease.

When the file finally lands on `main`, manual dispatch becomes available and you can pick any branch from the dropdown.

## Tag scheme

| Trigger | Tag | Prerelease | `/releases/latest`? |
| --- | --- | --- | --- |
| Push to `main` | `v2026.05.12.42` | no | yes |
| Manual dispatch (any branch) | `v2026.05.12.42-test` (default) | yes | no |
| Manual dispatch with custom suffix | `v2026.05.12.42-<suffix>` | as input | no by default |

The tag includes the workflow `run_number`, which is monotonic across the whole repo. Same-day runs get `.1`, `.2`, `.3`. Tags from previous days remain valid; reverting to one is a normal git tag operation.

## When a release fails

The two failure modes seen so far:

1. **`NETSDK1047: Assets file doesn't have a target for net10.0/win-x64`.** Happens when a separate `Restore` step ran without `--runtime` and `Publish` then ran with `--no-restore`. **Fix:** drop the explicit `Restore` step and let `publish` restore. Don't combine `dotnet restore` (no RID) with `dotnet publish --no-restore -r win-x64`.

2. **`Branch "X" is not allowed to deploy to github-pages due to environment protection rules.`** Pages workflow only. Fix in **Settings → Environments → github-pages → Deployment branches and tags** by allowing the branch or switching to "All branches".

For unexpected failures, the workflow logs include the full `dotnet publish` output. Check `Compute version` first if the tag looks wrong, then `Publish` for build failures, then `Create GitHub Release` for permission or asset issues.

## Adding a new build target

If we ever ship Linux or macOS builds:

1. Add a new job that runs on `ubuntu-latest` or `macos-latest`.
2. Adjust `--runtime` (`linux-x64`, `osx-arm64`, etc.).
3. Use the matching native-libs flag; `IncludeNativeLibrariesForSelfExtract` works cross-platform.
4. Zip under a platform-specific name: `Popocatepetl.CLI-linux-x64.zip`.
5. Add the new asset to the `softprops/action-gh-release` `files:` list — the same release carries all platform zips.

Don't try to cross-compile on a single runner. Each native runtime needs its own host.

## Related skills

- [[clean-architecture]] — what gets built; the publish target is `Popocatepetl.CLI`
- [[database]] — migrations apply at startup; the published .exe ships them
- [[localization]] — `.resx` files are embedded in the .exe; no extra packaging step
