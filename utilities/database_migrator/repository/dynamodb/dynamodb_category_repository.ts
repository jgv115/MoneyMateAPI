import { AttributeValue, DynamoDBClient, QueryCommand } from "@aws-sdk/client-dynamodb"
import { DynamoDbConfig } from "./config"
import { Logger } from "winston"
import { DynamoDBDocumentClient } from "@aws-sdk/lib-dynamodb";
import { DynamoDbCategory } from "./model";
import { TransactionTypes } from "../constants";

export const DynamoDbSourceCategoryRepository = (logger: Logger, client: DynamoDBClient, config: DynamoDbConfig) => {

    const retrieveCategoryFromDocumentResponse = (responseItems: Record<string, AttributeValue>[]): DynamoDbCategory[] => {
        const returnedCategories: DynamoDbCategory[] = [];
        for (const item of responseItems)
            returnedCategories.push({
                UserIdQuery: item.UserIdQuery.S,
                Subquery: item.Subquery.S,
                TransactionType: item.TransactionType.S as TransactionTypes,
                Subcategories: item.Subcategories.L.map(item => item.S)
            })

        return returnedCategories;
    }

    const getCategories = async (userIdentifiers: string[]): Promise<DynamoDbCategory[]> => {
        const docClient = DynamoDBDocumentClient.from(client);
        const returnedCategories: DynamoDbCategory[] = []

        for (const userIdentifier of userIdentifiers) {
            logger.info(`Retrieving DynamoDB categories for userIdentifier: ${userIdentifier}`)

            const queryCommandInput = {
                TableName: config.tableName,
                KeyConditionExpression:
                    "UserIdQuery = :userIdQuery",
                ExpressionAttributeValues: {
                    ":userIdQuery": {
                        S: `${userIdentifier}#Categories`
                    },
                }
            };

            let tempCategories: DynamoDbCategory[] = [];

            let response = await docClient.send(new QueryCommand(queryCommandInput));
            tempCategories.push(...retrieveCategoryFromDocumentResponse(response.Items))

            while (response.LastEvaluatedKey) {
                response = await docClient.send(new QueryCommand({
                    ...queryCommandInput,
                    ExclusiveStartKey: response.LastEvaluatedKey
                }));

                tempCategories.push(...retrieveCategoryFromDocumentResponse(response.Items))
            }

            logger.info(`Retrieved ${tempCategories.length} categories for user identifier: ${userIdentifier}`);
            returnedCategories.push(...tempCategories);
        }

        return returnedCategories;
    };

    return {
        getCategories
    }
}