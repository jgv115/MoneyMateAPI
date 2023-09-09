import { Logger } from "winston";
import { MigrationHandler } from "./migration_handler";
import { DynamoDbMoneyMateDbRepository } from "../repository/dynamodb/dynamodb_moneymate_repository";
import { CockroachDbTargetCategoryRepository } from "../repository/cockroachdb/cockroachdb_category_repository";
import { DynamoDbCategoriesMapper } from "../repository/dynamodb/mappers";
import { CockroachDbTargetUserRepository } from "../repository/cockroachdb/cockroachdb_user_repository";
import { CockroachDbCategory } from "../repository/cockroachdb/model";
import { DynamoDbCategory } from "../repository/dynamodb/model";
import { CockroachDbTargetTransactionRepository } from "../repository/cockroachdb/cockroachdb_transaction_repository";
import { DynamoDbSourceUserRepository } from "../repository/dynamodb/dynamodb_user_repository";
import { MigrationResult } from "./migration_result";


export const CategoryMigrationHandler = (
    logger: Logger,
    sourceCategoryRepository: DynamoDbMoneyMateDbRepository,
    targetCategoryRepository: CockroachDbTargetCategoryRepository,
    sourceUserRepository: DynamoDbSourceUserRepository,
    targetUserRepository: CockroachDbTargetUserRepository,
    targetTransactionRepository: CockroachDbTargetTransactionRepository
): MigrationHandler<DynamoDbCategory> => {

    const handleMigration = async (): Promise<MigrationResult<DynamoDbCategory>> => {
        logger.info("starting category migration")

        const transactionTypeIds = await targetTransactionRepository.retrieveTransactionTypeIds();
        const userIdentifierMap = await targetUserRepository.getUserIdentifierToUserIdMap();

        const userIdentifiers = await sourceUserRepository.getAllUserIdentifiers();
        const categories = await sourceCategoryRepository.query("Categories", userIdentifiers, DynamoDbCategoriesMapper);

        logger.info("attempting to migrate categories", { numCategories: categories.length });

        const failedCategories: DynamoDbCategory[] = []
        let numSavedCategories = 0;

        for (const category of categories) {
            let transactionTypeId: string;
            switch (category.TransactionType) {
                case 0: {
                    transactionTypeId = transactionTypeIds.expense;
                    break;
                }
                case 1: {
                    transactionTypeId = transactionTypeIds.income;
                    break;
                }
                default: {
                    logger.error(`category could not be saved because transaction type id was not found`, { attemptedPayload: JSON.stringify(category) })
                    failedCategories.push(category);
                    continue;
                }
            }

            if (!transactionTypeId) {
                logger.error(`category could not be saved because transaction type id was not found`, { attemptedPayload: JSON.stringify(category) })
                failedCategories.push(category);
                continue;
            }

            // UserIdQuery: "auth0|jgv115#Transaction"
            const userId = userIdentifierMap[category.UserIdQuery.split("#")[0]]
            if (!userId) {
                logger.error(`category could not be saved because user identifier was not found`, { attemptedPayload: JSON.stringify(category) })
                failedCategories.push(category);
                continue;
            }

            try {
                await targetCategoryRepository.saveCategory({
                    name: category.Subquery,
                    transaction_type_id: transactionTypeId,
                    user_id: userId,
                    subcategories: category.Subcategories
                });
                numSavedCategories++;
            }
            catch (ex) {
                logger.error("category could be not be saved because an error was encountered when attempting to persist", { ex, attemptedPayload: JSON.stringify(category) })
                failedCategories.push(category);
            }

        }

        logger.info("category migration completed", { numFailedRecords: failedCategories.length, numSuccessfulRecords: numSavedCategories })

        return {
            failedRecords: failedCategories,
            numberOfSuccessfullyMigratedRecords: numSavedCategories

        }
    }

    return {
        handleMigration
    }
}