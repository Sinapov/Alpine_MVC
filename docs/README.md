# Detailed Page Implementation Guide for Alpine Needs E-commerce Website

## 1. Public Pages (Guest Users)

### 1.1 Home Page (`/Index.cshtml`)

**Purpose:** To welcome users and showcase featured products.

**Key Components:**
- **Hero Section**
  - Full-width banner image (1920x500px) showing mountain/outdoor scenery
  - Overlaid heading: "Adventure Awaits at Alpine Needs"
  - Call-to-action button: "Shop Now" → links to Products page

- **Introduction Section**
  - Brief 2-3 paragraph store description
  - "Our Story" with image of store/owners
  - Mission statement highlighting commitment to quality outdoor gear

- **Featured Products Section**
  - Bootstrap carousel showing 4-6 featured products
  - Each slide contains:
    - Product image
    - Name
    - Price
    - "View Details" button
  - Auto-rotation every 5 seconds with manual navigation arrows

- **Categories Preview**
  - Grid of clickable category cards (Camping, Hiking, Climbing, etc.)
  - Each card has representative image and category name
  - Links to filtered Products page

- **Testimonials Section**
  - 2-3 customer quotes with star ratings
  - Customer name and small avatar image

**Implementation Notes:**
- Use `OnGet()` method to retrieve featured products from database
- Implement partial view (`_ProductCard.cshtml`) for reusable product displays
- Set meta description for SEO

### 1.2 Products Page (`/Products/Index.cshtml`)

**Purpose:** To display and filter the store's product catalog.

**Key Components:**
- **Filter Sidebar** (Left, collapsible on mobile)

  - **Category Filter**:
    - Checkboxes for each product category
    - Shows count of products in each category
    - Auto-submits on selection change

  - **Price Range Filter**:
    - Dual-handle slider (using noUiSlider or similar)
    - Min/max price inputs that update with slider
    - "Apply" button to trigger filter
  
  - **Stock Filter**:
    - Radio buttons: "All", "In Stock Only"
    - Auto-submits on selection change
  
  - **Other Filters**:
    - Brand selection dropdown
    - "Reset All Filters" button

- **Products Display** (Right)
  - **Search & Sort Controls**:
    - Search input field with "Search" button
    - Sort dropdown (Price: Low-High, Price: High-Low, Newest)
  
  - **Products Grid**:
    - Responsive grid (4 products per row on desktop, 2 on tablet, 1 on mobile)
    - Each product card shows:
      - Image (square aspect ratio)
      - Name (truncated if too long)
      - Price
      - Stock indicator (green "In Stock" or red "Out of Stock")
      - Star rating (if applicable)
      - "Add to Cart" button (disabled if out of stock)
      - "View Details" link
  
  - **Pagination**:
    - Show 24 products per page
    - Previous/Next buttons
    - Page number indicators
    - "Showing X-Y of Z products" text

**Implementation Notes:**
- Use query string parameters to maintain filter state
- Implement AJAX for filter updates to avoid full page reloads
- Include handler method for "Add to Cart" functionality
- Use model binding for filter parameters

### 1.3 Product Details Page (`/Products/Details.cshtml`)

**Purpose:** To show comprehensive information about a specific product.

**Key Components:**
- **Product Image Gallery**
  - Large main image
  - Thumbnails of additional images below (clickable)
  - Zoom functionality on image click

- **Product Information**
  - Product name (large font)
  - SKU/Product code
  - Price (with original price if on sale)
  - Stock status indicator
  - Short description (2-3 sentences) (with possibility to expand collapse)

- **Purchase Section**
  - Quantity selector (number input with +/- buttons)
  - "Add to Cart" button (large, prominent)
  - "Add to Wishlist" button (for registered users)

- **Detailed Information Tabs**
  - **Description Tab**: Comprehensive HTML-formatted description
  - **Specifications Tab**: Technical details in table format
  - **Shipping Tab**: Delivery information

- **Related Products Section**
  - Horizontal scrollable row of 4-8 related products
  - Based on same category or frequently bought together

