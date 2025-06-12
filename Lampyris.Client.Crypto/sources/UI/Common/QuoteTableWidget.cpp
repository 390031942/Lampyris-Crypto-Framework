// Project Include(s)
#include "QuoteTableWidget.h"
#include "AppSystem/Quote/Manager/QuoteManager.h"

QuoteTableWidget::QuoteTableWidget(UIDisplayMode displayMode, QuoteTableListMode listMode, QWidget* parent)
    : CrossPlatformWidget(displayMode,parent), m_sortOrder(DataSortingOrder::NONE), m_listMode(listMode) {
    // 初始化布局
    m_layout = new QVBoxLayout(this);
    m_layout->setContentsMargins(0, 0, 0, 0);
    m_layout->setSpacing(0);

    // 初始化表头
    m_tableHeader = new TableHeader(this);
    m_layout->addWidget(m_tableHeader);
    
    // 连接表头信号
    connect(m_tableHeader, &TableHeader::columnWidthResized, this, &QuoteTableWidget::onColumnWidthResized);
    connect(m_tableHeader, &TableHeader::sortRequested, this, &QuoteTableWidget::onSortRequested);

    // 分配视图
    m_dataView = QuoteManager::getInstance()->allocateQuoteTickerDataView();

    this->setupDisplayMode(displayMode);
}

QuoteTableWidget::~QuoteTableWidget() {
    m_dataView->onUpdate -= m_updateCallbackId;
    QuoteManager::getInstance()->recycleQuoteTickerDataView(m_dataView);
}

void QuoteTableWidget::setupDisplayMode(UIDisplayMode displayMode) {
    TableHeaderDefinition tableDef;

    if (m_listMode == QuoteTableListMode::QUOTE_LIST) {
        if (displayMode == UIDisplayMode::STANDALONG) {
            tableDef.startFieldGroup(0.4)
                .addField(COLUMN_NAME_SYMBOL, true)
                .end();
            tableDef.startFieldGroup(0.15)
                .addField(COLUMN_NAME_PRICE, true)
                .end();
            tableDef.startFieldGroup(0.15
            )
                .addField(COLUMN_NAME_24H_CURRENCY, true)
                .end();
            tableDef.startFieldGroup(0.15)
                .addField(COLUMN_NAME_PERCENTAGE, true)
                .end();
            tableDef.startFieldGroup(0.15)
                .addField(COLUMN_NAME_RISE_SPEED, true)
                .end();

        }
        else if (displayMode == UIDisplayMode::MOBILE) {
            tableDef.startFieldGroup(0.4)
                .addField(COLUMN_NAME_SYMBOL, true)
                .addField(COLUMN_NAME_24H_CURRENCY, true)
                .end();
            tableDef.startFieldGroup(0.3)
                .addField(COLUMN_NAME_PRICE, true)
                .addField(COLUMN_NAME_RISE_SPEED, true)
                .end();
            tableDef.startFieldGroup(0.3)
                .addField(COLUMN_NAME_PERCENTAGE, true)
                .end();

            m_itemMode = QuoteListItemDisplayMode::STANDALONG_QUOTE_LIST;
        }

    }
    else if(m_listMode == QuoteTableListMode::SEARCH_LIST) {
        tableDef.startFieldGroup(0.4)
            .addField(COLUMN_NAME_SYMBOL, true)
            .end();
        tableDef.startFieldGroup(0.3)
            .addField(COLUMN_NAME_PRICE, true)
            .end();
        tableDef.startFieldGroup(0.3)
            .addField(COLUMN_NAME_PERCENTAGE, true)
            .end();

        m_itemMode = QuoteListItemDisplayMode::SEARCH_LIST;
    }
    m_tableHeader->setHeaderDefinition(tableDef);
}

void QuoteTableWidget::onColumnWidthResized(const std::vector<TableColumnWidthInfo>& fieldWidths) {
    // 遍历所有的 QuoteListItem
    for (auto* item : m_items) {
        item->resizeFields(fieldWidths);
    }

    // 重绘界面
    update();
}

void QuoteTableWidget::onSortRequested(const QString& fieldName, DataSortingOrder sortOrder) {
    m_sortField = fieldName;
    m_sortOrder = sortOrder;

    m_dataView->sort(m_columnName2SortType[fieldName], sortOrder);
    m_updateCallbackId = m_dataView->onUpdate += [this]() {
        this->handleDataUpdate();
    };

    // 重绘
    update();
}

void QuoteTableWidget::sortItems() {
    // 重新排列布局
    for (auto* item : m_items) {
        m_layout->removeWidget(item);
        m_layout->addWidget(item);
    }
}

void QuoteTableWidget::handleDataUpdate() {
    int size = m_dataView->size();
    if (m_items.empty()) {
        m_items.reserve(size);

        for (int i = 0; i < size; i++) {
            m_items.push_back(new QuoteListItem(m_itemMode, this));
        }
    }
    else {
        int itemCount = m_items.size();
        if (size > itemCount) {
            m_items.push_back(new QuoteListItem(m_itemMode, this));
        }
    }

    for (int i = 0; i < m_items.size(); i++) {
        auto item = m_items[i];
        if (i < size) {
            auto tickerData = (*m_dataView)[i];
            const auto& tradeRule = QuoteManager::getInstance()->queryTradeRule(tickerData->symbol);
            item->refresh(tickerData, tradeRule->priceStep);
            item->show();
        }
        else {
            item->hide();
        }
    }

    // 重绘
    update();
}
