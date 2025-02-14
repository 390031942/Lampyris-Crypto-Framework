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

// K线数据结构
struct KLineData {
    double open;
    double close;
    double high;
    double low;
    double volume;
    QString time; // 时间戳或时间字符串
    double ma5 = 0.0;
    double ma10 = 0.0;
    double ma20 = 0.0;
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

    // k线图网格
    int    gridColunnCount;
    int    gridRowCount;
    int    gridTopPadding;

    QString minTick;
};

// 解析命令行参数
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

// 提取浮点数的前 N 个有效数字，并返回缩放后的整数和数量级
void extractSignificantDigits(double num, int N, int& significantDigits, double& magnitude) {
    if (num == 0) {
        significantDigits = 0;
        magnitude = 1;
        return;
    }

    // 计算数量级（10 的幂次）
    magnitude = std::pow(10, std::floor(std::log10(std::fabs(num))) + 1 - N);

    // 提取前 N 个有效数字并转换为整数
    significantDigits = static_cast<int>(std::round(num / magnitude));
}

// 对浮点数的前 N 个有效数字进行对模 M 的上取整
double ceilModulo(double num, int N, int M) {
    int significantDigits;
    double magnitude;

    // 提取前 N 个有效数字
    extractSignificantDigits(num, N, significantDigits, magnitude);

    // 上取整：向上调整到最近的 M 的倍数
    int ceilInt = (significantDigits % M == 0) ? significantDigits : ((significantDigits / M) + 1) * M;

    // 恢复为浮点数
    return ceilInt * magnitude;
}

// 对浮点数的前 N 个有效数字进行对模 M 的下取整
double floorModulo(double num, int N, int M) {
    int significantDigits;
    double magnitude;

    // 提取前 N 个有效数字
    extractSignificantDigits(num, N, significantDigits, magnitude);

    // 下取整：向下调整到最近的 M 的倍数
    int floorInt = (significantDigits / M) * M;

    // 恢复为浮点数
    return floorInt * magnitude;
}

// 将 double 转换为满足最小步进的字符串
QString formatDoubleWithStep(double value, const QString& step) {
    // 解析最小步进字符串，计算小数点后需要保留的位数
    int decimalPlaces = 0;
    int dotIndex = step.indexOf('.');
    if (dotIndex != -1) {
        decimalPlaces = step.length() - dotIndex - 1; // 小数点后的位数
    }

    // 使用 QString::number 格式化 double 数值
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

        // 绘制背景

        QRect klineAreaRect = image.rect();
        klineAreaRect.setBottom(image.rect().bottom() * 0.7);
        painter.setViewport(klineAreaRect);
        painter.setWindow(0, 0, image.rect().width(), image.rect().height() * 0.7f);
        painter.fillRect(image.rect(), config.backgroundColor);

        double maxPrice = 0, minPrice = 1e9;
        int maxIndex = -1, minIndex = -1;

        // 预计算找到最高价和最低价
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

        // 预计算均线
        calculateMovingAverages(klineData);

        double gridMaxPrice = ceilModulo(maxPrice * 1.005, 4, config.gridRowCount);
        double gridMinPrice = floorModulo(minPrice * 0.995, 4, config.gridRowCount);
        int gridTextWidth = painter.fontMetrics().horizontalAdvance(formatDoubleWithStep(gridMaxPrice, config.minTick));

        // 绘制描述文本
        drawIndicatorText(painter);

        // 绘制网格
        drawGrid(painter, maxPrice, minPrice, gridMaxPrice, gridMinPrice);

        // 绘制 K 线图
        drawKLines(painter, maxPrice, minPrice, maxIndex, minIndex, gridMaxPrice, gridMinPrice, gridTextWidth);
        
        // 绘制背景
        QRect volumeAreaRect = image.rect();
        volumeAreaRect.setTop(image.rect().height() * 0.7);
        volumeAreaRect.setBottom(image.rect().height());
        painter.setViewport(volumeAreaRect);
        painter.setWindow(0, 0, image.rect().width(), image.rect().height() * 0.3f);

        // 绘制成交量柱状图
        drawVolume(painter, gridTextWidth);
        
        // 保存图片
        image.save(config.outputFile);
    }

