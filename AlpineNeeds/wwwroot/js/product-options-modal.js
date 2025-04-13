/**
 * Alpine Needs Product Options Modal
 * 
 * This script handles the modal dialog for selecting product options (color, size)
 * when adding products to cart from the products listing page.
 */

// Store product data fetched from server
const productData = {};

document.addEventListener('DOMContentLoaded', function() {
    // Create modal element if it doesn't exist
    createProductOptionsModal();
    
    // Initialize event listeners
    initializeEventListeners();
});

function createProductOptionsModal() {
    // Check if modal already exists
    if (document.getElementById('productOptionsModal')) return;
    
    // Create modal HTML
    const modalHtml = `
        <div class="modal fade" id="productOptionsModal" tabindex="-1" aria-labelledby="productOptionsModalLabel" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="productOptionsModalLabel">Select Product Options</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        <form id="productOptionsForm">
                            <input type="hidden" id="modalProductId" name="productId">
                            
                            <div id="colorOptions" class="mb-3" style="display: none;">
                                <label for="modalColorSelect" class="form-label">Color</label>
                                <select id="modalColorSelect" name="color" class="form-select">
                                    <option value="">Select Color</option>
                                </select>
                            </div>
                            
                            <div id="sizeOptions" class="mb-3" style="display: none;">
                                <label for="modalSizeSelect" class="form-label">Size</label>
                                <select id="modalSizeSelect" name="size" class="form-select">
                                    <option value="">Select Size</option>
                                </select>
                            </div>
                            
                            <div class="mb-3">
                                <label for="modalQuantity" class="form-label">Quantity</label>
                                <div class="input-group">
                                    <button type="button" class="btn btn-outline-secondary modal-quantity-decrease">-</button>
                                    <input type="number" id="modalQuantity" name="quantity" value="1" min="1" class="form-control text-center">
                                    <button type="button" class="btn btn-outline-secondary modal-quantity-increase">+</button>
                                </div>
                            </div>
                        </form>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                        <button type="button" class="btn btn-primary" id="modalAddToCartBtn">Add to Cart</button>
                    </div>
                </div>
            </div>
        </div>
    `;
    
    // Insert modal into body
    document.body.insertAdjacentHTML('beforeend', modalHtml);
}

function initializeEventListeners() {
    // Listen for add to cart button clicks across the page
    document.addEventListener('click', function(event) {
        const addToCartBtn = event.target.closest('.add-to-cart-btn');
        if (addToCartBtn) {
            const productId = addToCartBtn.dataset.productId;
            if (productId) {
                event.preventDefault();
                handleAddToCartClick(productId);
            }
        }
    });
    
    // Initialize event listeners for the modal
    const modal = document.getElementById('productOptionsModal');
    if (modal) {
        // Set up quantity increment/decrement buttons
        const decreaseBtn = modal.querySelector('.modal-quantity-decrease');
        const increaseBtn = modal.querySelector('.modal-quantity-increase');
        const quantityInput = modal.querySelector('#modalQuantity');
        
        decreaseBtn.addEventListener('click', function() {
            const currentValue = parseInt(quantityInput.value);
            if (currentValue > 1) {
                quantityInput.value = currentValue - 1;
            }
        });
        
        increaseBtn.addEventListener('click', function() {
            const currentValue = parseInt(quantityInput.value);
            const maxStock = parseInt(quantityInput.getAttribute('max') || 100);
            if (currentValue < maxStock) {
                quantityInput.value = currentValue + 1;
            }
        });
        
        // Add to cart button in modal
        const modalAddToCartBtn = modal.querySelector('#modalAddToCartBtn');
        modalAddToCartBtn.addEventListener('click', function() {
            submitAddToCartFromModal();
        });
    }
}

function handleAddToCartClick(productId) {
    // First, check if we already have the product data
    if (productData[productId]) {
        const product = productData[productId];
        // Check if the product has color or size options
        if ((product.colors && product.colors.length > 0) || (product.sizes && product.sizes.length > 0)) {
            // Show modal only for products with color or size options
            showProductOptionsModal(productId, product);
        } else {
            // Directly add to cart for products without options
            directAddToCart(productId);
        }
    } else {
        // Fetch product data
        fetchProductData(productId).then(product => {
            if (product) {
                // Store product data for future use
                productData[productId] = product;
                
                // Check if the product has color or size options
                if ((product.colors && product.colors.length > 0) || (product.sizes && product.sizes.length > 0)) {
                    // Show modal only for products with color or size options
                    showProductOptionsModal(productId, product);
                } else {
                    // Directly add to cart for products without options
                    directAddToCart(productId);
                }
            }
        }).catch(error => {
            console.error('Error fetching product data:', error);
            // Fall back to direct form submission if fetch fails
            directAddToCart(productId);
        });
    }
}

