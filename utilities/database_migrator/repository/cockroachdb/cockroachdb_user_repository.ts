import { Pool } from 'pg';
import { Logger } from 'winston';
import { CockroachDbUser } from './model';

export type CockroachDbTargetUserRepository = ReturnType<typeof CockroachDbTargetUserRepositoryBuilder>;

export const CockroachDbTargetUserRepositoryBuilder = (logger: Logger, client: Pool) => {

    const saveUsers = async (userIdentifiers: string[]): Promise<string[]> => {
        logger.info("Writing user identifiers into CockroachDb", { numberOfUsers: userIdentifiers.length })

        const savedUserIdentifiers: string[] = [];

        for (const userIdentifier of userIdentifiers) {
            const response = await client.query(`INSERT INTO USERS (user_identifier) VALUES ($1) RETURNING id`, [userIdentifier])
            savedUserIdentifiers.push(response.rows[0].id);
        }

        logger.info("Successfully wrote user identifiers into CockroachDb", { numberOfUsers: userIdentifiers.length })

        return savedUserIdentifiers;
    };

    const getUserIdFromUserIdentifier = async (userIdentifier: string): Promise<string> => {
        const result = await client.query(`SELECT * FROM USERS WHERE user_identifier = ($1)`, [userIdentifier])
        return result.rows[0].id;
    };

    const getUserIdentifierToUserIdMap = async (): Promise<{ [userIdentifier: string]: string }> => {
        const result = await client.query(`SELECT * FROM USERS`);
        const userObjects = result.rows as CockroachDbUser[];

        return userObjects.reduce((acc, user) => ({ ...acc, [user.user_identifier]: user.id }), {})
    }

    return {
        saveUsers,
        getUserIdFromUserIdentifier,
        getUserIdentifierToUserIdMap
    }
}