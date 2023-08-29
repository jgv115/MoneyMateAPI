import { Logger } from "winston";
import { MigrationHandler } from "./migration_handler";
import { DynamoDbMoneyMateDbRepository } from "../repository/dynamodb/dynamodb_moneymate_repository";
import { CockroachDbTargetPayerPayeeRepository } from "../repository/cockroachdb/cockroachdb_payerpayee_repository";
import { CockroachDbTargetUserRepository } from "../repository/cockroachdb/cockroachdb_user_repository";
import { DynamoDbPayerPayee } from "../repository/dynamodb/model";
import { MigrationResult } from "./migration_result";
import { DynamoDbPayersPayeesMapper } from "../repository/dynamodb/mappers";
import { DynamoDbSourceUserRepository } from "../repository/dynamodb/dynamodb_user_repository";
import { CockroachDbPayerPayee } from "../repository/cockroachdb/model";

export const PayerPayeeMigrationHandler = (
    logger: Logger,
    sourcePayerPayeeRepository: DynamoDbMoneyMateDbRepository,
    targetPayerPayeeRepository: CockroachDbTargetPayerPayeeRepository,
    sourceUserRepository: DynamoDbSourceUserRepository,
    targetUserRepository: CockroachDbTargetUserRepository,

): MigrationHandler<DynamoDbPayerPayee> => {

    const handleMigration = async (): Promise<MigrationResult<DynamoDbPayerPayee>> => {
        logger.info("starting payerpayee migration");

        const userIdentifiers = await sourceUserRepository.getAllUserIdentifiers();
        const userIdentifierMap = await targetUserRepository.getUserIdentifierToUserIdMap();
        const externalLinkTypeIdMap = await targetPayerPayeeRepository.getExternalLinkTypeIds();
        const payerPayeeTypeMap = await targetPayerPayeeRepository.getPayerPayeeTypeIds();

        const payerPayees = await sourcePayerPayeeRepository.query("PayersPayees", userIdentifiers, DynamoDbPayersPayeesMapper);

        const failedPayerPayees: DynamoDbPayerPayee[] = [];
        const newPayerPayeeRecords: CockroachDbPayerPayee[] = []
        let numSavedPayerPayees = 0;

        for (const payerPayee of payerPayees) {

            const userId = userIdentifierMap[payerPayee.UserIdQuery.split("#")[0]];
            if (!userId) {
                logger.warn("payerpayee could not be saved because user identifier was not found", { attemptedPayload: JSON.stringify(payerPayee) });
                failedPayerPayees.push(payerPayee);
                continue;
            }

            const payerPayeeType = payerPayee.Subquery.split("#")[0];
            const payerPayeeTypeId = payerPayeeTypeMap[payerPayeeType];
            if (!payerPayeeTypeId) {
                logger.warn("payerpayee could not be saved because payerPayeeId was not found", { attemptedPayload: JSON.stringify(payerPayee) });
                failedPayerPayees.push(payerPayee);
                continue;
            }

            newPayerPayeeRecords.push({
                // payee#004fe4d2-5f30-4329-b84a-ce786bd367ab
                id: payerPayee.Subquery.split("#")[1],
                user_id: userId,
                name: payerPayee.PayerPayeeName,
                payerpayeetype_id: payerPayeeTypeId,
                external_link_type_id: payerPayee.ExternalId ? externalLinkTypeIdMap.Google : externalLinkTypeIdMap.Custom,
                external_link_id: payerPayee.ExternalId
            });

            numSavedPayerPayees++;
        }

        await targetPayerPayeeRepository.savePayerPayees(newPayerPayeeRecords);

        logger.info("payerpayee migration completed", { numFailedRecords: failedPayerPayees.length, numSuccessfulRecords: numSavedPayerPayees })

        return {
            failedRecords: failedPayerPayees,
            numberOfSuccessfullyMigratedRecords: numSavedPayerPayees
        }
    };

    return {
        handleMigration
    }
}