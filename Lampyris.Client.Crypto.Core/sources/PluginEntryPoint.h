#pragma once

// Project Include(s)
#include "Global.h"

#if defined(LAMPYRIS_EXE)
#include "UI/Standalong/Main/MainWidget.h"
#include <QApplication>
int main(int argc, char** argv) {
    QApplication a(argc, argv);
    MainWidget mainWidget;
    mainWidget.show();
    return a.exec();
}

#else 
class LAMPYRIS_CLIENT_CRYPTO_CORE_EXPORT PluginEntryPoint :public IPlugin {
public:
    virtual int main(const QStringList& args) {
        //MainWidget mainWidget = new MainWidget();
        //mainWidget->init(args);
        //mainWidget->show();
        return 0;
    }
};
#endif // !LAMPYRIS_EXE