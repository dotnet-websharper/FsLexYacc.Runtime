import { build } from 'esbuild'
import { readFileSync, writeFileSync, copyFileSync, mkdirSync } from 'fs'

const name = 'WebSharper.FsLexYacc.Sample'

mkdirSync('../dist/Scripts', { recursive: true })

// Bundle the WebSharper-generated module (and the WebSharper runtime) into a
// single IIFE for the static page.
await build({
  entryPoints: [`./build/${name}.js`],
  bundle: true,
  minify: true,
  format: 'iife',
  outfile: `../dist/Scripts/${name}.min.js`,
  globalName: 'wsbundle'
})

copyFileSync('./site.css', '../dist/site.css')

// Fold the WebSharper head (linked resources) into the page.
const head = readFileSync(`./build/${name}.head.html`, 'utf8')
const html = readFileSync('./index.html', 'utf8').replace('<!-- ws-head -->', head)
writeFileSync('../dist/index.html', html)

console.log('Bundled:', name)
