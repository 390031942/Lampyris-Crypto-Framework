// Project Include(s)
#include "QuoteTickerDataView.h"

void QuoteTickerDataView::sort(TickerDataSortType sortType, DataSortingOrder order) {
	// 检查排序模式是否有效
	if (sortType == TickerDataSortType::NONE || order == DataSortingOrder::NONE) {
		return; // 不进行排序
	}

	// 使用 std::sort 对 m_dataList 排序
	std::sort(m_dataList.begin(), m_dataList.end(), [sortType, order](const QuoteTickerDataPtr& lhs, const QuoteTickerDataPtr& rhs) {
		// 根据排序类型选择比较逻辑
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
			return false; // 不支持的排序类型
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
