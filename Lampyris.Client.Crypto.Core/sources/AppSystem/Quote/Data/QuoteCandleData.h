#pragma once
// QT Include(s)
#include <QDateTime>

// STD Include(s)
#include <memory>

// K线数据结构
struct QuoteCandleData {
    QDateTime dateTime;
    double    open;
    double    close;
    double    high;
    double    low;
    double    volume;
    double    currency;

    inline double getPercentage() const {
        if (open <= 0) {
            return 0.0;
        }
        return (close - open) / open * 100;
    }
};

typedef std::shared_ptr<QuoteCandleData> QuoteCandleDataPtr;

#include <QString>
#include <QList>
#include <QStringList>

class QuoteTickerData {
public:
    // 合约ID
    QString symbol;

    // 最新成交价
    double price;

    // 最新成交的数量，0 代表没有成交量
    double lastSize;

    // 24小时最高价
    double high;

    // 24小时最低价
    double low;

    // 24小时成交量，以币为单位
    double volumn;

    // 24小时成交量，以张为单位
    double currency;

    // ticker数据产生时间，Unix时间戳的毫秒数格式，如 1597026383085
    qint64 timestamp;

    // 涨幅
    double changePerc;

    // 涨跌额
    double change;

    // 标记价格
    double markPrice;

    // 指数价格
    double indexPrice;

    // 资金费率
    double fundingRate;

    // 下一次资金时间戳
    qint64 nextFundingTime;

    // 涨速
    double riseSpeed;

    // 异动标签
    QStringList labels;

    // 获取均价
    double getAvgPrice() const {
        return (currency != 0) ? volumn / currency : 0.0;
    }
};

class QuoteTradeData {
public:
    // 最新成交价
    double price;

    // 成交数量
    double quantity;

    // 成交时间
    QDateTime tradeTime;

    // 买方是否是做市商
    bool buyerIsMaker;

    // 设置成交时间（以 Unix 时间戳为输入）
    inline void setTradeTimeFromTimestamp(qint64 timestamp) {
        tradeTime = QDateTime::fromMSecsSinceEpoch(timestamp, Qt::UTC);
    }

    // 获取成交时间的 Unix 时间戳
    inline qint64 getTradeTimeAsTimestamp() const {
        return tradeTime.toMSecsSinceEpoch();
    }
};

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