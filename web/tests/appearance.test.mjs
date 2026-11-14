import test from 'node:test';
import assert from 'node:assert/strict';
import { validAppearance, brandRamp, desktopOverrides } from '../src/appearance.js';

function luminance(hex) {
  const rgb = hex.match(/[a-f0-9]{2}/gi).map(part => {
    const value = parseInt(part, 16) / 255;
    return value <= .04045 ? value / 12.92 : ((value + .055) / 1.055) ** 2.4;
  });
  return rgb[0] * .2126 + rgb[1] * .7152 + rgb[2] * .0722;
}

test('Only valid desktop modes and six-digit color values reach the CSS theme', () => {
  const appearance = { theme: 'Dark', accentColor: '#0078D4', accentFillColor: '#73b5e7', accentTextColor: '#a6d0f0' };
  assert.ok(validAppearance(appearance));
  for (const invalid of [null, {}, { ...appearance, theme: 'System' }, { ...appearance, accentColor: '#fff; background:url(x)' }])
    assert.ok(!validAppearance(invalid));
});

test('Brand ramps are ordered and preserve the desktop accent at the primary step', () => {
  for (const accent of ['#0078d4', '#d83b01', '#7c3aed', '#000000', '#ffffff', '#ffff00', '#00ff00']) {
    const ramp = brandRamp(accent);
    assert.equal(Object.keys(ramp).length, 16);
    assert.equal(ramp[80], accent);
    const values = Object.values(ramp).map(luminance);
    assert.ok(values.every((value, index) => !index || value >= values[index - 1]));
  }
});

test('Fluent primary buttons retain desktop fills with AA-readable foregrounds', () => {
  for (const theme of ['Light', 'Dark']) for (const fill of ['#0060aa', '#f7b572', '#7c3aed', '#000000', '#ffffff', '#ffff00', '#00ff00']) {
    const tokens = desktopOverrides({ theme, accentFillColor: fill, accentTextColor: fill });
    assert.equal(tokens.colorBrandBackground, fill);
    const luminances = [luminance(fill), luminance(tokens.colorNeutralForegroundOnBrand)].sort((a, b) => a - b);
    assert.ok((luminances[1] + .05) / (luminances[0] + .05) >= 4.5);
    assert.equal(tokens.colorNeutralBackground2, theme === 'Dark' ? '#202020' : '#f5f6f7');
  }
});
