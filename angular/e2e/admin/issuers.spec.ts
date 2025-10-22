import { test, expect } from '../fixtures/authenticated.fixture';
import { KendoGrid } from '../page-objects/kendo-grid.po';
import { KendoDialog } from '../page-objects/kendo-dialog.po';

test.describe('Admin Issuers', () => {
  test.beforeEach(async ({ adminPage }) => {
    await adminPage.goto('/issuers', { waitUntil: 'networkidle' });
  });

  test('grid loads with data', async ({ adminPage }) => {
    const grid = new KendoGrid(adminPage);
    await grid.waitForLoaded();
    expect(await grid.hasData()).toBe(true);
  });

  test('create issuer dialog opens and submits', async ({ adminPage }) => {
    const dialog = new KendoDialog(adminPage);

    await adminPage.locator('button', { hasText: 'Add Issuer' }).click();
    await dialog.waitForOpen();
    expect(await dialog.getTitle()).toContain('Create Issuer');

    await dialog.fillField('Name', `E2E-Issuer-${Date.now()}`);

    // Verify the create API call fires
    const createPromise = adminPage.waitForRequest((req) =>
      req.url().includes('/issuers/v1') && req.method() === 'POST' && !req.url().includes('/page'),
    );
    await dialog.clickAction('Create');
    const createReq = await createPromise;
    expect(createReq.method()).toBe('POST');

    const body = createReq.postDataJSON();
    expect(body).toHaveProperty('name');
    expect(body).toHaveProperty('isEnabled');
    expect(body).toHaveProperty('isDeletable');
  });

  test('edit issuer via dialog', async ({ adminPage }) => {
    const grid = new KendoGrid(adminPage);
    const dialog = new KendoDialog(adminPage);

    await grid.waitForLoaded();
    const firstRow = adminPage.locator('kendo-grid tbody tr[kendogridlogicalrow]').first();
    await firstRow.locator('button[title="Edit"]').click();

    await dialog.waitForOpen();
    expect(await dialog.getTitle()).toContain('Edit Issuer');
    await dialog.clickAction('Cancel');
  });

  test('pagination is present when data exceeds page size', async ({ adminPage }) => {
    const grid = new KendoGrid(adminPage);
    await grid.waitForLoaded();
    const pager = adminPage.locator('kendo-grid kendo-pager');
    await expect(pager).toBeVisible();
  });
});
