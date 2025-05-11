// Project Include(s)
#include "MainStatusBar.h"
#include "Base/AppConfigManager.h"
#include "UI/Common/ControlFactory.h"

MainStatusBar::MainStatusBar(QWidget* parent)
    : QWidget(parent), 
    m_layout(new QHBoxLayout(this)),
    m_timeWidget(new TimeWidget(this)),
    m_signalWidget(new SignalStrengthWidget(this)),
    m_versionLabel(new QLabel(this)) {
    setFixedHeight(32);
    setLayout(m_layout);
    reloadQuoteItems();

    m_layout->addSpacerItem(ControlFactory::createHorizontalSpacerItem());
    m_layout->addWidget(m_timeWidget);
    m_layout->addWidget(m_signalWidget);
    m_layout->addWidget(m_versionLabel);

    m_versionLabel->setText("ver.0.0.1");
}

void MainStatusBar::reloadQuoteItems() {
    for (auto item : m_quoteItemList) {
        item->reset();
    }
    // 加载配置
    auto setting = AppConfigManager::getInstance()->getConfig("preference");
    // 是否显示市场总览
    bool showOverView = setting->getValue("StatusBar", "ShowOverview", "1").toString() == "1";
    // Symbol列表(;分割)
    QStringList symbolList = setting->getValue("StatusBar", "SymbolList", "BTCUSDT;ETHUSDT").toString().split(";");

    // 计算所需要的数量: 市场总览 + symbol行情的总数量不超过MAX_QUOTE_ITEM_COUNT,超过这个数量的symbol将不会显示。
    int requireCount = std::min((int)showOverView + (int)symbolList.size(), MAX_QUOTE_ITEM_COUNT);
    int symbolCount = requireCount - (int)showOverView;
    int nowCount = m_quoteItemList.size();
    for (int i = 0; i < requireCount - nowCount; i++) {
        auto item = new StatusBarQuoteItem(this);
        m_layout->addWidget(item);
        m_quoteItemList.push_back(item);
    }

    // 订阅数据
    int index = 0;
    if(showOverView) {
        m_quoteItemList[index++]->setMarketOverviewMode();
    } 
    for (int i = 0; i < symbolCount; i++) {
        m_quoteItemList[index++]->setSymbolMode(symbolList[i]);
    }
}
