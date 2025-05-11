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
        Symbol,         // 显示单个 symbol 的行情
        MarketOverview  // 显示市场总览
    };

    explicit     StatusBarQuoteItem(QWidget* parent = nullptr);
    // 设置模式
    void         setSymbolMode(const QString& symbol);
    void         setMarketOverviewMode();
    // 设置 symbol 模式的数据
    void         setSymbolData(double price, double changePercent);
    // 设置市场总览模式的数据
    void         setMarketOverviewData(double averageChangePercent, double top10AvgChangePercent, double last10AvgChangePercent, int upCount, int flatCount, int downCount);
    // 设置是否显示右侧分割线
    void         setShowDivider(bool show);
    // 重置显示模式为None，取消行情数据监听，在设置模式或MainStatusBar调用。
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
