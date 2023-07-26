import { Logger } from "winston"
import { SourceUserRepository } from "../source_user_repository"

export const DynamoDbSourceUserRepository = (logger: Logger): SourceUserRepository => {

    const getAllUserIdentifiers = async (): Promise<string[]> => {
        return ["auth0|jgv115", "auth0|jgv551"];
    }

    return {
        getAllUserIdentifiers
    }
}