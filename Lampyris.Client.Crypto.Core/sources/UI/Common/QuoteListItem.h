#pragma once
// QT Include(s)
#include <QWidget>
#include <QLayout>
#include <QLabel>

#include "PercentageDisplayText.h"
#include "AmountText.h"
#include "PriceText.h"

/// <summary>
// 行情列表中的Item, 用于桌面平台顶部的搜索联想词列表
// 以及移动端的行情列表 + 搜索联想词列表
/// </summary>
class QuoteListItem:public QWidget {
	Q_OBJECT
public:
	explicit QuoteListItem(QWidget* parent = Q_NULLPTR);
private:
	QLabel* m_symbol;
	AmountText* m_24hVolumeCurrency;
	PriceText* m_price;
	PercentageDisplayText* m_perctange;
};

