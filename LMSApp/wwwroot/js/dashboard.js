/**
 * Dashboard JavaScript
 * Handles interactive features and animations
 */

document.addEventListener('DOMContentLoaded', function() {
    initializeDashboard();
});

function initializeDashboard() {
    updateCurrentTime();
    setupTimeUpdate();
    setupAnimations();
    setupInteractiveElements();
    setupCharts();
}

/**
 * Update and display current time
 */
function updateCurrentTime() {
    const timeElement = document.getElementById('currentTime');
    if (timeElement) {
        const now = new Date();
        const options = {
            weekday: 'long',
            year: 'numeric',
            month: 'long',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit',
            second: '2-digit'
        };
        timeElement.textContent = now.toLocaleDateString('en-US', options);
    }
}

/**
 * Setup automatic time updates
 */
function setupTimeUpdate() {
    // Update time every second
    setInterval(updateCurrentTime, 1000);
}

/**
 * Setup dashboard animations
 */
function setupAnimations() {
    // Add entrance animations to elements
    const animatedElements = document.querySelectorAll('.stat-card, .action-card, .activity-item, .status-item');
    
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.style.opacity = '1';
                entry.target.style.transform = 'translateY(0)';
            }
        });
    }, {
        threshold: 0.1
    });

    animatedElements.forEach(element => {
        element.style.opacity = '0';
        element.style.transform = 'translateY(20px)';
        element.style.transition = 'opacity 0.6s ease, transform 0.6s ease';
        observer.observe(element);
    });
}

/**
 * Setup interactive elements
 */
function setupInteractiveElements() {
    // Add hover effects to stat cards
    const statCards = document.querySelectorAll('.stat-card');
    statCards.forEach(card => {
        card.addEventListener('mouseenter', function() {
            this.style.transform = 'translateY(-8px) scale(1.02)';
        });
        
        card.addEventListener('mouseleave', function() {
            this.style.transform = 'translateY(0) scale(1)';
        });
    });

    // Add click effects to action cards
    const actionCards = document.querySelectorAll('.action-card');
    actionCards.forEach(card => {
        card.addEventListener('click', function() {
            // Add a subtle click effect
            this.style.transform = 'scale(0.98)';
            setTimeout(() => {
                this.style.transform = '';
            }, 150);
        });
    });

    // Add pulse animation to status indicators
    const statusIndicators = document.querySelectorAll('.status-indicator');
    statusIndicators.forEach(indicator => {
        if (indicator.parentElement.classList.contains('online')) {
            indicator.style.animation = 'pulse 2s infinite';
        }
    });
}

/**
 * Setup mock charts (placeholder for future chart implementations)
 */
function setupCharts() {
    // This is a placeholder for future chart implementations
    // You can integrate Chart.js, D3.js, or any other charting library here
    
    console.log('Dashboard charts initialized');
}

/**
 * Update statistics with animation
 */
function updateStatistic(elementId, newValue, duration = 1000) {
    const element = document.getElementById(elementId);
    if (!element) return;

    const startValue = parseInt(element.textContent) || 0;
    const increment = (newValue - startValue) / (duration / 16); // 60fps
    let currentValue = startValue;

    const timer = setInterval(() => {
        currentValue += increment;
        if ((increment > 0 && currentValue >= newValue) || 
            (increment < 0 && currentValue <= newValue)) {
            currentValue = newValue;
            clearInterval(timer);
        }
        element.textContent = Math.floor(currentValue);
    }, 16);
}

/**
 * Show notification
 */
function showDashboardNotification(message, type = 'info', duration = 3000) {
    if (window.AdminLayout && window.AdminLayout.showNotification) {
        window.AdminLayout.showNotification(message, type, duration);
    } else {
        // Fallback to simple alert
        alert(message);
    }
}

/**
 * Refresh dashboard data
 */
