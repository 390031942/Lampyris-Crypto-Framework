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
    // ���׶�
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

typedef std::shared_ptr<QuoteTickerData> QuoteTickerDataPtr;