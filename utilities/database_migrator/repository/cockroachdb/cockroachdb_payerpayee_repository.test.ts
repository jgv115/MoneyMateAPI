import { randomUUID } from "crypto";
import { CockroachDbTestHelper } from "../../utils/cockroachDbTestHelper";
import { createLogger } from "../../utils/logger";
import { CockroachDbTargetPayerPayeeRepository } from "./cockroachdb_payerpayee_repository";
import { PayerPayee } from "./model";
import { CockroachDbTargetUserRepository } from "./cockroachdb_user_repository";

const cockroachDbTestHelper = CockroachDbTestHelper();

const setupTest = async () => {
    const sut = CockroachDbTargetPayerPayeeRepository(createLogger(), cockroachDbTestHelper.cockroachDbConnection);

    const payerPayeeTypeIds = await sut.getPayerPayeeTypeIds();
    const externalLinkTypeIds = await sut.getExternalLinkTypeIds();

    const userRepository = CockroachDbTargetUserRepository(createLogger(), cockroachDbTestHelper.cockroachDbConnection);
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


    test("given input payerpayees then payerpayees saved correctly in db", async () => {
        const { sut, testUserId, payerPayeeTypeIds, externalLinkTypeIds } = await setupTest();

        const inputPayerPayees: PayerPayee[] = [{
            id: randomUUID(),
            name: "payer1",
            user_id: testUserId,
            payerpayeetype_id: payerPayeeTypeIds["payer"],
            external_link_id: "",
            external_link_type_id: externalLinkTypeIds["Custom"]

        }, {
            id: randomUUID(),
            name: "payee1",
            user_id: testUserId,
            payerpayeetype_id: payerPayeeTypeIds["payee"],
            external_link_id: "1234",
            external_link_type_id: externalLinkTypeIds["Google"]

        }]
        const savedPayerPayeeIds = await sut.savePayerPayees(inputPayerPayees);

        expect(savedPayerPayeeIds.sort()).toEqual(inputPayerPayees.map(input => input.id).sort());

        for (const inputPayerPayee of inputPayerPayees) {
            const savedPayerPayee = (await cockroachDbTestHelper.performAdhocQuery<PayerPayee>(
                `SELECT * FROM payerpayee 
                WHERE user_id = $1 AND id = $2`, [testUserId, inputPayerPayee.id]))[0];

            expect(savedPayerPayee).toEqual(inputPayerPayee)
        }
    })

    test("given input payerpayees that are duplicated then last payerpayee gets saved", async () => {
        const { sut, testUserId, payerPayeeTypeIds, externalLinkTypeIds } = await setupTest();

        const payerPayeeId = randomUUID();
        const inputPayerPayees: PayerPayee[] = [{
            id: payerPayeeId,
            name: "payer1",
            user_id: testUserId,
            payerpayeetype_id: payerPayeeTypeIds["payer"],
            external_link_id: "",
            external_link_type_id: externalLinkTypeIds["Custom"]

        }, {
            id: payerPayeeId,
            name: "payee1",
            user_id: testUserId,
            payerpayeetype_id: payerPayeeTypeIds["payee"],
            external_link_id: "1234",
            external_link_type_id: externalLinkTypeIds["Google"]

        }]
        const savedPayerPayeeIds = await sut.savePayerPayees(inputPayerPayees);

        expect(savedPayerPayeeIds).toEqual([payerPayeeId]);

        const savedPayerPayee = (await cockroachDbTestHelper.performAdhocQuery<PayerPayee>(
            `SELECT * FROM payerpayee 
                WHERE user_id = $1 AND id = $2`, [testUserId, payerPayeeId]))[0];

        expect(savedPayerPayee).toEqual(inputPayerPayees[1])

    });
})