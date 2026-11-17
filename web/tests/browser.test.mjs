import test from 'node:test';
import assert from 'node:assert/strict';
import { spawn } from 'node:child_process';
import { mkdir, writeFile, readFile } from 'node:fs/promises';
import { existsSync } from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { chromium } from 'playwright';
import { PNG } from 'pngjs';
import jsQR from 'jsqr';
import { foregroundOn } from '../src/appearance.js';

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '../..');
const artifacts = path.join(root, 'web/test-results');
const data = path.join(artifacts, 'desktop-data');
const base = 'http://127.0.0.1:15242';
const compatibilityBase = 'http://cx-audio.test:15242';
const delay = milliseconds => new Promise(resolve => setTimeout(resolve, milliseconds));
async function until(check, description, timeout = 15000) {
  const start = Date.now();
  while (Date.now() - start < timeout) {
    try { const result = await check(); if (result) return result; } catch { /* Server or UI still starting. */ }
    await delay(100);
  }
  throw new Error(`Timed out: ${description}`);
}

test('Fluent pages preserve audio control and live desktop appearance', { timeout: 210000 }, async () => {
  await mkdir(data, { recursive: true });
  const executable = path.join(root, 'diagnostic-build/CXAudioOutput.exe');
  assert.ok(existsSync(executable), 'Publish a Debug diagnostic-build first.');
  const app = spawn(executable, ['--demo', '--ui-smoke-test', '--port=15242'], {
    cwd: path.dirname(executable), stdio: 'ignore',
    env: { ...process.env, CXAUDIO_TEST_DATA_DIRECTORY: data, CXAUDIO_WEB_SMOKE: '1', CXAUDIO_PORT: '15242' }
  });
  let browser;
  const errors = [];
  const consoleErrors = [];
  let commandId = Date.now() % 1_000_000_000;
  async function desktop(theme, accentColor) {
    const id = ++commandId;
    await writeFile(path.join(data, 'appearance-command.json'), JSON.stringify({ id, theme, accentColor }));
    return until(async () => {
      const result = JSON.parse(await readFile(path.join(data, 'appearance-result.json'), 'utf8'));
      return result.id === id && result.appearance;
    }, 'native desktop command acknowledgement');
  }
  function observe(page) {
    page.on('pageerror', error => errors.push(String(error)));
    page.on('console', message => { if (message.type() === 'error') consoleErrors.push(message.text()); });
    page.on('request', request => {
      if (!request.url().startsWith(base) && !request.url().startsWith(compatibilityBase) && !/^(?:data|blob):/.test(request.url()))
        errors.push(`Unexpected external request: ${request.url()}`);
    });
  }
  async function synced(page, appearance) {
    await page.waitForFunction(expected => {
      const current = window.CXAppearance?.current;
      return current?.theme === expected.theme && current?.accentColor === expected.accentColor &&
        getComputedStyle(document.documentElement).getPropertyValue('--cx-accent-fill').trim().toLowerCase() === expected.accentFillColor.toLowerCase();
    }, appearance);
  }
  try {
    await until(async () => (await fetch(`${base}/api/health`)).ok, 'desktop HTTP service');
    const initial = await desktop('Light', '#0078D4');
    const installed = [process.env.CX_AUDIO_BROWSER,
      'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe',
      'C:/Program Files/Microsoft/Edge/Application/msedge.exe',
      'C:/Program Files/Google/Chrome/Application/chrome.exe'].find(value => value && existsSync(value));
    browser = await chromium.launch({ headless: true, executablePath: installed, args: ['--autoplay-policy=no-user-gesture-required', '--host-resolver-rules=MAP cx-audio.test 127.0.0.1', '--no-proxy-server'] });
    const desktopContext = await browser.newContext({ viewport: { width: 1440, height: 1080 }, locale: 'en-US' });
    const mobileContext = await browser.newContext({ viewport: { width: 390, height: 844 }, isMobile: true, hasTouch: true, locale: 'en-US' });
    const studio = await desktopContext.newPage();
    const receiver = await mobileContext.newPage();
    observe(studio); observe(receiver);
    let hostState;
    let audioFrames = 0;
    studio.on('websocket', socket => socket.on('framereceived', frame => {
      if (typeof frame.payload === 'string') {
        try { const payload = JSON.parse(frame.payload); if (payload.type === 'state') hostState = payload; } catch {}
      }
    }));
    receiver.on('websocket', socket => socket.on('framereceived', frame => { if (typeof frame.payload !== 'string') audioFrames++; }));
    await studio.goto(`${base}/studio`);
    await studio.waitForFunction(() => customElements.get('fluent-button') && document.documentElement.classList.contains('fluent-ready'));
    await synced(studio, initial);
    const desktopSlider = await studio.locator('#master-volume').evaluate(element => {
      const row = element.closest('.slider-row').getBoundingClientRect();
      const box = element.getBoundingClientRect();
      const style = getComputedStyle(element);
      return { centered: Math.abs((box.top + box.height / 2) - (row.top + row.height / 2)) <= 1, appearance: style.appearance, margin: style.margin, alignSelf: style.alignSelf, fill: style.getPropertyValue('--fill') };
    });
    assert.deepEqual(desktopSlider, { centered: true, appearance: 'none', margin: '0px', alignSelf: 'center', fill: '100%' }, 'Desktop volume slider is centered and uses the custom track');
    const state = await until(() => hostState?.accessCode && hostState, 'authenticated local studio state');
    const qr = PNG.sync.read(await studio.locator('#qr-target').screenshot());
    assert.ok(qr.width >= 112 && qr.height >= 112, 'QR artwork is not reduced to icon dimensions');
    const decoded = jsQR(new Uint8ClampedArray(qr.data), qr.width, qr.height);
    assert.ok(decoded, 'Rendered QR code is scannable');
    assert.equal(new URL(decoded.data).searchParams.get('code'), state.accessCode);
    const wired = await mobileContext.newPage();
    observe(wired);
    await wired.goto(`${base}/?wired=1&code=${state.accessCode}`);
    await wired.locator('#join-form').waitFor({ state: 'visible' });
    await synced(wired, initial);
    assert.ok(wired.url().includes('wired=1'), 'Android wired route is not redirected to studio');
    let wiredHello;
    let wiredProfile;
    let wiredStream;
    let wiredFrameBytes = 0;
    wired.on('websocket', socket => socket.on('framereceived', frame => {
      if (typeof frame.payload === 'string') {
        const payload = JSON.parse(frame.payload);
        if (payload.type === 'hello') wiredHello = payload;
        if (payload.type === 'profile') wiredProfile = payload;
        if (payload.type === 'stream') wiredStream = payload;
      } else wiredFrameBytes = frame.payload.length;
    }));
    await wired.locator('#device-name').fill('USB 低延迟测试');
    await wired.locator('#start-button').click();
    await wired.locator('#player-view').waitFor({ state: 'visible' });
    await until(() => wiredHello && wiredFrameBytes > 0, 'USB handshake and PCM frames');
    assert.equal(wiredHello.frameMs, 5);
    assert.equal(wiredHello.wired, true);
    assert.ok(wiredFrameBytes <= 48000 * 4 * 0.005, 'PCM packet duration is at most 5 ms');
    await mobileContext.addInitScript(() => {
      localStorage.setItem('cx.background', 'false');
      globalThis.__mediaActions = {};
      const original = navigator.mediaSession.setActionHandler.bind(navigator.mediaSession);
      navigator.mediaSession.setActionHandler = (name, callback) => {
        globalThis.__mediaActions[name] = callback; original(name, callback);
      };
    });
    await receiver.goto(`${base}/listen?code=${state.accessCode}`);
    await receiver.waitForFunction(() => customElements.get('fluent-switch') && customElements.get('fluent-button') && document.documentElement.classList.contains('fluent-ready'));
    await synced(receiver, initial);
    await studio.screenshot({ path: path.join(artifacts, 'studio-light.png'), fullPage: true });
    await receiver.screenshot({ path: path.join(artifacts, 'receiver-join-light.png'), fullPage: true });

    // Changes run through actual WinUI appearance methods, not a simulated web setter.
    const dark = await desktop('Dark', '#D83B01');
    await Promise.all([synced(studio, dark), synced(receiver, dark)]);
    assert.equal(await studio.locator('fluent-button[appearance=primary]').first().evaluate(element => getComputedStyle(element).backgroundColor),
      'rgb(' + dark.accentFillColor.match(/[a-f0-9]{2}/gi).map(part => parseInt(part, 16)).join(', ') + ')');
    const expectedText = foregroundOn(dark.accentFillColor);
    const expectedTextRgb = 'rgb(' + expectedText.match(/[a-f0-9]{2}/gi).map(part => parseInt(part, 16)).join(', ') + ')';
    await receiver.waitForFunction(expected => getComputedStyle(document.querySelector('#start-button')).color === expected, expectedTextRgb);
    assert.equal(await receiver.locator('#start-button').evaluate(element => getComputedStyle(element).color), expectedTextRgb, 'Actual Fluent primary label meets contrast requirements');
    await studio.screenshot({ path: path.join(artifacts, 'studio-dark.png'), fullPage: true });
    await receiver.screenshot({ path: path.join(artifacts, 'receiver-join-dark.png'), fullPage: true });

    await receiver.locator('#device-name').fill('Fluent 手机测试');
    await receiver.locator('#start-button').click();
    await receiver.locator('#player-view').waitFor({ state: 'visible' });
    const receiverSlider = await receiver.locator('#local-volume').evaluate(element => {
      const deck = element.closest('.volume-deck').getBoundingClientRect();
      const box = element.getBoundingClientRect();
      const style = getComputedStyle(element);
      return { centered: Math.abs((box.top + box.height / 2) - (deck.top + deck.height / 2)) <= 1, appearance: style.appearance, margin: style.margin, alignSelf: style.alignSelf };
    });
    assert.deepEqual(receiverSlider, { centered: true, appearance: 'none', margin: '0px', alignSelf: 'center' }, 'Receiver volume slider is centered and uses the custom track');
    await until(() => audioFrames >= 5, 'binary demo audio frames');
    await until(() => hostState?.receivers?.some(item => item.name === 'Fluent 手机测试'), 'receiver reflected in host state');
    const compatibilityContext = await browser.newContext({ viewport: { width: 390, height: 844 }, isMobile: true, hasTouch: true });
    const compatibility = await compatibilityContext.newPage();
    observe(compatibility);
    let compatibilityFrames = 0;
    compatibility.on('websocket', socket => socket.on('framereceived', frame => { if (typeof frame.payload !== 'string') compatibilityFrames++; }));
    await compatibility.goto(`${compatibilityBase}/listen?code=${state.accessCode}`);
    assert.equal(await compatibility.evaluate(() => isSecureContext), false, 'Test a real insecure LAN-style origin');
    await synced(compatibility, dark);
    await compatibility.locator('#device-name').fill('HTTP 兼容模式测试');
    await compatibility.locator('#start-button').click();
    await compatibility.locator('#player-view').waitFor({ state: 'visible' });
    await until(() => compatibilityFrames >= 5, 'HTTP compatibility audio frames');
    await compatibility.locator('#details-button').click();
    assert.match(await compatibility.locator('#detail-engine').textContent(), /ScriptProcessor|兼容/);
    await compatibility.locator('#close-details').click();
    await compatibility.screenshot({ path: path.join(artifacts, 'receiver-http-compatibility.png'), fullPage: true });
    await receiver.locator('#details-button').click();
    await receiver.locator('#details-dialog').waitFor({ state: 'visible' });
    assert.equal(await receiver.locator('#background-toggle').evaluate(element => element.checked && element.disabled), true,
      'Background online policy is always on, including migrated preferences');
    assert.equal(await receiver.evaluate(() => localStorage.getItem('cx.background')), '1');
    await receiver.locator('#close-details').click();
    await receiver.screenshot({ path: path.join(artifacts, 'receiver-playing-dark.png'), fullPage: true });
    await studio.screenshot({ path: path.join(artifacts, 'studio-with-receiver.png'), fullPage: true });

    await studio.locator('#mute-button').click();
    await until(() => hostState?.muted === true, 'Fluent master mute control');
    await studio.locator('#mute-button').click();
    await until(() => hostState?.muted === false, 'Fluent master unmute control');
    await studio.locator('[data-profile=low]').click();
    await until(() => hostState?.profile === 'low', 'Fluent latency profile control');
    await until(() => wiredProfile?.targetMs === 6, 'USB receives its 6 ms target');
    await receiver.locator('#details-button').click();
    await receiver.waitForFunction(() => /缓冲目标 (3\d|[4-7]\d|80) ms/.test(document.querySelector('#detail-profile').textContent));
    await receiver.locator('#close-details').click();
    await compatibility.locator('#details-button').click();
    await compatibility.waitForFunction(() => /缓冲目标 (3\d|[4-7]\d|80) ms/.test(document.querySelector('#detail-profile').textContent));
    await compatibility.locator('#close-details').click();
    await until(() => hostState?.receivers?.some(item => item.name === 'HTTP 兼容模式测试' && item.playback === 'playing'),
      'HTTP compatibility renderer produces audio in low mode');

    // Hold PC playback paused beyond the former 18-second expiry, with a hidden
    // document and media controls paused. Only explicit disconnect releases it.
    await receiver.evaluate(() => {
      Object.defineProperty(document, 'visibilityState', { configurable: true, get: () => 'hidden' });
      document.dispatchEvent(new Event('visibilitychange'));
    });
    await until(() => receiver.evaluate(() => typeof globalThis.__mediaActions?.pause === 'function'), 'media control handlers');
    await receiver.evaluate(() => globalThis.__mediaActions.pause());
    await studio.locator('#pause-button').click();
    await until(() => hostState?.capture?.paused === true, 'PC pause');
    await until(() => hostState?.receivers?.some(item => item.name === 'Fluent 手机测试' && item.playback === 'paused'), 'paused receiver stats');
    await delay(22000);
    const keepAlive = await receiver.evaluate(() => ({
      online: /已连接/.test(document.querySelector('#player-network').textContent),
      media: navigator.mediaSession.playbackState,
      audio: [...document.querySelectorAll('audio')].some(element => !element.paused && element.srcObject?.active),
      stored: localStorage.getItem('cx.background')
    }));
    assert.deepEqual(keepAlive, { online: true, media: 'playing', audio: true, stored: '1' }, 'Paused hidden receiver retains media keepalive and connection');
    assert.ok(hostState.receivers.some(item => item.name === 'Fluent 手机测试'), 'Paused mobile session still registered');
    await receiver.evaluate(() => { delete document.visibilityState; document.dispatchEvent(new Event('visibilitychange')); globalThis.__mediaActions.play(); });
    const resumeFrames = audioFrames;
    await studio.locator('#pause-button').click();
    await until(() => hostState?.capture?.paused === false && audioFrames > resumeFrames + 10, 'resume on existing receiver');
    await until(() => wiredStream?.playing === true && wiredStream.targetMs === 6, 'USB retains 6 ms target across pause/resume');
    await until(() => hostState.receivers.some(item => item.name === 'Fluent 手机测试' && item.playback === 'playing'), 'resumed audio playback');
    await wired.close();

    for (const page of [studio, receiver]) for (const width of [360, 390, 768, 1440]) {
      await page.setViewportSize({ width, height: 960 });
      const dimensions = await page.evaluate(() => ({ viewport: innerWidth, content: document.documentElement.scrollWidth }));
      assert.ok(dimensions.content <= dimensions.viewport + 1, `No horizontal overflow at ${width}px: ${JSON.stringify(dimensions)}`);
    }

    // A connected receiver follows changes without leaving playback or adding CDN traffic.
    const before = audioFrames;
    const custom = await desktop('Light', '#7C3AED');
    await Promise.all([synced(studio, custom), synced(receiver, custom)]);
    await until(() => audioFrames > before + 5, 'audio continues while the palette changes');
    await mobileContext.setOffline(true);
    const restored = await desktop('Dark', '#0099BC');
    await mobileContext.setOffline(false);
    await synced(receiver, restored);
    const system = await desktop('System', 'System');
    await Promise.all([synced(studio, system), synced(receiver, system)]);
    assert.equal((await receiver.evaluate(() => window.CXAppearance.current)).accentSource, 'System');
    assert.deepEqual(errors, [], 'No JS errors or external runtime requests');
    const csp = consoleErrors.filter(message => /Content Security Policy|Refused to|unsafe-eval/i.test(message));
    assert.deepEqual(csp, [], 'All components comply with the existing CSP');
    await writeFile(path.join(artifacts, 'results.json'), JSON.stringify({ passed: true, checks: [
      'Local Fluent components and primary colors', 'Chinese UI on English browser',
      'Actual WinUI to web SSE theme changes', 'Mobile and desktop responsive layouts',
      'Receiver form, binary audio and host controls', 'Uninterrupted playback during recoloring',
      'Offline reconnect and system accent mode', 'No CDN requests or CSP violations'
    ], audioFrames }, null, 2));
  } finally {
    await browser?.close();
    app.kill();
    await delay(400);
    if (errors.length || consoleErrors.length) await writeFile(path.join(artifacts, 'browser-console.json'), JSON.stringify({ errors, consoleErrors }, null, 2));
  }
});
