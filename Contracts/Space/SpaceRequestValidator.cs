namespace AI_genda_API.Contracts.Space;

public class SpaceRequestValidator : AbstractValidator<SpaceRequest>
{
    public SpaceRequestValidator()
    {
        RuleFor(x => x.Name)
             .NotEmpty()
             .MaximumLength(60);

        RuleFor(x => x.Descreption)
             .NotEmpty()
             .MaximumLength(300);

        RuleFor(x => x.IconHexa)
             .NotEmpty()
             .MaximumLength(500);
    }

}
