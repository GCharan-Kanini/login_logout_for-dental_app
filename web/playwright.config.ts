import { defineConfig, devices } from '@playwright/test';

// Ports match api/Properties/launchSettings.json and proxy.conf.json.
const WEB_PORT = 4200;
const API_PORT = 5080;

export default defineConfig({
  testDir: './e2e',
  fullyParallel: false,
  workers: 1,
  forbidOnly: !!process.env['CI'],
  // No retries: a flaky scenario must be fixed, not hidden.
  retries: 0,
  reporter: [['list'], ['html', { open: 'never' }]],
  use: {
    baseURL: `http://localhost:${WEB_PORT}`,
    trace: 'retain-on-failure',
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],
  webServer: [
    {
      command: 'dotnet run --launch-profile http',
      cwd: '../api',
      port: API_PORT,
      reuseExistingServer: !process.env['CI'],
      timeout: 120_000,
    },
    {
      command: `npx ng serve --port ${WEB_PORT}`,
      port: WEB_PORT,
      reuseExistingServer: !process.env['CI'],
      timeout: 120_000,
    },
  ],
});
