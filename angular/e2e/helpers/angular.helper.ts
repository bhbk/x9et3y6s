import { Page } from '@playwright/test';

/*
 * Playwright's route mocks can resolve fetch Promises outside Angular's zone.js,
 * updating component properties without triggering template re-render.
 * These helpers wait for component state to settle, then force change detection.
 */

/**
 * Wait for a component's internal state to match, then force Angular CD.
 *
 * @param page     - Playwright Page
 * @param selector - CSS selector for the Angular component element
 * @param check    - key/value pairs to match against the component instance
 */
export async function waitForAngularRender(
  page: Page,
  selector: string,
  check: Record<string, unknown>,
): Promise<void> {
  await page.waitForFunction(
    ({ sel, chk }) => {
      const ng = (window as any).ng;
      const el = document.querySelector(sel);
      if (!ng || !el) return false;
      const comp = ng.getComponent(el);
      if (!comp) return false;
      return Object.entries(chk).every(([k, v]) => comp[k] === v);
    },
    { sel: selector, chk: check },
    { timeout: 10000 },
  );

  await page.evaluate((sel) => {
    const ng = (window as any).ng;
    const el = document.querySelector(sel);
    if (ng && el) {
      const comp = ng.getComponent(el);
      if (comp) ng.applyChanges(comp);
    }
  }, selector);
}
