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

// Ticker������������
enum TickerDataSortType {
	NONE, // Ĭ������,�����ϼ�ʱ����絽������
	NAME, // �����ֵ���
 	PRICE, // ��ǰ��
	CURRENCY,// �ɽ���
	PERCENTAGE, // �ǵ���
};

class QuoteCandleDataView;

class QuoteUtil {
private:
	const static BidirectionalDictionary<BarSize, std::string> ms_barSizeDictionary;
public:
	// �� BarSize ת��Ϊ�ַ���
	static std::string toStdString(BarSize barSize) {
		return ms_barSizeDictionary.getByKey(barSize);
	}

	// ���ַ���ת��Ϊ BarSize
	static BarSize toBarSize(const std::string& barSizeString) {
		return ms_barSizeDictionary.getByValue(barSizeString);
	}

	/// <summary>
	/// ��ȡ BarSize ��Ӧ��ʱ�������Ժ���Ϊ��λ��
	/// </summary>
	static qint64 getIntervalMs(BarSize barSize) {
		switch (barSize) {
		case _1m:  return 1 * 60 * 1000;          // 1 ����
		case _3m:  return 3 * 60 * 1000;          // 3 ����
		case _5m:  return 5 * 60 * 1000;          // 5 ����
		case _15m: return 15 * 60 * 1000;         // 15 ����
		case _30m: return 30 * 60 * 1000;         // 30 ����
		case _1H:  return 1 * 60 * 60 * 1000;     // 1 Сʱ
		case _2H:  return 2 * 60 * 60 * 1000;     // 2 Сʱ
		case _4H:  return 4 * 60 * 60 * 1000;     // 4 Сʱ
		case _6H:  return 6 * 60 * 60 * 1000;     // 6 Сʱ
		case _12H: return 12 * 60 * 60 * 1000;    // 12 Сʱ
		case _1D:  return 1 * 24 * 60 * 60 * 1000; // 1 ��
		case _3D:  return 3 * 24 * 60 * 60 * 1000; // 3 ��
		case _1W:  return 7 * 24 * 60 * 60 * 1000; // 1 ��
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
	#pragma region Ticker����

	// symbol�ַ�����ϣֵ -> QuoteTickerDataPtr, �洢������Ticker������ֵ�
	std::unordered_map<uint32_t, QuoteTickerDataPtr> m_symbol2TickerDataMap;

	//  �洢������Ticker������б�����չʾ����Ľ��
	std::vector<QuoteTickerDataPtr> m_tickerDataList;

	// ����/������
	void subscribeTickerData();
	void cancelSubscribeTickerData();

	/// <summary>
	/// �������б��������
	/// </summary>
	/// <param name="sortType">�ֶ���������</param>
	/// <param name="descending">�Ƿ���</param>
	void sortTickerData(TickerDataSortType sortType, bool descending);

	void handleTickerData(ResSubscribeTickerData resSubscribeTickerData);
	#pragma endregion

	#pragma region K������

	void ConvertQuoteCandleData(CandlestickBean bean, QuoteCandleData& quoteCandleData) const {
		quoteCandleData.dateTime = DateTimeUtil::fromUtcTimestamp(bean.time());
		quoteCandleData.open = bean.open();
		quoteCandleData.close = bean.close();
		quoteCandleData.high = bean.high();
		quoteCandleData.low = bean.low();
		quoteCandleData.volume = bean.volume();
		quoteCandleData.currency = bean.currency();
	}

	// k���б��ѯ����ֻ��Ҫ������ͼ����
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

	// ����K��ʱ������µ���k�߸���
	void handleCandlestickBean(CandlestickUpdateBean candlestickUpdateBean) {
		auto symbol = candlestickUpdateBean.symbol();
		auto barSize = QuoteUtil::toBarSize(candlestickUpdateBean.barsize());
		auto isEnd = candlestickUpdateBean.isend();

		foreach(auto view, m_dataViewList) {
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
	#pragma endregion

	#pragma region ��ͼ���
private:
	// �����е�����k����ͼ���б�,���յ�k���б�ʱ��ͨ������ʵ�ֶ�Ӧ����ͼ�ĸ���
	std::vector<QuoteCandleDataView*>            m_dataViewList;

	// k�������б����ػ�����ͨ��allocateSegment/recycleSegement �����ظ��ڴ����
	std::queue<QuoteCandleDataSegmentPtr>        m_segmentPool;
	std::queue<QuoteCandleDataDynamicSegmentPtr> m_dynamicSegmentPool;

	template <typename Container>
	void clearSegment(Container& container) {
		static_assert(std::is_same<typename Container::value_type, QuoteCandleData>::value,
			"clearSegment function only supports QuoteCandleData.");

		for (auto& element : container) {
			element = typename Container::value_type{}; // ʹ��Ĭ�Ϲ��캯������
		}
	}

public:
	QuoteCandleDataSegmentPtr                    allocateSegment();
	void                                         recycleSegement(QuoteCandleDataSegmentPtr segment);
	QuoteCandleDataDynamicSegmentPtr             allocateDynamicSegment();
	void                                         recycleDynamicSegement(QuoteCandleDataDynamicSegmentPtr segment);

	/// <summary>
	/// ��ͼ���ú��������ú������˷�������k���б�
	/// </summary>
	/// <param name="view">��ͼ����</param>
	/// <returns>Ԥ���յ���k������</returns>
	int                                          requestCandleDataForView(QuoteCandleDataView* view);
	#pragma endregion

	// ����/������
	void subscribeAll();
	void cancelAllSubscription();
};