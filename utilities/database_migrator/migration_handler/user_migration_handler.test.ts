import { UserMigrationHandler } from "./user_migration_handler";
import { DynamoDbSourceUserRepository } from "../repository/dynamodb/dynamodb_user_repository";
import { CockroachDbTargetUserRepository } from "../repository/cockroachdb/cockroachdb_user_repository";
import { createLogger } from "../utils/logger";
import { MigrationResult } from "./migration_result";

describe("UserMigrationHandler", () => {
    test("given userIdentifiers from sourceUserRepository then correct users stored in targetUserRepository", async () => {
        const mockSourceUserRepository: DynamoDbSourceUserRepository = {
            getAllUserIdentifiers: jest.fn()
        };

        const mockTargetUserRepository: CockroachDbTargetUserRepository = {
            saveUsers: jest.fn(),
            getUserIdFromUserIdentifier: undefined,
            getUserIdentifierToUserIdMap: undefined
        };

        (mockSourceUserRepository.getAllUserIdentifiers as jest.Mock).mockReturnValue(["id1", "id2"]);
        (mockTargetUserRepository.saveUsers as jest.Mock).mockReturnValue(["id1", "id2"])
        const sut = UserMigrationHandler(createLogger(), mockSourceUserRepository, mockTargetUserRepository);

        const migrationResult = await sut.handleMigration();

        expect(mockTargetUserRepository.saveUsers).toHaveBeenCalledWith(["id1", "id2"])

        expect(migrationResult).toEqual({
            failedRecords: [],
            numberOfSuccessfullyMigratedRecords: 2

        } satisfies MigrationResult<string>)
    });

});