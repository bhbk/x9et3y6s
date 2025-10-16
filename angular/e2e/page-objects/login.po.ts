import { Locator, Page, expect } from '@playwright/test';

export class LoginPage {
  private readonly emailInput: Locator;
  private readonly passwordInput: Locator;
  private readonly submitButton: Locator;
  private readonly errorBanner: Locator;

  constructor(private readonly page: Page) {
    this.emailInput = page.locator('#user input');
    this.passwordInput = page.locator('#password input');
    this.submitButton = page.locator('button[type="submit"]');
    this.errorBanner = page.locator('.bg-red-50');
  }

  async goto(): Promise<void> {
    await this.page.goto('/login', { waitUntil: 'networkidle' });
  }

  async fillEmail(email: string): Promise<void> {
    await this.emailInput.fill(email);
  }

  async fillPassword(password: string): Promise<void> {
    await this.passwordInput.fill(password);
  }

  async submit(): Promise<void> {
    await this.submitButton.click();
  }

  async login(email: string, password: string): Promise<void> {
    await this.fillEmail(email);
    await this.fillPassword(password);
    await this.submit();
  }

  async getError(): Promise<string> {
    await this.errorBanner.waitFor({ state: 'visible', timeout: 10000 });
    return (await this.errorBanner.textContent())?.trim() ?? '';
  }

  async hasError(): Promise<boolean> {
    return this.errorBanner.isVisible();
  }

  async isLoading(): Promise<boolean> {
    const loader = this.page.locator('kendo-loader');
    return loader.isVisible();
  }

  async expectRedirectedToDashboard(): Promise<void> {
    await this.page.waitForURL('**/dashboard', { timeout: 10000 });
    await expect(this.page).toHaveURL(/\/dashboard/);
  }
}
