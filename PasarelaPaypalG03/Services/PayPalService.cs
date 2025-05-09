using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PasarelaPaypalG03.Services
{
    public class PayPalService
    {
        private readonly HttpClient _httpClient;    // Cliente HTTP para realizar peticiones a PayPal
        private readonly IConfiguration _config;    // Configuración de la aplicación (aqui es para leer credenciales y URL)

        // Constructor con inyección de dependencias
        public PayPalService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        // Método para obtener el token de acceso OAuth 2.0 desde PayPal
        public async Task<string> GetAccessTokenAsync()
        {
            // Obtener las credenciales y la URL base desde configuración
            var clientId = _config["PayPal:ClientId"];
            var secret = _config["PayPal:ClientSecret"];
            var baseUrl = _config["PayPal:BaseUrl"];

            // Codificar las credenciales en Base64 para autenticación básica
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{secret}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            // Preparar el contenido del cuerpo para la solicitud del token
            var content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");

            // Enviar solicitud POST a PayPal para obtener el token
            var response = await _httpClient.PostAsync($"{baseUrl}/v1/oauth2/token", content);

            // Leer y deserializar la respuesta JSON
            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<JsonElement>(json);

            // Extraer y devolver el token de acceso
            return data.GetProperty("access_token").GetString();
        }

        // Método para crear una orden de pago en PayPal
        public async Task<string> CreateOrderAsync(string returnUrl, string cancelUrl)
        {
            // Obtener token de acceso válido
            var accessToken = await GetAccessTokenAsync();
            var baseUrl = _config["PayPal:BaseUrl"];

            // Limpiar y configurar headers para autenticación Bearer
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // Construir el cuerpo JSON de la orden
            var orderPayload = new
            {
                intent = "CAPTURE", // Indica que el pago se captura inmediatamente después de la aprobación
                purchase_units = new[]
                {
                    new {
                        amount = new {
                            currency_code = "USD",
                            value = "10.00" // Monto fijo por ejemplo
                        }
                    }
                },
                application_context = new
                {
                    return_url = returnUrl, // Redirige aquí si el usuario aprueba el pago
                    cancel_url = cancelUrl  // Redirige aquí si el usuario cancela el pago
                }
            };

            // Serializar el cuerpo a JSON y preparar la solicitud
            var json = JsonSerializer.Serialize(orderPayload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Enviar la solicitud POST para crear la orden
            var response = await _httpClient.PostAsync($"{baseUrl}/v2/checkout/orders", content);

            // Leer y deserializar la respuesta
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(body);

            // Buscar el enlace de aprobación (rel="approve") en la respuesta
            var approveUrl = result.GetProperty("links")
                .EnumerateArray()
                .First(link => link.GetProperty("rel").GetString() == "approve")
                .GetProperty("href")
                .GetString();

            // Devolver la URL a la que se debe redirigir al usuario
            return approveUrl;
        }

        // Método para capturar la orden de pago aprobada
        public async Task<string> CaptureOrderAsync(string orderId)
        {
            // Obtener token de acceso válido
            var accessToken = await GetAccessTokenAsync();
            var baseUrl = _config["PayPal:BaseUrl"];

            // Configurar autorización Bearer
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // Enviar solicitud POST para capturar el pago de la orden
            var response = await _httpClient.PostAsync($"{baseUrl}/v2/checkout/orders/{orderId}/capture", null);

            // Leer la respuesta como string (podría deserializarse para obtener detalles)
            var result = await response.Content.ReadAsStringAsync();

            // Devolver la respuesta cruda
            return result;
        }
    }
}
