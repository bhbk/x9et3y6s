import { test, expect } from '../fixtures/authenticated.fixture';
import { KendoGrid } from '../page-objects/kendo-grid.po';

test.describe('Admin Auth Activity', () => {
  test.beforeEach(async ({ adminPage }) => {
    await adminPage.goto('/activity', { waitUntil: 'networkidle' });
  });

  test('grid loads with activity data', async ({ adminPage }) => {
    const grid = new KendoGrid(adminPage);
    await grid.waitForLoaded();
    expect(await grid.hasData()).toBe(true);
  });

  test('refresh button reloads data', async ({ adminPage }) => {
    const grid = new KendoGrid(adminPage);
    await grid.waitForLoaded();

    const refreshBtn = adminPage.locator('button', { hasText: 'Refresh' });
    await refreshBtn.click();
    await grid.waitForLoaded();
    expect(await grid.hasData()).toBe(true);
  });

  test('statistics are displayed', async ({ adminPage }) => {
    const grid = new KendoGrid(adminPage);
    await grid.waitForLoaded();

    await expect(adminPage.getByText('Total Activities')).toBeVisible();
    await expect(adminPage.getByText('Successful Logins')).toBeVisible();
    await expect(adminPage.getByText('Failed Attempts')).toBeVisible();
    await expect(adminPage.getByText('Locked Out')).toBeVisible();
  });

  test('pager is visible', async ({ adminPage }) => {
    const pager = adminPage.locator('kendo-grid kendo-pager');
    await expect(pager).toBeVisible();
  });
});
