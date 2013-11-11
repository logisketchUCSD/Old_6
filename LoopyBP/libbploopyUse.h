/*
 * MATLAB Compiler: 4.2 (R14SP2)
 * Date: Fri Jun 23 12:04:55 2006
 * Arguments: "-B" "macro_default" "-B" "csharedlib:libbploopyUse" "-W"
 * "lib:libbploopyUse" "-T" "link:lib" "bploopyUse.m" 
 */

#ifndef __libbploopyUse_h
#define __libbploopyUse_h 1

#if defined(__cplusplus) && !defined(mclmcr_h) && defined(__linux__)
#  pragma implementation "mclmcr.h"
#endif
#include "mclmcr.h"
#ifdef __cplusplus
extern "C" {
#endif

#if defined(__SUNPRO_CC)
/* Solaris shared libraries use __global, rather than mapfiles
 * to define the API exported from a shared library. __global is
 * only necessary when building the library -- files including
 * this header file to use the library do not need the __global
 * declaration; hence the EXPORTING_<library> logic.
 */

#ifdef EXPORTING_libbploopyUse
#define PUBLIC_libbploopyUse_C_API __global
#else
#define PUBLIC_libbploopyUse_C_API /* No import statement needed. */
#endif

#define LIB_libbploopyUse_C_API PUBLIC_libbploopyUse_C_API

#elif defined(_HPUX_SOURCE)

#ifdef EXPORTING_libbploopyUse
#define PUBLIC_libbploopyUse_C_API __declspec(dllexport)
#else
#define PUBLIC_libbploopyUse_C_API __declspec(dllimport)
#endif

#define LIB_libbploopyUse_C_API PUBLIC_libbploopyUse_C_API


#else

#define LIB_libbploopyUse_C_API

#endif

/* This symbol is defined in shared libraries. Define it here
 * (to nothing) in case this isn't a shared library. 
 */
#ifndef LIB_libbploopyUse_C_API 
#define LIB_libbploopyUse_C_API /* No special import/export declaration */
#endif

extern LIB_libbploopyUse_C_API 
bool libbploopyUseInitializeWithHandlers(mclOutputHandlerFcn error_handler,
                                         mclOutputHandlerFcn print_handler);

extern LIB_libbploopyUse_C_API 
bool libbploopyUseInitialize(void);

extern LIB_libbploopyUse_C_API 
void libbploopyUseTerminate(void);


extern LIB_libbploopyUse_C_API 
void mlxBploopyUse(int nlhs, mxArray *plhs[], int nrhs, mxArray *prhs[]);


extern LIB_libbploopyUse_C_API void mlfBploopyUse(int nargout, mxArray** belBP
                                                  , mxArray** belE
                                                  , mxArray** logZ, mxArray* G
                                                  , mxArray* pot
                                                  , mxArray* localEv);

#ifdef __cplusplus
}
#endif

#endif
