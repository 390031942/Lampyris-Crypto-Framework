// Project Include(s)
#include "QuoteManager.h"
#include "Network/WebSocketClient.h"
#include "../Data/QuoteCandleData.h"
#include "../Data/QuoteCandleDataView.h"
#include "AppSystem/Trade/Manager/TradeManager.h"

QuoteManager::QuoteManager() {
	WebSocketMessageHandlerRegistry::getInstance()->registry(this);
}

void QuoteManager::handleMessage(Response response) {
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
    // �����������
    auto comparator = [sortType, descending](const QuoteTickerDataPtr& a, const QuoteTickerDataPtr& b) {
        switch (sortType) {
        case TickerDataSortType::NONE:  // �����ϼ�ʱ����絽������
            return descending ? a->timestamp > b->timestamp : a->timestamp < b->timestamp;
        case TickerDataSortType::NAME:  // �����ֵ���
            return descending ? a->symbol > b->symbol : a->symbol < b->symbol;
        case TickerDataSortType::PRICE:  // ��ǰ��
            return descending ? a->price > b->price : a->price < b->price;
        case TickerDataSortType::CURRENCY:  // �ɽ���
            return descending ? a->currency > b->currency : a->currency < b->currency;
        case TickerDataSortType::PERCENTAGE:  // �ǵ���
            return descending ? a->changePerc > b->changePerc : a->changePerc < b->changePerc;
        default:
            return false;  // Ĭ������²���������
        }
    };

    // ʹ�� std::sort �� tickerDataList ����
    std::sort(m_tickerDataList.begin(), m_tickerDataList.end(), comparator);
}

void QuoteManager::handleTickerData(ResSubscribeTickerData resSubscribeTickerData) {
	for (auto tickerDataBean : resSubscribeTickerData.beanlist()) {
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

void QuoteManager::ConvertQuoteCandleData(CandlestickBean bean, QuoteCandleData& quoteCandleData) const {
	quoteCandleData.dateTime = DateTimeUtil::fromUtcTimestamp(bean.time());
	quoteCandleData.open = bean.open();
	quoteCandleData.close = bean.close();
	quoteCandleData.high = bean.high();
	quoteCandleData.low = bean.low();
	quoteCandleData.volume = bean.volume();
	quoteCandleData.currency = bean.currency();
}

// k���б��ѯ����ֻ��Ҫ������ͼ����
void QuoteManager::handleResCandlestickQuery(ResCandlestickQuery resCandlestickQuery) {
	auto symbol = resCandlestickQuery.symbol();
	auto barSize = QuoteUtil::toBarSize(resCandlestickQuery.barsize());

	for (auto view : m_dataViewList) {
		if (view->m_symbol.toUtf8().constData() == symbol && view->m_barSize == barSize) {
			if (!view->m_segments.empty()) {
				auto& segment = view->m_segments.back();
				for (int i = 0; i < resCandlestickQuery.beanlist().size(); i++) {
					auto candleBean = resCandlestickQuery.beanlist()[i];
					ConvertQuoteCandleData(candleBean, (*segment)[i]);
				}
				view->notifyDataReceived();
			}
		}
	}
}


// ����K��ʱ������µ���k�߸���
void QuoteManager::handleCandlestickBean(CandlestickUpdateBean candlestickUpdateBean) {
	auto symbol = candlestickUpdateBean.symbol();
	auto barSize = QuoteUtil::toBarSize(candlestickUpdateBean.barsize());
	auto isEnd = candlestickUpdateBean.isend();

	for (auto view : m_dataViewList) {
		if (view->m_symbol.toUtf8().constData() == symbol && view->m_barSize == barSize) {
			if (!view->m_segments.empty()) {
				auto& segment = view->m_segments.back();
				auto candleBean = candlestickUpdateBean.bean();

				if (isEnd) { // ׷��һ��k��
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
		return false;
	}

	// �·���һ��segment
	auto segment = allocateSegment();
	view->m_segments.push_back(segment);

	ReqCandlestickQuery reqCandlestickQuery;
	reqCandlestickQuery.set_symbol(view->m_symbol.toUtf8());
	reqCandlestickQuery.set_barsize(QuoteUtil::toStdString(view->m_barSize));
	reqCandlestickQuery.set_count(QUOTE_CANDLE_DATA_SEGMENT_SIZE);

	// k������Ľ���ʱ��
	QDateTime dateTime = view->getFirstDataDateTime();
	if (dateTime.isValid()) {
		// ����׸�k�����ݴ��ڣ��Ǿ���dateTime = ���k�߶�Ӧ��DateTime ��ȥ barSize��Ӧ�ĺ�����
		dateTime.addMSecs(-QuoteUtil::getIntervalMs(view->m_barSize));
		reqCandlestickQuery.set_endtime(DateTimeUtil::toUtcTimestamp(dateTime));
	}
	else {
		// k������ʱ������Ϊ��ǰUTCʱ��
		dateTime = QDateTime::currentDateTimeUtc();
	}

	WebSocketClient::getInstance()->sendMessage(reqCandlestickQuery);

	// �����ϼ�DateTime�� k������Ľ���ʱ�䣬��������õ���k�������� 
	// ���k������С�ڵ��� segment��k��������˵��û�ж������ʷk����
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
