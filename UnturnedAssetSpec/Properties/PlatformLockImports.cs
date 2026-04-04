#if NET9_0_OR_GREATER
global using TfmLock = System.Threading.Lock;
#else
global using TfmLock = object;
#endif