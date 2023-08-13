import { CockroachDbTransactionRepository } from "./cockroachdb_transaction_repository";
import { CockroachDbTestHelper } from "../../utils/cockroachDbTestHelper";
import { CockroachDbTargetPayerPayeeRepository } from "./cockroachdb_payerpayee_repository";
import { CockroachDbTargetCategoryRepository } from "./cockroachdb_category_repository";
import { CockroachDbTargetUserRepository } from "./cockroachdb_user_repository";
import { randomUUID } from "crypto";
import { PayerOrPayee, TransactionTypes } from "./constants";
import { Transaction } from "./model";
import { createLogger } from "../../utils/logger";

const cockroachDbTestHelper = CockroachDbTestHelper();

const setupTest = async () => {
    const stubLogger = createLogger();
    const sut = CockroachDbTransactionRepository(stubLogger, cockroachDbTestHelper.cockroachDbConnection);
    const payerPayeeRepo = CockroachDbTargetPayerPayeeRepository(stubLogger, cockroachDbTestHelper.cockroachDbConnection);
    const categoryRepo = CockroachDbTargetCategoryRepository(stubLogger, cockroachDbTestHelper.cockroachDbConnection);
    const userRepo = CockroachDbTargetUserRepository(stubLogger, cockroachDbTestHelper.cockroachDbConnection);

    const transactionTypeIds = await sut.retrieveTransactionTypeIds();
    const externalLinkTypeIds = await payerPayeeRepo.getExternalLinkTypeIds();
    const payerPayeeTypeIds = await payerPayeeRepo.getPayerPayeeTypeIds();

    const savedUserId = (await userRepo.saveUsers(["testUser123"]))[0];

    const savePayeePayee = async ({ name, payerPayee }: { name: string, payerPayee: PayerOrPayee }): Promise<string> => {
        const savedPayerPayeeIds = await payerPayeeRepo.savePayerPayees([{
            id: randomUUID(),
            name: name,
            user_id: savedUserId,
            payerpayeetype_id: payerPayeeTypeIds[payerPayee],
            external_link_type_id: externalLinkTypeIds["Custom"],
            external_link_id: ""
        }]);

        return savedPayerPayeeIds[0];
    }

    const saveCategoryAndSubcategory = async ({
        categoryName,
        subcategoryName,
        transactionType
    }: {
        categoryName: string,
        subcategoryName: string,
        transactionType: TransactionTypes
    }): Promise<{ categoryId: string, subcategoryId: string }> => {
        const savedCategoryId = (await categoryRepo.saveCategories([{
            name: categoryName,
            subcategories: [subcategoryName],
            transactionType: transactionType,
            userId: savedUserId
        }]))[0];

        const savedSubcategoryId = await categoryRepo.getSubcategoryId(savedUserId, savedCategoryId, subcategoryName);

        return {
            categoryId: savedCategoryId,
            subcategoryId: savedSubcategoryId
        }
    }

    return {
        sut,
        savedUserId: savedUserId,
        transactionTypeIds,
        transactionTestHelpers: {
            savePayeePayee,
            saveCategoryAndSubcategory
        }
    };
};

describe("CockroachDbTransactionRepository", () => {
    afterEach(async () => {
        await cockroachDbTestHelper.cleanUp();
    });

    afterAll(async () => {
        await cockroachDbTestHelper.terminateConnection();
    });

    describe("retrieveTransactionTypeIds", () => {
        test("returns correct transactionTypeIds", async () => {
            const { sut } = await setupTest();

            const transactionTypes = await cockroachDbTestHelper.performAdhocQuery<{ id: string, name: string }>('SELECT * FROM transactiontype', []);

            const response = await sut.retrieveTransactionTypeIds();

            const expectedTransactionTypes = {};

            transactionTypes.forEach(type => {
                expectedTransactionTypes[type.name] = type.id
            });

            expect(response).toEqual(expectedTransactionTypes);
        });

    });

    describe("saveTransactions", () => {
        test("given transactions then transactions written to db correctly", async () => {

            const { sut, savedUserId, transactionTypeIds, transactionTestHelpers } = await setupTest();

            const payerPayeeId = await transactionTestHelpers.savePayeePayee({ name: "payee1", payerPayee: "payee" });
            const subcategoryId = (await transactionTestHelpers.saveCategoryAndSubcategory({
                categoryName: "test category1",
                subcategoryName: "sub1",
                transactionType: "expense"
            })).subcategoryId

            const savedTransaction = {
                id: randomUUID(),
                amount: 123.23,
                payerpayee_id: payerPayeeId,
                subcategory_id: subcategoryId,
                transaction_timestamp: "2023-08-12T15:30:00.000Z",
                transaction_type_id: transactionTypeIds.expense,
                user_id: savedUserId,
                notes: "hello note"
            };

            const savedTransactionIds = await sut.saveTransactions([savedTransaction]);
            expect(savedTransactionIds).toEqual([savedTransaction.id])

            const retrievedTransactions = await cockroachDbTestHelper.performAdhocQuery<Transaction>(`SELECT * FROM transaction`, []);
            const expectedTransaction = {
                ...savedTransaction,
                transaction_timestamp: new Date(savedTransaction.transaction_timestamp),
                amount: savedTransaction.amount.toString()
            }
            expect(retrievedTransactions).toEqual([expectedTransaction]);
        });
    });
})