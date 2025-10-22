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
    const token = createMockJwt('admin@local');

    await page.goto(`/login?token=${encodeURIComponent(token)}`, { waitUntil: 'domcontentloaded' });

    /* Transfer UI should appear while token is being hydrated */
    const transferHeading = page.getByText('Signing you in...');
    await expect(transferHeading).toBeVisible({ timeout: 5000 });
    await expect(page.getByText('Transferring your session securely')).toBeVisible();

    const transferVisibleAt = Date.now();

    /* Should navigate to dashboard after the minimum delay */
    await page.waitForURL('**/dashboard', { timeout: 10000 });

    const elapsed = Date.now() - transferVisibleAt;
    expect(elapsed).toBeGreaterThanOrEqual(900);
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
