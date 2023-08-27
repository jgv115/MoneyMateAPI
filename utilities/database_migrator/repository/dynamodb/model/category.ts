import { TransactionTypes } from "../../constants"

export interface DynamoDbCategory {
    UserIdQuery: string
    Subquery: string
    TransactionType: number
    Subcategories: string[]
}