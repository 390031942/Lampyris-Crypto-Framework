#pragma once
// Project Include(s)
#include "../Data/QuoteTickerData.h"
#include "../Const/TickerDataSortType.h"
#include "Const/DataSortingOrder.h"
#include "Collections/Delegate.h"

// STD Include(s)
#include <unordered_map>

class QuoteTickerDataView {
	using TickerDataMap = std::unordered_map<uint32_t, QuoteTickerDataPtr>;
public:
	void                            sort(TickerDataSortType sortType, DataSortingOrder order);
	const QuoteTickerDataPtr        operator[](int index) const;
	const QuoteTickerDataPtr        operator[](const QString& symbol) const;
	size_t                          size() const { return m_dataList.size(); }
	Delegate<void>                  onUpdate;
private:
	template <typename T>
	static bool                     compare(const T& lhs, const T& rhs, DataSortingOrder order);
	const TickerDataMap*            m_dataMapRef;
	std::vector<QuoteTickerDataPtr> m_dataList;

	friend class QuoteManager;
};
typedef std::shared_ptr<QuoteTickerDataView> QuoteTickerDataViewPtr;

template<typename T>
inline bool QuoteTickerDataView::compare(const T& lhs, const T& rhs, DataSortingOrder order) {
	if (order == DataSortingOrder::ASCENDING) {
		return lhs < rhs; // 升序
	}
	else if (order == DataSortingOrder::DESCENDING) {
		return lhs > rhs; // 降序
	}
	return false; // 默认不比较
}
