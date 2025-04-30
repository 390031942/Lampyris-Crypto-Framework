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
    // ��ԼID
    QString symbol;

    // ���³ɽ���
    double price;

    // ���³ɽ���������0 ����û�гɽ���
    double lastSize;

    // 24Сʱ��߼�
    double high;

    // 24Сʱ��ͼ�
    double low;

    // 24Сʱ�ɽ������Ա�Ϊ��λ
    double volumn;

    // 24Сʱ�ɽ���������Ϊ��λ
    double currency;

    // ticker���ݲ���ʱ�䣬Unixʱ����ĺ�������ʽ���� 1597026383085
    qint64 timestamp;

    // �Ƿ�
    double changePerc;

    // �ǵ���
    double change;

    // ��Ǽ۸�
    double markPrice;

    // ָ���۸�
    double indexPrice;

    // �ʽ����
    double fundingRate;

    // ��һ���ʽ�ʱ���
    qint64 nextFundingTime;

    // ����
    double riseSpeed;

    // �춯��ǩ
    QStringList labels;

    // ��ȡ����
    double getAvgPrice() const {
        return (currency != 0) ? volumn / currency : 0.0;
    }
};

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

class MarketSummaryData {
public:
    // �ǵ�ƽ����
    int riseCount;
    int fallCount;
    int unchangedCount;

    // ƽ���ǵ���(���ʱ�� UTC+0)
    double avgChangePerc;

    // ǰ10��ƽ���ǵ���(���ʱ�� UTC+0)
    double top10AvgChangePerc;
    double last10AvgChangePerc;

    // ������ƽ���ǵ���
    double mainStreamAvgChangePerc;

    // ��ȡ USDT ������Լ����
    inline int getContractCount() const {
        return riseCount + fallCount + unchangedCount;
    }
};