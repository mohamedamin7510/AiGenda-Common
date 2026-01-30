
namespace AI_genda_API.Contracts.Tasks;

public class TaskValidator : AbstractValidator<TaskRequest>
{

    public TaskValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required.")
            .MaximumLength(200).WithMessage("Content cannot exceed 200 characters.");

        RuleFor(x=>x.IsTaskFinished)
            .NotNull().WithMessage("Is Task Finished must be specified.");

    }

}
