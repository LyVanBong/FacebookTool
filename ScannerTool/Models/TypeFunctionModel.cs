using Prism.Mvvm;

namespace ScannerTool.Models;

public class TypeFunctionModel : BindableBase
{
    private bool _isWebsite = true;
    private bool _isPostFacebook;
    private bool _isGroupMember;

    public bool IsWebsite
    {
        get => _isWebsite;
        set => SetProperty(ref _isWebsite, value);
    }

    public bool IsPostFacebook
    {
        get => _isPostFacebook;
        set => SetProperty(ref _isPostFacebook, value);
    }

    public bool IsGroupMember
    {
        get => _isGroupMember;
        set => SetProperty(ref _isGroupMember, value);
    }

    public int CheckTypeFunction()
    {
        if (IsWebsite) return 1;
        if (IsPostFacebook) return 2;
        if (IsGroupMember) return 3;
        return 0;
    }
}