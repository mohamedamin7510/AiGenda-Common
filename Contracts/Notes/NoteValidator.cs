
namespace AI_genda_API.Contracts.Tasks;

public class NoteValidator : AbstractValidator<NoteRequest>
{

    public NoteValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required.")
            .MaximumLength(200).WithMessage("Content cannot exceed 200 characters.");

        RuleFor(x=>x.IsTaskFinished)
            .NotNull().WithMessage("Is Task Finished must be specified.");

    }

}
