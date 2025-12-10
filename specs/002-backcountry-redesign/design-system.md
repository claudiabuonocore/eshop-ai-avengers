# Design System: Backcountry-Inspired Theme

**Project**: eShop Backcountry Redesign  
**Created**: 2025-12-09  
**Purpose**: Define visual design system based on backcountry.com aesthetic

## Color Palette

### Primary Colors (from backcountry.com)

**Greens** (Outdoor/Adventure):
- `--bc-green-dark`: `#2D5016` - Primary brand green, headers, CTAs
- `--bc-green-primary`: `#4A7729` - Interactive elements, links
- `--bc-green-light`: `#6B9447` - Hover states, accents
- `--bc-green-pale`: `#E8F3E0` - Backgrounds, subtle highlights

**Earth Tones** (Natural/Organic):
- `--bc-brown-dark`: `#3E2723` - Rich text, footers
- `--bc-brown-primary`: `#5D4037` - Secondary text, borders
- `--bc-tan`: `#A1887F` - Subtle accents, dividers
- `--bc-sand`: `#EFEBE9` - Light backgrounds, cards

**Blacks & Grays** (Modern/Clean):
- `--bc-black`: `#1A1A1A` - Primary text, strong emphasis
- `--bc-gray-dark`: `#424242` - Secondary text
- `--bc-gray-mid`: `#757575` - Tertiary text, placeholders
- `--bc-gray-light`: `#E0E0E0` - Borders, dividers
- `--bc-gray-pale`: `#F5F5F5` - Light backgrounds, alternate rows

### Accent Colors

**Call-to-Action**:
- `--bc-cta-primary`: `#4A7729` (green-primary)
- `--bc-cta-hover`: `#2D5016` (green-dark)
- `--bc-cta-active`: `#6B9447` (green-light)

**Semantic Colors**:
- `--bc-success`: `#4CAF50` - Success messages, confirmations
- `--bc-error`: `#D32F2F` - Errors, warnings
- `--bc-info`: `#1976D2` - Informational messages
- `--bc-warning`: `#F57C00` - Warning states

## Typography

### Font Families

**Primary Font Stack** (Body text):
```css
font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', 'Helvetica Neue', Arial, sans-serif;
```

**Display Font Stack** (Headings, hero text):
```css
font-family: 'Montserrat', 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
```

**Monospace** (Code, technical content):
```css
font-family: 'Fira Code', 'Courier New', Courier, monospace;
```

### Font Sizes

**Desktop Scale** (16px base):
- `--font-xs`: 0.75rem (12px) - Small labels, captions
- `--font-sm`: 0.875rem (14px) - Secondary text, metadata
- `--font-base`: 1rem (16px) - Body text
- `--font-lg`: 1.125rem (18px) - Large body text, subheadings
- `--font-xl`: 1.25rem (20px) - Card titles, section headers
- `--font-2xl`: 1.5rem (24px) - Page subheadings
- `--font-3xl`: 1.875rem (30px) - Page titles
- `--font-4xl`: 2.25rem (36px) - Hero headings
- `--font-5xl`: 3rem (48px) - Large hero text
- `--font-6xl`: 3.75rem (60px) - Extra large displays

**Mobile Scale** (Slightly reduced for smaller screens):
- Hero headings: Reduce by 25% on mobile (< 768px)
- Large headings: Reduce by 15% on mobile
- Body text: Maintain 16px minimum for readability

### Font Weights

- `--weight-light`: 300
- `--weight-normal`: 400
- `--weight-medium`: 500
- `--weight-semibold`: 600
- `--weight-bold`: 700
- `--weight-extrabold`: 800

### Line Heights

- `--leading-tight`: 1.25 - Headings, CTAs
- `--leading-snug`: 1.375 - Subheadings
- `--leading-normal`: 1.5 - Body text
- `--leading-relaxed`: 1.625 - Large body text
- `--leading-loose`: 2 - Spacious content

### Letter Spacing

- `--tracking-tighter`: -0.05em - Large display text
- `--tracking-tight`: -0.025em - Headings
- `--tracking-normal`: 0 - Body text
- `--tracking-wide`: 0.025em - Uppercase labels
- `--tracking-wider`: 0.05em - Buttons, CTAs

## Spacing & Grid System

### Base Grid

**4px Grid System**: All spacing uses multiples of 4px for consistency

### Spacing Scale

- `--space-1`: 0.25rem (4px)
- `--space-2`: 0.5rem (8px)
- `--space-3`: 0.75rem (12px)
- `--space-4`: 1rem (16px)
- `--space-5`: 1.25rem (20px)
- `--space-6`: 1.5rem (24px)
- `--space-8`: 2rem (32px)
- `--space-10`: 2.5rem (40px)
- `--space-12`: 3rem (48px)
- `--space-16`: 4rem (64px)
- `--space-20`: 5rem (80px)
- `--space-24`: 6rem (96px)
- `--space-32`: 8rem (128px)

### Layout Spacing

**Container Padding**:
- Desktop (> 1024px): 3rem (48px)
- Tablet (768px-1024px): 2rem (32px)
- Mobile (< 768px): 1rem (16px)

