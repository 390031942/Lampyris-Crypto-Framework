#pragma once
// Project Include(s)
#include "Network/WebSocketClient.h"
#include "Protocol/Protocols.h"
#include "Base/Singleton.h"
#include "Network/WebSocketMessageHandlerRegistry.h"
#include "Util/DateTimeUtil.h"
#include "Collections/Delegate.h"
#include "Base/ObjectPool.h"
#include "Const/DataSortingOrder.h"
#include "../Util/QuoteUtil.h"
#include "../Data/QuoteCandleData.h"
#include "../Data/MarketSummaryData.h"
#include "../Data/QuoteTickerDataView.h"
#include "../Data/SymbolTradeRule.h"

// STD Include(s)
#include <vector>
#include <queue>
#include <unordered_map>

class QuoteCandleDataView;

class QuoteManager:public QObject, public IWebSocketMessageHandler {
	Q_OBJECT
	DECLARE_SINGLETON_QOBJECT(QuoteManager)
public:
	QuoteManager();

	virtual void handleMessage(Response response);

	#pragma region Ticker行情
public:
	Delegate<const QuoteTickerData&>&   getSymbolTickerUpdateDelegate(const QString& symbol);
	QuoteTickerDataViewPtr              allocateQuoteTickerDataView();
	void                                recycleQuoteTickerDataView(QuoteTickerDataViewPtr dataView);
private:
	std::queue<QuoteTickerDataViewPtr>  m_quoteTickeDataViewPool;
	std::vector<QuoteTickerDataViewPtr> m_quoteTickeDataViewList;

	// symbol字符串哈希值 -> QuoteTickerDataPtr, 存储了所有Ticker行情的字典
	// 用uint32_t的原因是避免字符串的转换
	std::unordered_map<uint32_t, QuoteTickerDataPtr> m_symbol2TickerDataMap;

	//  存储了所有Ticker行情的列表，方便展示排序的结果
	std::vector<QuoteTickerDataPtr> m_tickerDataList;

	// 订阅/反订阅
	void subscribeTickerData();
	void cancelSubscribeTickerData();

	// 处理符号对增减
	void handleSymbolChange() {}

	void handleTickerData(ResSubscribeTickerData resSubscribeTickerData);

	std::unordered_map<QString,Delegate<const QuoteTickerData&>> m_symbolTickerDataUpdateDelegateMap;
	#pragma endregion

	#pragma region K线数据

	void ConvertQuoteCandleData(CandlestickBean bean, QuoteCandleData& quoteCandleData) const;

	// k线列表查询处理，只需要更新视图即可
	void handleResCandlestickQuery(ResCandlestickQuery resCandlestickQuery);

	// 订阅K线时候的最新单根k线更新
	void handleCandlestickBean(CandlestickUpdateBean candlestickUpdateBean);
	#pragma endregion

	#pragma region 市场总览
public:
	Delegate<const MarketSummaryData&> onMarketSummaryDataUpdate;
#pragma endregion

	#pragma region 视图相关
private:
	// 程序中的所有k线视图的列表,当收到k线列表时，通过遍历实现对应的视图的更新
	std::vector<QuoteCandleDataView*>            m_dataViewList;

	// k线数据列表做池化处理，通过allocateSegment/recycleSegement 避免重复内存分配
	std::queue<QuoteCandleDataSegmentPtr>        m_segmentPool;
	std::queue<QuoteCandleDataDynamicSegmentPtr> m_dynamicSegmentPool;

	template <typename Container>
	void clearSegment(Container& container) {
		static_assert(std::is_same<typename Container::value_type, QuoteCandleData>::value,
			"clearSegment function only supports QuoteCandleData.");

		for (auto& element : container) {
			element = typename Container::value_type{}; // 使用默认构造函数置零
		}
	}

public:
	QuoteCandleDataSegmentPtr                    allocateSegment();
	void                                         recycleSegement(QuoteCandleDataSegmentPtr segment);
	QuoteCandleDataDynamicSegmentPtr             allocateDynamicSegment();
	void                                         recycleDynamicSegement(QuoteCandleDataDynamicSegmentPtr segment);

	/// <summary>
	/// 视图调用函数，调用后向服务端发送请求k线列表
	/// </summary>
	/// <param name="view">视图对象</param>
	/// <returns>k线数据是否完备</returns>
	bool                                         requestCandleDataForView(QuoteCandleDataView* view);
	#pragma endregion

#pragma region 交易规则
	const SymbolTradeRulePtr queryTradeRule(const QString& symbol);
	void handleTradeRule(ResTradeRule resTradeRule);
private:
	std::unordered_map<uint32_t, SymbolTradeRulePtr> m_symbol2TradeRuleMap;
#pragma endregion
	// 订阅/反订阅
	void subscribeAll();
	void cancelAllSubscription();
};