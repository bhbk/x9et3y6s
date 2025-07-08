import { test, expect } from '../fixtures/authenticated.fixture';

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
});
