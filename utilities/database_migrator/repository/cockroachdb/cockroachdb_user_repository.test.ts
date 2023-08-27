import { CockroachDbTestHelper } from "../../utils/cockroachDbTestHelper";
import { CockroachDbTargetUserRepositoryBuilder } from "./cockroachdb_user_repository";
import { createLogger } from "../../utils/logger";
import { randomUUID } from "crypto";

const cockroachDbTestHelper = CockroachDbTestHelper();

const setupTest = () => {
    const repository = CockroachDbTargetUserRepositoryBuilder(createLogger(), cockroachDbTestHelper.cockroachDbConnection)

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
    });

    describe("getUserIdentifierToUserIdMap", () => {
        test("given users in database, then user objects are returned", async () => {
            const { repository } = setupTest();

            const userId1 = randomUUID();
            const userIdentifier1 = "test1";

            const userId2 = randomUUID();
            const userIdentifier2 = "test2";

            await cockroachDbTestHelper.performAdhocQuery(
                `INSERT INTO USERS (id, user_identifier) VALUES ($1, $2), ($3, $4)`,
                [userId1, userIdentifier1, userId2, userIdentifier2]
            );

            const userMap = await repository.getUserIdentifierToUserIdMap();

            expect(userMap).toEqual({ [userIdentifier1]: userId1, [userIdentifier2]: userId2 })
        });
    });
})