namespace MoonDiary.Controls;

public enum SheetOptionKind
{
    Normal,
    Primary,
    Destructive,
    Cancel
}

public sealed record SheetOption(string Text, SheetOptionKind Kind, object? Result);
