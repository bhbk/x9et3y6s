import { test, expect } from '@playwright/test';
import { mockStsLogin, mockStsPasswordReset, mockUserApi } from '../mocks/api-mocks';
import { waitForAngularRender } from '../helpers/angular.helper';

test.describe('User Forgot Password', () => {
  test.beforeEach(async ({ page }) => {
    await mockStsLogin(page);
    await mockStsPasswordReset(page);
    await mockUserApi(page);
    await page.goto('/forgot-password', { waitUntil: 'domcontentloaded' });
  });

  test('page loads with email form', async ({ page }) => {
    await expect(page.getByText('Enter your email to receive a reset link')).toBeVisible({ timeout: 10000 });
    await expect(page.locator('#email')).toBeVisible();
  });

  test('submit button is disabled with empty email', async ({ page }) => {
    const submitBtn = page.locator('button[type="submit"]');
    await expect(submitBtn).toBeVisible({ timeout: 10000 });
    await expect(submitBtn).toBeDisabled();
  });

  test('submitting valid email shows success state', async ({ page }) => {
    await page.locator('#email input').fill('user@example.com');
    await page.locator('button[type="submit"]').click();

    await waitForAngularRender(page, 'app-forgot-password', { submitted: true });

    await expect(page.getByText('Check Your Email')).toBeVisible({ timeout: 5000 });
    await expect(page.getByText("we've sent password reset instructions")).toBeVisible();
  });

  test('success state has back to login link', async ({ page }) => {
    await page.locator('#email input').fill('user@example.com');
    await page.locator('button[type="submit"]').click();

    await waitForAngularRender(page, 'app-forgot-password', { submitted: true });

    await expect(page.getByText('Check Your Email')).toBeVisible({ timeout: 5000 });
    const loginLink = page.getByText('Back to Login');
    await expect(loginLink).toBeVisible();
  });

  test('back to login link navigates to login', async ({ page }) => {
    const loginLink = page.locator('a[href="/login"]');
    await expect(loginLink).toBeVisible({ timeout: 10000 });
    await loginLink.click();
    await page.waitForURL('**/login', { timeout: 10000 });
  });
});
