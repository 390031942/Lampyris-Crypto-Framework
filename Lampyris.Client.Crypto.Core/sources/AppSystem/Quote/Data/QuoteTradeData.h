#pragma once
// QT Include(s)
#include <QDateTime>

// STD Include(s)
#include <memory>

class QuoteTradeData {
public:
    // ���³ɽ���
    double price;

    // �ɽ�����
    double quantity;

    // �ɽ�ʱ��
    QDateTime tradeTime;

    // ���Ƿ���������
    bool buyerIsMaker;

    // ���óɽ�ʱ�䣨�� Unix ʱ���Ϊ���룩
    inline void setTradeTimeFromTimestamp(qint64 timestamp) {
        tradeTime = QDateTime::fromMSecsSinceEpoch(timestamp, Qt::UTC);
    }

    // ��ȡ�ɽ�ʱ��� Unix ʱ���
    inline qint64 getTradeTimeAsTimestamp() const {
        return tradeTime.toMSecsSinceEpoch();
    }
};

typedef std::shared_ptr<QuoteTradeData> QuoteTradeDataPtr;