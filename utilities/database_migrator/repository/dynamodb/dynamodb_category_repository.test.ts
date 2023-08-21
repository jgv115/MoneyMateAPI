import { createLogger } from "../../utils/logger";
import { DynamoDbSourceCategoryRepository } from "./dynamodb_category_repository";
import { DynamoDbHelpers } from "./helpers";
import { DynamoDbCategory } from "./model";

const {
    dynamoDbClient,
    tableName,
    createMoneyMateDbTable,
    destroyMoneyMateDbTable,
    saveItemToDynamoDbTable
} = DynamoDbHelpers();

const setupTest = () => {
    const sut = DynamoDbSourceCategoryRepository(createLogger(), dynamoDbClient, { tableName })
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

    describe("getCategories", () => {
        test("given user identifier input then correct categories are returned", async () => {

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

            const returnedCategories = await sut.getCategories(["auth0|jgv115", "fgdshjkghwejhkj"]);

            expect(returnedCategories).toEqual(savedCategories);
        })
    });
});