import { test, expect } from '../fixtures/authenticated.fixture';

test.describe('User Portal Dashboard', () => {
  test('dashboard loads after authentication', async ({ userPage }) => {
    await expect(userPage).toHaveURL(/\/dashboard/);
  });

  test('quote of the day is displayed', async ({ userPage }) => {
    await userPage.goto('/dashboard', { waitUntil: 'networkidle' });

    /* Dashboard shows a MOTD/quote — the mock returns QUOTE data */
    const quoteArea = userPage.locator('blockquote').or(userPage.getByText(/Welcome to the Identity Portal/));
    await expect(quoteArea).toBeVisible({ timeout: 10000 });
  });

  test('sidebar navigation is visible', async ({ userPage }) => {
    await userPage.goto('/dashboard', { waitUntil: 'networkidle' });

    const sidebar = userPage.locator('aside nav');
    await expect(sidebar.locator('a[routerLink="/dashboard"]')).toBeVisible({ timeout: 10000 });
    await expect(sidebar.locator('a[routerLink="/profile"]')).toBeVisible();
    await expect(sidebar.locator('a[routerLink="/security"]')).toBeVisible();
    await expect(sidebar.locator('a[routerLink="/sessions"]')).toBeVisible();
  });
});