private:
    RenderConfig config;
    std::vector<KLineData> klineData;

    void drawGrid(QPainter& painter,double maxPrice, double minPrice, double gridMaxPrice, double gridMinPrice) {
        int rows = config.gridRowCount;
        int cols = config.gridColunnCount;
        int width = painter.viewport().width();

        // 刻度
        int textWidth = painter.fontMetrics().horizontalAdvance(formatDoubleWithStep(gridMaxPrice,config.minTick));
        int textHeight = painter.fontMetrics().height();

        // 网格高度
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
        // K 线图区域
        int padding = config.gridTopPadding; // 上下边距（像素）
        int klineAreaHeight = painter.viewport().height() - padding;

        // 增加上下边距到价格范围
        double priceRange = gridMaxPrice - gridMinPrice;
        int candleWidth = (painter.viewport().width() - gridTextWidth - 5)/ klineData.size();

        // 绘制 K 线
        for (size_t i = 0; i < klineData.size(); ++i) {
            const auto& data = klineData[i];
            int x = i * candleWidth;
            int yOpen  = padding + (1 - (data.open  - gridMinPrice) / priceRange) * (klineAreaHeight);
            int yClose = padding + (1 - (data.close - gridMinPrice) / priceRange) * (klineAreaHeight);
            int yHigh  = padding + (1 - (data.high  - gridMinPrice) / priceRange) * (klineAreaHeight);
            int yLow   = padding + (1 - (data.low   - gridMinPrice) / priceRange) * (klineAreaHeight);

            QColor color = (data.close >= data.open) ? config.riseColor : config.fallColor;
            painter.setPen(color);
            painter.drawLine(x + candleWidth / 2, yHigh, x + candleWidth / 2, yLow); // 绘制影线
            painter.setBrush(color);
            painter.drawRect(x, std::min(yOpen, yClose), candleWidth - 2, std::abs(yClose - yOpen)); // 绘制实体
        }

        // 绘制最高价和最低价标记
        drawPriceMarker(painter, maxIndex, maxPrice, gridMinPrice, true, klineAreaHeight, priceRange, minPrice, candleWidth, padding);
        drawPriceMarker(painter, minIndex, minPrice, gridMinPrice, false, klineAreaHeight, priceRange, minPrice, candleWidth, padding);

        // 绘制均线
        // drawKLineMA(painter  offsetof(KLineData, ma5), klineHeight, priceRange, minPrice, candleWidth);
        // drawKLineMA(painter, offsetof(KLineData, ma10), klineHeight, priceRange, minPrice, candleWidth);
        // drawKLineMA(painter, offsetof(KLineData, ma20), klineHeight, priceRange, minPrice, candleWidth);
    }

    void drawVolume(QPainter& painter, double gridTextWidth) {
        // 成交量柱状图区域
        int volumeHeight = painter.viewport().height();
        int candleWidth = (painter.viewport().width() - gridTextWidth - 5) / klineData.size();

        // 找到最大成交量，用于归一化
        double maxVolume = 0;
        double minVolume = 1e30;
        for (const auto& data : klineData) {
            maxVolume = std::max(maxVolume, data.volume);
            minVolume = std::min(minVolume, data.volume);
        }

        // 绘制每根成交量柱状图
        for (size_t i = 0; i < klineData.size(); ++i) {
            const auto& data = klineData[i];
            int x = i * candleWidth;
            int y = volumeHeight - (data.volume / maxVolume) * volumeHeight;
            int height = (data.volume / maxVolume) * volumeHeight;

            // 设置颜色（红涨绿跌）
            QColor color = (data.close >= data.open) ? config.riseColor : config.fallColor;
            painter.setBrush(color);
            painter.setPen(Qt::NoPen);
            painter.drawRect(x, y, candleWidth - 2, height);
        }
    }

    // 计算移动平均值并填充到 KLineData 中
    void calculateMovingAverages(std::vector<KLineData>& klineData) {
        int dataSize = klineData.size();

        for (int i = 0; i < dataSize; ++i) {
            // 计算 MA5
            if (i >= 4) { // 至少需要 5 个数据
                double sum = 0.0;
                for (int j = i; j > i - 5; --j) {
                    sum += klineData[j].close;
                }
                klineData[i].ma5 = sum / 5.0;
            }

            // 计算 MA10
            if (i >= 9) { // 至少需要 10 个数据
                double sum = 0.0;
                for (int j = i; j > i - 10; --j) {
                    sum += klineData[j].close;
                }
                klineData[i].ma10 = sum / 10.0;
            }

            // 计算 MA20
            if (i >= 19) { // 至少需要 20 个数据
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

    void drawPriceMarker(QPainter& painter, int index, double price, double gridMinPrice, bool isMax, int klineAreaHeight, double priceRange, double minPrice, int candleWidth, int padding) {
        if (index < 0 || index >= klineData.size()) return;

        // 计算标记位置
        int x = index * candleWidth + candleWidth / 2;
        int y = padding + (1 - (price - gridMinPrice) / priceRange) * (klineAreaHeight);

        // 判断虚线延伸方向
        bool extendRight = (index < klineData.size() / 2); // 如果在左半部分，则向右延伸

        // 设置虚线样式
        QPen pen;
        pen.setColor(Qt::darkGray);
        pen.setStyle(Qt::DashLine);                // 设置为虚线
        pen.setWidth(1);
        painter.setPen(pen);

        // 绘制虚线
        int lineLength = 20; // 虚线的长度
        int xEnd = extendRight ? x + lineLength : x - lineLength; // 根据方向计算虚线终点
        painter.drawLine(x, y, xEnd, y);

        // 绘制价格文字
        QString priceText = QString::number(price, 'f', 2);
        QFont font = painter.font();
        font.setPointSize(10);
        painter.setFont(font);
        painter.setPen(Qt::white); // 设置文字颜色为白色

        int textOffset = 5; // 文字与虚线的间距
        if (extendRight) {
            // 如果向右延伸，文字显示在虚线右端
            painter.drawText(xEnd + textOffset, y, priceText);
        }
        else {
            // 如果向左延伸，文字显示在虚线左端
            painter.drawText(xEnd - textOffset - painter.fontMetrics().horizontalAdvance(priceText), y, priceText);
        }
    }

    void drawKLineMA(QPainter& painter, int fieldOffset, const QColor& color, int klineHeight, double priceRange, double minPrice, int candleWidth) {
        painter.setPen(QPen(color, 2)); // 设置均线颜色和线宽

        QPainterPath path;
        for (size_t i = 0; i < klineData.size(); ++i) {
            double ma = *(double*)((&klineData[i]) + fieldOffset);
            if (ma == 0) continue; 
                // 跳过无效数据

            int x = i * candleWidth + candleWidth / 2;
            int y = klineHeight - (ma - minPrice) / priceRange * klineHeight;

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
