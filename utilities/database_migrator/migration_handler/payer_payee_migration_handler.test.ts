import { CockroachDbTargetPayerPayeeRepository } from "../repository/cockroachdb/cockroachdb_payerpayee_repository";
import { CockroachDbTargetUserRepository } from "../repository/cockroachdb/cockroachdb_user_repository";
import { DynamoDbMoneyMateDbRepository } from "../repository/dynamodb/dynamodb_moneymate_repository";
import { DynamoDbSourceUserRepository } from "../repository/dynamodb/dynamodb_user_repository";
import { DynamoDbPayerPayee } from "../repository/dynamodb/model";
import { createLogger } from "../utils/logger";
import { MigrationResult } from "./migration_result";
import { PayerPayeeMigrationHandler } from "./payer_payee_migration_handler";

describe("PayerPayeeMigrationHandler", () => {
    const setupTests = () => {
        const mockSourcePayerPayeeRepository: DynamoDbMoneyMateDbRepository = {
            query: jest.fn()
        };
        const mockTargetPayerPayeeRepository: CockroachDbTargetPayerPayeeRepository = {
            getPayerPayeeTypeIds: jest.fn(),
            getExternalLinkTypeIds: jest.fn(),
            savePayerPayee: jest.fn(),
            savePayerPayees: jest.fn(),
            retrievePayerPayeeId: undefined
        };
        const mockSourceUserRepository: DynamoDbSourceUserRepository = {
            getAllUserIdentifiers: jest.fn()
        }
        const mockTargetUserRepository: CockroachDbTargetUserRepository = {
            saveUsers: jest.fn(),
            getUserIdFromUserIdentifier: jest.fn(),
            getUserIdentifierToUserIdMap: jest.fn()
        };

        const sut = PayerPayeeMigrationHandler(
            createLogger(),
            mockSourcePayerPayeeRepository,
            mockTargetPayerPayeeRepository,
            mockSourceUserRepository,
            mockTargetUserRepository
        );

        return {
            sut,
            mocks: {
                mockSourcePayerPayeeRepository,
                mockTargetPayerPayeeRepository,
                mockSourceUserRepository,
                mockTargetUserRepository
            }
        }
    };

    describe("handleMigration", () => {
        test("given payerpayee from repository and no errors then correct payerpayees migrated", async () => {
            const { sut, mocks } = setupTests();

            (mocks.mockSourceUserRepository.getAllUserIdentifiers as jest.Mock).mockReturnValue(["auth0|jgv115", "auth0|test"]);

            (mocks.mockTargetUserRepository.getUserIdentifierToUserIdMap as jest.Mock).mockReturnValue({
                "auth0|jgv115": "user-id1",
                "auth0|test": "user-id2"
            });

            (mocks.mockTargetPayerPayeeRepository.getExternalLinkTypeIds as jest.Mock).mockReturnValue({
                Custom: "custom-id",
                Google: "google-id"
            });
            (mocks.mockTargetPayerPayeeRepository.getPayerPayeeTypeIds as jest.Mock).mockReturnValue({
                payer: "payer-id123",
                payee: "payee-id123"
            });

            (mocks.mockSourcePayerPayeeRepository.query as jest.Mock<Promise<DynamoDbPayerPayee[]>>).mockReturnValue(
                Promise.resolve([
                    {
                        UserIdQuery: "auth0|test#PayersPayees",
                        Subquery: "payee#004fe4d2-5f30-4329-b84a-ce786bd367ab",
                        ExternalId: "",
                        PayerPayeeName: "payeename"
                    },
                    {
                        UserIdQuery: "auth0|test#PayersPayees",
                        Subquery: "payee#004fe4d2-5f30-4329-b84a-ce786bd367ac",
                        ExternalId: "external-id-1",
                        PayerPayeeName: "payeename2"
                    },
                    {
                        UserIdQuery: "auth0|jgv115#PayersPayees",
                        Subquery: "payer#004fe4d2-5f30-4329-b84a-ce786bd367ad",
                        ExternalId: "external-id-2",
                        PayerPayeeName: "payer3"
                    }
                ])
            )


            const migrationResult = await sut.handleMigration();
            expect(migrationResult).toEqual({
                failedRecords: [],
                numberOfSuccessfullyMigratedRecords: 3
            } satisfies MigrationResult<DynamoDbPayerPayee>);

            expect(mocks.mockTargetPayerPayeeRepository.savePayerPayee).toHaveBeenNthCalledWith(1, {
                id: "004fe4d2-5f30-4329-b84a-ce786bd367ab",
                user_id: "user-id2",
                name: "payeename",
                payerpayeetype_id: "payee-id123",
                external_link_type_id: "custom-id",
                external_link_id: ""
            });
            expect(mocks.mockTargetPayerPayeeRepository.savePayerPayee).toHaveBeenNthCalledWith(2, {
                id: "004fe4d2-5f30-4329-b84a-ce786bd367ac",
                user_id: "user-id2",
                name: "payeename2",
                payerpayeetype_id: "payee-id123",
                external_link_type_id: "google-id",
                external_link_id: "external-id-1"
            });
            expect(mocks.mockTargetPayerPayeeRepository.savePayerPayee).toHaveBeenNthCalledWith(3, {
                id: "004fe4d2-5f30-4329-b84a-ce786bd367ad",
                user_id: "user-id1",
                name: "payer3",
                payerpayeetype_id: "payer-id123",
                external_link_type_id: "google-id",
                external_link_id: "external-id-2"
            });
        });

        test("given errors returned while saving payerpayee then correct migration result returned", async () => {
            const { sut, mocks } = setupTests();

            (mocks.mockSourceUserRepository.getAllUserIdentifiers as jest.Mock).mockReturnValue(["auth0|jgv115", "auth0|test"]);

            (mocks.mockTargetUserRepository.getUserIdentifierToUserIdMap as jest.Mock).mockReturnValue({
                "auth0|jgv115": "user-id1",
                "auth0|test": "user-id2"
            });

            (mocks.mockTargetPayerPayeeRepository.getExternalLinkTypeIds as jest.Mock).mockReturnValue({
                Custom: "custom-id",
                Google: "google-id"
            });
            (mocks.mockTargetPayerPayeeRepository.getPayerPayeeTypeIds as jest.Mock).mockReturnValue({
                payer: "payer-id123",
                payee: "payee-id123"
            });

            const failedRecord = {
                UserIdQuery: "auth0|test#PayersPayees",
                Subquery: "payee#004fe4d2-5f30-4329-b84a-ce786bd367ac",
                ExternalId: "external-id-1",
                PayerPayeeName: "payeename2"
            };

            (mocks.mockSourcePayerPayeeRepository.query as jest.Mock<Promise<DynamoDbPayerPayee[]>>).mockReturnValue(
                Promise.resolve([
                    {
                        UserIdQuery: "auth0|test#PayersPayees",
                        Subquery: "payee#004fe4d2-5f30-4329-b84a-ce786bd367ab",
                        ExternalId: "",
                        PayerPayeeName: "payeename"
                    },
                    failedRecord,
                    {
                        UserIdQuery: "auth0|jgv115#PayersPayees",
                        Subquery: "payer#004fe4d2-5f30-4329-b84a-ce786bd367ad",
                        ExternalId: "external-id-2",
                        PayerPayeeName: "payer3"
                    }
                ])
            );

            (mocks.mockTargetPayerPayeeRepository.savePayerPayee as jest.Mock).mockReturnValueOnce({});
            (mocks.mockTargetPayerPayeeRepository.savePayerPayee as jest.Mock).mockRejectedValueOnce({});
            (mocks.mockTargetPayerPayeeRepository.savePayerPayee as jest.Mock).mockReturnValueOnce({});


            const migrationResult = await sut.handleMigration();
            expect(migrationResult).toEqual({
                failedRecords: [failedRecord],
                numberOfSuccessfullyMigratedRecords: 2
            } satisfies MigrationResult<DynamoDbPayerPayee>);
        });

        test("given user identifier not found then migration result returned with invalid record", async () => {
            const { sut, mocks } = setupTests();

            (mocks.mockSourceUserRepository.getAllUserIdentifiers as jest.Mock).mockReturnValue(["auth0|jgv115", "auth0|test"]);

            (mocks.mockTargetUserRepository.getUserIdentifierToUserIdMap as jest.Mock).mockReturnValue({
                "auth0|jgv115": "user-id1",
                "auth0|test": "user-id2"
            });

            (mocks.mockTargetPayerPayeeRepository.getExternalLinkTypeIds as jest.Mock).mockReturnValue({
                Custom: "custom-id",
                Google: "google-id"
            });
            (mocks.mockTargetPayerPayeeRepository.getPayerPayeeTypeIds as jest.Mock).mockReturnValue({
                payer: "payer-id123",
                payee: "payee-id123"
            });

            const invalidRecord = {
                UserIdQuery: "auth0|invalid#PayersPayees",
                Subquery: "payee#004fe4d2-5f30-4329-b84a-ce786bd367ab",
                ExternalId: "",
                PayerPayeeName: "payeename"
            };

            (mocks.mockSourcePayerPayeeRepository.query as jest.Mock<Promise<DynamoDbPayerPayee[]>>).mockReturnValue(
                Promise.resolve([
                    invalidRecord,
                    {
                        UserIdQuery: "auth0|test#PayersPayees",
                        Subquery: "payee#004fe4d2-5f30-4329-b84a-ce786bd367ac",
                        ExternalId: "external-id-1",
                        PayerPayeeName: "payeename2"
                    },
                    {
                        UserIdQuery: "auth0|jgv115#PayersPayees",
                        Subquery: "payer#004fe4d2-5f30-4329-b84a-ce786bd367ad",
                        ExternalId: "external-id-2",
                        PayerPayeeName: "payer3"
                    }
                ])
            )


            const migrationResult = await sut.handleMigration();
            expect(migrationResult).toEqual({
                failedRecords: [invalidRecord],
                numberOfSuccessfullyMigratedRecords: 2
            } satisfies MigrationResult<DynamoDbPayerPayee>);
        });

        test("given invalid payerpayeetype then migration result returned with invalid record", async () => {
            const { sut, mocks } = setupTests();

            (mocks.mockSourceUserRepository.getAllUserIdentifiers as jest.Mock).mockReturnValue(["auth0|jgv115", "auth0|test"]);

            (mocks.mockTargetUserRepository.getUserIdentifierToUserIdMap as jest.Mock).mockReturnValue({
                "auth0|jgv115": "user-id1",
                "auth0|test": "user-id2"
            });

            (mocks.mockTargetPayerPayeeRepository.getExternalLinkTypeIds as jest.Mock).mockReturnValue({
                Custom: "custom-id",
                Google: "google-id"
            });
            (mocks.mockTargetPayerPayeeRepository.getPayerPayeeTypeIds as jest.Mock).mockReturnValue({
                payer: "payer-id123",
                payee: "payee-id123"
            });

            const invalidRecord = {
                UserIdQuery: "auth0|jgv115#PayersPayees",
                Subquery: "invalid#004fe4d2-5f30-4329-b84a-ce786bd367ab",
                ExternalId: "",
                PayerPayeeName: "payeename"
            };

            (mocks.mockSourcePayerPayeeRepository.query as jest.Mock<Promise<DynamoDbPayerPayee[]>>).mockReturnValue(
                Promise.resolve([
                    invalidRecord,
                    {
                        UserIdQuery: "auth0|test#PayersPayees",
                        Subquery: "payee#004fe4d2-5f30-4329-b84a-ce786bd367ac",
                        ExternalId: "external-id-1",
                        PayerPayeeName: "payeename2"
                    },
                    {
                        UserIdQuery: "auth0|jgv115#PayersPayees",
                        Subquery: "payer#004fe4d2-5f30-4329-b84a-ce786bd367ad",
                        ExternalId: "external-id-2",
                        PayerPayeeName: "payer3"
                    }
                ])
            )


            const migrationResult = await sut.handleMigration();
            expect(migrationResult).toEqual({
                failedRecords: [invalidRecord],
                numberOfSuccessfullyMigratedRecords: 2
            } satisfies MigrationResult<DynamoDbPayerPayee>);
        });
    });
})
