import { test, expect } from '../fixtures/authenticated.fixture';
import { KendoGrid } from '../page-objects/kendo-grid.po';
import { KendoDialog } from '../page-objects/kendo-dialog.po';

test.describe('Admin Claims', () => {
  test.beforeEach(async ({ adminPage }) => {
    await adminPage.goto('/claims', { waitUntil: 'networkidle' });
  });

  test('grid loads with data', async ({ adminPage }) => {
    const grid = new KendoGrid(adminPage);
    await grid.waitForLoaded();
    expect(await grid.hasData()).toBe(true);
  });

  test('issuer filter dropdown is present', async ({ adminPage }) => {
    const filter = adminPage.locator('kendo-dropdownlist#issuerFilter');
    await expect(filter).toBeVisible();
  });

  test('create claim dialog opens and submits', async ({ adminPage }) => {
    const dialog = new KendoDialog(adminPage);

    await adminPage.locator('button', { hasText: 'Add Claim' }).click();
    await dialog.waitForOpen();
    expect(await dialog.getTitle()).toContain('Create Claim');

    await dialog.selectFirstDropdownOption('Issuer');
    await dialog.fillField('Type', `e2e-claim-${Date.now()}`);
    await dialog.fillField('Value', 'e2e-test-value');

    const createPromise = adminPage.waitForRequest((req) =>
      req.url().includes('/claim/v1') && req.method() === 'POST' && !req.url().includes('/page'),
    );
    await dialog.clickAction('Create');
    const createReq = await createPromise;
    expect(createReq.method()).toBe('POST');
  });

  test('edit claim via dialog', async ({ adminPage }) => {
    const grid = new KendoGrid(adminPage);
    const dialog = new KendoDialog(adminPage);

    await grid.waitForLoaded();
    const firstRow = adminPage.locator('kendo-grid tbody tr[kendogridlogicalrow]').first();
    await firstRow.locator('button[title="Edit"]').click();

    await dialog.waitForOpen();
    expect(await dialog.getTitle()).toContain('Edit Claim');
    await dialog.clickAction('Cancel');
  });
});
