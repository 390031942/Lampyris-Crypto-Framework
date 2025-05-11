#pragma once
// QT Include(s)
#include <QWidget>
#include <QLayout>
#include <QLabel>

#include "PercentageDisplayText.h"
#include "AmountText.h"
#include "PriceText.h"

/// <summary>
// �����б��е�Item, ��������ƽ̨����������������б�
// �Լ��ƶ��˵������б� + ����������б�
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

