#pragma once

#include <QtCore/qglobal.h>
#include <Interface/IPlugin.h>

#if defined(LAMPYRIS_DLL)
#define LAMPYRIS_MODULE_EXPORT Q_DECL_EXPORT
#else
#define LAMPYRIS_MODULE_EXPORT Q_DECL_IMPORT
#endif