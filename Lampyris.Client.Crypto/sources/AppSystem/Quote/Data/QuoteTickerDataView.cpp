// Project Include(s)
#include "QuoteTickerDataView.h"

void QuoteTickerDataView::sort(TickerDataSortType sortType, DataSortingOrder order) {
	// �������ģʽ�Ƿ���Ч
	if (sortType == TickerDataSortType::NONE || order == DataSortingOrder::NONE) {
		return; // ����������
	}

	// ʹ�� std::sort �� m_dataList ����
	std::sort(m_dataList.begin(), m_dataList.end(), [sortType, order](const QuoteTickerDataPtr& lhs, const QuoteTickerDataPtr& rhs) {
		// ������������ѡ��Ƚ��߼�
		switch (sortType) {
		case TickerDataSortType::NAME:
			return compare(lhs->symbol, rhs->symbol, order);
		case TickerDataSortType::PRICE:
			return compare(lhs->price, rhs->price, order);
		case TickerDataSortType::CURRENCY:
			return compare(lhs->currency, rhs->currency, order);
		case TickerDataSortType::PERCENTAGE:
			return compare(lhs->changePerc, rhs->changePerc, order);
		case TickerDataSortType::RISE_SPEED:
			return compare(lhs->riseSpeed, rhs->riseSpeed, order);
		default:
			return false; // ��֧�ֵ���������
		}
	});
}

const QuoteTickerDataPtr QuoteTickerDataView::operator[](int index) const {
	return (index >= 0 && index < m_dataList.size()) ? m_dataList[index] : nullptr;
}

const QuoteTickerDataPtr QuoteTickerDataView::operator[](const QString& symbol) const {
	auto hashValue = std::hash<QString>()(symbol);
	return m_dataMapRef->contains(hashValue) ? m_dataList[hashValue] : nullptr;
}
