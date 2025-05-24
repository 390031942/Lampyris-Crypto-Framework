#pragma once
// QT Include(s)
#include <QWidget>
#include <QLayout>
#include <QLabel>

// Project Include(s)
#include "PercentageDisplayText.h"
#include "AmountText.h"
#include "PriceText.h"
#include "AppSystem/Quote/Data/QuoteTickerData.h"

/// <summary>
// �����б��е�Item, ��������ƽ̨����������������б� �Լ������б� 
/// </summary>
class QuoteListItem : public QWidget {
    Q_OBJECT
public:
    enum DisplayMode {
        SearchListMode,       // �����б�ģʽ
        MobileListMode,       // �ƶ��������б�ģʽ
        PCListMode            // PC�������б�ģʽ
    };
    explicit QuoteListItem(DisplayMode mode, QWidget* parent = Q_NULLPTR);
    void                   refresh(const QuoteTickerDataPtr tickerData, double minTick);
private:
    void                   setupUI(DisplayMode mode); // ����ģʽ����UI����
    QLabel*                m_symbol;
    AmountText*            m_24hVolumeCurrency;
    PriceText*             m_price;
    PercentageDisplayText* m_percentage;
    PercentageDisplayText* m_riseSpeed;
};