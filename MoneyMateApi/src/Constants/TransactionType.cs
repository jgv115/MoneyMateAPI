using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace MoneyMateApi.Constants
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum TransactionType
    {
        [EnumMember(Value = "expense")] Expense = 0,
        [EnumMember(Value = "income")] Income = 1
    }

    public static class TransactionTypeExtensions
    {
        public static string ToProperString(this TransactionType transactionType)
        {
            switch (transactionType)
            {
                case TransactionType.Expense:
                    return "expense";
                case TransactionType.Income:
                    return "income";
                default:
                    throw new Exception("unexpected transaction type");
            }
        }

        public static TransactionType ConvertToTransactionType(string transactionTypeString)
        {
            switch (transactionTypeString)
            {
                case "expense":
                    return TransactionType.Expense;
                case "income":
                    return TransactionType.Income;
                default:
                    throw new Exception("invalid transaction type string");
            }
        }
    }
}