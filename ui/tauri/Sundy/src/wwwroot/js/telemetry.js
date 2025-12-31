// Telemetry preference helper using localStorage
// This MUST be loaded before Sentry initializes to properly gate telemetry

window.telemetryHelper = {
    STORAGE_KEY: 'sundy_telemetry_enabled',

    // Check if telemetry is enabled (synchronous, for Sentry init)
    isEnabled: function() {
        try {
            var value = localStorage.getItem(this.STORAGE_KEY);
            // Default to false (opt-in) - only enabled if explicitly "true"
            return value === 'true';
        } catch (e) {
            console.warn('Could not read telemetry preference:', e);
            return false;
        }
    },

    // Set telemetry preference (called from Blazor)
    setEnabled: function(enabled) {
        try {
            localStorage.setItem(this.STORAGE_KEY, enabled ? 'true' : 'false');
            return true;
        } catch (e) {
            console.error('Could not save telemetry preference:', e);
            return false;
        }
    },

    // Get current value (for Blazor to read)
    getEnabled: function() {
        return this.isEnabled();
    }
};

// Expose for immediate access before Blazor loads
window.isTelemetryEnabled = function() {
    return window.telemetryHelper.isEnabled();
};