**Implementation Notes:**
- Use `OnGetAsync(int id)` method to retrieve product by ID
- Implement image gallery using lightweight JavaScript library
- Load related products based on category matching algorithm

### 1.4 Contact Page (`/Contact.cshtml`)

**Purpose:** To provide customers with ways to contact the store.

**Key Components:**
- **Contact Form**
  - Name field (required)
  - Email field (required, with validation)
  - Subject dropdown (General Inquiry, Order Question, Product Question, etc.)
  - Message textarea (required)
  - Anti-spam CAPTCHA
  - "Submit" button with loading state
  - Success message displayed after submission

- **Store Information**
  - Physical address
  - Phone number
  - Email address
  - Business hours table

- **Google Map**
  - Embedded map showing store location
  - Custom marker with store logo
  - 450px height, responsive width

- **FAQ Accordion**
  - Common questions and answers
  - Expandable/collapsible sections

**Implementation Notes:**
- Implement form validation (client and server-side)
- Add email sending functionality with error handling
- Store contact form submissions in database
- Set up Google Maps API key for the embedded map

### 1.5 Authentication Pages

#### 1.5.1 Login Page (`/Account/Login.cshtml`)

**Purpose:** To authenticate existing users.

**Key Components:**
- **Login Form**
  - Email input field (required, with validation)
  - Password field (required)
  - "Remember me" checkbox
  - "Login" button (primary color)
  - "Forgot Password?" link
  - "Register" link for new users
  - Optional social login buttons (Google, Facebook)

- **Error Handling**
  - Invalid credentials message
  - Account locked message (if applicable)
  - Email verification reminder (if applicable)

**Implementation Notes:**
- Use ASP.NET Core Identity for authentication
- Implement CSRF protection
- Add rate limiting for failed login attempts
- Redirect to requested page after successful login

#### 1.5.2 Registration Page (`/Account/Register.cshtml`)

**Purpose:** To create new user accounts.

**Key Components:**
- **Registration Form**
  - First name field (required)
  - Last name field (required)
  - Email field (required, with validation)
  - Password field (required)
    - Password strength meter
    - Requirements listed (min. 8 chars, uppercase, number, special char)
  - Confirm password field
  - Terms and conditions checkbox (required)
  - Marketing opt-in checkbox (optional)
  - "Register" button
  - "Already have an account? Log in" link

- **Validation Messages**
  - Email already in use
  - Password doesn't meet requirements
  - Passwords don't match

**Implementation Notes:**
- Implement client-side validation using jQuery Validation
- Send confirmation email with account verification link
- Hash passwords using ASP.NET Core Identity
- Show success message after registration

#### 1.5.3 Forgot Password Page (`/Account/ForgotPassword.cshtml`)

**Purpose:** To allow users to reset their password.

**Key Components:**
- **Email Input Form**
  - Email address field
  - "Send Reset Link" button
  - "Back to Login" link

- **Success Message**
  - Confirmation of email sent
  - Instructions to check email
  - Resend button (time-limited)

**Implementation Notes:**
- Generate secure time-limited token for password reset
- Send email with password reset link
- Implement rate limiting to prevent abuse

#### 1.5.4 Reset Password Page (`/Account/ResetPassword.cshtml`)

**Purpose:** To set a new password after receiving reset link.

**Key Components:**
- **Reset Form**
  - New password field
  - Confirm new password field
  - Password strength meter
  - "Reset Password" button

- **Success Message**
  - Confirmation of password change
  - "Login with new password" button

**Implementation Notes:**
- Validate reset token before allowing password change
- Apply same password complexity rules as registration

## 2. Registered User Pages

### 2.1 Shopping Cart (`/Cart/Index.cshtml`)

**Purpose:** To display and manage items added to the user's cart.

**Key Components:**
- **Cart Items Table**
  - Product image (thumbnail)
  - Product name (linked to product details)
  - Price per unit
  - Quantity selector (with update button)
  - Subtotal for item
  - "Remove" button for each item

- **Cart Summary**
  - Subtotal
  - Estimated shipping cost
  - Tax (if applicable)
  - Total price
  - "Continue Shopping" button
  - "Proceed to Checkout" button (disabled if cart empty)

