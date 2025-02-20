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

#include <vector>
#include <cmath>

// K�����ݽṹ
struct KLineData {
    double open;
    double close;
    double high;
    double low;
    double volume;
    QString time; // ʱ�����ʱ���ַ���
    double ma5 = 0.0;
    double ma10 = 0.0;
    double ma20 = 0.0;
};

// ���ò����ṹ
struct RenderConfig {
    QColor backgroundColor;
    QColor gridColor;
    QColor ma5Color;
    QColor ma10Color;
    QColor ma20Color;
    QColor riseColor; // �ǵ���ɫ
    QColor fallColor; // ������ɫ
    int width;
    int height;
    QString outputFile;

    // k��ͼ����
    int    gridColunnCount;
    int    gridRowCount;
    int    gridTopPadding;

    QString minTick;
};

// ���������в���
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

// ��ȡ��������ǰ N ����Ч���֣����������ź��������������
void extractSignificantDigits(double num, int N, int& significantDigits, double& magnitude) {
    if (num == 0) {
        significantDigits = 0;
        magnitude = 1;
        return;
    }

    // ������������10 ���ݴΣ�
    magnitude = std::pow(10, std::floor(std::log10(std::fabs(num))) + 1 - N);

    // ��ȡǰ N ����Ч���ֲ�ת��Ϊ����
    significantDigits = static_cast<int>(std::round(num / magnitude));
}

// �Ը�������ǰ N ����Ч���ֽ��ж�ģ M ����ȡ��
double ceilModulo(double num, int N, int M) {
    int significantDigits;
    double magnitude;

    // ��ȡǰ N ����Ч����
    extractSignificantDigits(num, N, significantDigits, magnitude);

    // ��ȡ�������ϵ���������� M �ı���
    int ceilInt = (significantDigits % M == 0) ? significantDigits : ((significantDigits / M) + 1) * M;

    // �ָ�Ϊ������
    return ceilInt * magnitude;
}

// �Ը�������ǰ N ����Ч���ֽ��ж�ģ M ����ȡ��
double floorModulo(double num, int N, int M) {
    int significantDigits;
    double magnitude;

    // ��ȡǰ N ����Ч����
    extractSignificantDigits(num, N, significantDigits, magnitude);

    // ��ȡ�������µ���������� M �ı���
    int floorInt = (significantDigits / M) * M;

    // �ָ�Ϊ������
    return floorInt * magnitude;
}

// �� double ת��Ϊ������С�������ַ���
QString formatDoubleWithStep(double value, const QString& step) {
    // ������С�����ַ���������С�������Ҫ������λ��
    int decimalPlaces = 0;
    int dotIndex = step.indexOf('.');
    if (dotIndex != -1) {
        decimalPlaces = step.length() - dotIndex - 1; // С������λ��
    }

    // ʹ�� QString::number ��ʽ�� double ��ֵ
    QString formattedValue = QString::number(value, 'f', decimalPlaces);

    return formattedValue;
}

class KLineRenderer {
public:
    KLineRenderer(const RenderConfig& config, const std::vector<KLineData>& data)
        : config(config), klineData(data) {}

