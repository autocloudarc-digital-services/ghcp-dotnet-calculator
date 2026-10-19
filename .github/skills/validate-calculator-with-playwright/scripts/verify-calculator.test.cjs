const assert = require('node:assert/strict');
const { randomUUID } = require('node:crypto');
const { join } = require('node:path');
const { spawnSync } = require('node:child_process');
const { test } = require('node:test');

const runner = join(__dirname, 'verify-calculator.cjs');

for (const url of ['file:///unread-file', 'https://example.invalid/?sample=1', 'https://example.invalid/not-the-root']) {
    test(`Rejects invalid target ${url} before launching a browser`, () => {
        const result = spawnSync(process.execPath, [runner, url], { encoding: 'utf8' });
        assert.equal(result.status, 1);
        assert.match(result.stderr, /Use (an HTTP\(S\) calculator URL|the calculator root URL)/);
    });
}

test('Rejects URL credentials without echoing their values', () => {
    const target = new URL('https://example.invalid/');
    target.username = randomUUID();
    target.password = randomUUID();
    for (const address of [target.href, target.href.replace('example.invalid', 'invalid host')]) {
        const result = spawnSync(process.execPath, [runner, address], { encoding: 'utf8', timeout: 5000 });
        assert.equal(result.status, 1);
        assert.match(result.stderr, /without credentials/);
        assert.ok(!result.stderr.includes(target.username));
        assert.ok(!result.stderr.includes(target.password));
    }
});
