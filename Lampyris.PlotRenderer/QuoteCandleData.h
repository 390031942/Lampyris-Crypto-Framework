#pragma once
// QT Include(s)
#include <QDateTime>

// STD Include(s)
#include <memory>

// K�����ݽṹ
struct QuoteCandleData {
    QDateTime dateTime;
    double    open;
    double    close;
    double    high;
    double    low;
    double    volume;
    double    currency;
              
    double    ma5 = 0.0;
    double    ma10 = 0.0;
    double    ma20 = 0.0;
};

typedef std::shared_ptr<QuoteCandleData> QuoteCandleDataPtr;