    void render() {
        QImage image(config.width, config.height, QImage::Format_ARGB32);
        QPainter painter(&image);

        // ���Ʊ���

        QRect klineAreaRect = image.rect();
        klineAreaRect.setBottom(image.rect().bottom() * 0.7);
        painter.setViewport(klineAreaRect);
        painter.setWindow(0, 0, image.rect().width(), image.rect().height() * 0.7f);
        painter.fillRect(image.rect(), config.backgroundColor);

        double maxPrice = 0, minPrice = 1e9;
        int maxIndex = -1, minIndex = -1;

        // Ԥ�����ҵ���߼ۺ���ͼ�
        for (size_t i = 0; i < klineData.size(); ++i) {
            const auto& data = klineData[i];
            if (data.high > maxPrice) {
                maxPrice = data.high;
                maxIndex = i;
            }
            if (data.low < minPrice) {
                minPrice = data.low;
                minIndex = i;
            }
        }

        // Ԥ�������
        calculateMovingAverages(klineData);

        double gridMaxPrice = ceilModulo(maxPrice * 1.005, 4, config.gridRowCount);
        double gridMinPrice = floorModulo(minPrice * 0.995, 4, config.gridRowCount);
        int gridTextWidth = painter.fontMetrics().horizontalAdvance(formatDoubleWithStep(gridMaxPrice, config.minTick));

        // ���������ı�
        drawIndicatorText(painter);

        // ��������
        drawGrid(painter, maxPrice, minPrice, gridMaxPrice, gridMinPrice);

        // ���� K ��ͼ
        drawKLines(painter, maxPrice, minPrice, maxIndex, minIndex, gridMaxPrice, gridMinPrice, gridTextWidth);
        
        // ���Ʊ���
        QRect volumeAreaRect = image.rect();
        volumeAreaRect.setTop(image.rect().height() * 0.7);
        volumeAreaRect.setBottom(image.rect().height());
        painter.setViewport(volumeAreaRect);
        painter.setWindow(0, 0, image.rect().width(), image.rect().height() * 0.3f);

        // ���Ƴɽ�����״ͼ
        drawVolume(painter, gridTextWidth);
        
        // ����ͼƬ
        image.save(config.outputFile);
    }

private:
    RenderConfig config;
    std::vector<KLineData> klineData;

    void drawGrid(QPainter& painter,double maxPrice, double minPrice, double gridMaxPrice, double gridMinPrice) {
        int rows = config.gridRowCount;
        int cols = config.gridColunnCount;
        int width = painter.viewport().width();

        // �̶�
        int textWidth = painter.fontMetrics().horizontalAdvance(formatDoubleWithStep(gridMaxPrice,config.minTick));
        int textHeight = painter.fontMetrics().height();

        // ����߶�
        int gridHeight = (painter.viewport().height() - config.gridTopPadding) / (rows - 1);

        for (int i = 0; i < rows; ++i) {
            int y = config.gridTopPadding + i * gridHeight;
            painter.setPen(config.gridColor);
            painter.drawLine(0, y, config.width, y);

            painter.setPen(Qt::darkGray);

            double  startX = painter.viewport().width() - textWidth - 5;
            QString str = formatDoubleWithStep(gridMaxPrice + i * (gridMinPrice - gridMaxPrice) / (rows - 1), config.minTick);
            painter.drawText(startX, y - textHeight, textWidth, textHeight, 0, str);
        }

        painter.setPen(config.gridColor);
        int y = config.gridTopPadding + (rows - 1) * painter.viewport().height() / rows;
        for (int i = 1; i <= cols; ++i) {
            int x = i * width / cols;
            painter.drawLine(x, config.gridTopPadding, x, y);
        }
    }

    QString makeMAIndicatorString(int period, double value) {
        return QString("MA(%1):%2").arg(period).arg(value > 0 ? formatDoubleWithStep(value, config.minTick) : "-");
    }

    void drawIndicatorText(QPainter& painter) {
        const KLineData& lastest = klineData.back();

        QString ma5String  = makeMAIndicatorString(5,  lastest.ma5);
        QString ma10String = makeMAIndicatorString(10, lastest.ma10);
        QString ma20String = makeMAIndicatorString(20, lastest.ma20);

        int ma5StringWidth  = painter.fontMetrics().horizontalAdvance(ma5String);
        int ma10StringWidth = painter.fontMetrics().horizontalAdvance(ma10String);
        int ma20StringWidth = painter.fontMetrics().horizontalAdvance(ma20String);

        int height = painter.fontMetrics().height();

        painter.save();
        QFont f = painter.font();
        f.setPointSizeF(9);
        painter.setFont(f);

        int basePosX = 5;
        int basePosY = 2;
        int spacingX = 7;
        painter.setPen(config.ma5Color);
        painter.drawText(basePosX, basePosY, ma5StringWidth, height, 0, ma5String);
        basePosX += (spacingX + ma5StringWidth);

        painter.setPen(config.ma10Color);
        painter.drawText(basePosX, basePosY, ma10StringWidth, height, 0, ma10String);
        basePosX += (spacingX + ma10StringWidth);

        painter.setPen(config.ma20Color);
        painter.drawText(basePosX, basePosY, ma20StringWidth, height, 0, ma20String);
        basePosX += (spacingX + ma20StringWidth);

        painter.restore();
    }

