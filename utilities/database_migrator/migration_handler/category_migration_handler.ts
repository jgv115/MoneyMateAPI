import { Logger } from "winston";
import { MigrationHandler } from "./migration_handler";
import { TargetCategoryRepository } from "../repository/target_category_repository";
import { SourceCategoryRepository } from "../repository/source_category_repository";
import { TargetUserRepository } from "../repository/target_user_repository";

export const CategoryMigrationHandler = (
    logger: Logger,
    sourceCategoryRepository: SourceCategoryRepository,
    targetCategoryRepository: TargetCategoryRepository,
    targetUserRepository: TargetUserRepository
): MigrationHandler => {

    const handleMigration = async () => {
        logger.info("starting category migration")

        const categories = await sourceCategoryRepository.getAllCategories()

        for (const category of categories) {
            const userId = await targetUserRepository.getUserIdFromUserIdentifier(category.userIdentifier);
            await targetCategoryRepository.saveCategories(userId, [category])
        }

        logger.info("category migration completed")
    }

    return {
        handleMigration
    }
}