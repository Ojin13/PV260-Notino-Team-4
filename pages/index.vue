<script setup lang="ts">
import TerminalWindow from '~/components/TerminalWindow.vue'

const repoUrl = 'https://github.com/Ojin13/PV260-Notino-Team-4'
const downloadUrl = `${repoUrl}/releases/latest/download/Popocatepetl.CLI-win-x64.zip`
const releasesUrl = `${repoUrl}/releases`

const features = [
  {
    glyph: '∆',
    title: 'Download & diff',
    body: 'Admins fetch the latest report and the app compares it against the previously stored one. Only the most recent report is kept.'
  },
  {
    glyph: '✉',
    title: 'Email reports',
    body: 'Power Users send the current diff to up to 100 recipients via SendGrid, with confirmation before sending.'
  },
  {
    glyph: '↧',
    title: 'Export to file',
    body: 'Admins can export the current diff to a file for sharing or archiving.'
  },
  {
    glyph: '☰',
    title: 'Audit log',
    body: 'Every login, download, export, theme change and email is logged. Admins can list and filter by user, date, or action.'
  },
  {
    glyph: '⚐',
    title: 'Roles & access',
    body: 'Three roles — User, Admin, Power User. Admin features are gated by a password.'
  },
  {
    glyph: '⌘',
    title: 'Themes & languages',
    body: 'Switchable color themes for Power Users and a fully translated UI for everyone.'
  }
]
</script>

<template>
  <div>
    <header class="nav">
      <div class="nav-inner">
        <div class="brand">
          <span class="brand-dot" />
          popocatepetl
        </div>
        <nav class="nav-links">
          <a href="#features">features</a>
          <a href="#install">install</a>
          <a :href="repoUrl" target="_blank" rel="noopener">github ↗</a>
        </nav>
      </div>
    </header>

    <main>
      <section class="hero">
        <div class="container">
          <div class="hero-grid">
            <div>
              <span class="eyebrow">
                <span aria-hidden="true">●</span> v2026 · windows · console
              </span>
              <h1>
                Track what changed<br />
                in your <span class="gradient">reports</span>.
              </h1>
              <p class="lead">
                A Windows console app. Download a report, compare it to the
                previous one, share what changed.
              </p>
              <div class="cta-row">
                <a class="btn btn-primary" :href="downloadUrl">
                  ⇣ Download for Windows
                </a>
                <a class="btn" :href="releasesUrl" target="_blank" rel="noopener">
                  all releases
                </a>
                <a class="btn" :href="repoUrl" target="_blank" rel="noopener">
                  source
                </a>
              </div>
            </div>
            <TerminalWindow />
          </div>
        </div>
      </section>

      <section class="section" id="features">
        <div class="container">
          <div class="section-label">// features</div>
          <h2>What it does</h2>
          <p class="section-lead">
            One job: download a report, compute the diff, and get it
            in front of the people who need it.
          </p>
          <div class="feature-grid">
            <div v-for="feature in features" :key="feature.title" class="card">
              <div class="icon">{{ feature.glyph }}</div>
              <h3>{{ feature.title }}</h3>
              <p>{{ feature.body }}</p>
            </div>
          </div>
        </div>
      </section>

      <section class="section" id="install">
        <div class="container">
          <div class="section-label">// install</div>
          <h2>Three steps</h2>
          <p class="section-lead">
            No installer. Self-contained build — the .NET runtime is in the zip.
          </p>
          <ol class="steps">
            <li>
              <span class="step-num">01</span>
              <div>
                <strong>Download</strong> the latest release.
                <a class="step-link" :href="downloadUrl">
                  Popocatepetl.CLI-win-x64.zip ↓
                </a>
              </div>
            </li>
            <li>
              <span class="step-num">02</span>
              <div>
                <strong>Unzip</strong> it anywhere — right-click → <em>Extract All</em>.
              </div>
            </li>
            <li>
              <span class="step-num">03</span>
              <div>
                <strong>Run</strong> <code class="mono">Popocatepetl.CLI.exe</code>
                from the extracted folder.
              </div>
            </li>
          </ol>
        </div>
      </section>
    </main>

    <footer class="footer">
      <div class="container footer-inner">
        <div>// built for PV260 — software quality, spring 2026</div>
        <div>
          <a :href="repoUrl" target="_blank" rel="noopener">github.com/Ojin13/PV260-Notino-Team-4</a>
        </div>
      </div>
    </footer>
  </div>
</template>
