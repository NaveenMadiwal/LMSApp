/**
 * User Management JavaScript
 * Handles AJAX CRUD operations and interactive functionality
 */

document.addEventListener('DOMContentLoaded', function() {
    initializeStudentPage();
});

function initializeStudentPage() {
    setupAutoHideAlerts();
    setupManualCloseButtons();
    setupFormValidation();
    setupTableInteractions();
    setupAjaxForms();
    setupAjaxDelete();
    loadRoles();
    setupRoleBasedActions();
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
 * Load roles from API
 */
function loadRoles() {
    fetch('/api/StudentApi/roles')
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                populateRoleSelects(data.data);
            } else {
                console.error('Failed to load roles:', data.message);
            }
        })
        .catch(error => {
            console.error('Error loading roles:', error);
        });
}

/**
 * Populate role select elements
 */
function populateRoleSelects(roles) {
    // Populate main role select
    const mainSelect = document.querySelector('select[name="RoleType"]');
    if (mainSelect) {
        // Clear existing options except first
        const firstOption = mainSelect.querySelector('option');
        mainSelect.innerHTML = '';
        if (firstOption) {
            mainSelect.appendChild(firstOption);
        }
        
        roles.forEach(function(role) {
            const option = document.createElement('option');
            option.value = role.name;
            option.textContent = role.name;
            mainSelect.appendChild(option);
        });
    }
    
    // Populate role selects for existing users
    const userRoleSelects = document.querySelectorAll('select[name="RoleType"]');
    userRoleSelects.forEach(function(select) {
        if (select !== mainSelect) {
            // Clear existing options except first
            const firstOption = select.querySelector('option');
            select.innerHTML = '';
            if (firstOption) {
                select.appendChild(firstOption);
            }
            
            roles.forEach(function(role) {
                const option = document.createElement('option');
                option.value = role.name;
                option.textContent = role.name;
                select.appendChild(option);
            });
        }
    });
}

/**
 * Setup AJAX form submissions for add and edit operations
 */
function setupAjaxForms() {
    // Add user form
    const addForm = document.querySelector('form[asp-action="CreateStudent"]');
    if (addForm) {
        addForm.setAttribute('data-ajax-handled', 'true');
        addForm.addEventListener('submit', function(e) {
            e.preventDefault();
            handleAddStudent(this);
        });
    }

    // Update user forms
    const updateForms = document.querySelectorAll('form[asp-action="UpdateStudent"]');
    updateForms.forEach(function(form) {
        form.setAttribute('data-ajax-handled', 'true');
        form.addEventListener('submit', function(e) {
            e.preventDefault();
            handleUpdateStudent(this);
        });
    });
}

/**
 * Setup AJAX delete functionality
 */
function setupAjaxDelete() {
    const deleteLinks = document.querySelectorAll('.delete-student-btn');
    deleteLinks.forEach(function(link) {
        link.addEventListener('click', function(e) {
            e.preventDefault();
            handleDeleteStudent(this);
        });
    });
}

/**
 * Handle add student via AJAX
 * @param {HTMLFormElement} form - The form element
 */
function handleAddStudent(form) {
    const formData = new FormData(form);
    const submitButton = form.querySelector('button[type="submit"]');
    const originalText = showButtonLoading(submitButton);

    // Convert FormData to JSON for API
    const jsonData = {};
    formData.forEach((value, key) => {
        jsonData[key] = value;
    });

    fetch('/api/StudentApi', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        },
        body: JSON.stringify(jsonData)
    })
    .then(response => response.json())
    .then(data => {
        resetButton(submitButton, originalText);
        
        if (data.success) {
            showNotification(data.message || 'User added successfully!', 'success');
            form.reset();
            // Refresh the page to show new user
            setTimeout(() => location.reload(), 1500);
        } else {
            showNotification(data.message || 'Failed to add user.', 'error');
        }
    })
    .catch(error => {
        resetButton(submitButton, originalText);
        showNotification('An error occurred while adding the user.', 'error');
        console.error('Error:', error);
    });
}

/**
 * Handle update student via AJAX
 * @param {HTMLFormElement} form - The form element
 */
function handleUpdateStudent(form) {
    const formData = new FormData(form);
    const submitButton = form.querySelector('button[type="submit"]');
    const originalText = showButtonLoading(submitButton);

    // Get user ID from hidden input
    const userId = formData.get('Id');

    // Convert FormData to JSON for API
    const jsonData = {};
    formData.forEach((value, key) => {
        jsonData[key] = value;
    });

    fetch(`/api/StudentApi/${userId}`, {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        },
        body: JSON.stringify(jsonData)
    })
    .then(response => response.json())
    .then(data => {
        resetButton(submitButton, originalText);
        
        if (data.success) {
            showNotification(data.message || 'User updated successfully!', 'success');
            // Update the form fields to show the new values
            const nameInput = form.querySelector('input[name="FullName"]');
            if (nameInput) {
                nameInput.value = jsonData.FullName || nameInput.value;
            }
        } else {
            showNotification(data.message || 'Failed to update user.', 'error');
        }
    })
    .catch(error => {
        resetButton(submitButton, originalText);
        showNotification('An error occurred while updating the user.', 'error');
        console.error('Error:', error);
    });
}

