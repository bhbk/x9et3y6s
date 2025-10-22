import { test, expect } from '@playwright/test';
import { mockStsLogin, mockUserApi } from '../mocks/api-mocks';
import { waitForAngularRender } from '../helpers/angular.helper';

const USER_API = 'http://localhost:55109/api';

test.describe('User Confirm Phone', () => {
  test('valid code shows success state', async ({ page }) => {
    await mockStsLogin(page);
    await mockUserApi(page);

    await page.goto('/confirm/phone/482910', { waitUntil: 'domcontentloaded' });

    await waitForAngularRender(page, 'app-confirm-phone', { success: true });

    await expect(page.getByText('Phone Confirmed!')).toBeVisible({ timeout: 5000 });
    await expect(page.getByText('successfully verified')).toBeVisible();
    await expect(page.getByText('Go to Dashboard')).toBeVisible();
  });

  test('invalid code shows error state', async ({ page }) => {
    await mockStsLogin(page);
    await mockUserApi(page);

    /* Override the confirm endpoint to fail */
    await page.route(`${USER_API}/credentials/v1/phone/confirm`, (route) =>
      route.fulfill({ status: 400, contentType: 'application/json', body: JSON.stringify({ message: 'Invalid code' }) }),
    );

    await page.goto('/confirm/phone/000000', { waitUntil: 'domcontentloaded' });

    await waitForAngularRender(page, 'app-confirm-phone', { isLoading: false });

    await expect(page.getByText('Confirmation Failed')).toBeVisible({ timeout: 5000 });
    await expect(page.getByText('Go to Dashboard')).toBeVisible();
  });
});
