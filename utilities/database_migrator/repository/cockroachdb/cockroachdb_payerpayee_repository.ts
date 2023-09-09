import { Pool } from "pg";
import { Logger } from "winston";
import { CockroachDbPayerPayee } from "./model/payerpayee";
import { PayerOrPayee } from "../constants";
import { ExternalLinkTypes } from "../constants/externalLinkType";


export type CockroachDbTargetPayerPayeeRepository = ReturnType<typeof CockroachDbTargetPayerPayeeRepositoryBuilder>;

export const CockroachDbTargetPayerPayeeRepositoryBuilder = (logger: Logger, client: Pool) => {

    const getPayerPayeeTypeIds = async (): Promise<{
        [key in PayerOrPayee]: string;
    }> => {
        const response = await client.query(`SELECT * FROM payerpayeetype`)
        return response.rows.reduce((a, v) => ({ ...a, [v.name]: v.id }), {});
    };

    const getExternalLinkTypeIds = async (): Promise<{ [key in ExternalLinkTypes]: string }> => {
        const response = await client.query(`SELECT * FROM payerpayeeexternallinktype`);
        return response.rows.reduce((a, v) => ({ ...a, [v.name]: v.id }), {});
    }

    const savePayerPayee = async (payerPayee: CockroachDbPayerPayee): Promise<string> => {
        logger.info(`Saving payerPayee: ${payerPayee.name}`);

        const response = await client.query(`
        INSERT INTO payerpayee (id, user_id, name, payerpayeetype_id, external_link_type_id, external_link_id)
            VALUES ($1, $2, $3, $4, $5, $6) 
            ON CONFLICT (id) DO UPDATE SET
                name = $3,
                payerpayeetype_id = $4,
                external_link_type_id = $5,
                external_link_id = $6
            RETURNING id
        `,
            [payerPayee.id,
            payerPayee.user_id,
            payerPayee.name,
            payerPayee.payerpayeetype_id,
            payerPayee.external_link_type_id,
            payerPayee.external_link_id,
            ]);

        return response.rows[0].id;
    }

    const savePayerPayees = async (payerPayees: CockroachDbPayerPayee[]): Promise<string[]> => {
        const savedPayerPayeeIds = new Set<string>();

        for (const payerPayee of payerPayees) {
            const savedPayerPayeeId = await savePayerPayee(payerPayee);

            savedPayerPayeeIds.add(savedPayerPayeeId);
        }

        return Array.from(savedPayerPayeeIds) as string[];
    };

    const retrievePayerPayeeId = async (userId: string, payerPayeeName: string, payerPayeeTypeId: string): Promise<CockroachDbPayerPayee> => {

        const response = await client.query(`SELECT * FROM payerpayee 
                                                WHERE name = $1 
                                                and user_id = $2 
                                                and payerpayeetype_id = $3`,
            [payerPayeeName, userId, payerPayeeTypeId]);

        return response.rows[0].id;
    }

    return {
        getPayerPayeeTypeIds,
        getExternalLinkTypeIds,
        savePayerPayee,
        savePayerPayees,
        retrievePayerPayeeId
    }
}