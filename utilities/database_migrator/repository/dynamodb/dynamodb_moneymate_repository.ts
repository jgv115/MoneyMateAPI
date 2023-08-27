import { Logger } from "winston"
import { DynamoDbConfig } from "./config"
import { DynamoDBDocumentClient, QueryCommand, QueryCommandInput } from "@aws-sdk/lib-dynamodb"
import { DynamoDbCategory, DynamoDbPayerPayee, DynamoDbTransaction } from "./model"
import { DynamoDBClient } from "@aws-sdk/client-dynamodb";

type SupportedDynamoDbItemNames = "Categories" | "PayersPayees" | "Transaction";
type SupportedDynamoDbModel<T> =
    T extends "Categories" ? DynamoDbCategory :
    T extends "PayersPayees" ? DynamoDbPayerPayee :
    T extends "Transaction" ? DynamoDbTransaction :
    never;

export type DynamoDbMoneyMateDbRepository = ReturnType<typeof DynamoDbMoneyMateDbRepositoryBuilder>;

export const DynamoDbMoneyMateDbRepositoryBuilder = (logger: Logger, client: DynamoDBClient, config: DynamoDbConfig) => {
    const query = async <T extends SupportedDynamoDbItemNames>(
        itemType: T,
        userIdentifiers: string[],
        mapper: (items: Record<string, any>[]
        ) => SupportedDynamoDbModel<T>[]): Promise<SupportedDynamoDbModel<T>[]> => {

        const docClient = DynamoDBDocumentClient.from(client);
        const returnedModels: SupportedDynamoDbModel<T>[] = []

        for (const userIdentifier of userIdentifiers) {
            logger.info(`Retrieving ${itemType} for userIdentifier: ${userIdentifier}`)

            const queryCommandInput: QueryCommandInput = {
                TableName: config.tableName,
                KeyConditionExpression:
                    "UserIdQuery = :userIdQuery",
                ExpressionAttributeValues: {
                    ":userIdQuery": `${userIdentifier}#${itemType}`,
                }
            }

            let tempItems: SupportedDynamoDbModel<T>[] = [];

            let response = await docClient.send(new QueryCommand(queryCommandInput));

            tempItems.push(...mapper(response.Items))

            while (response.LastEvaluatedKey) {
                response = await docClient.send(new QueryCommand({
                    ...queryCommandInput,
                    ExclusiveStartKey: response.LastEvaluatedKey
                }));

                tempItems.push(...mapper(response.Items))
            }

            logger.info(`Retrieved ${tempItems.length} ${itemType} items for user identifier: ${userIdentifier}`);
            returnedModels.push(...tempItems);
        }

        return returnedModels;
    }

    return {
        query
    }
}