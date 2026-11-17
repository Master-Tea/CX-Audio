// Shared by AudioWorklet and the HTTP ScriptProcessor fallback.
// These are receiver buffer budgets, not measurements of acoustic latency.
globalThis.CXAudioLatency = Object.freeze({
  clampTarget(value) {
    return Math.max(4, Math.min(1200, Number(value) || 240));
  },
  maxBoost(target) {
    return target <= 8 ? Math.max(0, 8 - target)
      : target <= 30 ? Math.max(0, 40 - target) : 180;
  },
  effectiveTarget(target, boost = 0, blockMs = 0) {
    const base = this.clampTarget(target);
    // A compatibility callback needs a complete render block plus interpolation.
    return Math.max(base + Math.min(this.maxBoost(base), Math.max(0, boost)), blockMs + 0.25);
  },
  increaseBoost(target, boost) {
    return Math.min(this.maxBoost(target), boost + (target <= 8 ? 1 : target <= 30 ? 2 : 35));
  },
  backlogMs(target) {
    return target <= 8 ? 10 : target <= 30 ? 10 : 350;
  }
});
