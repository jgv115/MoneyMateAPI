import { CockroachDbTargetCategoryRepository } from "../repository/cockroachdb/cockroachdb_category_repository";
import { CockroachDbTargetTransactionRepository } from "../repository/cockroachdb/cockroachdb_transaction_repository";
import { CockroachDbTargetUserRepository } from "../repository/cockroachdb/cockroachdb_user_repository";
import { CockroachDbCategory } from "../repository/cockroachdb/model";
import { DynamoDbMoneyMateDbRepository } from "../repository/dynamodb/dynamodb_moneymate_repository";
import { DynamoDbSourceUserRepository } from "../repository/dynamodb/dynamodb_user_repository";
import { DynamoDbCategory } from "../repository/dynamodb/model";
import { createLogger } from "../utils/logger";
import { CategoryMigrationHandler } from "./category_migration_handler"
import { MigrationResult } from "./migration_result";

describe("CategoryMigrationHandler", () => {
    const setupTests = () => {
        const mockSourceCategoryRepository: DynamoDbMoneyMateDbRepository = {
            query: jest.fn()
        };
        const mockTargetCategoryRepository: CockroachDbTargetCategoryRepository = {
            saveCategories: jest.fn(),
            getSubcategoryId: jest.fn()
        };
        const mockSourceUserRepository: DynamoDbSourceUserRepository = {
            getAllUserIdentifiers: jest.fn()
        }
        const mockTargetUserRepository: CockroachDbTargetUserRepository = {
            saveUsers: jest.fn(),
            getUserIdFromUserIdentifier: jest.fn(),
            getUserIdentifierToUserIdMap: jest.fn()
        };
        const mockTargetTransactionRepository: CockroachDbTargetTransactionRepository = {
            saveTransactions: jest.fn(),
            retrieveTransactionTypeIds: jest.fn()
        }

        const sut = CategoryMigrationHandler(createLogger(),
            mockSourceCategoryRepository,
            mockTargetCategoryRepository,
            mockSourceUserRepository,
            mockTargetUserRepository,
            mockTargetTransactionRepository);

        return {
            sut,
            mocks: {
                mockSourceCategoryRepository,
                mockTargetCategoryRepository,
                mockSourceUserRepository,
                mockTargetUserRepository,
                mockTargetTransactionRepository
            }
        }
    }
    describe("handleMigration", () => {

        test("Given categories from source repository and no errors, then correct categories are saved to target", async () => {
            const { sut, mocks } = setupTests();

            (mocks.mockTargetTransactionRepository.retrieveTransactionTypeIds as jest.Mock).mockReturnValue({
                "expense": "expense-id",
                "income": "income-id"
            });

            (mocks.mockTargetUserRepository.getUserIdentifierToUserIdMap as jest.Mock).mockReturnValue({
                "auth0|jgv115": "user-id1",
                "auth0|test": "user-id2"
            });

            (mocks.mockSourceUserRepository.getAllUserIdentifiers as jest.Mock).mockReturnValue(["auth0|jgv115", "auth0|test"]);

            (mocks.mockSourceCategoryRepository.query as jest.Mock<Promise<DynamoDbCategory[]>>)
                .mockReturnValue(Promise.resolve([
                    {
                        UserIdQuery: "auth0|jgv115#Categories",
                        Subquery: "Category name",
                        TransactionType: 0,
                        Subcategories: ["sub1", "sub2", "sub3", "sub4", "sub5"]
                    },
                    {
                        UserIdQuery: "auth0|jgv115#Categories",
                        Subquery: "Eating Out",
                        TransactionType: 0,
                        Subcategories: []
                    },
                    {
                        UserIdQuery: "auth0|test#Categories",
                        Subquery: "Eating Out",
                        TransactionType: 0,
                        Subcategories: ["sub5", "sub6"]
                    }
                ]));

            const migrationResult = await sut.handleMigration();

            expect(mocks.mockTargetCategoryRepository.saveCategories).toHaveBeenCalledWith([
                {
                    name: "Category name",
                    subcategories: ["sub1", "sub2", "sub3", "sub4", "sub5"],
                    transaction_type_id: "expense-id",
                    user_id: "user-id1"
                },
                {
                    name: "Eating Out",
                    subcategories: [],
                    transaction_type_id: "expense-id",
                    user_id: "user-id1"
                },
                {
                    name: "Eating Out",
                    subcategories: ["sub5", "sub6"],
                    transaction_type_id: "expense-id",
                    user_id: "user-id2"
                }
            ] satisfies CockroachDbCategory[])

            expect(migrationResult).toEqual({
                failedRecords: [],
                numberOfSuccessfullyMigratedRecords: 3
            } satisfies MigrationResult<DynamoDbCategory>)
        });

        test("given invalid transaction type in category then migration result returned with incorrect record", async () => {
            const { sut, mocks } = setupTests();

            (mocks.mockTargetTransactionRepository.retrieveTransactionTypeIds as jest.Mock).mockReturnValue({
                "expense": "expense-id",
                "income": "income-id"
            });

            (mocks.mockTargetUserRepository.getUserIdentifierToUserIdMap as jest.Mock).mockReturnValue({
                "auth0|jgv115": "user-id1",
                "auth0|test": "user-id2"
            });

            (mocks.mockSourceUserRepository.getAllUserIdentifiers as jest.Mock).mockReturnValue(["auth0|jgv115", "auth0|test"]);

            const incorrectRecord = {
                UserIdQuery: "auth0|test#Categories",
                Subquery: "Eating Out",
                TransactionType: 3,
                Subcategories: ["sub5", "sub6"]
            };
            (mocks.mockSourceCategoryRepository.query as jest.Mock<Promise<DynamoDbCategory[]>>)
                .mockReturnValue(Promise.resolve([
                    {
                        UserIdQuery: "auth0|jgv115#Categories",
                        Subquery: "Category name",
                        TransactionType: 0,
                        Subcategories: ["sub1", "sub2", "sub3", "sub4", "sub5"]
                    },
                    {
                        UserIdQuery: "auth0|jgv115#Categories",
                        Subquery: "Eating Out",
                        TransactionType: 0,
                        Subcategories: []
                    },
                    incorrectRecord
                ]));

            const migrationResult = await sut.handleMigration();

            expect(migrationResult).toEqual({
                failedRecords: [incorrectRecord],
                numberOfSuccessfullyMigratedRecords: 2
            } satisfies MigrationResult<DynamoDbCategory>)
        });

        test("given user identifier not in userIdentifierMap then migration result returned in incorrect result", async () => {
            const { sut, mocks } = setupTests();

            (mocks.mockTargetTransactionRepository.retrieveTransactionTypeIds as jest.Mock).mockReturnValue({
                "expense": "expense-id",
                "income": "income-id"
            });

            (mocks.mockTargetUserRepository.getUserIdentifierToUserIdMap as jest.Mock).mockReturnValue({
                "auth0|jgv115": "user-id1",
                "auth0|test": "user-id2"
            });

            (mocks.mockSourceUserRepository.getAllUserIdentifiers as jest.Mock).mockReturnValue(["auth0|jgv115", "auth0|test"]);

            const incorrectRecord = {
                UserIdQuery: "auth0|invalid_id#Categories",
                Subquery: "Eating Out",
                TransactionType: 0,
                Subcategories: ["sub5", "sub6"]
            };
            (mocks.mockSourceCategoryRepository.query as jest.Mock<Promise<DynamoDbCategory[]>>)
                .mockReturnValue(Promise.resolve([
                    {
                        UserIdQuery: "auth0|jgv115#Categories",
                        Subquery: "Category name",
                        TransactionType: 0,
                        Subcategories: ["sub1", "sub2", "sub3", "sub4", "sub5"]
                    },
                    {
                        UserIdQuery: "auth0|jgv115#Categories",
                        Subquery: "Eating Out",
                        TransactionType: 0,
                        Subcategories: []
                    },
                    incorrectRecord
                ]));

            const migrationResult = await sut.handleMigration();
            expect(migrationResult).toEqual({
                failedRecords: [incorrectRecord],
                numberOfSuccessfullyMigratedRecords: 2
            } satisfies MigrationResult<DynamoDbCategory>)
        });
    })
})