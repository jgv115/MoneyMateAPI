export interface MigrationResult<T> {
    numberOfSuccessfullyMigratedRecords: number;
    failedRecords: T[];
}