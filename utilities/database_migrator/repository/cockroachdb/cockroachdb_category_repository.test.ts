import winston from "winston";
import { CockroachDbTestHelper } from "../../utils/cockroachDbTestHelper";
import { CockroachDbTargetUserRepository } from "./cockroachdb_user_repository";

const cockroachDbTestHelper = CockroachDbTestHelper();

const setupTest = async () => {
    const repository = CockroachDbTargetUserRepository(new winston.Logger, cockroachDbTestHelper.cockroachDbConnection)

    const testUserIdentifier = "testUser123";
    await repository.saveUsers([testUserIdentifier]);

    return {
        repository,
        testUserIdentifier
    }
};

describe("CockroachDB Target Category Repository", () => {
    test("1+1=2", async () => {
        const { repository, testUserIdentifier } = await setupTest();
    })
})