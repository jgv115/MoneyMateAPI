import { CockroachDbTestHelper } from "../../utils/cockroachDbTestHelper";
import { CockroachDbTargetUserRepository } from "./cockroachdb_user_repository";
import { CockroachDbTargetCategoryRepository } from "./cockroachdb_category_repository";
import { createLogger } from "../../utils/logger";
import { CockroachDbCategory } from "./model/category";
import { randomUUID } from "crypto";
import { Subcategory } from "./model";

const cockroachDbTestHelper = CockroachDbTestHelper();

const setupTest = async () => {
    const sut = CockroachDbTargetCategoryRepository(createLogger(), cockroachDbTestHelper.cockroachDbConnection)

    const testUserIdentifier = "testUser123";
    const userRepository = CockroachDbTargetUserRepository(createLogger(), cockroachDbTestHelper.cockroachDbConnection);
    const savedUserIds = await userRepository.saveUsers([testUserIdentifier]);

    const transactionTypeIds = await cockroachDbTestHelper.getTransactionTypeIds();

    return {
        sut,
        testUserId: savedUserIds[0],
        testUserIdentifier,
        transactionTypeIds
    }
};

describe("CockroachDB Target Category Repository", () => {
    afterEach(async () => {
        await cockroachDbTestHelper.cleanUp();
    });

    afterAll(async () => {
        await cockroachDbTestHelper.terminateConnection();
    });

    describe("saveCategories", () => {
        test("given input userId and categories, then categories saved correctly in database ", async () => {
            const { sut, testUserId, transactionTypeIds } = await setupTest();

            const inputCategories = [
                {
                    name: "testcategory1",
                    userId: testUserId,
                    subcategories: ["sub1", "sub2"],
                    transactionType: "expense",
                },
                {
                    name: "testcategory2",
                    userId: testUserId,
                    subcategories: ["sub3", "sub4"],
                    transactionType: "income",
                }
            ]

            const savedCategoryIds = await sut.saveCategories(inputCategories);

            for (let i = 0; i < savedCategoryIds.length; i++) {
                const savedCategory = await cockroachDbTestHelper.getCategoryByCategoryId(savedCategoryIds[i])
                expect({
                    id: savedCategoryIds[i],
                    name: inputCategories[i].name,
                    transaction_type_id: transactionTypeIds[inputCategories[i].transactionType],
                    user_id: testUserId
                } satisfies CockroachDbCategory).toEqual(savedCategory);
            }
        });

        test("given input userId and categories, then subcategories saved correctly in database ", async () => {
            const { sut, testUserId, transactionTypeIds } = await setupTest();

            const inputCategories = [
                {
                    name: "testcategory1",
                    userId: testUserId,
                    subcategories: ["sub1", "sub2"],
                    transactionType: "expense",
                },
                {
                    name: "testcategory2",
                    userId: testUserId,
                    subcategories: ["sub3", "sub4"],
                    transactionType: "income",
                }
            ]

            const savedCategoryIds = await sut.saveCategories(inputCategories);


            for (let i = 0; i < savedCategoryIds.length; i++) {

                // Query for subcategories based on each saved category
                const subcategoryRows = await cockroachDbTestHelper.performAdhocQuery<Subcategory>(
                    `SELECT * FROM subcategory
                    WHERE category_id = $1`, [savedCategoryIds[i]]
                );

                // Ensure category names are all there
                const subcategoryNames = subcategoryRows.map(row => row.name);
                expect(subcategoryNames.sort()).toEqual(inputCategories[i].subcategories.sort());
            }

        });
    });

    describe("getSubcategoryId", () => {
        test("given userId and subcategoryName, then correct id returned", async () => {
            const { sut, transactionTypeIds, testUserId } = await setupTest()

            // Insert category
            const categoryId = randomUUID();
            await cockroachDbTestHelper.performAdhocQuery(`
                INSERT INTO category (id, name, user_id, transaction_type_id) VALUES ($1, $2, $3, $4)`,
                [categoryId, "category1", testUserId, transactionTypeIds["expense"]]);

            // Insert subcategory
            const subcategoryid = randomUUID();
            await cockroachDbTestHelper.performAdhocQuery(`
            INSERT INTO subcategory (id, name, category_id) VALUES ($1, $2, $3)`, [subcategoryid, "sub name", categoryId]);

            // assert
            const returnedSubcategoryId = await sut.getSubcategoryId(testUserId, categoryId, "sub name");
            expect(returnedSubcategoryId).toEqual(subcategoryid);
        });
    })
})