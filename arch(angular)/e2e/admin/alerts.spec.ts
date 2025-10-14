import { test, expect } from '../fixtures/authenticated.fixture';
import { KendoGrid } from '../page-objects/kendo-grid.po';
import { KendoDialog } from '../page-objects/kendo-dialog.po';

test.describe('Admin Alerts', () => {
  test.beforeEach(async ({ adminPage }) => {
    await adminPage.goto('/alerts', { waitUntil: 'networkidle' });
  });

  test('page loads with statistics cards', async ({ adminPage }) => {
    await expect(adminPage.getByText('Emails Queued')).toBeVisible({ timeout: 10000 });
    await expect(adminPage.getByText('Texts Queued')).toBeVisible();
    await expect(adminPage.getByText('Pending Delivery')).toBeVisible();
    const statsArea = adminPage.locator('.grid.grid-cols-1');
    await expect(statsArea.getByText('Cancelled')).toBeVisible();
  });

  test('email queue tab is selected by default with grid data', async ({ adminPage }) => {
    const grid = new KendoGrid(adminPage);
    await grid.waitForLoaded();
    expect(await grid.hasData()).toBe(true);

    await expect(adminPage.getByRole('columnheader', { name: 'From' })).toBeVisible();
    await expect(adminPage.getByRole('columnheader', { name: 'To' })).toBeVisible();
    await expect(adminPage.getByRole('columnheader', { name: 'Subject' })).toBeVisible();
    await expect(adminPage.getByRole('columnheader', { name: 'Status' })).toBeVisible();
  });

  test('email grid shows status badges', async ({ adminPage }) => {
    const grid = new KendoGrid(adminPage);
    await grid.waitForLoaded();

    // Mock data has one Delivered, one Pending, one Cancelled
    await expect(adminPage.getByText('Delivered').first()).toBeVisible({ timeout: 5000 });
    await expect(adminPage.getByText('Pending').first()).toBeVisible();
    await expect(adminPage.getByText('Cancelled').first()).toBeVisible();
  });

  test('email grid shows mock email data', async ({ adminPage }) => {
    const grid = new KendoGrid(adminPage);
    await grid.waitForLoaded();

    await expect(adminPage.getByText('Email Confirmation')).toBeVisible({ timeout: 5000 });
    await expect(adminPage.getByText('Password Reset')).toBeVisible();
    await expect(adminPage.getByText('Account Locked')).toBeVisible();
  });

  test('switching to text queue tab loads text grid', async ({ adminPage }) => {
    const textTab = adminPage.locator('kendo-tabstrip-tab', { hasText: 'Text Queue' })
      .or(adminPage.getByText('Text Queue'));
    await textTab.first().click();

    const grid = new KendoGrid(adminPage);
    await grid.waitForLoaded();
    expect(await grid.hasData()).toBe(true);

    await expect(adminPage.getByRole('columnheader', { name: 'From' })).toBeVisible();
    await expect(adminPage.getByRole('columnheader', { name: 'To' })).toBeVisible();
    await expect(adminPage.getByRole('columnheader', { name: 'Body' })).toBeVisible();
  });

  test('text grid shows phone numbers from mock data', async ({ adminPage }) => {
    const textTab = adminPage.getByText('Text Queue');
    await textTab.click();

    const grid = new KendoGrid(adminPage);
    await grid.waitForLoaded();

    await expect(adminPage.getByText('+15559876543')).toBeVisible({ timeout: 5000 });
    await expect(adminPage.getByText('+15558765432')).toBeVisible();
  });

  test('delete button opens confirmation dialog', async ({ adminPage }) => {
    const grid = new KendoGrid(adminPage);
    const dialog = new KendoDialog(adminPage);
    await grid.waitForLoaded();

    const firstRow = adminPage.locator('kendo-grid tbody tr[kendogridlogicalrow]').first();
    await firstRow.locator('button[title="Delete"]').click();

    await dialog.waitForOpen();
    expect(await dialog.getTitle()).toContain('Delete Queue Item');
  });

  test('delete confirmation sends DELETE request', async ({ adminPage }) => {
    const grid = new KendoGrid(adminPage);
    const dialog = new KendoDialog(adminPage);
    await grid.waitForLoaded();

    const firstRow = adminPage.locator('kendo-grid tbody tr[kendogridlogicalrow]').first();
    await firstRow.locator('button[title="Delete"]').click();
    await dialog.waitForOpen();

    const deletePromise = adminPage.waitForRequest((req) =>
      req.url().includes('/dequeue/v1/email/') && req.method() === 'DELETE',
    );
    await dialog.clickAction('Delete');
    const deleteReq = await deletePromise;
    expect(deleteReq.method()).toBe('DELETE');
  });

  test('delete dialog can be cancelled', async ({ adminPage }) => {
    const grid = new KendoGrid(adminPage);
    const dialog = new KendoDialog(adminPage);
    await grid.waitForLoaded();

    const firstRow = adminPage.locator('kendo-grid tbody tr[kendogridlogicalrow]').first();
    await firstRow.locator('button[title="Delete"]').click();
    await dialog.waitForOpen();

    await dialog.clickAction('Cancel');
    await expect(adminPage.locator('kendo-dialog')).not.toBeVisible();
  });

  test('refresh button reloads data', async ({ adminPage }) => {
    const grid = new KendoGrid(adminPage);
    await grid.waitForLoaded();

    const refreshPromise = adminPage.waitForRequest((req) =>
      req.url().includes('/enqueue/v1/email/page') && req.method() === 'POST',
    );
    await adminPage.locator('button', { hasText: 'Refresh' }).click();
    const refreshReq = await refreshPromise;
    expect(refreshReq.method()).toBe('POST');
  });

  test('sidebar alerts link is active when on alerts page', async ({ adminPage }) => {
    const alertsLink = adminPage.locator('a[routerLink="/alerts"]');
    await expect(alertsLink).toHaveClass(/bg-blue-50/);
  });
});
