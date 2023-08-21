import { DynamoDbHelpers } from "./helpers";
import { DynamoDbSourceUserRepository } from "./dynamodb_user_repository";
import { createLogger } from "../../utils/logger";
import { DynamoDbCategory, DynamoDbPayerPayee, DynamoDbTransaction } from "./model";
import { randomUUID } from "crypto";

const {
    dynamoDbClient,
    tableName,
    createMoneyMateDbTable,
    destroyMoneyMateDbTable,
    saveItemToDynamoDbTable
} = DynamoDbHelpers();

const setupTest = () => {
    const sut = DynamoDbSourceUserRepository(createLogger(), dynamoDbClient, { tableName })
    return {
        sut
    }
}

describe("DynamoDbSourceUserRepository", () => {
    beforeEach(async () => {
        await createMoneyMateDbTable();
    });

    afterEach(async () => {
        await destroyMoneyMateDbTable();
    })

    describe("getAllUserIdentifiers", () => {
        test("returns correct userIdentifiers from db", async () => {
            const { sut } = setupTest();

            await saveItemToDynamoDbTable({
                UserIdQuery: "auth0|jgv115#Categories",
                Subquery: "Eating Out",
                TransactionType: "expense",
                Subcategories: ["sub1", "sub2"]
            } satisfies DynamoDbCategory);

            await saveItemToDynamoDbTable({
                UserIdQuery: "auth0|jgv115#Categories",
                Subquery: "Entertainment",
                TransactionType: "expense",
                Subcategories: ["sub3", "sub4"]
            } satisfies DynamoDbCategory);

            await saveItemToDynamoDbTable({
                UserIdQuery: "auth0|test#PayersPayees",
                Subquery: "payee#004fe4d2-5f30-4329-b84a-ce786bd367ab",
                ExternalId: "",
                PayerPayeeName: "payeename"
            } satisfies DynamoDbPayerPayee);

            await saveItemToDynamoDbTable({
                UserIdQuery: "test1#Categories",
                Subquery: randomUUID().toString(),
                TransactionTimestamp: "2023-07-01T10:59:00.000Z",
                TransactionType: "expense",
                Amount: 1,
                Category: "cat",
                SubCategory: "subcat",
                Note: "",
                PayerPayeeId: "id",
                PayerPayeeName: "payer name"
            } satisfies DynamoDbTransaction);

            const userIdentifiers = await sut.getAllUserIdentifiers();

            expect(userIdentifiers.sort()).toEqual(["auth0|jgv115", "auth0|test", "test1"].sort());

        }, 10000000);
    });
})