function refreshDashboardData() {
    // Show loading state
    const refreshButton = document.querySelector('.refresh-btn');
    if (refreshButton) {
        const originalText = refreshButton.innerHTML;
        refreshButton.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Refreshing...';
        refreshButton.disabled = true;

        // Simulate data refresh
        setTimeout(() => {
            refreshButton.innerHTML = originalText;
            refreshButton.disabled = false;
            showDashboardNotification('Dashboard data refreshed successfully!', 'success');
        }, 2000);
    }
}

/**
 * Toggle dashboard sections
 */
function toggleSection(sectionId) {
    const section = document.getElementById(sectionId);
    if (section) {
        const isVisible = section.style.display !== 'none';
        section.style.display = isVisible ? 'none' : 'block';
        
        // Add smooth transition
        if (!isVisible) {
            section.style.opacity = '0';
            section.style.transform = 'translateY(20px)';
            setTimeout(() => {
                section.style.opacity = '1';
                section.style.transform = 'translateY(0)';
            }, 10);
        }
    }
}

/**
 * Export dashboard data
 */
function exportDashboardData() {
    const data = {
        timestamp: new Date().toISOString(),
        statistics: {
            totalCourses: document.querySelector('.stat-courses .stat-number')?.textContent,
            totalEnrollments: document.querySelector('.stat-enrollments .stat-number')?.textContent,
            totalStudents: document.querySelector('.stat-students .stat-number')?.textContent,
            totalCategories: document.querySelector('.stat-categories .stat-number')?.textContent
        }
    };

    const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `dashboard-data-${new Date().toISOString().split('T')[0]}.json`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);

    showDashboardNotification('Dashboard data exported successfully!', 'success');
}

/**
 * Setup keyboard shortcuts
 */
function setupKeyboardShortcuts() {
    document.addEventListener('keydown', function(e) {
        // Ctrl/Cmd + R to refresh
        if ((e.ctrlKey || e.metaKey) && e.key === 'r') {
            e.preventDefault();
            refreshDashboardData();
        }
        
        // Ctrl/Cmd + E to export
        if ((e.ctrlKey || e.metaKey) && e.key === 'e') {
            e.preventDefault();
            exportDashboardData();
        }
    });
}

/**
 * Initialize real-time updates (placeholder)
 */
function initializeRealTimeUpdates() {
    // This is a placeholder for real-time data updates
    // You can implement WebSocket connections or polling here
    
    setInterval(() => {
        // Simulate real-time updates
        const randomStat = Math.floor(Math.random() * 4);
        const statCards = document.querySelectorAll('.stat-card');
        if (statCards[randomStat]) {
            const numberElement = statCards[randomStat].querySelector('.stat-number');
            if (numberElement) {
                const currentValue = parseInt(numberElement.textContent);
                const newValue = currentValue + Math.floor(Math.random() * 5);
                updateStatistic(numberElement.id, newValue, 500);
            }
        }
    }, 30000); // Update every 30 seconds
}

// Initialize additional features
setupKeyboardShortcuts();
initializeRealTimeUpdates();

// Export functions for global access
window.Dashboard = {
    updateStatistic,
    showDashboardNotification,
    refreshDashboardData,
    toggleSection,
    exportDashboardData
};

// Add CSS for additional animations
const additionalStyles = `
    @keyframes pulse {
        0% { transform: scale(1); }
        50% { transform: scale(1.1); }
        100% { transform: scale(1); }
    }
    
    .refresh-btn {
        transition: all 0.3s ease;
    }
    
    .refresh-btn:hover {
        transform: rotate(180deg);
    }
    
    .stat-card.loading {
        position: relative;
        overflow: hidden;
    }
    
    .stat-card.loading::after {
        content: '';
        position: absolute;
        top: 0;
        left: -100%;
        width: 100%;
        height: 100%;
        background: linear-gradient(90deg, transparent, rgba(255,255,255,0.4), transparent);
        animation: loading 1.5s infinite;
    }
    
    @keyframes loading {
        0% { left: -100%; }
        100% { left: 100%; }
    }
`;

// Inject additional styles
const styleSheet = document.createElement('style');
styleSheet.textContent = additionalStyles;
document.head.appendChild(styleSheet); 