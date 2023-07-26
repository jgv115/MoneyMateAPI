import { Pool } from 'pg';
import { Logger } from 'winston';
import { TargetUserRepository } from '../target_user_repository';

export const CockroachDbTargetUserRepository = (logger: Logger, client: Pool): TargetUserRepository => {

    const saveUsers = async (userIdentifiers: string[]) => {
        logger.info("Writing user identifiers into CockroachDb", { numberOfUsers: userIdentifiers.length })

        for (const userIdentifier of userIdentifiers)
            await client.query(`INSERT INTO USERS (user_identifier) VALUES ($1)`, [userIdentifier])

        logger.info("Successfully wrote user identifiers into CockroachDb", { numberOfUsers: userIdentifiers.length })

        await client.end();
    }

    const getUserIdFromUserIdentifier = async (userIdentifier: string): Promise<string> => {
        const result = await client.query(`SELECT * FROM USERS WHERE user_identifier = ($1)`, [userIdentifier])

        if (result.rows.length !== 1)
            throw Error(`Found more than one userId with the user_identifier ${userIdentifier}`)

        return result.rows[0].id;

    }

    return {
        saveUsers,
        getUserIdFromUserIdentifier
    }


}