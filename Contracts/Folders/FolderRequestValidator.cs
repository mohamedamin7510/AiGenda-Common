namespace AI_genda_API.Contracts.Folders;

public class FolderRequestValidator:AbstractValidator<FolderRequset>
{
    public FolderRequestValidator()
    {
        RuleFor(x=>x.Name).NotNull().MaximumLength(50);
    }
}
