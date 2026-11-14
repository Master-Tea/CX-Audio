import { build } from 'esbuild';
import { mkdir, copyFile, writeFile, readFile, readdir } from 'node:fs/promises';
import { fileURLToPath } from 'node:url';
import path from 'node:path';

const root = path.dirname(fileURLToPath(import.meta.url));
const output = path.resolve(root, '../CX Audio/wwwroot/assets');
await mkdir(output, { recursive: true });
await build({
  entryPoints: [path.join(root, 'src/fluent.js')],
  outfile: path.join(output, 'fluent-ui.js'),
  bundle: true, format: 'esm', target: ['es2020'], minify: true,
  legalComments: 'external', charset: 'utf8'
});
const notices = [];
for (const pkg of ['@fluentui/web-components', '@fluentui/tokens', '@microsoft/fast-element', '@microsoft/focusgroup-polyfill', 'tslib']) {
  const folder = path.join(root, 'node_modules', pkg);
  const metadata = JSON.parse(await readFile(path.join(folder, 'package.json'), 'utf8'));
  const licenseFile = (await readdir(folder)).find(name => /^license(?:\.(?:txt|md))?$/i.test(name));
  if (!licenseFile && metadata.license !== 'MIT') throw new Error(`Missing license: ${pkg}`);
  // Microsoft packages declaring MIT sometimes omit their common license file.
  const license = await readFile(licenseFile ? path.join(folder, licenseFile) : path.join(root, 'node_modules/@fluentui/tokens/LICENSE'), 'utf8');
  notices.push(`${pkg} ${metadata.version}\n${license}`);
}
await writeFile(path.join(output, 'fluent-ui-NOTICES.txt'), notices.join('\n\n----------------------------------------\n\n'));
for (const name of ['logo.png', 'favicon.png', 'cover.png'])
  await copyFile(path.resolve(root, '../CX Audio/Assets/logo.png'), path.join(output, name));
console.log('Built offline Fluent UI components and third-party notices.');
