// QT Include(s)
#include <QStringList>
#include <QtPlugin>

class IPlugin {
public:
    virtual ~IPlugin() {}
    virtual int main(const QStringList& args) = 0;
};

Q_DECLARE_INTERFACE(IPlugin, "com.lampyris.client.IPlugin")