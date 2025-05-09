using Microsoft.AspNetCore.Mvc;
using PasarelaPaypalG03.Services;

namespace PasarelaPaypalG03.Controllers
{
    // Este es el controlador para manejar pagos usando PayPal
    public class PaymentController : Controller
    {
        private readonly PayPalService _paypal;

        // Inyecta el servicio de PayPal a través del constructor
        public PaymentController(PayPalService paypal)
        {
            _paypal = paypal;
        }

        // Crea una orden de pago y redirige al usuario a PayPal para aprobación
        public async Task<IActionResult> CreateOrder()
        {
            //  Esta es la URL a la que PayPal redirigirá después de aprobar el pago
            var returnUrl = Url.Action("CaptureOrder", "Payment", null, Request.Scheme);

            // Esta es la URL a la que PayPal redirigirá si el usuario cancela el pago
            var cancelUrl = Url.Action("Cancel", "Payment", null, Request.Scheme);

            // Aqui llama al servicio para crear la orden de PayPal y obtener la URL de aprobación
            var approvalUrl = await _paypal.CreateOrderAsync(returnUrl, cancelUrl);

            // Se redirige al usuario a PayPal para aprobar el pago
            return Redirect(approvalUrl);
        }

        // Captura la orden de pago después de que el usuario apruebe la transacción en PayPal
        public async Task<IActionResult> CaptureOrder(string token)
        {
            // Captura la orden usando el token proporcionado por PayPal
            var result = await _paypal.CaptureOrderAsync(token);

            // Muestra una vista de éxito (puede personalizarse con detalles del pago)
            return View("Success");
        }

        // Muestra la vista de cancelación si el usuario cancela el pago en PayPal
        public IActionResult Cancel()
        {
            return View("Cancel");
        }
    }
}