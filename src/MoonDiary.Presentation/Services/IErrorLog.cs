namespace MoonDiary.Presentation.Services;

public interface IErrorLog
{
    void Write(string source, Exception exception);
}
