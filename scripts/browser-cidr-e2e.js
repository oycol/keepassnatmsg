'use strict';
// Integration entrypoint: parent supplies a *new* opened fixture DB and KeePass PID.
// node scripts/browser-cidr-e2e.js <unpacked-official-extension> <result-dir> <fixture.kdbx> <keepass-pid> <fixture-title>
// Fixture contains CIDR:127.0.0.0/8 entry (E2E_USER/E2E_PASSWORD) and exact IP http://127.0.0.1 entry (E2E_EXACT_USER/E2E_EXACT_PASSWORD).
const fs = require('fs');
const os = require('os');
const path = require('path');
const crypto = require('crypto');
const http = require('http');
const { spawnSync, spawn } = require('child_process');
const { chromium } = require(path.join(os.tmpdir(), 'keepass-playwright-cli', 'node_modules', 'playwright'));
const hash = 'd5b780e28870deb8da260311bf141ac3a7a88d142b4b9e5c58156270c10ea3f7';
const [ext, resultDir, dbPath, pidArg, title] = process.argv.slice(2);
const user = process.env.E2E_USER;
const password = process.env.E2E_PASSWORD;
const exactUser = process.env.E2E_EXACT_USER || 'exact_tester';
const exactPassword = process.env.E2E_EXACT_PASSWORD || 'SyntheticExact2026!';
const exactTitle = process.env.E2E_EXACT_TITLE || 'Exact IP E2E';
function assert(ok, message) { if (!ok) throw Error(message); }
const zip = path.join(os.tmpdir(), 'kpxc-browser.zip');
assert(ext && resultDir && dbPath && title && /^\d+$/.test(pidArg || '') && user && password && exactUser && exactPassword && /^[A-Za-z0-9 _.-]{1,80}$/.test(title) && /^[A-Za-z0-9 _.-]{1,80}$/.test(exactTitle), 'Fixture arguments absent or invalid');
assert(path.resolve(resultDir) !== path.resolve(path.dirname(dbPath)), 'Evidence directory must differ from fixture directory');
fs.rmSync(path.join(resultDir, 'browser-cidr-e2e.json'), {force:true});
assert(fs.statSync(dbPath).isFile() && /^keepass-cidr-e2e-[a-f0-9]{16,}\.kdbx$/i.test(path.basename(dbPath)) && Date.now() - fs.statSync(dbPath).birthtimeMs < 3600000, 'Fixture must be freshly created and uniquely named');
assert(fs.existsSync(zip) && crypto.createHash('sha256').update(fs.readFileSync(zip)).digest('hex') === hash, 'Official archive mismatch');
const manifest = JSON.parse(fs.readFileSync(path.join(ext, 'manifest.json'), 'utf8'));
assert(manifest.version === '1.10.4' && manifest.permissions.includes('nativeMessaging'), 'Extension version mismatch');
const hostPath = path.join(process.env.LOCALAPPDATA || '', 'KeePassNatMsg', 'org.keepassxc.keepassxc_browser.json');
const profile = fs.mkdtempSync(path.join(os.tmpdir(), 'keepass-cidr-profile-'));
const association = 'E2E-' + crypto.randomBytes(12).toString('hex');
const pageHtml = '<!doctype html><html><body><form action="/login" method="post"><input id="user" name="username" autocomplete="username"><input id="pass" name="password" type="password" autocomplete="current-password"><button type="button">Sign in</button></form></body></html>';
let context, serverExact, serverCidr, exactPort, cidrPort, approval, original;
function approve(phase, expectedHost, expectedTitle) {
  const hostVal = expectedHost || `127.0.0.1:${exactPort || 0}`;
  const titleVal = expectedTitle || `${title} - ${user}`;
  const args = ['-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', path.join(__dirname, 'approve-browser-e2e.ps1'),
    '-Phase', phase, '-KeePassPid', pidArg, '-DatabasePath', dbPath, '-ExpectedHost', hostVal,
    '-ExpectedTitle', titleVal, '-AssociationName', association];
  approval = spawn('powershell.exe', args, { stdio: ['ignore', 'pipe', 'pipe'], windowsHide: true });
  let done = false;
  const result = new Promise(resolve => {
    approval.stdout.on('data', d => { const t = String(d).trim(); if (t) console.error(`approval-out: ${t.slice(0, 120)}`); });
    approval.stderr.on('data', d => { const t = String(d).trim(); if (t) console.error(`approval-err: ${t.slice(0, 160)}`); });
    approval.once('exit', (code) => { done = true; resolve(code === null ? 1 : code); });
    approval.once('error', () => { done = true; resolve(1); });
    // Hard safety: the approval script's own deadline is 35s; never wait
    // indefinitely for a Windows child process to report its exit.
    setTimeout(() => { if (!done) { try { approval.kill(); } catch (_) {} done = true; resolve(1); } }, 60000);
  });
  return { result, isDone: () => done };
}