- **Empty Cart State**
  - "Your cart is empty" message
  - Suggested products
  - "Browse Products" button

**Implementation Notes:**
- Store cart in session for guests, database for logged-in users
- Implement quantity validation against inventory
- Add AJAX updates for quantity changes and item removal
- Calculate shipping based on user location (if available)

### 2.2 Checkout Process

#### 2.2.1 Checkout Information (`/Checkout/Information.cshtml`)

**Purpose:** To collect shipping and billing information.

**Key Components:**
- **Shipping Address Form**
  - Full name
  - Address line 1
  - Address line 2 (optional)
  - City
  - State/Province dropdown
  - ZIP/Postal code
  - Country dropdown
  - Phone number
  - Address type (Home/Work)
  - "Save address for future orders" checkbox

- **Billing Address Section**
  - "Same as shipping" checkbox
  - Billing address form (shown if checkbox unchecked)

- **Order Summary Sidebar**
  - Collapsed list of items (expandable)
  - Subtotal, shipping, tax, total
  - "Return to Cart" link
  - "Continue to Payment" button

**Implementation Notes:**
- Pre-fill form with saved address if available
- Implement address validation
- Save information in session during checkout process
- Add input masking for phone and ZIP fields

#### 2.2.2 Checkout Payment (`/Checkout/Payment.cshtml`)

**Purpose:** To collect payment information.

**Key Components:**
- **Payment Methods**
  - Credit/Debit card form
    - Card number input (with card type detection)
    - Expiration date (month/year)
    - CVV code with info tooltip
    - Name on card
  - PayPal option
  - Other payment methods (optional)

- **Billing Summary**
  - Order items summary
  - All costs breakdown
  - Final total
  - "Return to Information" button
  - "Place Order" button

- **Terms & Conditions**
  - Checkbox to accept terms
  - Links to privacy policy and return policy

**Implementation Notes:**
- Integrate with payment gateway API
- Implement PCI-compliant card handling
- Add order confirmation before final submission
- Display loading indicator during payment processing

#### 2.2.3 Order Confirmation (`/Checkout/Confirmation.cshtml`)

**Purpose:** To confirm successful order placement.

**Key Components:**
- **Success Message**
  - Order number
  - Estimated delivery date
  - "Track Order" button

- **Order Details**
  - Items purchased
  - Shipping address
  - Payment method (masked)
  - Order total

- **Next Steps Section**
  - "Create Account" prompt (for guest checkout)
  - "Continue Shopping" button
  - "View Order Details" button (for registered users)
  - Email confirmation notice

**Implementation Notes:**
- Send order confirmation email
- Create order record in database
- Update inventory quantities
- Set up redirect prevention on page refresh

### 2.3 User Dashboard

#### 2.3.1 User Profile (`/Account/Profile.cshtml`)

**Purpose:** To display and edit account information.

**Key Components:**
- **Personal Information Section**
  - Name
  - Email
  - Phone number
  - "Edit" button that reveals form

- **Address Book**
  - List of saved addresses
  - Default shipping/billing indicators
  - "Add New Address" button
  - Edit/Delete buttons for each address

- **Account Settings**
  - Change password option
  - Email preferences
  - Delete account option

**Implementation Notes:**
- Add form validation for profile updates
- Require current password for sensitive changes
- Implement soft delete for accounts

#### 2.3.2 Order History (`/Account/Orders.cshtml`)

**Purpose:** To show the user's past orders.

**Key Components:**
- **Orders Table**
  - Order number (linked to details)
  - Date
  - Status (Pending, Processing, Shipped, Delivered)
  - Total amount
  - Payment method

- **Filters**
  - Status filter dropdown
  - Date range selector

- **Pagination**
  - 10 orders per page
  - Next/Previous buttons

**Implementation Notes:**
- Sort orders by date (newest first)
- Add search functionality by order number
- Implement lazy loading for better performance

#### 2.3.3 Order Details (`/Account/Orders/Details.cshtml`)

**Purpose:** To show detailed information about a specific order.

