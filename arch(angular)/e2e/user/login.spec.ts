import { test, expect } from '@playwright/test';
import { LoginPage } from '../page-objects/login.po';
import { mockStsLogin, mockUserApi } from '../mocks/api-mocks';

test.describe('User Portal Login', () => {
  test.beforeEach(async ({ page }) => {
    await mockStsLogin(page);
    await mockUserApi(page);
  });

  test('valid credentials redirect to dashboard', async ({ page }) => {
    const loginPage = new LoginPage(page);
    await loginPage.goto();
    await loginPage.login('user@local', 'password');
    await loginPage.expectRedirectedToDashboard();
  });

  test('invalid credentials show error', async ({ page }) => {
    const loginPage = new LoginPage(page);
    await loginPage.goto();
    await loginPage.login('bad@user.com', 'wrongpassword');
    const error = await loginPage.getError();
    expect(error).toContain('Authentication failed');
  });

  test('unauthenticated access redirects to login', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForURL('**/login**');
    await expect(page).toHaveURL(/\/login/);
  });
});
