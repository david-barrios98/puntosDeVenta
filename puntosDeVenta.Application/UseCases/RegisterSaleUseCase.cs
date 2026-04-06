using System;
using System.Threading.Tasks;
using FluentValidation;
using puntosDeVenta.Application.DTOs.Sales;
using puntosDeVenta.Application.Ports.Inbound;
using puntosDeVenta.Application.Ports.Outbound;

namespace puntosDeVenta.Application.UseCases
{
    /// <summary>
    /// Caso de uso: Registrar una venta desde un punto de venta
    /// Orquesta: Validación → Persistencia → Publicación de evento
    /// </summary>
    public class RegisterSaleUseCase : IRegisterSaleUseCase
    {
        private readonly ISalesRepositoryAdapter _repository;
        private readonly IMessagePublisher _messagePublisher;
        private readonly IValidator<RegisterSaleDTO> _validator;

        public RegisterSaleUseCase(
            ISalesRepositoryAdapter repository,
            IMessagePublisher messagePublisher,
            IValidator<RegisterSaleDTO> validator)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _messagePublisher = messagePublisher ?? throw new ArgumentNullException(nameof(messagePublisher));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        /// <summary>
        /// Ejecuta el flujo completo de registro de venta
        /// </summary>
        public async Task<SaleRegisteredEventDTO> ExecuteAsync(RegisterSaleDTO sale)
        {
            // Validación de estructura y cálculos
            var validationResult = await _validator.ValidateAsync(sale).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            // 1. Registrar en BD (con transacción en SP)
            var saleId = await _repository.RegisterSaleAsync(sale).ConfigureAwait(false);

            // 2. Crear evento de dominio
            var @event = new SaleRegisteredEventDTO
            {
                InternalSaleId = saleId,
                PosId = sale.PosId,
                TotalAmount = sale.TotalAmount,
                ProcessedAt = DateTime.UtcNow
            };

            // 3. Publicar evento (asíncrono, no bloquea respuesta del POS)
            // Publicación en background y manejo de errores con logging
            _ = Task.Run(async () =>
            {
                try
                {
                    await _messagePublisher.PublishSaleRegisteredAsync(@event).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    // No revertir la venta por fallo en la mensajería — solo loguear
                }
            });

            // 4. Retornar evento al POS inmediatamente
            return @event;
        }
    }
}