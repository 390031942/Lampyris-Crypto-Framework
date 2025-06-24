// QT Include(s)
#include <QtWidgets/QApplication>
#include <QDebug>
#include <QCommandLineParser>
#include <QPainter>
#include <QImage>
#include <QFile>
#include <QTextStream>
#include <QJsonDocument>
#include <QJsonArray>
#include <QJsonObject>
#include <QFontDatabase>
#include <QColor>

// STD Include(s)
#include <vector>
#include <cmath>

int main1(int argc, char* argv[]) {
    QApplication app(argc, argv);
    // 加载本地字体文件
    QString fontPath = "moon_plex_regular.otf"; // 替换为你的字体文件路径
    int fontId = QFontDatabase::addApplicationFont(fontPath);
    if (fontId == -1) {
        qDebug() << "Failed to load font:" << fontPath;
        return -1;
    }
    // 获取字体名称
    QStringList fontFamilies = QFontDatabase::applicationFontFamilies(fontId);
    if (fontFamilies.isEmpty()) {
        qDebug() << "No font families found for font ID:" << fontId;
        return -1;
    }

    QString fontFamily = fontFamilies.at(0); // 使用加载字体的第一个字体族
    qDebug() << "Loaded font family:" << fontFamily;
    QFont font(fontFamily);
    font.setPointSize(10); // 设置字体大小
    app.setFont(font);

    return 0;
}

#include "BetterSplitter.h"
#include <QMainWindow>
#include "CandleQuoteDisplayWidget.h"
#include <QSslConfiguration>
#include "qsslsocket.h"

int main(int argc, char* argv[]) {
    QApplication app(argc, argv);
    // 加载本地字体文件
    QString fontPath = "moon_plex_medium.otf"; // 替换为你的字体文件路径
    int fontId = QFontDatabase::addApplicationFont(fontPath);
    if (fontId == -1) {
        qDebug() << "Failed to load font:" << fontPath;
        return -1;
    }
    // 获取字体名称
    QStringList fontFamilies = QFontDatabase::applicationFontFamilies(fontId);
    if (fontFamilies.isEmpty()) {
        qDebug() << "No font families found for font ID:" << fontId;
        return -1;
    }

    QString fontFamily = fontFamilies.at(0); // 使用加载字体的第一个字体族
    qDebug() << "Loaded font family:" << fontFamily;
    QFont font(fontFamily);
    font.setPointSize(10); // 设置字体大小
    app.setFont(font);

    CandleQuoteDisplayWidget w(nullptr);
    w.setStyleSheet("QWidget{background-color:black;}");
    w.show();

    return app.exec();
}