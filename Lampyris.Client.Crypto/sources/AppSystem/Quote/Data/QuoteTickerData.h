#pragma once
// QT Include(s)
#include <QDateTime>
#include <QString>
#include <QList>
#include <QStringList>

// STD Include(s)
#include <memory>

class QuoteTickerData {
public:
    // 交易对
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

typedef std::shared_ptr<QuoteTickerData> QuoteTickerDataPtr;