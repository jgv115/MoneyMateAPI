import { BillingMode, CreateTableCommand, DeleteTableCommand, DynamoDBClient, ScanCommand } from "@aws-sdk/client-dynamodb";
import { DynamoDBDocumentClient, PutCommand } from "@aws-sdk/lib-dynamodb";

export const DynamoDbHelpers = () => {

    const tableName = "MoneyMate_TransactionDB"
    const client = new DynamoDBClient({
        endpoint: "http://localhost:4566"
    });
    const docClient = DynamoDBDocumentClient.from(client);

    const createMoneyMateDbTable = async () => {
        const command = new CreateTableCommand({
            TableName: tableName,
            AttributeDefinitions: [
                {
                    AttributeName: "UserIdQuery",
                    AttributeType: "S"
                },
                {
                    AttributeName: "Subquery",
                    AttributeType: "S"
                },
                // {
                //     AttributeName: "TransactionTimestamp",
                //     AttributeType: "S"
                // },
                // {
                //     AttributeName: "PayerPayeeName",
                //     AttributeType: "S"
                // }
            ],
            KeySchema: [
                {
                    AttributeName: "UserIdQuery",
                    KeyType: "HASH"
                },
                {
                    AttributeName: "Subquery",
                    KeyType: "RANGE"
                }
            ],
            BillingMode: BillingMode.PAY_PER_REQUEST
        });

        await client.send(command);
    }

    const destroyMoneyMateDbTable = async () => {
        const command = new DeleteTableCommand({
            TableName: tableName
        });

        await client.send(command);
    }

    const saveItemToDynamoDbTable = async (item: any) => {
        const command = new PutCommand({
            TableName: tableName,
            Item: item
        });

        await docClient.send(command);
        
    };

    const scanTable = async () => {
        const command = new ScanCommand({
            TableName: tableName,

        });
        const response = await docClient.send(command);


        return response.Items;
    }

    return {
        dynamoDbClient: client,
        tableName,
        createMoneyMateDbTable,
        destroyMoneyMateDbTable,
        saveItemToDynamoDbTable
    }
}