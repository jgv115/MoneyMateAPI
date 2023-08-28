import { MigrationResult } from "./migration_result";

export interface MigrationHandler<T> {
    handleMigration: () => Promise<MigrationResult<T>>;
}