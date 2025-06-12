#pragma once
// QT Include(s)
#include <QWidget>

// Project Include(s)
#include "Protocol/quote.h"
#include "AppSystem/Quote/Manager/QuoteManager.h"

// STD Include(s)
#include <vector>

class MarketOverviewBarChartWidget : public QWidget {
    Q_OBJECT
    using DataList = std::vector<MarketPreviewIntervalData>;
public:
    explicit         MarketOverviewBarChartWidget(QWidget* parent = nullptr);
                    ~MarketOverviewBarChartWidget();
    void             setBarWidth(int width);                            
    void             setDistributionBarHeight(int height);              
    void             setDistributionBarSpacing(int spacing);     
    void             setExpectedIntervals(std::initializer_list<int> list);
protected:         
    void             paintEvent(QPaintEvent* event) override;
private:           
    void             updateSumamryData(const MarketSummaryData& data);
    // 涨跌分布区域数据列表
    DataList         m_data;
    // 间隔列表,如{-10,-5,0,5,10},则会展示 
    // <-10%,  -10%~-5%,  -5%~0%,  0%~5%  ,5%~10%,  >10% 这几个区间的数据
    // 区间内的数据会从MarketSummaryData中统计得到
    std::vector<int> m_intervals;
    // 柱状宽度
    int              m_barWidth;          
    // 分布条高度
    int              m_distributionBarHeight;
    // 分布条矩形间距
    int              m_distributionBarSpacing;
    int              m_callbackId;
};
