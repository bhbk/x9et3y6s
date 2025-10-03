import { test, expect } from '@playwright/test';
import { LoginPage } from '../page-objects/login.po';
import { mockStsLogin, mockStsRefresh, mockAdminApi, mockUserApi, createMockJwt } from '../mocks/api-mocks';
import { injectMockAuth } from '../helpers/auth.helper';
import { ADMIN_USER } from '../helpers/test-data';

test.describe('Admin Login', () => {
  test.beforeEach(async ({ page }) => {
    await mockStsLogin(page);
    await mockAdminApi(page);
    await mockUserApi(page);
  });

  test('valid admin credentials redirect to dashboard', async ({ page }) => {
    const loginPage = new LoginPage(page);
    await loginPage.goto();
    await loginPage.login('admin@local', 'password');
    await loginPage.expectRedirectedToDashboard();
  });

  test('invalid credentials show authentication failed', async ({ page }) => {
    const loginPage = new LoginPage(page);
    await loginPage.goto();
    await loginPage.login('bad@user.com', 'wrongpassword');
    const error = await loginPage.getError();
    expect(error).toContain('Authentication failed');
  });

  test('empty form submit is disabled', async ({ page }) => {
    const loginPage = new LoginPage(page);
    await loginPage.goto();
    const submitBtn = page.locator('button[type="submit"]');
    await expect(submitBtn).toBeDisabled();
  });

  test('unauthenticated access redirects to login', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForURL('**/login**');
    await expect(page).toHaveURL(/\/login/);
  });
});

test.describe('Admin Cross-SPA Transfer', () => {
  test.beforeEach(async ({ page }) => {
    await mockStsLogin(page);
    await mockStsRefresh(page);
    await mockAdminApi(page);
    await mockUserApi(page);
  });

  test('transfer screen shows for at least 1 second when token param is present', async ({ page }) => {
    // Generate a valid admin JWT to pass as query parameter
    const token = createMockJwt('admin@local');

    // Navigate to login with the token query parameter (simulates cross-SPA link)
    await page.goto(`/login?token=${encodeURIComponent(token)}`, { waitUntil: 'domcontentloaded' });

    // The transfer screen should be visible
    const transferHeading = page.getByText('Signing you in...');
    await expect(transferHeading).toBeVisible({ timeout: 5000 });
    await expect(page.getByText('Transferring your session securely')).toBeVisible();

    // Record when the transfer screen appeared
    const transferVisibleAt = Date.now();

    // Wait for navigation to dashboard
    await page.waitForURL('**/dashboard', { timeout: 10000 });

    // Verify at least 1 second elapsed while the transfer screen was showing
    const elapsed = Date.now() - transferVisibleAt;
    expect(elapsed).toBeGreaterThanOrEqual(900); // 900ms to account for timing jitter
  });

  test('transfer screen shows even when localStorage already has a valid token', async ({ page }) => {
    // Pre-populate localStorage with a valid token (simulates returning user)
    await page.goto('/login', { waitUntil: 'domcontentloaded' });
    await injectMockAuth(page, ADMIN_USER.email);

    // Now navigate with a token param — the guard should still let it through
    const token = createMockJwt('admin@local');
    await page.goto(`/login?token=${encodeURIComponent(token)}`, { waitUntil: 'domcontentloaded' });

    // The transfer screen should still appear (guard bypasses auth check for ?token=)
    await expect(page.getByText('Signing you in...')).toBeVisible({ timeout: 5000 });
    await expect(page.getByText('Transferring your session securely')).toBeVisible();

    // Should eventually navigate to dashboard
    await page.waitForURL('**/dashboard', { timeout: 10000 });
  });
});
