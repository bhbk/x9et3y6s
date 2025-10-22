import { test, expect } from '../fixtures/authenticated.fixture';

test.describe('User Security', () => {
  test.beforeEach(async ({ userPage }) => {
    await userPage.goto('/security', { waitUntil: 'networkidle' });
  });

  test('page loads with change password card', async ({ userPage }) => {
    await expect(userPage.getByText('Change Password')).toBeVisible({ timeout: 10000 });
    await expect(userPage.getByText('Update your password to keep your account secure')).toBeVisible();
  });

  test('password form fields are present', async ({ userPage }) => {
    await expect(userPage.locator('#currentPassword')).toBeVisible({ timeout: 10000 });
    await expect(userPage.locator('#newPassword')).toBeVisible();
    await expect(userPage.locator('#confirmPassword')).toBeVisible();
  });

  test('password requirements checklist is visible', async ({ userPage }) => {
    await expect(userPage.getByText('Password Requirements')).toBeVisible({ timeout: 10000 });
    await expect(userPage.getByText('At least 10 characters')).toBeVisible();
    await expect(userPage.getByText('One uppercase letter')).toBeVisible();
    await expect(userPage.getByText('One lowercase letter')).toBeVisible();
    await expect(userPage.getByText('One number')).toBeVisible();
    await expect(userPage.getByText('One special character')).toBeVisible();
  });

  test('security tips are visible', async ({ userPage }) => {
    await expect(userPage.getByText('Security Tips')).toBeVisible({ timeout: 10000 });
    await expect(userPage.getByText('Keep your password private')).toBeVisible();
  });

  test('submit button is disabled when form is empty', async ({ userPage }) => {
    const submitBtn = userPage.locator('button[type="submit"]');
    await expect(submitBtn).toBeVisible({ timeout: 10000 });
    await expect(submitBtn).toBeDisabled();
  });

  test('submit sends PUT to credentials endpoint', async ({ userPage }) => {
    /* Fill all fields with a valid password */
    await userPage.locator('#currentPassword input').fill('OldPassword1!');
    await userPage.locator('#newPassword input').fill('NewPassword1!');
    await userPage.locator('#confirmPassword input').fill('NewPassword1!');

    const putPromise = userPage.waitForRequest((req) =>
      req.url().includes('/credentials/v1/password/set') && req.method() === 'PUT',
    );

    await userPage.locator('button[type="submit"]').click();
    const putReq = await putPromise;
    expect(putReq.method()).toBe('PUT');
  });

  test('cancel button resets the form', async ({ userPage }) => {
    await userPage.locator('#currentPassword input').fill('something');

    await userPage.locator('button', { hasText: 'Cancel' }).click();

    /* After reset, the inner input value should be empty */
    await expect(userPage.locator('#currentPassword input')).toHaveValue('');
  });
});
