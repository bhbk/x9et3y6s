import { Locator, Page } from '@playwright/test';

export class KendoDialog {
  private readonly dialog: Locator;

  constructor(private readonly page: Page) {
    this.dialog = page.locator('kendo-dialog');
  }

  /** Wait for the dialog to appear */
  async waitForOpen(): Promise<void> {
    await this.dialog.waitFor({ state: 'visible', timeout: 5000 });
  }

  /** Check if the dialog is currently visible */
  async isOpen(): Promise<boolean> {
    return this.dialog.isVisible();
  }

  /** Get the dialog title text */
  async getTitle(): Promise<string> {
    const title = this.dialog.locator('.k-dialog-title, .k-window-title');
    return (await title.textContent())?.trim() ?? '';
  }

  /**
   * Fill a textbox field by its associated label text.
   * Uses the kendo-label's `for` attribute to find the exact input component.
   */
  async fillField(labelText: string, value: string): Promise<void> {
    // Find the kendo-label with exact text match via the text attribute
    const label = this.dialog.locator(`kendo-label[text="${labelText}"]`);
    const forId = await label.getAttribute('for');

    if (forId) {
      // Try kendo-textbox input (most common)
      const textboxInput = this.dialog.locator(`kendo-textbox#${forId} input`);
      if (await textboxInput.isVisible().catch(() => false)) {
        await textboxInput.fill(value);
        return;
      }

      // Try kendo-textarea
      const textarea = this.dialog.locator(`kendo-textarea#${forId} textarea`);
      if (await textarea.isVisible().catch(() => false)) {
        await textarea.fill(value);
        return;
      }

      // Try any element with that id containing an input
      const anyInput = this.dialog.locator(`#${forId} input`);
      if (await anyInput.isVisible().catch(() => false)) {
        await anyInput.fill(value);
        return;
      }
    }

    // Last resort fallback: find the closest sibling input after the label
    const input = label.locator('..').locator('input').first();
    await input.fill(value);
  }

  /** Select a value from a kendo-dropdownlist by its label */
  async selectDropdown(labelText: string, optionText: string): Promise<void> {
    const label = this.dialog.locator(`kendo-label[text="${labelText}"]`);
    const forId = await label.getAttribute('for');

    let dropdown: Locator;
    if (forId) {
      dropdown = this.dialog.locator(`kendo-dropdownlist#${forId}`);
    } else {
      const container = label.locator('..');
      dropdown = container.locator('kendo-dropdownlist');
    }

    await dropdown.click();

    // Wait for popup and click the option
    const popup = this.page.locator('kendo-popup');
    await popup.waitFor({ state: 'visible' });
    await popup.locator('li', { hasText: optionText }).click();
  }

  /** Select the first available option from a kendo-dropdownlist by its label */
  async selectFirstDropdownOption(labelText: string): Promise<void> {
    const label = this.dialog.locator(`kendo-label[text="${labelText}"]`);
    const forId = await label.getAttribute('for');

    let dropdown: Locator;
    if (forId) {
      dropdown = this.dialog.locator(`kendo-dropdownlist#${forId}`);
    } else {
      const container = label.locator('..');
      dropdown = container.locator('kendo-dropdownlist');
    }

    await dropdown.click();

    const popup = this.page.locator('kendo-popup');
    await popup.waitFor({ state: 'visible' });
    await popup.locator('li').first().click();
  }

  /** Toggle a checkbox by its label text */
  async toggleCheckbox(labelText: string): Promise<void> {
    const label = this.dialog.locator('label', { hasText: labelText });
    await label.click();
  }

  /** Set a checkbox to a specific state */
  async setCheckbox(labelText: string, checked: boolean): Promise<void> {
    const label = this.dialog.locator('label', { hasText: labelText });
    const checkbox = label.locator('input[type="checkbox"]');
    const isChecked = await checkbox.isChecked();
    if (isChecked !== checked) {
      await label.click();
    }
  }

  /** Click a dialog action button by text (Create, Update, Cancel, Delete) */
  async clickAction(buttonText: string): Promise<void> {
    const actions = this.dialog.locator('kendo-dialog-actions');
    await actions.locator('button', { hasText: buttonText }).click();
  }

  /** Get the content text of a non-form dialog (confirmation dialogs) */
  async getContent(): Promise<string> {
    const content = this.dialog.locator('.k-dialog-content');
    return (await content.textContent())?.trim() ?? '';
  }
}
