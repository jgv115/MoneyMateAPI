import { CockroachDbTargetTransactionRepositoryBuilder } from "./cockroachdb_transaction_repository";
import { CockroachDbTestHelper } from "../../utils/cockroachDbTestHelper";
import { CockroachDbTargetPayerPayeeRepositoryBuilder } from "./cockroachdb_payerpayee_repository";
import { CockroachDbTargetCategoryRepositoryBuilder } from "./cockroachdb_category_repository";
import { CockroachDbTargetUserRepositoryBuilder } from "./cockroachdb_user_repository";
import { randomUUID } from "crypto";
import { PayerOrPayee, TransactionTypes } from "../constants";
import { CockroachDbTransaction } from "./model";
import { createLogger } from "../../utils/logger";

const cockroachDbTestHelper = CockroachDbTestHelper();

const setupTest = async () => {
    const stubLogger = createLogger();
    const sut = CockroachDbTargetTransactionRepositoryBuilder(stubLogger, cockroachDbTestHelper.cockroachDbConnection);
    const payerPayeeRepo = CockroachDbTargetPayerPayeeRepositoryBuilder(stubLogger, cockroachDbTestHelper.cockroachDbConnection);
    const categoryRepo = CockroachDbTargetCategoryRepositoryBuilder(stubLogger, cockroachDbTestHelper.cockroachDbConnection);
    const userRepo = CockroachDbTargetUserRepositoryBuilder(stubLogger, cockroachDbTestHelper.cockroachDbConnection);

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
            transaction_type_id: transactionTypeIds[transactionType],
            user_id: savedUserId
        }]))[0];

        const savedSubcategoryId = await categoryRepo.getSubcategoryIdWithCategoryId(savedUserId, savedCategoryId, subcategoryName);

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

            const payerPayeeId1 = await transactionTestHelpers.savePayeePayee({ name: "payee1", payerPayee: "payee" });
            const subcategoryId1 = (await transactionTestHelpers.saveCategoryAndSubcategory({
                categoryName: "test category1",
                subcategoryName: "sub1",
                transactionType: "expense"
            })).subcategoryId

            const savedTransaction1 = {
                id: randomUUID(),
                amount: 123.23,
                payerpayee_id: payerPayeeId1,
                subcategory_id: subcategoryId1,
                transaction_timestamp: "2023-08-12T15:30:00.000Z",
                transaction_type_id: transactionTypeIds.expense,
                user_id: savedUserId,
                notes: "hello note"
            };

            const payerPayeeId2 = await transactionTestHelpers.savePayeePayee({ name: "payer10", payerPayee: "payer" });
            const subcategoryId2 = (await transactionTestHelpers.saveCategoryAndSubcategory({
                categoryName: "income category",
                subcategoryName: "salary",
                transactionType: "income"
            })).subcategoryId

            const savedTransaction2 = {
                id: randomUUID(),
                amount: 1232.23,
                payerpayee_id: payerPayeeId2,
                subcategory_id: subcategoryId2,
                transaction_timestamp: "2024-08-12T01:30:00.000Z",
                transaction_type_id: transactionTypeIds.income,
                user_id: savedUserId,
                notes: "salary!"
            };

            const savedTransactionIds = await sut.saveTransactions([savedTransaction1, savedTransaction2]);
            expect(savedTransactionIds).toEqual([savedTransaction1.id, savedTransaction2.id])

            const retrievedTransactions = await cockroachDbTestHelper.performAdhocQuery<CockroachDbTransaction>(`SELECT * FROM transaction`, []);

            const expectedTransactions = [savedTransaction1, savedTransaction2].map(transaction => ({
                ...transaction,
                transaction_timestamp: new Date(transaction.transaction_timestamp),
                amount: transaction.amount.toString()
            }))

            expect(retrievedTransactions.sort((a, b) => a.id.localeCompare(b.id))).toEqual(expectedTransactions.sort((a, b) => a.id.localeCompare(b.id)));
        });
    });
})