    void drawKLines(QPainter& painter, double maxPrice, double minPrice, int maxIndex, int minIndex, double gridMaxPrice, double gridMinPrice, double gridTextWidth) {
        // K ��ͼ����
        int padding = config.gridTopPadding; // ���±߾ࣨ���أ�
        int klineAreaHeight = painter.viewport().height() - padding;

        // �������±߾ൽ�۸�Χ
        double priceRange = gridMaxPrice - gridMinPrice;
        int candleWidth = (painter.viewport().width() - gridTextWidth - 5)/ klineData.size();

        // ���� K ��
        for (size_t i = 0; i < klineData.size(); ++i) {
            const auto& data = klineData[i];
            int x = i * candleWidth;
            int yOpen  = padding + (1 - (data.open  - gridMinPrice) / priceRange) * (klineAreaHeight);
            int yClose = padding + (1 - (data.close - gridMinPrice) / priceRange) * (klineAreaHeight);
            int yHigh  = padding + (1 - (data.high  - gridMinPrice) / priceRange) * (klineAreaHeight);
            int yLow   = padding + (1 - (data.low   - gridMinPrice) / priceRange) * (klineAreaHeight);

            QColor color = (data.close >= data.open) ? config.riseColor : config.fallColor;
            painter.setPen(color);
            painter.drawLine(x + candleWidth / 2, yHigh, x + candleWidth / 2, yLow); // ����Ӱ��
            painter.setBrush(color);
            painter.drawRect(x, std::min(yOpen, yClose), candleWidth - 2, std::abs(yClose - yOpen)); // ����ʵ��
        }

        // ������߼ۺ���ͼ۱��
        drawPriceMarker(painter, maxIndex, maxPrice, gridMinPrice, true, klineAreaHeight, priceRange, minPrice, candleWidth, padding);
        drawPriceMarker(painter, minIndex, minPrice, gridMinPrice, false, klineAreaHeight, priceRange, minPrice, candleWidth, padding);

        // ���ƾ���
        // drawKLineMA(painter  offsetof(KLineData, ma5), klineHeight, priceRange, minPrice, candleWidth);
        // drawKLineMA(painter, offsetof(KLineData, ma10), klineHeight, priceRange, minPrice, candleWidth);
        // drawKLineMA(painter, offsetof(KLineData, ma20), klineHeight, priceRange, minPrice, candleWidth);
    }

    void drawVolume(QPainter& painter, double gridTextWidth) {
        // �ɽ�����״ͼ����
        int volumeHeight = painter.viewport().height();
        int candleWidth = (painter.viewport().width() - gridTextWidth - 5) / klineData.size();

        // �ҵ����ɽ��������ڹ�һ��
        double maxVolume = 0;
        double minVolume = 1e30;
        for (const auto& data : klineData) {
            maxVolume = std::max(maxVolume, data.volume);
            minVolume = std::min(minVolume, data.volume);
        }

        // ����ÿ���ɽ�����״ͼ
        for (size_t i = 0; i < klineData.size(); ++i) {
            const auto& data = klineData[i];
            int x = i * candleWidth;
            int y = volumeHeight - (data.volume / maxVolume) * volumeHeight;
            int height = (data.volume / maxVolume) * volumeHeight;

            // ������ɫ�������̵���
            QColor color = (data.close >= data.open) ? config.riseColor : config.fallColor;
            painter.setBrush(color);
            painter.setPen(Qt::NoPen);
            painter.drawRect(x, y, candleWidth - 2, height);
        }
    }

