import { randomUUID } from "crypto";
import { CockroachDbTargetCategoryRepository } from "../repository/cockroachdb/cockroachdb_category_repository";
import { CockroachDbTargetTransactionRepository } from "../repository/cockroachdb/cockroachdb_transaction_repository";
import { CockroachDbTargetUserRepository } from "../repository/cockroachdb/cockroachdb_user_repository";
import { DynamoDbMoneyMateDbRepository } from "../repository/dynamodb/dynamodb_moneymate_repository";
import { DynamoDbSourceUserRepository } from "../repository/dynamodb/dynamodb_user_repository";
import { DynamoDbTransaction } from "../repository/dynamodb/model";
import { createLogger } from "../utils/logger";
import { TransactionMigrationHandler } from "./transaction_migration_handler";
import { MigrationResult } from "./migration_result";
import { CockroachDbTransaction } from "../repository/cockroachdb/model";

describe("TransactionMigrationHandler", () => {

    const setupTests = () => {
        const mockSourceTransactionRepository: DynamoDbMoneyMateDbRepository = {
            query: jest.fn()
        };
        const mockTargetTransactionRepository: CockroachDbTargetTransactionRepository = {
            saveTransactions: jest.fn(),
            retrieveTransactionTypeIds: jest.fn()
        }
        const mockTargetCategoryRepository: CockroachDbTargetCategoryRepository = {
            saveCategories: jest.fn(),
            getSubcategoryIdWithCategoryId: jest.fn(),
            getSubcategoryIdByCategoryAndSubcategoryName: jest.fn()
        };
        const mockSourceUserRepository: DynamoDbSourceUserRepository = {
            getAllUserIdentifiers: jest.fn()
        }
        const mockTargetUserRepository: CockroachDbTargetUserRepository = {
            saveUsers: jest.fn(),
            getUserIdFromUserIdentifier: jest.fn(),
            getUserIdentifierToUserIdMap: jest.fn()
        };

        const sut = TransactionMigrationHandler(
            createLogger(),
            mockSourceTransactionRepository,
            mockTargetTransactionRepository,
            mockSourceUserRepository,
            mockTargetUserRepository,
            mockTargetCategoryRepository
        );

        return {
            sut,
            mocks: {
                mockSourceTransactionRepository,
                mockTargetTransactionRepository,
                mockSourceUserRepository,
                mockTargetUserRepository,
                mockTargetCategoryRepository
            }
        }
    };

    describe("handleMigration", () => {
        test("given transaction from source repository and no errors then correct transactions migrated", async () => {
            const { sut, mocks } = setupTests();

            (mocks.mockSourceUserRepository.getAllUserIdentifiers as jest.Mock).mockReturnValue(["auth0|jgv115", "auth0|test"]);

            (mocks.mockTargetUserRepository.getUserIdentifierToUserIdMap as jest.Mock).mockReturnValue({
                "auth0|jgv115": "user-id1",
                "auth0|test": "user-id2",
                "test1": "user-id3",
                "test2": "user-id4"
            });

            (mocks.mockTargetTransactionRepository.retrieveTransactionTypeIds as jest.Mock).mockReturnValue({
                "expense": "expense-id",
                "income": "income-id"
            });

            const transaction1: DynamoDbTransaction = {
                UserIdQuery: 'auth0|jgv115#Transaction',
                Subquery: "fa00567c-468e-4ccf-af4c-fca1c731915c",
                TransactionTimestamp: '2023-07-01T10:59:00.000Z',
                TransactionType: "income",
                Amount: 123.45,
                Category: "category1",
                SubCategory: "subcategory1",
                PayerPayeeId: randomUUID(),
                PayerPayeeName: "payer1",
                Note: ""
            };
            const transaction2: DynamoDbTransaction = {
                UserIdQuery: 'auth0|jgv115#Transaction',
                Subquery: "fa00567c-468e-4ccf-af4c-fca1c731915d",
                TransactionTimestamp: '2023-08-01T10:59:00.000Z',
                TransactionType: "expense",
                Amount: 1232.45,
                Category: "category2",
                SubCategory: "subcategory2",
                PayerPayeeId: randomUUID(),
                PayerPayeeName: "payee2",
                Note: "note"
            };
            const transaction3: DynamoDbTransaction = {
                UserIdQuery: 'test1#Transaction',
                Subquery: "fa00567c-468e-4ccf-af4c-fca1c731915e",
                TransactionTimestamp: '2023-07-01T10:59:00.000Z',
                TransactionType: "expense",
                Amount: 123.45,
                Category: "category1",
                SubCategory: "subcategory1",
                PayerPayeeId: randomUUID(),
                PayerPayeeName: "payer1",
                Note: "note"
            };
            (mocks.mockSourceTransactionRepository.query as jest.Mock<Promise<DynamoDbTransaction[]>>).mockReturnValue(
                Promise.resolve([
                    transaction1, transaction2, transaction3
                ])
            );

            (mocks.mockTargetCategoryRepository.getSubcategoryIdByCategoryAndSubcategoryName as jest.Mock).mockReturnValue("subcategory-id");

            const migrationResponse = await sut.handleMigration();

            expect(migrationResponse).toEqual({
                failedRecords: [],
                numberOfSuccessfullyMigratedRecords: 3
            } satisfies MigrationResult<DynamoDbTransaction>);

            expect(mocks.mockTargetTransactionRepository.saveTransactions).toHaveBeenCalledWith([
                {
                    id: transaction1.Subquery,
                    user_id: "user-id1",
                    transaction_timestamp: transaction1.TransactionTimestamp,
                    transaction_type_id: "income-id",
                    amount: transaction1.Amount,
                    subcategory_id: "subcategory-id",
                    payerpayee_id: transaction1.PayerPayeeId,
                    notes: transaction1.Note
                },
                {
                    id: transaction2.Subquery,
                    user_id: "user-id1",
                    transaction_timestamp: transaction2.TransactionTimestamp,
                    transaction_type_id: "expense-id",
                    amount: transaction2.Amount,
                    subcategory_id: "subcategory-id",
                    payerpayee_id: transaction2.PayerPayeeId,
                    notes: transaction2.Note
                },
                {
                    id: transaction3.Subquery,
                    user_id: "user-id3",
                    transaction_timestamp: transaction3.TransactionTimestamp,
                    transaction_type_id: "expense-id",
                    amount: transaction3.Amount,
                    subcategory_id: "subcategory-id",
                    payerpayee_id: transaction3.PayerPayeeId,
                    notes: transaction3.Note
                }
            ] satisfies CockroachDbTransaction[]);
        })
    })
})