'use strict';
// Integration entrypoint: parent supplies a *new* opened fixture DB and KeePass PID.
// node scripts/browser-cidr-e2e.js <unpacked-official-extension> <result-dir> <fixture.kdbx> <keepass-pid> <fixture-title>
// Fixture must contain exactly one entry URL CIDR:127.0.0.0/8, with E2E_USER/E2E_PASSWORD.
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
function assert(ok, message) { if (!ok) throw Error(message); }
const zip = path.join(os.tmpdir(), 'kpxc-browser.zip');
assert(ext && resultDir && dbPath && title && /^\d+$/.test(pidArg || '') && user && password && /^[A-Za-z0-9 _.-]{1,80}$/.test(title), 'Fixture arguments absent or invalid');
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
let context, server, approval, original;
function approve(phase) {
  const args = ['-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', path.join(__dirname, 'approve-browser-e2e.ps1'),
    '-Phase', phase, '-KeePassPid', pidArg, '-DatabasePath', dbPath, '-ExpectedHost', `127.0.0.1:${server.address().port}`,
    '-ExpectedTitle', `${title} - ${user}`, '-AssociationName', association];
  approval = spawn('powershell.exe', args, { stdio: 'ignore', windowsHide: true });
  let done = false;
  const result = new Promise(resolve => approval.once('exit', code => { done = true; resolve(code); }));
  return { result, isDone: () => done };
}
async function withApproval(phase, action) {
  const gate = approve(phase);
  try {
    const [_, code] = await Promise.all([action(), gate.result]);
    assert(code === 0, `${phase} approval failed`);
  } finally { if (!gate.isDone()) approval.kill(); approval = null; }
}
function serve() {
  return new Promise((resolve, reject) => {
    server = http.createServer((req, res) => { res.writeHead(200, {'Content-Type':'text/html; charset=utf-8', 'Cache-Control':'no-store'}); res.end(pageHtml); });
    server.once('error', reject);
    server.listen(0, '127.0.0.1', resolve);
  });
}
(async () => {
  await serve();
  context = await chromium.launchPersistentContext(profile, {headless:false, args:[`--disable-extensions-except=${ext}`, `--load-extension=${ext}`], timeout:20000});
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
  const step = (s) => { step.current = s; console.error('E2E step: ' + s); };
  try {
    step('open-options');
    await options.goto(`${origin}options/options.html`, {waitUntil:'load'});
    // initGeneralSettings/permissions flows can take a moment; wait for the
    // main content container the extension reveals after a successful init.
    await options.waitForFunction(() => {
      const mc = document.querySelector('#main-content');
      return mc && getComputedStyle(mc).display !== 'none' && document.querySelectorAll('.sidebar ul.nav li a').length > 0;
    }, {timeout:30000}).catch(() => {
      // fall through: the diagnostic below will record main-content state
    });
    await options.waitForTimeout(500);
    step('open-connected-tab');
    // Replicate exactly what the extension's own sidebar click handler does:
    // hide every tab, then reveal the connected-databases tab. The handler is
    // registered inside initMenu at runtime; invoking it directly avoids
    // synthetic-event trust issues and hash routing differences.
    const switchTab = await options.evaluate(() => {
      const el = document.querySelector("a[href='#connected-databases']");
      if (!el) return 'sidebar link absent';
      const handlers = typeof window.getEventListeners === 'function' ? null : null;
      // Direct DOM switch, same operations as initMenu's click handler.
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
    step('open-page');
    const page = await context.newPage();
    const url = `http://127.0.0.1:${server.address().port}/login`;
    // Without approval, the extension must not fill the page; DB fixture is a CIDR-only match.
    const gate = approve('access');
    try {
      await page.goto(url);
      assert(await gate.result === 0, 'Real KeePass access prompt absent or mismatched');
      step('wait-autofill');
      await page.waitForFunction(({user, password}) => document.querySelector('#user')?.value === user && document.querySelector('#pass')?.value === password,
        {user, password}, {timeout:20000});
    } finally { if (!gate.isDone()) approval.kill(); approval = null; }
    step('verify-autofill');
    assert(await page.locator('#user').inputValue() === user && await page.locator('#pass').inputValue() === password, 'Extension did not autofill the exact fixture values');
    step('write-evidence');
  } catch (e) { throw new Error(`step=${step.current}: ${e && e.message ? e.message.split('\n')[0] : e}`); }
  fs.mkdirSync(resultDir, {recursive:true});
  fs.writeFileSync(path.join(resultDir, 'browser-cidr-e2e.json'), JSON.stringify({version:'1.10.4', zipSha256:hash, associationUi:true, accessUi:true, encryptedGetLoginsViaOfficialExtension:true, cidrAutofill:true, profileIsolated:true},null,2));
  console.log('Official 1.10.4 extension association, access approval, CIDR credential retrieval and webpage autofill passed');
})().catch(e => { console.error('Browser CIDR E2E failed: ' + String(e && e.message ? e.message.split('\n')[0] : e)); process.exitCode = 1; })
  .finally(async () => {
    if (approval) approval.kill();
    if (context) await context.close().catch(() => { process.exitCode=1; });
    if (original) { try { fs.writeFileSync(hostPath, original); } catch (_) { process.exitCode=1; console.error('Native host manifest restoration failed'); } }
    if (server) await new Promise(resolve => server.close(resolve));
    fs.rmSync(profile, {recursive:true,force:true});
  });
