using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace TransactionService.Constants
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum TransactionType
    {
        [EnumMember(Value="expense")]
        Expense = 0,
        [EnumMember(Value="income")]
        Income = 1
    }
}