**Component Spacing**:
- Card padding: var(--space-6) (24px)
- Button padding: var(--space-4) var(--space-6) (16px 24px)
- Input padding: var(--space-3) var(--space-4) (12px 16px)
- Section margins: var(--space-16) (64px) desktop, var(--space-12) (48px) mobile

## Responsive Breakpoints

### Breakpoint Definitions

```css
/* Mobile First Approach */
--breakpoint-sm: 640px;   /* Small tablets */
--breakpoint-md: 768px;   /* Tablets */
--breakpoint-lg: 1024px;  /* Desktops */
--breakpoint-xl: 1280px;  /* Large desktops */
--breakpoint-2xl: 1536px; /* Extra large displays */
```

### Usage Guidelines

**Mobile (320px - 767px)**:
- Single column layouts
- Full-width components
- Touch-optimized (44x44px minimum touch targets)
- Simplified navigation (hamburger menu)
- Stacked product cards (1 column)

**Tablet (768px - 1024px)**:
- 2-column product grids
- Collapsible side navigation
- Larger touch targets maintained
- Optimized image sizes

**Desktop (1025px+)**:
- 3-4 column product grids
- Full navigation menu
- Hover states and interactions
- Maximum content width: 1440px (centered)

## Component States

### Interactive States

**Default**:
- Clear visual hierarchy
- Sufficient contrast (WCAG AA minimum 4.5:1)

**Hover**:
- Background color shift (10% darker)
- Subtle scale transform (1.02-1.05)
- Border color change
- Transition: 200ms ease-in-out

**Active/Pressed**:
- Background color shift (15% darker)
- Slight scale down (0.98)
- Border color darkens

**Focus**:
- Visible outline (2px solid accent color)
- Offset: 2px for clarity
- Maintain for keyboard accessibility

**Disabled**:
- Opacity: 0.5
- Cursor: not-allowed
- Remove interactive states

**Loading**:
- Skeleton screens (shimmer animation)
- Spinner for actions (button loading states)
- Progressive image loading

## Elevation & Shadows

### Shadow System

```css
--shadow-sm: 0 1px 2px 0 rgba(0, 0, 0, 0.05);
--shadow-base: 0 1px 3px 0 rgba(0, 0, 0, 0.1), 0 1px 2px 0 rgba(0, 0, 0, 0.06);
--shadow-md: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06);
--shadow-lg: 0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05);
--shadow-xl: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04);
--shadow-2xl: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
```

**Usage**:
- Cards: `--shadow-base`
- Hover cards: `--shadow-md`
- Modals/Overlays: `--shadow-xl`
- Dropdown menus: `--shadow-lg`

## Border Radius

```css
--radius-sm: 0.25rem (4px)   /* Small elements, tags */
--radius-base: 0.375rem (6px) /* Buttons, inputs */
--radius-md: 0.5rem (8px)     /* Cards */
--radius-lg: 0.75rem (12px)   /* Large cards, modals */
--radius-xl: 1rem (16px)      /* Hero sections */
--radius-full: 9999px         /* Pills, circular elements */
```

## Animation & Transitions

### Timing Functions

```css
--ease-linear: linear;
--ease-in: cubic-bezier(0.4, 0, 1, 1);
--ease-out: cubic-bezier(0, 0, 0.2, 1);
--ease-in-out: cubic-bezier(0.4, 0, 0.2, 1);
```

### Duration

- **Fast**: 150ms - Small UI changes, hover feedback
- **Base**: 200ms - Most transitions, color changes
- **Slow**: 300ms - Large movements, complex animations
- **Slower**: 500ms - Page transitions, modals

### Common Patterns

```css
/* Smooth hover */
transition: all 200ms var(--ease-out);

/* Scale on hover */
transform: scale(1.05);
transition: transform 200ms var(--ease-in-out);

/* Fade in */
opacity: 0;
animation: fadeIn 300ms var(--ease-out) forwards;
```

## Accessibility Standards

### WCAG 2.1 AA Compliance

**Color Contrast**:
- Normal text: 4.5:1 minimum
- Large text (18pt+): 3:1 minimum
- UI components: 3:1 minimum

**Touch Targets**:
- Minimum: 44x44px
- Recommended: 48x48px for primary actions

**Keyboard Navigation**:
- All interactive elements must be keyboard accessible
- Visible focus indicators required
- Logical tab order maintained

**Screen Reader Support**:
- Semantic HTML structure
- ARIA labels for complex components
- Alt text for all images

## Implementation Notes

1. **CSS Custom Properties**: All design tokens should be defined as CSS custom properties in `backcountry-theme.css`

2. **Dark Mode**: Consider dark mode variations in future phase (out of scope for initial implementation)

3. **Component Library**: Reusable Blazor components should reference these design tokens consistently

4. **Performance**: Optimize fonts with `font-display: swap` for better loading experience

5. **Browser Support**: Target modern browsers (last 2 versions of Chrome, Firefox, Safari, Edge)

---

**Version**: 1.0.0  
**Last Updated**: 2025-12-09