/**
 * Handle delete student via AJAX
 * @param {HTMLElement} link - The delete link element
 */
function handleDeleteStudent(link) {
    const userId = link.getAttribute('href').split('/').pop();
    const userName = link.closest('tr').querySelector('input[name="FullName"]').value;
    
    if (confirm(`Are you sure you want to delete the user "${userName}"?`)) {
        fetch(`/api/StudentApi/${userId}`, {
            method: 'DELETE',
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                showNotification(data.message || 'User deleted successfully!', 'success');
                // Refresh the page to reflect changes
                setTimeout(() => location.reload(), 1500);
            } else {
                showNotification(data.message || 'Failed to delete user.', 'error');
            }
        })
        .catch(error => {
            showNotification('An error occurred while deleting the user.', 'error');
            console.error('Error:', error);
        });
    }
}

/**
 * Setup role-based actions
 */
function setupRoleBasedActions() {
    // Handle enrollments button for students
    document.addEventListener('click', function(e) {
        if (e.target.closest('button[onclick*="openEnrollmentsModal"]')) {
            e.preventDefault();
            const userId = e.target.closest('button').getAttribute('onclick').match(/'([^']+)'/)[1];
            openUserDetailsModal(userId, 'enrollments');
        }
        
        // Handle courses button for instructors
        if (e.target.closest('button[onclick*="openCoursesModal"]')) {
            e.preventDefault();
            const userId = e.target.closest('button').getAttribute('onclick').match(/'([^']+)'/)[1];
            openUserDetailsModal(userId, 'courses');
        }
        
        // Handle admin info button for admins
        if (e.target.closest('button[onclick*="openAdminInfoModal"]')) {
            e.preventDefault();
            const userId = e.target.closest('button').getAttribute('onclick').match(/'([^']+)'/)[1];
            openUserDetailsModal(userId, 'admin');
        }
    });
}

/**
 * Open user details modal based on role
 * @param {string} userId - The user ID
 * @param {string} type - The type of details to show (enrollments, courses, admin)
 */
function openUserDetailsModal(userId, type) {
    console.log(`Opening ${type} modal for user:`, userId);
    
    // Update modal title
    const modalTitle = document.getElementById('modalTitle');
    switch(type) {
        case 'enrollments':
            modalTitle.textContent = 'Student Enrollments';
            loadUserEnrollments(userId);
            break;
        case 'courses':
            modalTitle.textContent = 'Instructor Courses';
            loadInstructorCourses(userId);
            break;
        case 'admin':
            modalTitle.textContent = 'Admin Information';
            loadAdminInfo(userId);
            break;
    }
    
    // Show modal
    const modal = new bootstrap.Modal(document.getElementById('userDetailsModal'));
    modal.show();
}

/**
 * Load user enrollments from API
 * @param {string} userId - The user ID
 */
function loadUserEnrollments(userId) {
    const contentDiv = document.getElementById('userDetailsContent');
    contentDiv.innerHTML = '<div class="text-center"><i class="fas fa-spinner fa-spin"></i> Loading enrollments...</div>';
    
    fetch(`/api/StudentApi/${userId}/enrollments`)
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                displayUserEnrollments(data.data, userId);
            } else {
                contentDiv.innerHTML = `<div class="alert alert-danger">${data.message || 'Failed to load enrollments'}</div>`;
            }
        })
        .catch(error => {
            console.error('Error loading enrollments:', error);
            contentDiv.innerHTML = '<div class="alert alert-danger">Failed to load enrollments. Please try again.</div>';
        });
}

/**
 * Load instructor courses from API
 * @param {string} userId - The user ID
 */
function loadInstructorCourses(userId) {
    const contentDiv = document.getElementById('userDetailsContent');
    contentDiv.innerHTML = '<div class="text-center"><i class="fas fa-spinner fa-spin"></i> Loading courses...</div>';
    
    // For now, show a placeholder since we don't have instructor courses API yet
    contentDiv.innerHTML = `
        <div class="text-center text-muted">
            <i class="fas fa-book fa-3x mb-3"></i>
            <h5>Instructor Courses</h5>
            <p>Course management for instructors will be implemented in the next phase.</p>
            <div class="alert alert-info">
                <i class="fas fa-info-circle"></i>
                This feature will show all courses created by this instructor.
            </div>
        </div>
    `;
}

