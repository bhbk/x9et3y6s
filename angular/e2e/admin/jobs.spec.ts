import { test, expect } from '../fixtures/authenticated.fixture';

test.describe('Admin Jobs', () => {
  test.beforeEach(async ({ adminPage }) => {
    await adminPage.goto('/jobs', { waitUntil: 'networkidle' });
  });

  test('jobs page loads with job cards', async ({ adminPage }) => {
    await expect(adminPage.getByText('Maintain Quotes')).toBeVisible({ timeout: 10000 });
    await expect(adminPage.getByText('Groom Chat History')).toBeVisible();
  });

  test('job descriptions are visible', async ({ adminPage }) => {
    await expect(adminPage.getByText('Fetches and rotates daily quotes')).toBeVisible({ timeout: 10000 });
    await expect(adminPage.getByText('Removes stale chat conversations')).toBeVisible();
  });

  test('enabled job shows green badge', async ({ adminPage }) => {
    await expect(adminPage.getByText('Maintain Quotes')).toBeVisible({ timeout: 10000 });
    /* First job (Maintain Quotes) is enabled */
    const badges = adminPage.locator('.bg-green-100', { hasText: 'Enabled' });
    await expect(badges.first()).toBeVisible();
  });

  test('disabled job shows gray badge', async ({ adminPage }) => {
    await expect(adminPage.getByText('Groom Chat History')).toBeVisible({ timeout: 10000 });
    const badges = adminPage.locator('.bg-gray-100', { hasText: 'Disabled' });
    await expect(badges.first()).toBeVisible();
  });

  test('schedule cron expression is displayed', async ({ adminPage }) => {
    await expect(adminPage.getByText('0 0 6 * * ?')).toBeVisible({ timeout: 10000 });
  });

  test('expanding settings shows key/value table', async ({ adminPage }) => {
    await expect(adminPage.getByText('Maintain Quotes')).toBeVisible({ timeout: 10000 });

    /* Click the Settings button on the first job */
    const settingsBtn = adminPage.locator('button', { hasText: 'Settings (2)' }).first();
    await settingsBtn.click();

    /* Should show config keys */
    await expect(adminPage.getByText('Schedule').first()).toBeVisible({ timeout: 5000 });
    await expect(adminPage.getByText('ApiKey')).toBeVisible();
  });

  test('settings panel has save and reset buttons', async ({ adminPage }) => {
    await expect(adminPage.getByText('Maintain Quotes')).toBeVisible({ timeout: 10000 });

    const settingsBtn = adminPage.locator('button', { hasText: 'Settings (2)' }).first();
    await settingsBtn.click();

    await expect(adminPage.locator('button', { hasText: 'Save Settings' })).toBeVisible({ timeout: 5000 });
    await expect(adminPage.locator('button', { hasText: 'Reset' })).toBeVisible();
  });

  test('toggling enable sends PUT request', async ({ adminPage }) => {
    await expect(adminPage.getByText('Maintain Quotes')).toBeVisible({ timeout: 10000 });

    const putPromise = adminPage.waitForRequest((req) =>
      req.url().includes('/jobs/v1') && req.method() === 'PUT',
    );

    /* Click the checkbox next to the first job */
    const checkbox = adminPage.locator('input[type="checkbox"]').first();
    await checkbox.click();

    const putReq = await putPromise;
    expect(putReq.method()).toBe('PUT');
  });
});
