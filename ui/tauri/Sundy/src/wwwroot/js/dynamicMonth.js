window.dynamicMonth = {
    dotNetRef: null,
    container: null,
    isInitialized: false,
    scrollThreshold: 200,
    weekRowHeight: 100,
    boundScrollHandler: null,
    isScrolling: false,
    scrollTimeout: null,

    init: function(dotNetRef, containerElement) {
        this.dotNetRef = dotNetRef;
        this.container = containerElement;
        this.isInitialized = true;

        this.boundScrollHandler = this.onScroll.bind(this);
        this.container.addEventListener('scroll', this.boundScrollHandler, { passive: true });

        // Calculate actual week row height
        this.updateWeekRowHeight();
    },

    dispose: function() {
        if (this.container && this.boundScrollHandler) {
            this.container.removeEventListener('scroll', this.boundScrollHandler);
        }
        if (this.scrollTimeout) {
            clearTimeout(this.scrollTimeout);
        }
        this.dotNetRef = null;
        this.container = null;
        this.isInitialized = false;
    },

    onScroll: function() {
        if (!this.container || !this.dotNetRef || this.isScrolling) return;

        const scrollTop = this.container.scrollTop;
        const scrollHeight = this.container.scrollHeight;
        const clientHeight = this.container.clientHeight;

        // Debounce scroll events
        if (this.scrollTimeout) {
            clearTimeout(this.scrollTimeout);
        }

        this.scrollTimeout = setTimeout(() => {
            // Check if near top (load more past weeks)
            if (scrollTop < this.scrollThreshold) {
                this.isScrolling = true;
                this.dotNetRef.invokeMethodAsync('OnScrollNearTop').finally(() => {
                    this.isScrolling = false;
                });
            }

            // Check if near bottom (load more future weeks)
            if (scrollTop + clientHeight > scrollHeight - this.scrollThreshold) {
                this.isScrolling = true;
                this.dotNetRef.invokeMethodAsync('OnScrollNearBottom').finally(() => {
                    this.isScrolling = false;
                });
            }
        }, 100);
    },

    scrollToToday: function() {
        if (!this.container) return;

        requestAnimationFrame(() => {
            const currentWeek = this.container.querySelector('.current-week');
            if (currentWeek) {
                // Update height calculation
                this.updateWeekRowHeight();

                // Get the position of the current week relative to the container
                const containerRect = this.container.getBoundingClientRect();
                const weekRect = currentWeek.getBoundingClientRect();
                const currentScrollTop = this.container.scrollTop;

                // Calculate scroll position to center the current week
                const weekOffsetFromTop = weekRect.top - containerRect.top + currentScrollTop;
                const centerOffset = (containerRect.height - weekRect.height) / 3;
                const targetScroll = weekOffsetFromTop - centerOffset;

                this.container.scrollTop = Math.max(0, targetScroll);
            }
        });
    },

    maintainScrollPosition: function(weeksAdded) {
        if (!this.container) return;

        // When prepending weeks, adjust scroll position to maintain visual stability
        this.updateWeekRowHeight();
        const additionalHeight = weeksAdded * this.weekRowHeight;
        this.container.scrollTop += additionalHeight;
    },

    updateWeekRowHeight: function() {
        if (!this.container) return;

        const firstRow = this.container.querySelector('.dynamic-week-row');
        if (firstRow) {
            this.weekRowHeight = firstRow.offsetHeight;
        }
    }
};
