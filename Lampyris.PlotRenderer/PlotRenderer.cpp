#include "PlotRenderer.h"
#include <QPainterPath>

void PlotRenderer::render(QPainter& painter) {
    double maxPrice = 0, minPrice = 1e9;
    int maxIndex = -1, minIndex = -1;

    // Ԥ�����ҵ���߼ۺ���ͼ�
    for (size_t i = 0; i < m_dataList.size(); ++i) {
        const auto& data = m_dataList[i];
        if (data->high > maxPrice) {
            maxPrice = data->high;
            maxIndex = i;
        }
        if (data->low < minPrice) {
            minPrice = data->low;
            minIndex = i;
        }
    }

    // Ԥ�������
    calculateMovingAverages(m_dataList);

    double gridMaxPrice = MathUtil::ceilModulo(maxPrice * 1.005, 4, m_config.gridRowCount);
    double gridMinPrice = MathUtil::floorModulo(minPrice * 0.995, 4, m_config.gridRowCount);
    int gridTextWidth = painter.fontMetrics().horizontalAdvance(MathUtil::formatDoubleWithStep(gridMaxPrice, m_config.minTick));

    // ���������ı�
    drawIndicatorText(painter);

    // ��������
    drawGrid(painter, maxPrice, minPrice, gridMaxPrice, gridMinPrice);

    // ���� K ��ͼ
    // drawCandleChart(painter, maxPrice, minPrice, maxIndex, minIndex, gridMaxPrice, gridMinPrice, gridTextWidth);

    // ���Ʊ���
    // QRect volumeAreaRect = image.rect();
    // volumeAreaRect.setTop(image.rect().height() * 0.7);
    // volumeAreaRect.setBottom(image.rect().height());
    // painter.setViewport(volumeAreaRect);
    // painter.setWindow(0, 0, image.rect().width(), image.rect().height() * 0.3f);

    // ���Ƴɽ�����״ͼ
    drawVolume(painter);

    // ����ͼƬ
    // image.save(config.outputFile);
}

void PlotRenderer::drawGrid(QPainter& painter, double maxPrice, double minPrice, double gridMaxPrice, double gridMinPrice) {
    int rows = m_config.gridRowCount;
    int cols = m_config.gridColunnCount;
    int width = painter.viewport().width();

    // �̶�
    int textWidth = painter.fontMetrics().horizontalAdvance(MathUtil::formatDoubleWithStep(gridMaxPrice, m_config.minTick));
    int textHeight = painter.fontMetrics().height();

    // ����߶�
    int gridHeight = (painter.viewport().height() - m_config.gridTopPadding) / (rows - 1);

    for (int i = 0; i < rows; ++i) {
        int y = m_config.gridTopPadding + i * gridHeight;
        painter.setPen(m_config.gridColor);
        painter.drawLine(0, y, m_config.width, y);

        painter.setPen(Qt::darkGray);

        double  startX = painter.viewport().width() - textWidth - 5;
        QString str = MathUtil::formatDoubleWithStep(gridMaxPrice + i * (gridMinPrice - gridMaxPrice) / (rows - 1), m_config.minTick);
        painter.drawText(startX, y - textHeight, textWidth, textHeight, 0, str);
    }

    painter.setPen(m_config.gridColor);
    int y = m_config.gridTopPadding + (rows - 1) * painter.viewport().height() / rows;
    for (int i = 1; i <= cols; ++i) {
        int x = i * width / cols;
        painter.drawLine(x, m_config.gridTopPadding, x, y);
    }
}

QString PlotRenderer::makeMAIndicatorString(int period, double value) {
    return QString("MA(%1):%2").arg(period).arg(value > 0 ? MathUtil::formatDoubleWithStep(value, m_config.minTick) : "-");
}

