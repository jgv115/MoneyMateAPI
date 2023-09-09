import { Pool } from "pg";
import { Logger } from "winston";
import { CockroachDbTransaction } from "./model";
import { TransactionTypes } from "../constants";

export type CockroachDbTargetTransactionRepository = ReturnType<typeof CockroachDbTargetTransactionRepositoryBuilder>;

export const CockroachDbTargetTransactionRepositoryBuilder = (logger: Logger, client: Pool) => {

    const saveTransaction = async (transaction: CockroachDbTransaction): Promise<string> => {
        logger.info(`Saving transaction`, { transaction });

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

        return response.rows[0].id
    }


    const saveTransactions = async (transactions: CockroachDbTransaction[]): Promise<string[]> => {
        const savedTransactionIds: string[] = [];

        for (const transaction of transactions) {
            const savedTransactionId = await saveTransaction(transaction);
            savedTransactionIds.push(savedTransactionId);
        }


        return savedTransactionIds;
    }

    const retrieveTransactionTypeIds = async (): Promise<{ [key in TransactionTypes]: string }> => {
        const response = await client.query("SELECT * FROM transactiontype");
        return response.rows.reduce((a, v) => ({ ...a, [v.name]: v.id }), {});
    }

    return {
        saveTransaction,
        saveTransactions,
        retrieveTransactionTypeIds
    }
}