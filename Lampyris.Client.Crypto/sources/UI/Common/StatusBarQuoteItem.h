#pragma once
// QT Include(s)
#include <QWidget>
#include <QLabel>
#include <QHBoxLayout>
#include <QFrame>
#include <QString>

// Project Include(s)
#include "AppSystem/Quote/Data/QuoteTickerData.h"
#include "AppSystem/Quote/Data/MarketSummaryData.h"

class StatusBarQuoteItem : public QWidget {
    Q_OBJECT

public:
    enum Mode {
        None,      
        Symbol,         // ��ʾ���� symbol ������
        MarketOverview  // ��ʾ�г�����
    };

    explicit     StatusBarQuoteItem(QWidget* parent = nullptr);
    // ����ģʽ
    void         setSymbolMode(const QString& symbol);
    void         setMarketOverviewMode();
    // ���� symbol ģʽ������
    void         setSymbolData(double price, double changePercent);
    // �����г�����ģʽ������
    void         setMarketOverviewData(double averageChangePercent, double top10AvgChangePercent, double last10AvgChangePercent, int upCount, int flatCount, int downCount);
    // �����Ƿ���ʾ�Ҳ�ָ���
    void         setShowDivider(bool show);
    // ������ʾģʽΪNone��ȡ���������ݼ�����������ģʽ��MainStatusBar���á�
    void         reset();
private:
    void         updateSymbolQuoteData(const QuoteTickerData& tickerData);
    void         updateMarketOverviewData(const MarketSummaryData& summaryData);
    Mode         m_mode;
    QString      m_symbol;
    QString      m_price;
    QString      m_changePercent;
                 
    QString      m_averageChangePercent;
    QString      m_top10AvgChangePercent;
    QString      m_last10AvgChangePercent;

    QString      m_upCount;
    QString      m_flatCount;
    QString      m_downCount;
    bool         m_showDivider;

    int          m_callbackId = -1;

    QLabel*      m_symbolLabel;
    QLabel*      m_priceLabel;
    QLabel*      m_changePercentLabel;
    QLabel*      m_dividerLabel;
    QLabel*      m_averageChangePercentLabel;
    QLabel*      m_top10AvgChangePercentLabel;
    QLabel*      m_last10AvgChangePercentLabel;
    QLabel*      m_upCountLabel;
    QLabel*      m_flatCountLabel;
    QLabel*      m_downCountLabel;
    QHBoxLayout* m_mainLayout;

    QWidget*     m_symbolWidget;
    QWidget*     m_marketOverviewWidget;

    void         initUI();
    void         updateUI();
};
