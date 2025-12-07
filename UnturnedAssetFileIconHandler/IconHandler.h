// IconHandler.h : Declaration of the CIconHandler
// Code adapted from https://scorpiosoftware.net/2021/12/11/icon-handler-with-atl/

#pragma once
#include "resource.h"       // main symbols
#include <shlobj_core.h>



#include "UnturnedAssetFileIconHandler_i.h"



#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif

using namespace ATL;


// CIconHandler

class ATL_NO_VTABLE CIconHandler :
	public CComObjectRootEx<CComSingleThreadModel>,
	public CComCoClass<CIconHandler, &CLSID_IconHandler>,
	public IPersistFile,
	public IExtractIcon
{
public:
	CIconHandler()
	{
	}

	DECLARE_REGISTRY_RESOURCEID(106)


	BEGIN_COM_MAP(CIconHandler)
		COM_INTERFACE_ENTRY(IPersistFile)
		COM_INTERFACE_ENTRY(IExtractIcon)
	END_COM_MAP()



	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct()
	{
		return S_OK;
	}

	void FinalRelease()
	{
	}

private:

	// Inherited via IPersistFile
	HRESULT __stdcall GetClassID(CLSID* pClassID) override;
	HRESULT __stdcall IsDirty(void) override;
	HRESULT __stdcall Load(LPCOLESTR pszFileName, DWORD dwMode) override;
	HRESULT __stdcall Save(LPCOLESTR pszFileName, BOOL fRemember) override;
	HRESULT __stdcall SaveCompleted(LPCOLESTR pszFileName) override;
	HRESULT __stdcall GetCurFile(LPOLESTR* ppszFileName) override;

	// Inherited via IExtractIconW
	HRESULT __stdcall GetIconLocation(UINT uFlags, PWSTR pszIconFile, UINT cchMax, int* piIndex, UINT* pwFlags) override;
	HRESULT __stdcall Extract(PCWSTR pszFile, UINT nIconIndex, HICON* phiconLarge, HICON* phiconSmall, UINT nIconSize) override;

	WCHAR s_ModulePath[MAX_PATH]{ };
};

OBJECT_ENTRY_AUTO(__uuidof(IconHandler), CIconHandler)
