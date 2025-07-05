// Instructor Enrollments Management JS
// Handles viewing enrollments for instructor's courses

document.addEventListener('DOMContentLoaded', function() {
    loadInstructorCourses();
    setupCourseSelection();
});

function loadInstructorCourses() {
    fetch('/Instructor/GetMyCourses', {
        method: 'GET',
        headers: { 
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
    .then(res => res.json())
    .then(data => {
        if (data.success) {
            populateCourseDropdown(data.data);
        } else {
            showEnrollmentNotification('Failed to load courses', 'danger');
        }
    })
    .catch(() => showEnrollmentNotification('Error loading courses', 'danger'));
}

function populateCourseDropdown(courses) {
    const select = document.getElementById('courseSelect');
    select.innerHTML = '<option value="">Choose a course to view enrollments...</option>';
    
    if (courses && courses.length > 0) {
        courses.forEach(course => {
            select.innerHTML += `<option value="${course.id}">${course.title}</option>`;
        });
    } else {
        select.innerHTML = '<option value="">No courses available</option>';
    }
}

function setupCourseSelection() {
    const courseSelect = document.getElementById('courseSelect');
    courseSelect.addEventListener('change', function() {
        const courseId = this.value;
        if (courseId) {
            loadCourseEnrollments(courseId);
        } else {
            showDefaultMessage();
        }
    });
}

function loadCourseEnrollments(courseId) {
    fetch(`/Instructor/GetCourseEnrollments/${courseId}`, {
        method: 'GET',
        headers: { 
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
    .then(res => res.json())
    .then(data => {
        if (data.success) {
            renderEnrollmentsList(data.data, data.courseTitle);
        } else {
            showEnrollmentNotification('Failed to load enrollments', 'danger');
        }
    })
    .catch(() => showEnrollmentNotification('Error loading enrollments', 'danger'));
}

function renderEnrollmentsList(enrollments, courseTitle) {
    const listDiv = document.getElementById('enrollmentsList');
    
    if (!enrollments || enrollments.length === 0) {
        listDiv.innerHTML = `
            <div class="alert alert-info">
                <i class="fas fa-info-circle me-2"></i>
                No students are currently enrolled in "${courseTitle}".
            </div>`;
        return;
    }
    
    let html = `
        <div class="card enrollment-card">
            <div class="card-header bg-primary text-white">
                <h5 class="mb-0"><i class="fas fa-users me-2"></i>Enrollments for "${courseTitle}" (${enrollments.length} students)</h5>
            </div>
            <div class="card-body p-0">
                <div class="table-responsive">
                    <table class="table table-hover mb-0">
                        <thead class="table-light">
                            <tr>
                                <th>Student Name</th>
                                <th>Email</th>
                                <th>Enrollment Date</th>
                                <th>Status</th>
                            </tr>
                        </thead>
                        <tbody>`;
    
    enrollments.forEach(enrollment => {
        const enrollmentDate = enrollment.enrollmentDate ? formatDate(enrollment.enrollmentDate) : 'N/A';
        html += `
            <tr>
                <td>
                    <div class="d-flex align-items-center">
                        <div class="avatar-sm bg-primary text-white rounded-circle d-flex align-items-center justify-content-center me-2">
                            <i class="fas fa-user"></i>
                        </div>
                        <div>
                            <div class="fw-semibold">${enrollment.studentName || 'Unknown'}</div>
                        </div>
                    </div>
                </td>
                <td>${enrollment.studentEmail || 'N/A'}</td>
                <td>${enrollmentDate}</td>
                <td>
                    <span class="badge bg-success">Enrolled</span>
                </td>
            </tr>`;
    });
    
    html += `
                        </tbody>
                    </table>
                </div>
            </div>
        </div>`;
    
    listDiv.innerHTML = html;
}

function showDefaultMessage() {
    const listDiv = document.getElementById('enrollmentsList');
    listDiv.innerHTML = `
        <div class="alert alert-info">
            <i class="fas fa-info-circle me-2"></i>
            Please select a course to view its enrollments.
        </div>`;
}

function showEnrollmentNotification(message, type = 'info') {
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