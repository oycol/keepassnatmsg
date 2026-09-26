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
  let originalManifest;
  const hostManifestPath = path.join(process.env.LOCALAPPDATA || '', 'KeePassNatMsg', 'org.keepassxc.keepassxc_browser.json');
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
    if (!fs.existsSync(hostManifestPath)) throw new Error('Native messaging test manifest absent');
    originalManifest = fs.readFileSync(hostManifestPath);
    const hostManifest = JSON.parse(originalManifest.toString('utf8'));
    const registry = require('child_process').spawnSync('reg', ['query', 'HKCU\\Software\\Google\\Chrome\\NativeMessagingHosts\\org.keepassxc.keepassxc_browser', '/ve'], {encoding:'utf8'});
    if (registry.status !== 0 || !registry.stdout.includes(hostManifestPath)) throw new Error('Native host registry key missing or points elsewhere');
    if (!fs.existsSync(hostManifest.path)) throw new Error('Registered native host executable absent');
    if (hostManifest.name !== 'org.keepassxc.keepassxc_browser' || !Array.isArray(hostManifest.allowed_origins)) throw new Error('Native host manifest mismatch');
    const testOrigin = `chrome-extension://${ids[0]}/`;
    if (!hostManifest.allowed_origins.includes(testOrigin)) hostManifest.allowed_origins.push(testOrigin);
    fs.writeFileSync(hostManifestPath, JSON.stringify(hostManifest));
    const result = await workers[0].evaluate(async () => {
      return await new Promise(resolve => {
        let done = false;
        const finish = (status) => { if (!done) { done=true; resolve(status); } };
        const timer = setTimeout(() => finish('timeout'), 8000);
        try {
          const port = chrome.runtime.connectNative('org.keepassxc.keepassxc_browser');
          port.onMessage.addListener(message => { clearTimeout(timer); finish(message && message.action === 'change-public-keys' ? 'reply' : 'unexpected'); port.disconnect(); });
          port.onDisconnect.addListener(() => {
            clearTimeout(timer);
            const reason = chrome.runtime.lastError?.message || 'without-runtime-error';
            const category = /not found|not registered/i.test(reason) ? 'host-not-found' : /forbidden|not allowed|permission/i.test(reason) ? 'origin-not-allowed' : /failed to start|exited|terminated/i.test(reason) ? 'host-exited' : 'disconnected';
            finish(category);
          });
          port.postMessage({action:'change-public-keys', publicKey:btoa(String.fromCharCode(...Array(32).fill(1))), nonce:btoa(String.fromCharCode(...Array(24).fill(3))), clientID:btoa(String.fromCharCode(...Array(24).fill(2)))});
        } catch (_) { clearTimeout(timer); finish('error'); }
      });
    });
    if (result !== 'reply') {
      const logPath = path.join(process.env.LOCALAPPDATA || '', 'KeePassNatMsg', 'proxy.log');
      const newLog = fs.existsSync(logPath) ? fs.readFileSync(logPath, 'utf8').slice(-12000) : '';
      const proxyState = !newLog ? 'no-proxy-log' : /Connected to pipe successfully!/.test(newLog) ? 'pipe-connected' : /Connecting to named pipe:/.test(newLog) ? 'pipe-connection-attempted' : /Proxy started/.test(newLog) ? 'proxy-started' : 'log-without-start';
      throw new Error(`Extension-origin native messaging handshake failed: ${result}; proxy=${proxyState}`);
    }
    fs.mkdirSync(resultDir, {recursive:true});
    fs.writeFileSync(path.join(resultDir, 'extension-probe.json'), JSON.stringify({version:'1.10.4',zipSha256:expected,extensionLoaded:true,runtimeId:ids[0],profileIsolated:true,nativeMessagingVerified:true,getLoginsVerified:false},null,2));
    console.log(`Isolated extension native messaging handshake passed; runtime ID=${ids[0]}; get-logins UNVERIFIED`);
  } finally {
    if (context) await context.close();
    if (originalManifest) fs.writeFileSync(hostManifestPath, originalManifest);
    fs.rmSync(profile, {recursive:true,force:true});
  }
})().catch(e => { console.error(`Extension probe failed: ${e.message.split('\n')[0]}`); process.exitCode=1; });
