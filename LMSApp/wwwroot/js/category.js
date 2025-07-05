/**
 * Category Management JavaScript
 * Handles auto-hide alerts and interactive functionality
 */

document.addEventListener('DOMContentLoaded', function() {
    initializeCategoryPage();
});

function initializeCategoryPage() {
    setupAutoHideAlerts();
    setupManualCloseButtons();
    setupFormValidation();
    setupTableInteractions();
    setupAjaxForms();
    setupAjaxDelete();
}

/**
 * Auto-hide alert messages after specified time
 */
function setupAutoHideAlerts() {
    const alerts = document.querySelectorAll('.alert');
    
    alerts.forEach(function(alert) {
        // Add fade-out animation after 3 seconds
        setTimeout(function() {
            fadeOutAlert(alert);
        }, 3000);
    });
}

/**
 * Add manual close buttons to alerts
 */
function setupManualCloseButtons() {
    const alerts = document.querySelectorAll('.alert');
    
    alerts.forEach(function(alert) {
        // Create close button
        const closeButton = createCloseButton();
        
        // Make alert position relative for absolute positioning
        alert.style.position = 'relative';
        alert.appendChild(closeButton);
        
        // Add click event
        closeButton.addEventListener('click', function() {
            fadeOutAlert(alert, 300);
        });
    });
}

/**
 * Create a close button element
 */
function createCloseButton() {
    const closeButton = document.createElement('button');
    closeButton.innerHTML = '<i class="fas fa-times"></i>';
    closeButton.className = 'btn-close position-absolute top-0 end-0 p-3';
    closeButton.style.cssText = 'background: none; border: none; color: inherit; font-size: 1.2rem; cursor: pointer;';
    closeButton.setAttribute('aria-label', 'Close');
    closeButton.setAttribute('title', 'Close this message');
    
    return closeButton;
}

/**
 * Fade out an alert element
 * @param {HTMLElement} alert - The alert element to fade out
 * @param {number} duration - Animation duration in milliseconds (default: 500)
 */
function fadeOutAlert(alert, duration = 500) {
    alert.style.transition = `opacity ${duration}ms ease-out`;
    alert.style.opacity = '0';
    
    // Remove from DOM after animation
    setTimeout(function() {
        if (alert.parentNode) {
            alert.remove();
        }
    }, duration);
}

/**
 * Setup AJAX form submissions for add and edit operations
 */
function setupAjaxForms() {
    // Add category form
    const addForm = document.querySelector('form[asp-action="CreateCategory"]');
    if (addForm) {
        addForm.setAttribute('data-ajax-handled', 'true');
        addForm.addEventListener('submit', function(e) {
            e.preventDefault();
            handleAddCategory(this);
        });
    }

    // Update category forms
    const updateForms = document.querySelectorAll('form[asp-action="UpdateCategory"]');
    updateForms.forEach(function(form) {
        form.setAttribute('data-ajax-handled', 'true');
        form.addEventListener('submit', function(e) {
            e.preventDefault();
            handleUpdateCategory(this);
        });
    });
}

/**
 * Setup AJAX delete functionality
 */
function setupAjaxDelete() {
    const deleteLinks = document.querySelectorAll('.delete-category-btn');
    deleteLinks.forEach(function(link) {
        link.addEventListener('click', function(e) {
            e.preventDefault();
            handleDeleteCategory(this);
        });
    });
}

/**
 * Handle add category via AJAX
 * @param {HTMLFormElement} form - The form element
 */
