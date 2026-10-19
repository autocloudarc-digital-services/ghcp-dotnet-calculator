const assert = require('node:assert/strict');

async function display(page, expected, timeout = 15000) {
    await page.waitForFunction(value =>
        document.querySelector('output[aria-label="Calculator display"]')?.textContent.trim() === value,
    expected, { timeout });
}

async function ready(page) {
    const keypad = page.getByRole('region', { name: 'Current Calculation', exact: true });
    const deadline = Date.now() + 120000;
    while (Date.now() < deadline) {
        try {
            await keypad.getByRole('button', { name: 'Clear', exact: true }).click({ timeout: 2000 });
            await display(page, '0', 2000);
            await keypad.getByRole('button', { name: 'Seven', exact: true }).click({ timeout: 2000 });
            await display(page, '7', 2000);
            await keypad.getByRole('button', { name: 'Clear', exact: true }).click({ timeout: 2000 });
            await display(page, '0', 2000);
            return;
        } catch (error) {
            if (error.name !== 'TimeoutError') throw error;
        }
    }
    throw new Error('Calculator did not become interactive within the readiness deadline.');
}

async function press(page, names) {
    const keypad = page.getByRole('region', { name: 'Current Calculation', exact: true });
    for (const name of names) await keypad.getByRole('button', { name, exact: true }).click();
}

async function historyChecks(page) {
    const history = page.getByRole('region', { name: 'History', exact: true });
    await history.getByRole('button', { name: 'Replay 7 + 3 equals 10', exact: true }).waitFor();
    await press(page, ['Clear', 'Nine']);
    await display(page, '9');
    await history.getByRole('button', { name: 'Replay 7 + 3 equals 10', exact: true }).click();
    await display(page, '10');
    await history.getByRole('button', { name: 'Clear', exact: true }).click();
    await page.waitForFunction(() => document.querySelectorAll('.history-entry').length === 0);
    await display(page, '10');
    for (let index = 0; index < 51; index++) {
        await press(page, ['One', 'Add', 'One', 'Equals']);
    }
    await press(page, ['Seven', 'Add', 'Three', 'Equals']);
    await display(page, '10');
    await page.waitForFunction(() => {
        const entries = document.querySelectorAll('.history-entry');
        return entries.length === 50 && entries[0].getAttribute('aria-label') === 'Replay 7 + 3 equals 10';
    });
    await history.getByRole('button', { name: 'Clear', exact: true }).click();
    await page.waitForFunction(() => document.querySelectorAll('.history-entry').length === 0);
    await press(page, ['Clear']);
    await page.getByRole('button', { name: 'Seven', exact: true }).focus();
    await page.keyboard.press('Tab');
    assert.equal(await page.getByRole('button', { name: 'Eight', exact: true })
        .evaluate(button => button === document.activeElement), true);
    await page.keyboard.press('Enter');
    await display(page, '8');
    await page.keyboard.type('c4*5');
    await page.keyboard.press('Enter');
    await display(page, '20');
    await history.getByRole('button', { name: 'Replay 4 * 5 equals 20', exact: true }).waitFor();
}

async function circuitChecks(browser, page, transport) {
    const theme = page.getByRole('button', { name: 'Dark mode', exact: true });
    await theme.click();
    await page.waitForFunction(() =>
        document.querySelector('[aria-label="Dark mode"]').getAttribute('aria-pressed') === 'true');
    assert.ok(transport.current, 'Blazor did not establish a WebSocket; check transport/hosting configuration.');
    const before = transport.connections;
    await transport.current.close({ code: 1012, reason: 'Reconnect validation' });
    const deadline = Date.now() + 120000;
    while (transport.connections === before && Date.now() < deadline) await page.waitForTimeout(100);
    assert.ok(transport.connections > before, 'The interrupted WebSocket must be replaced.');
    await ready(page);
    await page.getByRole('button', { name: 'Replay 4 * 5 equals 20', exact: true }).waitFor();
    assert.equal(await theme.getAttribute('aria-pressed'), 'true', 'Reconnect must preserve circuit theme.');
    await press(page, ['Seven', 'Add', 'Three', 'Equals']);
    await display(page, '10');

    const isolated = await browser.newContext();
    try {
        const other = await isolated.newPage();
        await other.goto(page.url());
        await ready(other);
        assert.equal(await other.locator('.history-entry').count(), 0);
        assert.equal(await other.getByRole('button', { name: 'Dark mode' }).getAttribute('aria-pressed'), 'false');
        await display(page, '10');
    } finally {
        await isolated.close();
    }
    await page.reload();
    await ready(page);
    assert.equal(await page.locator('.history-entry').count(), 0);
    assert.equal(await theme.getAttribute('aria-pressed'), 'false');
    assert.ok(transport.connections > before + 1, 'Refresh must create a new WebSocket.');
}

