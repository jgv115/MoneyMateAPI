import { Pool } from "pg";
import { Logger } from "winston";
import { PayerPayee } from "./model/payerpayee";
import { PayerOrPayee } from "./constants";


export const CockroachDbTargetPayerPayeeRepository = (logger: Logger, client: Pool) => {

    const getPayerPayeeTypeIds = async (): Promise<{
        [key in PayerOrPayee]: string;
    }> => {
        const response = await client.query(`SELECT * FROM payerpayeetype`)
        return response.rows.reduce((a, v) => ({ ...a, [v.name]: v.id }), {});
    };

    const getExternalLinkTypeIds = async (): Promise<{ string: string }> => {
        const response = await client.query(`SELECT * FROM payerpayeeexternallinktype`);
        return response.rows.reduce((a, v) => ({ ...a, [v.name]: v.id }), {});
    }

    const savePayerPayees = async (payerPayees: PayerPayee[]): Promise<string[]> => {
        const savedPayerPayeeIds = new Set<string>();

        for (const payerPayee of payerPayees) {
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

            savedPayerPayeeIds.add(response.rows[0].id);
        }

        return Array.from(savedPayerPayeeIds) as string[];
    }

    return {
        getPayerPayeeTypeIds,
        getExternalLinkTypeIds,
        savePayerPayees
    }
}