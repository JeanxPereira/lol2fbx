using Microsoft.WindowsAPICodePack.Dialogs;

namespace lol2fbx.Utils;

public static class FileDialogUtils
{
    public static IEnumerable<CommonFileDialogFilter> CreateFbxFilters()
    {
        yield return new("FBX File", "fbx");
    }
}
