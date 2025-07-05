// Course Management JavaScript

$(document).ready(function() {
    console.log('Course management page loaded');
    
    // Load categories for all select elements
    loadCategories();
    
    // Handle form submissions with AJAX
    handleFormSubmissions();
    
    // Handle delete operations
    handleDeleteOperations();
    
    // Initialize tooltips and other Bootstrap components
    initializeComponents();
    
    // Set categories after a short delay to ensure data is loaded
    setTimeout(function() {
        setCategorySelections();
    }, 500);
});

// Load categories from API
function loadCategories() {
    console.log('Loading categories...');
    
    $.ajax({
        url: '/api/CourseApi/categories',
        method: 'GET',
        success: function(response) {
            if (response.success) {
                populateCategorySelects(response.data);
            } else {
                console.error('Failed to load categories:', response.message);
            }
        },
        error: function(xhr, status, error) {
            console.error('Error loading categories:', error);
        }
    });
}

// Populate category select elements
function populateCategorySelects(categories) {
    console.log('Populating category selects with:', categories);
    
    // Populate main category select
    const mainSelect = $('#categorySelect');
    mainSelect.find('option:not(:first)').remove();
    
    categories.forEach(function(category) {
        mainSelect.append(`<option value="${category.id}">${category.name}</option>`);
    });
    
    // Populate category selects for existing courses
    $('[id^="categorySelect_"]').each(function() {
        const select = $(this);
        const courseId = select.attr('id').replace('categorySelect_', '');
        
        // Clear existing options except first
        select.find('option:not(:first)').remove();
        
        categories.forEach(function(category) {
            select.append(`<option value="${category.id}">${category.name}</option>`);
        });
        
        // Set the current category if available
        const currentCategory = select.data('current-category');
        console.log(`Setting category for course ${courseId}: ${currentCategory}`);
        if (currentCategory && currentCategory !== '0' && currentCategory !== '') {
            select.val(currentCategory);
        }
    });
}

// Handle form submissions with AJAX
function handleFormSubmissions() {
    console.log('Setting up form submissions...');
    
    // Handle course creation form
    $('form[asp-action="CreateCourse"]').on('submit', function(e) {
        e.preventDefault();
        console.log('Course creation form submitted');
        
        const form = $(this);
        const formData = new FormData(this);
        
        // Add AJAX header
        formData.append('X-Requested-With', 'XMLHttpRequest');
        
        $.ajax({
            url: form.attr('action'),
            method: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function(response) {
                if (response.success) {
                    showAlert('success', response.message);
                    form[0].reset();
                    setTimeout(function() {
                        location.reload();
                    }, 1500);
                } else {
                    showAlert('danger', response.message);
                }
            },
            error: function(xhr, status, error) {
                console.error('Error creating course:', error);
                showAlert('danger', 'Failed to create course. Please try again.');
            }
        });
    });
    
    // Handle course update forms
    $('form[asp-action="UpdateCourse"]').on('submit', function(e) {
        e.preventDefault();
        console.log('Course update form submitted');
        
        const form = $(this);
        const formData = new FormData(this);
        
        // Add AJAX header
        formData.append('X-Requested-With', 'XMLHttpRequest');
        
        $.ajax({
            url: form.attr('action'),
            method: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function(response) {
                if (response.success) {
                    showAlert('success', response.message);
                    // Update the course title in the UI if provided
                    if (response.courseTitle) {
                        form.find('input[name="Title"]').val(response.courseTitle);
                    }
                } else {
                    showAlert('danger', response.message);
                }
            },
            error: function(xhr, status, error) {
                console.error('Error updating course:', error);
                showAlert('danger', 'Failed to update course. Please try again.');
            }
        });
    });
}

// Handle delete operations
function handleDeleteOperations() {
    console.log('Setting up delete operations...');
    
    $('.delete-course-btn').on('click', function(e) {
        e.preventDefault();
        console.log('Delete course clicked');
        
        const link = $(this);
        const courseId = link.attr('href').split('/').pop();
        const courseTitle = link.closest('tr').find('input[name="Title"]').val();
        
        if (confirm(`Are you sure you want to delete the course "${courseTitle}"?`)) {
            $.ajax({
                url: `/Admin/DeleteCourse/${courseId}`,
                method: 'POST',
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                },
                success: function(response) {
                    if (response.success) {
                        showAlert('success', response.message);
                        setTimeout(function() {
                            location.reload();
                        }, 1500);
                    } else {
                        showAlert('danger', response.message);
                    }
                },
                error: function(xhr, status, error) {
                    console.error('Error deleting course:', error);
                    showAlert('danger', 'Failed to delete course. Please try again.');
                }
            });
        }
    });
}