function handleAddCategory(form) {
    const formData = new FormData(form);
    const submitButton = form.querySelector('button[type="submit"]');
    const originalText = showButtonLoading(submitButton);

    fetch('/Admin/CreateCategory', {
        method: 'POST',
        body: formData,
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
    .then(response => response.json())
    .then(data => {
        resetButton(submitButton, originalText);
        
        if (data.success) {
            showNotification(data.message || 'Category added successfully!', 'success');
            form.reset();
            // Refresh the page to show new category
            setTimeout(() => location.reload(), 1500);
        } else {
            showNotification(data.message || 'Failed to add category.', 'error');
        }
    })
    .catch(error => {
        resetButton(submitButton, originalText);
        showNotification('An error occurred while adding the category.', 'error');
        console.error('Error:', error);
    });
}

/**
 * Handle update category via AJAX
 * @param {HTMLFormElement} form - The form element
 */
function handleUpdateCategory(form) {
    const formData = new FormData(form);
    const submitButton = form.querySelector('button[type="submit"]');
    const originalText = showButtonLoading(submitButton);

    fetch('/Admin/UpdateCategory', {
        method: 'POST',
        body: formData,
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
    .then(response => response.json())
    .then(data => {
        resetButton(submitButton, originalText);
        
        if (data.success) {
            showNotification(data.message || 'Category updated successfully!', 'success');
            // Update the input field to show the new value
            const nameInput = form.querySelector('input[name="Name"]');
            if (nameInput) {
                nameInput.value = data.categoryName || nameInput.value;
            }
        } else {
            showNotification(data.message || 'Failed to update category.', 'error');
        }
    })
    .catch(error => {
        resetButton(submitButton, originalText);
        showNotification('An error occurred while updating the category.', 'error');
        console.error('Error:', error);
    });
}

/**
 * Handle delete category via AJAX
 * @param {HTMLElement} link - The delete link element
 */
function handleDeleteCategory(link) {
    // Extract category ID from href attribute
    const href = link.getAttribute('href');
    const categoryId = href ? href.split('/').pop() : null;
    const categoryName = link.closest('tr').querySelector('input[name="Name"]')?.value || 'this category';
    
    console.log('Delete category:', { href, categoryId, categoryName }); // Debug log
    
    if (!confirm(`Are you sure you want to delete "${categoryName}"? This action cannot be undone.`)) {
        return;
    }

    const deleteButton = link;
    const originalText = deleteButton.innerHTML;
    deleteButton.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Deleting...';
    deleteButton.disabled = true;

    console.log('Sending delete request to:', `/Admin/DeleteCategory/${categoryId}`); // Debug log
    
    fetch(`/Admin/DeleteCategory/${categoryId}`, {
        method: 'POST',
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
    .then(response => {
        console.log('Delete response status:', response.status); // Debug log
        return response.json();
    })
    .then(data => {
        console.log('Delete response data:', data); // Debug log
        if (data.success) {
            showNotification(data.message || 'Category deleted successfully!', 'success');
            // Remove the table row
            const tableRow = link.closest('tr');
            tableRow.style.transition = 'opacity 0.3s ease';
            tableRow.style.opacity = '0';
            setTimeout(() => {
                tableRow.remove();
                // Update category count
                updateCategoryCount();
            }, 300);
        } else {
            resetButton(deleteButton, originalText);
            showNotification(data.message || 'Failed to delete category.', 'error');
        }
    })
    .catch(error => {
        resetButton(deleteButton, originalText);
        showNotification('An error occurred while deleting the category.', 'error');
        console.error('Error:', error);
    });
}

/**
 * Update the category count display
 */
function updateCategoryCount() {
    const statsNumber = document.querySelector('.stats-number');
    if (statsNumber) {
        const currentCount = parseInt(statsNumber.textContent);
        statsNumber.textContent = currentCount - 1;
    }
}

/**
 * Setup form validation for category forms
 */
function setupFormValidation() {
    const forms = document.querySelectorAll('form');
    
    forms.forEach(function(form) {
        // Only add validation if it's not an AJAX form
        if (!form.hasAttribute('data-ajax-handled')) {
            form.addEventListener('submit', function(e) {
                const requiredFields = form.querySelectorAll('[required]');
                let isValid = true;
                
                requiredFields.forEach(function(field) {
                    if (!field.value.trim()) {
                        isValid = false;
                        field.classList.add('is-invalid');
                    } else {
                        field.classList.remove('is-invalid');
                    }
                });
                
                if (!isValid) {
                    e.preventDefault();
                    showValidationMessage('Please fill in all required fields.');
                }
            });
        }
    });
}

/**
 * Setup table row interactions
 */
function setupTableInteractions() {
    const tableRows = document.querySelectorAll('.table tbody tr');
    
    tableRows.forEach(function(row) {
        // Add hover effect
        row.addEventListener('mouseenter', function() {
            this.style.cursor = 'pointer';
        });
        
        // Add click effect for better UX
        row.addEventListener('click', function(e) {
            // Don't trigger if clicking on buttons or inputs
            if (e.target.tagName === 'BUTTON' || e.target.tagName === 'INPUT' || e.target.tagName === 'A') {
                return;
            }
            
            // Add a subtle click effect
            this.style.transform = 'scale(0.98)';
            setTimeout(() => {
                this.style.transform = '';
            }, 150);
        });
    });
}

/**
 * Show a validation message
 * @param {string} message - The message to display
 */
function showValidationMessage(message) {
    // Create a temporary alert
    const alertDiv = document.createElement('div');
    alertDiv.className = 'alert alert-warning';
    alertDiv.innerHTML = `
        <i class="fas fa-exclamation-triangle icon"></i>
        ${message}
    `;
    
    // Insert at the top of the container
    const container = document.querySelector('.container-fluid');
    container.insertBefore(alertDiv, container.firstChild);
    
    // Auto-hide after 5 seconds
    setTimeout(function() {
        fadeOutAlert(alertDiv);
    }, 5000);
}

/**
 * Show notification using AdminLayout if available
 * @param {string} message - The message to display
 * @param {string} type - The notification type (success, error, warning, info)
 * @param {number} duration - Duration in milliseconds
 */
function showNotification(message, type = 'info', duration = 3000) {
    if (window.AdminLayout && window.AdminLayout.showNotification) {
        window.AdminLayout.showNotification(message, type, duration);
    } else {
        // Fallback to simple alert
        alert(message);
    }
}

/**
 * Show loading state for buttons
 * @param {HTMLElement} button - The button to show loading state
 */
function showButtonLoading(button) {
    const originalText = button.innerHTML;
    button.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Processing...';
    button.disabled = true;
    
    // Reset after 3 seconds (fallback)
    setTimeout(function() {
        button.innerHTML = originalText;
        button.disabled = false;
    }, 3000);
    
    return originalText;
}

/**
 * Reset button to original state
 * @param {HTMLElement} button - The button to reset
 * @param {string} originalText - The original button text
 */
function resetButton(button, originalText) {
    button.innerHTML = originalText;
    button.disabled = false;
}

// Export functions for global access (if needed)
window.CategoryManager = {
    fadeOutAlert,
    showValidationMessage,
    showNotification,
    showButtonLoading,
    resetButton,
    handleAddCategory,
    handleUpdateCategory,
    handleDeleteCategory
}; 