async function verify(url) {
    const { chromium } = require('playwright');
    const browser = await chromium.launch({ headless: true });
    try {
        const context = await browser.newContext();
        const page = await context.newPage();
        const errors = [];
        const assets = [];
        const transport = { current: null, connections: 0 };
        page.on('pageerror', error => errors.push(error.message));
        page.on('response', response => {
            if (['script', 'stylesheet'].includes(response.request().resourceType()) && response.status() >= 400) {
                assets.push(`${response.status()} ${new URL(response.url()).pathname}`);
            }
        });
        page.on('requestfailed', request => {
            if (['script', 'stylesheet'].includes(request.resourceType())) {
                assets.push(`${request.failure()?.errorText} ${new URL(request.url()).pathname}`);
            }
        });
        await page.routeWebSocket(target => target.pathname === '/_blazor', socket => {
            transport.current = socket;
            transport.connections++;
            socket.connectToServer();
        });
        const health = await context.request.get(new URL('health', url).href);
        assert.equal(health.status(), 200);
        assert.equal(await health.text(), 'Healthy');
        const home = await page.goto(url.href, { waitUntil: 'domcontentloaded', timeout: 120000 });
        assert.equal(home.status(), 200);
        assert.equal(await page.title(), 'Calculator');
        await ready(page);
        await press(page, ['Seven', 'Add', 'Three', 'Equals']);
        await display(page, '10');
        const observedDisplay = (await page.getByRole('status').textContent()).trim();
        await historyChecks(page);
        await circuitChecks(browser, page, transport);
        for (const dark of [true, false]) {
            if (dark) await page.getByRole('button', { name: 'Dark mode', exact: true }).click();
            await page.waitForFunction(value =>
                document.querySelector('[aria-label="Dark mode"]').getAttribute('aria-pressed') === String(value), dark);
            await page.setViewportSize({ width: 320, height: 780 });
            assert.equal(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth), true);
            assert.equal(await page.locator('#blazor-error-ui').count(), 1);
            assert.equal(await page.locator('#blazor-error-ui').isVisible(), false);
            if (dark) await page.getByRole('button', { name: 'Dark mode', exact: true }).click();
        }
        assert.deepEqual(errors, []);
        assert.deepEqual(assets, []);
        console.log(JSON.stringify({
            url: url.href, health: 'Healthy', home: 200, expression: '7 + 3 =',
            expectedDisplay: '10', observedDisplay, verdict: 'passed',
            history: 'immediate updates, replay, clear, and newest entry at 50-entry cap',
            keyboard: 'native Tab/Enter and 4 * 5 = 20 with a keypad button focused',
            theme: 'passed', isolation: 'passed',
            reconnect: 'new WebSocket with preserved circuit history/theme',
            refresh: 'new circuit with empty history/default theme', mobile: '320px in both themes',
            webSocketConnections: transport.connections, pageErrors: errors, assetFailures: assets
        }, null, 2));
    } finally {
        await browser.close();
    }
}

async function main() {
    let url;
    try {
        url = new URL(process.argv[2] || 'http://localhost:5000');
    } catch (error) {
        if (error.code !== 'ERR_INVALID_URL') throw error;
        throw new Error('Use an absolute HTTP(S) calculator root URL without credentials, query parameters, or fragments.');
    }
    assert.ok(['http:', 'https:'].includes(url.protocol), 'Use an HTTP(S) calculator URL.');
    assert.ok(!url.username && !url.password && !url.search && !url.hash && url.pathname === '/',
        'Use the calculator root URL without credentials, query parameters, or fragments.');
    assert.ok(process.argv.length <= 3, 'Usage: node verify-calculator.cjs [calculator-root-url]');
    await verify(url);
}

main().catch(error => {
    console.error(error);
    process.exitCode = 1;
});