**Key Components:**
- **Order Information**
  - Order number and date
  - Status with visual indicator
  - Shipping address
  - Payment method
  - Tracking number (if available)

- **Items List**
  - Product images and names
  - Quantity
  - Price per item
  - Item subtotals

- **Order Total Breakdown**
  - Subtotal
  - Shipping cost
  - Tax
  - Discounts applied
  - Final total

- **Actions**
  - "Reorder" button (adds all items to cart)
  - "Contact Support" link
  - "Return to Orders" link

**Implementation Notes:**
- Check for product availability before enabling "Reorder"
- Display tracking information if available
- Show order timeline with status history

## 3. Administrator Pages

### 3.1 Admin Dashboard (`/Admin/Dashboard.cshtml`)

**Purpose:** To provide an overview of store performance and activities.

**Key Components:**
- **Statistics Cards**
  - Total sales (daily, weekly, monthly)
  - New users
  - Average order value
  - Conversion rate

- **Sales Chart**
  - Line chart showing sales over time
  - Options to change time period (week, month, year)
  - Comparison with previous period

- **Recent Orders Table**
  - Last 5-10 orders with basic information
  - Status indicators
  - "View All" link to Order Management

- **Inventory Alerts**
  - Low stock products list
  - Out of stock products list

- **Top Selling Products**
  - Ranked list with sales numbers
  - Previous period comparison

**Implementation Notes:**
- Use Chart.js or similar for visualizations
- Add date range selectors for statistics
- Implement data caching to improve dashboard loading speed
- Use background service to calculate statistics periodically

### 3.2 Product Management

#### 3.2.1 Product List (`/Admin/Products/Index.cshtml`)

**Purpose:** To display and manage all products.

**Key Components:**
- **Filter Controls**
  - Search box
  - Category dropdown
  - Stock status filter
  - Date added filter

- **Products Table**
  - Product image thumbnail
  - Name
  - SKU
  - Price
  - Stock quantity
  - Status (Active/Inactive)
  - Category
  - Actions column (Edit, Delete, View)

- **Batch Actions**
  - Checkbox for each product
  - Dropdown for batch actions (Delete, Change Category, etc.)
  - "Apply" button

- **Add New Product Button**
  - Prominent button at top of page

**Implementation Notes:**
- Implement server-side pagination for performance
- Add sorting functionality for all columns
- Use soft delete for products
- Add export functionality (CSV, Excel)

#### 3.2.2 Add/Edit Product (`/Admin/Products/Edit.cshtml`)

**Purpose:** To create new products or edit existing ones.

**Key Components:**
- **Basic Information Section**
  - Product name
  - SKU/Product code
  - Short description
  - Full description (rich text editor)
  - Status toggle (Active/Inactive)

- **Pricing Section**
  - Regular price
  - Sale price (optional)
  - Sale start/end dates

- **Inventory Section**
  - Quantity in stock
  - Low stock threshold
  - Backorder allowed toggle

- **Categorization**
  - Category dropdown (multi-select)
  - Tags input
  - Brand dropdown

- **Images Section**
  - Main image upload
  - Additional images upload (drag-and-drop area)
  - Image reordering functionality

- **Specifications Section**
  - Add key-value pairs for specifications
  - Add/remove buttons for multiple specifications

- **SEO Section**
  - Meta title
  - Meta description
  - URL slug

- **Action Buttons**
  - "Save" button
  - "Save and Add Another" button
  - "Cancel" button

**Implementation Notes:**
- Implement image resizing and optimization
- Use AJAX for form submission to prevent data loss
- Add autosave functionality
- Implement validation for all fields
- Add preview functionality

### 3.3 Order Management

#### 3.3.1 Orders List (`/Admin/Orders/Index.cshtml`)

**Purpose:** To display and manage all customer orders.

**Key Components:**
- **Filter Panel**
  - Date range picker
  - Status dropdown (multiple select)
  - Customer name/email search
  - Order number search
  - Payment method filter
  - Price range filter

- **Orders Table**
  - Order number
  - Date and time
  - Customer name (linked to customer details)
  - Number of items
  - Total amount
  - Payment status
  - Fulfillment status
  - Actions column (View, Edit, Cancel)

