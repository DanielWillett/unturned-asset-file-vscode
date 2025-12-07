// IconHandler.cpp : Implementation of CIconHandler

#include "pch.h"
#include "IconHandler.h"


// CIconHandler

HRESULT __stdcall CIconHandler::GetClassID(CLSID* pClassID)
{
    return E_NOTIMPL;
}

HRESULT __stdcall CIconHandler::IsDirty(void)
{
    return E_NOTIMPL;
}

HRESULT __stdcall CIconHandler::Load(LPCOLESTR pszFileName, DWORD dwMode)
{
    return S_OK;
}

HRESULT __stdcall CIconHandler::Save(LPCOLESTR pszFileName, BOOL fRemember)
{
    return E_NOTIMPL;
}

HRESULT __stdcall CIconHandler::SaveCompleted(LPCOLESTR pszFileName)
{
    return E_NOTIMPL;
}

HRESULT __stdcall CIconHandler::GetCurFile(LPOLESTR* ppszFileName)
{
    return E_NOTIMPL;
}

HRESULT __stdcall CIconHandler::GetIconLocation(UINT uFlags, PWSTR pszIconFile, UINT cchMax, int* piIndex, UINT* pwFlags)
{
    if (s_ModulePath[0] == 0) {
        ::GetModuleFileName(_pModule->GetModuleInstance(), s_ModulePath, _countof(s_ModulePath));
    }
    if (s_ModulePath[0] == 0)
        return S_FALSE;

    wcscpy_s(pszIconFile, min((UINT)wcslen(s_ModulePath) + 1, cchMax), s_ModulePath);
    //ATLTRACE(L"CIconHandler::GetIconLocation: %s\n", pszIconFile);
    *piIndex = 0;

    // todo: if each file has a different icon this needs changed
    *pwFlags = GIL_PERCLASS;

    return S_OK;
}

HRESULT __stdcall CIconHandler::Extract(PCWSTR pszFile, UINT nIconIndex, HICON* phiconLarge, HICON* phiconSmall, UINT nIconSize)
{
    return S_FALSE;
}
