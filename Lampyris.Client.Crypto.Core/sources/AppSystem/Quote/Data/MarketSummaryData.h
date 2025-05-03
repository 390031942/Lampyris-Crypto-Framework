#pragma once
// QT Include(s)
#include <QDateTime>

// STD Include(s)
#include <memory>

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

typedef std::shared_ptr<MarketSummaryData> MarketSummaryDataPtr;