// Open materials modal
function openMaterialsModal(courseId) {
    console.log('Opening materials modal for course:', courseId);
    
    // Store course ID for later use
    $('#materialsModal').data('course-id', courseId);
    
    // Load course materials
    loadCourseMaterials(courseId);
    
    // Show modal
    $('#materialsModal').modal('show');
    
    // Handle modal close event
    $('#materialsModal').off('hidden.bs.modal').on('hidden.bs.modal', function() {
        // Refresh the course list to update material counts
        setTimeout(function() {
            location.reload();
        }, 500);
    });
}

// Load course materials
function loadCourseMaterials(courseId) {
    console.log('Loading materials for course:', courseId);
    
    $.ajax({
        url: `/api/CourseApi/${courseId}`,
        method: 'GET',
        success: function(response) {
            if (response.success) {
                displayCourseMaterials(response.data, courseId);
            } else {
                showAlert('danger', response.message);
            }
        },
        error: function(xhr, status, error) {
            console.error('Error loading course materials:', error);
            showAlert('danger', 'Failed to load course materials.');
        }
    });
}

// Display course materials in modal
function displayCourseMaterials(course, courseId) {
    console.log('Displaying materials for course:', course);
    
    const materialsContent = $('#materialsContent');
    let html = `
        <div class="mb-4">
            <h6 class="text-primary">
                <i class="fas fa-book"></i>
                ${course.title}
            </h6>
            <p class="text-muted">${course.description}</p>
        </div>
        
        <div class="mb-4">
            <h6>
                <i class="fas fa-file-upload"></i>
                Add New Material
            </h6>
            <form id="addMaterialForm" enctype="multipart/form-data">
                <div class="row g-3">
                    <div class="col-md-6">
                        <label class="form-label">Material Title *</label>
                        <input type="text" class="form-control" name="Title" required maxlength="100" />
                    </div>
                    <div class="col-md-6">
                        <label class="form-label">File *</label>
                        <input type="file" class="form-control" name="File" required />
                    </div>
                    <div class="col-12">
                        <button type="submit" class="btn btn-primary">
                            <i class="fas fa-plus"></i>
                            Add Material
                        </button>
                    </div>
                </div>
            </form>
        </div>
    `;
    
    if (course.materials && course.materials.length > 0) {
        html += `
            <div>
                <h6>
                    <i class="fas fa-list"></i>
                    Current Materials (${course.materials.length})
                </h6>
                <div class="table-responsive">
                    <table class="table table-sm">
                        <thead class="table-light">
                            <tr>
                                <th>Title</th>
                                <th>Type</th>
                                <th>Added</th>
                                <th>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
        `;
        
        course.materials.forEach(function(material) {
            // Format the date safely
            let formattedDate = 'Unknown';
            if (material.createdAt) {
                try {
                    const date = new Date(material.createdAt);
                    if (!isNaN(date.getTime())) {
                        formattedDate = date.toLocaleDateString('en-US', {
                            year: 'numeric',
                            month: 'short',
                            day: 'numeric'
                        });
                    }
                } catch (e) {
                    console.error('Error formatting date:', e);
                }
            }
            
            html += `
                <tr>
                    <td>${material.title}</td>
                    <td><span class="badge bg-secondary">${material.fileType}</span></td>
                    <td>${formattedDate}</td>
                    <td>
                        <a href="${material.filePath}" class="btn btn-sm btn-outline-primary" target="_blank">
                            <i class="fas fa-download"></i>
                        </a>
                        <button class="btn btn-sm btn-outline-danger" onclick="deleteMaterial(${material.id})">
                            <i class="fas fa-trash"></i>
                        </button>
                    </td>
                </tr>
            `;
        });
        
        html += `
                        </tbody>
                    </table>
                </div>
            </div>
        `;
    } else {
        html += `
            <div class="text-center text-muted py-4">
                <i class="fas fa-file fa-3x mb-3"></i>
                <h6>No materials uploaded yet</h6>
                <p>Upload the first material using the form above.</p>
            </div>
        `;
    }
    
    materialsContent.html(html);
    
    // Handle material form submission
    $('#addMaterialForm').on('submit', function(e) {
        e.preventDefault();
        addCourseMaterial(courseId);
    });
}

