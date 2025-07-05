// Instructor Course Management JS
// Handles CRUD for instructor's own courses

document.addEventListener('DOMContentLoaded', function() {
    loadInstructorCourses();
    setupAddCourseModal();
    setupCourseFormSubmit();
});

function getCurrentUserId() {
    // This should be set server-side (e.g., in a hidden field or JS variable)
    return document.getElementById('currentInstructorId')?.value || '';
}

function loadInstructorCourses() {
    console.log('Loading instructor courses...');
    fetch('/Instructor/GetMyCourses', {
        method: 'GET',
        headers: { 
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
    .then(res => {
        console.log('Response status:', res.status);
        return res.json();
    })
    .then(data => {
        console.log('Response data:', data);
        if (data.success) {
            console.log('Courses data:', data.data);
            renderCourseList(data.data);
        } else {
            console.error('Failed to load courses:', data.message);
            showCourseNotification('Failed to load courses: ' + data.message, 'danger');
        }
    })
    .catch((error) => {
        console.error('Error loading courses:', error);
        showCourseNotification('Error loading courses', 'danger');
    });
}

function renderCourseList(courses) {
    const listDiv = document.getElementById('coursesList');
    if (!courses.length) {
        listDiv.innerHTML = '<div class="alert alert-info">No courses to display. Start by adding a new course.</div>';
        return;
    }
    let html = '<div class="row g-3">';
    courses.forEach(course => {
        html += `
        <div class="col-md-6">
            <div class="card shadow-sm border-0 h-100">
                <div class="card-body">
                    <h5 class="card-title">${course.title}</h5>
                    <div class="text-muted mb-2">${course.categoryName || 'Uncategorized'}</div>
                    <div class="mb-2">${course.description || ''}</div>
                    <div class="mb-2 small">
                        <i class="fas fa-calendar"></i> ${course.startDate ? formatDate(course.startDate) : '-'}
                        &rarr; ${course.endDate ? formatDate(course.endDate) : '-'}
                    </div>
                    <div class="mb-2 small">
                        <i class="fas fa-users"></i> ${course.enrollmentCount} Enrolled
                    </div>
                    <div class="d-flex gap-2 mt-2">
                        <button class="btn btn-sm btn-outline-primary" onclick="openEditCourseModal(${course.id})"><i class="fas fa-edit"></i> Edit</button>
                        <button class="btn btn-sm btn-outline-danger" onclick="deleteCourse(${course.id})"><i class="fas fa-trash"></i> Delete</button>
                    </div>
                </div>
            </div>
        </div>`;
    });
    html += '</div>';
    listDiv.innerHTML = html;
}

function setupAddCourseModal() {
    const btn = document.querySelector('.btn.btn-primary.mb-3');
    if (btn) {
        btn.addEventListener('click', function() {
            openCourseModal();
        });
    }
}

function openCourseModal(course = null) {
    // Create or edit modal
    let modalHtml = `
    <div class="modal fade" id="courseModal" tabindex="-1">
        <div class="modal-dialog">
            <div class="modal-content">
                <form id="courseForm">
                    <div class="modal-header">
                        <h5 class="modal-title">${course ? 'Edit Course' : 'Add New Course'}</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body">
                        <input type="hidden" name="Id" value="${course ? course.id : ''}" />
                        <div class="mb-3">
                            <label class="form-label">Title</label>
                            <input type="text" class="form-control" name="Title" value="${course ? course.title : ''}" required maxlength="100" />
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Description</label>
                            <textarea class="form-control" name="Description" rows="2" required maxlength="500">${course ? course.description : ''}</textarea>
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Category</label>
                            <select class="form-control" name="CategoryId" id="courseCategorySelect">
                                <option value="">Select Category</option>
                            </select>
                        </div>
                        <div class="row">
                            <div class="col">
                                <label class="form-label">Start Date</label>
                                <input type="date" class="form-control" name="StartDate" value="${course && course.startDate ? course.startDate.split('T')[0] : ''}" />
                            </div>
                            <div class="col">
                                <label class="form-label">End Date</label>
                                <input type="date" class="form-control" name="EndDate" value="${course && course.endDate ? course.endDate.split('T')[0] : ''}" />
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                        <button type="submit" class="btn btn-primary">${course ? 'Update' : 'Add'} Course</button>
                    </div>
                </form>
            </div>
        </div>
    </div>`;
    document.body.insertAdjacentHTML('beforeend', modalHtml);
    loadCategories(course ? course.categoryId : null);
    const modal = new bootstrap.Modal(document.getElementById('courseModal'));
    modal.show();
    document.getElementById('courseModal').addEventListener('hidden.bs.modal', function() {
        document.getElementById('courseModal').remove();
    });
    document.getElementById('courseForm').onsubmit = function(e) {
        e.preventDefault();
        submitCourseForm(course);
    };
}

function setupCourseFormSubmit() {
    // For edit modal opened from card
    window.openEditCourseModal = function(id) {
        fetch(`/api/CourseApi/${id}`)
            .then(res => res.json())
            .then(data => {
                if (data.success) {
                    openCourseModal(data.data);
                } else {
                    showCourseNotification('Failed to load course', 'danger');
                }
            });
    };
}

function submitCourseForm(existingCourse) {
    const form = document.getElementById('courseForm');
    const formData = new FormData(form);
    
    // Debug: Log form data
    console.log('Form data being sent:');
    for (let [key, value] of formData.entries()) {
        console.log(key + ': ' + value);
    }
    
    // Clean up form data - handle empty values properly
    const title = formData.get('Title')?.trim();
    const description = formData.get('Description')?.trim();
    const categoryId = formData.get('CategoryId');
    const startDate = formData.get('StartDate');
    const endDate = formData.get('EndDate');
    
    // Validate required fields
    if (!title) {
        showCourseNotification('Course title is required.', 'danger');
        return;
    }
    if (!description) {
        showCourseNotification('Course description is required.', 'danger');
        return;
    }
    
    // Create new FormData with cleaned values
    const cleanFormData = new FormData();
    cleanFormData.append('Title', title);
    cleanFormData.append('Description', description);
    if (categoryId) cleanFormData.append('CategoryId', categoryId);
    if (startDate) cleanFormData.append('StartDate', startDate);
    if (endDate) cleanFormData.append('EndDate', endDate);
    cleanFormData.append('IsActive', 'true');
    
    // Add anti-forgery token
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    if (token) {
        cleanFormData.append('__RequestVerificationToken', token);
    }
    
    const url = existingCourse ? '/Instructor/UpdateCourse' : '/Instructor/CreateCourse';
    
    fetch(url, {
        method: 'POST',
        headers: { 
            'X-Requested-With': 'XMLHttpRequest'
        },
        body: cleanFormData
    })
    .then(res => res.json())
    .then(data => {
        if (data.success) {
            showCourseNotification(data.message || 'Course saved!', 'success');
            document.querySelector('.modal.show .btn-close').click();
            loadInstructorCourses();
        } else {
            let errorMessage = data.message || 'Failed to save course.';
            if (data.errors && data.errors.length > 0) {
                errorMessage += '\n' + data.errors.join('\n');
            }
            if (data.receivedData) {
                console.log('Received data:', data.receivedData);
                errorMessage += '\nReceived: ' + JSON.stringify(data.receivedData);
            }
            showCourseNotification(errorMessage, 'danger');
        }
    })
    .catch((error) => {
        console.error('Error:', error);
        showCourseNotification('Error saving course.', 'danger');
    });
}

function deleteCourse(id) {
    if (!confirm('Are you sure you want to delete this course?')) return;
    
    const formData = new FormData();
    formData.append('id', id);
    
    // Add anti-forgery token
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    if (token) {
        formData.append('__RequestVerificationToken', token);
    }
    
    fetch('/Instructor/DeleteCourse', {
        method: 'POST',
        headers: { 
            'X-Requested-With': 'XMLHttpRequest'
        },
        body: formData
    })
    .then(res => res.json())
    .then(data => {
        if (data.success) {
            showCourseNotification('Course deleted.', 'success');
            loadInstructorCourses();
        } else {
            showCourseNotification(data.message || 'Failed to delete course.', 'danger');
        }
    })
    .catch((error) => {
        console.error('Error:', error);
        showCourseNotification('Error deleting course.', 'danger');
    });
}

function loadCategories(selectedId) {
    fetch('/api/CourseApi/categories')
        .then(res => res.json())
        .then(data => {
            if (data.success) {
                const select = document.getElementById('courseCategorySelect');
                select.innerHTML = '<option value="">Select Category</option>';
                data.data.forEach(cat => {
                    select.innerHTML += `<option value="${cat.id}"${selectedId == cat.id ? ' selected' : ''}>${cat.name}</option>`;
                });
            }
        })
        .catch(error => {
            console.error('Error loading categories:', error);
        });
}

function showCourseNotification(message, type = 'info') {
    const alert = document.createElement('div');
    alert.className = `alert alert-${type} position-fixed top-0 end-0 m-4 fade show`;
    alert.style.zIndex = 9999;
    alert.innerHTML = `<i class="fas fa-info-circle me-2"></i>${message}`;
    document.body.appendChild(alert);
    setTimeout(() => alert.remove(), 3000);
}

function formatDate(dateString) {
    if (!dateString) return '';
    const d = new Date(dateString);
    return d.toLocaleDateString();
} 