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

  test('audience column shows names for multi-audience entries', async ({ adminPage }) => {
    const grid = new KendoGrid(adminPage);
    await grid.waitForLoaded();

    // Row 1 has two audiences (identity-admin, identity-user) — the multi-audience case
    const audienceColumn = adminPage.locator('kendo-grid tbody tr[kendogridlogicalrow]').nth(1).locator('td').nth(4);
    const audienceText = await audienceColumn.textContent();
    expect(audienceText).toContain('identity-admin');
    expect(audienceText).toContain('identity-user');
  });

  test('audience column shows dash for entries with no audiences', async ({ adminPage }) => {
    const grid = new KendoGrid(adminPage);
    await grid.waitForLoaded();

    // Row 2 (Failure) has empty audienceIds — should show a dash
    const audienceColumn = adminPage.locator('kendo-grid tbody tr[kendogridlogicalrow]').nth(2).locator('td').nth(4);
    await expect(audienceColumn.locator('.text-gray-400')).toHaveText('-');
  });
});
