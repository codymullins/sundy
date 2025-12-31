import { test, expect, Page } from '@playwright/test';

/**
 * E2E tests for Sundy Tauri/Blazor Calendar Application
 *
 * These tests verify:
 * 1. The app starts successfully
 * 2. The calendars load properly
 *
 * Prerequisites:
 * - The Blazor WebAssembly app must be running on http://localhost:1420
 * - Start with: ./dev.sh (in the ui/tauri/Sundy directory)
 */

/**
 * Wait for Blazor WebAssembly to fully initialize.
 * WebKit is significantly slower at loading WASM, so we need robust waiting.
 */
async function waitForBlazorApp(page: Page): Promise<void> {
  // Navigate and wait for network to settle (WASM files to download)
  await page.goto('/', { waitUntil: 'networkidle' });

  // Wait for the Blazor framework to be ready
  // The app shows "Loading..." while the .NET runtime initializes
  const loadingIndicator = page.locator('text=Loading...');

  // Give WebKit extra time - wait up to 60s for loading to disappear
  try {
    await loadingIndicator.waitFor({ state: 'visible', timeout: 5000 });
    await loadingIndicator.waitFor({ state: 'hidden', timeout: 60000 });
  } catch {
    // Loading might have already finished, continue
  }

  // Wait for the main app container with generous timeout for WebKit
  const calendarApp = page.locator('.calendar-app');
  await expect(calendarApp).toBeVisible({ timeout: 60000 });

  // Additional stabilization wait for WebKit rendering
  await page.waitForLoadState('domcontentloaded');
}

test.describe('App Startup', () => {
  test('should load the application successfully', async ({ page }) => {
    await waitForBlazorApp(page);

    // Verify no error state is displayed
    const errorIndicator = page.locator('text=Error:');
    await expect(errorIndicator).not.toBeVisible();

    // Verify the main app container is present
    const calendarApp = page.locator('.calendar-app');
    await expect(calendarApp).toBeVisible();
  });

  test('should display the main toolbar', async ({ page }) => {
    await waitForBlazorApp(page);

    // Verify toolbar is present
    const toolbar = page.locator('.calendar-toolbar');
    await expect(toolbar).toBeVisible();

    // Verify "Today" button is present
    const todayButton = page.locator('.today-btn');
    await expect(todayButton).toBeVisible();
    await expect(todayButton).toHaveText('Today');

    // Verify navigation buttons are present
    const prevButton = page.locator('.nav-btn').first();
    const nextButton = page.locator('.nav-btn').nth(1);
    await expect(prevButton).toBeVisible();
    await expect(nextButton).toBeVisible();

    // Verify current month header is displayed
    const monthHeader = page.locator('.current-month');
    await expect(monthHeader).toBeVisible();
    // Should contain month and year (e.g., "December 2024")
    await expect(monthHeader).toHaveText(/\w+ \d{4}/);
  });

  test('should display view toggle buttons', async ({ page }) => {
    await waitForBlazorApp(page);

    // Verify view toggle container is present
    const viewToggle = page.locator('.view-toggle');
    await expect(viewToggle).toBeVisible();

    // Verify all view buttons are present
    const dayButton = page.locator('.view-btn:has-text("Day")');
    const weekButton = page.locator('.view-btn:has-text("Week")');
    const monthButton = page.locator('.view-btn:has-text("Month")');
    const dynamicButton = page.locator('.view-btn:has-text("Dynamic")');

    await expect(dayButton).toBeVisible();
    await expect(weekButton).toBeVisible();
    await expect(monthButton).toBeVisible();
    await expect(dynamicButton).toBeVisible();
  });

  test('should display the New Event button', async ({ page }) => {
    await waitForBlazorApp(page);

    // Verify "New Event" button is present
    const newEventButton = page.locator('.new-event-btn');
    await expect(newEventButton).toBeVisible();
    await expect(newEventButton).toContainText('New Event');
  });
});

