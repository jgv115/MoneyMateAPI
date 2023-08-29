import { Logger } from "winston";
import { MigrationHandler } from "./migration_handler";
import { DynamoDbSourceUserRepository } from "../repository/dynamodb/dynamodb_user_repository";
import { CockroachDbTargetUserRepository } from "../repository/cockroachdb/cockroachdb_user_repository";
import { MigrationResult } from "./migration_result";


export const UserMigrationHandler = (
    logger: Logger,
    sourceUserRepository: DynamoDbSourceUserRepository,
    targetUserRepository: CockroachDbTargetUserRepository
): MigrationHandler<string> => {

    const handleMigration = async (): Promise<MigrationResult<string>> => {
        logger.info("starting user migration")

        const sourceUserIdentifiers = await sourceUserRepository.getAllUserIdentifiers();

        const migratedUsers = await targetUserRepository.saveUsers(sourceUserIdentifiers);

        logger.info("user migration completed", { numberOfMigratedUsers: migratedUsers.length });

        return {
            failedRecords: [],
            numberOfSuccessfullyMigratedRecords: migratedUsers.length
        }
    }

    return {
        handleMigration
    }
}