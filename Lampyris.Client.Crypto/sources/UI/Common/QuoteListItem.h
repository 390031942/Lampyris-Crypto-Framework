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
#include "TableHeader.h"

enum QuoteListItemDisplayMode {
    STANDALONG_QUOTE_LIST = 0,
    MOBILE_QUOTE_LIST = 0,
    SEARCH_LIST = 2,
};

/// <summary>
// 行情列表中的Item, 用于桌面平台顶部的搜索联想词列表 以及行情列表 
/// </summary>
class QuoteListItem : public QWidget {
    Q_OBJECT
public:
    explicit QuoteListItem(QuoteListItemDisplayMode mode, QWidget* parent = Q_NULLPTR);
    void                   refresh(const QuoteTickerDataPtr tickerData, double minTick);
    void                   resizeFields(const std::vector<TableColumnWidthInfo>& widthList);
private:
    void                   setupUI(QuoteListItemDisplayMode mode); // 根据模式设置UI布局
    QLabel*                m_symbol;
    AmountText*            m_24hVolumeCurrency;
    PriceText*             m_price;
    PercentageDisplayText* m_percentage;
    PercentageDisplayText* m_riseSpeed;

    std::vector<QWidget*>  m_resizeWidgets;
};