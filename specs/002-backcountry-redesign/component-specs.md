# Backcountry Redesign - Component Specifications

This document provides detailed specifications for all new and modified components in the backcountry redesign feature.

---

## Design System Foundation

### `src/WebApp/wwwroot/css/backcountry-theme.css`
**Purpose**: Core CSS custom properties for the backcountry design system

**Key Features**:
- Color palette (greens, earth tones, blacks)
- Typography scale (font sizes, weights, line heights)
- Spacing scale (4px base grid: space-1 through space-24)
- Responsive breakpoints (mobile: <768px, tablet: 768-1023px, desktop: 1024px+)
- Component tokens (buttons, inputs, cards)
- Animation/transition values

**Variables**:
- Colors: `--bc-green-primary`, `--bc-black`, `--bc-sand`, etc.
- Typography: `--font-base`, `--weight-semibold`, `--leading-normal`, etc.
- Spacing: `--space-1` through `--space-24`
- Components: `--button-padding-x`, `--card-padding`, etc.

### `src/WebApp/wwwroot/css/site.css`
**Purpose**: Site-wide utility classes and element styling

**Key Features**:
- Button styles (primary, secondary, outline)
- Form element styling
- Card components
- Background and text color utilities
- Spacing utilities (mt-*, mb-*, p-*, etc.)
- Responsive display utilities

---

## Page Components

### `src/WebApp/Components/Pages/Index.razor`
**Purpose**: New homepage with hero and featured collections

**Route**: `/`

**Key Features**:
- Full-width hero section with imagery and CTA
- Three featured product collections (Winter Essentials, New Arrivals, Top Picks)
- Responsive grid layouts
- Server-side streaming rendering

**Dependencies**:
- `HeroSection` component
- `FeaturedCollection` component
- `CatalogService`

**Parameters**:
- None (data loaded in OnInitializedAsync)

### `src/WebApp/Components/Pages/Catalog/Catalog.razor`
**Purpose**: Product catalog page with filtering and search

**Route**: `/catalog`

**Key Features**:
- Product grid with modern card layouts
- Search and filter integration
- Quick-view modal support
- Pagination
- Responsive layouts (1-4 columns based on screen size)

**Dependencies**:
- `CatalogSearch` component
- `CatalogListItem` component
- `QuickViewModal` component
- `CatalogService`

**Query Parameters**:
- `page`: Page number
- `brand`: Brand filter ID
- `type`: Category filter ID
- `q`: Search query

### `src/WebApp/Components/Pages/Catalog/Item.razor`
**Purpose**: Product detail page with image gallery and comprehensive information

**Route**: `/item/{id:int}`

**Key Features**:
- Image gallery with thumbnails and lightbox
- Comprehensive product details and specifications
- Sticky "Add to Cart" button (desktop) / Fixed bottom (mobile)
- Related products section
- Responsive two-column layout (desktop) / single column (mobile)

**Dependencies**:
- `ImageGallery` component
- `ProductDetails` component
- `FeaturedCollection` component
- `CatalogService`
- `IProductImageUrlProvider`

**Parameters**:
- `Id`: Product ID from route

---

## Layout Components

### `src/WebApp/Components/Layout/MainLayout.razor`
**Purpose**: Main site layout wrapper

**Key Updates**:
- Added `site-wrapper` and `backcountry-theme` classes
- Wrapped body content in `<main class="main-content">`
- Maintains existing header/footer structure

### `src/WebApp/Components/Layout/HeaderBar.razor`
**Purpose**: Site header with logo, navigation, and user actions

**Key Updates**:
- Integrated `NavMenu` component
- Reorganized layout to accommodate navigation menu
- Added `header-actions` wrapper for user/cart menus

### `src/WebAppComponents/Layout/NavMenu.razor`
**Purpose**: Main site navigation with category dropdowns and mobile menu

**Key Features**:
- Desktop: Horizontal menu with dropdown on hover
- Mobile: Hamburger menu with full-screen overlay
- Category navigation with type and brand filters
- Active state highlighting
- Touch-friendly (44px+ targets)
- Smooth animations and transitions

**Dependencies**:
- `CatalogService` (for loading types and brands)

**Parameters**:
- None (data loaded in OnInitializedAsync)

---

## Shared Components

### `src/WebAppComponents/Shared/HeroSection.razor`
**Purpose**: Full-width hero banner with image, headline, and CTA

