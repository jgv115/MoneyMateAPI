import { DynamoDbCategory, DynamoDbPayerPayee, DynamoDbTransaction } from "./model";
import { TransactionTypes } from "../constants";
import { NativeAttributeValue } from "@aws-sdk/util-dynamodb/dist-types/models"

export const DynamoDbCategoriesMapper = (items: Record<string, NativeAttributeValue>[]): DynamoDbCategory[] =>
    items.map(item => ({
        UserIdQuery: item.UserIdQuery,
        Subquery: item.Subquery,
        TransactionType: item.TransactionType as TransactionTypes,
        Subcategories: item.Subcategories
    }));

export const DynamoDbPayersPayeesMapper = (items: Record<string, NativeAttributeValue>[]): DynamoDbPayerPayee[] =>
    items.map(item => ({
        UserIdQuery: item.UserIdQuery,
        Subquery: item.Subquery,
        ExternalId: item.ExternalId,
        PayerPayeeName: item.PayerPayeeName
    }))

export const DynamoDbTransactionMapper = (items: Record<string, NativeAttributeValue>[]): DynamoDbTransaction[] =>
    items.map(item => ({
        UserIdQuery: item.UserIdQuery,
        Subquery: item.Subquery,
        Amount: item.Amount,
        Category: item.Category,
        PayerPayeeId: item.PayerPayeeId,
        PayerPayeeName: item.PayerPayeeName,
        SubCategory: item.SubCategory,
        TransactionTimestamp: item.TransactionTimestamp,
        TransactionType: item.TransactionType as TransactionTypes,
        Note: item.Note
    }))