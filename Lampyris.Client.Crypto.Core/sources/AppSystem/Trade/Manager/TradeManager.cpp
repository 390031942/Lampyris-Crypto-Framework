#pragma once
// Project Include(s)
#include "Network/WebSocketClient.h"
#include "Protocol/Protocols.h"
#include "Base/Singleton.h"
#include "../Data/QuoteTickerData.h"
#include "../Data/QuoteCandleData.h"
#include "Network/WebSocketMessageHandlerRegistry.h"
#include "Util/DateTimeUtil.h"
#include "Collections/BidirectionalDictionary.h"

// STD Include(s)
#include <vector>
#include <queue>
#include <unordered_map>

// Ticker数据排序类型
enum TickerDataSortType {
	NONE, // 默认排序,根据上架时间从早到晚排序
	NAME, // 名称字典序
 	PRICE, // 当前价
	CURRENCY,// 成交额
	PERCENTAGE, // 涨跌幅
};

class QuoteCandleDataView;

class QuoteUtil {
private:
	const static BidirectionalDictionary<BarSize, std::string> ms_barSizeDictionary;
public:
	// 将 BarSize 转换为字符串
	static std::string toStdString(BarSize barSize) {
		return ms_barSizeDictionary.getByKey(barSize);
	}

	// 将字符串转换为 BarSize
	static BarSize toBarSize(const std::string& barSizeString) {
		return ms_barSizeDictionary.getByValue(barSizeString);
	}

	/// <summary>
	/// 获取 BarSize 对应的时间间隔（以毫秒为单位）
	/// </summary>
	static qint64 getIntervalMs(BarSize barSize) {
		switch (barSize) {
		case _1m:  return 1 * 60 * 1000;          // 1 分钟
		case _3m:  return 3 * 60 * 1000;          // 3 分钟
		case _5m:  return 5 * 60 * 1000;          // 5 分钟
		case _15m: return 15 * 60 * 1000;         // 15 分钟
		case _30m: return 30 * 60 * 1000;         // 30 分钟
		case _1H:  return 1 * 60 * 60 * 1000;     // 1 小时
		case _2H:  return 2 * 60 * 60 * 1000;     // 2 小时
		case _4H:  return 4 * 60 * 60 * 1000;     // 4 小时
		case _6H:  return 6 * 60 * 60 * 1000;     // 6 小时
		case _12H: return 12 * 60 * 60 * 1000;    // 12 小时
		case _1D:  return 1 * 24 * 60 * 60 * 1000; // 1 天
		case _3D:  return 3 * 24 * 60 * 60 * 1000; // 3 天
		case _1W:  return 7 * 24 * 60 * 60 * 1000; // 1 周
		}
	}
};

class QuoteManager:public SingletonQObject<QuoteManager>, public IWebSocketMessageHandler {
public:
	QuoteManager();

	virtual void handleMessage(Response response) {
		auto typeCase = response.response_type_case();
		if (typeCase == Response::ResponseTypeCase::kResSubscribeTickerData) {
			handleTickerData(response.ressubscribetickerdata());
		}
		else if (typeCase == Response::ResponseTypeCase::kResCandlestickQuery) {
			handleResCandlestickQuery(response.rescandlestickquery());
		}
		else if (typeCase == Response::ResponseTypeCase::kCandlestickUpdateBean) {
			handleCandlestickBean(response.candlestickupdatebean());
		}
	}
private:
	#pragma region Ticker行情

	// symbol字符串哈希值 -> QuoteTickerDataPtr, 存储了所有Ticker行情的字典
	std::unordered_map<uint32_t, QuoteTickerDataPtr> m_symbol2TickerDataMap;

	//  存储了所有Ticker行情的列表，方便展示排序的结果
	std::vector<QuoteTickerDataPtr> m_tickerDataList;

	// 订阅/反订阅
	void subscribeTickerData();
	void cancelSubscribeTickerData();

	/// <summary>
	/// 对行情列表进行排序
	/// </summary>
	/// <param name="sortType">字段排序类型</param>
	/// <param name="descending">是否降序</param>
	void sortTickerData(TickerDataSortType sortType, bool descending);

	void handleTickerData(ResSubscribeTickerData resSubscribeTickerData);
	#pragma endregion

	#pragma region K线数据

	void ConvertQuoteCandleData(CandlestickBean bean, QuoteCandleData& quoteCandleData) const {
		quoteCandleData.dateTime = DateTimeUtil::fromUtcTimestamp(bean.time());
		quoteCandleData.open = bean.open();
		quoteCandleData.close = bean.close();
		quoteCandleData.high = bean.high();
		quoteCandleData.low = bean.low();
		quoteCandleData.volume = bean.volume();
		quoteCandleData.currency = bean.currency();
	}

	// k线列表查询处理，只需要更新视图即可
	void handleResCandlestickQuery(ResCandlestickQuery resCandlestickQuery) {
		auto symbol = resCandlestickQuery.symbol();
		auto barSize = QuoteUtil::toBarSize(resCandlestickQuery.barsize());

		foreach(auto view, m_dataViewList) {
			if (view->m_symbol.toUtf8().constData() == symbol && view->m_barSize == barSize) {
				if (!view->m_segments.empty()) {
					auto& segment = view->m_segments.back();
					for (int i = 0; i < resCandlestickQuery.beanlist().size();i++) {
						auto candleBean = resCandlestickQuery.beanlist()[i];
						ConvertQuoteCandleData(candleBean, (*segment)[i]);
					}
					view->notifyDataReceived();
				}
			}
		}
	}

	// 订阅K线时候的最新单根k线更新
	void handleCandlestickBean(CandlestickUpdateBean candlestickUpdateBean) {
		auto symbol = candlestickUpdateBean.symbol();
		auto barSize = QuoteUtil::toBarSize(candlestickUpdateBean.barsize());
		auto isEnd = candlestickUpdateBean.isend();

		foreach(auto view, m_dataViewList) {
			if (view->m_symbol.toUtf8().constData() == symbol && view->m_barSize == barSize) {
				if (!view->m_segments.empty()) {
					auto& segment = view->m_segments.back();
					auto candleBean = candlestickUpdateBean.bean();

					if (isEnd) { // 追加一条k线
						QuoteCandleData quoteCandleData;
						ConvertQuoteCandleData(candleBean, quoteCandleData);
						(*view->m_dynamicSegment).push_back(quoteCandleData);
					}
					else {
						ConvertQuoteCandleData(candleBean, (*view->m_dynamicSegment).back());
					}
					view->notifyDataReceived();
				}	
			}
		}
	}
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
	/// <returns>预期收到的k线数量</returns>
	int                                          requestCandleDataForView(QuoteCandleDataView* view);
	#pragma endregion

	// 订阅/反订阅
	void subscribeAll();
	void cancelAllSubscription();
};