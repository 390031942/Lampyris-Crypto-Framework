#pragma once
// QT Include(s)
#include <QDateTime>

// STD Include(s)
#include <memory>
#include <array>

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

    inline bool isValid() const {
        return close > 0 && dateTime.isValid();
    }
};

#define QUOTE_CANDLE_DATA_SEGMENT_SIZE 1500
typedef std::shared_ptr<QuoteCandleData> QuoteCandleDataPtr;
typedef std::array<QuoteCandleDataPtr, QUOTE_CANDLE_DATA_SEGMENT_SIZE> QuoteCandleDataSegment;
typedef std::shared_ptr<QuoteCandleDataSegment> QuoteCandleDataSegmentPtr;
typedef std::vector<QuoteCandleDataPtr> QuoteCandleDataDynamicSegment;
typedef std::shared_ptr<QuoteCandleDataDynamicSegment> QuoteCandleDataDynamicSegmentPtr;