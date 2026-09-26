const fs = require('fs');
const path = require('path');
const os = require('os');
const { chromium } = require(path.join(os.tmpdir(), 'keepass-playwright-cli', 'node_modules', 'playwright'));
const crypto = require('crypto');
const expected = 'd5b780e28870deb8da260311bf141ac3a7a88d142b4b9e5c58156270c10ea3f7';
const archive = path.join(os.tmpdir(), 'kpxc-browser.zip');
if (!fs.existsSync(archive) || crypto.createHash('sha256').update(fs.readFileSync(archive)).digest('hex') !== expected) throw new Error('Official extension archive mismatch');
const ext = process.argv[2];
const resultDir = process.argv[3];
const manifest = JSON.parse(fs.readFileSync(path.join(ext, 'manifest.json'), 'utf8'));
if (manifest.version !== '1.10.4' || !manifest.permissions.includes('nativeMessaging')) throw new Error('Extension manifest mismatch');
(async () => {
  const profile = fs.mkdtempSync(path.join(os.tmpdir(), 'keepass-playwright-profile-'));
  let context;
  try {
    context = await chromium.launchPersistentContext(profile, {
      headless: false,
      args: [`--disable-extensions-except=${ext}`, `--load-extension=${ext}`],
      timeout: 20000,
    });
    let workers = context.serviceWorkers();
    if (!workers.length) {
      try { await context.waitForEvent('serviceworker', {timeout: 15000}); } catch (_) {}
      workers = context.serviceWorkers();
    }
    const ids = workers.map(w => /^chrome-extension:\/\/([a-p]{32})\//.exec(w.url())).filter(Boolean).map(m => m[1]);
    if (ids.length !== 1) throw new Error('Official extension service worker was not observed');
    fs.mkdirSync(resultDir, {recursive:true});
    fs.writeFileSync(path.join(resultDir, 'extension-probe.json'), JSON.stringify({version:'1.10.4',zipSha256:expected,extensionLoaded:true,runtimeId:ids[0],profileIsolated:true,nativeMessagingVerified:false,getLoginsVerified:false},null,2));
    console.log(`Isolated extension loaded; runtime ID=${ids[0]}; native messaging and get-logins UNVERIFIED`);
  } finally {
    if (context) await context.close();
    fs.rmSync(profile, {recursive:true,force:true});
  }
})().catch(e => { console.error(`Extension probe failed: ${e.message.split('\n')[0]}`); process.exitCode=1; });
