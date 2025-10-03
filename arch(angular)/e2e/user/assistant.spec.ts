import { test, expect } from '../fixtures/authenticated.fixture';

/**
 * User Assistant Tests
 *
 * These tests verify the chat assistant UI behavior for the user portal.
 * The default mock in api-mocks.ts returns 503 for SignalR negotiate,
 * so the hub connection fails fast and the component settles into
 * a disconnected/unavailable state by default.
 */

test.describe('User Assistant', () => {
  test.beforeEach(async ({ userPage }) => {
    await userPage.goto('/assistant', { waitUntil: 'domcontentloaded' });
    // Wait for the chat component to render
    await expect(userPage.locator('lib-chat')).toBeVisible({ timeout: 10000 });
  });

  test('assistant page loads without crash', async ({ userPage }) => {
    const errors: string[] = [];
    userPage.on('pageerror', (error) => {
      errors.push(error.message);
    });

    await userPage.waitForTimeout(2000);

    const viewChildErrors = errors.filter(
      (e) => e.includes('querySelector') || e.includes('nativeElement'),
    );
    expect(viewChildErrors).toHaveLength(0);
  });

  test('shows disconnected state when hub unavailable', async ({ userPage }) => {
    // Wait for connection attempt to settle (default mock returns 503 fast)
    await userPage.waitForTimeout(2000);

    // Should show unavailable state (user sees "Assistant unavailable")
    const unavailableText = userPage.locator('lib-chat').getByText(/unavailable/i);
    await expect(unavailableText).toBeVisible({ timeout: 5000 });
  });

  test('never shows raw error text', async ({ userPage }) => {
    await userPage.waitForTimeout(2000);

    const pageText = (await userPage.locator('lib-chat').textContent()) ?? '';

    // Should never contain raw exception details
    expect(pageText).not.toMatch(/HttpRequestException/i);
    expect(pageText).not.toMatch(/AmazonServiceException/i);
    expect(pageText).not.toMatch(/System\./);
    expect(pageText).not.toMatch(/stack\s*trace/i);
    expect(pageText).not.toMatch(/at\s+Bhbk\./);
    expect(pageText).not.toMatch(/NullReferenceException/i);
  });

  test('never shows admin-only text', async ({ userPage }) => {
    await userPage.waitForTimeout(2000);

    const pageText = (await userPage.locator('lib-chat').textContent()) ?? '';

    // User SPA should never see admin-specific messages
    expect(pageText).not.toMatch(/mis-configured/i);
    expect(pageText).not.toMatch(/No LLM configured/i);
  });
});

test.describe('User Display Name', () => {
  test('user display name appears in hamburger menu', async ({ userPage }) => {
    await userPage.goto('/dashboard', { waitUntil: 'domcontentloaded' });

    const menuButton = userPage.locator('[data-menu-container] button');
    await expect(menuButton).toBeVisible({ timeout: 10000 });
    await menuButton.click();

    // Mock JWT sets name: 'Regular User' for user@local
    const displayNameEl = userPage.locator('[data-menu-container] .text-gray-900');
    await expect(displayNameEl).toBeVisible();
    await expect(displayNameEl).toHaveText('Regular User');
  });
});
