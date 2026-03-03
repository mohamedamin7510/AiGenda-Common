
namespace AI_genda_API.Contracts.Workspace;

public class WorkSpaceRequestValidator:AbstractValidator<WorkSpaceRequest>
{
    public WorkSpaceRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .Length(3, 60);

        RuleFor(x => x.Description)
            .MaximumLength(300);

        RuleFor(x => x.IconPath)
           .MaximumLength(500);

    }
}
