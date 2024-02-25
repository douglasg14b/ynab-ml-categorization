using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using YnabCategoryAi.Models;

namespace YnabCategoryAi;

public static class Utilities
{
    public static IEnumerable<string> CreateNGrams(this string text, int nMin, int nMax)
    {
        for (int n = nMin; n <= nMax; n++)
        {
            for (int i = 0; i < text.Length - n + 1; i++)
            {
                yield return text.Substring(i, n);
            }
        }
    }

    public static double CalculateCosineSimilarity(Dictionary<string, double> vec1, Dictionary<string, double> vec2)
    {
        IEnumerable<string> intersection = vec1.Keys.Intersect(vec2.Keys);
        double dotProduct = intersection.Sum(t => vec1[t] * vec2[t]);

        double magnitude1 = Math.Sqrt(vec1.Sum(kvp => kvp.Value * kvp.Value));
        double magnitude2 = Math.Sqrt(vec2.Sum(kvp => kvp.Value * kvp.Value));

        if (magnitude1 == 0 || magnitude2 == 0) return 0;

        return dotProduct / (magnitude1 * magnitude2);
    }

    public static double GetSimilarity(this string a, string b)
    {
        if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b)) return 0;

        Dictionary<string, double> vec1 = a.Preprocess().CreateNGrams(2, 3).GroupBy(x => x)
            .ToDictionary(x => x.Key, x => (double)x.Count());
        Dictionary<string, double> vec2 = b.Preprocess().CreateNGrams(2, 3).GroupBy(x => x)
            .ToDictionary(x => x.Key, x => (double)x.Count());

        return CalculateCosineSimilarity(vec1, vec2);
    }

    public static string? Preprocess(this string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;

        return Regex.Replace(input.ToLower().AsAscii(), @"[^\w\s]", "");
    }

    // https://stackoverflow.com/a/135473/3547347
    public static string AsAscii(this string value)
    {
        return Encoding.ASCII.GetString(
            Encoding.Convert(
                Encoding.UTF8,
                Encoding.GetEncoding(
                    Encoding.ASCII.EncodingName,
                    new EncoderReplacementFallback(string.Empty),
                    new DecoderExceptionFallback()
                ),
                Encoding.UTF8.GetBytes(value)
            )
        );
    }

    public static TransactionDto[] GetExpandedTransactionsData()
    {
        var random = new Random();
        TransactionDto[] transactions = GetFilteredTransactions();

        TransactionDto[] directNames = transactions.Select(x => x with { ImportPayeeNameOriginal = x.PayeeName }).ToArray();
        TransactionDto[] directNames2 = transactions.Select(x => x with { ImportPayeeNameOriginal = x.ImportPayeeName }).ToArray();

        TransactionDto[] shuffledNames1 = transactions
            .Select(x => x with { ImportPayeeNameOriginal = $"{x.ImportPayeeName} {random.Next(1000, 50000)}" })
            .ToArray();

        TransactionDto[] shuffledNames2 = transactions
            .Select(x => x with { ImportPayeeNameOriginal = $"{random.Next(1000, 50000)} {x.ImportPayeeName}" })
            .ToArray();

        return transactions
            .Concat(directNames)
            .Concat(directNames2)
            .Concat(shuffledNames1)
            .Concat(shuffledNames2)
            .ToArray();
    }

    public static TransactionDto[] GetUnapprovedTransactions()
    {
        TransactionDto[] transactions = LoadTransactions();

        return transactions
            .Where(x => x.Cleared == ClearedEnum.Cleared)
            .Where(x => !x.Subtransactions.Any())
            .Where(x => !x.Approved)
            .Where(x => !x.Deleted)
            .Where(x => x.TransferAccountId == null)
            .ToArray();
    }

    public static TransactionDto[] GetFilteredTransactions()
    {
        TransactionDto[] transactions = LoadTransactions();

        return transactions
            .Where(x => x.Cleared == ClearedEnum.Cleared)
            .Where(x => !x.Subtransactions.Any())
            .Where(x => x.Approved)
            .Where(x => !x.Deleted)
            .ToArray();
    }

    public static TransactionDto[] LoadTransactions()
    {
        string fileJson = File.ReadAllText("transactions.json");
        // var transactionsData = JsonConvert.DeserializeObject<TransactionsResponse>(fileJson);

        var transactionsData =
            JsonSerializer.Deserialize<TransactionsResponseDto>(fileJson, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        return transactionsData.Data.Transactions.ToArray();
    }
}