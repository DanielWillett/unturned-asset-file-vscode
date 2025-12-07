// dllmain.h : Declaration of module class.

class CUnturnedAssetFileIconHandlerModule : public ATL::CAtlDllModuleT< CUnturnedAssetFileIconHandlerModule >
{
public :
	DECLARE_LIBID(LIBID_UnturnedAssetFileIconHandlerLib)
	DECLARE_REGISTRY_APPID_RESOURCEID(IDR_UNTURNEDASSETFILEICONHANDLER, "{21a84523-c7ae-4fb7-9ec5-f24790490915}")
};

extern class CUnturnedAssetFileIconHandlerModule _AtlModule;
