using MoonDiary.Core.Transfer;

namespace MoonDiary.Core.Abstractions;

public interface IDataTransferRepository
{
    Task<ExportBundle> ReadAllAsync();

    Task ReplaceAllAsync(ExportBundle bundle);
}
