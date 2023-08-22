import { randomUUID } from "crypto";
import { createLogger } from "../../utils/logger";
import { DynamoDbMoneyMateDbRepository } from "./dynamodb_moneymate_repository";
import { DynamoDbHelpers } from "./helpers";
import { DynamoDbCategoriesMapper, DynamoDbPayersPayeesMapper, DynamoDbTransactionMapper } from "./mappers";
import { DynamoDbCategory, DynamoDbPayerPayee, DynamoDbTransaction } from "./model";

const {
    dynamoDbClient,
    tableName,
    createMoneyMateDbTable,
    destroyMoneyMateDbTable,
    saveItemToDynamoDbTable
} = DynamoDbHelpers();

const setupTest = () => {
    const sut = DynamoDbMoneyMateDbRepository(createLogger(), dynamoDbClient, { tableName })
    return {
        sut
    }
}

describe("DynamoDbSourceCategoryRepository", () => {
    beforeEach(async () => {
        await createMoneyMateDbTable();
    });

    afterEach(async () => {
        await destroyMoneyMateDbTable();
    });
    describe("Categories", () => {
        test("given user identifier input and correct mapper then correct categories are returned", async () => {
            const { sut } = setupTest();

            const savedCategories = [
                {
                    UserIdQuery: "auth0|jgv115#Categories",
                    Subquery: "Category name",
                    TransactionType: "expense",
                    Subcategories: ["sub1", "sub2", "sub3", "sub4", "sub5"]
                },
                {
                    UserIdQuery: "auth0|jgv115#Categories",
                    Subquery: "Eating Out",
                    TransactionType: "expense",
                    Subcategories: []
                },
                {
                    UserIdQuery: "auth0|jgv115#Categories",
                    Subquery: "income category1",
                    TransactionType: "income",
                    Subcategories: ["sub6", "sub7"]
                },
                {
                    UserIdQuery: "fgdshjkghwejhkj#Categories",
                    Subquery: "Eating Out",
                    TransactionType: "expense",
                    Subcategories: ["sub1", "sub2"]
                },
                {
                    UserIdQuery: "fgdshjkghwejhkj#Categories",
                    Subquery: "income category1",
                    TransactionType: "income",
                    Subcategories: ["sub6", "sub7"]
                },
            ] satisfies DynamoDbCategory[];

            for (const category of savedCategories)
                await saveItemToDynamoDbTable(category);

            const returnedCategories = await sut.query("Categories", ["auth0|jgv115", "fgdshjkghwejhkj"], DynamoDbCategoriesMapper);

            expect(returnedCategories).toEqual(savedCategories);
        })
    });

    describe("PayersPayees", () => {
        test("given user identifier and correct mapper then correct payerspayees are returned", async () => {
            const { sut } = setupTest();

            const savedPayersPayees: DynamoDbPayerPayee[] = [
                {
                    UserIdQuery: "auth0|jgv115#PayersPayees",
                    Subquery: "payee#028a1215-da68-435d-b528-9d7d6895a499",
                    ExternalId: "",
                    PayerPayeeName: "Woolworths"
                },
                {
                    UserIdQuery: "auth0|jgv115#PayersPayees",
                    Subquery: "payee#03a97785-d0e5-4733-82e8-3db6355b9a3b",
                    ExternalId: "abdhej",
                    PayerPayeeName: "Exxon"
                },
                {
                    UserIdQuery: "test1#PayersPayees",
                    Subquery: "payee#028a1215-da68-435d-b528-9d7d6895a4ab",
                    ExternalId: "",
                    PayerPayeeName: "Woolworths"
                },
                {
                    UserIdQuery: "test2#PayersPayees",
                    Subquery: "payee#03a97785-d0e5-4733-82e8-3db6355b9aac",
                    ExternalId: "",
                    PayerPayeeName: "Exxon"
                },
            ];

            for (const payerPayee of savedPayersPayees)
                await saveItemToDynamoDbTable(payerPayee);

            const returnedPayersPayees = await sut.query("PayersPayees", ["auth0|jgv115", "test1", "test2"], DynamoDbPayersPayeesMapper);

            expect(returnedPayersPayees).toEqual(savedPayersPayees);
        })
    });

    describe("Transactions", () => {
        test("given user identifiers and correct mapper then correct transactions are returned", async () => {
            const { sut } = setupTest();

            const savedTransactions: DynamoDbTransaction[] = [
                {
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
                },
                {
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
                },
                {
                    UserIdQuery: 'test1#Transaction',
                    Subquery: "fa00567c-468e-4ccf-af4c-fca1c731915e",
                    TransactionTimestamp: '2023-07-01T10:59:00.000Z',
                    TransactionType: "income",
                    Amount: 123.45,
                    Category: "category1",
                    SubCategory: "subcategory1",
                    PayerPayeeId: randomUUID(),
                    PayerPayeeName: "payer1",
                    Note: "note"
                },
                {
                    UserIdQuery: 'test2#Transaction',
                    Subquery: "fa00567c-468e-4ccf-af4c-fca1c731915f",
                    TransactionTimestamp: '2023-08-01T10:59:00.000Z',
                    TransactionType: "expense",
                    Amount: 1232.45,
                    Category: "category2",
                    SubCategory: "subcategory2",
                    PayerPayeeId: randomUUID(),
                    PayerPayeeName: "payee2",
                    Note: ""
                }
            ];

            for (const transaction of savedTransactions)
                await saveItemToDynamoDbTable(transaction);

            const returnedTransactions = await sut.query("Transaction", ["auth0|jgv115", "test1", "test2"], DynamoDbTransactionMapper);

            expect(returnedTransactions).toEqual(savedTransactions);
        });
    })
});