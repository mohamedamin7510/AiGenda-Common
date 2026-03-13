
namespace AI_genda_API.Contracts.Workspace;

using FluentValidation;

public class WorkSpaceRequestValidator : AbstractValidator<WorkSpaceRequest>
{
    public WorkSpaceRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(60);

        RuleFor(x => x.Description)
            .MaximumLength(300);

        RuleFor(x => x.IconCode)
            .MaximumLength(500);
   
    }



}
