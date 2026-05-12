# Popocatepetl — presentation site

This branch (`github-page`) holds the source for the project's GitHub Pages
site, built with **Nuxt 3**. The .NET application itself lives on `main`.

Live site: <https://ojin13.github.io/PV260-Notino-Team-4/>

## Local development

```bash
npm install
npm run dev          # http://localhost:3000
```

## Production build

```bash
npm run generate     # outputs static files to .output/public
```

## Deployment

Every push to `github-page` triggers `.github/workflows/deploy-pages.yml`,
which runs `nuxt generate` and publishes the static output via
`actions/deploy-pages`. The repository's **Settings → Pages** must have
**Source** set to **"GitHub Actions"** (not a branch) for the workflow to
deploy.

## Download link

The site's download button points to
`https://github.com/Ojin13/PV260-Notino-Team-4/releases/latest/download/Popocatepetl.CLI-win-x64.zip`.
That URL is produced by the release workflow on `main`, which builds a
self-contained Windows executable on every merge.