test.describe('Calendar Loading', () => {
  test('should display the calendar sidebar', async ({ page }) => {
    await waitForBlazorApp(page);

    // The sidebar exists but may be hidden on smaller screens
    // Check it exists in the DOM
    const sidebar = page.locator('.calendar-sidebar');
    await expect(sidebar).toBeAttached();
  });

  test('should display "My Calendars" section in sidebar', async ({ page }) => {
    await waitForBlazorApp(page);

    // Open sidebar if not visible (click menu button)
    const menuBtn = page.locator('.menu-btn');
    const sidebar = page.locator('.calendar-sidebar');

    // Check if sidebar has 'open' class, if not click menu button
    const isOpen = await sidebar.evaluate(el => el.classList.contains('open'));
    if (!isOpen) {
      await menuBtn.click();
      // Wait for sidebar animation
      await page.waitForTimeout(300);
    }

    // Verify "My Calendars" header is present
    const sidebarHeader = page.locator('.sidebar-header h2');
    await expect(sidebarHeader).toBeVisible();
    await expect(sidebarHeader).toHaveText('My Calendars');
  });

  test('should display calendar list container', async ({ page }) => {
    await waitForBlazorApp(page);

    // Open sidebar if needed
    const menuBtn = page.locator('.menu-btn');
    const sidebar = page.locator('.calendar-sidebar');

    const isOpen = await sidebar.evaluate(el => el.classList.contains('open'));
    if (!isOpen) {
      await menuBtn.click();
      await page.waitForTimeout(300);
    }

    // Verify calendar list container is present
    const calendarList = page.locator('.calendar-list');
    await expect(calendarList).toBeVisible();
  });

  test('should load calendar groups after initialization', async ({ page }) => {
    await waitForBlazorApp(page);

    // Open sidebar
    const menuBtn = page.locator('.menu-btn');
    const sidebar = page.locator('.calendar-sidebar');

    const isOpen = await sidebar.evaluate(el => el.classList.contains('open'));
    if (!isOpen) {
      await menuBtn.click();
      await page.waitForTimeout(300);
    }

    // Wait for calendar groups to load
    // There should be at least one calendar group (e.g., "Local")
    const calendarGroups = page.locator('.calendar-group');

    // Wait up to 10 seconds for calendar groups to appear
    await expect(calendarGroups.first()).toBeVisible({ timeout: 10000 });

    // Verify at least one group exists
    const groupCount = await calendarGroups.count();
    expect(groupCount).toBeGreaterThanOrEqual(1);
  });

  test('should display calendar grid in month view', async ({ page }) => {
    await waitForBlazorApp(page);

    // Click on Month view to ensure we're in month view
    const monthButton = page.locator('.view-btn:has-text("Month")');
    await monthButton.click();

    // Wait for calendar grid to be visible
    const calendarGrid = page.locator('.calendar-grid');
    await expect(calendarGrid).toBeVisible({ timeout: 10000 });

    // Verify day headers are present
    const dayHeaders = page.locator('.day-headers');
    await expect(dayHeaders).toBeVisible();

    // Verify calendar cells container is present
    const calendarCells = page.locator('.calendar-cells');
    await expect(calendarCells).toBeVisible();

    // Verify we have calendar weeks
    const calendarWeeks = page.locator('.calendar-week');
    const weekCount = await calendarWeeks.count();
    expect(weekCount).toBeGreaterThanOrEqual(4); // A month should have at least 4 weeks
    expect(weekCount).toBeLessThanOrEqual(6); // And at most 6 weeks
  });

  test('should display day headers with correct names', async ({ page }) => {
    await waitForBlazorApp(page);

    // Ensure month view
    const monthButton = page.locator('.view-btn:has-text("Month")');
    await monthButton.click();

    // Wait for calendar grid
    const calendarGrid = page.locator('.calendar-grid');
    await expect(calendarGrid).toBeVisible({ timeout: 10000 });

    // Verify day headers contain expected day names
    const dayHeaders = page.locator('.day-header');
    const headerCount = await dayHeaders.count();
    expect(headerCount).toBe(7); // 7 days in a week

    // Check that Sunday header exists (first day)
    const firstHeader = dayHeaders.first();
    await expect(firstHeader).toContainText('S'); // Short form for Sunday
  });

  test('should highlight today in the calendar', async ({ page }) => {
    await waitForBlazorApp(page);

    // Ensure month view
    const monthButton = page.locator('.view-btn:has-text("Month")');
    await monthButton.click();

    // Wait for calendar grid
    const calendarGrid = page.locator('.calendar-grid');
    await expect(calendarGrid).toBeVisible({ timeout: 10000 });

    // Click "Today" button to ensure we're viewing the current month
    const todayButton = page.locator('.today-btn');
    await todayButton.click();

    // Wait a moment for navigation
    await page.waitForTimeout(500);

    // Verify today cell is highlighted
    const todayCell = page.locator('.calendar-cell.today');
    await expect(todayCell).toBeVisible();

    // Verify today's day number is highlighted
    const todayNumber = page.locator('.day-number.today-number');
    await expect(todayNumber).toBeVisible();

    // Verify the today number matches actual today's date
    const today = new Date();
    await expect(todayNumber).toHaveText(today.getDate().toString());
  });

  test('should display settings button in sidebar', async ({ page }) => {
    await waitForBlazorApp(page);

    // Open sidebar
    const menuBtn = page.locator('.menu-btn');
    const sidebar = page.locator('.calendar-sidebar');

    const isOpen = await sidebar.evaluate(el => el.classList.contains('open'));
    if (!isOpen) {
      await menuBtn.click();
      await page.waitForTimeout(300);
    }

    // Verify settings button is present
    const settingsButton = page.locator('.settings-btn');
    await expect(settingsButton).toBeVisible();
    await expect(settingsButton).toContainText('Settings');
  });
});