async function withApproval(phase, action, expectedHost, expectedTitle) {
  const gate = approve(phase, expectedHost, expectedTitle);
  try {
    const [_, code] = await Promise.all([action(), gate.result]);
    assert(code === 0, `${phase} approval failed`);
  } finally { if (!gate.isDone()) approval.kill(); approval = null; }
}

function createServer() {
  return http.createServer((req, res) => {
    res.writeHead(200, {'Content-Type':'text/html; charset=utf-8', 'Cache-Control':'no-store'});
    res.end(pageHtml);
  });
}

function listen(srv, host, port = 0) {
  return new Promise((resolve, reject) => {
    srv.once('error', reject);
    srv.listen(port, host, () => {
      srv.removeListener('error', reject);
      resolve(srv.address().port);
    });
  });
}

async function serve() {
  serverExact = createServer();
  exactPort = await listen(serverExact, '127.0.0.1', 0);

  serverCidr = createServer();
  try {
    // Attempt binding to the same port on 127.0.0.2 first
    cidrPort = await listen(serverCidr, '127.0.0.2', exactPort);
  } catch (_) {
    // Fall back to distinct loopback port if same port is unavailable
    serverCidr = createServer();
    cidrPort = await listen(serverCidr, '127.0.0.2', 0);
  }
}

let currentStep = 'initializing';
const step = (s) => { currentStep = s; console.error('E2E step: ' + s); };
// Guard the whole E2E: if any stage stalls (a hung child process, an unresolved
// protocol promise), fail the run instead of hanging the runner job forever.
const OVERALL_DEADLINE_MS = 240000;
const overallTimer = setTimeout(() => {
  console.error(`Browser CIDR E2E failed: overall deadline exceeded at step=${currentStep}`);
  try { if (approval) approval.kill(); } catch (_) {}
  try { if (context) context.close(); } catch (_) {}
  process.exit(1);
}, OVERALL_DEADLINE_MS);
overallTimer.unref?.();

