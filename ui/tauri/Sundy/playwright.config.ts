import { defineConfig, devices } from '@playwright/test';

/**
 * Playwright configuration for Sundy Tauri/Blazor E2E tests.
 *
 * Expects the Blazor WebAssembly app to be running on http://localhost:1420
 * Start the app with: ./dev.sh (in the Sundy directory)
 */
export default defineConfig({
  testDir: './tests',

  // Run tests in parallel
  fullyParallel: true,

  // Fail the build on CI if you accidentally left test.only in the source code
  forbidOnly: !!process.env.CI,

  // Retry on CI only
  retries: process.env.CI ? 2 : 0,

  // Opt out of parallel tests on CI
  workers: process.env.CI ? 1 : undefined,

  // Reporter to use
  reporter: [
    ['html', { open: 'never' }],
    ['list']
  ],

  // Shared settings for all projects
  use: {
    // Base URL for the Blazor WebAssembly app
    baseURL: 'http://localhost:1420',

    // Collect trace when retrying the failed test
    trace: 'on-first-retry',

    // Capture screenshot on failure
    screenshot: 'only-on-failure',

    // Video on failure
    video: 'on-first-retry',
  },

  // Timeout for each test (Blazor WASM can take time to load)
  timeout: 60000,

  // Timeout for expect assertions
  expect: {
    timeout: 30000,
  },

  // Configure projects for major browsers
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
    {
      name: 'firefox',
      use: { ...devices['Desktop Firefox'] },
    },
    {
      name: 'webkit',
      use: {
        ...devices['Desktop Safari'],
        // WebKit needs longer timeouts for Blazor WASM initialization
        navigationTimeout: 90000,
        actionTimeout: 30000,
      },
      // WebKit-specific longer test timeout
      timeout: 120000,
    },
  ],

  // Run your local dev server before starting the tests
  // Comment this out if you want to run the server manually
  // webServer: {
  //   command: './dev.sh',
  //   url: 'http://localhost:1420',
  //   reuseExistingServer: !process.env.CI,
  //   timeout: 120000,
  // },
});
