import { test, expect } from '../fixtures/authenticated.fixture';

test.describe('User Portal Dashboard', () => {
  test('dashboard loads after authentication', async ({ userPage }) => {
    await expect(userPage).toHaveURL(/\/dashboard/);
  });
});