(async () => {
  await serve();
  await new Promise((resolve, reject) => {
    const req = http.get(`http://127.0.0.2:${cidrPort}/login`, (res) => { res.resume(); resolve(); });
    req.on('error', (err) => reject(new Error(`Failed to reach server on 127.0.0.2:${cidrPort}: ${err.message}`)));
  });
  await new Promise((resolve, reject) => {
    const req = http.get(`http://127.0.0.1:${exactPort}/login`, (res) => { res.resume(); resolve(); });
    req.on('error', (err) => reject(new Error(`Failed to reach server on 127.0.0.1:${exactPort}: ${err.message}`)));
  });

  context = await chromium.launchPersistentContext(profile, {
    headless: false,
    args: [
      `--disable-extensions-except=${ext}`,
      `--load-extension=${ext}`,
      '--proxy-server=direct://',
      '--proxy-bypass-list=127.0.0.1;127.0.0.2;localhost'
    ],
    timeout: 20000
  });
  let workers = context.serviceWorkers();
  if (!workers.length) { await context.waitForEvent('serviceworker', {timeout:15000}); workers = context.serviceWorkers(); }
  const ids = workers.map(w => /^chrome-extension:\/\/([a-p]{32})\//.exec(w.url())).filter(Boolean);
  assert(ids.length === 1, 'Official service worker absent or ambiguous');
  const extensionId = ids[0][1];
  assert(fs.existsSync(hostPath), 'Native host manifest missing');
  original = fs.readFileSync(hostPath);
  const host = JSON.parse(original.toString('utf8'));
  const reg = spawnSync('reg', ['query', 'HKCU\\Software\\Google\\Chrome\\NativeMessagingHosts\\org.keepassxc.keepassxc_browser', '/ve'], {encoding:'utf8'});
  assert(reg.status === 0 && reg.stdout.includes(hostPath) && host.name === 'org.keepassxc.keepassxc_browser' && fs.existsSync(host.path) && Array.isArray(host.allowed_origins), 'Native host registration invalid');
  const origin = `chrome-extension://${extensionId}/`;
  if (!host.allowed_origins.includes(origin)) host.allowed_origins.push(origin);
  fs.writeFileSync(hostPath, JSON.stringify(host));

  const options = await context.newPage();
  const consoleLog = [];
  options.on('console', msg => { if (consoleLog.length < 50) consoleLog.push({source:'options', type: msg.type(), text: msg.text().slice(0, 400)}); });
  options.on('pageerror', err => { if (consoleLog.length < 50) consoleLog.push({source:'options', type:'pageerror', text: String(err).slice(0, 400)}); });
  context.on('serviceworker', worker => { if (consoleLog.length < 50) worker.on('console', msg => consoleLog.push({source:'serviceworker', type: msg.type(), text: msg.text().slice(0, 400)})); });

  let calls = [];
  try {
    step('open-options');
    // MV3 cold-start race: the options page's init IIFE sends runtime messages
    // before the service worker registers its onMessage listener; if the send
    // fails the page hides #main-content permanently. Retry fresh loads until
    // the page initializes (deterministic, no protocol payloads involved).
    await options.goto(`${origin}options/options.html`, {waitUntil:'load'});
    const optionsReady = async () => options.waitForFunction(() => {
      const mc = document.querySelector('#main-content');
      return mc && getComputedStyle(mc).display !== 'none';
    }, {timeout:8000}).then(() => true).catch(() => false);
    let ready = await optionsReady();
    for (let attempt = 0; attempt < 5 && !ready; attempt++) {
      await options.reload({waitUntil:'load'});
      ready = await optionsReady();
    }
    if (!ready) throw new Error('options page never initialized (SW onMessage race persists)');

    step('open-connected-tab');
    // Replicate exactly what the extension's own sidebar click handler does:
    // hide every tab, then reveal the connected-databases tab. The handler is
    // registered inside initMenu at runtime; invoking it directly avoids
    // synthetic-event trust issues and hash routing differences.
    const switchTab = await options.evaluate(() => {
      const el = document.querySelector("a[href='#connected-databases']");
      if (!el) return 'sidebar link absent';
      const tabs = [].slice.call(document.querySelectorAll('div.tab'));
      const links = [].slice.call(document.querySelectorAll('.sidebar ul.nav li a'));
      links.forEach(t => t.parentElement.classList.remove('active'));
      el.parentElement.classList.add('active');
      tabs.forEach(t => t.style.display = 'none');
      const activated = document.querySelector('div.tab#tab-connected-databases');
      activated.classList.remove('d-none');
      activated.style.display = 'block';
      return 'switched';
    });
    if (switchTab !== 'switched') throw new Error('tab switch failed: ' + switchTab);
    await options.waitForFunction(() => !document.querySelector('#tab-connected-databases').className.includes('d-none'), {timeout:10000});

    step('wait-connect-button');
    await options.locator('#tab-connected-databases #connect-button').waitFor({state:'visible', timeout:30000}).catch(async (e) => {
      const diag = await options.evaluate(() => {
        const btn = document.querySelector('#tab-connected-databases #connect-button');
        const r = btn ? btn.getBoundingClientRect() : null;
        return {
          url: location.href,
          readyState: document.readyState,
          connectButtonCount: document.querySelectorAll('#connect-button').length,
          connectedTabClass: document.querySelector('#tab-connected-databases')?.className || null,
          tabClasses: [].map.call(document.querySelectorAll('div.tab'), t => ({id: t.id, cls: t.className})),
          visibleModal: !!document.querySelector('.modal.show, .modal[style*="display: block"]'),
          bodyChildren: [].map.call(document.body.children, c => c.tagName + '.' + (c.className || '')).slice(0, 12),
          buttonRect: r ? {x: r.x, y: r.y, w: r.width, h: r.height} : null,
          buttonOffsetParent: btn ? (btn.offsetParent ? btn.offsetParent.tagName + '.' + btn.offsetParent.className : null) : null,
          buttonDisplay: btn ? getComputedStyle(btn).display : null,
          buttonVisibility: btn ? getComputedStyle(btn).visibility : null,
          viewport: {w: innerWidth, h: innerHeight},
          mainContentDisplay: document.querySelector('#main-content') ? getComputedStyle(document.querySelector('#main-content')).display : 'absent',
          ancestorsHidden: (() => { let n = btn, hidden = []; while (n && n !== document.body) { if (getComputedStyle(n).display === 'none') hidden.push(n.tagName + '#' + (n.id || '')); n = n.parentElement; } return hidden; })(),
        };
      });
      diag.swProbe = await options.evaluate(async () => {
        try {
          const pong = await chrome.runtime.sendMessage({ action: 'load_settings' });
          return { load_settings: 'answered', keys: typeof pong === 'object' && pong ? Object.keys(pong).slice(0, 5) : String(pong) };
        } catch (err) {
          return { load_settings: 'failed', error: String(err).slice(0, 200) };
        }
      });
      diag.console = consoleLog.slice(0, 30);
      fs.mkdirSync(resultDir, {recursive:true});
      fs.writeFileSync(path.join(resultDir, 'options-diagnostic.json'), JSON.stringify(diag, null, 2));
      await options.screenshot({path: path.join(resultDir, 'options-diagnostic.png'), fullPage: true}).catch(() => {});
      throw e;
    });

    step('check-fresh-profile');
    assert(await options.locator('#tab-connected-databases table tbody tr:not(.clone):not(.empty)').count() === 0, 'Extension profile is not fresh');

    step('association');
    await withApproval('association', async () => {
      await options.locator('#tab-connected-databases #connect-button').click();
      await options.locator('#tab-connected-databases table tbody tr:not(.clone):not(.empty)').first().waitFor({state:'visible',timeout:35000});
    });

    step('verify-association-row');
    const rows = options.locator('#tab-connected-databases table tbody tr:not(.clone):not(.empty)');
    assert(await rows.count() === 1 && await rows.first().locator('td.identifier').innerText() === association, 'Association not reflected by official extension');

    // Use the extension's own options UI, never call its crypto/protocol APIs directly.
    step('enable-autofill');
    await options.locator('a[href="#general-settings"]').click();
    const autoFill = options.locator('#autoFillSingleEntry');
    await autoFill.check();
    await options.reload();
    assert(await options.locator('#autoFillSingleEntry').isChecked(), 'Official autofill setting not persisted');

    step('instrument-sw-getlogins');
    await workers[0].evaluate(() => {
      if (typeof keepassClient === 'undefined' || typeof keepassClient.sendMessage !== 'function') {
        throw new Error('keepassClient.sendMessage is unavailable in ServiceWorker for instrumentation');
      }
      globalThis.__e2eGetLoginsCalls = [];
      const orig = keepassClient.sendMessage;
      keepassClient.sendMessage = async function(action, tab, data, nonce, enableTimeout, triggerUnlock) {
        const resp = await orig.apply(this, arguments);
        if (action === 'get-logins') {
          let reqHost = '';
          try { reqHost = new URL(data && data.url).host; } catch (_) {}
          const rawEntries = Array.isArray(resp && resp.entries) ? resp.entries : [];
          // Capture only synthetic username and title identity in memory; never store passwords or secrets
          const syntheticLogins = rawEntries.map(e => ({
            name: typeof e.name === 'string' ? e.name : '',
            login: typeof e.login === 'string' ? e.login : ''
          }));
          globalThis.__e2eGetLoginsCalls.push({
            action: action,
            host: reqHost,
            entryCount: rawEntries.length,
            logins: syntheticLogins
          });
        }
        return resp;
      };
    });

    step('open-cidr-page');
    const page = await context.newPage();
    const cidrUrl = `http://127.0.0.2:${cidrPort}/login`;
    const cidrHost = `127.0.0.2:${cidrPort}`;
    // Without approval, the extension must not fill the page; DB fixture on 127.0.0.2 is a CIDR-only match.
    const cidrGate = approve('access', cidrHost, `${title} - ${user}`);
    try {
      await page.goto(cidrUrl);
      assert(await cidrGate.result === 0, 'Real KeePass access prompt absent or mismatched for CIDR fallback');
      step('wait-cidr-autofill');
      await page.waitForFunction(({user, password}) => document.querySelector('#user')?.value === user && document.querySelector('#pass')?.value === password,
        {user, password}, {timeout:20000});
    } finally { if (!cidrGate.isDone()) approval.kill(); approval = null; }
    step('verify-cidr-autofill');
    assert(await page.locator('#user').inputValue() === user && await page.locator('#pass').inputValue() === password, 'Extension did not autofill CIDR fixture values on CIDR-only IP');
    await page.close();

    step('open-exact-page');
    const exactPage = await context.newPage();
    const exactUrl = `http://127.0.0.1:${exactPort}/login`;
    const exactHost = `127.0.0.1:${exactPort}`;
    // On 127.0.0.1, both CIDR and Exact IP match, but Exact IP must suppress CIDR.
    const exactGate = approve('access', exactHost, `${exactTitle} - ${exactUser}`);
    try {
      await exactPage.goto(exactUrl);
      assert(await exactGate.result === 0, 'Real KeePass access prompt absent or mismatched for Exact IP');
      step('wait-exact-autofill');
      await exactPage.waitForFunction(({exactUser, exactPassword}) => document.querySelector('#user')?.value === exactUser && document.querySelector('#pass')?.value === exactPassword,
        {exactUser, exactPassword}, {timeout:20000});
    } finally { if (!exactGate.isDone()) approval.kill(); approval = null; }
    step('verify-exact-autofill');
    const exactFilledUser = await exactPage.locator('#user').inputValue();
    const exactFilledPass = await exactPage.locator('#pass').inputValue();
    assert(exactFilledUser === exactUser && exactFilledPass === exactPassword, 'Extension did not autofill Exact IP fixture values');
    assert(exactFilledUser !== user, 'CIDR credential was not suppressed by Exact IP credential');
    await exactPage.close();

    step('verify-get-logins-evidence');
    calls = await workers[0].evaluate(() => globalThis.__e2eGetLoginsCalls || []);
    assert(Array.isArray(calls) && calls.length >= 2, 'Official extension did not call get-logins for both requests');
    const cidrCall = calls.find(c => c.host && c.host.startsWith(`127.0.0.2:${cidrPort}`));
    const exactCall = calls.find(c => c.host && c.host.startsWith(`127.0.0.1:${exactPort}`));
    assert(cidrCall && cidrCall.entryCount === 1, 'CIDR get-logins call missing or returned unexpected entry count');
    assert(exactCall && exactCall.entryCount === 1, 'Exact IP get-logins call missing or returned unexpected entry count (CIDR not suppressed)');
    assert(cidrCall.logins && cidrCall.logins.length === 1 && cidrCall.logins[0].login === user,
      'CIDR get-logins call did not return expected synthetic user identity');
    assert(exactCall.logins && exactCall.logins.length === 1 && exactCall.logins[0].login === exactUser,
      'Exact IP get-logins call did not return expected synthetic exactUser identity');

    step('write-evidence');
  } catch (e) { throw new Error(`step=${currentStep}: ${e && e.message ? e.message.split('\n')[0] : e}`); }
  fs.mkdirSync(resultDir, {recursive:true});
  fs.writeFileSync(path.join(resultDir, 'browser-cidr-e2e.json'), JSON.stringify({
    version: '1.10.4',
    zipSha256: hash,
    associationUi: true,
    accessUi: true,
    encryptedGetLoginsViaOfficialExtension: true,
    cidrAutofill: true,
    cidrFallbackVerified: true,
    exactIpPriorityVerified: true,
    cidrSuppressedByExactIp: true,
    getLoginsCallsCount: calls.length,
    profileIsolated: true
  }, null, 2));
  console.log('Official 1.10.4 extension association, access approval, CIDR fallback, and Exact IP priority suppression autofill passed');
})().catch(e => { console.error('Browser CIDR E2E failed: ' + String(e && e.message ? e.message.split('\n')[0] : e)); process.exitCode = 1; })
  .finally(async () => {
    if (approval) approval.kill();
    if (context) await context.close().catch(() => { process.exitCode=1; });
    if (original) { try { fs.writeFileSync(hostPath, original); } catch (_) { process.exitCode=1; console.error('Native host manifest restoration failed'); } }
    if (serverExact) await new Promise(resolve => serverExact.close(resolve)).catch(() => {});
    if (serverCidr) await new Promise(resolve => serverCidr.close(resolve)).catch(() => {});
    fs.rmSync(profile, {recursive:true,force:true});
  });
