import { test, expect } from '../fixtures/authenticated.fixture';

test.describe('Admin LLM Providers', () => {
  test.beforeEach(async ({ adminPage }) => {
    await adminPage.goto('/llm-providers', { waitUntil: 'networkidle' });
  });

  test('page loads with provider cards', async ({ adminPage }) => {
    await expect(adminPage.getByText('Ollama Local')).toBeVisible({ timeout: 10000 });
    await expect(adminPage.getByText('AWS Bedrock')).toBeVisible();
  });

  test('providers show failover order numbers', async ({ adminPage }) => {
    await expect(adminPage.getByText('Ollama Local')).toBeVisible({ timeout: 10000 });
    /* Failover order displayed as small font-mono numbers */
    const orderNumbers = adminPage.locator('.font-mono.text-gray-400');
    await expect(orderNumbers.first()).toBeVisible();
  });

  test('enabled provider shows green badge', async ({ adminPage }) => {
    await expect(adminPage.getByText('Ollama Local')).toBeVisible({ timeout: 10000 });
    const badges = adminPage.locator('.bg-green-100', { hasText: 'Enabled' });
    await expect(badges.first()).toBeVisible();
  });

  test('disabled provider shows gray badge', async ({ adminPage }) => {
    await expect(adminPage.getByText('AWS Bedrock')).toBeVisible({ timeout: 10000 });
    const badges = adminPage.locator('.bg-gray-100', { hasText: 'Disabled' });
    await expect(badges.first()).toBeVisible();
  });

  test('expanding settings shows key/value table', async ({ adminPage }) => {
    await expect(adminPage.getByText('Ollama Local')).toBeVisible({ timeout: 10000 });

    const settingsBtn = adminPage.locator('button', { hasText: 'Settings (2)' }).first();
    await settingsBtn.click();

    await expect(adminPage.getByText('BaseUrl')).toBeVisible({ timeout: 5000 });
    await expect(adminPage.getByText('ModelName')).toBeVisible();
  });

  test('settings panel has save and reset buttons', async ({ adminPage }) => {
    await expect(adminPage.getByText('Ollama Local')).toBeVisible({ timeout: 10000 });

    const settingsBtn = adminPage.locator('button', { hasText: 'Settings (2)' }).first();
    await settingsBtn.click();

    await expect(adminPage.locator('button', { hasText: 'Save Settings' })).toBeVisible({ timeout: 5000 });
    await expect(adminPage.locator('button', { hasText: 'Reset' })).toBeVisible();
  });

  test('toggling enable sends PUT request', async ({ adminPage }) => {
    await expect(adminPage.getByText('Ollama Local')).toBeVisible({ timeout: 10000 });

    const putPromise = adminPage.waitForRequest((req) =>
      req.url().includes('/llm-providers/v1') && req.method() === 'PUT' && !req.url().includes('/order'),
    );

    const checkbox = adminPage.locator('input[type="checkbox"]').first();
    await checkbox.click();

    const putReq = await putPromise;
    expect(putReq.method()).toBe('PUT');
  });

  test('drag handle is visible for reordering', async ({ adminPage }) => {
    await expect(adminPage.getByText('Ollama Local')).toBeVisible({ timeout: 10000 });
    /* Drag handle has cursor-grab class */
    const dragHandles = adminPage.locator('.cursor-grab');
    await expect(dragHandles.first()).toBeVisible();
  });

  test('subtitle text is visible', async ({ adminPage }) => {
    await expect(adminPage.getByText('Manage LLM providers and failover priority')).toBeVisible({ timeout: 10000 });
  });
});
