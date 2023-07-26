export interface MigrationHandler {
    handleMigration: () => Promise<void>;
}