    // �����ƶ�ƽ��ֵ����䵽 KLineData ��
    void calculateMovingAverages(std::vector<KLineData>& klineData) {
        int dataSize = klineData.size();

        for (int i = 0; i < dataSize; ++i) {
            // ���� MA5
            if (i >= 4) { // ������Ҫ 5 ������
                double sum = 0.0;
                for (int j = i; j > i - 5; --j) {
                    sum += klineData[j].close;
                }
                klineData[i].ma5 = sum / 5.0;
            }

            // ���� MA10
            if (i >= 9) { // ������Ҫ 10 ������
                double sum = 0.0;
                for (int j = i; j > i - 10; --j) {
                    sum += klineData[j].close;
                }
                klineData[i].ma10 = sum / 10.0;
            }

            // ���� MA20
            if (i >= 19) { // ������Ҫ 20 ������
                double sum = 0.0;
                for (int j = i; j > i - 20; --j) {
                    sum += klineData[j].close;
                }
                klineData[i].ma20 = sum / 20.0;
            }
        }
    }

    void drawVolumeMA(QPainter& painter, const std::vector<double>& ma, const QColor& color, int volumeTop, int volumeHeight, double maxVolume) {
        if (ma.empty()) return;

        painter.setPen(QPen(color, 2));

        QPainterPath path;
        for (size_t i = 0; i < ma.size(); ++i) {
            if (ma[i] == 0) continue; // ������Ч����

            int x = i * (config.width / klineData.size()) + (config.width / klineData.size()) / 2;
            int y = volumeTop + volumeHeight - (ma[i] / maxVolume) * volumeHeight;

            if (i == 0) {
                path.moveTo(x, y);
            }
            else {
                path.lineTo(x, y);
            }
        }

        painter.drawPath(path);
    }

    void drawPriceMarker(QPainter& painter, int index, double price, double gridMinPrice, bool isMax, int klineAreaHeight, double priceRange, double minPrice, int candleWidth, int padding) {
        if (index < 0 || index >= klineData.size()) return;

        // ������λ��
        int x = index * candleWidth + candleWidth / 2;
        int y = padding + (1 - (price - gridMinPrice) / priceRange) * (klineAreaHeight);

        // �ж��������췽��
        bool extendRight = (index < klineData.size() / 2); // �������벿�֣�����������

        // ����������ʽ
        QPen pen;
        pen.setColor(Qt::darkGray);
        pen.setStyle(Qt::DashLine);                // ����Ϊ����
        pen.setWidth(1);
        painter.setPen(pen);

        // ��������
        int lineLength = 20; // ���ߵĳ���
        int xEnd = extendRight ? x + lineLength : x - lineLength; // ���ݷ�����������յ�
        painter.drawLine(x, y, xEnd, y);

        // ���Ƽ۸�����
        QString priceText = QString::number(price, 'f', 2);
        QFont font = painter.font();
        font.setPointSize(10);
        painter.setFont(font);
        painter.setPen(Qt::white); // ����������ɫΪ��ɫ

        int textOffset = 5; // ���������ߵļ��
        if (extendRight) {
            // ����������죬������ʾ�������Ҷ�
            painter.drawText(xEnd + textOffset, y, priceText);
        }
        else {
            // ����������죬������ʾ���������
            painter.drawText(xEnd - textOffset - painter.fontMetrics().horizontalAdvance(priceText), y, priceText);
        }
    }

    void drawKLineMA(QPainter& painter, int fieldOffset, const QColor& color, int klineHeight, double priceRange, double minPrice, int candleWidth) {
        painter.setPen(QPen(color, 2)); // ���þ�����ɫ���߿�

        QPainterPath path;
        for (size_t i = 0; i < klineData.size(); ++i) {
            double ma = *(double*)((&klineData[i]) + fieldOffset);
            if (ma == 0) continue; 
                // ������Ч����

            int x = i * candleWidth + candleWidth / 2;
            int y = klineHeight - (ma - minPrice) / priceRange * klineHeight;

            if (i == 0) {
                path.moveTo(x, y); // ���
            }
            else {
                path.lineTo(x, y); // ����
            }
        }
        painter.drawPath(path);
    }
};

int main(int argc, char* argv[]) {
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

    QCommandLineParser parser;
    RenderConfig config;
    std::vector<KLineData> klineData;

    if (!parseCommandLine(parser, config, klineData)) {
        return 1;
    }

    KLineRenderer renderer(config, klineData);
    renderer.render();

    return 0;
}