void PlotRenderer::drawIndicatorText(QPainter& painter) {
    const QuoteCandleDataPtr& lastest = m_dataList.back();

    QString ma5String = makeMAIndicatorString(5, lastest->ma5);
    QString ma10String = makeMAIndicatorString(10, lastest->ma10);
    QString ma20String = makeMAIndicatorString(20, lastest->ma20);

    int ma5StringWidth = painter.fontMetrics().horizontalAdvance(ma5String);
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
    painter.setPen(m_config.ma5Color);
    painter.drawText(basePosX, basePosY, ma5StringWidth, height, 0, ma5String);
    basePosX += (spacingX + ma5StringWidth);

    painter.setPen(m_config.ma10Color);
    painter.drawText(basePosX, basePosY, ma10StringWidth, height, 0, ma10String);
    basePosX += (spacingX + ma10StringWidth);

    painter.setPen(m_config.ma20Color);
    painter.drawText(basePosX, basePosY, ma20StringWidth, height, 0, ma20String);
    basePosX += (spacingX + ma20StringWidth);

    painter.restore();
}

void PlotRenderer::drawCandleChart(QPainter& painter) {
    if (m_context->dataList.empty()) {
        return;
    }

    QRect viewport = painter.viewport();
    // K ��ͼ����
    int klineAreaHeight = viewport.height();

    // �������±߾ൽ�۸�Χ
    double priceRange = m_context->gridMaxPrice - m_context->gridMinPrice;
    float candleWidth = m_context->width;

    // ���� K ��
    for (size_t i = m_context->startIndex; i < m_context->endIndex; ++i) {
        const auto& data = m_context->dataList[i];
        float x = m_context->leftOffset + (i - m_context->startIndex) * (candleWidth + m_context->spacing);
        float yOpen  = (1.0 - float(data->open  - m_context->gridMinPrice) / priceRange) * float(klineAreaHeight);
        float yClose = (1.0 - float(data->close - m_context->gridMinPrice) / priceRange) * float(klineAreaHeight);
        float yHigh  = (1.0 - float(data->high  - m_context->gridMinPrice) / priceRange) * float(klineAreaHeight);
        float yLow   = (1.0 - float(data->low   - m_context->gridMinPrice) / priceRange) * float(klineAreaHeight);

        QColor color = (data->close >= data->open) ? m_config.riseColor : m_config.fallColor;
        painter.setPen(color);
        painter.drawLine(QPointF(x + candleWidth / 2.0, yHigh),QPointF(x + candleWidth / 2, yLow)); // ����Ӱ��
        painter.setBrush(color);
        painter.drawRect(QRectF(x, std::min(yOpen, yClose), candleWidth, std::abs(yClose - yOpen))); // ����ʵ��

        // ����focus������
        if (m_context->focusIndex >= 0 && m_context->focusIndex == i) {
            painter.setPen(Qt::black);
            painter.drawLine(QPointF(x + candleWidth / 2.0, 0), QPointF(x + candleWidth / 2, klineAreaHeight)); // ����Ӱ��
        }
    }

    // ������߼ۺ���ͼ۱��
    drawPriceMarker(painter, m_context->maxIndex, m_context->maxPrice, m_context->gridMinPrice, true, klineAreaHeight, priceRange,  m_context->minPrice, candleWidth, 0);
    drawPriceMarker(painter, m_context->minIndex, m_context->minPrice, m_context->gridMinPrice, false, klineAreaHeight, priceRange, m_context->minPrice, candleWidth, 0);

    // ���ƾ���
    // drawKLineMA(painter  offsetof(QuoteCandleData, ma5), klineHeight, priceRange, minPrice, candleWidth);
    // drawKLineMA(painter, offsetof(QuoteCandleData, ma10), klineHeight, priceRange, minPrice, candleWidth);
    // drawKLineMA(painter, offsetof(QuoteCandleData, ma20), klineHeight, priceRange, minPrice, candleWidth);
}

