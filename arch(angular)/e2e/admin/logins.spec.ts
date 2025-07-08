import { test, expect } from '../fixtures/authenticated.fixture';
import { KendoGrid } from '../page-objects/kendo-grid.po';
import { KendoDialog } from '../page-objects/kendo-dialog.po';

test.describe('Admin Logins', () => {
  test.beforeEach(async ({ adminPage }) => {
    await adminPage.goto('/logins', { waitUntil: 'networkidle' });
  });

  test('grid loads with data', async ({ adminPage }) => {
    const grid = new KendoGrid(adminPage);
    await grid.waitForLoaded();
    const pager = adminPage.locator('kendo-grid kendo-pager');
    await expect(pager).toBeVisible();
  });

  test('create login provider dialog opens and submits', async ({ adminPage }) => {
    const dialog = new KendoDialog(adminPage);

    await adminPage.locator('button', { hasText: 'Add Login Provider' }).click();
    await dialog.waitForOpen();
    expect(await dialog.getTitle()).toContain('Create Login Provider');

    await dialog.fillField('Name', `E2E-Login-${Date.now()}`);

    const createPromise = adminPage.waitForRequest((req) =>
      req.url().includes('/login/v1') && req.method() === 'POST' && !req.url().includes('/page'),
    );
    await dialog.clickAction('Create');
    const createReq = await createPromise;
    expect(createReq.method()).toBe('POST');
  });

  test('edit login provider via dialog', async ({ adminPage }) => {
    const grid = new KendoGrid(adminPage);
    const dialog = new KendoDialog(adminPage);

    await grid.waitForLoaded();
    if (await grid.hasData()) {
      const firstRow = adminPage.locator('kendo-grid tbody tr[kendogridlogicalrow]').first();
      await firstRow.locator('button[title="Edit"]').click();

      await dialog.waitForOpen();
      expect(await dialog.getTitle()).toContain('Edit Login Provider');
      await dialog.clickAction('Cancel');
    }
  });
});
