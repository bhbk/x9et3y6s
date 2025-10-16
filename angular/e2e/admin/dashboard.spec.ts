import { test, expect } from '../fixtures/authenticated.fixture';

test.describe('Admin Dashboard', () => {
  test('dashboard loads with summary cards', async ({ adminPage }) => {
    await expect(adminPage).toHaveURL(/\/dashboard/);
    // Wait for Angular to render — cards appear after forkJoin completes
    await expect(adminPage.getByText('Total Users')).toBeVisible({ timeout: 10000 });
    await expect(adminPage.getByText('Active Sessions')).toBeVisible();
    await expect(adminPage.getByText('Failed Logins')).toBeVisible();
    await expect(adminPage.getByText('Locked Accounts')).toBeVisible();
    await expect(adminPage.getByText('Pending Confirmations')).toBeVisible();

    // Cards should show actual counts from mock data (not --)
    const userCard = adminPage.locator('text=Total Users').locator('..');
    await expect(userCard.locator('p')).not.toHaveText('--', { timeout: 5000 });

    const sessionCard = adminPage.locator('text=Active Sessions').locator('..');
    await expect(sessionCard.locator('p')).not.toHaveText('--', { timeout: 5000 });

    const failedCard = adminPage.locator('text=Failed Logins').locator('..');
    await expect(failedCard.locator('p')).not.toHaveText('--', { timeout: 5000 });

    const lockedCard = adminPage.locator('text=Locked Accounts').locator('..');
    await expect(lockedCard.locator('p')).not.toHaveText('--', { timeout: 5000 });

    const pendingCard = adminPage.locator('text=Pending Confirmations').locator('..');
    await expect(pendingCard.locator('p')).not.toHaveText('--', { timeout: 5000 });
  });

  test('dashboard shows recent activity table with username column', async ({ adminPage }) => {
    // Wait for the activity section to appear
    await expect(adminPage.getByText('Recent Activity')).toBeVisible({ timeout: 10000 });

    // Column headers
    await expect(adminPage.getByRole('columnheader', { name: 'User' })).toBeVisible({ timeout: 5000 });
    await expect(adminPage.getByRole('columnheader', { name: 'Type' })).toBeVisible();
    await expect(adminPage.getByRole('columnheader', { name: 'Outcome' })).toBeVisible();

    // Should show activity rows from mock data with resolved usernames
    await expect(adminPage.getByText('ResourceOwner').first()).toBeVisible({ timeout: 5000 });
    await expect(adminPage.getByText('Success').first()).toBeVisible();
    // Mock data has admin@local and user@local — at least one username should be visible
    await expect(adminPage.getByText('admin@local').first()).toBeVisible({ timeout: 5000 });
  });
});
