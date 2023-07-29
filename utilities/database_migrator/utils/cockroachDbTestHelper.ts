import { Pool } from "pg";
import { getCockroachDbConfig } from "../config/cockroachdb_config_provider";
import { Environment } from "../constants";
import { CockroachDbUser } from "../repository/cockroachdb/model";

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
        await cockroachDbConnection.end();
    }

    const getUserByUserIdentifier = async (userIdentifier: string): Promise<CockroachDbUser> => {
        const result = await cockroachDbConnection.query(`SELECT * FROM users WHERE user_identifier = $1`, [userIdentifier]);

        if (result.rows.length !== 1)
            throw Error(`Found more than one userId with the user_identifier ${userIdentifier}`)

        return result.rows[0];
    }

    return {
        cockroachDbConnection,
        cleanUp,
        getUserByUserIdentifier
    }
}