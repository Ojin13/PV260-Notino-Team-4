<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'

type Segment = { text: string; tone?: 'prompt' | 'ok' | 'muted' | 'info' | 'warn' }
type Line = { segments: Segment[]; pauseAfter?: number }

const props = withDefaults(
  defineProps<{
    title?: string
    charDelay?: number
    firstLineCharDelay?: number
    lineGap?: number
  }>(),
  {
    title: 'popocatepetl — bash',
    charDelay: 9,
    firstLineCharDelay: 32,
    lineGap: 130
  }
)

const lines: Line[] = [
  {
    segments: [
      { text: '$ ', tone: 'prompt' },
      { text: 'Popocatepetl.CLI.exe' }
    ],
    pauseAfter: 280
  },
  {
    segments: [
      { text: '─── Popocatepetl ───────────────────', tone: 'muted' }
    ]
  },
  {
    segments: [
      { text: 'email > ', tone: 'prompt' },
      { text: 'admin@example.com' }
    ]
  },
  { segments: [{ text: '' }] },
  {
    segments: [
      { text: '? select action', tone: 'info' }
    ]
  },
  {
    segments: [
      { text: '  ▸ ', tone: 'prompt' },
      { text: 'download latest report' }
    ]
  },
  {
    segments: [
      { text: '    export diff', tone: 'muted' }
    ]
  },
  {
    segments: [
      { text: '    audit logs', tone: 'muted' }
    ]
  },
  {
    segments: [
      { text: '    quit', tone: 'muted' }
    ]
  },
  { segments: [{ text: '' }] },
  {
    segments: [
      { text: 'password > ', tone: 'prompt' },
      { text: '●●●●●●●●●●●●●●●', tone: 'muted' }
    ]
  },
  {
    segments: [
      { text: '✓ ', tone: 'ok' },
      { text: 'report downloaded' }
    ]
  },
  {
    segments: [
      { text: '✓ ', tone: 'ok' },
      { text: 'diff recomputed — ', tone: 'muted' },
      { text: '14 changes', tone: 'info' }
    ]
  },
  { segments: [{ text: '' }] },
  { segments: [{ text: '$ ', tone: 'prompt' }] }
]

const lineCount = lines.length
const visibleChars = ref<number[]>(Array(lineCount).fill(0))
const finished = ref(false)
let timers: ReturnType<typeof setTimeout>[] = []

const lineLength = (line: Line) =>
  line.segments.reduce((sum, seg) => sum + seg.text.length, 0)

const renderLine = (lineIdx: number) => {
  const line = lines[lineIdx]
  let remaining = visibleChars.value[lineIdx]
  const out: Segment[] = []
  for (const seg of line.segments) {
    if (remaining <= 0) break
    const take = Math.min(remaining, seg.text.length)
    out.push({ text: seg.text.slice(0, take), tone: seg.tone })
    remaining -= take
  }
  return out
}

const isLineActive = (lineIdx: number) =>
  !finished.value && visibleChars.value[lineIdx] > 0 && visibleChars.value[lineIdx] < lineLength(lines[lineIdx])

const showFinalCursor = computed(() => finished.value)

const typeLine = (idx: number) => {
  if (idx >= lineCount) {
    finished.value = true
    return
  }
  const total = lineLength(lines[idx])
  if (total === 0) {
    timers.push(setTimeout(() => typeLine(idx + 1), 80))
    return
  }
  const delay = idx === 0 ? props.firstLineCharDelay : props.charDelay
  const step = () => {
    if (visibleChars.value[idx] >= total) {
      const after = lines[idx].pauseAfter ?? props.lineGap
      timers.push(setTimeout(() => typeLine(idx + 1), after))
      return
    }
    visibleChars.value[idx] = visibleChars.value[idx] + 1
    timers.push(setTimeout(step, delay))
  }
  step()
}

onMounted(() => {
  timers.push(setTimeout(() => typeLine(0), 350))
})

onBeforeUnmount(() => {
  timers.forEach((t) => clearTimeout(t))
  timers = []
})
</script>

<template>
  <div class="term" role="img" :aria-label="`Terminal showing ${title} running`">
    <div class="term-chrome">
      <div class="term-dots" aria-hidden="true">
        <span class="dot dot-red" />
        <span class="dot dot-amber" />
        <span class="dot dot-green" />
      </div>
      <div class="term-title">{{ title }}</div>
      <div class="term-spacer" />
    </div>
    <pre class="term-body"><span
        v-for="(line, idx) in lines"
        :key="idx"
        class="term-line"
      ><template v-for="(seg, sIdx) in renderLine(idx)" :key="sIdx"><span
        :class="['seg', seg.tone ? `tone-${seg.tone}` : '']"
        >{{ seg.text }}</span></template><span
        v-if="isLineActive(idx)"
        class="caret"
        aria-hidden="true"
      />{{ '\n' }}</span><span
        v-if="showFinalCursor"
        class="caret caret-blink"
        aria-hidden="true"
      /></pre>
  </div>
</template>

<style scoped>
.term {
  background: #0a0e14;
  border: 1px solid var(--border);
  border-radius: 12px;
  box-shadow: var(--shadow-lg);
  overflow: hidden;
  min-height: 360px;
  display: flex;
  flex-direction: column;
}

.term-chrome {
  display: flex;
  align-items: center;
  padding: 10px 14px;
  background: linear-gradient(180deg, #1a2030 0%, #131826 100%);
  border-bottom: 1px solid var(--border);
  gap: 12px;
}

.term-dots {
  display: flex;
  gap: 7px;
  flex-shrink: 0;
}

.dot {
  width: 12px;
  height: 12px;
  border-radius: 50%;
  display: inline-block;
}

.dot-red {
  background: #ff5f56;
}

.dot-amber {
  background: #ffbd2e;
}

.dot-green {
  background: #27c93f;
}

.term-title {
  flex: 1;
  text-align: center;
  font-family: var(--font-mono);
  font-size: 12px;
  color: var(--text-muted);
  letter-spacing: 0.04em;
}

.term-spacer {
  width: 54px;
  flex-shrink: 0;
}

.term-body {
  flex: 1;
  margin: 0;
  padding: 22px 22px 26px;
  font-family: var(--font-mono);
  font-size: 13.5px;
  line-height: 1.7;
  color: var(--text);
  background: transparent;
  white-space: pre-wrap;
  word-break: break-word;
  overflow-x: auto;
}

.term-line {
  display: inline;
}

.seg.tone-prompt {
  color: var(--accent);
  font-weight: 600;
}

.seg.tone-ok {
  color: var(--accent);
}

.seg.tone-muted {
  color: #a0a8b3;
}

.seg.tone-info {
  color: var(--accent-blue);
  font-weight: 600;
}

.seg.tone-warn {
  color: var(--warn);
}

.caret {
  display: inline-block;
  width: 8px;
  height: 1.05em;
  vertical-align: text-bottom;
  background: var(--accent);
  margin-left: 2px;
  border-radius: 1px;
}

.caret-blink {
  animation: caret-blink 1s steps(2, end) infinite;
}

@keyframes caret-blink {
  to {
    opacity: 0;
  }
}
</style>
