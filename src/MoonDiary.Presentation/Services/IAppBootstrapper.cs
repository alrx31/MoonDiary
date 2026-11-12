namespace MoonDiary.Presentation.Services;

public interface IAppBootstrapper
{
    Task InitializeAsync();

    Task<string?> QuarantineDatabaseAsync();
}