/**
 * Load admin information
 * @param {string} userId - The user ID
 */
function loadAdminInfo(userId) {
    const contentDiv = document.getElementById('userDetailsContent');
    contentDiv.innerHTML = '<div class="text-center"><i class="fas fa-spinner fa-spin"></i> Loading admin information...</div>';
    
    // For now, show admin information
    contentDiv.innerHTML = `
        <div class="text-center">
            <div class="mb-4">
                <i class="fas fa-shield-alt fa-3x text-danger mb-3"></i>
                <h5>System Administrator</h5>
                <p class="text-muted">Full system access and administrative privileges</p>
            </div>
            <div class="row">
                <div class="col-md-6">
                    <div class="card">
                        <div class="card-body">
                            <h6 class="card-title"><i class="fas fa-users text-primary"></i> User Management</h6>
                            <p class="card-text">Can create, edit, and delete all user accounts</p>
                        </div>
                    </div>
                </div>
                <div class="col-md-6">
                    <div class="card">
                        <div class="card-body">
                            <h6 class="card-title"><i class="fas fa-cog text-warning"></i> System Settings</h6>
                            <p class="card-text">Can access all system configuration options</p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    `;
}

/**
 * Display user enrollments in modal
 * @param {Array} enrollments - Array of enrollment objects
 * @param {string} userId - The user ID
 */
function displayUserEnrollments(enrollments, userId) {
    const contentDiv = document.getElementById('userDetailsContent');
    
    if (enrollments.length === 0) {
        contentDiv.innerHTML = `
            <div class="text-center text-muted">
                <i class="fas fa-graduation-cap fa-3x mb-3"></i>
                <h5>No enrollments found</h5>
                <p>This user has not enrolled in any courses yet.</p>
            </div>
        `;
        return;
    }
    
    let html = `
        <div class="table-responsive">
            <table class="table table-hover">
                <thead class="table-light">
                    <tr>
                        <th><i class="fas fa-book icon"></i> Course</th>
                        <th><i class="fas fa-calendar icon"></i> Enrolled On</th>
                        <th><i class="fas fa-tasks icon"></i> Status</th>
                        <th><i class="fas fa-circle icon"></i> Status</th>
                    </tr>
                </thead>
                <tbody>
    `;
    
    enrollments.forEach(function(enrollment) {
        const statusClass = getStatusClass(enrollment.completionStatus);
        const enrolledDate = new Date(enrollment.enrolledOn).toLocaleDateString();
        
        html += `
            <tr>
                <td>
                    <div class="fw-bold">${enrollment.courseTitle}</div>
                </td>
                <td>${enrolledDate}</td>
                <td>
                    <span class="badge ${statusClass}">${enrollment.completionStatus}</span>
                </td>
                <td>
                    <i class="fas fa-circle ${enrollment.isActive ? 'text-success' : 'text-danger'}"></i>
                    <span class="small">${enrollment.isActive ? 'Active' : 'Inactive'}</span>
                </td>
            </tr>
        `;
    });
    
    html += `
                </tbody>
            </table>
        </div>
    `;
    
    contentDiv.innerHTML = html;
}

/**
 * Get CSS class for completion status
 * @param {string} status - The completion status
 * @returns {string} CSS class name
 */
function getStatusClass(status) {
    switch (status.toLowerCase()) {
        case 'completed':
            return 'bg-success';
        case 'in progress':
            return 'bg-warning';
        case 'pending':
            return 'bg-secondary';
        default:
            return 'bg-info';
    }
}

/**
 * Setup form validation
 */
function setupFormValidation() {
    const forms = document.querySelectorAll('form');
    
    forms.forEach(function(form) {
        form.addEventListener('submit', function(e) {
            if (!form.checkValidity()) {
                e.preventDefault();
                e.stopPropagation();
                showValidationMessage('Please fill in all required fields correctly.');
            }
            form.classList.add('was-validated');
        });
    });
}

/**
 * Setup table interactions
 */
function setupTableInteractions() {
    const tableRows = document.querySelectorAll('.table tbody tr');
    
    tableRows.forEach(function(row) {
        row.addEventListener('mouseenter', function() {
            this.style.transform = 'scale(1.01)';
        });
        
        row.addEventListener('mouseleave', function() {
            this.style.transform = 'scale(1)';
        });
    });
}

/**
 * Show validation message
 * @param {string} message - The message to display
 */
function showValidationMessage(message) {
    showNotification(message, 'warning');
}

