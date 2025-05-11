#include "MarketOverviewBarChartWidget.h"
#include <QPainter>
#include <QFontMetrics>
#include <QDebug>

MarketOverviewBarChartWidget::MarketOverviewBarChartWidget(QWidget* parent)
    : QWidget(parent), m_barWidth(30), m_distributionBarHeight(20), m_distributionBarSpacing(5) {
    // 默认柱状宽度为 30，分布条高度为 20，矩形间距为 5
}

void MarketOverviewBarChartWidget::setData(const QVector<MarketPreviewIntervalDataBean>& data) {
    m_data = data;
    update(); // 更新绘图
}

void MarketOverviewBarChartWidget::setBarWidth(int width) {
    m_barWidth = width;
    update(); // 更新绘图
}

void MarketOverviewBarChartWidget::setDistributionBarHeight(int height) {
    m_distributionBarHeight = height;
    update(); // 更新绘图
}

void MarketOverviewBarChartWidget::setDistributionBarSpacing(int spacing) {
    m_distributionBarSpacing = spacing;
    update(); // 更新绘图
}

void MarketOverviewBarChartWidget::paintEvent(QPaintEvent* event) {
    Q_UNUSED(event);

    if (m_data.isEmpty()) {
        return; // 如果没有数据，不绘制
    }

    QPainter painter(this);
    painter.setRenderHint(QPainter::Antialiasing);

    // 获取窗口宽高
    int widgetWidth = width();
    int widgetHeight = height();

    // 计算水平线的位置
    int baselineY = widgetHeight * 0.6;

    // 确保柱状图不会超出窗口顶部
    int maxBarHeight = baselineY * 0.8; // 柱状图最大高度为水平线以上 80% 的空间

    // 绘制水平线
    QPen linePen(Qt::black, 2);
    painter.setPen(linePen);
    painter.drawLine(0, baselineY, widgetWidth, baselineY);

    // 计算柱状图总宽度
    int totalBarWidth = m_data.size() * m_barWidth + (m_data.size() - 1) * m_barWidth / 2;

    // 计算柱状图的起始位置，使其居中
    int startX = (widgetWidth - totalBarWidth) / 2;

    // 统计涨跌平数量
    int totalUp = 0, totalDown = 0, totalFlat = 0;
    for (const auto& bean : m_data) {
        if (bean.lowerBoundPerc == 0 && bean.upperBoundPerc == 0) {
            totalFlat += bean.count; // 平盘
        }
        else if (bean.upperBoundPerc < 0) {
            totalDown += bean.count; // 跌幅
        }
        else {
            totalUp += bean.count; // 涨幅
        }
    }
    int totalCount = totalUp + totalDown + totalFlat;

    // 绘制柱状图
    for (int i = 0; i < m_data.size(); ++i) {
        const auto& bean = m_data[i];

        // 设置柱状颜色
        QColor barColor;
        if (bean.lowerBoundPerc == 0 && bean.upperBoundPerc == 0) {
            barColor = Qt::gray; // 平盘
        }
        else if (bean.upperBoundPerc < 0) {
            barColor = Qt::red; // 跌幅
        }
        else {
            barColor = Qt::green; // 涨幅
        }

        // 计算柱状高度
        int barHeight = bean.count * 10; // 假设每个单位数量对应 10 像素高度
        barHeight = qMin(barHeight, maxBarHeight); // 限制柱状高度不超过最大高度
        int barX = startX + i * (m_barWidth + m_barWidth / 2);
        int barY = baselineY - barHeight;

        // 绘制柱状
        painter.setBrush(barColor);
        painter.setPen(Qt::NoPen);
        painter.drawRect(barX, barY, m_barWidth, barHeight);

        // 绘制价格区间文字
        QString label;
        if (bean.lowerBoundPerc == 0 && bean.upperBoundPerc == 0) {
            label = "平盘";
        }
        else if (bean.upperBoundPerc < 0 && bean.lowerBoundPerc == -1) {
            label = QString("<%1%").arg(bean.upperBoundPerc);
        }
        else if (bean.lowerBoundPerc > 0 && bean.upperBoundPerc == -1) {
            label = QString(">%1%").arg(bean.lowerBoundPerc);
        }
        else {
            label = QString("%1%~%2%").arg(bean.lowerBoundPerc).arg(bean.upperBoundPerc);
        }

        QFont font = painter.font();
        font.setPointSize(10);
        painter.setFont(font);
        QFontMetrics metrics(font);
        int textWidth = metrics.horizontalAdvance(label);
        int textX = barX + (m_barWidth - textWidth) / 2;
        int textY = baselineY + 20;

        painter.setPen(Qt::black);
        painter.drawText(textX, textY, label);
    }

    // 绘制涨跌平水平分布条
    int distributionBarY = baselineY + 50;
    int distributionBarX = 50;
    int distributionBarWidth = widgetWidth - 100;

    int upWidth = (totalUp * (distributionBarWidth - 2 * m_distributionBarSpacing)) / totalCount;
    int flatWidth = (totalFlat * (distributionBarWidth - 2 * m_distributionBarSpacing)) / totalCount;
    int downWidth = (totalDown * (distributionBarWidth - 2 * m_distributionBarSpacing)) / totalCount;

    // 涨幅矩形
    painter.setBrush(Qt::green);
    painter.drawRect(distributionBarX, distributionBarY, upWidth, m_distributionBarHeight);

    // 平盘矩形
    painter.setBrush(Qt::gray);
    painter.drawRect(distributionBarX + upWidth + m_distributionBarSpacing, distributionBarY, flatWidth, m_distributionBarHeight);

    // 跌幅矩形
    painter.setBrush(Qt::red);
    painter.drawRect(distributionBarX + upWidth + flatWidth + 2 * m_distributionBarSpacing, distributionBarY, downWidth, m_distributionBarHeight);

    // 绘制涨跌总数文本
    QString upText = QString("上涨: %1家").arg(totalUp);
    QString downText = QString("下跌: %1家").arg(totalDown);

    QFontMetrics metrics(painter.font());
    int upTextWidth = metrics.horizontalAdvance(upText);
    int downTextWidth = metrics.horizontalAdvance(downText);

    painter.setPen(Qt::black);
    painter.drawText(distributionBarX, distributionBarY + m_distributionBarHeight + 20, upText);
    painter.drawText(distributionBarX + distributionBarWidth - downTextWidth, distributionBarY + m_distributionBarHeight + 20, downText);
}