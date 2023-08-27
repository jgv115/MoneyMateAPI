import { CockroachDbTargetCategoryRepository } from "../repository/cockroachdb/cockroachdb_category_repository";
import { CockroachDbTargetTransactionRepository } from "../repository/cockroachdb/cockroachdb_transaction_repository";
import { CockroachDbTargetUserRepository } from "../repository/cockroachdb/cockroachdb_user_repository";
import { CockroachDbCategory } from "../repository/cockroachdb/model";
import { DynamoDbMoneyMateDbRepository } from "../repository/dynamodb/dynamodb_moneymate_repository";
import { DynamoDbCategory } from "../repository/dynamodb/model";
import { createLogger } from "../utils/logger";
import { CategoryMigrationHandler } from "./category_migration_handler"

describe("CategoryMigrationHandler", () => {
    const setupTests = () => {
        const mockSourceCategoryRepository: DynamoDbMoneyMateDbRepository = {
            query: jest.fn()
        };
        const mockTargetCategoryRepository: CockroachDbTargetCategoryRepository = {
            saveCategories: jest.fn(),
            getSubcategoryId: jest.fn()
        };
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
            mockTargetUserRepository,
            mockTargetTransactionRepository);

        return {
            sut,
            mocks: {
                mockSourceCategoryRepository,
                mockTargetCategoryRepository,
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

            await sut.handleMigration();

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
        })
    })
})