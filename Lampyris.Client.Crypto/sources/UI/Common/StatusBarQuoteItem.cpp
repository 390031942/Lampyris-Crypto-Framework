// Project Include(s)
#include "StatusBarQuoteItem.h"
#include "ControlFactory.h"
#include "Util/StringUtil.h"
#include "AppSystem/Quote/Manager/QuoteManager.h"

StatusBarQuoteItem::StatusBarQuoteItem(QWidget* parent)
    : QWidget(parent), m_mode(Mode::None), m_showDivider(true), m_callbackId(-1) {
    initUI();
}

void StatusBarQuoteItem::setSymbolMode(const QString& symbol) {
    reset();
    m_callbackId = QuoteManager::getInstance()->getSymbolTickerUpdateDelegate(m_symbol) += [this](const QuoteTickerData& data) {
        this->setSymbolData(data.price,data.changePerc);
    };
    m_mode = Mode::Symbol;
    m_symbol = symbol;
    m_price = "--";
    m_changePercent = "--";
    updateUI();
}

void StatusBarQuoteItem::setMarketOverviewMode() {
    reset();
    m_callbackId = QuoteManager::getInstance()->onMarketSummaryDataUpdate += [this](const MarketSummaryData& data) {
       this->updateMarketOverviewData(data);
    };
    m_mode = Mode::MarketOverview;
    m_averageChangePercent = "0.00%";
    m_top10AvgChangePercent = "0.00%";
    m_last10AvgChangePercent = "0.00%";

    m_upCount = "9999";
    m_flatCount = "0";
    m_downCount = "12";
    updateUI();
}

// 设置 symbol 模式的数据
void StatusBarQuoteItem::setSymbolData(double price, double changePercent) {
    m_price = QString::number(price);
    m_changePercent = StringUtil::getPercentageText(changePercent);
    updateUI();
}

// 设置市场总览模式的数据
void StatusBarQuoteItem::setMarketOverviewData(double averageChangePercent, double top10ChangePercent, double last10ChangePercent, 
    int upCount, int flatCount, int downCount) {
    m_averageChangePercent = StringUtil::getPercentageText(averageChangePercent);
    m_top10AvgChangePercent = StringUtil::getPercentageText(top10ChangePercent);
    m_last10AvgChangePercent = StringUtil::getPercentageText(last10ChangePercent);
    m_upCount   = QString::number(upCount);
    m_flatCount = QString::number(flatCount);
    m_downCount = QString::number(downCount);
    updateUI();
}

// 设置是否显示右侧分割线
void StatusBarQuoteItem::setShowDivider(bool show) {
    m_showDivider = show;
    m_dividerLabel->setVisible(show);
}

void StatusBarQuoteItem::reset() {
    if (m_mode == Mode::Symbol) {
        if (!m_symbol.isEmpty() && m_callbackId != -1) {
            QuoteManager::getInstance()->getSymbolTickerUpdateDelegate(m_symbol) -= m_callbackId;
        }
    }
    else if (m_mode == Mode::MarketOverview) {
        if (m_callbackId != -1) {
            QuoteManager::getInstance()->onMarketSummaryDataUpdate -= m_callbackId;
        }
    }
    m_mode = Mode::None;
}

void StatusBarQuoteItem::updateSymbolQuoteData(const QuoteTickerData& tickerData) {
    this->setSymbolData(tickerData.price, tickerData.changePerc);
}

void StatusBarQuoteItem::updateMarketOverviewData(const MarketSummaryData& summaryData) {
    this->setMarketOverviewData(summaryData.avgChangePerc,
                                summaryData.top10AvgChangePerc, 
                                summaryData.last10AvgChangePerc,
                                summaryData.riseCount, 
                                summaryData.unchangedCount,
                                summaryData.fallCount);
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
        m_averageChangePercentLabel = new QLabel(m_marketOverviewWidget);
        m_top10AvgChangePercentLabel = new QLabel(m_marketOverviewWidget);
        m_last10AvgChangePercentLabel = new QLabel(m_marketOverviewWidget);
        m_upCountLabel = new QLabel(m_marketOverviewWidget);
        m_flatCountLabel = new QLabel(m_marketOverviewWidget);
        m_downCountLabel = new QLabel(m_marketOverviewWidget);

        QHBoxLayout* layout = new QHBoxLayout();
        layout->setContentsMargins(0, 0, 0, 0);
        layout->setSpacing(5);
        layout->addWidget(new QLabel("市场总览:", m_marketOverviewWidget));
        layout->addWidget(m_averageChangePercentLabel);
        layout->addWidget(m_upCountLabel);
        layout->addWidget(ControlFactory::createVerticalSplitterLabel(m_marketOverviewWidget));
        layout->addWidget(m_flatCountLabel);
        layout->addWidget(ControlFactory::createVerticalSplitterLabel(m_marketOverviewWidget));
        layout->addWidget(m_downCountLabel);
        layout->addWidget(new QLabel("前10平均:", m_marketOverviewWidget));
        layout->addWidget(m_top10AvgChangePercentLabel);
        layout->addWidget(new QLabel("后10平均:", m_marketOverviewWidget));
        layout->addWidget(m_last10AvgChangePercentLabel);

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
        m_upCountLabel->setText(m_upCount);
        m_flatCountLabel->setText(m_flatCount);
        m_downCountLabel->setText(m_downCount);
        m_averageChangePercentLabel->setText(m_averageChangePercent);
        m_top10AvgChangePercentLabel->setText(m_top10AvgChangePercent);
        m_last10AvgChangePercentLabel->setText(m_last10AvgChangePercent);

        m_marketOverviewWidget->setVisible(true);
    }
}
