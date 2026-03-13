namespace AI_genda_API.Contracts.Workspace;

public class InviteMemberRequestValidator : AbstractValidator<InviteMemberRequest>
{
    public InviteMemberRequestValidator()
    {
        RuleFor(x => x.email)
            .NotEmpty()
            .EmailAddress();

       
    }



}