- **Batch Processing**
  - Checkbox selection for orders
  - Status update dropdown
  - "Apply" button
  - Batch invoice/packing slip generation

**Implementation Notes:**
- Implement color-coding for different order statuses
- Add export functionality for orders
- Implement detailed filters with saved preferences
- Add sorting for all columns

#### 3.3.2 Order Details (`/Admin/Orders/Details.cshtml`)

**Purpose:** To view and manage a specific order.

**Key Components:**
- **Order Summary**
  - Order number and date
  - Customer information with link to profile
  - Payment status and method
  - Fulfillment status with timeline

- **Status Management**
  - Status dropdown with update button
  - Add note functionality
  - Status history log

- **Items Section**
  - Products list with images
  - Quantities and prices
  - Edit quantities functionality
  - Add/remove items capability

- **Customer Information**
  - Billing address
  - Shipping address
  - Contact information

- **Payment Details**
  - Payment method
  - Transaction ID
  - Payment date
  - Refund button (if applicable)

- **Actions**
  - "Print Invoice" button
  - "Print Packing Slip" button
  - "Send Order Update" button
  - "Cancel Order" button

**Implementation Notes:**
- Implement order modification tracking
- Add email notifications for status changes
- Create PDF generation for invoices and packing slips
- Add inventory update on order status changes

### 3.4 User Management

#### 3.4.1 Users List (`/Admin/Users/Index.cshtml`)

**Purpose:** To display and manage user accounts.

**Key Components:**
- **Search and Filter**
  - Name/email search
  - Role filter
  - Registration date range
  - Account status filter

- **Users Table**
  - Name
  - Email
  - Registration date
  - Last login date
  - Role
  - Status (Active/Inactive)
  - Actions (Edit, Deactivate, Delete)

- **Add New User Button**
  - Opens user creation form

**Implementation Notes:**
- Implement pagination and sorting
- Add user impersonation functionality
- Create export functionality
- Add bulk action capabilities

#### 3.4.2 Edit User (`/Admin/Users/Edit.cshtml`)

**Purpose:** To edit user information and permissions.

**Key Components:**
- **Personal Information**
  - Name
  - Email
  - Phone
  - Registration date (non-editable)
  - Avatar upload

- **Account Settings**
  - Status toggle (Active/Inactive)
  - Role selection
  - Password reset button
  - Force password change toggle

- **Orders Section**
  - Recent orders summary
  - Link to full order history

- **Activity Log**
  - Login history
  - Significant actions

- **Notes Section**
  - Admin notes about the user
  - Add note functionality

**Implementation Notes:**
- Log all changes to user settings
- Add validation for critical field changes
- Implement proper permission checks
- Create notification system for account changes

## Technical Implementation Considerations

### Database Structure
- **Products Table**: id, name, description, price, sale_price, stock_quantity, category_id, brand_id, created_at, updated_at, is_active
- **Categories Table**: id, name, description, parent_id
- **Orders Table**: id, user_id, status, total, payment_method, payment_status, shipping_address_id, billing_address_id, created_at, updated_at
- **OrderItems Table**: id, order_id, product_id, quantity, price_at_time
- **Users Table**: id, first_name, last_name, email, password_hash, role, created_at, last_login
- **Addresses Table**: id, user_id, type, address_line1, address_line2, city, state, postal_code, country, is_default
- **Reviews Table**: id, product_id, user_id, rating, comment, created_at

### Security Implementation
- Use ASP.NET Core Identity for authentication and authorization
- Implement HTTPS with proper certificate
- Apply role-based access control (RBAC)
- Add input validation on all forms
- Implement CSRF protection on all POST actions
- Use parameterized queries to prevent SQL injection
- Implement rate limiting for sensitive operations

### Performance Optimizations
- Use caching for product listings and category trees
- Implement lazy loading for images
- Use pagination for large data sets
- Implement CDN for static assets
- Add database indexing for common queries
- Use output caching for public pages

This detailed guide should provide a comprehensive roadmap for implementing all the pages and functionality required for the Alpine Needs e-commerce website.