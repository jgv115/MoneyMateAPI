import { Logger } from "winston"
import { DynamoDBClient, ScanCommandInput, ScanCommand } from "@aws-sdk/client-dynamodb";

import { DynamoDbConfig } from "./config";
import { DynamoDBDocumentClient } from "@aws-sdk/lib-dynamodb";


export type DynamoDbSourceUserRepository = ReturnType<typeof DynamoDbSourceUserRepositoryBuilder>

export const DynamoDbSourceUserRepositoryBuilder = (logger: Logger, client: DynamoDBClient, config: DynamoDbConfig) => {

    const getAllUserIdentifiers = async (): Promise<string[]> => {

        const docClient = DynamoDBDocumentClient.from(client);
        const scanCommandInput = {
            TableName: config.tableName,
            ProjectionExpression: "UserIdQuery"
        } satisfies ScanCommandInput

        const userIdentifiers = new Set<string>();

        let response = await docClient.send(new ScanCommand(scanCommandInput));
        for (const item of response.Items)
            userIdentifiers.add(item.UserIdQuery.S.split("#")[0])

        while (response.LastEvaluatedKey) {
            response = await docClient.send(new ScanCommand({
                ...scanCommandInput,
                ExclusiveStartKey: response.LastEvaluatedKey
            }));

            for (const item of response.Items)
                userIdentifiers.add(item.UserIdQuery.S.split("#")[0])
        };

        return Array.from(userIdentifiers);
    }

    return {
        getAllUserIdentifiers
    }
}