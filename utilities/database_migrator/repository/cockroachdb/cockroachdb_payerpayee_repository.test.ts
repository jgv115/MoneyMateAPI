import { randomUUID } from "crypto";
import { CockroachDbTestHelper } from "../../utils/cockroachDbTestHelper";
import { createLogger } from "../../utils/logger";
import { CockroachDbTargetPayerPayeeRepositoryBuilder } from "./cockroachdb_payerpayee_repository";
import { CockroachDbPayerPayee } from "./model";
import { CockroachDbTargetUserRepositoryBuilder } from "./cockroachdb_user_repository";

const cockroachDbTestHelper = CockroachDbTestHelper();

const setupTest = async () => {
    const sut = CockroachDbTargetPayerPayeeRepositoryBuilder(createLogger(), cockroachDbTestHelper.cockroachDbConnection);

    const payerPayeeTypeIds = await sut.getPayerPayeeTypeIds();
    const externalLinkTypeIds = await sut.getExternalLinkTypeIds();

    const userRepository = CockroachDbTargetUserRepositoryBuilder(createLogger(), cockroachDbTestHelper.cockroachDbConnection);
    const savedUserIds = await userRepository.saveUsers(["testUser123"]);

    return {
        sut,
        testUserId: savedUserIds[0],
        payerPayeeTypeIds,
        externalLinkTypeIds
    }
}

describe("CockroachDb PayerPayee Repository tests", () => {
    afterEach(async () => {
        await cockroachDbTestHelper.cleanUp();
    });

    afterAll(async () => {
        await cockroachDbTestHelper.terminateConnection();
    });


    describe("savePayerPayees", () => {
        test("given input payerpayees then payerpayees saved correctly in db", async () => {
            const { sut, testUserId, payerPayeeTypeIds, externalLinkTypeIds } = await setupTest();

            const inputPayerPayees: CockroachDbPayerPayee[] = [{
                id: randomUUID(),
                name: "payer1",
                user_id: testUserId,
                payerpayeetype_id: payerPayeeTypeIds.payer,
                external_link_id: "",
                external_link_type_id: externalLinkTypeIds.Custom

            }, {
                id: randomUUID(),
                name: "payee1",
                user_id: testUserId,
                payerpayeetype_id: payerPayeeTypeIds.payee,
                external_link_id: "1234",
                external_link_type_id: externalLinkTypeIds.Google

            }]
            const savedPayerPayeeIds = await sut.savePayerPayees(inputPayerPayees);

            expect(savedPayerPayeeIds.sort()).toEqual(inputPayerPayees.map(input => input.id).sort());

            for (const inputPayerPayee of inputPayerPayees) {
                const savedPayerPayee = (await cockroachDbTestHelper.performAdhocQuery<CockroachDbPayerPayee>(
                    `SELECT * FROM payerpayee 
                WHERE user_id = $1 AND id = $2`, [testUserId, inputPayerPayee.id]))[0];

                expect(savedPayerPayee).toEqual(inputPayerPayee)
            }
        })

        test("given input payerpayees that are duplicated then last payerpayee gets saved", async () => {
            const { sut, testUserId, payerPayeeTypeIds, externalLinkTypeIds } = await setupTest();

            const payerPayeeId = randomUUID();
            const inputPayerPayees: CockroachDbPayerPayee[] = [{
                id: payerPayeeId,
                name: "payer1",
                user_id: testUserId,
                payerpayeetype_id: payerPayeeTypeIds.payer,
                external_link_id: "",
                external_link_type_id: externalLinkTypeIds["Custom"]

            }, {
                id: payerPayeeId,
                name: "payee1",
                user_id: testUserId,
                payerpayeetype_id: payerPayeeTypeIds.payee,
                external_link_id: "1234",
                external_link_type_id: externalLinkTypeIds["Google"]

            }]
            const savedPayerPayeeIds = await sut.savePayerPayees(inputPayerPayees);

            expect(savedPayerPayeeIds).toEqual([payerPayeeId]);

            const savedPayerPayee = (await cockroachDbTestHelper.performAdhocQuery<CockroachDbPayerPayee>(
                `SELECT * FROM payerpayee 
                WHERE user_id = $1 AND id = $2`, [testUserId, payerPayeeId]))[0];

            expect(savedPayerPayee).toEqual(inputPayerPayees[1])

        });
    });

    describe("getPayerPayeeTypeIds", () => {
        test("given input parameters then correct saved payerpayeeid retrieved", async () => {
            const { sut, testUserId, payerPayeeTypeIds, externalLinkTypeIds } = await setupTest();

            const payerPayee1 = {
                id: '9ee7ef0f-515f-4a4b-a88e-894b9dc41aaa',
                name: "payer1",
                user_id: testUserId,
                payerpayeetype_id: payerPayeeTypeIds.payer,
                external_link_id: "",
                external_link_type_id: externalLinkTypeIds.Custom
            };

            const payerPayee2 = {
                id: '9ee7ef0f-515f-4a4b-a88e-894b9dc41aab',
                name: "payee1",
                user_id: testUserId,
                payerpayeetype_id: payerPayeeTypeIds.payee,
                external_link_id: "1234",
                external_link_type_id: externalLinkTypeIds.Google

            }
            await sut.savePayerPayees([payerPayee1, payerPayee2])

            const retrievedId1 = await sut.retrievePayerPayeeId(testUserId, payerPayee1.name, payerPayee1.payerpayeetype_id);
            const retrievedId2 = await sut.retrievePayerPayeeId(testUserId, payerPayee2.name, payerPayee2.payerpayeetype_id);

            expect(retrievedId1).toEqual(payerPayee1.id);
            expect(retrievedId2).toEqual(payerPayee2.id);
        });
    });
})