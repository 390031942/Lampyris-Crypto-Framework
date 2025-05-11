#pragma once
// QT Include(s)
#include <QWidget>
#include <QVector>

struct MarketPreviewIntervalDataBean {
    int lowerBoundPerc; // 下界(%)
    int upperBoundPerc; // 上界(%)
    int count;          // 数量
};

class MarketOverviewBarChartWidget : public QWidget {
    Q_OBJECT

public:
    explicit MarketOverviewBarChartWidget(QWidget* parent = nullptr);

    void setData(const QVector<MarketPreviewIntervalDataBean>& data); // 设置数据
    void setBarWidth(int width);                                     // 设置柱状宽度
    void setDistributionBarHeight(int height);                       // 设置分布条高度
    void setDistributionBarSpacing(int spacing);                     // 设置分布条矩形间距

protected:
    void paintEvent(QPaintEvent* event) override;

private:
    QVector<MarketPreviewIntervalDataBean> m_data; // 数据
    int m_barWidth;                                // 柱状宽度
    int m_distributionBarHeight;                  // 分布条高度
    int m_distributionBarSpacing;                 // 分布条矩形间距
};
