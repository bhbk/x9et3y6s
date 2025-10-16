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

  test('logout clears session and redirects to login', async ({ adminPage }) => {
    // Remove the old STS refresh handler and re-register to return 400.
    // Without this, the guest guard on /login calls tryRefreshToken(),
    // the mock succeeds, and the guard redirects back to /dashboard.
    // Use 400 (not 401) because the error interceptor catches 401s and
    // calls router.navigate(['/login']), which cancels the in-progress
    // navigation and creates a conflict loop.
    const STS_API = 'http://localhost:55114/api';
    await adminPage.unroute(`${STS_API}/oauth2/v2/ropg-rt`);
    await adminPage.route(`${STS_API}/oauth2/v2/ropg-rt`, (route) =>
      route.fulfill({
        status: 400,
        contentType: 'application/json',
        body: JSON.stringify({ error: 'invalid_grant', error_description: 'Session expired' }),
      }),
    );

    // Listen for the logout POST to verify the button click works
    const logoutPromise = adminPage.waitForRequest((req) =>
      req.url().includes('/session/v1/logout') && req.method() === 'POST',
    );

    // Open hamburger menu
    const hamburger = adminPage.locator('[data-menu-container] button');
    await expect(hamburger).toBeVisible({ timeout: 10000 });
    await hamburger.click();

    // Wait for the Sign Out button to appear, then click it
    const signOutBtn = adminPage.locator('[data-menu-container]').locator('button', { hasText: 'Sign Out' });
    await expect(signOutBtn).toBeVisible({ timeout: 5000 });
    await signOutBtn.click();

    // Verify the logout POST was actually sent
    const logoutReq = await logoutPromise;
    expect(logoutReq.method()).toBe('POST');

    // Should eventually reach /login
    await adminPage.waitForURL('**/login**', { timeout: 15000 });
    await expect(adminPage).toHaveURL(/\/login/);
  });
});
