// Cart operations
$(function() {
    // Add to Cart form submission via AJAX
    $('.add-to-cart-form').on('submit', function(e) {
        e.preventDefault();
        
        const $form = $(this);
        const formData = $form.serialize();
        
        // Show loading state
        const $submitBtn = $form.find('button[type="submit"]');
        const originalBtnText = $submitBtn.html();
        $submitBtn.html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Adding...');
        $submitBtn.prop('disabled', true);
        
        $.ajax({
            url: '/Cart?handler=AddToCart',
            type: 'POST',
            data: formData,
            headers: {
                'X-Requested-With': 'XMLHttpRequest',
                'RequestVerificationToken': $('input:hidden[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                if (response.success) {
                    // Update cart count in navbar
                    updateCartCount(response.count);
                    
                    // Show success modal
                    showCartModal('Product added to cart successfully!');
                } else {
                    // Show error message
                    showErrorMessage(response.message || 'Failed to add product to cart');
                }
            },
            error: function() {
                showErrorMessage('An error occurred. Please try again.');
            },
            complete: function() {
                // Restore button state
                $submitBtn.html(originalBtnText);
                $submitBtn.prop('disabled', false);
            }
        });
    });
    
    // Function to update cart count in navbar
    function updateCartCount(count) {
        const $cartCount = $('.cart-count');
        $cartCount.text(count > 0 ? count : '');
        
        // Optional: animate the cart icon to draw attention
        $('.bi-cart').parent().addClass('cart-updated');
        setTimeout(function() {
            $('.bi-cart').parent().removeClass('cart-updated');
        }, 1000);
    }
    
    // Function to show cart success modal
    function showCartModal(message) {
        // Create modal if it doesn't exist
        if ($('#cartModal').length === 0) {
            const modalHtml = `
                <div class="modal fade" id="cartModal" tabindex="-1" aria-hidden="true">
                    <div class="modal-dialog">
                        <div class="modal-content">
                            <div class="modal-header bg-success text-white">
                                <h5 class="modal-title">Success</h5>
                                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                            </div>
                            <div class="modal-body">
                                <p class="cart-modal-message"></p>
                            </div>
                            <div class="modal-footer">
                                <a href="/Cart" class="btn btn-primary">View Cart</a>
                                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Continue Shopping</button>
                            </div>
                        </div>
                    </div>
                </div>
            `;
            $('body').append(modalHtml);
        }
        
        // Set message and show modal
        $('.cart-modal-message').text(message);
        const cartModal = new bootstrap.Modal(document.getElementById('cartModal'));
        cartModal.show();
    }
    
    // Function to show error message
    function showErrorMessage(message) {
        // You can use a toast, alert, or any other notification method
        alert(message);
    }
    
    // Load cart count on page load
    $.get('/Cart?handler=CartCount', function(count) {
        updateCartCount(count);
    });
});