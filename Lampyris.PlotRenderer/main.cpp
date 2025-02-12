#include "LampyrisPlotRenderer.h"
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
#include <QColor>
#include <vector>

// K线数据结构
struct KLineData {
    double open;
    double close;
    double high;
    double low;
    double volume;
    QString time; // 时间戳或时间字符串
};

// 配置参数结构
struct RenderConfig {
    QColor backgroundColor;
    QColor gridColor;
    QColor ma5Color;
    QColor ma10Color;
    QColor ma20Color;
    QColor riseColor; // 涨的颜色
    QColor fallColor; // 跌的颜色
    int width;
    int height;
    QString outputFile;
};

// 解析命令行参数
bool parseCommandLine(QCommandLineParser& parser, RenderConfig& config, std::vector<KLineData>& klineData) {
    parser.addHelpOption();
    parser.addOption({ "background", "背景颜色 (十六进制)", "color", "#000000" });
    parser.addOption({ "grid", "网格颜色 (十六进制)", "color", "#333333" });
    parser.addOption({ "ma5", "MA5 颜色 (十六进制)", "color", "#FF0000" });
    parser.addOption({ "ma10", "MA10 颜色 (十六进制)", "color", "#00FF00" });
    parser.addOption({ "ma20", "MA20 颜色 (十六进制)", "color", "#0000FF" });
    parser.addOption({ "rise", "涨的颜色 (十六进制)", "color", "#FF0000" });
    parser.addOption({ "fall", "跌的颜色 (十六进制)", "color", "#00FF00" });
    parser.addOption({ "width", "输出图片宽度", "width", "800" });
    parser.addOption({ "height", "输出图片高度", "height", "600" });
    parser.addOption({ "output", "输出文件路径", "file", "output.png" });
    parser.addOption({ "data", "K线数据文件路径 (JSON 格式)", "file" });

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

class KLineRenderer {
public:
    KLineRenderer(const RenderConfig& config, const std::vector<KLineData>& data)
        : config(config), klineData(data) {}

    void render() {
        QImage image(config.width, config.height, QImage::Format_ARGB32);
        QPainter painter(&image);

        // 绘制背景
        QRect klineAreaRect = image.rect();
        klineAreaRect.setBottom(image.rect().bottom() * 0.7);
        painter.setClipRect(klineAreaRect);
        painter.fillRect(klineAreaRect, config.backgroundColor);

        // 绘制网格
        drawGrid(painter);

        // 绘制 K 线图
        drawKLines(painter);
        
        // 绘制背景
        QRect volumeAreaRect = image.rect();
        volumeAreaRect.setTop(image.rect().bottom() * 0.7);
        volumeAreaRect.setBottom(image.rect().bottom());
        painter.setClipRect(volumeAreaRect);
        painter.fillRect(volumeAreaRect, config.backgroundColor);

        // 绘制成交量柱状图
        drawVolume(painter);
        
        // // 保存图片
        image.save(config.outputFile);
    }

private:
    RenderConfig config;
    std::vector<KLineData> klineData;

    void drawGrid(QPainter& painter) {
        painter.setPen(config.gridColor);
        int rows = 10;
        int cols = 10;

        for (int i = 1; i < rows; ++i) {
            int y = i * config.height / rows;
            painter.drawLine(0, y, config.width, y);
        }

        for (int i = 1; i < cols; ++i) {
            int x = i * config.width / cols;
            painter.drawLine(x, 0, x, config.height);
        }
    }

    void drawKLines(QPainter& painter) {
        // K 线图区域
        int klineHeight = painter.clipBoundingRect().height();
        double maxPrice = 0, minPrice = 1e9;
        int maxIndex = -1, minIndex = -1;

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

        double priceRange = maxPrice - minPrice;
        int candleWidth = config.width / klineData.size();

        // 绘制 K 线
        for (size_t i = 0; i < klineData.size(); ++i) {
            const auto& data = klineData[i];
            int x = i * candleWidth;
            int yOpen = klineHeight - (data.open - minPrice) / priceRange * klineHeight;
            int yClose = klineHeight - (data.close - minPrice) / priceRange * klineHeight;
            int yHigh = klineHeight - (data.high - minPrice) / priceRange * klineHeight;
            int yLow = klineHeight - (data.low - minPrice) / priceRange * klineHeight;

            QColor color = (data.close >= data.open) ? config.riseColor : config.fallColor;
            painter.setPen(color);
            painter.drawLine(x + candleWidth / 2, yHigh, x + candleWidth / 2, yLow);
            painter.setBrush(color);
            painter.drawRect(x, std::min(yOpen, yClose), candleWidth - 2, std::abs(yClose - yOpen));
        }

        // 绘制最高价和最低价标记
        drawPriceMarker(painter, maxIndex, maxPrice, true, klineHeight, priceRange, minPrice, candleWidth);
        drawPriceMarker(painter, minIndex, minPrice, false, klineHeight, priceRange, minPrice, candleWidth);

        // 绘制均线
        // drawKLineMA(painter, calculateMovingAverage(klineData, 5), config.ma5Color, klineHeight, priceRange, minPrice, candleWidth);
        // drawKLineMA(painter, calculateMovingAverage(klineData, 10), config.ma10Color, klineHeight, priceRange, minPrice, candleWidth);
        // drawKLineMA(painter, calculateMovingAverage(klineData, 20), config.ma20Color, klineHeight, priceRange, minPrice, candleWidth);
    }

    void drawVolume(QPainter& painter) {
        // 成交量柱状图区域
        int volumeHeight = painter.clipBoundingRect().height();
        int volumeTop = config.height * 0.7;   // 成交量区域的顶部位置
        int candleWidth = config.width / klineData.size();

        // 找到最大成交量，用于归一化
        double maxVolume = 0;
        for (const auto& data : klineData) {
            maxVolume = std::max(maxVolume, data.volume);
        }

        // 绘制每根成交量柱状图
        for (size_t i = 0; i < klineData.size(); ++i) {
            const auto& data = klineData[i];
            int x = i * candleWidth;
            int y = volumeTop + volumeHeight - (data.volume / maxVolume) * volumeHeight;
            int height = (data.volume / maxVolume) * volumeHeight;

            // 设置颜色（红涨绿跌）
            QColor color = (data.close >= data.open) ? config.riseColor : config.fallColor;
            painter.setBrush(color);
            painter.setPen(Qt::NoPen);
            painter.drawRect(x, y, candleWidth - 2, height);
        }

        // 绘制均线
        // drawVolumeMA(painter, calculateMovingAverage(klineData, 5), config.ma5Color, volumeTop, volumeHeight, maxVolume);
        // drawVolumeMA(painter, calculateMovingAverage(klineData, 10), config.ma10Color, volumeTop, volumeHeight, maxVolume);
        // drawVolumeMA(painter, calculateMovingAverage(klineData, 20), config.ma20Color, volumeTop, volumeHeight, maxVolume);
    }

    void drawVolumeMA(QPainter& painter, const std::vector<double>& ma, const QColor& color, int volumeTop, int volumeHeight, double maxVolume) {
        if (ma.empty()) return;

        painter.setPen(QPen(color, 2));

        QPainterPath path;
        for (size_t i = 0; i < ma.size(); ++i) {
            if (ma[i] == 0) continue; // 跳过无效数据

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

    std::vector<double> calculateMovingAverage(const std::vector<KLineData>& data, int period) {
        std::vector<double> ma;
        if (data.size() < period) return ma;

        double sum = 0;
        for (size_t i = 0; i < data.size(); ++i) {
            sum += data[i].volume;
            if (i >= period) {
                sum -= data[i - period].volume;
            }
            if (i >= period - 1) {
                ma.push_back(sum / period);
            }
            else {
                ma.push_back(0); // 前期不足均线周期的数据填充为 0
            }
        }
        return ma;
    }

    void drawPriceMarker(QPainter& painter, int index, double price, bool isMax, int klineHeight, double priceRange, double minPrice, int candleWidth) {
        if (index < 0 || index >= klineData.size()) return;

        // 计算标记位置
        int x = index * candleWidth + candleWidth / 2;
        int y = klineHeight - (price - minPrice) / priceRange * klineHeight;

        // 绘制标记（小圆点）
        painter.setBrush(isMax ? Qt::red : Qt::green);
        painter.setPen(Qt::NoPen);
        painter.drawEllipse(QPointF(x, y), 5, 5);

        // 绘制价格文字
        QString priceText = QString::number(price, 'f', 2);
        QFont font = painter.font();
        font.setPointSize(10);
        painter.setFont(font);
        painter.setPen(Qt::white);

        int textOffset = 10; // 文字与标记的间距
        if (isMax) {
            // 最高价文字在标记上方
            painter.drawText(x + textOffset, y - textOffset, priceText);
        }
        else {
            // 最低价文字在标记下方
            painter.drawText(x + textOffset, y + textOffset + 10, priceText);
        }
    }

    void drawKLineMA(QPainter& painter, const std::vector<double>& ma, const QColor& color, int klineHeight, double priceRange, double minPrice, int candleWidth) {
        if (ma.empty()) return;

        painter.setPen(QPen(color, 2)); // 设置均线颜色和线宽

        QPainterPath path;
        for (size_t i = 0; i < ma.size(); ++i) {
            if (ma[i] == 0) continue; // 跳过无效数据

            int x = i * candleWidth + candleWidth / 2;
            int y = klineHeight - (ma[i] - minPrice) / priceRange * klineHeight;

            if (i == 0) {
                path.moveTo(x, y); // 起点
            }
            else {
                path.lineTo(x, y); // 连线
            }
        }

        painter.drawPath(path);
    }

};

int main(int argc, char* argv[]) {
    QApplication app(argc, argv);

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