// Add course material
function addCourseMaterial(courseId) {
    console.log('Adding material for course:', courseId);
    
    const form = $('#addMaterialForm')[0];
    const formData = new FormData(form);
    
    $.ajax({
        url: `/api/CourseApi/${courseId}/materials`,
        method: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function(response) {
            if (response.success) {
                // Show success message
                showAlert('success', 'Material added successfully!');
                
                // Reset form
                form.reset();
                
                // Reload materials
                loadCourseMaterials(courseId);
                
                // Update the material count in the main table
                updateCourseMaterialCount(courseId);
                
                // Show success message in modal
                $('#materialsContent').prepend(`
                    <div class="alert alert-success alert-dismissible fade show" role="alert">
                        <i class="fas fa-check-circle"></i>
                        Material "${response.data.title}" added successfully!
                        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
                    </div>
                `);
                
                // Auto-dismiss modal alert after 3 seconds
                setTimeout(function() {
                    $('#materialsContent .alert').fadeOut();
                }, 3000);
                
            } else {
                showAlert('danger', response.message);
            }
        },
        error: function(xhr, status, error) {
            console.error('Error adding material:', error);
            showAlert('danger', 'Failed to add material. Please try again.');
        }
    });
}

// Delete course material
function deleteMaterial(materialId) {
    console.log('Deleting material:', materialId);
    
    if (confirm('Are you sure you want to delete this material?')) {
        $.ajax({
            url: `/api/CourseApi/materials/${materialId}`,
            method: 'DELETE',
            success: function(response) {
                if (response.success) {
                    showAlert('success', response.message);
                    // Reload materials
                    const courseId = $('#materialsModal').data('course-id');
                    loadCourseMaterials(courseId);
                } else {
                    showAlert('danger', response.message);
                }
            },
            error: function(xhr, status, error) {
                console.error('Error deleting material:', error);
                showAlert('danger', 'Failed to delete material. Please try again.');
            }
        });
    }
}

// Show alert message
function showAlert(type, message) {
    console.log(`Showing ${type} alert:`, message);
    
    const alertHtml = `
        <div class="alert alert-${type} alert-dismissible fade show" role="alert">
            <i class="fas fa-${type === 'success' ? 'check-circle' : 'exclamation-triangle'} icon"></i>
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;
    
    // Remove existing alerts
    $('.alert').remove();
    
    // Add new alert at the top
    $('.course-container').prepend(alertHtml);
    
    // Auto-dismiss after 5 seconds
    setTimeout(function() {
        $('.alert').fadeOut();
    }, 5000);
}

// Initialize Bootstrap components
function initializeComponents() {
    // Initialize tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
    
    // Initialize popovers
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });
}

// Utility function to format dates
function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric'
    });
}

// Set category selections after categories are loaded
function setCategorySelections() {
    $('[id^="categorySelect_"]').each(function() {
        const select = $(this);
        const currentCategory = select.data('current-category');
        console.log(`Setting category for select ${select.attr('id')}: ${currentCategory}`);
        if (currentCategory && currentCategory !== '0' && currentCategory !== '') {
            select.val(currentCategory);
        }
    });
}

// Update course material count in the main table
function updateCourseMaterialCount(courseId) {
    // Find the course row and update the material count
    const courseRow = $(`tr:has(input[name="Id"][value="${courseId}"])`);
    const materialCountElement = courseRow.find('.stat-item:first span');
    
    // Get current count and increment it
    const currentText = materialCountElement.text();
    const currentCount = parseInt(currentText.match(/\d+/)[0]) || 0;
    const newCount = currentCount + 1;
    
    // Update the text
    materialCountElement.text(`${newCount} Materials`);
}

// Utility function to format file size
function formatFileSize(bytes) {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
} 