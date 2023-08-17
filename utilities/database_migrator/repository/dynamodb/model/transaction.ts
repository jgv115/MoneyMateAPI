import { TransactionTypes } from "../../constants"

export interface DynamoDbTransaction {
    UserIdQuery: string
    Subquery: string
    TransactionTimestamp: string
    TransactionType: TransactionTypes
    Amount: number
    Category: string
    SubCategory: string
    PayerPayeeId: string
    PayerPayeeName: string
    Note: string
}