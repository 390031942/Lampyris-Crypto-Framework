#pragma once
// QT Include(s)
#include <QDateTime>

// STD Include(s)
#include <memory>

class MarketSummaryData {
public:
    // 涨跌平数量
    int riseCount;
    int fallCount;
    int unchangedCount;

    // 平均涨跌幅(相对时区 UTC+0)
    double avgChangePerc;

    // 前10名平均涨跌幅(相对时区 UTC+0)
    double top10AvgChangePerc;
    double last10AvgChangePerc;

    // 主流币平均涨跌幅
    double mainStreamAvgChangePerc;

    // 获取 USDT 永续合约总数
    inline int getContractCount() const {
        return riseCount + fallCount + unchangedCount;
    }
};

typedef std::shared_ptr<MarketSummaryData> MarketSummaryDataPtr;