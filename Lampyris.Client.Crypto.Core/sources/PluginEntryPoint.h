// Project Include(s)
#include "Global.h"

class LAMPYRISCLIENTCRYPTOCORE_EXPORT PluginEntryPoint:public IPlugin {
public:
    virtual int main(const QStringList& args) {
        MainWidget mainWidget = new MainWidget();
        mainWidget->init(args);
        mainWidget->show();
        return 0;
    }
};
