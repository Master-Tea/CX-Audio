// Pure theme mapping shared by the browser and regression tests.
export function validAppearance(value) {
  return value && ['Light', 'Dark'].includes(value.theme) &&
    ['accentColor', 'accentFillColor', 'accentTextColor'].every(key => /^#[0-9a-f]{6}$/i.test(value[key] || ''));
}

export function mix(hex, target, amount) {
  const channels = color => color.match(/[a-f0-9]{2}/gi).map(channel => parseInt(channel, 16));
  const source = channels(hex);
  return '#' + channels(target).map((channel, index) =>
    Math.round(source[index] + (channel - source[index]) * amount).toString(16).padStart(2, '0')).join('');
}

export function foregroundOn(hex) {
  const channels = hex.match(/[a-f0-9]{2}/gi).map(channel => {
    const value = parseInt(channel, 16) / 255;
    return value <= .04045 ? value / 12.92 : ((value + .055) / 1.055) ** 2.4;
  });
  const luminance = channels[0] * .2126 + channels[1] * .7152 + channels[2] * .0722;
  return (luminance + .05) / .05 > 1.05 / (luminance + .05) ? '#000000' : '#ffffff';
}

export function brandRamp(accent) {
  return Object.fromEntries(Array.from({ length: 16 }, (_, index) => {
    const key = (index + 1) * 10;
    return [key, key <= 80 ? mix('#000000', accent, (key / 80) ** 1.2) : mix(accent, '#ffffff', (key - 80) / 90)];
  }));
}

export function desktopOverrides(appearance) {
  const dark = appearance.theme === 'Dark';
  const fill = appearance.accentFillColor;
  const hover = mix(fill, dark ? '#ffffff' : '#000000', .08);
  const pressed = mix(fill, dark ? '#ffffff' : '#000000', .16);
  return {
    colorBrandBackground: fill, colorBrandBackgroundHover: hover, colorBrandBackgroundPressed: pressed,
    colorBrandBackgroundSelected: pressed, colorCompoundBrandBackground: fill,
    colorCompoundBrandBackgroundHover: hover, colorCompoundBrandBackgroundPressed: pressed,
    colorBrandForeground1: appearance.accentTextColor, colorBrandForeground2: appearance.accentTextColor,
    colorBrandForegroundLink: appearance.accentTextColor,
    colorBrandForegroundLinkHover: appearance.accentTextColor, colorBrandForegroundLinkPressed: appearance.accentTextColor,
    colorCompoundBrandForeground1: fill, colorCompoundBrandForeground1Hover: hover,
    colorCompoundBrandForeground1Pressed: pressed, colorCompoundBrandStroke: fill,
    colorBrandStroke1: fill, colorBrandStroke2: mix(fill, dark ? '#202020' : '#ffffff', .7),
    colorNeutralForegroundOnBrand: foregroundOn(fill),
    colorNeutralBackground1: dark ? '#2c2c2c' : '#ffffff',
    colorNeutralBackground2: dark ? '#202020' : '#f5f6f7',
    colorNeutralForeground1: dark ? '#f5f5f5' : '#1a1a1a',
    colorNeutralForeground2: dark ? '#cecece' : '#4a4a4a',
    fontFamilyBase: '"Segoe UI Variable Text", "Segoe UI", "Microsoft YaHei UI", "Microsoft YaHei", system-ui, sans-serif',
    fontFamilyNumeric: '"Segoe UI Variable Display", "Segoe UI", system-ui, sans-serif',
    borderRadiusMedium: '4px', borderRadiusLarge: '8px'
  };
}
