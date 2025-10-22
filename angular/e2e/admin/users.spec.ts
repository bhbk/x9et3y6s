import { test, expect } from '../fixtures/authenticated.fixture';
import { KendoGrid } from '../page-objects/kendo-grid.po';
import { KendoDialog } from '../page-objects/kendo-dialog.po';

test.describe('Admin Users', () => {
  test.beforeEach(async ({ adminPage }) => {
    await adminPage.goto('/users', { waitUntil: 'networkidle' });
  });

  test('grid loads with data', async ({ adminPage }) => {
    const grid = new KendoGrid(adminPage);
    await grid.waitForLoaded();
    expect(await grid.hasData()).toBe(true);
  });

  test('create user dialog opens and submits', async ({ adminPage }) => {
    const dialog = new KendoDialog(adminPage);

    await adminPage.locator('button', { hasText: 'Add User' }).click();
    await dialog.waitForOpen();
    expect(await dialog.getTitle()).toContain('Create User');

    await dialog.fillField('First Name', 'E2E');
    await dialog.fillField('Last Name', 'TestUser');
    await dialog.fillField('Username', `e2e-user-${Date.now()}`);
    await dialog.fillField('Email', `e2e-${Date.now()}@test.local`);

    const createPromise = adminPage.waitForRequest((req) =>
      req.url().includes('/users/v1') && req.method() === 'POST' && !req.url().includes('/page'),
    );
    await dialog.clickAction('Create');
    const createReq = await createPromise;
    expect(createReq.method()).toBe('POST');

    const body = createReq.postDataJSON();
    expect(body).toHaveProperty('userName');
    expect(body).toHaveProperty('email');
    expect(body).toHaveProperty('firstName');
    expect(body).toHaveProperty('lastName');
    expect(body).toHaveProperty('isHumanBeing');
    expect(body).toHaveProperty('isDeletable');
  });

  test('edit user via dialog', async ({ adminPage }) => {
    const grid = new KendoGrid(adminPage);
    const dialog = new KendoDialog(adminPage);

    await grid.waitForLoaded();
    const firstRow = adminPage.locator('kendo-grid tbody tr[kendogridlogicalrow]').first();
    await firstRow.locator('button[title="Edit"]').click();

    await dialog.waitForOpen();
    expect(await dialog.getTitle()).toContain('Edit User');
    await dialog.clickAction('Cancel');
  });
});
