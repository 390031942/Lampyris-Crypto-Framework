// Project Include(s)
#include "StatusBarQuoteItem.h"
#include "ControlFactory.h"

StatusBarQuoteItem::StatusBarQuoteItem(QWidget* parent)
    : QWidget(parent), m_mode(Symbol), m_showDivider(true) {
    initUI();
}

// 设置模式
void StatusBarQuoteItem::setMode(Mode mode) {
    m_mode = mode;
    updateUI();
}

// 设置 symbol 模式的数据
void StatusBarQuoteItem::setSymbolData(const QString& symbol, const QString& price, const QString& changePercent) {
    m_symbol = symbol;
    m_price = price;
    m_changePercent = changePercent;
    updateUI();
}

// 设置市场总览模式的数据
void StatusBarQuoteItem::setMarketOverviewData(const QString& averageChangePercent, int upCount, int flatCount, int downCount) {
    m_averageChangePercent = averageChangePercent;
    m_upCount = upCount;
    m_flatCount = flatCount;
    m_downCount = downCount;
    updateUI();
}

// 设置是否显示右侧分割线
void StatusBarQuoteItem::setShowDivider(bool show) {
    m_showDivider = show;
    m_dividerLabel->setVisible(show);
}

void StatusBarQuoteItem::initUI() {
    m_mainLayout = new QHBoxLayout(this);
    m_mainLayout->setContentsMargins(5, 0, 5, 0);
    m_mainLayout->setSpacing(10);

    // Symbol 模式控件
    {
        m_symbolWidget = new QWidget(this);
        m_symbolLabel = new QLabel(m_symbolWidget);
        m_priceLabel = new QLabel(m_symbolWidget);
        m_changePercentLabel = new QLabel(m_symbolWidget);

        QHBoxLayout* layout = new QHBoxLayout();
        layout->setContentsMargins(0, 0, 0, 0);
        layout->setSpacing(10);

        layout->addWidget(m_symbolLabel);
        layout->addWidget(m_priceLabel);
        layout->addWidget(m_changePercentLabel);

        m_symbolWidget->setLayout(layout);
        m_mainLayout->addWidget(m_symbolWidget);
    }

    // 市场总览模式控件
    {
        m_marketOverviewWidget = new QWidget(this);
        m_marketOverviewLabel = new QLabel(QString::fromLocal8Bit("市场总览"), m_marketOverviewWidget);
        m_averageChangePercentLabel = new QLabel(m_marketOverviewWidget);
        m_upCountLabel = new QLabel(m_marketOverviewWidget);
        m_flatCountLabel = new QLabel(m_marketOverviewWidget);
        m_downCountLabel = new QLabel(m_marketOverviewWidget);

        QHBoxLayout* layout = new QHBoxLayout();
        layout->setContentsMargins(0, 0, 0, 0);
        layout->setSpacing(5);

        layout = new QHBoxLayout();
        layout->setSpacing(5);
        layout->addWidget(m_marketOverviewLabel);
        layout->addWidget(m_averageChangePercentLabel);
        layout->addWidget(m_upCountLabel);
        layout->addWidget(ControlFactory::createVerticalSplitterLabel(m_marketOverviewWidget));
        layout->addWidget(m_flatCountLabel);
        layout->addWidget(ControlFactory::createVerticalSplitterLabel(m_marketOverviewWidget));
        layout->addWidget(m_downCountLabel);

        m_upCountLabel->setStyleSheet("color: #F2505F;");
        m_flatCountLabel->setStyleSheet("color: #999999;");
        m_downCountLabel->setStyleSheet("color: #52BD87;");

        m_marketOverviewWidget->setLayout(layout);
        m_mainLayout->addWidget(m_marketOverviewWidget);
    }

    // 分割线
    {
        m_dividerLabel = ControlFactory::createVerticalSplitterLabel(this);
        m_mainLayout->addWidget(m_dividerLabel);
    }

    setLayout(m_mainLayout);
    updateUI();
}

void StatusBarQuoteItem::updateUI() {
    m_symbolWidget->setVisible(false);
    m_marketOverviewWidget->setVisible(false);

    if (m_mode == Symbol) {
        m_symbolLabel->setText(m_symbol);
        m_priceLabel->setText(m_price);
        m_changePercentLabel->setText(m_changePercent);

        m_symbolWidget->setVisible(true);
    }
    else if (m_mode == MarketOverview) {
        m_upCountLabel->setText(QString::number(m_upCount));
        m_flatCountLabel->setText(QString::number(m_flatCount));
        m_downCountLabel->setText(QString::number(m_downCount));

        m_marketOverviewWidget->setVisible(true);
    }
}
