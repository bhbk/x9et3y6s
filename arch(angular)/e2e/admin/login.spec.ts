import { test, expect } from '@playwright/test';
import { LoginPage } from '../page-objects/login.po';
import { mockStsLogin, mockAdminApi, mockUserApi } from '../mocks/api-mocks';

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
