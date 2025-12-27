using System.Text;
using Common;
using Models.Dto.V1.Requests;
using Models.Dto.V1.Responses;

namespace WebApi.Consumer.Clients;

public class OmsClient(HttpClient client)
{
    public async Task<V1AuditLogOrderResponse> LogOrder(V1AuditLogOrderRequest request, CancellationToken token)
    {
        try
        {
            var json = request.ToJson();
            var url = $"{client.BaseAddress}api/v1/audit/log-order";
            
            Console.WriteLine($"[OmsClient] Sending POST request to: {url}");
            Console.WriteLine($"[OmsClient] Request body: {json}");
            
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var msg = await client.PostAsync("api/v1/audit/log-order", content, token);
            
            var responseBody = await msg.Content.ReadAsStringAsync(cancellationToken: token);
            
            Console.WriteLine($"[OmsClient] Response status: {(int)msg.StatusCode} {msg.StatusCode}");
            Console.WriteLine($"[OmsClient] Response body: {responseBody}");
            
            if (msg.IsSuccessStatusCode)
            {
                var result = responseBody.FromJson<V1AuditLogOrderResponse>();
                Console.WriteLine($"[OmsClient] Successfully parsed response: Success={result.Success}");
                return result;
            }

            throw new HttpRequestException($"HTTP request failed with status {(int)msg.StatusCode} {msg.StatusCode}: {responseBody}");
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            Console.WriteLine($"[OmsClient] Request timeout - WebApi might be slow or not responding");
            throw new HttpRequestException("Request timeout - WebApi might be slow or not responding", ex);
        }
        catch (TaskCanceledException ex)
        {
            Console.WriteLine($"[OmsClient] Request cancelled: {ex.Message}");
            throw new HttpRequestException("Request was cancelled", ex);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"[OmsClient] HTTP Request Exception: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"[OmsClient] Inner exception: {ex.InnerException.Message}");
            }
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OmsClient] Unexpected error: {ex.GetType().Name}: {ex.Message}");
            Console.WriteLine($"[OmsClient] Stack trace: {ex.StackTrace}");
            throw;
        }
    }
}