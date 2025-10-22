import { test, expect } from '@playwright/test';
import { mockStsLogin, mockUserApi } from '../mocks/api-mocks';
import { waitForAngularRender } from '../helpers/angular.helper';

const USER_API = 'http://localhost:55109/api';

test.describe('User Confirm Email', () => {
  test('valid token shows success state', async ({ page }) => {
    await mockStsLogin(page);
    await mockUserApi(page);

    await page.goto('/confirm/email/valid-token-123', { waitUntil: 'domcontentloaded' });

    await waitForAngularRender(page, 'app-confirm-email', { success: true });

    await expect(page.getByText('Email Confirmed!')).toBeVisible({ timeout: 5000 });
    await expect(page.getByText('successfully verified')).toBeVisible();
    await expect(page.getByText('Sign In')).toBeVisible();
  });

  test('failed token shows error state', async ({ page }) => {
    await mockStsLogin(page);
    await mockUserApi(page);

    /* Override the confirm endpoint to fail */
    await page.route(`${USER_API}/credentials/v1/email/confirm`, (route) =>
      route.fulfill({ status: 400, contentType: 'application/json', body: JSON.stringify({ message: 'Token expired' }) }),
    );

    await page.goto('/confirm/email/expired-token', { waitUntil: 'domcontentloaded' });

    await waitForAngularRender(page, 'app-confirm-email', { isLoading: false });

    await expect(page.getByText('Confirmation Failed')).toBeVisible({ timeout: 5000 });
    await expect(page.getByText('Back to Login')).toBeVisible();
  });

  test('missing token shows error', async ({ page }) => {
    await mockStsLogin(page);
    await mockUserApi(page);

    await page.goto('/confirm/email/', { waitUntil: 'domcontentloaded' });

    /* Without a token the route doesn't match — shows 404 or error */
    const hasError = page.getByText('Confirmation Failed');
    const hasNotFound = page.getByText('Page Not Found');
    await expect(hasError.or(hasNotFound)).toBeVisible({ timeout: 10000 });
  });
});
