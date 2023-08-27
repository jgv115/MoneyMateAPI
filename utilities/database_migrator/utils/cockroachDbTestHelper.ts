import { Pool } from "pg";
import { getCockroachDbConfig } from "../config/cockroachdb_config_provider";
import { Environment } from "../constants";
import { CockroachDbUser } from "../repository/cockroachdb/model";
import { CockroachDbCategory } from "../repository/cockroachdb/model/category";
import { TransactionTypes } from "../repository/constants";

export const CockroachDbTestHelper = () => {
    const cockroachDbConfig = getCockroachDbConfig(Environment.local);
    const cockroachDbConnection = new Pool({
        host: cockroachDbConfig.host,
        port: cockroachDbConfig.port,
        database: cockroachDbConfig.database,
        user: cockroachDbConfig.user,
        password: cockroachDbConfig.password,
        max: 20,
        idleTimeoutMillis: 10000,
        connectionTimeoutMillis: 2000
    });

    const cleanUp = async () => {
        await cockroachDbConnection.query('TRUNCATE users CASCADE');
    }

    const terminateConnection = async () => {
        await cockroachDbConnection.end();
    }

    const performAdhocQuery = async <T>(query: string, values: string[]): Promise<T[]> => {
        const result = await cockroachDbConnection.query(query, values);
        return result.rows;
    }

    const getTransactionTypeIds = async (): Promise<{ [key in TransactionTypes]: string }> => {
        const result = await cockroachDbConnection.query(`SELECT * FROM transactiontype`);
        return result.rows.reduce((a, v) => ({ ...a, [v.name]: v.id }), {})
    }

    const getUserByUserIdentifier = async (userIdentifier: string): Promise<CockroachDbUser> => {
        const result = await cockroachDbConnection.query(`SELECT * FROM users WHERE user_identifier = $1`, [userIdentifier]);
        return result.rows[0];
    }

    const getCategoryByCategoryId = async (categoryId: string): Promise<CockroachDbCategory> => {
        const result = await cockroachDbConnection.query(`SELECT * FROM category WHERE id = $1`, [categoryId]);
        return result.rows[0];
    }

    return {
        cockroachDbConnection,
        cleanUp,
        terminateConnection,
        performAdhocQuery,
        getTransactionTypeIds,
        getUserByUserIdentifier,
        getCategoryByCategoryId
    }
}