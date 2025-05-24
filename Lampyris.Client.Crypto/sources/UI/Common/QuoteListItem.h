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
// 行情列表中的Item, 用于桌面平台顶部的搜索联想词列表 以及行情列表 
/// </summary>
class QuoteListItem : public QWidget {
    Q_OBJECT
public:
    enum DisplayMode {
        SearchListMode,       // 搜索列表模式
        MobileListMode,       // 移动端行情列表模式
        PCListMode            // PC端行情列表模式
    };
    explicit QuoteListItem(DisplayMode mode, QWidget* parent = Q_NULLPTR);
    void                   refresh(const QuoteTickerDataPtr tickerData, double minTick);
private:
    void                   setupUI(DisplayMode mode); // 根据模式设置UI布局
    QLabel*                m_symbol;
    AmountText*            m_24hVolumeCurrency;
    PriceText*             m_price;
    PercentageDisplayText* m_percentage;
    PercentageDisplayText* m_riseSpeed;
};