import { setTheme } from '@fluentui/web-components';
import { createLightTheme, createDarkTheme, webLightTheme, webDarkTheme } from '@fluentui/tokens';
import '@fluentui/web-components/button.js';
import '@fluentui/web-components/switch.js';
import '@fluentui/web-components/badge.js';
import { validAppearance, brandRamp, desktopOverrides } from './appearance.js';

const root = document.documentElement;
let current;
let signature = '';
let events;
let pollTimer;
let refreshController;

function setStatus(connected) {
  root.dataset.appearanceState = connected ? 'synced' : 'reconnecting';
  for (const indicator of document.querySelectorAll('.theme-sync')) {
    const mode = current?.theme === 'Dark' ? '深色' : '浅色';
    indicator.textContent = connected ? `${mode} · 与电脑同步` : '外观同步重连中';
    indicator.title = current ? `电脑外观：${mode}；${current.accentSource === 'System' ? '系统' : '自定义'}强调色 ${current.accentColor}` : '正在读取电脑外观';
  }
}

export function applyAppearance(appearance) {
  if (!validAppearance(appearance)) return false;
  setStatus(true);
  const next = JSON.stringify(appearance);
  if (next === signature) return true;
  signature = next;
  current = Object.freeze({ ...appearance });
  const dark = appearance.theme === 'Dark';
  const theme = {
    ...(dark ? createDarkTheme(brandRamp(appearance.accentColor)) : createLightTheme(brandRamp(appearance.accentColor))),
    ...desktopOverrides(appearance)
  };
  setTheme(theme);
  root.dataset.theme = dark ? 'dark' : 'light';
  root.style.colorScheme = dark ? 'dark' : 'light';
  root.style.setProperty('--cx-accent', appearance.accentColor);
  root.style.setProperty('--cx-accent-fill', appearance.accentFillColor);
  root.style.setProperty('--cx-accent-text', appearance.accentTextColor);
  root.style.setProperty('--cx-on-accent', theme.colorNeutralForegroundOnBrand);
  const rgb = appearance.accentFillColor.match(/[a-f0-9]{2}/gi).map(channel => parseInt(channel, 16)).join(', ');
  root.style.setProperty('--cx-accent-rgb', rgb);
  document.querySelector('meta[name="theme-color"]')?.setAttribute('content', theme.colorNeutralBackground2);
  setStatus(true);
  document.dispatchEvent(new CustomEvent('cx-appearance-changed', { detail: current }));
  return true;
}

try {
  const initial = document.querySelector('meta[name="cx-appearance"]')?.content;
  if (!applyAppearance(initial ? JSON.parse(initial) : null)) {
    const dark = matchMedia('(prefers-color-scheme: dark)').matches;
    root.dataset.theme = dark ? 'dark' : 'light';
    setTheme(dark ? webDarkTheme : webLightTheme);
  }
} catch {
  setTheme(webLightTheme);
}

let refreshing = false;
async function refresh() {
  if (refreshing) return;
  refreshing = true;
  const before = signature;
  const controller = new AbortController();
  refreshController = controller;
  const timeout = setTimeout(() => controller.abort(), 5000);
  try {
    const response = await fetch('/api/appearance', { cache: 'no-store', signal: controller.signal });
    if (!response.ok) throw new Error('Appearance unavailable');
    const appearance = await response.json();
    if (signature === before && !applyAppearance(appearance)) throw new Error('Invalid appearance');
  } catch {
    if (!controller.signal.aborted) setStatus(false);
  } finally {
    clearTimeout(timeout);
    refreshing = false;
  }
}

function stop() {
  events?.close();
  events = undefined;
  clearInterval(pollTimer);
  pollTimer = undefined;
  refreshController?.abort();
}

function connect() {
  stop();
  if ('EventSource' in window) {
    events = new EventSource('/api/appearance/events');
    events.onmessage = event => {
      try { applyAppearance(JSON.parse(event.data)); } catch { /* Keep the last valid palette. */ }
    };
    events.onopen = () => { void refresh(); };
    events.onerror = () => { setStatus(false); };
  }
  // Also recovers after mobile suspend, dropped SSE connections and older browsers.
  pollTimer = setInterval(() => {
    if (!document.hidden && (!events || events.readyState !== EventSource.OPEN)) void refresh();
  }, 5000);
  void refresh();
}

window.CXAppearance = Object.freeze({ apply: applyAppearance, get current() { return current; } });
window.addEventListener('pagehide', stop);
window.addEventListener('pageshow', event => { if (event.persisted) connect(); });
window.addEventListener('online', connect);
document.addEventListener('visibilitychange', () => { if (!document.hidden) void refresh(); });
connect();
root.classList.add('fluent-ready');
