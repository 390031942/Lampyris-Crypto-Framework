// Project Include(s)
#include "QuoteManager.h"
#include "Network/WebSocketClient.h"
#include "../Data/QuoteCandleData.h"
#include "../Data/QuoteCandleDataView.h"
#include "AppSystem/Trade/Manager/TradeManager.h"

QuoteManager::QuoteManager() {
	WebSocketMessageHandlerRegistry::getInstance()->registry(this);
}

void QuoteManager::subscribeTickerData() {
	ReqSubscribeTickerData reqSubscribeTickerData;
	reqSubscribeTickerData.set_iscancel(false);
	WebSocketClient::getInstance()->sendMessage(reqSubscribeTickerData);
}

void QuoteManager::cancelSubscribeTickerData() {
	ReqSubscribeTickerData reqSubscribeTickerData;
	reqSubscribeTickerData.set_iscancel(true);
	WebSocketClient::getInstance()->sendMessage(reqSubscribeTickerData);
}

void QuoteManager::sortTickerData(TickerDataSortType sortType, bool descending) {
    // 定义排序规则
    auto comparator = [sortType, descending](const QuoteTickerDataPtr& a, const QuoteTickerDataPtr& b) {
        switch (sortType) {
        case TickerDataSortType::NONE:  // 根据上架时间从早到晚排序
            return descending ? a->timestamp > b->timestamp : a->timestamp < b->timestamp;
        case TickerDataSortType::NAME:  // 名称字典序
            return descending ? a->symbol > b->symbol : a->symbol < b->symbol;
        case TickerDataSortType::PRICE:  // 当前价
            return descending ? a->price > b->price : a->price < b->price;
        case TickerDataSortType::CURRENCY:  // 成交额
            return descending ? a->currency > b->currency : a->currency < b->currency;
        case TickerDataSortType::PERCENTAGE:  // 涨跌幅
            return descending ? a->changePerc > b->changePerc : a->changePerc < b->changePerc;
        default:
            return false;  // 默认情况下不进行排序
        }
    };

    // 使用 std::sort 对 tickerDataList 排序
    std::sort(m_tickerDataList.begin(), m_tickerDataList.end(), comparator);
}

void QuoteManager::handleTickerData(ResSubscribeTickerData resSubscribeTickerData) {
	foreach(auto tickerDataBean, resSubscribeTickerData.beanlist()) {
		uint32_t hashValue = std::hash<std::string>()(tickerDataBean.symbol());
		if (!m_symbol2TickerDataMap.contains(hashValue)) {
			auto tickerData = m_symbol2TickerDataMap[hashValue] = std::make_shared<QuoteTickerData>();
			tickerData->symbol = QString::fromStdString(tickerDataBean.symbol());
		}

		QuoteTickerDataPtr tickerData = m_symbol2TickerDataMap[hashValue];
		tickerData->price = tickerDataBean.price();
		tickerData->changePerc = tickerDataBean.percentage();
		tickerData->currency = tickerDataBean.currency();
		tickerData->markPrice = tickerDataBean.markprice();
		tickerData->indexPrice = tickerDataBean.indexprice();
		tickerData->fundingRate = tickerDataBean.fundingrate();
		tickerData->nextFundingTime = tickerDataBean.nextfundingtime();
		tickerData->riseSpeed = tickerDataBean.risespeed();
	}
}

QuoteCandleDataSegmentPtr QuoteManager::allocateSegment() {
	if (m_segmentPool.empty()) {
		auto segment = m_segmentPool.front();
		m_segmentPool.pop();
	}
	return std::make_shared<QuoteCandleDataSegment>();
}

void QuoteManager::recycleSegement(QuoteCandleDataSegmentPtr segment) {
	if (segment != nullptr) {
		m_segmentPool.push(segment);
		clearSegment(*segment);
	}
}

QuoteCandleDataDynamicSegmentPtr QuoteManager::allocateDynamicSegment() {
	if (m_dynamicSegmentPool.empty()) {
		auto segment = m_dynamicSegmentPool.front();
		m_dynamicSegmentPool.pop();
	}
	return std::make_shared<QuoteCandleDataDynamicSegment>();
}

void QuoteManager::recycleDynamicSegement(QuoteCandleDataDynamicSegmentPtr segment) {
	if (segment != nullptr) {
		m_dynamicSegmentPool.push(segment);
		clearSegment(*segment);
	}
}

bool QuoteManager::requestCandleDataForView(QuoteCandleDataView* view) {
	auto tradeRule = TradeManager::getInstance()->getTradeRule(view->m_symbol);
	if (tradeRule == nullptr) {
		return;
	}

	// 新分配一个segment
	auto segment = allocateSegment();
	view->m_segments.push_back(segment);

	ReqCandlestickQuery reqCandlestickQuery;
	reqCandlestickQuery.set_symbol(view->m_symbol.toUtf8());
	reqCandlestickQuery.set_barsize(QuoteUtil::toStdString(view->m_barSize));
	reqCandlestickQuery.set_count(QUOTE_CANDLE_DATA_SEGMENT_SIZE);

	// k线请求的结束时间
	QDateTime dateTime = view->getFirstDataDateTime();
	if (dateTime.isValid()) {
		// 如果首个k线数据存在，那就让dateTime = 这根k线对应的DateTime 减去 barSize对应的毫秒数
		dateTime.addMSecs(-QuoteUtil::getIntervalMs(view->m_barSize));
		reqCandlestickQuery.set_endtime(DateTimeUtil::toUtcTimestamp(dateTime));
	}
	else {
		// k线请求时间设置为当前UTC时间
		dateTime = QDateTime::currentDateTimeUtc();
	}

	WebSocketClient::getInstance()->sendMessage(reqCandlestickQuery);

	// 根据上架DateTime和 k线请求的结束时间，计算请求得到的k线数量， 
	// 如果k线数量小于等于 segment的k线数量，说明没有多余的历史k线了
	QDateTime onBoardTime = tradeRule->onBoardTime;
	QDateTime endDateTime = dateTime;
	BarSize barSize = view->m_barSize;

	bool fullData = QuoteUtil::calculateCandleCount(onBoardTime, endDateTime, barSize) <= QUOTE_CANDLE_DATA_SEGMENT_SIZE;
	return fullData;
}

void QuoteManager::subscribeAll() {
    subscribeTickerData();
}

void QuoteManager::cancelAllSubscription() {

}
