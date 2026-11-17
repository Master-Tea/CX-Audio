import test from 'node:test';
import assert from 'node:assert/strict';
import vm from 'node:vm';
import { readFileSync } from 'node:fs';

const assets = new URL('../../CX Audio/wwwroot/assets/', import.meta.url);
const policy = readFileSync(new URL('audio-latency.js', assets), 'utf8');
const worklet = readFileSync(new URL('audio-worklet.js', assets), 'utf8').replace(/^import .*;\r?\n/m, '');
const receiver = readFileSync(new URL('receiver.js', assets), 'utf8');
const fallback = receiver.slice(receiver.indexOf('  function createScriptProcessorPlayer(context) {'), receiver.indexOf('  function updateEngineUi() {'));

function engine(kind, target, outputRate = 48000, sourceRate = 48000) {
  let Processor, callback, block;
  const stats = [];
  const sandbox = vm.createContext({
    sampleRate: outputRate, currentTime: 0, ArrayBuffer, DataView, Float32Array,
    performance: { now: () => sandbox.currentTime * 1000 },
    AudioWorkletProcessor: class { constructor() { this.port = { postMessage: data => stats.push(data) }; } },
    registerProcessor: (_, value) => { Processor = value; }
  });
  vm.runInContext(policy, sandbox);
  let player, render;
  if (kind === 'worklet') {
    vm.runInContext(worklet, sandbox);
    const processor = new Processor();
    block = 128;
    player = {
      sendAudio: buffer => processor.handleMessage({ type: 'audio', buffer }),
      setConfig: (rate, channels, targetMs) => processor.handleMessage({ type: 'config', sampleRate: rate, channels, targetMs }),
      setTarget: targetMs => processor.handleMessage({ type: 'target', targetMs }),
      pause: () => processor.handleMessage({ type: 'pause' }),
      resume: () => processor.handleMessage({ type: 'resume' })
    };
    render = (left, right) => processor.process([], [[left, right]]);
  } else {
    sandbox.latencyPolicy = sandbox.CXAudioLatency;
    sandbox.context = { sampleRate: outputRate, createScriptProcessor(size) {
      block = size;
      return { bufferSize: size, set onaudioprocess(value) { callback = value; }, disconnect() {} };
    }};
    vm.runInContext(fallback + '\n globalThis.player = createScriptProcessorPlayer(context);', sandbox);
    player = sandbox.player;
    player.onMessage = data => stats.push(data);
    render = (left, right) => callback({ outputBuffer: { numberOfChannels: 2, getChannelData: channel => channel ? right : left } });
  }
  player.setConfig(sourceRate, 2, target);
  return { player, stats, block, sandbox, render, sourceRate, outputRate };
}

// Runs actual renderer code with a PCM impulse; excludes network and hardware latency.
function simulate(e, { duration = 2, packetMs = 5, deliveryMs = 0, stallStart = -1, stallEnd = -1, clockPpm = 0 } = {}) {
  const packetFrames = Math.round(e.sourceRate * packetMs / 1000);
  let sourceFrame = 0, nextPacketAt = 0, firstImpulse = null, nonzero = 0;
  let silenceFrames = 0, longestSilence = 0;
  const renderMs = e.block * 1000 / e.outputRate;
  const startTime = e.sandbox.currentTime;
  for (let ms = 0; ms < duration * 1000; ms += renderMs) {
    e.sandbox.currentTime = startTime + ms / 1000;
    if (!(ms >= stallStart && ms < stallEnd)) {
      const batchMs = typeof deliveryMs === 'function' ? deliveryMs(ms) : deliveryMs;
      const deliveredUntil = batchMs > 0 ? Math.floor(ms / batchMs) * batchMs : ms;
      while (nextPacketAt <= deliveredUntil) {
        const buffer = new ArrayBuffer(packetFrames * 4);
        const view = new DataView(buffer);
        for (let i = 0; i < packetFrames; i++) {
          const at = (sourceFrame + i) / e.sourceRate;
          const sample = at >= 0.1 && at < 0.105 ? 16000 : at >= 0.2 ? Math.round(Math.sin(at * Math.PI * 440) * 8000) : 0;
          view.setInt16(i * 4, sample, true); view.setInt16(i * 4 + 2, -sample, true);
        }
        sourceFrame += packetFrames;
        nextPacketAt += packetMs / (1 + clockPpm / 1e6);
        e.player.sendAudio(buffer);
      }
    }
    const left = new Float32Array(e.block), right = new Float32Array(e.block);
    e.render(left, right);
    for (let i = 0; i < e.block; i++) {
      assert.ok(Number.isFinite(left[i]) && Number.isFinite(right[i]), 'finite PCM');
      assert.ok(Math.abs(left[i] + right[i]) < 0.00001, 'stereo channel polarity preserved');
      if (firstImpulse === null && Math.abs(left[i]) > 0.1) firstImpulse = ms + i * 1000 / e.outputRate;
      if (Math.abs(left[i]) > 0.01) nonzero++;
      if (ms > 1000) {
        silenceFrames = Math.abs(left[i]) < 0.00001 ? silenceFrames + 1 : 0;
        longestSilence = Math.max(longestSilence, silenceFrames);
      }
    }
  }
  return { delayMs: firstImpulse - 100, firstImpulse, nonzero, longestSilenceMs: longestSilence * 1000 / e.outputRate };
}

