<script setup lang="ts">
import TerminalWindow from '~/components/TerminalWindow.vue'

const repoUrl = 'https://github.com/Ojin13/PV260-Notino-Team-4'
const downloadUrl = `${repoUrl}/releases/latest/download/Popocatepetl.CLI-win-x64.zip`
const releasesUrl = `${repoUrl}/releases/latest`

const features = [
  {
    glyph: '⇣',
    title: 'ARK report fetcher',
    body: 'Pulls the latest daily holdings CSV for any ARK ETF straight from ark-funds.com and stores a compact snapshot in a local SQLite database.'
  },
  {
    glyph: '∆',
    title: 'Daily diffs',
    body: 'Compares today\'s holdings against the previous report and surfaces additions, removals, and weight changes — no spreadsheet wrangling needed.'
  },
  {
    glyph: '✉',
    title: 'Email reports',
    body: 'Ships the daily diff as a clean CSV attachment to a configurable list of recipients. SMTP credentials are read from your environment.'
  },
  {
    glyph: '⛭',
    title: 'Configurable funds',
    body: 'Track one or many funds — ARKK, ARKG, ARKW, ARKQ, ARKF, IZRL, PRNT — pass them as CLI flags or persist a default list.'
  },
  {
    glyph: '☰',
    title: 'Audit logging',
    body: 'Every fetch, diff, and email is logged with Spectre.Console output and persisted, so you can replay what happened on any given day.'
  },
  {
    glyph: '◇',
    title: 'Self-contained',
    body: 'Ships as a single .exe with the .NET 10 runtime baked in. No installer, no SDK on the target machine — download and run.'
  }
]

const commands = [
  {
    invocation: 'popocatepetl fetch-ark',
    desc: 'Download the latest holdings for the configured ARK funds and write a snapshot to the database.'
  },
  {
    invocation: 'popocatepetl diff',
    desc: 'Compute the diff between the two most recent snapshots and print it to the console.'
  },
  {
    invocation: 'popocatepetl send-report',
    desc: 'Build a CSV diff and email it to every recipient configured in appsettings.'
  },
  {
    invocation: 'popocatepetl run --schedule daily',
    desc: 'Fetch, diff, and email in a single pass — the command you want behind a Windows scheduled task.'
  },
  {
    invocation: 'popocatepetl --help',
    desc: 'Show every available command and flag with examples.'
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
          <a href="#commands">commands</a>
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
                Track ARK funds<br />
                from your <span class="gradient">terminal</span>.
              </h1>
              <p class="lead">
                Popocatepetl is a Windows console application that downloads the
                daily ARK ETF holdings, diffs them against yesterday, and emails
                the changes — all in one command.
              </p>
              <div class="cta-row">
                <a class="btn btn-primary" :href="downloadUrl">
                  ⇣ Download for Windows
                </a>
                <a class="btn" :href="releasesUrl" target="_blank" rel="noopener">
                  view all releases
                </a>
                <a class="btn" :href="repoUrl" target="_blank" rel="noopener">
                  source on github
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
            Small, focused, and built for one job: keep an eye on ARK
            holdings without opening a single browser tab.
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
          <h2>Get running in 60 seconds</h2>
          <p class="section-lead">
            No installer. Download the self-contained build, unzip, and run.
          </p>
          <pre class="code"><span class="c-comment"># 1. download &amp; unzip the latest release</span>
<span class="c-prompt">PS&gt;</span> Invoke-WebRequest <span class="accent">{{ downloadUrl }}</span> -OutFile pop.zip
<span class="c-prompt">PS&gt;</span> Expand-Archive pop.zip -DestinationPath .\popocatepetl

<span class="c-comment"># 2. configure your SMTP credentials (one-time)</span>
<span class="c-prompt">PS&gt;</span> cd .\popocatepetl
<span class="c-prompt">PS&gt;</span> notepad appsettings.json

<span class="c-comment"># 3. run it</span>
<span class="c-prompt">PS&gt;</span> .\Popocatepetl.CLI.exe run --schedule daily</pre>
        </div>
      </section>

      <section class="section" id="commands">
        <div class="container">
          <div class="section-label">// commands</div>
          <h2>Command reference</h2>
          <p class="section-lead">
            Every command works on its own — chain them yourself or use
            <code class="mono">run</code> to do it all at once.
          </p>
          <div class="cmd-list">
            <div v-for="cmd in commands" :key="cmd.invocation" class="cmd">
              <code>{{ cmd.invocation }}</code>
              <span class="desc">{{ cmd.desc }}</span>
            </div>
          </div>
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
