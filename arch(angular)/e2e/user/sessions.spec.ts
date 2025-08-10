import { test as base, expect } from '../fixtures/authenticated.fixture';
import { mockStsLogin, mockStsRefresh } from '../mocks/api-mocks';
import { USERS } from '../mocks/mock-data';
import { REGULAR_USER } from '../helpers/test-data';
import { injectMockAuth } from '../helpers/auth.helper';

// Re-use the authenticated fixture for most tests
const test = base;

test.describe('User Portal Sessions', () => {
  test('sessions page loads', async ({ userPage }) => {
    await userPage.goto('/sessions', { waitUntil: 'networkidle' });
    await expect(userPage).toHaveURL(/\/sessions/);
  });

  test('session cards display with revoke buttons', async ({ userPage }) => {
    await userPage.goto('/sessions', { waitUntil: 'networkidle' });

    // Wait for sessions to load
    const sessionCards = userPage.locator('.bg-white.rounded-lg.shadow.p-4');
    await expect(sessionCards.first()).toBeVisible();

    // Revoke buttons should be present
    const revokeButtons = userPage.locator('button', { hasText: 'Revoke' });
    expect(await revokeButtons.count()).toBeGreaterThan(0);
  });

  test('revoke button opens confirmation dialog and revokes session', async ({ userPage }) => {
    await userPage.goto('/sessions', { waitUntil: 'networkidle' });

    // Wait for sessions to load
    const revokeButton = userPage.locator('button', { hasText: 'Revoke' }).first();
    await expect(revokeButton).toBeVisible();

    // Count sessions before revoke
    const sessionCards = userPage.locator('.bg-white.rounded-lg.shadow.p-4');
    const countBefore = await sessionCards.count();

    // Click Revoke
    await revokeButton.click();

    // Confirmation dialog should appear
    const dialog = userPage.locator('kendo-dialog');
    await expect(dialog).toBeVisible();
    await expect(dialog.locator('.k-dialog-title, .k-window-title')).toContainText('Revoke Session');

    // Set up request listener before clicking confirm
    const revokePromise = userPage.waitForRequest((req) =>
      req.url().includes('/session/v1/refreshes/') && req.method() === 'DELETE',
    );

    // Click Revoke in the dialog
    const confirmButton = dialog.locator('kendo-dialog-actions button', { hasText: 'Revoke' });
    await confirmButton.click();

    // Verify DELETE request was made
    const revokeReq = await revokePromise;
    expect(revokeReq.method()).toBe('DELETE');

    // Session should be removed from the list
    await expect(sessionCards).toHaveCount(countBefore - 1);
  });

  test('sign out all sessions clears auth and redirects to login', async ({ userPage }) => {
    await userPage.goto('/sessions', { waitUntil: 'networkidle' });

    // Wait for sessions to load — need at least 2 for the "Sign Out All" button
    const sessionCards = userPage.locator('.bg-white.rounded-lg.shadow.p-4');
    await expect(sessionCards.first()).toBeVisible();

    // Click "Sign Out All"
    const signOutAllBtn = userPage.locator('button', { hasText: 'Sign Out All' });
    await expect(signOutAllBtn).toBeVisible();
    await signOutAllBtn.click();

    // Confirmation dialog should appear
    const dialog = userPage.locator('kendo-dialog');
    await expect(dialog).toBeVisible();
    await expect(dialog.locator('.k-dialog-title, .k-window-title')).toContainText('Sign Out All');

    // Set up request listener for the bulk DELETE
    const deletePromise = userPage.waitForRequest((req) =>
      req.url().includes('/session/v1/refreshes') && req.method() === 'DELETE',
    );

    // Confirm sign out all
    const confirmButton = dialog.locator('kendo-dialog-actions button', { hasText: 'Sign Out All' });
    await confirmButton.click();

    // Verify DELETE request was made
    const deleteReq = await deletePromise;
    expect(deleteReq.method()).toBe('DELETE');

    // Should redirect to login page
    await userPage.waitForURL('**/login', { timeout: 10000 });
    await expect(userPage).toHaveURL(/\/login/);

    // Auth token should be cleared from localStorage
    const token = await userPage.evaluate(() => localStorage.getItem('identity_access_token'));
    expect(token).toBeNull();
  });
});

test.describe('User Portal Sessions - Empty State', () => {
  test('sessions page shows empty state when no sessions exist', async ({ page }) => {
    // Set up mocks that return an empty sessions array (the 404 → empty array fix)
    await mockStsLogin(page);
    await mockStsRefresh(page);

    const USER_API = 'http://localhost:55109/api';
    await page.route(`${USER_API}/profile/v1`, (route) =>
      route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify(USERS[1]) }),
    );
    await page.route(`${USER_API}/session/v1/refreshes`, (route) => {
      if (route.request().method() === 'GET') {
        // Return empty array — not 404
        return route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify([]) });
      }
      return route.fulfill({ status: 204 });
    });
    await page.route(`${USER_API}/session/v1/logout`, (route) =>
      route.fulfill({ status: 200 }),
    );
    await page.route(`${USER_API}/motd/v1/page`, (route) =>
      route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify({ data: [], total: 0 }) }),
    );
    await page.route(`${USER_API}/motd/v1`, (route) =>
      route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify({}) }),
    );

    // Authenticate and navigate
    await page.goto('/login', { waitUntil: 'domcontentloaded' });
    await injectMockAuth(page, REGULAR_USER.email);
    await page.goto('/sessions', { waitUntil: 'networkidle' });

    // Should show "No active sessions found" instead of an error
    await expect(page.getByText('No active sessions found')).toBeVisible();

    // Should NOT show an error message
    const errorBanner = page.locator('.bg-red-50');
    await expect(errorBanner).not.toBeVisible();
  });
});
