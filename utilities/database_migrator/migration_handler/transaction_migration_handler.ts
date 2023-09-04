import { Logger } from "winston";
import { DynamoDbMoneyMateDbRepository } from "../repository/dynamodb/dynamodb_moneymate_repository";
import { CockroachDbTargetTransactionRepository } from "../repository/cockroachdb/cockroachdb_transaction_repository";
import { DynamoDbSourceUserRepository } from "../repository/dynamodb/dynamodb_user_repository";
import { CockroachDbTargetUserRepository } from "../repository/cockroachdb/cockroachdb_user_repository";
import { DynamoDbTransaction } from "../repository/dynamodb/model";
import { MigrationResult } from "./migration_result";
import { MigrationHandler } from "./migration_handler";
import { DynamoDbTransactionMapper } from "../repository/dynamodb/mappers";
import { CockroachDbTransaction } from "../repository/cockroachdb/model";
import { CockroachDbTargetPayerPayeeRepository } from "../repository/cockroachdb/cockroachdb_payerpayee_repository";
import { CockroachDbTargetCategoryRepository } from "../repository/cockroachdb/cockroachdb_category_repository";

export const TransactionMigrationHandler = (
    logger: Logger,
    sourceTransactionRepository: DynamoDbMoneyMateDbRepository,
    targetTransactionRepository: CockroachDbTargetTransactionRepository,
    sourceUserRepository: DynamoDbSourceUserRepository,
    targetUserRepository: CockroachDbTargetUserRepository,
    targetCategoryRepository: CockroachDbTargetCategoryRepository
): MigrationHandler<DynamoDbTransaction> => {
    const handleMigration = async (): Promise<MigrationResult<DynamoDbTransaction>> => {
        logger.info("starting transaction migration");

        const userIdentifiers = await sourceUserRepository.getAllUserIdentifiers();
        const userIdentifierMap = await targetUserRepository.getUserIdentifierToUserIdMap();

        const transactionTypeIds = await targetTransactionRepository.retrieveTransactionTypeIds();


        const sourceTransactions = await sourceTransactionRepository.query("Transaction", userIdentifiers, DynamoDbTransactionMapper);

        const failedTransactions: DynamoDbTransaction[] = [];
        const transactionsToBeSaved: CockroachDbTransaction[] = [];
        let numSavedTransactions = 0;

        for (const transaction of sourceTransactions) {
            const userId = userIdentifierMap[transaction.UserIdQuery.split("#")[0]];
            if (!userId) {
                logger.warn("transaction could not be saved because user identifier was not found", { attemptedPayload: JSON.stringify(transaction) });
                failedTransactions.push(transaction);
                continue;
            }

            const transactionTypeId = transaction.TransactionType == "expense" ? transactionTypeIds.expense : transactionTypeIds.income;
            const subcategoryId = await targetCategoryRepository.getSubcategoryIdByCategoryAndSubcategoryName(userId,
                transaction.Category,
                transaction.SubCategory,
                transactionTypeId)

            if (!subcategoryId) {
                logger.warn("transaction could not be saved because subcategoryId could not be found", { attemptedPayload: JSON.stringify(transaction) });
                failedTransactions.push(transaction);
                continue;
            }


            transactionsToBeSaved.push({
                id: transaction.Subquery,
                amount: transaction.Amount,
                notes: transaction.Note,
                payerpayee_id: transaction.PayerPayeeId,
                subcategory_id: subcategoryId,
                transaction_timestamp: transaction.TransactionTimestamp,
                transaction_type_id: transactionTypeId,
                user_id: userIdentifierMap[transaction.UserIdQuery.split("#")[0]]
            });

            numSavedTransactions++;
        }

        await targetTransactionRepository.saveTransactions(transactionsToBeSaved);


        return {
            failedRecords: failedTransactions,
            numberOfSuccessfullyMigratedRecords: numSavedTransactions
        }
    };

    return {
        handleMigration
    }
}