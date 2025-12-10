import { test, expect } from '@playwright/test';

test.describe('Product Search', () => {
  test.describe('User Story 1: Basic Text Search', () => {
    test('should display search input on catalog page', async ({ page }) => {
      await page.goto('/');
      
      const searchInput = page.locator('input[name="q"]');
      await expect(searchInput).toBeVisible();
    });

    test('should search for products by keyword', async ({ page }) => {
      await page.goto('/');
      
      // Enter search term
      await page.fill('input[name="q"]', 'Watch');
      await page.click('button[type="submit"]');
      
      // Verify URL contains search parameter
      await expect(page).toHaveURL(/[?&]q=Watch/);
      
      // Verify results are displayed
      await expect(page.locator('.catalog-items')).toBeVisible();
    });

    test('should display no results message for non-matching search', async ({ page }) => {
      await page.goto('/?q=xyznonexistentproduct123');
      
      // Wait for page to load
      await page.waitForLoadState('networkidle');
      
      // Should show no results message
      const noResults = page.locator('.no-results');
      await expect(noResults).toBeVisible();
    });

    test('should clear search when clicking clear button', async ({ page }) => {
      await page.goto('/?q=Alpine');
      
      // Click clear button
      const clearButton = page.locator('.search-clear-btn');
      if (await clearButton.isVisible()) {
        await clearButton.click();
        
        // URL should not contain search parameter
        await expect(page).not.toHaveURL(/[?&]q=/);
      }
    });

    test('should preserve search term in URL for shareability', async ({ page }) => {
      // Navigate directly with search parameter
      await page.goto('/?q=backpack');
      
      // Search input should be populated with the term
      const searchInput = page.locator('input[name="q"]');
      await expect(searchInput).toHaveValue('backpack');
    });
  });
});