function directAddToCart(productId) {
    // Find the form for this product
    const form = document.querySelector(`form.add-to-cart-form input[id="productId-${productId}"]`).closest('form');
    if (form) {
        // Set quantity to 1 (default)
        const quantityInput = form.querySelector('input[name="quantity"]');
        if (quantityInput) {
            quantityInput.value = 1;
        }
        
        // Submit the form
        form.submit();
    }
}

function fetchProductData(productId) {
    return fetch(`/api/products/${productId}`)
        .then(response => response.json())
        .then(data => {
            return data;
        });
}

function showProductOptionsModal(productId, product) {
    const modal = document.getElementById('productOptionsModal');
    if (!modal) return;
    
    // Update modal title
    modal.querySelector('.modal-title').textContent = `Add ${product.name} to Cart`;
    
    // Set product ID
    modal.querySelector('#modalProductId').value = productId;
    
    // Handle color options
    const colorOptions = modal.querySelector('#colorOptions');
    const colorSelect = modal.querySelector('#modalColorSelect');
    
    // Clear previous options
    while (colorSelect.options.length > 1) {
        colorSelect.remove(1);
    }
    
    if (product.colors && product.colors.length > 0) {
        // Add color options
        product.colors.forEach(color => {
            const option = document.createElement('option');
            option.value = color;
            option.textContent = color;
            colorSelect.appendChild(option);
        });
        
        // Show color selection
        colorOptions.style.display = 'block';
        colorSelect.setAttribute('required', 'required');
    } else {
        // Hide color selection
        colorOptions.style.display = 'none';
        colorSelect.removeAttribute('required');
    }
    
    // Handle size options
    const sizeOptions = modal.querySelector('#sizeOptions');
    const sizeSelect = modal.querySelector('#modalSizeSelect');
    
    // Clear previous options
    while (sizeSelect.options.length > 1) {
        sizeSelect.remove(1);
    }
    
    if (product.sizes && product.sizes.length > 0) {
        // Add size options
        product.sizes.forEach(size => {
            const option = document.createElement('option');
            option.value = size;
            option.textContent = size;
            sizeSelect.appendChild(option);
        });
        
        // Show size selection
        sizeOptions.style.display = 'block';
        sizeSelect.setAttribute('required', 'required');
    } else {
        // Hide size selection
        sizeOptions.style.display = 'none';
        sizeSelect.removeAttribute('required');
    }
    
    // Set quantity max based on stock
    const quantityInput = modal.querySelector('#modalQuantity');
    quantityInput.max = product.stockQuantity;
    quantityInput.value = 1;
    
    // Show modal
    const modalInstance = new bootstrap.Modal(modal);
    modalInstance.show();
}

function submitAddToCartFromModal() {
    const modal = document.getElementById('productOptionsModal');
    if (!modal) return;
    
    const productId = modal.querySelector('#modalProductId').value;
    const quantity = modal.querySelector('#modalQuantity').value;
    
    const colorSelect = modal.querySelector('#modalColorSelect');
    const color = colorSelect.style.display !== 'none' && colorSelect.value ? colorSelect.value : null;
    
    const sizeSelect = modal.querySelector('#modalSizeSelect');
    const size = sizeSelect.style.display !== 'none' && sizeSelect.value ? sizeSelect.value : null;
    
    // Validate required fields
    if ((colorSelect.hasAttribute('required') && !colorSelect.value) || 
        (sizeSelect.hasAttribute('required') && !sizeSelect.value)) {
        alert('Please select all required options');
        return;
    }
    
    // Find the original form
    const form = document.querySelector(`form.add-to-cart-form input[id="productId-${productId}"]`).closest('form');
    if (!form) return;
    
    // Add values to the form
    let colorInput = form.querySelector('input[name="color"]');
    if (!colorInput && color) {
        colorInput = document.createElement('input');
        colorInput.type = 'hidden';
        colorInput.name = 'color';
        form.appendChild(colorInput);
    }
    if (colorInput && color) {
        colorInput.value = color;
    }
    
    let sizeInput = form.querySelector('input[name="size"]');
    if (!sizeInput && size) {
        sizeInput = document.createElement('input');
        sizeInput.type = 'hidden';
        sizeInput.name = 'size';
        form.appendChild(sizeInput);
    }
    if (sizeInput && size) {
        sizeInput.value = size;
    }
    
    // Update quantity
    const quantityInput = form.querySelector('input[name="quantity"]');
    if (quantityInput) {
        quantityInput.value = quantity;
    }
    
    // Submit the form
    form.submit();
    
    // Hide the modal
    const modalInstance = bootstrap.Modal.getInstance(modal);
    modalInstance.hide();
}