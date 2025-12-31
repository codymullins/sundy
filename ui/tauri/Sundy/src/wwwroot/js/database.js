// Database utilities for OPFS-backed SQLite

window.databaseHelper = {
    // Delete the database file from OPFS (Origin Private File System)
    // This is a nuclear option when the database is corrupted beyond repair
    deleteDatabase: async function(dbName) {
        try {
            const root = await navigator.storage.getDirectory();

            // Try to delete the main database file
            try {
                await root.removeEntry(dbName);
                console.log(`Deleted ${dbName} from OPFS`);
            } catch (e) {
                console.log(`${dbName} not found or already deleted`);
            }

            // Also try common SQLite auxiliary files
            const auxFiles = [
                dbName + '-journal',
                dbName + '-wal',
                dbName + '-shm'
            ];

            for (const file of auxFiles) {
                try {
                    await root.removeEntry(file);
                    console.log(`Deleted ${file} from OPFS`);
                } catch (e) {
                    // Ignore - file may not exist
                }
            }

            return true;
        } catch (e) {
            console.error('Failed to delete database from OPFS:', e);
            return false;
        }
    },

    // Force reload the page after database deletion
    reloadPage: function() {
        window.location.reload();
    }
};
