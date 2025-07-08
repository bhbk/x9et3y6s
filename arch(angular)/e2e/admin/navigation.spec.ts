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
      { text: 'Logins', url: '/logins' },
      { text: 'MOTDs', url: '/motds' },
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

  test('logout clears session and redirects to login', async ({ adminPage }) => {
    // Open hamburger menu first
    const hamburger = adminPage.locator('header button').first();
    await hamburger.click();

    // Click Sign Out in the dropdown
    await adminPage.locator('button', { hasText: 'Sign Out' }).click();
    await adminPage.waitForURL('**/login**', { timeout: 10000 });
    await expect(adminPage).toHaveURL(/\/login/);
  });
});
