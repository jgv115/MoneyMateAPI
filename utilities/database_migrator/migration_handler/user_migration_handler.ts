import { Logger } from "winston";
import { SourceUserRepository } from "../repository/source_user_repository";
import { TargetUserRepository } from "../repository/target_user_repository";
import { MigrationHandler } from "./migration_handler";

export const UserMigrationHandler = (
    logger: Logger,
    sourceUserRepository: SourceUserRepository,
    targetUserRepository: TargetUserRepository
): MigrationHandler => {

    const handleMigration = async () => {
        logger.info("starting user migration")

        const sourceUserIdentifiers = await sourceUserRepository.getAllUserIdentifiers();

        await targetUserRepository.saveUsers(sourceUserIdentifiers);

        logger.info("user migration completed")
    }

    return {
        handleMigration
    }
}