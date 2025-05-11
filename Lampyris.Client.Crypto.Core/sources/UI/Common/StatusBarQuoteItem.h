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
        Symbol,         // ��ʾ���� symbol ������
        MarketOverview  // ��ʾ�г�����
    };

    explicit     StatusBarQuoteItem(QWidget* parent = nullptr);
    // ����ģʽ
    void         setMode(Mode mode);
    // ���� symbol ģʽ������
    void         setSymbolData(const QString& symbol, const QString& price, const QString& changePercent);
    // �����г�����ģʽ������
    void         setMarketOverviewData(const QString& averageChangePercent, int upCount, int flatCount, int downCount);
    // �����Ƿ���ʾ�Ҳ�ָ���
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
