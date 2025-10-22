import { test, expect } from '../fixtures/authenticated.fixture';
import { KendoGrid } from '../page-objects/kendo-grid.po';

test.describe('Admin Quotes', () => {
  test.beforeEach(async ({ adminPage }) => {
    await adminPage.goto('/quotes', { waitUntil: 'networkidle' });
  });

  test('page loads with current quote display', async ({ adminPage }) => {
    /* The quotes page fetches a random quote on load */
    await expect(adminPage.locator('blockquote')).toBeVisible({ timeout: 10000 });
  });

  test('quote grid loads with data', async ({ adminPage }) => {
    const grid = new KendoGrid(adminPage);
    await grid.waitForLoaded();
    expect(await grid.hasData()).toBe(true);
  });

  test('grid shows author and quote columns', async ({ adminPage }) => {
    const grid = new KendoGrid(adminPage);
    await grid.waitForLoaded();

    await expect(adminPage.getByRole('columnheader', { name: 'Author' })).toBeVisible();
    await expect(adminPage.getByRole('columnheader', { name: 'Quote' })).toBeVisible();
    await expect(adminPage.getByRole('columnheader', { name: 'Category' })).toBeVisible();
    await expect(adminPage.getByRole('columnheader', { name: 'Tags' })).toBeVisible();
  });

  test('grid shows mock quote authors', async ({ adminPage }) => {
    const grid = new KendoGrid(adminPage);
    await grid.waitForLoaded();

    await expect(adminPage.getByText('Albert Einstein')).toBeVisible({ timeout: 5000 });
    await expect(adminPage.getByText('Winston Churchill')).toBeVisible();
  });

  test('get new quote button triggers API call', async ({ adminPage }) => {
    await expect(adminPage.locator('blockquote')).toBeVisible({ timeout: 10000 });

    const quoteRequest = adminPage.waitForRequest((req) =>
      req.url().includes('/quotes/v1') && req.method() === 'GET' && !req.url().includes('/page'),
    );
    await adminPage.locator('button', { hasText: 'Get New Quote' }).click();
    const req = await quoteRequest;
    expect(req.method()).toBe('GET');
  });

  test('current quote shows category and tags', async ({ adminPage }) => {
    await expect(adminPage.locator('blockquote')).toBeVisible({ timeout: 10000 });

    await expect(adminPage.getByText('Category:')).toBeVisible();
    await expect(adminPage.getByText('Tags:')).toBeVisible();
  });
});
