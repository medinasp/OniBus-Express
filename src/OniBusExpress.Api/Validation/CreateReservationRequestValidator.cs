using FluentValidation;
using OniBusExpress.Api.Contracts;
using OniBusExpress.Domain.Passengers;

namespace OniBusExpress.Api.Validation;

public sealed class CreateReservationRequestValidator : AbstractValidator<CreateReservationRequest>
{
    public CreateReservationRequestValidator()
    {
        RuleFor(x => x.TripId).NotEmpty().WithMessage("A viagem é obrigatória.");
        RuleFor(x => x.Passenger).NotNull().WithMessage("Os dados do passageiro são obrigatórios.");

        When(x => x.Passenger is not null, () =>
        {
            RuleFor(x => x.Passenger!.Name)
                .Must(name => PassengerName.TryCreate(name, out _))
                .WithMessage("O nome do passageiro é obrigatório.");

            RuleFor(x => x.Passenger!.Cpf)
                .Must(cpf => Cpf.TryCreate(cpf, out _))
                .WithMessage("O CPF do passageiro é inválido.");
        });
    }
}
