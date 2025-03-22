// Project Include(s)
#include "LauncherWidget.h"

int main(int argc, char *argv[]) {
    QApplication a(argc, argv);

    QStringList arguments;
    for (int i = 0; i < argc; ++i) {
        arguments << QString::fromUtf8(argv[i]);
    }

    LauncherWidget w("appconfig.ini", arguments);
    w.show();

    return a.exec();
}
