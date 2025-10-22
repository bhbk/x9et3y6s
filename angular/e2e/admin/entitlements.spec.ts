import { test, expect } from '../fixtures/authenticated.fixture';
import { KendoGrid } from '../page-objects/kendo-grid.po';
import { KendoDialog } from '../page-objects/kendo-dialog.po';

test.describe('Admin Entitlements', () => {
  test.beforeEach(async ({ adminPage }) => {
    await adminPage.goto('/entitlements', { waitUntil: 'networkidle' });
  });

  test('grid loads with entitlement data', async ({ adminPage }) => {
    const grid = new KendoGrid(adminPage);
    await grid.waitForLoaded();
    expect(await grid.hasData()).toBe(true);
  });

  test('grid shows user, type, scope columns', async ({ adminPage }) => {
    const grid = new KendoGrid(adminPage);
    await grid.waitForLoaded();

    await expect(adminPage.getByRole('columnheader', { name: 'User' })).toBeVisible();
    await expect(adminPage.getByRole('columnheader', { name: 'Type' })).toBeVisible();
    await expect(adminPage.getByRole('columnheader', { name: 'Scope' })).toBeVisible();
    await expect(adminPage.getByRole('columnheader', { name: 'Status' })).toBeVisible();
  });

  test('entitlement type badges are visible', async ({ adminPage }) => {
    const grid = new KendoGrid(adminPage);
    await grid.waitForLoaded();

    await expect(adminPage.getByText('Admin').first()).toBeVisible({ timeout: 5000 });
    await expect(adminPage.getByText('User').first()).toBeVisible();
  });

  test('create entitlement dialog opens and submits', async ({ adminPage }) => {
    const dialog = new KendoDialog(adminPage);

    await adminPage.locator('button', { hasText: 'Add Entitlement' }).click();
    await dialog.waitForOpen();
    expect(await dialog.getTitle()).toContain('Create Entitlement');

    const createPromise = adminPage.waitForRequest((req) =>
      req.url().includes('/user-entitlements/v1') && req.method() === 'POST' && !req.url().includes('/page'),
    );
    await dialog.clickAction('Create');
    const createReq = await createPromise;
    expect(createReq.method()).toBe('POST');

    const body = createReq.postDataJSON();
    expect(body).toHaveProperty('userId');
    expect(body).toHaveProperty('entitlementTypeId');
    expect(body).toHaveProperty('entitlementScopeId');
  });

  test('edit entitlement via dialog', async ({ adminPage }) => {
    const grid = new KendoGrid(adminPage);
    const dialog = new KendoDialog(adminPage);

    await grid.waitForLoaded();
    const firstRow = adminPage.locator('kendo-grid tbody tr[kendogridlogicalrow]').first();
    await firstRow.locator('button[title="Edit"]').click();

    await dialog.waitForOpen();
    expect(await dialog.getTitle()).toContain('Edit Entitlement');
    await dialog.clickAction('Cancel');
  });

  test('delete button only on deletable rows', async ({ adminPage }) => {
    const grid = new KendoGrid(adminPage);
    await grid.waitForLoaded();

    /* First row (admin@local) is not deletable — no trash button */
    const firstRow = adminPage.locator('kendo-grid tbody tr[kendogridlogicalrow]').first();
    await expect(firstRow.locator('button[title="Delete"]')).not.toBeVisible();

    /* Second row (user@local) is deletable */
    const secondRow = adminPage.locator('kendo-grid tbody tr[kendogridlogicalrow]').nth(1);
    await expect(secondRow.locator('button[title="Delete"]')).toBeVisible();
  });

  test('delete dialog opens with confirmation', async ({ adminPage }) => {
    const grid = new KendoGrid(adminPage);
    const dialog = new KendoDialog(adminPage);
    await grid.waitForLoaded();

    const deletableRow = adminPage.locator('kendo-grid tbody tr[kendogridlogicalrow]').nth(1);
    await deletableRow.locator('button[title="Delete"]').click();

    await dialog.waitForOpen();
    expect(await dialog.getTitle()).toContain('Delete Entitlement');
  });
});