test.describe('Calendar View Navigation', () => {
  test('should switch between calendar views', async ({ page }) => {
    await waitForBlazorApp(page);

    // Test Day view
    const dayButton = page.locator('.view-btn:has-text("Day")');
    await dayButton.click();
    await expect(dayButton).toHaveClass(/active/);

    // Test Week view
    const weekButton = page.locator('.view-btn:has-text("Week")');
    await weekButton.click();
    await expect(weekButton).toHaveClass(/active/);

    // Test Month view
    const monthButton = page.locator('.view-btn:has-text("Month")');
    await monthButton.click();
    await expect(monthButton).toHaveClass(/active/);
    await expect(page.locator('.calendar-grid')).toBeVisible();

    // Test Dynamic view
    const dynamicButton = page.locator('.view-btn:has-text("Dynamic")');
    await dynamicButton.click();
    await expect(dynamicButton).toHaveClass(/active/);
  });

  test('should navigate to previous and next periods', async ({ page }) => {
    await waitForBlazorApp(page);

    // Ensure month view
    const monthButton = page.locator('.view-btn:has-text("Month")');
    await monthButton.click();

    // Get initial month text
    const monthHeader = page.locator('.current-month');
    const initialMonth = await monthHeader.textContent();

    // Navigate to next month
    const nextButton = page.locator('.nav-btn').nth(1);
    await nextButton.click();
    await page.waitForTimeout(300);

    // Verify month changed
    const nextMonth = await monthHeader.textContent();
    expect(nextMonth).not.toBe(initialMonth);

    // Navigate to previous month
    const prevButton = page.locator('.nav-btn').first();
    await prevButton.click();
    await page.waitForTimeout(300);

    // Verify we're back to original month
    const currentMonth = await monthHeader.textContent();
    expect(currentMonth).toBe(initialMonth);
  });

  test('should return to today when clicking Today button', async ({ page }) => {
    await waitForBlazorApp(page);

    // Ensure month view
    const monthButton = page.locator('.view-btn:has-text("Month")');
    await monthButton.click();

    // Navigate away from current month
    const nextButton = page.locator('.nav-btn').nth(1);
    await nextButton.click();
    await nextButton.click();
    await page.waitForTimeout(300);

    // Click Today button
    const todayButton = page.locator('.today-btn');
    await todayButton.click();
    await page.waitForTimeout(500);

    // Verify today cell is visible
    const todayCell = page.locator('.calendar-cell.today');
    await expect(todayCell).toBeVisible();
  });
});
