using System.Text;
using Android.App;
using Android.Content;

namespace MoonDiary;

public static class DocumentSaver
{
    private const int RequestCode = 4101;

    private static TaskCompletionSource<global::Android.Net.Uri?>? pending;

    public static async Task<bool> SaveAsync(string fileName, string content)
    {
        var activity = Platform.CurrentActivity;

        if (activity is null)
        {
            return false;
        }

        var intent = new Intent(Intent.ActionCreateDocument);
        intent.AddCategory(Intent.CategoryOpenable);
        intent.SetType("application/json");
        intent.PutExtra(Intent.ExtraTitle, fileName);

        var completion = new TaskCompletionSource<global::Android.Net.Uri?>();
        pending = completion;
        MainThread.BeginInvokeOnMainThread(() => activity.StartActivityForResult(intent, RequestCode));

        var uri = await completion.Task;

        if (uri is null)
        {
            return false;
        }

        await using var stream = activity.ContentResolver?.OpenOutputStream(uri, "wt")
                                 ?? throw new IOException("Не удалось открыть файл для записи");
        await using var writer = new StreamWriter(stream, new UTF8Encoding(false));
        await writer.WriteAsync(content);

        return true;
    }

    public static void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        if (requestCode != RequestCode)
        {
            return;
        }

        pending?.TrySetResult(resultCode == Result.Ok ? data?.Data : null);
        pending = null;
    }
}
