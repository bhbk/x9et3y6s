import { Locator, Page } from '@playwright/test';

export class KendoGrid {
  private readonly grid: Locator;

  constructor(private readonly page: Page, selector = 'kendo-grid') {
    this.grid = page.locator(selector);
  }

  /** Wait for the grid loading indicator to disappear */
  async waitForLoaded(): Promise<void> {
    // Wait for grid to be visible
    await this.grid.waitFor({ state: 'visible' });
    // Wait for loading overlay to disappear (Kendo uses .k-loading-mask)
    const loadingMask = this.grid.locator('.k-loading-mask');
    if (await loadingMask.isVisible().catch(() => false)) {
      await loadingMask.waitFor({ state: 'hidden', timeout: 15000 });
    }
    // Also wait for any kendo-loader inside the grid
    const loader = this.grid.locator('kendo-loader');
    if (await loader.isVisible().catch(() => false)) {
      await loader.waitFor({ state: 'hidden', timeout: 15000 });
    }
  }

  /** Get the number of visible data rows */
  async getRowCount(): Promise<number> {
    await this.waitForLoaded();
    return this.grid.locator('tbody tr[kendogridlogicalrow]').count();
  }

  /** Get the text content of a cell by row and column index (0-based) */
  async getCellText(rowIndex: number, colIndex: number): Promise<string> {
    const row = this.grid.locator('tbody tr[kendogridlogicalrow]').nth(rowIndex);
    const cell = row.locator('td').nth(colIndex);
    return (await cell.textContent())?.trim() ?? '';
  }

  /** Find a row containing the specified text */
  getRowByText(text: string): Locator {
    return this.grid.locator('tbody tr[kendogridlogicalrow]', { hasText: text });
  }

  /** Click edit button on a row containing the specified text */
  async clickEditOnRow(text: string): Promise<void> {
    const row = this.getRowByText(text);
    await row.locator('button[title="Edit"]').click();
  }

  /** Click delete button on a row containing the specified text */
  async clickDeleteOnRow(text: string): Promise<void> {
    const row = this.getRowByText(text);
    await row.locator('button[title="Delete"]').click();
  }

  /** Click the next page button */
  async clickPageNext(): Promise<void> {
    await this.grid.locator('.k-pager .k-i-caret-alt-right, .k-pager [aria-label="Go to the next page"]').click();
    await this.waitForLoaded();
  }

  /** Click the previous page button */
  async clickPagePrev(): Promise<void> {
    await this.grid.locator('.k-pager .k-i-caret-alt-left, .k-pager [aria-label="Go to the previous page"]').click();
    await this.waitForLoaded();
  }

  /** Check if the grid has any data rows */
  async hasData(): Promise<boolean> {
    return (await this.getRowCount()) > 0;
  }

  /** Get the pager info text (e.g., "1 - 20 of 50 items") */
  async getPagerInfo(): Promise<string> {
    const pagerInfo = this.grid.locator('.k-pager-info');
    return (await pagerInfo.textContent())?.trim() ?? '';
  }
}
