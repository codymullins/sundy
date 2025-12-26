// View toggle slider
window.viewSlider = {
    update: function(activeIndex) {
        const toggle = document.querySelector('.view-toggle');
        const slider = document.querySelector('.view-slider');
        const buttons = toggle?.querySelectorAll('.view-btn');

        if (!toggle || !slider || !buttons || !buttons[activeIndex]) return;

        const btn = buttons[activeIndex];
        const toggleRect = toggle.getBoundingClientRect();
        const btnRect = btn.getBoundingClientRect();

        const left = btnRect.left - toggleRect.left;
        const width = btnRect.width;

        slider.style.transform = `translateX(${left}px)`;
        slider.style.width = `${width}px`;
    }
};

// Scroll scheduler to position
window.schedulerScroll = {
    scrollToTime: function(topPosition) {
        // Use requestAnimationFrame to ensure DOM is ready
        requestAnimationFrame(() => {
            const grid = document.querySelector('.scheduler-time-grid');
            if (!grid) return;

            // Calculate scroll position to center the time block in view
            const gridHeight = grid.clientHeight;
            const scrollTarget = Math.max(0, topPosition - (gridHeight / 3));

            grid.scrollTop = scrollTarget;
        });
    }
};

// Scheduler drag functionality
window.schedulerDrag = {
    dotNetRef: null,
    isDragging: false,
    dragType: null, // 'move', 'top', 'bottom'
    startY: 0,
    startTop: 0,
    startHeight: 0,
    hourHeight: 48,
    snapIncrement: 15, // minutes (15-minute increments)
    snapHeight: 12, // 48px / 4 = 12px per 15 minutes
    minHour: 0,
    maxHour: 24,
    boundMouseMove: null,
    boundMouseUp: null,
    boundTouchMove: null,
    boundTouchEnd: null,

    init: function(dotNetReference) {
        this.dotNetRef = dotNetReference;

        // Create bound functions for proper removal
        this.boundMouseMove = this.onMouseMove.bind(this);
        this.boundMouseUp = this.onMouseUp.bind(this);
        this.boundTouchMove = this.onTouchMove.bind(this);
        this.boundTouchEnd = this.onTouchEnd.bind(this);

        // Add global mouse event listeners
        document.addEventListener('mousemove', this.boundMouseMove);
        document.addEventListener('mouseup', this.boundMouseUp);
        document.addEventListener('touchmove', this.boundTouchMove, { passive: false });
        document.addEventListener('touchend', this.boundTouchEnd);
    },

    dispose: function() {
        if (this.boundMouseMove) {
            document.removeEventListener('mousemove', this.boundMouseMove);
            document.removeEventListener('mouseup', this.boundMouseUp);
            document.removeEventListener('touchmove', this.boundTouchMove);
            document.removeEventListener('touchend', this.boundTouchEnd);
        }
        this.dotNetRef = null;
    },

    startDrag: function(type, event) {
        const indicator = document.querySelector('.time-range-indicator');
        if (!indicator) return;

        this.isDragging = true;
        this.dragType = type;
        this.startY = event.clientY || (event.touches && event.touches[0].clientY);
        this.startTop = indicator.offsetTop;
        this.startHeight = indicator.offsetHeight;

        // Prevent text selection during drag
        document.body.style.cursor = type === 'move' ? 'grabbing' : 'ns-resize';
        document.body.style.userSelect = 'none';
        document.body.style.webkitUserSelect = 'none';
        document.body.style.mozUserSelect = 'none';
        document.body.style.msUserSelect = 'none';

        // Add class to scheduler for additional selection prevention
        const grid = document.querySelector('.scheduler-time-grid');
        if (grid) grid.classList.add('dragging');

        indicator.classList.add('dragging');
    },

    onMouseMove: function(event) {
        if (!this.isDragging) return;
        this.handleDrag(event.clientY);
    },

    onTouchMove: function(event) {
        if (!this.isDragging) return;
        event.preventDefault();
        this.handleDrag(event.touches[0].clientY);
    },

    handleDrag: function(clientY) {
        const deltaY = clientY - this.startY;
        const indicator = document.querySelector('.time-range-indicator');
        const grid = document.querySelector('.scheduler-time-grid');

        if (!indicator || !grid) return;

        // Use 15-minute snap increments (12px per 15 minutes)
        const snap = this.snapHeight;
        const minHeight = snap; // Minimum 15 minutes

        if (this.dragType === 'move') {
            // Move the entire range
            let newTop = this.startTop + deltaY;

            // Snap to 15-minute increments
            newTop = Math.round(newTop / snap) * snap;

            // Constrain within bounds
            const minTop = 0;
            const maxTop = (this.maxHour * this.hourHeight) - this.startHeight;
            newTop = Math.max(minTop, Math.min(newTop, maxTop));

            indicator.style.top = newTop + 'px';

        } else if (this.dragType === 'top') {
            // Resize from top
            let newTop = this.startTop + deltaY;
            let newHeight = this.startHeight - deltaY;

            // Snap to 15-minute increments
            newTop = Math.round(newTop / snap) * snap;
            newHeight = this.startTop + this.startHeight - newTop;

            // Minimum 15 minutes
            if (newHeight < minHeight) {
                newHeight = minHeight;
                newTop = this.startTop + this.startHeight - minHeight;
            }

            // Constrain within bounds
            if (newTop < 0) {
                newTop = 0;
                newHeight = this.startTop + this.startHeight;
            }

            indicator.style.top = newTop + 'px';
            indicator.style.height = newHeight + 'px';

        } else if (this.dragType === 'bottom') {
            // Resize from bottom
            let newHeight = this.startHeight + deltaY;

            // Snap to 15-minute increments
            newHeight = Math.round(newHeight / snap) * snap;

            // Minimum 15 minutes
            newHeight = Math.max(minHeight, newHeight);

            // Constrain within bounds
            const maxHeight = (this.maxHour * this.hourHeight) - this.startTop;
            newHeight = Math.min(newHeight, maxHeight);

            indicator.style.height = newHeight + 'px';
        }

        // Update the display text
        this.updateRangeText(indicator);
    },

    updateRangeText: function(indicator) {
        const top = parseInt(indicator.style.top) || indicator.offsetTop;
        const height = parseInt(indicator.style.height) || indicator.offsetHeight;

        // Calculate time in minutes (12px = 15 minutes, 48px = 60 minutes)
        const startMinutes = Math.round(top / this.snapHeight) * this.snapIncrement;
        const endMinutes = Math.round((top + height) / this.snapHeight) * this.snapIncrement;

        const rangeContent = indicator.querySelector('.range-content span:first-child');
        const durationSpan = indicator.querySelector('.range-duration');

        if (rangeContent) {
            rangeContent.textContent = this.formatTimeRange(startMinutes, endMinutes);
        }
        if (durationSpan) {
            durationSpan.textContent = this.formatDuration(endMinutes - startMinutes);
        }
    },

    formatTimeRange: function(startMinutes, endMinutes) {
        const formatTime = (totalMinutes) => {
            const hours = Math.floor(totalMinutes / 60) % 24;
            const minutes = totalMinutes % 60;
            const ampm = hours < 12 ? 'AM' : 'PM';
            const displayHour = hours === 0 ? 12 : (hours > 12 ? hours - 12 : hours);
            return displayHour + ':' + minutes.toString().padStart(2, '0') + ' ' + ampm;
        };
        return formatTime(startMinutes) + ' → ' + formatTime(endMinutes);
    },

    formatDuration: function(minutes) {
        const hours = Math.floor(minutes / 60);
        const mins = minutes % 60;

        if (hours === 0) {
            return mins + ' min';
        } else if (mins === 0) {
            return hours === 1 ? '1 hr' : hours + ' hrs';
        } else {
            return hours + ' hr ' + mins + ' min';
        }
    },

    onMouseUp: function(event) {
        this.endDrag();
    },

    onTouchEnd: function(event) {
        this.endDrag();
    },

    endDrag: function() {
        if (!this.isDragging) return;

        this.isDragging = false;

        // Clean up styles
        document.body.style.cursor = '';
        document.body.style.userSelect = '';
        document.body.style.webkitUserSelect = '';
        document.body.style.mozUserSelect = '';
        document.body.style.msUserSelect = '';

        // Remove dragging class from grid
        const grid = document.querySelector('.scheduler-time-grid');
        if (grid) grid.classList.remove('dragging');

        const indicator = document.querySelector('.time-range-indicator');
        if (indicator) {
            indicator.classList.remove('dragging');

            // Calculate final time in minutes and notify Blazor
            const top = parseInt(indicator.style.top) || indicator.offsetTop;
            const height = parseInt(indicator.style.height) || indicator.offsetHeight;

            const startMinutes = Math.round(top / this.snapHeight) * this.snapIncrement;
            const endMinutes = Math.round((top + height) / this.snapHeight) * this.snapIncrement;

            // Convert to hours and minutes for Blazor
            const startHour = Math.floor(startMinutes / 60);
            const startMin = startMinutes % 60;
            const endHour = Math.floor(endMinutes / 60) % 24;
            const endMin = endMinutes % 60;

            if (this.dotNetRef) {
                this.dotNetRef.invokeMethodAsync('OnTimeRangeChanged', startHour, startMin, endHour, endMin);
            }
        }

        this.dragType = null;
    }
};