**Key Features**:
- Full-width responsive image with overlay
- Centered content (eyebrow, headline, subheadline, CTA)
- Responsive typography scaling
- Accessible image alt text

**Parameters**:
- `ImageUrl`: Hero image URL
- `ImageAlt`: Image alt text (default: "Hero image")
- `Eyebrow`: Optional eyebrow text
- `Headline`: Main headline
- `Subheadline`: Supporting text
- `CtaText`: Call-to-action button text
- `CtaUrl`: Call-to-action button URL

**Responsive Behavior**:
- Mobile: 350px min-height, font-3xl headline
- Tablet: 500px min-height, font-5xl headline
- Desktop: 600px min-height, font-6xl headline

### `src/WebAppComponents/Shared/FeaturedCollection.razor`
**Purpose**: Product collection display with title and grid

**Key Features**:
- Flexible header (title, description, "View All" link)
- Responsive grid (1-4 columns based on screen size)
- Integrates `CatalogListItem` components
- Empty state handling

**Parameters**:
- `Title`: Collection title
- `Description`: Optional description
- `Products`: IEnumerable<CatalogItem>
- `ViewAllUrl`: Optional "View All" link URL

**Grid Behavior**:
- Mobile: 1 column
- Small tablet (640px+): 2 columns
- Tablet (768px+): 3 columns
- Desktop (1024px+): 4 columns

### `src/WebAppComponents/Shared/ImageGallery.razor`
**Purpose**: Product image gallery with thumbnails, navigation, and lightbox

**Key Features**:
- Main image display with zoom hint
- Thumbnail navigation (prev/next buttons + thumbnail grid)
- Full-screen lightbox modal
- Keyboard navigation support
- Touch-friendly controls
- Image counter display

**Parameters**:
- `Images`: IEnumerable<string> of image URLs
- `ImageAlt`: Alt text for images
- `InitialIndex`: Starting image index (default: 0)

**Interactions**:
- Click main image → Open lightbox
- Click thumbnail → Select image
- Arrow buttons → Navigate images
- Lightbox: Click outside → Close

### `src/WebAppComponents/Shared/LoadingSkeleton.razor`
**Purpose**: Animated skeleton placeholder for loading states

**Key Features**:
- Multiple skeleton types (text, image, circle, product-card)
- Shimmer animation effect
- Size variants (small, medium, large)
- Custom styling support

**Parameters**:
- `Type`: "text", "image", "circle", or "product-card" (default: "text")
- `Size`: "small", "medium", or "large" (default: "medium")
- `CustomStyle`: Optional inline styles

### `src/WebAppComponents/Shared/ProductImage.razor`
**Purpose**: Product image display with error handling and fallback

**Key Features**:
- Automatic fallback to placeholder on image load error
- Lazy loading support
- Srcset support for responsive images
- Accessible placeholder with icon and text

**Parameters**:
- `Src`: Image URL
- `Alt`: Alt text (default: "Product image")
- `Loading`: "lazy" or "eager" (default: "lazy")
- `SrcSet`: Optional srcset attribute
- `CssClass`: Optional container CSS class
- `ImageClass`: Optional image CSS class

---

## Catalog Components

### `src/WebAppComponents/Catalog/CatalogListItem.razor`
**Purpose**: Individual product card in catalog listings

**Key Features**:
- Large 1:1 aspect ratio product image
- Hover effects (scale, shadow, quick-view overlay)
- Product name with search term highlighting
- Prominent price display
- Quick-view button on hover
- "Add to Cart" button
- Responsive card layouts

**Parameters**:
- `Item`: CatalogItem (required)
- `IsLoggedIn`: bool (optional)
- `SearchTerm`: string (optional, for highlighting)
- `OnQuickViewRequested`: EventCallback<CatalogItem>

**Hover Behavior**:
- Desktop: Show quick-view overlay, scale image
- Mobile: Always show "Add to Cart" button

### `src/WebAppComponents/Catalog/CatalogSearch.razor`
**Purpose**: Search input and filter controls

**Key Features**:
- Text search input with clear button
- Brand filter tags
- Category/type filter tags
- Preserves URL query parameters
- Responsive layout (vertical on mobile)

**Parameters**:
- `BrandId`: int? (optional)
- `ItemTypeId`: int? (optional)
- `SearchTerm`: string? (optional)

**Responsive Behavior**:
- Desktop: Sidebar layout (14rem width)
- Mobile: Full-width stacked layout

### `src/WebAppComponents/Catalog/QuickViewModal.razor`
**Purpose**: Quick product preview modal without leaving catalog

