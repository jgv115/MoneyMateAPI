export interface Transaction {
    id: string;
    user_id: string;
    transaction_timestamp: string;
    transaction_type_id: string;
    amount: number;
    subcategory_id: string;
    payerpayee_id: string;
    notes: string;
}