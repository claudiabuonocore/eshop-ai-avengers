import { test, expect } from '@playwright/test';
import { STORAGE_STATE } from '../playwright.config';

test.describe('Product Reviews', () => {
  test('Anonymous user sees reviews list and aggregate', async ({ page }) => {
    await page.goto('/');
    await page.getByRole('link', { name: /adventure/i }).first().click();
    await expect(page.getByRole('heading', { name: 'Customer Reviews' })).toBeVisible();

    // Aggregate values appear (may be zero if none)
    const aggregateText = await page.getByText(/Average rating:/).textContent();
    expect(aggregateText).toBeTruthy();

    // Submit form prompts login for anonymous
    await expect(page.getByText(/Log in to write a review/i)).toBeVisible();
  });

  test.describe('Logged in', () => {
    test.use({ storageState: STORAGE_STATE });

    test('Can submit a review and see it listed', async ({ page }) => {
      await page.goto('/');
      await page.getByRole('link', { name: /adventure/i }).first().click();

      await expect(page.getByRole('heading', { name: 'Customer Reviews' })).toBeVisible();

      // Fill and submit review form
      await page.getByLabel('Rating (1-5)').fill('5');
      await page.getByLabel('Title').fill('Great product');
      await page.getByLabel('Content').fill('Really enjoyed using this.');
      await page.getByRole('button', { name: /submit review/i }).click();

      // New review should appear in the list
      await expect(page.getByText('Great product')).toBeVisible();
      await expect(page.getByText(/Really enjoyed using this/i)).toBeVisible();

      // Aggregate should update (text present)
      await expect(page.getByText(/Average rating:/)).toBeVisible();
    });
  });
});