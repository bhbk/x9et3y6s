import { test, expect } from '@playwright/test';
import { mockStsLogin, mockStsPasswordReset, mockUserApi } from '../mocks/api-mocks';
import { waitForAngularRender } from '../helpers/angular.helper';

test.describe('User Reset Password', () => {
  test.beforeEach(async ({ page }) => {
    await mockStsLogin(page);
    await mockStsPasswordReset(page);
    await mockUserApi(page);
  });

  test('page loads with password form', async ({ page }) => {
    await page.goto('/reset-password/valid-reset-token-123', { waitUntil: 'domcontentloaded' });

    await expect(page.getByRole('heading', { name: 'Reset Password' })).toBeVisible({ timeout: 10000 });
    await expect(page.locator('#password')).toBeVisible();
    await expect(page.locator('#confirmPassword')).toBeVisible();
  });

  test('submit button is disabled when form is empty', async ({ page }) => {
    await page.goto('/reset-password/valid-reset-token-123', { waitUntil: 'domcontentloaded' });

    const submitBtn = page.locator('button[type="submit"]');
    await expect(submitBtn).toBeVisible({ timeout: 10000 });
    await expect(submitBtn).toBeDisabled();
  });

  test('submitting valid passwords shows success', async ({ page }) => {
    await page.goto('/reset-password/valid-reset-token-123', { waitUntil: 'domcontentloaded' });

    await expect(page.getByRole('heading', { name: 'Reset Password' })).toBeVisible({ timeout: 10000 });

    await page.locator('#password input').fill('NewSecure1!');
    await page.locator('#confirmPassword input').fill('NewSecure1!');

    await page.locator('button[type="submit"]').click();

    await waitForAngularRender(page, 'app-reset-password', { success: true });

    await expect(page.getByText('Password Reset!')).toBeVisible({ timeout: 5000 });
    await expect(page.getByText('successfully reset')).toBeVisible();
    await expect(page.getByText('Sign In')).toBeVisible();
  });

  test('back to login link is visible', async ({ page }) => {
    await page.goto('/reset-password/valid-reset-token-123', { waitUntil: 'domcontentloaded' });

    const loginLink = page.locator('a[href="/login"]');
    await expect(loginLink).toBeVisible({ timeout: 10000 });
  });

  test('missing token shows error', async ({ page }) => {
    await page.goto('/reset-password/', { waitUntil: 'domcontentloaded' });

    /* Without a token, the component shows an error or route goes to not-found */
    const hasError = page.getByText('Invalid reset link');
    const hasNotFound = page.getByText('Page Not Found');
    await expect(hasError.or(hasNotFound)).toBeVisible({ timeout: 10000 });
  });
});
