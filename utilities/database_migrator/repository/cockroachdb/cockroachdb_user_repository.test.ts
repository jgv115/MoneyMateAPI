import { CockroachDbTestHelper } from "../../utils/cockroachDbTestHelper";
import { CockroachDbTargetUserRepository } from "./cockroachdb_user_repository";
import { createLogger } from "../../utils/logger";

const cockroachDbTestHelper = CockroachDbTestHelper();

const setupTest = () => {
    const repository = CockroachDbTargetUserRepository(createLogger(), cockroachDbTestHelper.cockroachDbConnection)

    return {
        repository
    }
};


describe("CockroachDB Target User Repository", () => {
    afterEach(async () => {
        await cockroachDbTestHelper.cleanUp();
    });

    afterAll(async () => {
        await cockroachDbTestHelper.terminateConnection();
    });

    describe("saveUsers", () => {
        test("given input userIdentifier then user saved correctly to the database", async () => {
            const { repository } = setupTest();

            const userIdentifiers = ["test1", "test2", "test3"]
            await repository.saveUsers(userIdentifiers);

            for (const userIdentifier of userIdentifiers) {
                const returnedUser = await cockroachDbTestHelper.getUserByUserIdentifier(userIdentifier);

                expect(returnedUser).toBeDefined();
                expect(returnedUser.user_identifier).toEqual(userIdentifier);
            }
        });
    })

    describe("getUserIdFromUserIdentifier", () => {
        test("given input userIdentifier then correct userId returned", async () => {
            const { repository } = setupTest();

            const userIdentifier = 'test123';
            const savedUserIds = await repository.saveUsers([userIdentifier]);

            const userId = await repository.getUserIdFromUserIdentifier(userIdentifier);
            expect(userId).toEqual(savedUserIds[0])
        });
    })
})