for (const rate of [44100, 48000, 96000]) {
  for (const [kind, target, budget] of [['worklet', 6, 15], ['worklet', 30, 60], ['script', 30, 60]]) {
    test(`${kind} renders a PCM impulse within ${budget} ms at ${rate} Hz`, () => {
      const e = engine(kind, target, rate);
      const result = simulate(e);
      assert.ok(result.firstImpulse !== null && result.delayMs >= 0 && result.delayMs < budget, JSON.stringify(result));
      assert.ok(result.nonzero > rate, 'continuous decoded output after priming');
      const stats = e.stats.filter(item => item.type === 'stats').at(-1);
      assert.equal(stats.underruns, 0, 'no underruns on an evenly paced link');
      assert.ok(stats.targetMs <= (target === 6 ? 8 : 40), JSON.stringify(stats));
    });
  }
  test(`USB handles 10 ms Windows engine deliveries at ${rate} Hz`, () => {
    const e = engine('worklet', 6, rate);
    const result = simulate(e, { deliveryMs: 10 });
    assert.ok(result.firstImpulse !== null && result.delayMs < 15, JSON.stringify(result));
    assert.ok(result.nonzero > rate, 'continuous output with engine packet batching');
    const stats = e.stats.filter(item => item.type === 'stats').at(-1);
    assert.ok(stats.underruns < 5, JSON.stringify(stats));
  });
}

for (const kind of ['worklet', 'script']) {
  test(`${kind} recovers from stalls without accumulating old audio`, () => {
    const e = engine(kind, 30);
    simulate(e, { duration: 3, stallStart: 500, stallEnd: 800 });
    const stats = e.stats.filter(item => item.type === 'stats').at(-1);
    assert.ok(stats.underruns > 0, 'simulated outage is visible');
    assert.ok(stats.overruns > 0, 'old backlog is discarded on recovery');
    assert.ok(stats.targetMs <= 80 && stats.bufferMs <= 90, JSON.stringify(stats));
    assert.equal(stats.playing, true, 'recovers to playback');
    e.player.setTarget(6);
    simulate(e, { duration: 1 });
    const low = e.stats.filter(item => item.type === 'stats').at(-1);
    assert.ok(low.targetMs <= (kind === 'script' ? 24 : 8), 'switching profile clears the previous boost within the render-block floor');
    e.player.pause(); e.sandbox.currentTime += 1;
    const left = new Float32Array(e.block), right = new Float32Array(e.block);
    e.render(left, right);
    assert.ok(left.every(value => value === 0), 'paused output is silent');
    e.player.resume();
  });
}

for (const [kind, target, deliveryMs, ceiling] of [
  ['worklet', 30, 45, 80], ['script', 30, 45, 80], ['worklet', 6, 15, 24]
]) {
  test(`${kind} low mode adapts to recurring ${deliveryMs} ms delivery batches`, () => {
    const e = engine(kind, target);
    const result = simulate(e, { duration: 8, deliveryMs });
    const stats = e.stats.filter(item => item.type === 'stats').at(-1);
    assert.ok(stats.underruns <= 3, `repeated starvation: ${JSON.stringify(stats)}`);
    assert.ok(stats.overruns <= 3, `normal batches discarded: ${JSON.stringify(stats)}`);
    assert.ok(result.longestSilenceMs < 2, `audible gaps after warmup: ${JSON.stringify(result)}`);
    assert.ok(stats.targetMs <= ceiling && stats.bufferMs <= ceiling + deliveryMs + 10, JSON.stringify(stats));
    console.log(`${kind}, ${deliveryMs} ms batches: ${stats.underruns} underruns, ${stats.overruns} backlog trims, target ${stats.targetMs} ms, longest silence ${result.longestSilenceMs.toFixed(2)} ms`);
  });
}

for (const kind of ['worklet', 'script']) {
  test(`${kind} releases temporary jitter margin after the link settles`, () => {
    const e = engine(kind, 30);
    const result = simulate(e, { duration: 30, deliveryMs: ms => ms < 4000 ? 45 : 0 });
    const reports = e.stats.filter(item => item.type === 'stats');
    const peak = Math.max(...reports.map(item => item.targetMs));
    const final = reports.at(-1);
    assert.ok(peak > 40 && final.targetMs < peak - 3, JSON.stringify({ peak, final }));
    assert.ok(final.underruns <= 3 && final.overruns <= 3, JSON.stringify(final));
    assert.ok(result.longestSilenceMs < 2, JSON.stringify(result));
  });
  for (const clockPpm of [-1000, 1000]) {
    test(`${kind} stays continuous with ${clockPpm} ppm clock drift and 20 ms deliveries`, () => {
      const e = engine(kind, 30, 44100);
      const result = simulate(e, { duration: 30, deliveryMs: 20, clockPpm });
      const stats = e.stats.filter(item => item.type === 'stats').at(-1);
      assert.equal(stats.underruns, 0, JSON.stringify(stats));
      assert.equal(stats.overruns, 0, JSON.stringify(stats));
      assert.ok(result.longestSilenceMs < 2 && stats.bufferMs < 100, JSON.stringify({ stats, result }));
    });
  }
}
