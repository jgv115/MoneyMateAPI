import { Pool } from "pg";
import { Logger } from "winston";
import { Transaction } from "./model";
import { TransactionTypes } from "./constants";

export const CockroachDbTransactionRepository = (logger: Logger, client: Pool) => {

    const saveTransactions = async (transactions: Transaction[]): Promise<string[]> => {
        const savedTransactionIds: string[] = [];

        for (const transaction of transactions) {
            const response = await client.query(`
            INSERT INTO transaction (id, user_id, transaction_timestamp, transaction_type_id, amount, subcategory_id, payerpayee_id, notes)
            VALUES ($1, $2, $3, $4, $5, $6, $7, $8) RETURNING id;
            `, [
                transaction.id,
                transaction.user_id,
                transaction.transaction_timestamp,
                transaction.transaction_type_id,
                transaction.amount,
                transaction.subcategory_id,
                transaction.payerpayee_id,
                transaction.notes
            ]);

            savedTransactionIds.push(response.rows[0].id)
        }


        return savedTransactionIds;
    }

    const retrieveTransactionTypeIds = async (): Promise<{ [key in TransactionTypes]: string }> => {
        const response = await client.query("SELECT * FROM transactiontype");
        return response.rows.reduce((a, v) => ({ ...a, [v.name]: v.id }), {});
    }

    return {
        saveTransactions,
        retrieveTransactionTypeIds
    }
}