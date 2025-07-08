import { test, expect } from '../fixtures/authenticated.fixture';

test.describe('User Portal Profile', () => {
  test('profile page loads', async ({ userPage }) => {
    await userPage.goto('/profile', { waitUntil: 'networkidle' });
    await expect(userPage).toHaveURL(/\/profile/);
  });
});
