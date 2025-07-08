import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './e2e',
  fullyParallel: false,
  forbidOnly: !!process.env['CI'],
  retries: process.env['CI'] ? 2 : 1,
  workers: 1,
  reporter: [['html', { open: 'never' }]],

  use: {
    actionTimeout: 10000,
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
  },

  webServer: [
    {
      command: 'npx ng build lib-identity && npx ng serve app-identity-admin',
      port: 4302,
      reuseExistingServer: true,
      timeout: 120000,
    },
    {
      command: 'npx ng serve app-identity-user',
      port: 4301,
      reuseExistingServer: true,
      timeout: 120000,
    },
  ],

  projects: [
    {
      name: 'admin',
      testDir: './e2e/admin',
      use: {
        ...devices['Desktop Chrome'],
        baseURL: 'http://localhost:4302',
      },
    },
    {
      name: 'user',
      testDir: './e2e/user',
      use: {
        ...devices['Desktop Chrome'],
        baseURL: 'http://localhost:4301',
      },
    },
  ],
});
