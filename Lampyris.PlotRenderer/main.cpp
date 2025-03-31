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

// ���������в���
/*
bool parseCommandLine(QCommandLineParser& parser, RenderConfig& config, std::vector<KLineData>& klineData) {
    parser.addHelpOption();
    parser.addOption({ "background", "������ɫ (ʮ������)", "color", "#000000" });
    parser.addOption({ "grid", "������ɫ (ʮ������)", "color", "#333333" });
    parser.addOption({ "ma5", "MA5 ��ɫ (ʮ������)", "color", "#EAB835" });
    parser.addOption({ "ma10", "MA10 ��ɫ (ʮ������)", "color", "#EA4ABB" });
    parser.addOption({ "ma20", "MA20 ��ɫ (ʮ������)", "color", "#8B68C6" });
    parser.addOption({ "rise", "�ǵ���ɫ (ʮ������)", "color", "#EE525C" });
    parser.addOption({ "fall", "������ɫ (ʮ������)", "color", "#56BB84" });
    parser.addOption({ "width", "���ͼƬ���", "width", "800" });
    parser.addOption({ "height", "���ͼƬ�߶�", "height", "600" });
    parser.addOption({ "output", "����ļ�·��", "file", "output.png" });
    parser.addOption({ "gridColunnCount", "k��ͼ��������", "gridColunnCount", "4" });
    parser.addOption({ "gridRowCount", "k��ͼ��������", "gridRowCount", "6" });
    parser.addOption({ "gridTopPadding", "k��ͼ���񶥲��߾�", "gridTopPadding", "25" });
    parser.addOption({ "data", "K�������ļ�·�� (JSON ��ʽ)", "file" });
    parser.addOption({ "minTick", "�۸���С�仯��λ", "minTick", "0.0001"});

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
        qWarning() << "K�������ļ�δָ��";
        return false;
    }

    QFile file(dataFile);
    if (!file.open(QIODevice::ReadOnly)) {
        qWarning() << "�޷��������ļ���" << dataFile;
        return false;
    }

    QByteArray jsonData = file.readAll();
    file.close();

    QJsonDocument doc = QJsonDocument::fromJson(jsonData);
    if (!doc.isArray()) {
        qWarning() << "�����ļ���ʽ����";
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
    // ���ر��������ļ�
    QString fontPath = "moon_plex_regular.otf"; // �滻Ϊ��������ļ�·��
    int fontId = QFontDatabase::addApplicationFont(fontPath);
    if (fontId == -1) {
        qDebug() << "Failed to load font:" << fontPath;
        return -1;
    }
    // ��ȡ��������
    QStringList fontFamilies = QFontDatabase::applicationFontFamilies(fontId);
    if (fontFamilies.isEmpty()) {
        qDebug() << "No font families found for font ID:" << fontId;
        return -1;
    }

    QString fontFamily = fontFamilies.at(0); // ʹ�ü�������ĵ�һ��������
    qDebug() << "Loaded font family:" << fontFamily;
    QFont font(fontFamily);
    font.setPointSize(10); // ���������С
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