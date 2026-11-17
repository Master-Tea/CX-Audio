// Shared by AudioWorklet and the HTTP ScriptProcessor fallback.
// These are receiver buffer budgets, not measurements of acoustic latency.
globalThis.CXAudioLatency = Object.freeze({
  clampTarget(value) {
    return Math.max(4, Math.min(1200, Number(value) || 240));
  },
  maxBoost(target) {
    return target <= 8 ? Math.max(0, 24 - target)
      : target <= 30 ? Math.max(0, 80 - target) : 180;
  },
  effectiveTarget(target, boost = 0, blockMs = 0) {
    const base = this.clampTarget(target);
    // A compatibility callback needs a complete render block plus interpolation.
    return Math.max(base + Math.min(this.maxBoost(base), Math.max(0, boost)), blockMs + 0.25);
  },
  increaseBoost(target, boost) {
    return Math.min(this.maxBoost(target), boost + (target <= 8 ? 3 : target <= 30 ? 8 : 35));
  },
  resetArrival(state) {
    state.arrivalAt = null;
    state.arrivalBurstMs = 0;
    state.deliveryMs = 5;
    state.arrivalRiskAt = null;
    state.arrivalDecayAt = null;
    state.catchupPending = false;
    state.clockError = 0;
  },
  observeArrival(state, packetMs, now, blockMs = 0) {
    if (state.arrivalAt == null) {
      state.arrivalAt = now;
      state.arrivalBurstMs = packetMs;
      state.arrivalRiskAt = now;
      state.arrivalDecayAt = now;
      return;
    }
    const gap = Math.max(0, now - state.arrivalAt);
    // WebSocket/MessagePort callbacks can deliver many packets in one turn.
    // Count that burst once instead of treating its final packet as stale audio.
    if (gap > 0.25) {
      if (gap > state.baseTargetMs + this.maxBoost(state.baseTargetMs)) state.catchupPending = true;
      state.arrivalBurstMs = packetMs;
      state.arrivalAt = now;
      state.deliveryMs = Math.max(packetMs, Math.min(250, gap), state.deliveryMs * 0.995);
    } else {
      state.arrivalBurstMs += packetMs;
      state.deliveryMs = Math.max(state.deliveryMs, Math.min(250, state.arrivalBurstMs));
    }
    const needed = Math.max(gap, state.arrivalBurstMs) + Math.max(1, blockMs);
    const boost = Math.min(this.maxBoost(state.baseTargetMs), Math.max(0, needed - state.baseTargetMs));
    if (boost > state.boostMs) {
      state.boostMs = boost;
      state.arrivalRiskAt = now;
      state.arrivalDecayAt = now;
    } else if (now - state.arrivalRiskAt > 10000 && now - state.arrivalDecayAt > 2000) {
      // Drain a temporary safety margin gradually after the link settles.
      state.boostMs = Math.max(0, state.boostMs - 1);
      state.arrivalDecayAt = now;
    }
  },
  trimCatchup(state, targetFrames) {
    if (!state.catchupPending) return;
    state.catchupPending = false;
    // Run once at render time, after all messages in a recovery burst arrived.
    if (state.writePos - state.readPos > targetFrames) {
      state.readPos = state.writePos - targetFrames;
      state.fade = 0;
      state.overruns += 1;
    }
  },
  clockTarget(target, boost, deliveryMs, blockMs) {
    // Occupancy is sampled after rendering and oscillates with packet batches.
    // Aim at its midpoint so normal batching is not learned as clock drift.
    return Math.max(blockMs, this.effectiveTarget(target, boost, blockMs) - deliveryMs / 2);
  },
  clockCorrection(state, error, elapsed) {
    // Follow clock drift, not the sawtooth of individual delivery batches.
    state.clockError += (error - state.clockError) * Math.min(1, elapsed / 0.25);
    state.rateBias = Math.max(-0.005, Math.min(0.005, state.rateBias + state.clockError * elapsed * 0.001));
    return Math.max(-0.01, Math.min(0.01, state.clockError * 0.006 + state.rateBias));
  },
  backlogMs(target) {
    // Allow ordinary delivery batches while bounding delayed replay after a stall.
    return target <= 8 ? 24 : target <= 30 ? 80 : 350;
  }
});
