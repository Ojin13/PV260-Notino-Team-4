// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  compatibilityDate: '2025-01-01',
  devtools: { enabled: false },

  // Project pages live under https://<user>.github.io/<repo>/, so every asset
  // and link needs to be prefixed with the repo name. The workflow can override
  // this via NUXT_APP_BASE_URL if the repo is ever renamed.
  app: {
    baseURL: process.env.NUXT_APP_BASE_URL || '/PV260-Notino-Team-4/',
    buildAssetsDir: '/_nuxt/',
    head: {
      title: 'Popocatepetl — Track ARK fund holdings',
      htmlAttrs: { lang: 'en' },
      meta: [
        { charset: 'utf-8' },
        { name: 'viewport', content: 'width=device-width, initial-scale=1' },
        {
          name: 'description',
          content:
            'Popocatepetl is a Windows console application that downloads ARK ETF holdings, diffs them against the previous day, and emails the changes to your inbox.'
        },
        { name: 'theme-color', content: '#0d1117' }
      ],
      link: [
        { rel: 'preconnect', href: 'https://fonts.googleapis.com' },
        { rel: 'preconnect', href: 'https://fonts.gstatic.com', crossorigin: '' },
        {
          rel: 'stylesheet',
          href: 'https://fonts.googleapis.com/css2?family=JetBrains+Mono:wght@400;500;600;700&family=Inter:wght@400;500;600;700&display=swap'
        }
      ]
    }
  },

  css: ['~/assets/css/main.css'],

  // The github-pages preset writes a .nojekyll file into the output and tunes
  // the build for static hosting on GitHub Pages.
  nitro: {
    preset: 'github-pages'
  }
})
