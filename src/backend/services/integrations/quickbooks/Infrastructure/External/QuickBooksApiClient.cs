using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Harvestry.Integration.QuickBooks.Infrastructure.External;

/// <summary>
/// HTTP client for QuickBooks Online API.
/// Handles OAuth2 authentication and API requests.
/// </summary>
public sealed class QuickBooksApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<QuickBooksApiClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    private const string QBO_BASE_URL = "https://quickbooks.api.intuit.com/v3/company";
    private const string QBO_SANDBOX_URL = "https://sandbox-quickbooks.api.intuit.com/v3/company";

    public QuickBooksApiClient(
        HttpClient httpClient,
        ILogger<QuickBooksApiClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Makes an authenticated GET request to QuickBooks
    /// </summary>
    public async Task<QuickBooksResponse<T>> GetAsync<T>(
        string realmId,
        string accessToken,
        string endpoint,
        bool useSandbox = false,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = useSandbox ? QBO_SANDBOX_URL : QBO_BASE_URL;
        var url = $"{baseUrl}/{realmId}/{endpoint}";

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request, cancellationToken);
            return await ProcessResponseAsync<T>(response, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "QuickBooks API error for {Endpoint}", endpoint);
            return QuickBooksResponse<T>.Error("EXCEPTION", ex.Message);
        }
    }

    /// <summary>
    /// Makes an authenticated POST request to QuickBooks
    /// </summary>
    public async Task<QuickBooksResponse<T>> PostAsync<T>(
        string realmId,
        string accessToken,
        string endpoint,
        object payload,
        bool useSandbox = false,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = useSandbox ? QBO_SANDBOX_URL : QBO_BASE_URL;
        var url = $"{baseUrl}/{realmId}/{endpoint}";

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = JsonContent.Create(payload, options: _jsonOptions);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            return await ProcessResponseAsync<T>(response, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "QuickBooks API POST error for {Endpoint}", endpoint);
            return QuickBooksResponse<T>.Error("EXCEPTION", ex.Message);
        }
    }

    /// <summary>
    /// Gets all items from QuickBooks
    /// </summary>
    public async Task<QuickBooksResponse<QuickBooksQueryResponse<QuickBooksItem>>> GetItemsAsync(
        string realmId,
        string accessToken,
        int maxResults = 1000,
        bool useSandbox = false,
        CancellationToken cancellationToken = default)
    {
        var query = $"query?query=SELECT * FROM Item MAXRESULTS {maxResults}";
        return await GetAsync<QuickBooksQueryResponse<QuickBooksItem>>(
            realmId, accessToken, query, useSandbox, cancellationToken);
    }

    /// <summary>
    /// Creates an item in QuickBooks
    /// </summary>
    public async Task<QuickBooksResponse<QuickBooksItem>> CreateItemAsync(
        string realmId,
        string accessToken,
        QuickBooksItem item,
        bool useSandbox = false,
        CancellationToken cancellationToken = default)
    {
        return await PostAsync<QuickBooksItem>(
            realmId, accessToken, "item", item, useSandbox, cancellationToken);
    }

    /// <summary>
    /// Creates an invoice in QuickBooks
    /// </summary>
    public async Task<QuickBooksResponse<QuickBooksInvoice>> CreateInvoiceAsync(
        string realmId,
        string accessToken,
        QuickBooksInvoice invoice,
        bool useSandbox = false,
        CancellationToken cancellationToken = default)
    {
        return await PostAsync<QuickBooksInvoice>(
            realmId, accessToken, "invoice", invoice, useSandbox, cancellationToken);
    }

    /// <summary>
    /// Creates a bill in QuickBooks
    /// </summary>
    public async Task<QuickBooksResponse<QuickBooksBill>> CreateBillAsync(
        string realmId,
        string accessToken,
        QuickBooksBill bill,
        bool useSandbox = false,
        CancellationToken cancellationToken = default)
    {
        return await PostAsync<QuickBooksBill>(
            realmId, accessToken, "bill", bill, useSandbox, cancellationToken);
    }

    private async Task<QuickBooksResponse<T>> ProcessResponseAsync<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("QuickBooks API error: {StatusCode} - {Content}",
                response.StatusCode, content);

            return QuickBooksResponse<T>.Error(
                ((int)response.StatusCode).ToString(),
                content);
        }

        var data = JsonSerializer.Deserialize<T>(content, _jsonOptions);
        return QuickBooksResponse<T>.Success(data);
    }
}

