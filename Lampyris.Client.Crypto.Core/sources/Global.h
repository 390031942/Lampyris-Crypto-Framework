#pragma once

#include <QtCore/qglobal.h>
#include <interface/IPlugin.h>

#if defined(LAMPYRISCLIENTCRYPTOCORE_LIBRARY)
#define LAMPYRIS_CLIENT_CRYPTO_CORE_EXPORT Q_DECL_EXPORT
#else
#define LAMPYRIS_CLIENT_CRYPTO_CORE_EXPORT Q_DECL_IMPORT
#endif

#if defined(Q_OS_WINDOWS) || defined(Q_OS_LINUX) || defined(Q_OS_MAC)
#define LAMPYRIS_STANDALONG
#elif defined(Q_OS_ANDROID) || defined(Q_OS_IOS)
#define LAMPYRIS_MOBILE
#endif
