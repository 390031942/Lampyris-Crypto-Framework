#include "LampyrisClientCrypto.h"
#include <QtWidgets/QApplication>

int main(int argc, char *argv[])
{
    QApplication a(argc, argv);
    LampyrisClientCrypto w;
    w.show();
    return a.exec();
}
