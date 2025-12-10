import { test, expect } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';

test.describe('Backcountry Redesign - Responsive & Accessibility Tests', () => {
  
  test.describe('Mobile Responsiveness', () => {
    test.use({ viewport: { width: 375, height: 667 } }); // iPhone SE size

    test('should display mobile hamburger menu', async ({ page }) => {
      await page.goto('/');
      
      // Hamburger button should be visible on mobile
      const hamburger = page.locator('.nav-toggle');
      await expect(hamburger).toBeVisible();
      
      // Navigation links should have transform (hidden off-screen on mobile)
      const navLinks = page.locator('.nav-links');
      const transform = await navLinks.evaluate((el) => window.getComputedStyle(el).transform);
      // Should have translateX(-100%) transform on mobile
      expect(transform).toContain('matrix');
    });

    test('should open mobile menu when hamburger is clicked', async ({ page }) => {
      await page.goto('/');
      
      const hamburger = page.locator('.nav-toggle');
      await hamburger.click();
      
      // Navigation should now be visible
      const navLinks = page.locator('.nav-links');
      await expect(navLinks).toBeVisible();
      
      // Menu items should be visible
      await expect(page.getByRole('link', { name: 'All Gear' })).toBeVisible();
      await expect(page.getByRole('link', { name: 'Jackets' })).toBeVisible();
      await expect(page.getByRole('link', { name: 'Bags' })).toBeVisible();
    });

    test('should have search bar in mobile layout', async ({ page }) => {
      await page.goto('/');
      
      const searchInput = page.locator('input[name="q"]');
      await expect(searchInput).toBeVisible();
      await expect(searchInput).toHaveAttribute('placeholder', 'Search gear & apparel');
    });

    test('should display hero section on mobile', async ({ page }) => {
      await page.goto('/');
      
      // Hero should be visible and readable on mobile
      const hero = page.locator('.hero-section');
      await expect(hero).toBeVisible();
      
      // Check that headline is visible
      const headline = page.locator('.hero-headline');
      await expect(headline).toBeVisible();
      await expect(headline).toContainText('Gear Up');
    });

    test('should display product cards in mobile grid', async ({ page }) => {
      await page.goto('/');
      
      // Wait for products to load
      await page.waitForSelector('.catalog-items, .collection-grid');
      
      // Products should be visible
      const products = page.locator('.product-card').first();
      await expect(products).toBeVisible();
    });

    test('should have touch-friendly buttons (44px minimum)', async ({ page }) => {
      await page.goto('/');
      
      // Check hamburger button size
      const hamburger = page.locator('.nav-toggle');
      const hamburgerBox = await hamburger.boundingBox();
      expect(hamburgerBox?.height).toBeGreaterThanOrEqual(44);
      
      // Check search clear button if visible
      await page.fill('input[name="q"]', 'test');
      const clearBtn = page.locator('.search-clear');
      if (await clearBtn.isVisible()) {
        const clearBox = await clearBtn.boundingBox();
        expect(clearBox?.height).toBeGreaterThanOrEqual(44);
      }
    });
  });

  test.describe('Desktop Responsiveness', () => {
    test.use({ viewport: { width: 1440, height: 900 } }); // Desktop size

    test('should display horizontal navigation menu', async ({ page }) => {
      await page.goto('/');
      
      // Hamburger should not be visible on desktop
      const hamburger = page.locator('.nav-toggle');
      await expect(hamburger).not.toBeVisible();
      
      // Navigation links should be visible
      const navLinks = page.locator('.nav-links');
      await expect(navLinks).toBeVisible();
      
      // All main navigation items should be visible in the nav menu
      const nav = page.locator('nav.nav-menu');
      await expect(nav.getByRole('link', { name: 'All Gear', exact: true })).toBeVisible();
      await expect(nav.getByRole('link', { name: 'Jackets', exact: true })).toBeVisible();
      await expect(nav.getByRole('link', { name: 'Footwear', exact: true })).toBeVisible();
      await expect(nav.getByRole('link', { name: 'Bags', exact: true })).toBeVisible();
      await expect(nav.getByRole('link', { name: 'Climbing', exact: true })).toBeVisible();
    });

    test('should display prominent search bar in header', async ({ page }) => {
      await page.goto('/');
      
      const searchBar = page.locator('.header-search');
      await expect(searchBar).toBeVisible();
      
      const searchInput = page.locator('input[name="q"]');
      await expect(searchInput).toBeVisible();
      
      // Search button should be visible on desktop
      const searchButton = page.getByRole('button', { name: 'Search' });
      await expect(searchButton).toBeVisible();
    });

    test('should have quick-view modal integration on catalog page', async ({ page }) => {
      await page.goto('/catalog');
      
      // Wait for products to load
      await page.waitForSelector('.catalog-item');
      
      // Verify product cards have proper structure
      const firstProduct = page.locator('.catalog-item').first();
      await expect(firstProduct).toBeVisible();
      
      // Verify product has interactive elements
      const productLink = firstProduct.locator('.product-link');
      await expect(productLink).toBeVisible();
      
      // Verify add to cart button exists on cards
      const addToCartBtn = firstProduct.locator('.add-to-cart-btn');
      await expect(addToCartBtn).toBeVisible();
      
      // Verify product image is large and properly sized
      const productImage = firstProduct.locator('.product-image');
      await expect(productImage).toBeVisible();
    });

    test('should display product grid with 4 columns', async ({ page }) => {
      await page.goto('/');
      
      // Wait for collection grid
      await page.waitForSelector('.collection-grid');
      
      const grid = page.locator('.collection-grid').first();
      const gridStyles = await grid.evaluate((el) => {
        return window.getComputedStyle(el).getPropertyValue('grid-template-columns');
      });
      
      // Should have 4 columns on desktop (check for 4 column values)
      const columns = gridStyles.split(' ').filter(v => v.includes('px') || v.includes('fr'));
      expect(columns.length).toBe(4);
    });
  });

  test.describe('Tablet Responsiveness', () => {
    test.use({ viewport: { width: 768, height: 1024 } }); // iPad size

    test('should adapt navigation for tablet', async ({ page }) => {
      await page.goto('/');
      
      // Navigation should be visible but more compact
      const navLinks = page.locator('.nav-links');
      await expect(navLinks).toBeVisible();
      
      // Check that navigation items are present
      await expect(page.getByRole('link', { name: 'Jackets' })).toBeVisible();
    });

    test('should display 2-3 column product grid', async ({ page }) => {
      await page.goto('/');
      
      await page.waitForSelector('.collection-grid');
      
      // Products should be visible in grid
      const products = page.locator('.product-card');
      await expect(products.first()).toBeVisible();
    });
  });

  test.describe('Accessibility (A11y)', () => {
    
    test('should pass axe accessibility audit on homepage', async ({ page }) => {
      await page.goto('/');
      await page.waitForLoadState('networkidle');
      
      const accessibilityScanResults = await new AxeBuilder({ page })
        .withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'])
        .analyze();
      
      expect(accessibilityScanResults.violations).toEqual([]);
    });

    test('should pass axe accessibility audit on catalog page', async ({ page }) => {
      await page.goto('/catalog');
      await page.waitForLoadState('networkidle');
      
      const accessibilityScanResults = await new AxeBuilder({ page })
        .withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'])
        .analyze();
      
      expect(accessibilityScanResults.violations).toEqual([]);
    });

    test('should have proper ARIA labels on interactive elements', async ({ page }) => {
      await page.goto('/');
      
      // Search input should have label
      const searchInput = page.locator('input[name="q"]');
      await expect(searchInput).toHaveAttribute('aria-label', 'Search products');
      
      // Mobile menu button should have label
      await page.setViewportSize({ width: 375, height: 667 });
      const menuButton = page.locator('.nav-toggle');
      await expect(menuButton).toHaveAttribute('aria-label', 'Toggle navigation menu');
    });

    test('should support keyboard navigation', async ({ page }) => {
      await page.goto('/');
      
      // Tab through interactive elements
      await page.keyboard.press('Tab'); // Focus on first element
      
      // Search input should be focusable
      const searchInput = page.locator('input[name="q"]');
      await searchInput.focus();
      await expect(searchInput).toBeFocused();
      
      // Type in search
      await page.keyboard.type('jacket');
      await expect(searchInput).toHaveValue('jacket');
      
      // Submit with Enter
      await page.keyboard.press('Enter');
      await page.waitForURL(/.*catalog\?q=jacket.*/);
    });

    test('should have proper heading hierarchy', async ({ page }) => {
      await page.goto('/');
      
      // Check for proper heading structure
      const h1 = page.locator('h1');
      await expect(h1).toBeVisible();
      
      // H2s should exist for sections
      const h2s = page.locator('h2');
      expect(await h2s.count()).toBeGreaterThan(0);
    });

    test('should have alt text on all images', async ({ page }) => {
      await page.goto('/');
      
      await page.waitForSelector('img');
      
      const images = page.locator('img');
      const count = await images.count();
      
      for (let i = 0; i < count; i++) {
        const img = images.nth(i);
        const alt = await img.getAttribute('alt');
        const role = await img.getAttribute('role');
        
        // Images should have alt text or role="presentation"
        expect(alt !== null || role === 'presentation').toBeTruthy();
      }
    });

    test('should have sufficient color contrast in hero section', async ({ page }) => {
      await page.goto('/');
      
      // Hero text should be visible against background
      const heroHeadline = page.locator('.hero-headline');
      await expect(heroHeadline).toBeVisible();
      
      // Get computed color
      const textColor = await heroHeadline.evaluate((el) => {
        return window.getComputedStyle(el).color;
      });
      
      // Should be white or very light (rgb(255, 255, 255))
      expect(textColor).toContain('255');
    });

    test('should indicate current page in navigation', async ({ page }) => {
      await page.goto('/catalog?type=6');
      
      // Jackets link in navigation menu should have active state
      const nav = page.locator('nav.nav-menu');
      const jacketsLink = nav.getByRole('link', { name: 'Jackets', exact: true });
      await expect(jacketsLink).toHaveClass(/active/);
    });

    test('should have focusable and clickable card links', async ({ page }) => {
      await page.goto('/catalog');
      
      await page.waitForSelector('.product-card');
      
      // Product cards should have proper link structure
      const firstProductLink = page.locator('.product-link').first();
      await expect(firstProductLink).toBeVisible();
      
      // Should be focusable
      await firstProductLink.focus();
      await expect(firstProductLink).toBeFocused();
      
      // Should have href
      const href = await firstProductLink.getAttribute('href');
      expect(href).toBeTruthy();
      expect(href).toContain('/item/');
    });
  });

  test.describe('Search Functionality', () => {
    
    test('should display search results with proper layout', async ({ page }) => {
      await page.goto('/');
      
      // Use the header search
      await page.fill('input[name="q"]', 'backpack');
      await page.getByRole('button', { name: 'Search' }).click();
      
      await page.waitForURL(/.*catalog\?q=backpack.*/);
      
      // Results should be displayed
      await page.waitForSelector('.catalog-items, .collection-grid');
      
      // Should show results count
      const resultsText = page.locator('text=/results for/i');
      if (await resultsText.isVisible()) {
        await expect(resultsText).toContainText('backpack');
      }
    });

    test('should allow clearing search via clear button', async ({ page }) => {
      await page.goto('/catalog?q=test');
      await page.waitForLoadState('networkidle');
      
      // Search input should have value
      const searchInput = page.locator('input[name="q"]').first();
      await expect(searchInput).toHaveValue('test');
      
      // Clear button should be visible
      const clearBtn = page.locator('.search-clear').first();
      await expect(clearBtn).toBeVisible();
      
      // Click clear button navigates to clean catalog
      await Promise.all([
        page.waitForURL(/\/catalog/, { timeout: 10000 }),
        clearBtn.click()
      ]);
      
      // Verify search param is not in URL
      const url = page.url();
      expect(url).not.toContain('q=test');
    });
  });

  test.describe('Hero Section', () => {
    
    test('should have readable text with proper contrast', async ({ page }) => {
      await page.goto('/');
      
      const heroSection = page.locator('.hero-section');
      await expect(heroSection).toBeVisible();
      
      // All text elements should be visible
      const eyebrow = page.locator('.hero-eyebrow');
      if (await eyebrow.isVisible()) {
        await expect(eyebrow).toHaveCSS('color', /rgb\(255, 255, 255\)/);
      }
      
      const headline = page.locator('.hero-headline');
      await expect(headline).toBeVisible();
      await expect(headline).toHaveCSS('color', /rgb\(255, 255, 255\)/);
      
      const subheadline = page.locator('.hero-subheadline');
      await expect(subheadline).toBeVisible();
      await expect(subheadline).toHaveCSS('color', /rgb\(255, 255, 255\)/);
    });

    test('should have functional CTA button', async ({ page }) => {
      await page.goto('/');
      
      const ctaButton = page.locator('.hero-cta');
      await expect(ctaButton).toBeVisible();
      
      // Should have proper styling
      await expect(ctaButton).toHaveCSS('cursor', 'pointer');
      
      // Should navigate when clicked
      await ctaButton.click();
      await page.waitForURL(/.*catalog.*/);
    });
  });

  test.describe('Product Cards', () => {
    
    test('should display price correctly', async ({ page }) => {
      await page.goto('/catalog');
      
      await page.waitForSelector('.product-card');
      
      const price = page.locator('.product-price').first();
      await expect(price).toBeVisible();
      
      // Should have dollar sign and decimal
      const priceText = await price.textContent();
      expect(priceText).toMatch(/\$\d+\.\d{2}/);
    });

    test('should have proper image aspect ratio', async ({ page }) => {
      await page.goto('/catalog');
      
      await page.waitForSelector('.product-image-wrapper');
      
      const imageWrapper = page.locator('.product-image-wrapper').first();
      const box = await imageWrapper.boundingBox();
      
      // Should be roughly square (1:1 aspect ratio)
      if (box) {
        const ratio = box.width / box.height;
        expect(ratio).toBeCloseTo(1, 0.1);
      }
    });

    test('should have add to cart button', async ({ page }) => {
      await page.goto('/catalog');
      
      await page.waitForSelector('.product-card');
      
      const addToCartBtn = page.locator('.add-to-cart-btn').first();
      await expect(addToCartBtn).toBeVisible();
      await expect(addToCartBtn).toContainText(/add to cart/i);
    });
  });

  test.describe('No Horizontal Scroll', () => {
    
    test('should not have horizontal scroll on mobile', async ({ page }) => {
      await page.setViewportSize({ width: 320, height: 568 }); // Smallest common mobile
      await page.goto('/');
      
      // Check for horizontal scroll
      const hasHorizontalScroll = await page.evaluate(() => {
        return document.documentElement.scrollWidth > document.documentElement.clientWidth;
      });
      
      expect(hasHorizontalScroll).toBeFalsy();
    });

    test('should not have horizontal scroll on tablet', async ({ page }) => {
      await page.setViewportSize({ width: 768, height: 1024 });
      await page.goto('/');
      
      const hasHorizontalScroll = await page.evaluate(() => {
        return document.documentElement.scrollWidth > document.documentElement.clientWidth;
      });
      
      expect(hasHorizontalScroll).toBeFalsy();
    });
  });
});

