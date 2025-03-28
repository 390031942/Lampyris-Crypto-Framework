QT += widgets network

TEMPLATE = lib
DEFINES += LAMPYRISCLIENTCRYPTOCORE_LIBRARY

CONFIG += c++17

# You can make your code fail to compile if it uses deprecated APIs.
# In order to do so, uncomment the following line.
#DEFINES += QT_DISABLE_DEPRECATED_BEFORE=0x060000    # disables all the APIs deprecated before Qt 6.0.0

INCLUDEPATH += \
    $$PWD/../Lampyris.Client.Common/includes/ \
    $$PWD/sources/

SOURCES += \
    sources/PluginEntryPoint.cpp \
    sources/UI/Mobile/MainWidget.cpp \
    sources/UI/Standalong/MainStatusBar.cpp \
    sources/UI/Standalong/MainTitleBar.cpp \
    sources/UI/Standalong/MainToolBar.cpp \
    sources/UI/Standalong/MainWidget.cpp

HEADERS += \
    sources/Global.h \
    sources/PluginEntryPoint.h \
    sources/UI/Mobile/MainWidget.h \
    sources/UI/Standalong/MainStatusBar.h \
    sources/UI/Standalong/MainTitleBar.h \
    sources/UI/Standalong/MainToolBar.h \
    sources/UI/Standalong/MainWidget.h

# Default rules for deployment.
unix {
    target.path = /usr/lib
}
!isEmpty(target.path): INSTALLS += target

FORMS += \
    sources/UI/Standalong/MainWidget.ui
