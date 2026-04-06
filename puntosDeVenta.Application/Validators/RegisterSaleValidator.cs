using FluentValidation;
using puntosDeVenta.Application.DTOs.Sales;

namespace puntosDeVenta.Application.Validators
{
    public class RegisterSaleValidator : AbstractValidator<RegisterSaleDTO>
    {
        public RegisterSaleValidator()
        {
            RuleFor(x => x.PosId)
                .NotEmpty().WithMessage("PosId es requerido")
                .MaximumLength(20).WithMessage("PosId no puede exceder 20 caracteres");

            RuleFor(x => x.CashierId)
                .NotEmpty().WithMessage("CashierId es requerido")
                .MaximumLength(10).WithMessage("CashierId no puede exceder 10 caracteres");

            RuleFor(x => x.SaleDate)
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("La fecha de venta no puede ser futura");

            RuleFor(x => x.TotalAmount)
                .GreaterThan(0).WithMessage("TotalAmount debe ser mayor a 0");

            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("Items no puede estar vac�o");

            RuleForEach(x => x.Items)
                .SetValidator(new SaleItemValidator())
                .When(x => x.Items != null);

            // Validaci�n personalizada: suma matem�tica
            RuleFor(x => x)
                .Custom((dto, context) =>
                {
                    decimal calculatedTotal = dto.Items.Sum(i => i.quantity * i.unit_price);
                    if (Math.Abs(calculatedTotal - dto.TotalAmount) > 0.01m)
                    {
                        context.AddFailure("TotalAmount", 
                            $"El total no coincide. Esperado: {calculatedTotal:F2}, Recibido: {dto.TotalAmount:F2}");
                    }
                });
        }
    }

    public class SaleItemValidator : AbstractValidator<SaleItemDTO>
    {
        public SaleItemValidator()
        {
            RuleFor(x => x.producto_id)
                .GreaterThan(0).WithMessage("ProductId debe ser mayor a 0");

            RuleFor(x => x.quantity)
                .GreaterThan(0).WithMessage("Quantity debe ser mayor a 0");

            RuleFor(x => x.unit_price)
                .GreaterThan(0).WithMessage("UnitPrice debe ser mayor a 0");
        }
    }
}