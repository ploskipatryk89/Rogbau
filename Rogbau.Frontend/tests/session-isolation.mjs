import assert from 'node:assert/strict';
import fs from 'node:fs';
import vm from 'node:vm';
import ts from 'typescript';

const source = fs.readFileSync(new URL('../src/api/apiService.ts', import.meta.url), 'utf8');
const compiled = ts.transpileModule(source, { compilerOptions: { module: ts.ModuleKind.CommonJS } }).outputText;
const sharedStorage = { getItem: () => 'manager-token' };
const requests = [];

function tab(token) {
  const exports = {};
  const context = vm.createContext({
    exports,
    require: () => ({ API_BASE_URL: 'https://test.invalid' }),
    sessionStorage: { getItem: () => token },
    localStorage: sharedStorage,
    console: { log() {}, error() {} },
    fetch: async (_url, options) => {
      requests.push(options.headers.Authorization);
      return { ok: true, status: 204 };
    },
  });
  vm.runInContext(compiled, context);
  return exports;
}

await tab('admin-token').get('/api/orders');
await tab('manager-token').get('/api/orders');
await tab('admin-token').patch('/api/transfers/test/confirm');
assert.deepEqual(requests, ['Bearer admin-token', 'Bearer manager-token', 'Bearer admin-token']);
console.log('PASS: API requests in separate tabs use separate session tokens, regardless of shared localStorage.');

let sessionToken = 'expired-token';
let expiredEvents = 0;
const expiredExports = {};
const expiredContext = vm.createContext({
  exports: expiredExports,
  require: () => ({ API_BASE_URL: 'https://test.invalid' }),
  sessionStorage: { getItem: () => sessionToken, removeItem: () => { sessionToken = null; } },
  window: { dispatchEvent: event => { if (event.type === 'session-expired') expiredEvents++; } },
  Event: class { constructor(type) { this.type = type; } },
  console: { log() {}, error() {} },
  fetch: async () => ({ ok: false, status: 401 }),
});
vm.runInContext(compiled, expiredContext);
await assert.rejects(expiredExports.get('/api/sites'), /Sesja wygasła/);
assert.equal(sessionToken, null);
assert.equal(expiredEvents, 1);
console.log('PASS: expired session clears its token and notifies the authentication context.');
