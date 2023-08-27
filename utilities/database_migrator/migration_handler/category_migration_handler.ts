import { Logger } from "winston";
import { MigrationHandler } from "./migration_handler";
import { DynamoDbMoneyMateDbRepository } from "../repository/dynamodb/dynamodb_moneymate_repository";
import { CockroachDbTargetCategoryRepository } from "../repository/cockroachdb/cockroachdb_category_repository";
import { DynamoDbCategoriesMapper } from "../repository/dynamodb/mappers";
import { CockroachDbTargetUserRepository } from "../repository/cockroachdb/cockroachdb_user_repository";
import { CockroachDbCategory } from "../repository/cockroachdb/model";
import { DynamoDbCategory } from "../repository/dynamodb/model";
import { CockroachDbTargetTransactionRepository } from "../repository/cockroachdb/cockroachdb_transaction_repository";


export const CategoryMigrationHandler = (
    logger: Logger,
    sourceCategoryRepository: DynamoDbMoneyMateDbRepository,
    targetCategoryRepository: CockroachDbTargetCategoryRepository,
    targetUserRepository: CockroachDbTargetUserRepository,
    targetTransactionRepository: CockroachDbTargetTransactionRepository
): MigrationHandler => {

    const handleMigration = async () => {
        logger.info("starting category migration")

        const transactionTypeIds = await targetTransactionRepository.retrieveTransactionTypeIds();
        const userIdentifierMap = await targetUserRepository.getUserIdentifierToUserIdMap();


        const categories = await sourceCategoryRepository.query("Categories", [], DynamoDbCategoriesMapper);

        const failedCategories: DynamoDbCategory[] = []
        const categoriesToBeSaved: CockroachDbCategory[] = []

        for (const category of categories) {
            const transactionTypeId = transactionTypeIds[category.TransactionType === 0 ? "expense" : "income"];

            if (!transactionTypeId) {
                logger.warn(`category could not be saved because transaction type id was not found`, { attemptedPayload: JSON.stringify(category) })
                failedCategories.push(category);
                continue;
            }

            // UserIdQuery: "auth0|jgv115#Transaction"
            const userId = userIdentifierMap[category.UserIdQuery.split("#")[0]]
            if (!userId) {
                failedCategories.push(category);
                continue;
            }

            categoriesToBeSaved.push({
                name: category.Subquery,
                transaction_type_id: transactionTypeId,
                user_id: userId,
                subcategories: category.Subcategories
            })
        }

        await targetCategoryRepository.saveCategories(categoriesToBeSaved);

        logger.info("category migration completed")
    }

    return {
        handleMigration
    }
}