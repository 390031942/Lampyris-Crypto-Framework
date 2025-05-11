#pragma once
// QT Include(s)
#include <QDateTime>

// STD Include(s)
#include <memory>

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

typedef std::shared_ptr<QuoteTradeData> QuoteTradeDataPtr;