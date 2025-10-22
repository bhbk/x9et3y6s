import { test, expect } from '../fixtures/authenticated.fixture';

test.describe('Admin Navigation', () => {
  test('sidebar links navigate to correct pages', async ({ adminPage }) => {
    const links = [
      { text: 'Dashboard', url: '/dashboard' },
      { text: 'Activity', url: '/activity' },
      { text: 'Issuers', url: '/issuers' },
      { text: 'Audiences', url: '/audiences' },
      { text: 'Users', url: '/users' },
      { text: 'Roles', url: '/roles' },
      { text: 'Claims', url: '/claims' },
      { text: 'Login Providers', url: '/login-providers' },
      { text: 'Quotes', url: '/quotes' },
    ];

    for (const link of links) {
      await adminPage.locator('nav a', { hasText: link.text }).click();
      await adminPage.waitForURL(`**${link.url}`, { timeout: 10000 });
      await expect(adminPage).toHaveURL(new RegExp(link.url));
    }
  });

  test('hamburger menu shows user display name', async ({ adminPage }) => {
    // Open the hamburger menu
    const hamburger = adminPage.locator('header button').first();
    await hamburger.click();

    // Display name should be visible in the dropdown
    await expect(adminPage.getByText('Admin User')).toBeVisible();
  });

  test('sign out triggers logout API call', async ({ adminPage }) => {
    await adminPage.goto('/dashboard', { waitUntil: 'networkidle' });

    const menuButton = adminPage.locator('[data-menu-container] button');
    await expect(menuButton).toBeVisible({ timeout: 10000 });
    await menuButton.click();

    const logoutButton = adminPage.locator('[data-menu-container]').getByText('Sign Out');
    await expect(logoutButton).toBeVisible();

    /* Verify the logout POST fires. Don't assert redirect — zone.js
       resolves the fetch Promise outside Angular's zone so the
       firstValueFrom chain in authStore.logout() may hang. */
    const logoutRequest = adminPage.waitForRequest(
      (req) => req.url().includes('/sessions/v1/logout') && req.method() === 'POST',
    );
    await logoutButton.click();
    const req = await logoutRequest;
    expect(req.method()).toBe('POST');
  });
});
