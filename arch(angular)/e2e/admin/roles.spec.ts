import { test, expect } from '../fixtures/authenticated.fixture';
import { KendoGrid } from '../page-objects/kendo-grid.po';
import { KendoDialog } from '../page-objects/kendo-dialog.po';

test.describe('Admin Roles', () => {
  test.beforeEach(async ({ adminPage }) => {
    await adminPage.goto('/roles', { waitUntil: 'networkidle' });
  });

  test('grid loads with data', async ({ adminPage }) => {
    const grid = new KendoGrid(adminPage);
    await grid.waitForLoaded();
    expect(await grid.hasData()).toBe(true);
  });

  test('cascade filter dropdowns are present', async ({ adminPage }) => {
    const issuerFilter = adminPage.locator('kendo-dropdownlist#issuerFilter');
    const audienceFilter = adminPage.locator('kendo-dropdownlist#audienceFilter');
    await expect(issuerFilter).toBeVisible();
    await expect(audienceFilter).toBeVisible();
  });

  test('create role dialog opens and submits', async ({ adminPage }) => {
    const dialog = new KendoDialog(adminPage);

    await adminPage.locator('button', { hasText: 'Add Role' }).click();
    await dialog.waitForOpen();
    expect(await dialog.getTitle()).toContain('Create Role');

    await dialog.selectFirstDropdownOption('Audience');
    await dialog.fillField('Name', `E2E-Role-${Date.now()}`);

    const createPromise = adminPage.waitForRequest((req) =>
      req.url().includes('/role/v1') && req.method() === 'POST' && !req.url().includes('/page'),
    );
    await dialog.clickAction('Create');
    const createReq = await createPromise;
    expect(createReq.method()).toBe('POST');
  });

  test('edit role via dialog', async ({ adminPage }) => {
    const grid = new KendoGrid(adminPage);
    const dialog = new KendoDialog(adminPage);

    await grid.waitForLoaded();
    const firstRow = adminPage.locator('kendo-grid tbody tr[kendogridlogicalrow]').first();
    await firstRow.locator('button[title="Edit"]').click();

    await dialog.waitForOpen();
    expect(await dialog.getTitle()).toContain('Edit Role');
    await dialog.clickAction('Cancel');
  });
});