**Key Features**:
- Split layout (image left, details right on desktop)
- Product image, name, price, description
- Stock availability badge
- "Add to Cart" and "View Full Details" buttons
- Backdrop click to close
- Smooth animations

**Parameters**:
- `IsVisible`: bool (controls modal visibility)
- `Product`: CatalogItem? (product to display)
- `OnClose`: EventCallback (close handler)
- `OnAddToCartRequested`: EventCallback<CatalogItem>

**Responsive Behavior**:
- Mobile: Full-screen modal, vertical layout
- Desktop: Centered modal (max 900px), horizontal layout

### `src/WebAppComponents/Catalog/ProductDetails.razor`
**Purpose**: Comprehensive product information display

**Key Features**:
- Product header (title, price)
- Description section
- Specifications table (brand, category, SKU, availability)
- Dynamic features list (based on product type)
- Care instructions
- Stock availability badge

**Parameters**:
- `Product`: CatalogItem (required)

**Specifications Display**:
- Key-value pairs in a definition list
- Responsive layout (two-column desktop, stacked mobile)
- Stock badge with color coding (green = in stock, red = out of stock)

---

## Responsive Design Strategy

### Breakpoints
- **Mobile**: < 768px (1 column layouts, hamburger menu, simplified UI)
- **Tablet**: 768px - 1023px (2-3 column layouts, compact navigation)
- **Desktop**: ≥ 1024px (4 column layouts, full navigation, hover effects)

### Touch Targets
- Minimum 44x44px for all interactive elements
- Mobile buttons have increased padding
- Mobile menu items have full-width touch areas

### Performance
- Lazy loading images (`loading="lazy"`)
- Responsive images with `srcset` support
- CSS transitions use GPU-accelerated properties
- Minimal JavaScript (Blazor component logic only)

---

## Accessibility Features

### Semantic HTML
- Proper heading hierarchy (h1 → h6)
- Semantic elements (`<nav>`, `<main>`, `<article>`, etc.)
- Definition lists for specifications
- Button vs. link semantics

### ARIA
- `aria-label` on icon-only buttons
- `aria-hidden="true"` on decorative icons
- Modal focus management
- Keyboard navigation support

### Keyboard Navigation
- Tab order follows visual order
- Enter/Space activate buttons
- Escape closes modals
- Arrow keys navigate galleries

### Color Contrast
- All text meets WCAG AA standards
- Focus indicators visible on all interactive elements
- High contrast mode support via CSS custom properties

---

## Browser Support

### Target Browsers
- Chrome/Edge (latest 2 versions)
- Firefox (latest 2 versions)
- Safari (latest 2 versions)
- iOS Safari (latest 2 versions)
- Android Chrome (latest 2 versions)

### Progressive Enhancement
- CSS Grid with fallback to Flexbox
- CSS Custom Properties with fallback values
- Modern CSS features (aspect-ratio) with fallbacks
- Touch events with mouse event fallbacks

---

## Testing Recommendations

### Visual Regression
- Homepage hero and collections
- Catalog grid layouts at all breakpoints
- Product detail page layouts
- Navigation menu (desktop and mobile)
- Modal overlays (quick-view, lightbox)

### Functional Testing
- Search and filter functionality
- Pagination
- Image gallery navigation
- Quick-view modal interactions
- Mobile menu toggle
- Add to cart actions

### Performance Testing
- Lighthouse audit (target: 90+ performance score)
- Image loading optimization
- CSS bundle size
- JavaScript bundle size
- Time to Interactive (TTI)

### Accessibility Testing
- Keyboard navigation flow
- Screen reader compatibility (NVDA, JAWS, VoiceOver)
- Color contrast validation
- Focus indicator visibility
- ARIA attribute correctness

---

## Maintenance Notes

### CSS Architecture
- Design tokens in `backcountry-theme.css`
- Component-specific styles in `.razor.css` files (scoped)
- Utility classes in `site.css` (global)
- Avoid inline styles (use CSS classes)

### Component Structure
- Presentational components in `Shared/` and `Catalog/`
- Page components in `Pages/`
- Layout components in `Layout/`
- Keep components focused and composable

### Future Enhancements
- Wishlist/favorites functionality
- Product comparison feature
- User reviews and ratings
- Advanced filtering (price range, rating, etc.)
- Product variants (sizes, colors)
- Enhanced search (autocomplete, suggestions)
- Shopping cart improvements
- Checkout flow redesign

