import { test as base, Page } from '@playwright/test';
import { injectMockAuth } from '../helpers/auth.helper';
import { ADMIN_USER, REGULAR_USER } from '../helpers/test-data';
import { mockAdminApi, mockStsLogin, mockStsRefresh, mockUserApi } from '../mocks/api-mocks';

type AuthFixtures = {
  adminPage: Page;
  userPage: Page;
};

/**
 * Extends the base Playwright test with pre-authenticated page fixtures.
 * All API calls are intercepted with route mocks — no real backend needed.
 *
 * Usage:
 *   import { test, expect } from '../fixtures/authenticated.fixture';
 *   test('admin dashboard loads', async ({ adminPage }) => { ... });
 */
export const test = base.extend<AuthFixtures>({
  adminPage: async ({ page }, use) => {
    // Set up all route mocks before any navigation
    await mockStsLogin(page);
    await mockStsRefresh(page);
    await mockAdminApi(page);
    await mockUserApi(page);

    // Navigate to login page so localStorage is accessible
    await page.goto('/login', { waitUntil: 'domcontentloaded' });
    // Inject mock JWT for admin user
    await injectMockAuth(page, ADMIN_USER.email);
    // Reload login — Angular's LoginComponent.ngOnInit calls initFromStorage()
    // which hydrates the auth store, then auto-redirects to /dashboard
    await page.goto('/login', { waitUntil: 'domcontentloaded' });
    await page.waitForURL('**/dashboard', { timeout: 15000 });
    await use(page);
  },

  userPage: async ({ page }, use) => {
    await mockStsLogin(page);
    await mockStsRefresh(page);
    await mockUserApi(page);

    await page.goto('/login', { waitUntil: 'domcontentloaded' });
    await injectMockAuth(page, REGULAR_USER.email);
    await page.goto('/login', { waitUntil: 'domcontentloaded' });
    await page.waitForURL('**/dashboard', { timeout: 15000 });
    await use(page);
  },
});

export { expect } from '@playwright/test';
