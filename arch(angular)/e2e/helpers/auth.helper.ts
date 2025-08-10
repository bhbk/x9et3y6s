import { Page } from '@playwright/test';
import { STORAGE_KEY } from './test-data';
import { createMockJwt } from '../mocks/api-mocks';

/**
 * Inject a mock auth token into the page's localStorage so the Angular
 * app recognizes the user as authenticated on next navigation.
 *
 * This generates a fake JWT locally — no real API call needed.
 */
export async function injectMockAuth(
  page: Page,
  email: string,
  roles?: string[],
): Promise<void> {
  const accessToken = createMockJwt(email, roles ?? []);

  const storageData = JSON.stringify({
    accessToken,
    expiresIn: 3600,
    savedAt: new Date().toISOString(),
  });

  await page.evaluate(
    ({ key, value }) => {
      localStorage.setItem(key, value);
    },
    { key: STORAGE_KEY, value: storageData },
  );
}