void PlotRenderer::drawVolume(QPainter& painter) {
    if (m_context->dataList.empty()) {
        return;
    }

    // �ɽ�����״ͼ����
    int volumeHeight = painter.viewport().height();
    int candleWidth = m_context->width;
    // K ��ͼ����
    QRect viewport = painter.viewport();
    int klineAreaHeight = viewport.height();

    // �ҵ����ɽ��������ڹ�һ��
    double maxVolume = 0;
    double minVolume = 1e30;
    for (size_t i = m_context->startIndex; i < m_context->endIndex; ++i) {
        const auto& data = m_context->dataList[i];
        maxVolume = std::max(maxVolume, data->volume);
        minVolume = std::min(minVolume, data->volume);
    }

    // ����ÿ���ɽ�����״ͼ
    for (size_t i = m_context->startIndex; i < m_context->endIndex; ++i) {
        const auto& data = m_context->dataList[i];
        float x = m_context->leftOffset + (i - m_context->startIndex) * (candleWidth + m_context->spacing);
        int y = volumeHeight - (data->volume / maxVolume) * volumeHeight;
        int height = (data->volume / maxVolume) * volumeHeight;

        // ������ɫ�������̵���
        QColor color = (data->close >= data->open) ? m_config.riseColor : m_config.fallColor;
        painter.setBrush(color);
        painter.setPen(Qt::NoPen);
        painter.drawRect(x, y, candleWidth, height);

        // ����focus������
        if (m_context->focusIndex >= 0 && m_context->focusIndex == i) {
            painter.setPen(Qt::black);
            painter.drawLine(QPointF(x + candleWidth / 2.0, 0), QPointF(x + candleWidth / 2, klineAreaHeight)); // ����Ӱ��
        }
    }
}

// �����ƶ�ƽ��ֵ����䵽 QuoteCandleData ��
void PlotRenderer::calculateMovingAverages(std::vector<QuoteCandleDataPtr>& dataList) {
    int dataSize = dataList.size();

    for (int i = 0; i < dataSize; ++i) {
        // ���� MA5
        if (i >= 4) { // ������Ҫ 5 ������
            double sum = 0.0;
            for (int j = i; j > i - 5; --j) {
                sum += dataList[j]->close;
            }
            dataList[i]->ma5 = sum / 5.0;
        }

        // ���� MA10
        if (i >= 9) { // ������Ҫ 10 ������
            double sum = 0.0;
            for (int j = i; j > i - 10; --j) {
                sum += dataList[j]->close;
            }
            dataList[i]->ma10 = sum / 10.0;
        }

        // ���� MA20
        if (i >= 19) { // ������Ҫ 20 ������
            double sum = 0.0;
            for (int j = i; j > i - 20; --j) {
                sum += dataList[j]->close;
            }
            dataList[i]->ma20 = sum / 20.0;
        }
    }
}

void PlotRenderer::drawVolumeMA(QPainter& painter, const std::vector<double>& ma, const QColor& color, int volumeTop, int volumeHeight, double maxVolume) {
    if (ma.empty()) return;

    painter.setPen(QPen(color, 2));

    QPainterPath path;
    for (size_t i = 0; i < ma.size(); ++i) {
        if (ma[i] == 0) continue; // ������Ч����

        int x = i * (m_config.width / m_dataList.size()) + (m_config.width / m_dataList.size()) / 2;
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

void PlotRenderer::drawPriceMarker(QPainter& painter, int index, double price, double gridMinPrice, bool isMax, int klineAreaHeight, double priceRange, double minPrice, int candleWidth, int padding) {
    if (m_context->dataList.empty()) {
        return;
    }

    // ������λ��
    float x = m_context->leftOffset + (index - m_context->startIndex) * (candleWidth + m_context->spacing);
    float y = padding + (1 - (price - gridMinPrice) / priceRange) * (klineAreaHeight);

    // �ж��������췽��
    bool extendRight = (index < (m_context->endIndex - m_context->startIndex) / 2); // �������벿�֣�����������

    // ����������ʽ
    QPen pen;
    pen.setColor(Qt::darkGray);
    pen.setStyle(Qt::DashLine);// ����Ϊ����
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

void PlotRenderer::drawKLineMA(QPainter& painter, int fieldOffset, const QColor& color, int klineHeight, double priceRange, double minPrice, int candleWidth) {
    painter.setPen(QPen(color, 2)); // ���þ�����ɫ���߿�

    QPainterPath path;
    for (size_t i = 0; i < m_dataList.size(); ++i) {
        double ma = *(double*)((&m_dataList[i]) + fieldOffset);
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
