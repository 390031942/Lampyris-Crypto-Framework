#pragma once
#include <QWidget>
#include <QLabel>
#include <QHBoxLayout>
#include <QFrame>
#include <QString>

class StatusBarQuoteItem : public QWidget {
    Q_OBJECT

public:
    enum Mode {
        Symbol,         // 显示单个 symbol 的行情
        MarketOverview  // 显示市场总览
    };

    explicit     StatusBarQuoteItem(QWidget* parent = nullptr);
    // 设置模式
    void         setMode(Mode mode);
    // 设置 symbol 模式的数据
    void         setSymbolData(const QString& symbol, const QString& price, const QString& changePercent);
    // 设置市场总览模式的数据
    void         setMarketOverviewData(const QString& averageChangePercent, int upCount, int flatCount, int downCount);
    // 设置是否显示右侧分割线
    void         setShowDivider(bool show);
private:
    Mode         m_mode;
    QString      m_symbol;
    QString      m_price;
    QString      m_changePercent;
                 
    QString      m_averageChangePercent;
    int          m_upCount;
    int          m_flatCount;
    int          m_downCount;
    bool         m_showDivider;

    QLabel*      m_symbolLabel;
    QLabel*      m_priceLabel;
    QLabel*      m_changePercentLabel;
    QLabel*      m_dividerLabel;
    QLabel*      m_marketOverviewLabel;
    QLabel*      m_averageChangePercentLabel;
    QLabel*      m_upCountLabel;
    QLabel*      m_flatCountLabel;
    QLabel*      m_downCountLabel;
    QHBoxLayout* m_mainLayout;

    QWidget*     m_symbolWidget;
    QWidget*     m_marketOverviewWidget;

    void         initUI();
    void         updateUI();
};