/// <summary>
/// Response wrapper for QuickBooks API
/// </summary>
public sealed class QuickBooksResponse<T>
{
    public bool IsSuccess { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
    public T? Data { get; init; }

    public static QuickBooksResponse<T> Success(T? data) => new()
    {
        IsSuccess = true,
        Data = data
    };

    public static QuickBooksResponse<T> Error(string code, string message) => new()
    {
        IsSuccess = false,
        ErrorCode = code,
        ErrorMessage = message
    };
}

/// <summary>
/// QuickBooks query response wrapper
/// </summary>
public sealed class QuickBooksQueryResponse<T>
{
    public QueryResponseInner<T>? QueryResponse { get; set; }
}

public sealed class QueryResponseInner<T>
{
    public List<T>? Item { get; set; }
    public int? StartPosition { get; set; }
    public int? MaxResults { get; set; }
    public int? TotalCount { get; set; }
}

/// <summary>
/// QuickBooks Item entity
/// </summary>
public sealed class QuickBooksItem
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Type { get; set; }
    public decimal? UnitPrice { get; set; }
    public bool? Active { get; set; }
    public bool? Taxable { get; set; }
    public string? Sku { get; set; }
    public decimal? QtyOnHand { get; set; }
    public string? SyncToken { get; set; }
}

/// <summary>
/// QuickBooks Invoice entity
/// </summary>
public sealed class QuickBooksInvoice
{
    public string? Id { get; set; }
    public string? DocNumber { get; set; }
    public string? TxnDate { get; set; }
    public QuickBooksCustomerRef? CustomerRef { get; set; }
    public List<QuickBooksLine>? Line { get; set; }
    public decimal? TotalAmt { get; set; }
    public string? SyncToken { get; set; }
}

/// <summary>
/// QuickBooks Bill entity
/// </summary>
public sealed class QuickBooksBill
{
    public string? Id { get; set; }
    public string? DocNumber { get; set; }
    public string? TxnDate { get; set; }
    public string? DueDate { get; set; }
    public QuickBooksVendorRef? VendorRef { get; set; }
    public List<QuickBooksLine>? Line { get; set; }
    public decimal? TotalAmt { get; set; }
    public string? SyncToken { get; set; }
}

public sealed class QuickBooksCustomerRef
{
    public string? Value { get; set; }
    public string? Name { get; set; }
}

public sealed class QuickBooksVendorRef
{
    public string? Value { get; set; }
    public string? Name { get; set; }
}

public sealed class QuickBooksLine
{
    public decimal? Amount { get; set; }
    public string? DetailType { get; set; }
    public QuickBooksSalesItemLineDetail? SalesItemLineDetail { get; set; }
    public QuickBooksAccountBasedExpenseLineDetail? AccountBasedExpenseLineDetail { get; set; }
}

public sealed class QuickBooksSalesItemLineDetail
{
    public QuickBooksItemRef? ItemRef { get; set; }
    public decimal? Qty { get; set; }
    public decimal? UnitPrice { get; set; }
}

public sealed class QuickBooksAccountBasedExpenseLineDetail
{
    public QuickBooksAccountRef? AccountRef { get; set; }
}

public sealed class QuickBooksItemRef
{
    public string? Value { get; set; }
    public string? Name { get; set; }
}

public sealed class QuickBooksAccountRef
{
    public string? Value { get; set; }
    public string? Name { get; set; }
}
