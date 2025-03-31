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

// 解析命令行参数
/*
bool parseCommandLine(QCommandLineParser& parser, RenderConfig& config, std::vector<KLineData>& klineData) {
    parser.addHelpOption();
    parser.addOption({ "background", "背景颜色 (十六进制)", "color", "#000000" });
    parser.addOption({ "grid", "网格颜色 (十六进制)", "color", "#333333" });
    parser.addOption({ "ma5", "MA5 颜色 (十六进制)", "color", "#EAB835" });
    parser.addOption({ "ma10", "MA10 颜色 (十六进制)", "color", "#EA4ABB" });
    parser.addOption({ "ma20", "MA20 颜色 (十六进制)", "color", "#8B68C6" });
    parser.addOption({ "rise", "涨的颜色 (十六进制)", "color", "#EE525C" });
    parser.addOption({ "fall", "跌的颜色 (十六进制)", "color", "#56BB84" });
    parser.addOption({ "width", "输出图片宽度", "width", "800" });
    parser.addOption({ "height", "输出图片高度", "height", "600" });
    parser.addOption({ "output", "输出文件路径", "file", "output.png" });
    parser.addOption({ "gridColunnCount", "k线图网格列数", "gridColunnCount", "4" });
    parser.addOption({ "gridRowCount", "k线图网格行数", "gridRowCount", "6" });
    parser.addOption({ "gridTopPadding", "k线图网格顶部边距", "gridTopPadding", "25" });
    parser.addOption({ "data", "K线数据文件路径 (JSON 格式)", "file" });
    parser.addOption({ "minTick", "价格最小变化单位", "minTick", "0.0001"});

    if (!parser.parse(QCoreApplication::arguments())) {
        qWarning() << parser.errorText();
        return false;
    }

    config.backgroundColor = QColor(parser.value("background"));
    config.gridColor = QColor(parser.value("grid"));
    config.ma5Color = QColor(parser.value("ma5"));
    config.ma10Color = QColor(parser.value("ma10"));
    config.ma20Color = QColor(parser.value("ma20"));
    config.riseColor = QColor(parser.value("rise"));
    config.fallColor = QColor(parser.value("fall"));
    config.width = parser.value("width").toInt();
    config.height = parser.value("height").toInt();
    config.outputFile = parser.value("output");
    config.gridColunnCount = parser.value("gridColunnCount").toInt();
    config.gridRowCount = parser.value("gridRowCount").toInt();
    config.gridTopPadding = parser.value("gridTopPadding").toInt();
    config.minTick = parser.value("minTick");

    QString dataFile = parser.value("data");
    if (dataFile.isEmpty()) {
        qWarning() << "K线数据文件未指定";
        return false;
    }

    QFile file(dataFile);
    if (!file.open(QIODevice::ReadOnly)) {
        qWarning() << "无法打开数据文件：" << dataFile;
        return false;
    }

    QByteArray jsonData = file.readAll();
    file.close();

    QJsonDocument doc = QJsonDocument::fromJson(jsonData);
    if (!doc.isArray()) {
        qWarning() << "数据文件格式错误";
        return false;
    }

    QJsonArray array = doc.array();
    for (const QJsonValue& value : array) {
        QJsonObject obj = value.toObject();
        KLineData data;
        data.open = obj["open"].toDouble();
        data.close = obj["close"].toDouble();
        data.high = obj["high"].toDouble();
        data.low = obj["low"].toDouble();
        data.volume = obj["volume"].toDouble();
        data.time = obj["time"].toString();
        klineData.push_back(data);
    }

    return true;
}
*/

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

    // QCommandLineParser parser;
    // RenderConfig config;
    // std::vector<KLineData> klineData;
    // 
    // if (!parseCommandLine(parser, config, klineData)) {
    //     return 1;
    // }
    // KLineRenderer renderer(config, klineData);
    // renderer.render();

    return 0;
}

#include "BetterSplitter.h"
#include <QMainWindow>
#include "TestWidget.h"
#include <QSslConfiguration>
#include "qsslsocket.h"

int main(int argc, char* argv[]) {
    QApplication app(argc, argv);

    TestWidget w(nullptr);
    w.show();

    return app.exec();
}