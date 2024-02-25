using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace YnabCategoryAi.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ClearedEnum
{
    [EnumMember(Value = "cleared")]
    Cleared = 1,

    [EnumMember(Value = "uncleared")]
    Uncleared = 2,

    [EnumMember(Value = "reconciled")]
    Reconciled = 3
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FlagColorEnum
{
    [EnumMember(Value = "red")]
    Red = 1,

    [EnumMember(Value = "orange")]
    Orange = 2,

    [EnumMember(Value = "yellow")]
    Yellow = 3,

    [EnumMember(Value = "green")]
    Green = 4,

    [EnumMember(Value = "blue")]
    Blue = 5,

    [EnumMember(Value = "purple")]
    Purple = 6
}

public class TransactionsResponseDto
{
    public TransactionsResponseDataDto Data { get; set; }
}

public class TransactionsResponseDataDto
{
    public TransactionDto[] Transactions { get; set; }
}

// Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
public record TransactionDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("amount")]
    public int? Amount { get; set; }

    [JsonPropertyName("memo")]
    public string Memo { get; set; }

    [JsonPropertyName("cleared")]
    public ClearedEnum Cleared { get; set; }

    [JsonPropertyName("approved")]
    public bool Approved { get; set; }

    [JsonPropertyName("flag_color")]
    public FlagColorEnum? FlagColor { get; set; }

    [JsonPropertyName("flag_name")]
    public string FlagName { get; set; }

    [JsonPropertyName("account_id")]
    public string AccountId { get; set; }

    [JsonPropertyName("account_name")]
    public string AccountName { get; set; }

    [JsonPropertyName("payee_id")]
    public string PayeeId { get; set; }

    [JsonPropertyName("payee_name")]
    public string PayeeName { get; set; }

    [JsonPropertyName("category_id")]
    public string CategoryId { get; set; }

    [JsonPropertyName("category_name")]
    public string CategoryName { get; set; }

    [JsonPropertyName("transfer_account_id")]
    public string? TransferAccountId { get; set; }

    [JsonPropertyName("transfer_transaction_id")]
    public string? TransferTransactionId { get; set; }

    [JsonPropertyName("matched_transaction_id")]
    public string? MatchedTransactionId { get; set; }

    [JsonPropertyName("import_id")]
    public string ImportId { get; set; }

    [JsonPropertyName("import_payee_name")]
    public string ImportPayeeName { get; set; }

    [JsonPropertyName("import_payee_name_original")]
    public string ImportPayeeNameOriginal { get; set; }

    [JsonPropertyName("debt_transaction_type")]
    public object DebtTransactionType { get; set; }

    [JsonPropertyName("deleted")]
    public bool Deleted { get; set; }

    [JsonPropertyName("subtransactions")]
    public List<SubtransactionDto> Subtransactions { get; set; }
}

public class SubtransactionDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("transaction_id")]
    public string TransactionId { get; set; }

    [JsonPropertyName("amount")]
    public int? Amount { get; set; }

    [JsonPropertyName("memo")]
    public string Memo { get; set; }

    [JsonPropertyName("payee_id")]
    public string PayeeId { get; set; }

    [JsonPropertyName("payee_name")]
    public string PayeeName { get; set; }

    [JsonPropertyName("category_id")]
    public string CategoryId { get; set; }

    [JsonPropertyName("category_name")]
    public string CategoryName { get; set; }

    [JsonPropertyName("transfer_account_id")]
    public string TransferAccountId { get; set; }

    [JsonPropertyName("transfer_transaction_id")]
    public string TransferTransactionId { get; set; }

    [JsonPropertyName("deleted")]
    public bool? Deleted { get; set; }
}