import { test, expect } from '../fixtures/authenticated.fixture';

test.describe('User Portal Profile', () => {
  test.beforeEach(async ({ userPage }) => {
    await userPage.goto('/profile', { waitUntil: 'networkidle' });
  });

  test('profile page loads', async ({ userPage }) => {
    await expect(userPage).toHaveURL(/\/profile/);
  });

  test('profile update sends all required fields', async ({ userPage }) => {
    // Wait for the kendo-textbox to be visible (form only renders after GET completes)
    const firstNameTextbox = userPage.locator('kendo-textbox#firstName');
    await expect(firstNameTextbox).toBeVisible({ timeout: 10000 });

    // Wait for the inner input to have a value (proves the GET /profile/v1 data loaded)
    const firstNameInput = firstNameTextbox.locator('input');
    await expect(firstNameInput).toHaveValue('Regular', { timeout: 5000 });

    // Set up request listener BEFORE modifying the form
    const updatePromise = userPage.waitForRequest((req) =>
      req.url().includes('/profile/v1') && req.method() === 'PUT',
    );

    // Clear and type new value — triggers Angular form dirty state
    await firstNameInput.clear();
    await firstNameInput.pressSequentially('UpdatedFirst');

    // Wait for submit button to be enabled (form is dirty and valid)
    const submitButton = userPage.locator('button[type="submit"]');
    await expect(submitButton).toBeEnabled({ timeout: 5000 });
    await submitButton.click();

    const updateReq = await updatePromise;
    const body = updateReq.postDataJSON();

    // Required fields from Users abstract class + UserV1
    expect(body).toHaveProperty('id');
    expect(body).toHaveProperty('userName');
    expect(body).toHaveProperty('email');
    expect(body).toHaveProperty('firstName');
    expect(body).toHaveProperty('lastName');
    expect(body).toHaveProperty('isHumanBeing');
    expect(body).toHaveProperty('isDeletable');

    // The updated field should reflect the change
    expect(body.firstName).toBe('UpdatedFirst');
  });
});
