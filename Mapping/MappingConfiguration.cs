using AI_genda_API.Contracts.Folders;
using AI_genda_API.Contracts.Tasks;
using Mapster;

namespace AI_genda_API.Mapping;

public class MappingConfiguration : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<TaskRequest, task>()
            .TwoWays();

        config.NewConfig<FolderRequset, Folder>()
            .TwoWays();

        config.NewConfig<Folder, FolderResponse>()
            .TwoWays();

    }
}