/**
 * Show notification message
 * @param {string} message - The message to display
 * @param {string} type - The type of notification (success, error, warning, info)
 * @param {number} duration - Duration in milliseconds
 */
function showNotification(message, type = 'info', duration = 3000) {
    // Create notification element
    const notification = document.createElement('div');
    notification.className = `alert alert-${type === 'error' ? 'danger' : type} alert-dismissible fade show position-fixed`;
    notification.style.cssText = `
        top: 20px;
        right: 20px;
        z-index: 9999;
        min-width: 300px;
        box-shadow: 0 4px 12px rgba(0,0,0,0.15);
    `;
    
    notification.innerHTML = `
        <i class="fas fa-${getNotificationIcon(type)} me-2"></i>
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    
    // Add to page
    document.body.appendChild(notification);
    
    // Auto-remove after duration
    setTimeout(function() {
        if (notification.parentNode) {
            notification.remove();
        }
    }, duration);
}

/**
 * Get icon for notification type
 * @param {string} type - The notification type
 * @returns {string} Icon class name
 */
function getNotificationIcon(type) {
    switch (type) {
        case 'success':
            return 'check-circle';
        case 'error':
        case 'danger':
            return 'exclamation-circle';
        case 'warning':
            return 'exclamation-triangle';
        default:
            return 'info-circle';
    }
}

/**
 * Show loading state on button
 * @param {HTMLElement} button - The button element
 * @returns {string} Original button text
 */
function showButtonLoading(button) {
    const originalText = button.innerHTML;
    button.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Processing...';
    button.disabled = true;
    return originalText;
}

/**
 * Reset button to original state
 * @param {HTMLElement} button - The button element
 * @param {string} originalText - The original button text
 */
function resetButton(button, originalText) {
    button.innerHTML = originalText;
    button.disabled = false;
}

// Student Portal JavaScript
// Minimal JavaScript for sidebar functionality

document.addEventListener('DOMContentLoaded', function() {
    // Sidebar toggle functionality
    const sidebarToggle = document.getElementById('sidebarToggle');
    const sidebar = document.getElementById('sidebar');
    
    if (sidebarToggle && sidebar) {
        sidebarToggle.addEventListener('click', function() {
            sidebar.classList.toggle('show');
        });
    }
    
    // Close sidebar when clicking outside on mobile
    document.addEventListener('click', function(event) {
        if (window.innerWidth <= 992) {
            if (!sidebar.contains(event.target) && !sidebarToggle.contains(event.target)) {
                sidebar.classList.remove('show');
            }
        }
    });
    
    // Auto-hide alerts after 5 seconds
    const alerts = document.querySelectorAll('.alert');
    alerts.forEach(function(alert) {
        setTimeout(function() {
            if (alert.parentNode) {
                alert.classList.remove('show');
                setTimeout(function() {
                    if (alert.parentNode) {
                        alert.remove();
                    }
                }, 150);
            }
        }, 5000);
    });
    
    // Add fade-in animation to cards
    const cards = document.querySelectorAll('.card, .stats-card');
    cards.forEach(function(card, index) {
        card.style.opacity = '0';
        card.style.transform = 'translateY(20px)';
        
        setTimeout(function() {
            card.style.transition = 'all 0.6s ease-out';
            card.style.opacity = '1';
            card.style.transform = 'translateY(0)';
        }, index * 100);
    });
});

// Utility function for showing notifications (if needed)
function showNotification(message, type = 'info') {
    const alertDiv = document.createElement('div');
    alertDiv.className = `alert alert-${type} alert-dismissible fade show position-fixed`;
    alertDiv.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
    alertDiv.innerHTML = `
        <i class="fas fa-${type === 'success' ? 'check-circle' : type === 'error' ? 'exclamation-circle' : 'info-circle'} me-2"></i>
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    
    document.body.appendChild(alertDiv);
    
    // Auto-remove after 5 seconds
    setTimeout(function() {
        if (alertDiv.parentNode) {
            alertDiv.remove();
        }
    }, 5000);
}

function toggleUserActive(userId, makeActive) {
    var action = makeActive ? 'activate' : 'inactivate';
    var confirmMsg = makeActive
        ? 'Are you sure you want to make this user active?'
        : 'Are you sure you want to make this user inactive?';
    if (confirm(confirmMsg)) {
        fetch('/Admin/ToggleUserActive', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
            },
            body: JSON.stringify({ userId: userId, isActive: makeActive })
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                showNotification('User status updated successfully.', 'success');
                setTimeout(function() { location.reload(); }, 1000);
            } else {
                showNotification(data.message || 'Failed to update user status.', 'danger');
            }
        })
        .catch(() => {
            showNotification('Error updating user status.', 'danger');
        });
    }
} 