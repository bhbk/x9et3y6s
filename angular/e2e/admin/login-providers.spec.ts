import { test, expect } from '../fixtures/authenticated.fixture';
import { KendoDialog } from '../page-objects/kendo-dialog.po';

test.describe('Admin Login Providers', () => {
  test.beforeEach(async ({ adminPage }) => {
    await adminPage.goto('/login-providers', { waitUntil: 'networkidle' });
  });

  test('card list loads with data', async ({ adminPage }) => {
    /* The login providers page uses a card layout, not kendo-grid */
    const cards = adminPage.locator('.bg-white.rounded-lg.border');
    await expect(cards.first()).toBeVisible({ timeout: 10000 });
    expect(await cards.count()).toBeGreaterThan(0);
  });

  test('create login provider dialog opens and submits', async ({ adminPage }) => {
    const dialog = new KendoDialog(adminPage);

    await adminPage.locator('button', { hasText: 'Add Login Provider' }).click();
    await dialog.waitForOpen();
    expect(await dialog.getTitle()).toContain('Create Login Provider');

    await dialog.fillField('Name', `E2E-LoginProvider-${Date.now()}`);

    const createPromise = adminPage.waitForRequest((req) =>
      req.url().includes('/login-providers/v1') && req.method() === 'POST' && !req.url().includes('/page'),
    );
    await dialog.clickAction('Create');
    const createReq = await createPromise;
    expect(createReq.method()).toBe('POST');

    const body = createReq.postDataJSON();
    expect(body).toHaveProperty('name');
    expect(body).toHaveProperty('isEnabled');
    expect(body).toHaveProperty('isDeletable');
  });

  test('expand settings panel for a login provider', async ({ adminPage }) => {
    /* Card has a "Settings" button that expands inline editing */
    const firstCard = adminPage.locator('.bg-white.rounded-lg.border').first();
    await expect(firstCard).toBeVisible({ timeout: 10000 });

    const settingsBtn = firstCard.locator('button', { hasText: 'Settings' });
    await settingsBtn.click();

    /* Expanded panel has a table with Name, Description, ProviderKey, IsDeletable fields */
    const nameField = firstCard.locator('td', { hasText: 'Name' });
    await expect(nameField).toBeVisible();

    const saveBtn = firstCard.locator('button', { hasText: 'Save Settings' });
    await expect(saveBtn).toBeVisible();
  });
});
