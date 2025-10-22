import { test, expect } from '../fixtures/authenticated.fixture';

test.describe('User Navigation', () => {
  test('sidebar links navigate to correct pages', async ({ userPage }) => {
    await userPage.goto('/dashboard', { waitUntil: 'networkidle' });

    const sidebar = userPage.locator('aside nav');
    const links = [
      { route: '/profile' },
      { route: '/security' },
      { route: '/sessions' },
      { route: '/dashboard' },
    ];

    for (const { route } of links) {
      const link = sidebar.locator(`a[routerLink="${route}"]`);
      await expect(link).toBeVisible({ timeout: 10000 });
      await link.click();
      await userPage.waitForURL(`**${route}`, { timeout: 10000 });
    }
  });

  test('hamburger menu shows user display name', async ({ userPage }) => {
    await userPage.goto('/dashboard', { waitUntil: 'networkidle' });

    const menuButton = userPage.locator('[data-menu-container] button');
    await expect(menuButton).toBeVisible({ timeout: 10000 });
    await menuButton.click();

    const displayNameEl = userPage.locator('[data-menu-container] .text-gray-900');
    await expect(displayNameEl).toBeVisible();
    await expect(displayNameEl).toHaveText('Regular User');
  });

  test('sign out triggers logout API call', async ({ userPage }) => {
    await userPage.goto('/dashboard', { waitUntil: 'networkidle' });

    const menuButton = userPage.locator('[data-menu-container] button');
    await expect(menuButton).toBeVisible({ timeout: 10000 });
    await menuButton.click();

    const logoutButton = userPage.locator('[data-menu-container]').getByText('Sign Out');
    await expect(logoutButton).toBeVisible();

    /* Verify Sign Out triggers the logout API call */
    const logoutRequest = userPage.waitForRequest(
      (req) => req.url().includes('/sessions/v1/logout') && req.method() === 'POST',
    );
    await logoutButton.click();

    const req = await logoutRequest;
    expect(req.method()).toBe('POST');
  });

  test('active route highlights in sidebar', async ({ userPage }) => {
    await userPage.goto('/profile', { waitUntil: 'networkidle' });

    const profileLink = userPage.locator('aside nav a[routerLink="/profile"]');
    await expect(profileLink).toHaveClass(/bg-blue-50/);
  });

  test('assistant link is visible in sidebar', async ({ userPage }) => {
    await userPage.goto('/dashboard', { waitUntil: 'networkidle' });

    const assistantLink = userPage.locator('a[routerLink="/assistant"]');
    await expect(assistantLink).toBeVisible({ timeout: 10000 });
  });
});
